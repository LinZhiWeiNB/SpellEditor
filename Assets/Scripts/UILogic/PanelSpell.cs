using System.Collections.Generic;
using FairyGUI;
using PackageAtrribute;
using PackageSpell;
using RebornFrame.GameData;
using RebornFrame.Spell;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelSpell : FguiPanelBase
    {
        enum TrackKind
        {
            Skill,
            Particles,
            Vfx,
            Spine,
            Animation,
        }

        sealed class KeyFrame
        {
            public int Frame;
            public int Pick = 1;
            public int Condition;
            public readonly List<ulong> Tags = new List<ulong>();
            public readonly List<ulong> Buffs = new List<ulong>();
            public string Anim = "";
            public string Speed = "1";
            public int EndMode;
            public int PauseAt = 1;
            public bool Pingpong;
        }

        sealed class Track
        {
            public TrackKind Kind;
            public string Title;
            public readonly List<KeyFrame> Keys = new List<KeyFrame>();
        }

        UI_PanelSpell view;
        readonly List<Track> _tracks = new List<Track>();
        readonly List<ulong> _tags = new List<ulong>();
        readonly List<ulong> _buffs = new List<ulong>();
        readonly List<EditorUnit> _owners = new List<EditorUnit>();
        readonly List<ulong> _dummies = new List<ulong>();
        ulong _spellId;
        bool _hasPrefer;
        ulong _preferOwner;
        int _owner = -1;
        int _track = -1;
        int _key = -1;
        bool _applying;

        public override void Start(object[] data)
        {
            view = (UI_PanelSpell)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            if (data != null && data.Length > 0 && data[0] is ulong spellId)
            {
                _spellId = spellId;
                view.m_title.text = "技能 " + new SpellId(spellId);
            }

            if (data != null && data.Length > 1 && data[1] is ulong unitId)
            {
                _hasPrefer = true;
                _preferOwner = unitId;
            }

            view.m_btnSwitchUnit.onClick.Set(ToggleUnits);
            view.m_unitName.text = "未选择";
            view.m_btnAddTrack.onClick.Set(AddSkillTrack);
            view.m_btnAddKey.onClick.Set(AddSkillKey);
            view.m_btnAddClip.onClick.Set(AddSpineClip);
            BindPick(view.m_pickOwner, 0);
            BindPick(view.m_pickPrimary, 1);
            BindPick(view.m_pickSubject, 2);
            BindPick(view.m_pickSource, 3);
            BindPick(view.m_pickExplicit, 4);
            BindCondition(view.m_condAll, 0);
            BindCondition(view.m_condAny, 1);
            BindCondition(view.m_condNotAny, 2);
            view.m_endPause.onChanged.Add(() => SaveEnd(0, view.m_endPause.selected));
            view.m_endLoop.onChanged.Add(() => SaveEnd(1, view.m_endLoop.selected));
            view.m_pauseFirst.onChanged.Add(() => SavePause(0, view.m_pauseFirst.selected));
            view.m_pauseLast.onChanged.Add(() => SavePause(1, view.m_pauseLast.selected));
            view.m_pingpong.onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                Current().Pingpong = view.m_pingpong.selected;
            });
            view.m_inputAnim.onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                Current().Anim = view.m_inputAnim.text;
            });
            view.m_inputSpeed.onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                Current().Speed = view.m_inputSpeed.text;
            });

            CollectCatalogs();
            BuildTracks();
            CollectOwners();
            ClearSelection();
        }

        void BuildTracks()
        {
            var archive = ProjectSession.Archive;
            var mask = ProjectSession.AuthorId != 0
                ? ProjectSession.GameRenderPlugins()
                : archive == null ? 0u : archive.RenderPlugins;
            view.m_cSpine.selectedIndex = (mask & GameDataRenderPlugins.Spine) != 0
                ? (int)UI_PanelSpell.EcSpine.开
                : (int)UI_PanelSpell.EcSpine.关;

            _tracks.Clear();
            _tracks.Add(new Track { Kind = TrackKind.Skill, Title = "技能" });
            AddPlugin(mask, GameDataRenderPlugins.Particles, TrackKind.Particles, "粒子");
            AddPlugin(mask, GameDataRenderPlugins.Vfx, TrackKind.Vfx, "新版粒子");
            AddPlugin(mask, GameDataRenderPlugins.Spine, TrackKind.Spine, "Spine");
            AddPlugin(mask, GameDataRenderPlugins.Animation, TrackKind.Animation, "动画");
            RenderTracks();
        }

        void AddPlugin(uint mask, uint bit, TrackKind kind, string title)
        {
            if ((mask & bit) == 0) return;
            _tracks.Add(new Track { Kind = kind, Title = title });
        }

        void AddSpellTrack()
        {
            var count = 0;
            var insert = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill) continue;
                count++;
                insert = i + 1;
            }

            count++;
            _tracks.Insert(insert, new Track
            {
                Kind = TrackKind.Skill,
                Title = "技能 " + count,
            });
            RenderTracks();
        }

        void AddSkillTrack()
        {
            AddSpellTrack();
        }

        void AddSpellKey(TrackKind kind)
        {
            var index = -1;
            if (kind == TrackKind.Skill && _track >= 0 && _tracks[_track].Kind == TrackKind.Skill)
                index = _track;
            if (index < 0)
            {
                for (var i = _tracks.Count - 1; i >= 0; i--)
                {
                    if (_tracks[i].Kind != kind) continue;
                    index = i;
                    break;
                }
            }

            if (index < 0) return;
            var keys = _tracks[index].Keys;
            var frame = keys.Count == 0 ? 0 : keys[keys.Count - 1].Frame + 12;
            keys.Add(new KeyFrame { Frame = frame });
            Select(index, keys.Count - 1);
        }

        void AddSkillKey()
        {
            AddSpellKey(TrackKind.Skill);
        }

        void AddSpineClip()
        {
            AddSpellKey(TrackKind.Spine);
        }

        void Select(int track, int key)
        {
            _track = track;
            _key = key;
            ApplyInspector();
            RenderTracks();
            RefreshPreview();
        }

        void ClearSelection()
        {
            _track = -1;
            _key = -1;
            _applying = true;
            view.m_cInspect.selectedIndex = (int)UI_PanelSpell.EcInspect.空;
            view.m_inspectKind.text = "";
            _applying = false;
            RefreshPreview();
        }

        void ApplyInspector()
        {
            var track = _tracks[_track];
            var key = track.Keys[_key];
            _applying = true;
            if (track.Kind == TrackKind.Skill)
            {
                view.m_cInspect.selectedIndex = (int)UI_PanelSpell.EcInspect.技能关键帧;
                view.m_inspectKind.text = "技能关键帧";
                Pick(key.Pick).selected = true;
                Condition(key.Condition).selected = true;
                RenderChoices(view.m_tags, _tags, key.Tags, view.m_tagHint, "载入的数据里还没有标签。");
                RenderChoices(view.m_buffs, _buffs, key.Buffs, view.m_buffHint, "载入的数据里还没有 Buff。");
            }
            else if (track.Kind == TrackKind.Spine)
            {
                view.m_cInspect.selectedIndex = (int)UI_PanelSpell.EcInspect.Spine关键帧;
                view.m_inspectKind.text = "Spine 关键帧";
                view.m_inputAnim.text = key.Anim;
                view.m_inputSpeed.text = key.Speed;
                view.m_cSpineEnd.selectedIndex = key.EndMode;
                view.m_endPause.selected = key.EndMode == 0;
                view.m_endLoop.selected = key.EndMode == 1;
                view.m_pauseFirst.selected = key.PauseAt == 0;
                view.m_pauseLast.selected = key.PauseAt == 1;
                view.m_pingpong.selected = key.Pingpong;
            }
            else
            {
                view.m_cInspect.selectedIndex = (int)UI_PanelSpell.EcInspect.空;
                view.m_inspectKind.text = "";
            }

            _applying = false;
        }

        void RenderTracks()
        {
            view.m_tracks.itemRenderer = RenderTrack;
            view.m_tracks.numItems = _tracks.Count;
        }

        void RenderTrack(int index, GObject obj)
        {
            var item = (UI_ItemTrack)obj;
            var track = _tracks[index];
            item.m_title.text = track.Title;
            item.m_keys.itemRenderer = (keyIndex, keyObj) =>
            {
                var button = (GButton)keyObj;
                button.title = track.Keys[keyIndex].Frame.ToString();
                button.selected = index == _track && keyIndex == _key;
                var trackIndex = index;
                var captured = keyIndex;
                button.onClick.Set(() => Select(trackIndex, captured));
            };
            item.m_keys.numItems = track.Keys.Count;
        }

        void RenderChoices(GList list, List<ulong> catalog, List<ulong> chosen, GTextField hint, string empty)
        {
            hint.text = catalog.Count == 0 ? empty : "";
            list.itemRenderer = (index, obj) =>
            {
                var button = (UI_ButtonCheck)obj;
                var id = catalog[index];
                button.title = FormatId(id);
                button.SetSize(list.width, button.height);
                button.selected = chosen.Contains(id);
                button.onChanged.Set(() =>
                {
                    if (_applying) return;
                    if (button.selected)
                    {
                        if (!chosen.Contains(id)) chosen.Add(id);
                    }
                    else
                    {
                        chosen.Remove(id);
                    }

                    if (list == view.m_tags) RefreshPreview();
                });
            };
            list.numItems = catalog.Count;
        }

        void CollectCatalogs()
        {
            var tags = new HashSet<ulong>();
            var buffs = new HashSet<ulong>();
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var module = ProjectSession.Sources[i].Module;
                if (module == null) continue;
                for (var t = 0; t < module.Tags.Count; t++) tags.Add(module.Tags[t].Id);
                for (var b = 0; b < module.Buffs.Count; b++) buffs.Add(module.Buffs[b].Id);
            }

            _tags.AddRange(tags);
            _buffs.AddRange(buffs);
        }

        void BindPick(UI_ButtonRadio radio, int index)
        {
            radio.onChanged.Add(() =>
            {
                if (_applying || _key < 0 || !radio.selected) return;
                if (_tracks[_track].Kind != TrackKind.Skill) return;
                Current().Pick = index;
            });
        }

        void BindCondition(UI_ButtonRadio radio, int index)
        {
            radio.onChanged.Add(() =>
            {
                if (_applying || _key < 0 || !radio.selected) return;
                if (_tracks[_track].Kind != TrackKind.Skill) return;
                Current().Condition = index;
            });
        }

        void SaveEnd(int mode, bool selected)
        {
            if (_applying || _key < 0 || !selected) return;
            if (_tracks[_track].Kind != TrackKind.Spine) return;
            Current().EndMode = mode;
            view.m_cSpineEnd.selectedIndex = mode;
        }

        void SavePause(int at, bool selected)
        {
            if (_applying || _key < 0 || !selected) return;
            if (_tracks[_track].Kind != TrackKind.Spine) return;
            Current().PauseAt = at;
        }

        UI_ButtonRadio Pick(int index)
        {
            switch (index)
            {
                case 0: return view.m_pickOwner;
                case 2: return view.m_pickSubject;
                case 3: return view.m_pickSource;
                case 4: return view.m_pickExplicit;
                default: return view.m_pickPrimary;
            }
        }

        UI_ButtonRadio Condition(int index)
        {
            switch (index)
            {
                case 1: return view.m_condAny;
                case 2: return view.m_condNotAny;
                default: return view.m_condAll;
            }
        }

        KeyFrame Current()
        {
            return _tracks[_track].Keys[_key];
        }

        void CollectOwners()
        {
            _owners.Clear();
            if (_spellId != 0)
            {
                var records = UnitLibrary.List();
                for (var i = 0; i < records.Count; i++)
                {
                    var unit = records[i].Unit;
                    if (unit.ActiveSpells.Contains(_spellId) || unit.PassiveSpells.Contains(_spellId))
                        _owners.Add(unit);
                }
            }

            _owner = _owners.Count == 0 ? -1 : 0;
            if (_hasPrefer)
            {
                var found = -1;
                for (var i = 0; i < _owners.Count; i++)
                {
                    if (_owners[i].Id == _preferOwner)
                    {
                        found = i;
                        break;
                    }
                }

                if (found < 0)
                {
                    var record = UnitLibrary.Find(_preferOwner);
                    if (record != null)
                    {
                        _owners.Insert(0, record.Unit);
                        found = 0;
                    }
                }

                if (found >= 0)
                    _owner = found;
            }

            ShowOwner();
        }

        void ShowOwner()
        {
            view.m_unitName.text = _owner < 0
                ? "没有单位拥有这个技能"
                : new SpellId(_owners[_owner].Id).ToString();
            var cast = view.GetChild("previewCast") as GTextField;
            if (cast == null) return;

            cast.text = _owner < 0 ? "" : "主体  " + view.m_unitName.text;
            var picked = _track >= 0
                         && _key >= 0
                         && _tracks[_track].Kind == TrackKind.Skill
                         && _tracks[_track].Keys[_key].Pick == 0;
            cast.color = picked
                ? new Color(0.94f, 0.78f, 0.42f)
                : new Color(0.91f, 0.90f, 0.89f);
        }

        void ToggleUnits()
        {
            var controller = view.GetController("cUnits");
            var list = view.GetChild("units") as GList;
            if (controller == null || list == null)
            {
                if (_owners.Count == 0) return;
                _owner = (_owner + 1) % _owners.Count;
                ShowOwner();
                return;
            }

            var open = controller.selectedIndex != 1;
            controller.selectedIndex = open ? 1 : 0;
            if (!open) return;

            list.itemRenderer = (index, obj) =>
            {
                var button = (GButton)obj;
                var captured = index;
                button.title = new SpellId(_owners[captured].Id).ToString();
                button.selected = captured == _owner;
                button.onClick.Set(() =>
                {
                    _owner = captured;
                    controller.selectedIndex = 0;
                    ShowOwner();
                });
            };
            list.numItems = _owners.Count;
        }

        void RefreshPreview()
        {
            ShowOwner();
            _dummies.Clear();
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill) continue;
                var keys = _tracks[i].Keys;
                for (var k = 0; k < keys.Count; k++)
                {
                    var tags = keys[k].Tags;
                    for (var t = 0; t < tags.Count; t++)
                    {
                        if (!_dummies.Contains(tags[t])) _dummies.Add(tags[t]);
                    }
                }
            }

            var list = view.GetChild("dummies") as GList;
            if (list == null) return;

            var current = _track >= 0 && _key >= 0 && _tracks[_track].Kind == TrackKind.Skill
                ? _tracks[_track].Keys[_key].Tags
                : null;
            list.itemRenderer = (index, obj) =>
            {
                var button = (GButton)obj;
                var id = _dummies[index];
                var picked = current != null && current.Contains(id);
                var local = (uint)id;
                button.title = (picked ? "● " : "") + ProjectSession.TagName(id, local);
                var title = button.GetChild("title") as GTextField;
                if (title != null)
                {
                    title.color = picked
                        ? new Color(0.94f, 0.78f, 0.42f)
                        : new Color(0.55f, 0.53f, 0.49f);
                }
            };
            list.numItems = _dummies.Count;
        }

        static string FormatId(ulong id)
        {
            var author = (uint)(id >> 32);
            var local = (uint)id;
            return author == 0 ? local.ToString() : author + ":" + local;
        }
    }
}
