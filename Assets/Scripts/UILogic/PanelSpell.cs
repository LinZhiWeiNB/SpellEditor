using System;
using System.Collections.Generic;
using System.Text;
using FairyGUI;
using Google.Protobuf;
using PackageAtrribute;
using PackageResource;
using PackageSpell;
using RebornFrame.GameData;
using RebornFrame.Spell;
using RebornFrame.Spell.Animation;
using RebornFrame.Spell.Particles;
using RebornFrame.Spell.Spine;
using RebornFrame.Spell.Vfx;
using SpellEditor;
using Unity.Collections;
using Unity.Entities;
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
            public string Trigger = "";
            public int ClipSource;
            public int Span;
            public float StartSeconds;
            public float DurationSeconds;
            public byte[] Parameters = Array.Empty<byte>();
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

        struct PlacedClip
        {
            public GObject View;
            public float X;
            public float Width;
        }

        UI_PanelSpell view;
        readonly List<Track> _tracks = new List<Track>();
        readonly List<SpellPreviewStage.SpineClipChoice> _animations = new List<SpellPreviewStage.SpineClipChoice>();
        readonly List<SpellPreviewStage.SpineClipChoice> _triggers = new List<SpellPreviewStage.SpineClipChoice>();
        GComboBox _comboAnim;
        GComboBox _comboTrigger;
        readonly List<ulong> _tags = new List<ulong>();
        readonly List<ulong> _buffs = new List<ulong>();
        readonly List<EditorUnit> _owners = new List<EditorUnit>();
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
        float _scrollable;
        EventCallback1 _onDeleteKey;
        BlobAssetReference<SpellCatalogBlob> _previewCatalog;
        bool _ignoreProgress;
        bool _progressBound;
        bool _showingPreview;
        bool _entered;
        bool _editing = true;
        ulong _shownOwner;
        readonly List<SpellEventConfig> _extraEvents = new List<SpellEventConfig>();
        readonly List<SpellPresentationTrack> _extraTracks = new List<SpellPresentationTrack>();
        readonly List<SpellPreviewSim.Frame> _samples = new List<SpellPreviewSim.Frame>();
        readonly List<SpellPreviewSim.Seed> _seeds = new List<SpellPreviewSim.Seed>();
        GComponent _func;
        GComponent _inspect;
        GComponent _timeline;
        Controller _cInspect;
        Controller _cSpine;
        Controller _cClipName;
        Controller _cSpineEnd;
        const int InspectEmpty = 0;
        const int InspectSkill = 1;
        const int InspectSpine = 2;

        void BindRegions()
        {
            _func = view.GetChild("func") as GComponent;
            _inspect = view.GetChild("inspect") as GComponent;
            _timeline = view.GetChild("timeline") as GComponent;
            _cInspect = Control(_inspect, "cInspect");
            _cSpine = Control(_func, "cSpine");
            _cClipName = Control(_inspect, "cClipName");
            _cSpineEnd = Control(_inspect, "cSpineEnd");
        }

        GObject Part(GComponent part, string name)
        {
            var child = part == null ? null : part.GetChild(name);
            return child ?? view.GetChild(name);
        }

        Controller Control(GComponent part, string name)
        {
            var controller = part == null ? null : part.GetController(name);
            return controller ?? view.GetController(name);
        }

        GButton Btn(GComponent part, string name)
        {
            return Part(part, name) as GButton;
        }

        GTextField Label(GComponent part, string name)
        {
            return Part(part, name) as GTextField;
        }

        GTextInput Input(GComponent part, string name)
        {
            return Part(part, name) as GTextInput;
        }

        UI_ButtonRadio Radio(GComponent part, string name)
        {
            return Part(part, name) as UI_ButtonRadio;
        }

        GList ListOf(GComponent part, string name)
        {
            return Part(part, name) as GList;
        }

        GComponent Piece(GComponent part, string name)
        {
            return Part(part, name) as GComponent;
        }

        public override void Start(object[] data)
        {
            view = (UI_PanelSpell)FGUIComponent;
            BindRegions();
            Btn(_func, "btnBack").onClick.Set(() => FGUIPanelManager.Instance.Back());
            if (data != null && data.Length > 0 && data[0] is ulong spellId)
            {
                _spellId = spellId;
                ProjectSession.EnsureOwnSpell(spellId);
                Label(_func, "title").text = "技能 " + new SpellId(spellId);
            }

            if (data != null && data.Length > 1 && data[1] is ulong unitId)
            {
                _hasPrefer = true;
                _preferOwner = unitId;
            }

            Btn(_func, "btnSwitchUnit").onClick.Set(ToggleUnits);
            Label(_func, "unitName").text = "未选择";
            Btn(_func, "btnAddKey").onClick.Set(AddSkillKey);
            var addDummy = Btn(_func, "btnAddDummy");
            if (addDummy == null)
            {
                ShiftForDummyButton();
                addDummy = UI_ButtonSecondary.CreateInstance();
                addDummy.name = "btnAddDummy";
                addDummy.title = "添加木桩";
                addDummy.SetSize(288, 36);
                addDummy.SetXY(16, 168);
                (_func ?? view).AddChild(addDummy);
            }

            addDummy.onClick.Set(() => SpellPreviewStage.AddDummy());
            var deleteKey = Btn(_func, "btnDeleteKey");
            if (deleteKey != null)
                deleteKey.onClick.Set(DeleteSkillKey);
            _onDeleteKey = OnDeleteKey;
            Stage.inst.onKeyDown.Add(_onDeleteKey);
            Btn(_func, "btnAddClip").onClick.Set(AddSpineClip);
            BindPick(Radio(_inspect, "pickOwner"), 0);
            BindPick(Radio(_inspect, "pickPrimary"), 1);
            BindPick(Radio(_inspect, "pickSubject"), 2);
            BindPick(Radio(_inspect, "pickSource"), 3);
            BindPick(Radio(_inspect, "pickExplicit"), 4);
            BindCondition(Radio(_inspect, "condAll"), 0);
            BindCondition(Radio(_inspect, "condAny"), 1);
            BindCondition(Radio(_inspect, "condNotAny"), 2);
            Radio(_inspect, "endPause").onChanged.Add(() => SaveEnd(0, Radio(_inspect, "endPause").selected));
            Radio(_inspect, "endLoop").onChanged.Add(() => SaveEnd(1, Radio(_inspect, "endLoop").selected));
            Radio(_inspect, "pauseFirst").onChanged.Add(() => SavePause(0, Radio(_inspect, "pauseFirst").selected));
            Radio(_inspect, "pauseLast").onChanged.Add(() => SavePause(1, Radio(_inspect, "pauseLast").selected));
            var pingpong = Part(_inspect, "pingpong") as UI_ButtonCheck;
            pingpong.onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                Current().Pingpong = pingpong.selected;
            });
            EnsureClipBoxes();
            _cClipName.onChanged.Add(() =>
            {
                SyncClipBoxes();
                if (_applying || _key < 0) return;
                if (_tracks[_track].Kind != TrackKind.Spine) return;
                Current().ClipSource = _cClipName.selectedIndex;
                FitLength(Current());
            });
            Input(_inspect, "inputSpeed").onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                if (_tracks[_track].Kind != TrackKind.Spine) return;
                Current().Speed = Input(_inspect, "inputSpeed").text;
                FitLength(Current());
            });

            CollectCatalogs();
            BuildTracks();
            RenderMarks();
            CollectOwners();
            ClearSelection();
            OpenPreview();
            SpellPreviewStage.Changed += OnStageChanged;
            RestoreLayout();
            _entered = true;
            RecordLogic();
            PoseAll();
        }

        public override void OnClose()
        {
            SpellPreviewStage.Changed -= OnStageChanged;
            SaveLayout();
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
            ReleasePreview();
            SpellPreviewStage.Close();
            if (_onDeleteKey != null)
                Stage.inst.onKeyDown.Remove(_onDeleteKey);
        }

        void ShiftForDummyButton()
        {
            const float drop = 48f;
            Label(_func, "funcSkill").y += drop;
            Btn(_func, "btnAddKey").y += drop;
            var deleteKey = Part(_func, "btnDeleteKey");
            if (deleteKey != null)
                deleteKey.y += drop;
            Label(_func, "funcSpine").y += drop;
            Btn(_func, "btnAddClip").y += drop;
            var units = ListOf(_func, "units");
            if (units != null)
                units.y += drop;
        }

        void OpenPreview()
        {
            HideLeftover("preview");
            HideLeftover("previewLabel");
            HideLeftover("floor");
            HideLeftover("previewCast");
            HideLeftover("dummies");
            var hit = view.GetChild("previewHit");
            if (hit == null)
                hit = view.GetChild("preview");
            if (hit != null)
            {
                hit.alpha = 0f;
                hit.visible = true;
                hit.touchable = true;
            }

            PaintPreviewBorder();

            var reset = view.GetChild("btnResetCamera") as GButton;
            if (reset == null)
            {
                reset = UI_ButtonSecondary.CreateInstance();
                reset.name = "btnResetCamera";
                reset.title = "重置摄像机";
                reset.SetSize(128, 32);
                reset.SetXY(332, 586);
                view.AddChild(reset);
            }

            reset.onClick.Set(() => SpellPreviewStage.ResetCamera());
            BindPreviewMode();
            SpellPreviewStage.SetPlacement(true);
            SpellPreviewStage.Open(hit, OnPreviewUnit);
            SpellPreviewStage.SetCaster(CurrentOwner());
        }

        void BindPreviewMode()
        {
            var host = view.GetChild("modeToggle") as GComponent;
            var mode = host == null ? null : host.GetController("cMode");
            if (mode != null)
            {
                var legacy = view.GetChild("btnPreviewMode");
                if (legacy != null)
                    legacy.visible = false;
                _editing = true;
                mode.selectedIndex = 0;
                mode.onChanged.Add(() => ApplyMode(mode.selectedIndex == 0));
                return;
            }

            var button = view.GetChild("btnPreviewMode") as GButton;
            if (button == null)
            {
                button = UI_ButtonSecondary.CreateInstance();
                button.name = "btnPreviewMode";
                button.title = "编辑";
                button.SetSize(96, 32);
                button.SetXY(468, 586);
                view.AddChild(button);
            }

            _editing = true;
            button.title = "编辑";
            button.onClick.Set(() => ApplyMode(!_editing));
        }

        void ApplyMode(bool editing)
        {
            if (_editing == editing)
                return;
            _editing = editing;
            var button = view.GetChild("btnPreviewMode") as GButton;
            if (button != null && button.visible)
                button.title = _editing ? "编辑" : "预览";
            SpellPreviewStage.SetPlacement(_editing);
            PaintPreviewBorder();
            if (_editing)
                SpellPreviewStage.RestoreDefaults();
            else
                ApplyWorld();
            var actor = SpellPreviewStage.Selected;
            if (actor != null && _key < 0)
                ShowUnitInspect(actor);
        }

        void PaintPreviewBorder()
        {
            var border = view.GetChild("previewBorder") as GGraph;
            var line = _editing
                ? (Color)new Color32(0x3d, 0xb8, 0x6a, 0xff)
                : (Color)new Color32(0xf0, 0x8a, 0x2a, 0xff);
            var fill = new Color(0f, 0f, 0f, 0f);
            if (border != null)
            {
                border.touchable = false;
                border.alpha = 1f;
                border.DrawRect(border.width, border.height, 3, line, fill);
                return;
            }

            var hit = view.GetChild("previewHit") as GGraph;
            if (hit == null)
                hit = view.GetChild("previewHit") as GGraph;
            if (hit == null)
                return;
            hit.alpha = 1f;
            hit.touchable = true;
            hit.DrawRect(hit.width, hit.height, 3, line, fill);
        }

        void OnPreviewUnit(PreviewActor actor)
        {
            if (actor == null)
            {
                if (_track >= 0 && _key >= 0 && _track < _tracks.Count)
                    ApplyInspector();
                else
                {
                    _cInspect.selectedIndex = InspectEmpty;
                    Label(_inspect, "inspectKind").text = "";
                    RestoreInspectEmpty();
                }

                return;
            }

            ShowUnitInspect(actor);
        }

        void ShowUnitInspect(PreviewActor actor)
        {
            _applying = true;
            _cInspect.selectedIndex = InspectEmpty;
            Label(_inspect, "inspectKind").text = actor.Unit == null
                ? actor.name
                : new SpellId(actor.Unit.Id).ToString();
            var empty = Label(_inspect, "inspectEmpty");
            empty.singleLine = false;
            empty.SetSize(288, 520);
            empty.text = UnitReport(actor);
            _applying = false;
        }

        void RestoreInspectEmpty()
        {
            var empty = Label(_inspect, "inspectEmpty");
            empty.SetSize(288, 48);
            empty.text = "选中技能或 Spine 的关键帧后在这里配置。";
        }

        static string UnitReport(PreviewActor actor)
        {
            var text = new StringBuilder();
            text.Append("属性\n");
            var any = false;
            if (actor.HasSample)
            {
                var seen = new HashSet<ulong>();
                for (var i = 0; i < actor.Sampled.Count; i++)
                {
                    var value = actor.Sampled[i];
                    if (!seen.Add(value.AttributeId)) continue;
                    var parsed = new AttributeId(value.AttributeId);
                    text.Append(ProjectSession.AttributeName(value.AttributeId, parsed.LocalId));
                    text.Append("  ");
                    text.Append(value.Raw);
                    text.Append('\n');
                    any = true;
                }
            }
            else if (actor.Unit != null)
            {
                var seen = new HashSet<ulong>();
                var unit = actor.Unit;
                for (var i = 0; i < unit.PresetAttributes.Count; i++)
                    any |= AppendAttribute(text, unit, unit.PresetAttributes[i], seen);
                for (var i = 0; i < unit.Values.Count; i++)
                    any |= AppendAttribute(text, unit, unit.Values[i].AttributeId, seen);
            }

            if (!any)
                text.Append("没有属性\n");
            text.Append("\n标签\n");
            if (actor.Tags.Count == 0)
                text.Append("没有标签\n");
            for (var i = 0; i < actor.Tags.Count; i++)
            {
                var id = actor.Tags[i];
                text.Append(ProjectSession.TagName(id, (uint)id));
                text.Append('\n');
            }

            text.Append("\nBuff\n");
            if (actor.Buffs.Count == 0)
                text.Append("没有 Buff\n");
            for (var i = 0; i < actor.Buffs.Count; i++)
            {
                text.Append(FormatId(actor.Buffs[i]));
                text.Append('\n');
            }

            return text.ToString();
        }

        static bool AppendAttribute(StringBuilder text, EditorUnit unit, ulong id, HashSet<ulong> seen)
        {
            if (!seen.Add(id))
                return false;
            var parsed = new AttributeId(id);
            text.Append(ProjectSession.AttributeName(id, parsed.LocalId));
            text.Append("  ");
            text.Append(UnitCatalogs.Raw(unit, id));
            text.Append('\n');
            return true;
        }

        void OnDeleteKey(EventContext context)
        {
            if (context.inputEvent.keyCode != KeyCode.Delete) return;
            if (Stage.inst.focus is GTextInput) return;
            DeleteSkillKey();
        }

        void RenderMarks()
        {
            LayoutTimeline();
        }

        void BuildTracks()
        {
            var archive = ProjectSession.Archive;
            var mask = ProjectSession.AuthorId != 0
                ? ProjectSession.GameRenderPlugins()
                : archive == null ? 0u : archive.RenderPlugins;
            _cSpine.selectedIndex = (mask & GameDataRenderPlugins.Spine) != 0 ? 1 : 0;

            _tracks.Clear();
            _tracks.Add(new Track { Kind = TrackKind.Skill, Title = "技能" });
            AddPlugin(mask, GameDataRenderPlugins.Particles, TrackKind.Particles, "粒子");
            AddPlugin(mask, GameDataRenderPlugins.Vfx, TrackKind.Vfx, "新版粒子");
            AddPlugin(mask, GameDataRenderPlugins.Spine, TrackKind.Spine, "Spine");
            AddPlugin(mask, GameDataRenderPlugins.Animation, TrackKind.Animation, "动画");
            LoadTracks();
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
            var start = 0f;
            if (keys.Count > 0)
            {
                var last = keys[keys.Count - 1];
                start = last.StartSeconds + Mathf.Max(last.DurationSeconds, 1f / LogicRate());
            }

            keys.Add(new KeyFrame { StartSeconds = start, DurationSeconds = 12f / LogicRate() });
            Select(index, keys.Count - 1);
            RecordRender(_tracks[index].Kind);
        }

        void AddSkillKey()
        {
            var key = new KeyFrame { Frame = PreviewFrame() };
            SkillTrack().Keys.Add(key);
            PackSkillTracks();
            SelectKey(key);
            RecordLogic();
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
            RecordLogic();
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
            var host = Part(_timeline, "progress");
            var slider = host as GSlider ?? (host as GComponent)?.GetChild("progress") as GSlider;
            if (slider == null) return 0;
            return Mathf.Max(0, Mathf.FloorToInt((float)slider.value * LogicRate() + 0.0001f));
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
            var button = Btn(_func, "btnDeleteKey");
            if (button == null) return;
            var skill = _track >= 0
                        && _track < _tracks.Count
                        && _tracks[_track].Kind == TrackKind.Skill;
            button.grayed = !skill;
            button.touchable = skill;
        }

        void BindKey(GButton button, KeyFrame key, float px, bool skill)
        {
            var originX = 0f;
            var originW = 0f;
            var pointerX = 0f;
            var moved = false;
            var home = key.Frame;
            var edge = 0;
            var row = new List<PlacedClip>();
            button.onTouchBegin.Add(context =>
            {
                context.CaptureTouch();
                moved = false;
                originX = button.x;
                originW = button.width;
                pointerX = context.inputEvent.x;
                home = skill ? key.Frame : Mathf.RoundToInt(key.StartSeconds * LogicRate());
                edge = 0;
                row.Clear();
                if (skill) return;
                var parent = button.parent as GComponent;
                if (parent == null) return;
                var local = parent.GlobalToLocal(context.inputEvent.position);
                var grip = Mathf.Min(8f, button.width * 0.25f);
                if (local.x <= button.x + grip) edge = 1;
                else if (local.x >= button.x + button.width - grip) edge = 2;
                CaptureRow(button, row);
            });
            button.onTouchMove.Add(context =>
            {
                var parent = button.parent as GComponent;
                if (parent == null) return;
                var dx = LocalDelta(parent, context.inputEvent.x - pointerX, 0f).x;
                if (!moved && Mathf.Abs(dx) < 4f) return;
                moved = true;
                if (edge == 1)
                {
                    ResizeLeft(button, originX, originW, dx, px, home, key);
                    return;
                }

                if (edge == 2)
                {
                    ResizeRight(button, originX, originW, dx, px, row);
                    return;
                }

                var x = Mathf.Max(0f, originX + dx);
                x = SnapToProgress(x, px);
                if (!skill)
                    x = BlockedX(button, originX, x, button.width);

                button.x = x;
                if (skill)
                    button.title = Mathf.Max(0, Mathf.RoundToInt(x / px)).ToString();
            });
            button.onTouchEnd.Add(() =>
            {
                if (!moved) return;
                if (skill)
                {
                    key.Frame = Mathf.Max(0, Mathf.RoundToInt(button.x / px));
                    RecordLogic();
                }
                else
                {
                    if (edge == 0)
                        button.x = BlockedX(button, originX, button.x, button.width);
                    key.StartSeconds = ToSeconds(button.x, px);
                    key.DurationSeconds = Mathf.Max(ToSeconds(8f, px), ToSeconds(button.width, px));
                    if (edge != 0)
                    {
                        var track = TrackOf(key);
                        if (track != null)
                            PushTail(track, key);
                    }

                    RecordRender(TrackOf(key) == null ? TrackKind.Spine : TrackOf(key).Kind);
                }

                Timers.inst.CallLater(_ =>
                {
                    if (skill && edge == 0)
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
            _cInspect.selectedIndex = InspectEmpty;
            Label(_inspect, "inspectKind").text = "";
            _applying = false;
            SyncClipBoxes();
            var actor = SpellPreviewStage.Selected;
            if (actor != null)
                ShowUnitInspect(actor);
            else
                RestoreInspectEmpty();
            RefreshDeleteKey();
            RefreshPreview();
        }

        void ApplyInspector()
        {
            RestoreInspectEmpty();
            var track = _tracks[_track];
            var key = track.Keys[_key];
            _applying = true;
            if (track.Kind == TrackKind.Skill)
            {
                _cInspect.selectedIndex = InspectSkill;
                Label(_inspect, "inspectKind").text = "技能关键帧";
                Pick(key.Pick).selected = true;
                Condition(key.Condition).selected = true;
                RenderChoices(ListOf(_inspect, "tags"), _tags, key.Tags, Label(_inspect, "tagHint"), "载入的数据里还没有标签。");
                RenderChoices(ListOf(_inspect, "buffs"), _buffs, key.Buffs, Label(_inspect, "buffHint"), "载入的数据里还没有 Buff。");
            }
            else if (track.Kind == TrackKind.Spine)
            {
                _cInspect.selectedIndex = InspectSpine;
                Label(_inspect, "inspectKind").text = "Spine 关键帧";
                _cClipName.selectedIndex = key.ClipSource == 1 ? 1 : 0;
                Radio(_inspect, "clipByName").selected = key.ClipSource != 1;
                Radio(_inspect, "clipByTrigger").selected = key.ClipSource == 1;
                FillClipCombos(key);
                Input(_inspect, "inputSpeed").text = key.Speed;
                _cSpineEnd.selectedIndex = key.EndMode;
                Radio(_inspect, "endPause").selected = key.EndMode == 0;
                Radio(_inspect, "endLoop").selected = key.EndMode == 1;
                Radio(_inspect, "pauseFirst").selected = key.PauseAt == 0;
                Radio(_inspect, "pauseLast").selected = key.PauseAt == 1;
                (Part(_inspect, "pingpong") as UI_ButtonCheck).selected = key.Pingpong;
            }
            else
            {
                _cInspect.selectedIndex = InspectEmpty;
                Label(_inspect, "inspectKind").text = "";
            }

            _applying = false;
            SyncClipBoxes();
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
            LayoutTimeline();
        }

        void PlaceKeys(float px)
        {
            var keys = KeysPane();
            if (keys == null) return;
            RefreshCasterClips();

            for (var i = keys.numChildren - 1; i >= 0; i--)
            {
                var child = keys.GetChildAt(i);
                if (child.name == "extent") continue;
                child.Dispose();
            }
            for (var t = 0; t < _tracks.Count; t++)
            {
                var track = _tracks[t];
                for (var k = 0; k < track.Keys.Count; k++)
                {
                    var frame = track.Keys[k];
                    var skill = track.Kind == TrackKind.Skill;
                    var button = skill ? (GButton)UI_ItemKey.CreateInstance() : ClipButton();
                    button.title = skill ? frame.Frame.ToString() : ClipLabel(frame);
                    button.selected = t == _track && k == _key;
                    var start = skill ? frame.Frame / LogicRate() : frame.StartSeconds;
                    var length = ClipLengthSeconds(frame, skill);
                    button.SetXY(ToPixels(start, px), t * 44f + 6f);
                    button.SetSize(Mathf.Max(8f, ToPixels(length, px)), button.height);
                    BindKey(button, frame, px, skill);

                    keys.AddChild(button);
                }
            }
        }

        void LayoutTimeline()
        {
            BindTimelineScroll();
            var marks = Piece(_timeline, "marks");
            var tracks = Piece(_timeline, "tracks");
            var progress = Piece(_timeline, "progress");
            var keys = KeysPane();
            if (marks == null || tracks == null || progress == null || keys == null) return;

            var kept = ScrollPerc(keys);
            var laneX = tracks.x + keys.x;
            var laneW = keys.width;
            marks.SetXY(laneX, marks.y);
            marks.SetSize(laneW, marks.height);
            progress.SetXY(laneX, progress.y);
            progress.SetSize(laneW, progress.height);
            if (_scrollH != null)
            {
                _scrollH.SetXY(laneX, _scrollH.y);
                _scrollH.SetSize(laneW, _scrollH.height);
            }

            var contentH = _tracks.Count == 0 ? 0f : _tracks.Count * 40f + (_tracks.Count - 1) * 4f;
            var trackList = TrackList();
            if (trackList != null)
                trackList.SetSize(trackList.width, contentH);
            keys.SetSize(keys.width, Mathf.Max(contentH, 1f));

            var px = PaneView(keys, true) / 60f;
            if (px < 1f) px = 1f;
            var span = FrameSpan(px);
            var timeline = Mathf.CeilToInt(px * span);
            PlaceKeys(px);
            var content = timeline;
            for (var i = 0; i < keys.numChildren; i++)
            {
                var child = keys.GetChildAt(i);
                if (child.name == "extent") continue;
                content = Mathf.Max(content, Mathf.CeilToInt(child.x + child.width));
            }

            PlaceMarks(marks, px, span);
            for (var i = 0; i < marks.numChildren; i++)
            {
                var child = marks.GetChildAt(i);
                if (child.name != "tick") continue;
                content = Mathf.Max(content, Mathf.CeilToInt(child.x + child.width));
            }

            var markList = MarkList();
            if (markList != null)
            {
                markList.visible = false;
                markList.SetSize(0f, 0f);
            }

            var extent = keys.GetChild("extent");
            if (extent != null)
                extent.SetSize(content, Mathf.Max(contentH, keys.height));

            var slider = progress.GetChild("progress") as GSlider;
            if (slider != null)
            {
                _ignoreProgress = true;
                slider.x = 0f;
                slider.max = span / LogicRate();
                slider.SetSize(timeline, slider.height);
                slider.opaque = true;
                _ignoreProgress = false;
                BindProgress(slider);
            }

            Pad(marks, content);
            Pad(progress, content);
            marks.EnsureBoundsCorrect();
            tracks.EnsureBoundsCorrect();
            progress.EnsureBoundsCorrect();
            keys.EnsureBoundsCorrect();
            _scrollable = SharedScrollable(marks, keys, progress);
            Stretch(marks, "pad");
            Stretch(keys, "extent");
            Stretch(progress, "pad");
            ApplyScrollX(kept);
            UpdateScrollBars();
            Timers.inst.CallLater(RefreshScrollLater);
        }

        void PlaceMarks(GComponent marks, float px, int span)
        {
            for (var i = marks.numChildren - 1; i >= 0; i--)
            {
                var child = marks.GetChildAt(i);
                if (child.name == "tick")
                    child.Dispose();
            }

            const int step = 12;
            for (var frame = 0; frame <= span; frame += step)
            {
                var item = UI_ItemMark.CreateInstance();
                item.name = "tick";
                item.touchable = false;
                item.SetXY(frame * px, 0f);
                item.m_mark0.SetSize(64f, 28f);
                var seconds = frame / LogicRate();
                item.m_mark0.text = frame + "\n" + seconds.ToString("0.00") + "s";
                marks.AddChild(item);
            }
        }

        static void Pad(GComponent host, float width)
        {
            var pad = host.GetChild("pad");
            if (pad == null)
            {
                var graph = new GGraph();
                graph.name = "pad";
                graph.touchable = false;
                host.AddChild(graph);
                pad = graph;
            }

            pad.SetXY(0f, 0f);
            pad.SetSize(width, 1f);
        }

        static float ScrollPerc(GComponent keys)
        {
            var pane = keys?.scrollPane;
            if (pane == null || pane.contentWidth <= pane.viewWidth + 1f)
                return 0f;
            return pane.percX;
        }

        static float SharedScrollable(GComponent marks, GComponent keys, GComponent progress)
        {
            return Mathf.Max(Scrollable(marks), Mathf.Max(Scrollable(keys), Scrollable(progress)));
        }

        static float Scrollable(GComponent com)
        {
            var pane = com?.scrollPane;
            if (pane == null) return 0f;
            return Mathf.Max(0f, pane.contentWidth - pane.viewWidth);
        }

        void Stretch(GComponent com, string spacer)
        {
            var pane = com?.scrollPane;
            var child = com?.GetChild(spacer);
            if (pane == null || child == null) return;
            var desired = pane.viewWidth + _scrollable;
            if (pane.contentWidth + 0.5f >= desired) return;
            child.SetSize(desired, Mathf.Max(child.height, 1f));
            com.EnsureBoundsCorrect();
        }

        void RefreshScrollLater(object param)
        {
            UpdateScrollBars();
        }

        void BindTimelineScroll()
        {
            if (_scrollReady) return;
            var marks = Piece(_timeline, "marks");
            var tracks = Piece(_timeline, "tracks");
            var progress = Piece(_timeline, "progress");
            var keys = KeysPane();
            _scrollH = Piece(_timeline, "scrollH");
            _scrollV = Piece(_timeline, "scrollV");
            if (marks == null || tracks == null || progress == null || keys == null || _scrollH == null || _scrollV == null)
                return;

            Hold(marks);
            Hold(tracks);
            Hold(progress);
            Hold(keys);
            progress.opaque = false;
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
            var pos = _scrollable * Mathf.Clamp01(perc);
            SetPosX(Piece(_timeline, "marks"), pos);
            SetPosX(KeysPane(), pos);
            SetPosX(Piece(_timeline, "progress"), pos);
        }

        void ApplyScrollY(float perc)
        {
            var pane = Piece(_timeline, "tracks")?.scrollPane;
            if (pane == null) return;
            pane.percY = perc;
        }

        static void SetPosX(GComponent com, float pos)
        {
            var pane = com?.scrollPane;
            if (pane == null) return;
            pane.posX = pos;
        }

        void UpdateScrollBars()
        {
            var keys = KeysPane();
            var tracks = Piece(_timeline, "tracks");
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
            var host = Part(_timeline, "marks");
            var nested = (host as GComponent)?.GetChild("marks") as GList;
            return nested ?? host as GList;
        }

        GList TrackList()
        {
            var host = Part(_timeline, "tracks");
            var nested = (host as GComponent)?.GetChild("tracks") as GList;
            return nested ?? host as GList;
        }

        GComponent KeysPane()
        {
            return Piece(_timeline, "tracks")?.GetChild("keys") as GComponent;
        }

        float PaneView(GComponent com, bool horizontal)
        {
            var pane = com?.scrollPane;
            if (pane == null) return com == null ? 0f : horizontal ? com.width : com.height;
            var size = horizontal ? pane.viewWidth : pane.viewHeight;
            return size > 1f ? size : horizontal ? com.width : com.height;
        }

        int ClipSpan(float px)
        {
            return Mathf.Max(1, Mathf.CeilToInt(64f / Mathf.Max(px, 1f)));
        }

        int ClipFrames(KeyFrame key)
        {
            if (key.Span > 0) return key.Span;
            var keys = KeysPane();
            var px = keys == null ? 8f : PaneView(keys, true) / 60f;
            if (px < 1f) px = 1f;
            return ClipSpan(px);
        }

        void RefreshCasterClips()
        {
            SpellPreviewStage.CopyCasterAnimations(_animations);
            SpellPreviewStage.CopyCasterTriggers(_triggers);
        }

        GButton ClipButton()
        {
            var clip = UI_ItemClip.CreateInstance();
            return clip != null ? clip : UI_ItemKey.CreateInstance();
        }

        string ClipLabel(KeyFrame key)
        {
            var name = key.ClipSource == 1 ? ChoiceLabel(_triggers, key.Trigger) : ChoiceLabel(_animations, key.Anim);
            if (string.IsNullOrEmpty(name))
                name = key.ClipSource == 1 ? key.Trigger : key.Anim;
            if (string.IsNullOrEmpty(name))
                name = key.StartSeconds.ToString("0.00") + "s";
            var length = ClipLengthSeconds(key, false);
            return name + "  " + key.StartSeconds.ToString("0.00") + "s · " + length.ToString("0.00") + "s";
        }

        static string ChoiceLabel(List<SpellPreviewStage.SpineClipChoice> options, string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].Id == id) return options[i].Label;
            }

            return id;
        }

        float SecondsOf(KeyFrame key)
        {
            var id = key.ClipSource == 1 ? key.Trigger : key.Anim;
            if (string.IsNullOrEmpty(id)) return -1f;
            var options = key.ClipSource == 1 ? _triggers : _animations;
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].Id == id) return options[i].Seconds;
            }

            return -1f;
        }

        static float ScaledSeconds(float seconds, string speedText)
        {
            var speed = 1f;
            if (float.TryParse(speedText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) && parsed > 0.0001f)
                speed = parsed;
            return Mathf.Max(0.01f, seconds / speed);
        }

        void FitLength(KeyFrame key)
        {
            RefreshCasterClips();
            var seconds = SecondsOf(key);
            if (seconds >= 0f)
            {
                key.DurationSeconds = ScaledSeconds(seconds, key.Speed);
                var track = TrackOf(key);
                if (track != null)
                    PushTail(track, key);
            }

            RenderTracks();
            var trackKind = TrackOf(key);
            RecordRender(trackKind == null ? TrackKind.Spine : trackKind.Kind);
        }

        Track TrackOf(KeyFrame key)
        {
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Keys.Contains(key)) return _tracks[i];
            }

            return null;
        }

        void PushTail(Track track, KeyFrame origin)
        {
            var keys = track.Keys;
            var ordered = new List<KeyFrame>(keys);
            ordered.Sort((a, b) =>
            {
                var time = a.StartSeconds.CompareTo(b.StartSeconds);
                if (time != 0) return time;
                if (a == origin) return -1;
                if (b == origin) return 1;
                return keys.IndexOf(a).CompareTo(keys.IndexOf(b));
            });

            var start = ordered.IndexOf(origin);
            if (start < 0) return;
            var end = origin.StartSeconds + ClipLengthSeconds(origin, false);
            for (var i = start + 1; i < ordered.Count; i++)
            {
                var key = ordered[i];
                if (key.StartSeconds >= end - 0.0001f) break;
                key.StartSeconds = end;
                end = key.StartSeconds + ClipLengthSeconds(key, false);
            }
        }

        void EnsureClipBoxes()
        {
            _comboAnim = ClipBox("comboAnim");
            _comboTrigger = ClipBox("comboTrigger");
            SyncClipBoxes();
        }

        GComboBox ClipBox(string name)
        {
            var existing = Part(_inspect, name) as GComboBox;
            if (existing != null) return existing;
            var box = UI_ComboValue.CreateInstance();
            box.name = name;
            box.SetSize(264, 32);
            if (_inspect != null)
            {
                box.SetXY(28, 192);
                _inspect.AddChild(box);
            }
            else
            {
                box.SetXY(1308, 192);
                view.AddChild(box);
            }

            return box;
        }

        void SyncClipBoxes()
        {
            var spine = _cInspect.selectedIndex == InspectSpine;
            var byName = _cClipName.selectedIndex != 1;
            if (_comboAnim != null) _comboAnim.visible = spine && byName;
            if (_comboTrigger != null) _comboTrigger.visible = spine && !byName;
            HideClipField("inputAnim");
            HideClipField("inputTrigger");
            HideClipField("bgAnim");
            HideClipField("bgTrigger");
        }

        void HideClipField(string name)
        {
            var child = Part(_inspect, name);
            if (child != null)
                child.visible = false;
        }

        void FillClipCombos(KeyFrame key)
        {
            RefreshCasterClips();
            var applying = _applying;
            _applying = true;
            FillClipBox(_comboAnim, _animations, key.Anim, "选择动画", value => PickClip(0, value));
            FillClipBox(_comboTrigger, _triggers, key.Trigger, "选择触发器", value => PickClip(1, value));
            _applying = applying;
        }

        void FillClipBox(
            GComboBox box,
            List<SpellPreviewStage.SpineClipChoice> options,
            string current,
            string prompt,
            Action<string> pick)
        {
            if (box == null) return;
            var known = false;
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].Id == current)
                {
                    known = true;
                    break;
                }
            }

            var extra = string.IsNullOrEmpty(current) || !known ? 1 : 0;
            var labels = new string[options.Count + extra];
            var values = new string[options.Count + extra];
            var index = 0;
            if (extra == 1)
            {
                labels[0] = string.IsNullOrEmpty(current) ? (options.Count == 0 ? "预览单位没有这项" : prompt) : current;
                values[0] = current ?? "";
                index = 1;
            }

            for (var i = 0; i < options.Count; i++)
            {
                labels[index] = options[i].Label;
                values[index] = options[i].Id;
                index++;
            }

            box.onChanged.Set(() => { });
            box.items = labels;
            box.values = values;
            box.value = current ?? "";
            box.onChanged.Set(() =>
            {
                if (pick != null) pick(box.value ?? "");
            });
        }

        void PickClip(int source, string value)
        {
            if (_applying || _key < 0) return;
            if (_track < 0 || _track >= _tracks.Count || _tracks[_track].Kind != TrackKind.Spine) return;
            var key = Current();
            key.ClipSource = source;
            if (source == 0) key.Anim = value ?? "";
            else key.Trigger = value ?? "";
            _cClipName.selectedIndex = source;
            FitLength(key);
        }

        static void CaptureRow(GButton button, List<PlacedClip> into)
        {
            into.Clear();
            var parent = button.parent;
            if (parent == null) return;
            for (var i = 0; i < parent.numChildren; i++)
            {
                var other = parent.GetChildAt(i);
                if (other == button || other.name == "extent") continue;
                if (Mathf.Abs(other.y - button.y) > 2f) continue;
                into.Add(new PlacedClip { View = other, X = other.x, Width = other.width });
            }

            into.Sort((a, b) => a.X.CompareTo(b.X));
        }

        void ResizeLeft(GButton button, float originX, float originW, float dx, float px, int home, KeyFrame key)
        {
            var right = originX + originW;
            var start = Mathf.Max(0f, originX + dx);
            var wall = 0f;
            var track = TrackOf(key);
            if (track != null)
            {
                var homeSeconds = home / LogicRate();
                var keys = track.Keys;
                for (var i = 0; i < keys.Count; i++)
                {
                    var other = keys[i];
                    if (other == key || other.StartSeconds >= homeSeconds - 0.0001f) continue;
                    var otherEnd = ToPixels(other.StartSeconds + ClipLengthSeconds(other, false), px);
                    if (otherEnd > wall) wall = otherEnd;
                }
            }

            if (start < wall) start = wall;
            var minWidth = 8f;
            if (right - start < minWidth) start = right - minWidth;
            if (start < 0f) start = 0f;
            var progressX = ToPixels(PreviewTime(), px);
            if (Mathf.Abs(start - progressX) <= SnapRange(px)
                && progressX >= wall
                && right - progressX >= minWidth)
                start = progressX;
            button.x = start;
            button.SetSize(Mathf.Max(minWidth, right - start), button.height);
        }

        void ResizeRight(GButton button, float originX, float originW, float dx, float px, List<PlacedClip> row)
        {
            var width = Mathf.Max(8f, originW + dx);
            var end = originX + width;
            var progressX = ToPixels(PreviewTime(), px);
            if (Mathf.Abs(end - progressX) <= SnapRange(px) && progressX >= originX + 8f)
            {
                end = progressX;
                width = end - originX;
            }

            button.x = originX;
            button.SetSize(width, button.height);
            var cursor = originX + width;
            for (var i = 0; i < row.Count; i++)
            {
                var other = row[i];
                if (other.View == null || other.X + 0.5f < originX) continue;
                var next = Mathf.Max(cursor, other.X);
                other.View.x = next;
                other.View.SetSize(Mathf.Max(8f, other.Width), other.View.height);
                cursor = other.View.x + other.View.width;
            }
        }

        float BlockedX(GButton button, float homeX, float desiredX, float width)
        {
            desiredX = Mathf.Max(0f, desiredX);
            if (width < 8f) width = 8f;
            var parent = button.parent;
            if (parent == null) return desiredX;

            var occupied = new List<float>();
            for (var i = 0; i < parent.numChildren; i++)
            {
                var other = parent.GetChildAt(i);
                if (other == button || other.name == "extent") continue;
                if (Mathf.Abs(other.y - button.y) > 2f) continue;
                occupied.Add(other.x);
                occupied.Add(other.x + Mathf.Max(8f, other.width));
            }

            if (occupied.Count == 0) return desiredX;
            for (var i = 2; i < occupied.Count; i += 2)
            {
                var start = occupied[i];
                var end = occupied[i + 1];
                var j = i;
                while (j > 0 && occupied[j - 2] > start)
                {
                    occupied[j] = occupied[j - 2];
                    occupied[j + 1] = occupied[j - 1];
                    j -= 2;
                }

                occupied[j] = start;
                occupied[j + 1] = end;
            }

            var merged = new List<float>();
            for (var i = 0; i < occupied.Count; i += 2)
            {
                if (merged.Count == 0 || occupied[i] > merged[merged.Count - 1] + 0.5f)
                {
                    merged.Add(occupied[i]);
                    merged.Add(occupied[i + 1]);
                }
                else if (occupied[i + 1] > merged[merged.Count - 1])
                {
                    merged[merged.Count - 1] = occupied[i + 1];
                }
            }

            var best = desiredX;
            var bestDist = float.MaxValue;
            var found = false;
            Consider(0f, merged[0] - width);
            for (var i = 0; i < merged.Count - 2; i += 2)
                Consider(merged[i + 1], merged[i + 2] - width);
            Consider(merged[merged.Count - 1], float.MaxValue);
            return found ? best : desiredX;

            void Consider(float lo, float hi)
            {
                if (hi < 0f || hi < lo) return;
                if (lo < 0f) lo = 0f;
                var candidate = desiredX < lo ? lo : desiredX > hi ? hi : desiredX;
                var dist = Mathf.Abs(candidate - desiredX);
                if (found && dist > bestDist + 0.01f) return;
                if (found && Mathf.Abs(dist - bestDist) <= 0.01f && Mathf.Abs(candidate - homeX) >= Mathf.Abs(best - homeX)) return;
                best = candidate;
                bestDist = dist;
                found = true;
            }
        }

        int BlockedStart(GButton button, int home, int desired, int span, float px)
        {
            desired = Mathf.Max(0, desired);
            if (span < 1) span = 1;
            var parent = button.parent;
            if (parent == null || px < 1f) return desired;

            var occupied = new List<int>();
            for (var i = 0; i < parent.numChildren; i++)
            {
                var other = parent.GetChildAt(i);
                if (other == button || other.name == "extent") continue;
                if (Mathf.Abs(other.y - button.y) > 2f) continue;
                var otherStart = Mathf.RoundToInt(other.x / px);
                var otherSpan = Mathf.Max(1, Mathf.RoundToInt(other.width / px));
                occupied.Add(otherStart);
                occupied.Add(otherStart + otherSpan);
            }

            if (occupied.Count == 0) return desired;
            for (var i = 2; i < occupied.Count; i += 2)
            {
                var start = occupied[i];
                var end = occupied[i + 1];
                var j = i;
                while (j > 0 && occupied[j - 2] > start)
                {
                    occupied[j] = occupied[j - 2];
                    occupied[j + 1] = occupied[j - 1];
                    j -= 2;
                }

                occupied[j] = start;
                occupied[j + 1] = end;
            }

            var merged = new List<int>();
            for (var i = 0; i < occupied.Count; i += 2)
            {
                if (merged.Count == 0 || occupied[i] > merged[merged.Count - 1])
                {
                    merged.Add(occupied[i]);
                    merged.Add(occupied[i + 1]);
                }
                else if (occupied[i + 1] > merged[merged.Count - 1])
                {
                    merged[merged.Count - 1] = occupied[i + 1];
                }
            }

            var best = desired;
            var bestDist = int.MaxValue;
            var found = false;
            Consider(0, merged[0] - span);
            for (var i = 0; i < merged.Count - 2; i += 2)
                Consider(merged[i + 1], merged[i + 2] - span);
            Consider(merged[merged.Count - 1], int.MaxValue);
            return found ? best : desired;

            void Consider(int lo, int hi)
            {
                if (hi < 0 || hi < lo) return;
                if (lo < 0) lo = 0;
                var candidate = desired < lo ? lo : desired > hi ? hi : desired;
                var dist = Mathf.Abs(candidate - desired);
                if (found && dist > bestDist) return;
                if (found && dist == bestDist && Mathf.Abs(candidate - home) >= Mathf.Abs(best - home)) return;
                best = candidate;
                bestDist = dist;
                found = true;
            }
        }

        int ContentFrames()
        {
            var frames = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var keys = _tracks[i].Keys;
                var skill = _tracks[i].Kind == TrackKind.Skill;
                for (var k = 0; k < keys.Count; k++)
                {
                    var end = skill
                        ? keys[k].Frame
                        : Mathf.CeilToInt((keys[k].StartSeconds + ClipLengthSeconds(keys[k], false)) * LogicRate());
                    if (end > frames) frames = end;
                }
            }

            return frames;
        }

        int FrameSpan(float px)
        {
            var span = 60;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var keys = _tracks[i].Keys;
                var skill = _tracks[i].Kind == TrackKind.Skill;
                for (var k = 0; k < keys.Count; k++)
                {
                    var end = skill
                        ? keys[k].Frame + 1
                        : Mathf.CeilToInt((keys[k].StartSeconds + ClipLengthSeconds(keys[k], false)) * LogicRate());
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

                    if (list == ListOf(_inspect, "tags")) RefreshPreview();
                    if (_track >= 0 && _track < _tracks.Count && _tracks[_track].Kind == TrackKind.Skill)
                        RecordLogic();
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
                RecordLogic();
            });
        }

        void BindCondition(UI_ButtonRadio radio, int index)
        {
            radio.onChanged.Add(() =>
            {
                if (_applying || _key < 0 || !radio.selected) return;
                if (_tracks[_track].Kind != TrackKind.Skill) return;
                Current().Condition = index;
                RecordLogic();
            });
        }

        void SaveEnd(int mode, bool selected)
        {
            if (_applying || _key < 0 || !selected) return;
            if (_tracks[_track].Kind != TrackKind.Spine) return;
            Current().EndMode = mode;
            _cSpineEnd.selectedIndex = mode;
            RecordRender(TrackKind.Spine);
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
                case 0: return Radio(_inspect, "pickOwner");
                case 2: return Radio(_inspect, "pickSubject");
                case 3: return Radio(_inspect, "pickSource");
                case 4: return Radio(_inspect, "pickExplicit");
                default: return Radio(_inspect, "pickPrimary");
            }
        }

        UI_ButtonRadio Condition(int index)
        {
            switch (index)
            {
                case 1: return Radio(_inspect, "condAny");
                case 2: return Radio(_inspect, "condNotAny");
                default: return Radio(_inspect, "condAll");
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
            Label(_func, "unitName").text = _owner < 0
                ? "没有单位拥有这个技能"
                : new SpellId(_owners[_owner].Id).ToString();
            var owner = CurrentOwner();
            SpellPreviewStage.SetCaster(owner);
            if (_key >= 0 && _track >= 0 && _track < _tracks.Count && _tracks[_track].Kind == TrackKind.Spine)
                FillClipCombos(Current());
            var id = owner == null ? 0UL : owner.Id;
            if (_entered && id != _shownOwner)
                RecordLogic();
            _shownOwner = id;
        }

        EditorUnit CurrentOwner()
        {
            return _owner < 0 || _owner >= _owners.Count ? null : _owners[_owner];
        }

        void BindProgress(GSlider slider)
        {
            if (_progressBound || slider == null) return;
            _progressBound = true;
            slider.onChanged.Add(() =>
            {
                if (_ignoreProgress) return;
                Scrub();
            });
        }

        void Scrub()
        {
            if (_editing)
                ShowPreviewMode();
            ApplyWorld();
            ShowAt(PreviewTime(), false, 0);
        }

        void ShowPreviewMode()
        {
            var host = view.GetChild("modeToggle") as GComponent;
            var mode = host == null ? null : host.GetController("cMode");
            if (mode != null && mode.selectedIndex != 1)
            {
                mode.selectedIndex = 1;
                return;
            }

            ApplyMode(false);
        }

        void RebuildPreview()
        {
            if (_previewCatalog.IsCreated)
                _previewCatalog.Dispose();
            _previewCatalog = default;
            if (_spellId == 0) return;

            var spell = ProjectSession.EnsureOwnSpell(_spellId);
            var activation = SpellActivation.Active;
            var duration = 0;
            var cooldown = 0;
            if (spell != null)
            {
                if (spell.Activation == (int)SpellActivation.Passive)
                    activation = SpellActivation.Passive;
                else
                {
                    activation = SpellActivation.Active;
                    duration = Mathf.Max(0, spell.DurationFrames);
                    cooldown = Mathf.Max(0, spell.CooldownFrames);
                }
            }

            var definition = new SpellDefinitionData
            {
                Id = new SpellId(_spellId),
                Activation = activation,
                DurationFrames = duration,
                CooldownFrames = cooldown,
            };
            var lane = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var track = _tracks[i];
                if (track.Kind != TrackKind.Spine)
                    continue;
                for (var k = 0; k < track.Keys.Count; k++)
                {
                    var key = track.Keys[k];
                    if (!TryClipAnimation(key, out var animation)) continue;
                    var config = new SpineSpellTrackConfig
                    {
                        AnimationName = animation,
                        Loop = key.EndMode == 1,
                        TrackIndex = lane,
                    };
                    definition.PresentationTracks.Add(new SpellPresentationTrackAuthoring
                    {
                        TypeId = SpineSpellTracks.TypeId,
                        StartSeconds = Mathf.Max(0f, key.StartSeconds),
                        DurationSeconds = Mathf.Max(0.01f, ClipLengthSeconds(key, false)),
                        Parameters = config.ToByteArray(),
                    });
                }

                lane++;
            }

            try
            {
                _previewCatalog = SpellCatalog.Build(
                    new[] { definition },
                    Array.Empty<BuffId>(),
                    Allocator.Persistent);
                var rate = ProjectSession.LogicFrameRate;
                if (rate == 0) rate = ProjectSession.DefaultLogicFrameRate;
                SpellPresentation.SetFrameDuration(1f / rate);
                SpineSpellTracks.EnsureRegistered();
                SpellPresentation.Prepare(_previewCatalog);
            }
            catch (Exception error)
            {
                if (_previewCatalog.IsCreated)
                    _previewCatalog.Dispose();
                _previewCatalog = default;
                Debug.LogWarning(error.Message);
            }
        }

        bool TryClipAnimation(KeyFrame key, out string animation)
        {
            animation = "";
            if (key.ClipSource == 1)
                return SpellPreviewStage.TryCasterTriggerAnimation(key.Trigger, out animation);
            animation = key.Anim ?? "";
            return animation.Length > 0;
        }

        void ShowAt(float time, bool play, ulong typeId)
        {
            if (_showingPreview || _spellId == 0) return;
            if (!_previewCatalog.IsCreated)
                RebuildPreview();
            if (!_previewCatalog.IsCreated) return;
            var owner = CurrentOwner();
            if (owner == null || owner.Id == 0) return;
            if (!SpellPreviewStage.TryBindCaster(owner.Id)) return;

            _showingPreview = true;
            try
            {
                var spell = new SpellId(_spellId);
                if (typeId == 0)
                    SpellPresentation.ShowTime(_previewCatalog, spell, owner.Id, Mathf.Max(0f, time), play);
                else
                    SpellPresentation.RefreshType(_previewCatalog, spell, owner.Id, new SpellPresentationTrackId(typeId), Mathf.Max(0f, time), play);
            }
            catch (Exception error)
            {
                Debug.LogWarning(error.Message);
            }
            finally
            {
                _showingPreview = false;
            }
        }

        void ReleasePreview()
        {
            var owner = CurrentOwner();
            if (_previewCatalog.IsCreated && owner != null && owner.Id != 0)
            {
                try
                {
                    SpellPresentation.ShowTime(_previewCatalog, new SpellId(_spellId), owner.Id, 1000000f, false);
                }
                catch (Exception)
                {
                }

                try
                {
                    SpineSpellTracks.Bind(owner.Id, null);
                }
                catch (Exception)
                {
                }
            }

            if (_previewCatalog.IsCreated)
                _previewCatalog.Dispose();
            _previewCatalog = default;
            _samples.Clear();
        }

        float LogicRate()
        {
            var rate = ProjectSession.LogicFrameRate;
            if (rate == 0) rate = ProjectSession.DefaultLogicFrameRate;
            return rate;
        }

        float ToPixels(float seconds, float px)
        {
            return Mathf.Max(0f, seconds) * px * LogicRate();
        }

        float SnapRange(float px)
        {
            var limit = ToPixels(0.08f, px);
            var pixels = 8f;
            if (pixels > limit)
                pixels = limit;
            if (pixels < 2f)
                pixels = 2f;
            return pixels;
        }

        float SnapToProgress(float x, float px)
        {
            var progressX = ToPixels(PreviewTime(), px);
            if (Mathf.Abs(x - progressX) <= SnapRange(px))
                return progressX;
            return x;
        }

        float ToSeconds(float pixels, float px)
        {
            return Mathf.Max(0f, pixels) / (px * LogicRate());
        }

        float ClipLengthSeconds(KeyFrame key, bool skill)
        {
            if (skill) return 1f / LogicRate();
            if (key.DurationSeconds > 0.0001f) return key.DurationSeconds;
            var keys = KeysPane();
            var px = keys == null ? 8f : PaneView(keys, true) / 60f;
            if (px < 1f) px = 1f;
            return ClipSpan(px) / LogicRate();
        }

        float PreviewTime()
        {
            var host = Part(_timeline, "progress");
            var slider = host as GSlider ?? (host as GComponent)?.GetChild("progress") as GSlider;
            if (slider == null) return 0f;
            return Mathf.Max(0f, (float)slider.value);
        }

        void OnStageChanged()
        {
            if (!_entered) return;
            SaveLayout();
            RecordLogic();
        }

        void PoseAll()
        {
            RebuildPreview();
            ShowAt(PreviewTime(), false, 0);
            ApplyWorld();
        }

        void ApplyWorld()
        {
            if (_editing || _samples.Count == 0) return;
            var frame = Mathf.FloorToInt(PreviewTime() * LogicRate() + 0.0001f);
            if (frame < 0) frame = 0;
            if (frame >= _samples.Count) frame = _samples.Count - 1;
            SpellPreviewStage.ApplyFrame(_samples[frame]);
            var actor = SpellPreviewStage.Selected;
            if (actor != null && _key < 0)
                ShowUnitInspect(actor);
        }

        void RecordLogic()
        {
            if (!_entered || _spellId == 0) return;
            WriteLogic();
            SpellPreviewStage.CopySeeds(_seeds);
            var frames = ContentFrames();
            var next = SpellPreviewSim.Run(_spellId, Mathf.Max(1, frames + 1), _seeds);
            _samples.Clear();
            _samples.AddRange(next);
            ApplyWorld();
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
        }

        void RecordRender(TrackKind kind)
        {
            if (!_entered || _spellId == 0) return;
            WritePresentation();
            RebuildPreview();
            ShowAt(PreviewTime(), false, TypeOf(kind));
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
        }

        void WriteLogic()
        {
            var spell = ProjectSession.EnsureOwnSpell(_spellId);
            if (spell == null) return;
            var frames = ContentFrames();
            if (spell.Activation != (int)SpellActivation.Passive && frames > 0)
                spell.DurationFrames = frames;
            spell.Events.Clear();
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill) continue;
                var keys = _tracks[i].Keys;
                for (var k = 0; k < keys.Count; k++)
                    spell.Events.Add(ToEvent(keys[k]));
            }

            for (var i = 0; i < _extraEvents.Count; i++)
                spell.Events.Add(_extraEvents[i].Clone());
        }

        static SpellEventConfig ToEvent(KeyFrame key)
        {
            var evt = new SpellEventConfig();
            if (key.Frame <= 0)
            {
                evt.Trigger = (int)SpellEventTrigger.OnStart;
                evt.Frame = 0;
            }
            else
            {
                evt.Trigger = (int)SpellEventTrigger.Frame;
                evt.Frame = key.Frame;
            }

            for (var i = 0; i < key.Tags.Count; i++) evt.Tags.Add(key.Tags[i]);
            for (var i = 0; i < key.Buffs.Count; i++) evt.BuffIds.Add(key.Buffs[i]);
            return evt;
        }

        void WritePresentation()
        {
            var spell = ProjectSession.EnsureOwnSpell(_spellId);
            if (spell == null) return;
            spell.PresentationTracks.Clear();
            var lane = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var track = _tracks[i];
                var type = TypeOf(track.Kind);
                if (type == 0) continue;
                for (var k = 0; k < track.Keys.Count; k++)
                {
                    var key = track.Keys[k];
                    var item = new SpellPresentationTrack
                    {
                        TypeId = type,
                        StartSeconds = Mathf.Max(0f, key.StartSeconds),
                        DurationSeconds = Mathf.Max(0.01f, ClipLengthSeconds(key, false)),
                    };
                    if (track.Kind == TrackKind.Spine)
                    {
                        if (!TryClipAnimation(key, out var animation)) continue;
                        var config = new SpineSpellTrackConfig
                        {
                            AnimationName = animation,
                            Loop = key.EndMode == 1,
                            TrackIndex = lane,
                        };
                        item.Parameters = config.ToByteString();
                    }
                    else if (key.Parameters != null && key.Parameters.Length > 0)
                    {
                        item.Parameters = Google.Protobuf.ByteString.CopyFrom(key.Parameters);
                    }
                    else
                    {
                        continue;
                    }

                    spell.PresentationTracks.Add(item);
                }

                if (track.Kind == TrackKind.Spine)
                    lane++;
            }

            for (var i = 0; i < _extraTracks.Count; i++)
                spell.PresentationTracks.Add(_extraTracks[i].Clone());
        }

        void LoadTracks()
        {
            _extraEvents.Clear();
            _extraTracks.Clear();
            var spell = ProjectSession.EnsureOwnSpell(_spellId);
            if (spell == null) return;
            var skill = SkillTrack();
            for (var i = 0; i < spell.Events.Count; i++)
            {
                var evt = spell.Events[i];
                var trigger = (SpellEventTrigger)evt.Trigger;
                if (trigger != SpellEventTrigger.OnStart && trigger != SpellEventTrigger.Frame)
                {
                    _extraEvents.Add(evt.Clone());
                    continue;
                }

                var key = new KeyFrame { Frame = trigger == SpellEventTrigger.OnStart ? 0 : evt.Frame };
                key.Tags.AddRange(evt.Tags);
                key.Buffs.AddRange(evt.BuffIds);
                skill.Keys.Add(key);
            }

            if (skill.Keys.Count > 1)
                PackSkillTracks();

            for (var i = 0; i < spell.PresentationTracks.Count; i++)
            {
                var source = spell.PresentationTracks[i];
                var kind = KindOf(source.TypeId);
                var row = Row(kind);
                if (row == null)
                {
                    _extraTracks.Add(source.Clone());
                    continue;
                }

                var key = new KeyFrame
                {
                    StartSeconds = source.StartSeconds,
                    DurationSeconds = source.DurationSeconds,
                    Parameters = source.Parameters == null || source.Parameters.Length == 0
                        ? Array.Empty<byte>()
                        : source.Parameters.ToByteArray(),
                };
                if (kind == TrackKind.Spine && source.Parameters != null && source.Parameters.Length > 0)
                {
                    var config = SpineSpellTrackConfig.Parser.ParseFrom(source.Parameters);
                    key.Anim = config.AnimationName ?? "";
                    key.EndMode = config.Loop ? 1 : 0;
                }

                row.Keys.Add(key);
            }

            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind == TrackKind.Skill || _tracks[i].Keys.Count == 0) continue;
                PushTail(_tracks[i], _tracks[i].Keys[0]);
            }
        }

        Track Row(TrackKind kind)
        {
            if (kind == TrackKind.Skill) return null;
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind == kind) return _tracks[i];
            }

            return null;
        }

        static ulong TypeOf(TrackKind kind)
        {
            switch (kind)
            {
                case TrackKind.Particles: return ParticleSpellTracks.TypeId.Value;
                case TrackKind.Vfx: return VfxSpellTracks.TypeId.Value;
                case TrackKind.Spine: return SpineSpellTracks.TypeId.Value;
                case TrackKind.Animation: return AnimationSpellTracks.TypeId.Value;
                default: return 0;
            }
        }

        static TrackKind KindOf(ulong typeId)
        {
            if (typeId == ParticleSpellTracks.TypeId.Value) return TrackKind.Particles;
            if (typeId == VfxSpellTracks.TypeId.Value) return TrackKind.Vfx;
            if (typeId == SpineSpellTracks.TypeId.Value) return TrackKind.Spine;
            if (typeId == AnimationSpellTracks.TypeId.Value) return TrackKind.Animation;
            return TrackKind.Skill;
        }

        void SaveLayout()
        {
            var archive = ProjectSession.Archive;
            if (archive == null || _spellId == 0) return;
            SpellPreviewLayout layout = null;
            for (var i = 0; i < archive.SpellPreviews.Count; i++)
            {
                if (archive.SpellPreviews[i].SpellId != _spellId) continue;
                layout = archive.SpellPreviews[i];
                break;
            }

            if (layout == null)
            {
                layout = new SpellPreviewLayout { SpellId = _spellId };
                archive.SpellPreviews.Add(layout);
            }

            layout.Actors.Clear();
            SpellPreviewStage.CopySeeds(_seeds);
            for (var i = 0; i < _seeds.Count; i++)
            {
                var seed = _seeds[i];
                var record = new SpellPreviewActorRecord
                {
                    StableId = seed.StableId,
                    X = seed.Position.x,
                    Y = seed.Position.y,
                    Z = seed.Position.z,
                };
                var owner = CurrentOwner();
                if (owner != null && owner.Id == seed.StableId)
                {
                    record.UnitId = owner.Id;
                    if (owner.PresetAttributes.Count > 0)
                        record.PresetId = owner.PresetAttributes[0];
                }

                record.Tags.AddRange(seed.Tags);
                record.Values.AddRange(seed.Values);
                layout.Actors.Add(record);
            }
        }

        void RestoreLayout()
        {
            var archive = ProjectSession.Archive;
            if (archive == null || _spellId == 0) return;
            SpellPreviewLayout layout = null;
            for (var i = 0; i < archive.SpellPreviews.Count; i++)
            {
                if (archive.SpellPreviews[i].SpellId != _spellId) continue;
                layout = archive.SpellPreviews[i];
                break;
            }

            if (layout == null) return;
            var owner = CurrentOwner();
            for (var i = 0; i < layout.Actors.Count; i++)
            {
                var record = layout.Actors[i];
                var caster = owner != null && (record.UnitId == owner.Id || record.StableId == owner.Id);
                if (caster)
                {
                    var actors = UnityEngine.Object.FindObjectsByType<PreviewActor>(FindObjectsSortMode.None);
                    for (var a = 0; a < actors.Length; a++)
                    {
                        if (actors[a] == null || actors[a].StableId != owner.Id) continue;
                        actors[a].transform.position = new Vector3(record.X, record.Y, record.Z);
                        actors[a].Tags.Clear();
                        actors[a].Tags.AddRange(record.Tags);
                        actors[a].PresetId = record.PresetId;
                        actors[a].InitialValues.Clear();
                        actors[a].InitialValues.AddRange(record.Values);
                        actors[a].RememberHome();
                        actors[a].RememberTags();
                    }

                    continue;
                }

                var dummy = SpellPreviewStage.SpawnDummy(record.StableId, new Vector3(record.X, record.Y, record.Z));
                if (dummy == null) continue;
                dummy.Tags.AddRange(record.Tags);
                dummy.PresetId = record.PresetId;
                dummy.InitialValues.AddRange(record.Values);
                dummy.RememberHome();
                dummy.RememberTags();
            }
        }

        void ToggleUnits()
        {
            var controller = Control(_func, "cUnits");
            var list = ListOf(_func, "units");
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
        }

        void HideLeftover(string name)
        {
            var child = view.GetChild(name);
            if (child != null)
                child.visible = false;
        }

        static string FormatId(ulong id)
        {
            var author = (uint)(id >> 32);
            var local = (uint)id;
            return author == 0 ? local.ToString() : author + ":" + local;
        }
    }
}
