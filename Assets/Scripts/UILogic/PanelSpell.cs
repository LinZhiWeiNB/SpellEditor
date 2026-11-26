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
        bool _scrollReady;
        bool _dragH;
        bool _dragV;
        float _hPointer;
        float _hGrip;
        float _vPointer;
        float _vGrip;
        GComponent _scrollH;
        GComponent _scrollV;
        EventCallback1 _onDeleteKey;

        public override void Start(object[] data)
        {
            view = (UI_PanelSpell)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            if (data != null && data.Length > 0 && data[0] is ulong spellId)
            {
                _spellId = spellId;
                ProjectSession.EnsureOwnSpell(spellId);
                view.m_title.text = "技能 " + new SpellId(spellId);
            }

            if (data != null && data.Length > 1 && data[1] is ulong unitId)
            {
                _hasPrefer = true;
                _preferOwner = unitId;
            }

            view.m_btnSwitchUnit.onClick.Set(ToggleUnits);
            view.m_unitName.text = "未选择";
            view.m_btnAddKey.onClick.Set(AddSkillKey);
            var deleteKey = view.GetChild("btnDeleteKey") as GButton;
            if (deleteKey != null)
                deleteKey.onClick.Set(DeleteSkillKey);
            _onDeleteKey = OnDeleteKey;
            Stage.inst.onKeyDown.Add(_onDeleteKey);
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
            RenderMarks();
            CollectOwners();
            ClearSelection();
        }

        public override void OnClose()
        {
            if (_onDeleteKey != null)
                Stage.inst.onKeyDown.Remove(_onDeleteKey);
        }

        void OnDeleteKey(EventContext context)
        {
            if (context.inputEvent.keyCode != KeyCode.Delete) return;
            if (Stage.inst.focus is GTextInput) return;
            DeleteSkillKey();
        }

        void RenderMarks()
        {
            var list = MarkList();
            if (list == null) return;

            var span = FrameSpan();
            const int step = 12;
            var count = span / step + 1;
            var host = view.GetChild("marks") as GComponent;
            var viewWidth = PaneView(host, true);
            var tickSpan = viewWidth * span / 60f;
            var itemWidth = 36f;
            var gap = count <= 1 ? 0f : tickSpan / (count - 1) - itemWidth;
            list.autoResizeItem = false;
            list.columnGap = Mathf.RoundToInt(gap);
            list.touchable = false;
            list.itemRenderer = (index, obj) =>
            {
                var label = ((GComponent)obj).GetChild("mark0") as GTextField;
                if (label != null)
                    label.text = (index * step).ToString();
            };
            list.numItems = count;
            if (host == null || host.GetChild("marks") == null) return;
            if (count > 0)
                itemWidth = list.GetChildAt(0).width;
            var width = count <= 1 ? itemWidth : (count - 1) * (itemWidth + list.columnGap) + itemWidth;
            list.SetSize(width, list.height);
            LayoutTimeline();
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

        void AddSpellKey(TrackKind kind)
        {
            var index = -1;
            for (var i = _tracks.Count - 1; i >= 0; i--)
            {
                if (_tracks[i].Kind != kind) continue;
                index = i;
                break;
            }

            if (index < 0) return;
            var keys = _tracks[index].Keys;
            var frame = keys.Count == 0 ? 0 : keys[keys.Count - 1].Frame + 12;
            keys.Add(new KeyFrame { Frame = frame });
            Select(index, keys.Count - 1);
        }

        void AddSkillKey()
        {
            var key = new KeyFrame { Frame = PreviewFrame() };
            SkillTrack().Keys.Add(key);
            PackSkillTracks();
            SelectKey(key);
        }

        void DeleteSkillKey()
        {
            if (_track < 0 || _key < 0 || _track >= _tracks.Count) return;
            var track = _tracks[_track];
            if (track.Kind != TrackKind.Skill || _key >= track.Keys.Count) return;
            track.Keys.RemoveAt(_key);
            _track = -1;
            _key = -1;
            PackSkillTracks();
            ClearSelection();
            RenderTracks();
        }

        Track SkillTrack()
        {
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind == TrackKind.Skill)
                    return _tracks[i];
            }

            var track = new Track { Kind = TrackKind.Skill, Title = "技能" };
            _tracks.Insert(0, track);
            return track;
        }

        int PreviewFrame()
        {
            var host = view.GetChild("progress");
            var slider = host as GSlider ?? (host as GComponent)?.GetChild("progress") as GSlider;
            if (slider == null) return 0;
            return Mathf.Max(0, Mathf.RoundToInt((float)slider.value));
        }

        void PackSkillTracks()
        {
            KeyFrame selected = null;
            if (_track >= 0 && _track < _tracks.Count && _key >= 0 && _key < _tracks[_track].Keys.Count)
                selected = _tracks[_track].Keys[_key];

            var plugins = new List<Track>();
            var events = new List<KeyFrame>();
            var laneOf = new Dictionary<KeyFrame, int>();
            var orderOf = new Dictionary<KeyFrame, int>();
            var lane = 0;
            var order = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill)
                {
                    plugins.Add(_tracks[i]);
                    continue;
                }

                var keys = _tracks[i].Keys;
                for (var k = 0; k < keys.Count; k++)
                {
                    var key = keys[k];
                    events.Add(key);
                    laneOf[key] = lane;
                    orderOf[key] = order++;
                }

                lane++;
            }

            events.Sort((a, b) =>
            {
                var frame = a.Frame.CompareTo(b.Frame);
                if (frame != 0) return frame;
                var kept = laneOf[a].CompareTo(laneOf[b]);
                if (kept != 0) return kept;
                return orderOf[a].CompareTo(orderOf[b]);
            });

            var lanes = new List<List<KeyFrame>>();
            for (var i = 0; i < events.Count; i++)
            {
                var key = events[i];
                var placed = -1;
                for (var n = 0; n < lanes.Count; n++)
                {
                    if (LaneHasFrame(lanes[n], key.Frame)) continue;
                    placed = n;
                    break;
                }

                if (placed < 0)
                {
                    placed = lanes.Count;
                    lanes.Add(new List<KeyFrame>());
                }

                lanes[placed].Add(key);
            }

            if (lanes.Count == 0)
                lanes.Add(new List<KeyFrame>());

            _tracks.Clear();
            for (var i = 0; i < lanes.Count; i++)
            {
                var row = new Track
                {
                    Kind = TrackKind.Skill,
                    Title = i == 0 ? "技能" : "技能 " + (i + 1),
                };
                row.Keys.AddRange(lanes[i]);
                _tracks.Add(row);
            }

            _tracks.AddRange(plugins);
            _track = -1;
            _key = -1;
            if (selected == null) return;
            for (var t = 0; t < _tracks.Count; t++)
            {
                var index = _tracks[t].Keys.IndexOf(selected);
                if (index < 0) continue;
                _track = t;
                _key = index;
                return;
            }
        }

        static bool LaneHasFrame(List<KeyFrame> lane, int frame)
        {
            for (var i = 0; i < lane.Count; i++)
            {
                if (lane[i].Frame == frame) return true;
            }

            return false;
        }

        void SelectKey(KeyFrame key)
        {
            for (var t = 0; t < _tracks.Count; t++)
            {
                var index = _tracks[t].Keys.IndexOf(key);
                if (index < 0) continue;
                Select(t, index);
                return;
            }
        }

        void RefreshDeleteKey()
        {
            var button = view.GetChild("btnDeleteKey") as GButton;
            if (button == null) return;
            var skill = _track >= 0
                        && _track < _tracks.Count
                        && _tracks[_track].Kind == TrackKind.Skill;
            button.grayed = !skill;
            button.touchable = skill;
        }

        void BindSkillKey(GButton button, KeyFrame key, float px)
        {
            var originX = 0f;
            var pointerX = 0f;
            var moved = false;
            button.onTouchBegin.Add(context =>
            {
                context.CaptureTouch();
                moved = false;
                originX = button.x;
                pointerX = context.inputEvent.x;
            });
            button.onTouchMove.Add(context =>
            {
                var parent = button.parent as GComponent;
                if (parent == null) return;
                var dx = LocalDelta(parent, context.inputEvent.x - pointerX, 0f).x;
                if (!moved && Mathf.Abs(dx) < 4f) return;
                moved = true;
                var x = Mathf.Max(0f, originX + dx);
                button.x = x;
                button.title = Mathf.RoundToInt(x / px).ToString();
            });
            button.onTouchEnd.Add(() =>
            {
                if (!moved) return;
                key.Frame = Mathf.Max(0, Mathf.RoundToInt(button.x / px));
                Timers.inst.CallLater(_ =>
                {
                    PackSkillTracks();
                    SelectKey(key);
                });
            });
            button.onClick.Set(() =>
            {
                if (moved) return;
                SelectKey(key);
            });
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
            RefreshDeleteKey();
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
            RefreshDeleteKey();
        }

        void RenderTracks()
        {
            var list = TrackList();
            if (list == null) return;

            list.itemRenderer = (index, obj) =>
            {
                var item = (UI_ItemTrack)obj;
                item.m_title.text = _tracks[index].Title;
            };
            list.numItems = _tracks.Count;
            list.ResizeToFit(_tracks.Count);
            PlaceKeys();
            LayoutTimeline();
        }

        void PlaceKeys()
        {
            var keys = KeysPane();
            if (keys == null) return;

            for (var i = keys.numChildren - 1; i >= 0; i--)
            {
                var child = keys.GetChildAt(i);
                if (child.name == "extent") continue;
                child.Dispose();
            }

            var px = PaneView(keys, true) / 60f;
            if (px < 1f) px = 1f;
            for (var t = 0; t < _tracks.Count; t++)
            {
                var track = _tracks[t];
                for (var k = 0; k < track.Keys.Count; k++)
                {
                    var frame = track.Keys[k];
                    var button = UI_ItemKey.CreateInstance();
                    button.title = frame.Frame.ToString();
                    button.selected = t == _track && k == _key;
                    button.SetXY(frame.Frame * px, t * 44f + 6f);
                    if (track.Kind == TrackKind.Skill)
                    {
                        button.SetSize(px, button.height);
                        BindSkillKey(button, frame, px);
                    }
                    else
                    {
                        var trackIndex = t;
                        var keyIndex = k;
                        button.onClick.Set(() => Select(trackIndex, keyIndex));
                    }

                    keys.AddChild(button);
                }
            }
        }

        void LayoutTimeline()
        {
            BindTimelineScroll();
            var span = FrameSpan();
            var marks = view.GetChild("marks") as GComponent;
            var tracks = view.GetChild("tracks") as GComponent;
            var progress = view.GetChild("progress") as GComponent;
            var keys = KeysPane();
            if (marks == null || tracks == null || progress == null || keys == null) return;

            var contentH = _tracks.Count == 0 ? 0f : _tracks.Count * 40f + (_tracks.Count - 1) * 4f;
            var trackList = TrackList();
            if (trackList != null)
                trackList.SetSize(trackList.width, contentH);
            keys.SetSize(keys.width, Mathf.Max(contentH, 1f));

            var extent = keys.GetChild("extent");
            if (extent != null)
            {
                var contentW = PaneView(keys, true) * span / 60f;
                extent.SetSize(contentW, Mathf.Max(contentH, keys.height));
            }

            var slider = progress.GetChild("progress") as GSlider;
            if (slider != null)
            {
                slider.max = span;
                slider.SetSize(PaneView(progress, true) * span / 60f, slider.height);
            }

            marks.EnsureBoundsCorrect();
            tracks.EnsureBoundsCorrect();
            progress.EnsureBoundsCorrect();
            keys.EnsureBoundsCorrect();
            UpdateScrollBars();
            Timers.inst.CallLater(RefreshScrollLater);
        }

        void RefreshScrollLater(object param)
        {
            UpdateScrollBars();
        }

        void BindTimelineScroll()
        {
            if (_scrollReady) return;
            var marks = view.GetChild("marks") as GComponent;
            var tracks = view.GetChild("tracks") as GComponent;
            var progress = view.GetChild("progress") as GComponent;
            var keys = KeysPane();
            _scrollH = view.GetChild("scrollH") as GComponent;
            _scrollV = view.GetChild("scrollV") as GComponent;
            if (marks == null || tracks == null || progress == null || keys == null || _scrollH == null || _scrollV == null)
                return;

            Hold(marks);
            Hold(tracks);
            Hold(progress);
            Hold(keys);
            BindGrip(_scrollH, true);
            BindGrip(_scrollV, false);
            _scrollReady = true;
        }

        static void Hold(GComponent com)
        {
            var pane = com?.scrollPane;
            if (pane == null) return;
            pane.touchEffect = false;
            pane.mouseWheelEnabled = false;
            pane.bouncebackEffect = false;
        }

        void BindGrip(GComponent bar, bool horizontal)
        {
            var grip = bar.GetChild("grip");
            if (grip == null) return;
            grip.onTouchBegin.Add(context =>
            {
                context.CaptureTouch();
                if (horizontal)
                {
                    _dragH = true;
                    _hPointer = context.inputEvent.x;
                    _hGrip = grip.x;
                }
                else
                {
                    _dragV = true;
                    _vPointer = context.inputEvent.y;
                    _vGrip = grip.y;
                }
            });
            grip.onTouchMove.Add(context =>
            {
                if (horizontal)
                {
                    if (!_dragH) return;
                    var dx = LocalDelta(bar, context.inputEvent.x - _hPointer, 0f).x;
                    var travel = Mathf.Max(0f, bar.width - grip.width);
                    grip.x = Mathf.Clamp(_hGrip + dx, 0f, travel);
                    ApplyScrollX(travel <= 0f ? 0f : grip.x / travel);
                    return;
                }

                if (!_dragV) return;
                var dy = LocalDelta(bar, 0f, context.inputEvent.y - _vPointer).y;
                var room = Mathf.Max(0f, bar.height - grip.height);
                grip.y = Mathf.Clamp(_vGrip + dy, 0f, room);
                ApplyScrollY(room <= 0f ? 0f : grip.y / room);
            });
            grip.onTouchEnd.Add(() =>
            {
                if (horizontal) _dragH = false;
                else _dragV = false;
            });
        }

        static Vector2 LocalDelta(GComponent parent, float dx, float dy)
        {
            var origin = parent.GlobalToLocal(Vector2.zero);
            var shifted = parent.GlobalToLocal(new Vector2(dx, dy));
            return shifted - origin;
        }

        void ApplyScrollX(float perc)
        {
            SetPercX(view.GetChild("marks") as GComponent, perc);
            SetPercX(KeysPane(), perc);
            SetPercX(view.GetChild("progress") as GComponent, perc);
        }

        void ApplyScrollY(float perc)
        {
            var pane = (view.GetChild("tracks") as GComponent)?.scrollPane;
            if (pane == null) return;
            pane.percY = perc;
        }

        static void SetPercX(GComponent com, float perc)
        {
            var pane = com?.scrollPane;
            if (pane == null) return;
            pane.percX = pane.contentWidth <= pane.viewWidth + 1f ? 0f : perc;
        }

        void UpdateScrollBars()
        {
            var keys = KeysPane();
            var tracks = view.GetChild("tracks") as GComponent;
            if (_scrollH != null && keys?.scrollPane != null)
            {
                var pane = keys.scrollPane;
                LayoutGrip(_scrollH, pane.viewWidth, pane.contentWidth, true, pane.percX);
            }

            if (_scrollV != null && tracks?.scrollPane != null)
            {
                var pane = tracks.scrollPane;
                LayoutGrip(_scrollV, pane.viewHeight, pane.contentHeight, false, pane.percY);
            }
        }

        void LayoutGrip(GComponent bar, float viewSize, float content, bool horizontal, float perc)
        {
            var grip = bar.GetChild("grip");
            var show = content > viewSize + 1f;
            bar.visible = show;
            if (!show || grip == null) return;

            var span = horizontal ? bar.width : bar.height;
            var size = Mathf.Clamp(span * (viewSize / content), 28f, span);
            var travel = Mathf.Max(0f, span - size);
            if (horizontal)
            {
                grip.width = size;
                if (!_dragH)
                    grip.x = travel * perc;
            }
            else
            {
                grip.height = size;
                if (!_dragV)
                    grip.y = travel * perc;
            }
        }

        GList MarkList()
        {
            var host = view.GetChild("marks");
            var nested = (host as GComponent)?.GetChild("marks") as GList;
            return nested ?? host as GList;
        }

        GList TrackList()
        {
            var host = view.GetChild("tracks");
            var nested = (host as GComponent)?.GetChild("tracks") as GList;
            return nested ?? host as GList;
        }

        GComponent KeysPane()
        {
            return (view.GetChild("tracks") as GComponent)?.GetChild("keys") as GComponent;
        }

        float PaneView(GComponent com, bool horizontal)
        {
            var pane = com?.scrollPane;
            if (pane == null) return com == null ? 0f : horizontal ? com.width : com.height;
            var size = horizontal ? pane.viewWidth : pane.viewHeight;
            return size > 1f ? size : horizontal ? com.width : com.height;
        }

        int FrameSpan()
        {
            var span = 60;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var keys = _tracks[i].Keys;
                var skill = _tracks[i].Kind == TrackKind.Skill;
                for (var k = 0; k < keys.Count; k++)
                {
                    var end = skill ? keys[k].Frame + 1 : keys[k].Frame;
                    if (end > span) span = end;
                }
            }

            var step = 12;
            return ((span + step - 1) / step) * step;
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
