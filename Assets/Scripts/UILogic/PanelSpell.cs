using System;
using System.Collections.Generic;
using System.Text;
using FairyGUI;
using Google.Protobuf;
using PackageAtrribute;
using PackageSpell;
using RebornFrame.GameData;
using RebornFrame.Spell;
using RebornFrame.Spell.Animation;
using RebornFrame.Spell.Particles;
using RebornFrame.Spell.Spine;
using RebornFrame.Spell.Vfx;
using GameContentEditor;
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
            public int Match;
            public bool LinkCast = true;
            public bool Cooldown;
            public readonly List<ulong> Tags = new List<ulong>();
            public readonly List<ulong> Buffs = new List<ulong>();
            public string Anim = "";
            public string Trigger = "";
            public int ClipSource;
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
        BlobAssetReference<SpellCatalogBlob> _previewCatalog;
        bool _ignoreProgress;
        bool _progressBound;
        bool _showingPreview;
        bool _entered;
        bool _editing = true;
        bool _logicDirty = true;
        ulong _shownOwner;
        readonly List<SpellEventConfig> _extraEvents = new List<SpellEventConfig>();
        readonly List<SpellPresentationTrack> _extraTracks = new List<SpellPresentationTrack>();
        readonly List<SpellPreviewSim.Frame> _samples = new List<SpellPreviewSim.Frame>();
        readonly List<SpellPreviewSim.Seed> _seeds = new List<SpellPreviewSim.Seed>();
        GComponent _func;
        GComponent _inspect;
        GComponent _skillInspect;
        GComponent _spineInspect;
        GComponent _unitInspect;
        PreviewActor _unitActor;
        readonly List<ulong> _previewAttrs = new List<ulong>();
        bool _presetUiNoted;
        PreviewActor _presetTitleActor;
        ulong _presetTitleId;
        string _presetTitle = "选择";
        GComponent _timeline;
        Controller _cInspect;
        Controller _cSpine;
        Controller _cClipName;
        Controller _cSpineEnd;
        const int InspectEmpty = 0;
        const int InspectSkill = 1;
        const int InspectSpine = 2;
        const int InspectUnit = 3;
        static readonly Color EditableColor = new Color(0.91f, 0.90f, 0.89f);
        static readonly Color LockedColor = new Color(0.55f, 0.53f, 0.49f);

        void BindRegions()
        {
            _func = view.GetChild("func") as GComponent;
            _inspect = view.GetChild("inspect") as GComponent;
            _skillInspect = _inspect == null ? null : _inspect.GetChild("skillInspect") as GComponent;
            _spineInspect = _inspect == null ? null : _inspect.GetChild("spineInspect") as GComponent;
            _unitInspect = _inspect == null ? null : _inspect.GetChild("unitInspect") as GComponent;
            _timeline = view.GetChild("timeline") as GComponent;
            _cInspect = Control(_inspect, "cInspect");
            _cSpine = Control(_func, "cSpine");
            _cClipName = Control(_spineInspect, "cClipName") ?? Control(_inspect, "cClipName");
            _cSpineEnd = Control(_spineInspect, "cSpineEnd") ?? Control(_inspect, "cSpineEnd");
        }

        GComponent SkillRoot()
        {
            return _skillInspect ?? _inspect;
        }

        GComponent SpineRoot()
        {
            return _spineInspect ?? _inspect;
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
            Btn(_func, "btnBack").onClick.Set(() =>
            {
                CloseDialogs();
                FGUIPanelManager.Instance.Back();
            });
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
            Btn(_func, "btnAddDummy").onClick.Set(() => SpellPreviewStage.AddDummy());
            var deleteKey = Btn(_func, "btnDeleteKey");
            if (deleteKey != null)
                deleteKey.onClick.Set(DeleteSelectedKey);
            DeleteGesture.Bind(this, OnDeletePressed);
            Btn(_func, "btnAddClip").onClick.Set(AddSpineClip);
            BindSkillFrame();
            BindUnitInspect();
            Radio(SpineRoot(), "endPause").onChanged.Add(() => SaveEnd(0, Radio(SpineRoot(), "endPause").selected));
            Radio(SpineRoot(), "endLoop").onChanged.Add(() => SaveEnd(1, Radio(SpineRoot(), "endLoop").selected));
            Radio(SpineRoot(), "pauseFirst").onChanged.Add(() => SavePause(0, Radio(SpineRoot(), "pauseFirst").selected));
            Radio(SpineRoot(), "pauseLast").onChanged.Add(() => SavePause(1, Radio(SpineRoot(), "pauseLast").selected));
            var pingpong = Part(SpineRoot(), "pingpong") as UI_ButtonCheck;
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
            Input(SpineRoot(), "inputSpeed").onChanged.Add(() =>
            {
                if (_applying || _key < 0) return;
                if (_tracks[_track].Kind != TrackKind.Spine) return;
                Current().Speed = Input(SpineRoot(), "inputSpeed").text;
                FitLength(Current());
            });

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

        public override void OnShow()
        {
            DeleteGesture.Bind(this, OnDeletePressed);
            if (!_entered || !SkillSelected()) return;
            ApplyInspector();
        }

        public override void OnClose()
        {
            DeleteGesture.Unbind(this);
            CloseDialogs();
            SpellPreviewStage.Changed -= OnStageChanged;
            SaveLayout();
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
            ReleasePreview();
            SpellPreviewStage.Close();
        }

        void OpenPreview()
        {
            var hit = view.GetChild("previewHit");
            if (hit != null)
            {
                hit.alpha = 0f;
                hit.visible = true;
                hit.touchable = true;
            }

            PaintPreviewBorder();
            (view.GetChild("btnResetCamera") as GButton).onClick.Set(() => SpellPreviewStage.ResetCamera());
            BindPreviewMode();
            SpellPreviewStage.SetPlacement(true);
            SpellPreviewStage.Open(hit, OnPreviewUnit);
            SpellPreviewStage.SetCaster(CurrentOwner());
        }

        void BindPreviewMode()
        {
            var mode = (view.GetChild("modeToggle") as GComponent).GetController("cMode");
            _editing = true;
            mode.selectedIndex = 0;
            mode.onChanged.Add(() => ApplyMode(mode.selectedIndex == 0));
        }

        void ApplyMode(bool editing)
        {
            if (_editing == editing)
                return;
            _editing = editing;
            SpellPreviewStage.SetPlacement(_editing);
            PaintPreviewBorder();
            if (_editing)
                SpellPreviewStage.RestoreDefaults();
            else
                SimulateIfNeeded();
            var actor = SpellPreviewStage.Selected;
            if (actor != null && _key < 0)
                ShowUnitInspect(actor);
        }

        void PaintPreviewBorder()
        {
            var border = view.GetChild("previewBorder") as GGraph;
            if (border == null) return;
            var line = _editing
                ? (Color)new Color32(0x3d, 0xb8, 0x6a, 0xff)
                : (Color)new Color32(0xf0, 0x8a, 0x2a, 0xff);
            border.touchable = false;
            border.alpha = 1f;
            border.DrawRect(border.width, border.height, 3, line, new Color(0f, 0f, 0f, 0f));
        }

        void OnPreviewUnit(PreviewActor actor)
        {
            if (view == null || view.isDisposed)
                return;
            if (actor == null)
            {
                _unitActor = null;
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
            _unitActor = actor;
            _applying = true;
            Label(_inspect, "inspectKind").text = actor.Unit == null
                ? actor.name
                : new SpellId(actor.Unit.Id).ToString();
            if (UnitInspectReady())
            {
                _cInspect.selectedIndex = InspectUnit;
                var mode = _unitInspect.GetController("cMode");
                if (mode != null)
                    mode.selectedIndex = _editing ? 0 : 1;
                var heading = Label(_unitInspect, "labelTags");
                if (heading != null)
                    heading.text = _editing ? "起始标签" : "标签";
                var add = Btn(_unitInspect, "btnAddTag");
                if (add != null)
                    add.visible = _editing;
                RenderUnitTags();
                RenderUnitValues();
                var report = Label(_unitInspect, "report");
                if (report != null)
                {
                    report.text = UnitBody(actor);
                    report.visible = ListOf(_unitInspect, "values") == null;
                }
            }
            else
            {
                _cInspect.selectedIndex = InspectEmpty;
                var empty = Label(_inspect, "inspectEmpty");
                empty.singleLine = false;
                FitEmpty(empty, 496);
                empty.text = UnitReport(actor, _editing ? actor.DefaultTags : actor.Tags);
            }

            _applying = false;
        }

        bool UnitInspectReady()
        {
            return _unitInspect != null && _cInspect != null && _cInspect.pageCount > InspectUnit;
        }

        void BindUnitInspect()
        {
            if (_unitInspect == null)
                return;
            var add = Btn(_unitInspect, "btnAddTag");
            if (add != null)
                add.onClick.Set(AddUnitTag);
            var preset = Btn(_unitInspect, "btnPreset");
            if (preset != null)
                preset.onClick.Set(PickDummyPreset);
        }

        void RenderUnitTags()
        {
            var actor = _unitActor;
            var list = ListOf(_unitInspect, "tags");
            if (actor == null || list == null)
                return;
            var tags = _editing ? actor.DefaultTags : actor.Tags;
            var empty = Label(_unitInspect, "empty");
            if (empty != null)
                empty.visible = tags.Count == 0;
            list.itemRenderer = (index, obj) =>
            {
                var row = obj.asCom;
                var id = tags[index];
                row.SetSize(list.width, _editing ? 76f : 44f);
                var title = row.GetChild("title") as GTextField;
                if (title != null)
                {
                    title.singleLine = false;
                    title.SetSize(list.width, 40f);
                    title.text = TagPath(id);
                }

                var mode = row.GetController("cMode");
                if (mode != null)
                    mode.selectedIndex = _editing ? 0 : 1;
                var check = row.GetChild("castLink") as GButton;
                if (check != null)
                {
                    check.visible = _editing;
                    check.onChanged.Set(() => SetUnitCast(actor, id, check.selected));
                    check.selected = actor.CastTags.Contains(id);
                }

                var edit = row.GetChild("btnEdit") as GButton;
                if (edit != null)
                {
                    edit.visible = _editing;
                    edit.onClick.Set(() => EditTag(id));
                }

                var remove = row.GetChild("btnRemove") as GButton;
                if (remove != null)
                {
                    remove.visible = _editing;
                    remove.onClick.Set(() => RemoveUnitTag(actor, id));
                }
            };
            list.numItems = tags.Count;
        }

        void RenderUnitValues()
        {
            var actor = _unitActor;
            var list = ListOf(_unitInspect, "values");
            var button = Btn(_unitInspect, "btnPreset");
            var heading = Label(_unitInspect, "labelPreset");
            var empty = Label(_unitInspect, "valueEmpty");
            if (actor == null)
                return;
            if (list == null && _editing && !_presetUiNoted)
            {
                _presetUiNoted = true;
                Debug.LogWarning("PackageSpell 里的单位检视还没有属性预设，先在 FairyGUI 发布技能包。");
            }

            var dummy = actor.Unit == null;
            if (heading != null)
                heading.text = _editing ? (dummy ? "属性预设" : "初始属性") : "本帧属性";
            if (button != null)
            {
                button.visible = dummy;
                button.touchable = _editing;
                button.grayed = !_editing;
                button.title = dummy ? PresetTitle(actor) : "选择";
            }

            CollectPreviewAttributes(actor, _previewAttrs);
            if (empty != null)
            {
                empty.visible = _previewAttrs.Count == 0;
                if (!_editing)
                    empty.text = "这一帧没有属性";
                else
                    empty.text = dummy ? "选择单位预设后编辑默认值" : "这个单位没有属性";
            }

            if (list == null)
                return;
            var attrs = _previewAttrs;
            list.itemRenderer = (index, obj) =>
            {
                var row = obj.asCom;
                var id = attrs[index];
                var resource = UnitCatalogs.IsResource(id);
                row.SetSize(list.width, 64f);
                var title = row.GetChild("title") as GTextField;
                if (title != null)
                    title.text = UnitCatalogs.AttributeLabel(id);
                var input = row.GetChild("input") as GTextInput;
                var currentBg = row.GetChild("bg");
                if (!resource)
                {
                    if (currentBg != null)
                        currentBg.SetSize(248f, 24f);
                    if (input != null)
                        input.SetSize(236f, 24f);
                }
                else
                {
                    if (currentBg != null)
                        currentBg.SetSize(96f, 24f);
                    if (input != null)
                        input.SetSize(84f, 24f);
                }

                if (input != null)
                {
                    input.singleLine = true;
                    input.touchable = _editing;
                    input.color = _editing ? EditableColor : LockedColor;
                    input.text = (_editing || !actor.HasSample ? DisplayedRaw(actor, id) : FrameRaw(actor, id)).ToString();
                    input.onChanged.Set(() =>
                    {
                        if (_applying || !_editing)
                            return;
                        long.TryParse(input.text, out var raw);
                        WritePreviewValue(actor, id, raw);
                    });
                }

                var showMax = resource;
                var maxEditable = _editing;
                var maxLabel = row.GetChild("maxLabel");
                var maxBg = row.GetChild("maxBg");
                if (maxLabel != null)
                    maxLabel.visible = showMax;
                if (maxBg != null)
                    maxBg.visible = showMax;
                var maxInput = row.GetChild("maxInput") as GTextInput;
                if (maxInput != null)
                {
                    maxInput.visible = showMax;
                    maxInput.singleLine = true;
                    maxInput.touchable = maxEditable;
                    maxInput.color = maxEditable ? EditableColor : LockedColor;
                    if (showMax)
                    {
                        maxInput.text = (_editing || !actor.HasSample ? DisplayedMax(actor, id) : FrameMax(actor, id)).ToString();
                        maxInput.onChanged.Set(() =>
                        {
                            if (_applying || !maxEditable)
                                return;
                            long.TryParse(maxInput.text, out var max);
                            WritePreviewMax(actor, id, max);
                        });
                    }
                }

                var track = row.GetChild("track") as GButton;
                if (track != null)
                {
                    track.touchable = _editing;
                    track.selected = actor.Tracks(id);
                    track.onChanged.Set(() =>
                    {
                        if (_applying || !_editing)
                            return;
                        SetTracked(actor, id, track.selected);
                    });
                }
            };
            list.numItems = attrs.Count;
        }

        string PresetTitle(PreviewActor actor)
        {
            if (_editing || _presetTitleActor != actor || _presetTitleId != actor.PresetId)
            {
                _presetTitleActor = actor;
                _presetTitleId = actor.PresetId;
                _presetTitle = UnitCatalogs.PresetTitle(UnitCatalogs.FindPreset(actor.PresetId));
            }

            return _presetTitle;
        }

        void CollectPreviewAttributes(PreviewActor actor, List<ulong> into)
        {
            into.Clear();
            if (actor.Unit == null)
            {
                if (_editing)
                {
                    var preset = UnitCatalogs.FindPreset(actor.PresetId);
                    if (preset != null)
                    {
                        for (var i = 0; i < preset.Attributes.Count; i++)
                            AddPreviewAttribute(into, preset.Attributes[i]);
                        return;
                    }
                }

                for (var i = 0; i < actor.InitialValues.Count; i++)
                    AddPreviewAttribute(into, actor.InitialValues[i].AttributeId);
                return;
            }

            for (var i = 0; i < actor.Unit.PresetAttributes.Count; i++)
                AddPreviewAttribute(into, actor.Unit.PresetAttributes[i]);
            for (var i = 0; i < actor.Unit.Values.Count; i++)
                AddPreviewAttribute(into, actor.Unit.Values[i].AttributeId);
        }

        static bool KeptMax(PreviewActor actor, ulong id, out long max)
        {
            max = 0;
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                var value = actor.InitialValues[i];
                if (value.AttributeId != id || !value.HasMax)
                    continue;
                max = value.Max;
                return true;
            }

            return false;
        }

        static void Prune(List<ulong> ids, List<EditorUnitValue> values)
        {
            for (var i = ids.Count - 1; i >= 0; i--)
            {
                if (!Listed(values, ids[i]))
                    ids.RemoveAt(i);
            }
        }

        static bool Listed(List<EditorUnitValue> values, ulong id)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (values[i].AttributeId == id)
                    return true;
            }

            return false;
        }

        static void AddPreviewAttribute(List<ulong> into, ulong id)
        {
            if (id == 0)
                return;
            for (var i = 0; i < into.Count; i++)
            {
                if (into[i] == id)
                    return;
            }

            into.Add(id);
        }

        static long FrameRaw(PreviewActor actor, ulong id)
        {
            for (var i = 0; i < actor.Sampled.Count; i++)
            {
                if (actor.Sampled[i].AttributeId == id)
                    return actor.Sampled[i].Raw;
            }

            return 0;
        }

        static long FrameMax(PreviewActor actor, ulong id)
        {
            for (var i = 0; i < actor.Sampled.Count; i++)
            {
                if (actor.Sampled[i].AttributeId == id)
                    return actor.Sampled[i].Max;
            }

            return 0;
        }

        void SetTracked(PreviewActor actor, ulong id, bool tracked)
        {
            if (UnitCatalogs.IsResource(id))
            {
                if (tracked)
                    actor.HiddenAttributes.Remove(id);
                else if (!actor.HiddenAttributes.Contains(id))
                    actor.HiddenAttributes.Add(id);
            }
            else if (tracked)
            {
                if (!actor.TrackedAttributes.Contains(id))
                    actor.TrackedAttributes.Add(id);
            }
            else
            {
                actor.TrackedAttributes.Remove(id);
            }

            OnStageChanged();
        }

        static long DisplayedRaw(PreviewActor actor, ulong id)
        {
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                if (actor.InitialValues[i].AttributeId == id)
                    return actor.InitialValues[i].Raw;
            }

            if (actor.Unit != null)
            {
                for (var i = 0; i < actor.Unit.Values.Count; i++)
                {
                    if (actor.Unit.Values[i].AttributeId == id)
                        return actor.Unit.Values[i].Raw;
                }
            }

            if (UnitCatalogs.LayoutRaw(id, out var layout))
                return layout;
            return 0;
        }

        static long DisplayedMax(PreviewActor actor, ulong id)
        {
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                var value = actor.InitialValues[i];
                if (value.AttributeId == id && value.HasMax)
                    return value.Max;
            }

            if (UnitCatalogs.LayoutRaw(id, out var layout))
                return layout;
            return 0;
        }

        void WritePreviewValue(PreviewActor actor, ulong id, long raw)
        {
            EnsurePreviewValues(actor);
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                if (actor.InitialValues[i].AttributeId != id)
                    continue;
                if (actor.InitialValues[i].Raw == raw)
                    return;
                actor.InitialValues[i].Raw = raw;
                OnStageChanged();
                return;
            }

            actor.InitialValues.Add(PreviewValue(actor, id, raw));
            OnStageChanged();
        }

        void WritePreviewMax(PreviewActor actor, ulong id, long max)
        {
            EnsurePreviewValues(actor);
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                var value = actor.InitialValues[i];
                if (value.AttributeId != id)
                    continue;
                if (value.HasMax && value.Max == max)
                    return;
                value.HasMax = true;
                value.Max = max;
                OnStageChanged();
                return;
            }
        }

        static EditorUnitValue PreviewValue(PreviewActor actor, ulong id, long raw)
        {
            var value = new EditorUnitValue { AttributeId = id, Raw = raw };
            if (UnitCatalogs.IsResource(id))
            {
                value.HasMax = true;
                value.Max = DisplayedMax(actor, id);
            }

            return value;
        }

        void EnsurePreviewValues(PreviewActor actor)
        {
            CollectPreviewAttributes(actor, _previewAttrs);
            for (var i = 0; i < _previewAttrs.Count; i++)
            {
                var id = _previewAttrs[i];
                var found = false;
                for (var v = 0; v < actor.InitialValues.Count; v++)
                {
                    if (actor.InitialValues[v].AttributeId != id)
                        continue;
                    found = true;
                    break;
                }

                if (!found)
                    actor.InitialValues.Add(PreviewValue(actor, id, DisplayedRaw(actor, id)));
            }
        }

        void PickDummyPreset()
        {
            var actor = _unitActor;
            if (actor == null || actor.Unit != null || !_editing)
                return;
            var presets = new List<UnitPreset>();
            UnitCatalogs.CollectPresets(presets);
            var keys = new List<string>(presets.Count);
            var titles = new List<string>(presets.Count);
            for (var i = 0; i < presets.Count; i++)
            {
                keys.Add(presets[i].Id.ToString());
                titles.Add(UnitCatalogs.PresetTitle(presets[i]));
            }

            PanelUnitPick.Show("选择单位预设", keys, titles, key =>
            {
                if (actor == null || !ulong.TryParse(key, out var id))
                    return;
                ApplyDummyPreset(actor, id);
            });
        }

        void ApplyDummyPreset(PreviewActor actor, ulong presetId)
        {
            if (actor == null || actor.Unit != null)
                return;
            var preset = UnitCatalogs.FindPreset(presetId);
            if (preset == null)
                return;
            var next = new List<EditorUnitValue>();
            for (var i = 0; i < preset.Attributes.Count; i++)
            {
                var id = preset.Attributes[i];
                if (id == 0 || Listed(next, id))
                    continue;
                var found = false;
                long raw = 0;
                for (var v = 0; v < actor.InitialValues.Count; v++)
                {
                    if (actor.InitialValues[v].AttributeId != id)
                        continue;
                    raw = actor.InitialValues[v].Raw;
                    found = true;
                    break;
                }

                if (!found && UnitCatalogs.LayoutRaw(id, out var layout))
                    raw = layout;
                var value = new EditorUnitValue { AttributeId = id, Raw = raw };
                if (UnitCatalogs.IsResource(id))
                {
                    value.HasMax = true;
                    value.Max = found && KeptMax(actor, id, out var kept) ? kept : raw;
                }

                next.Add(value);
            }

            actor.PresetId = presetId;
            actor.InitialValues.Clear();
            actor.InitialValues.AddRange(next);
            Prune(actor.HiddenAttributes, next);
            Prune(actor.TrackedAttributes, next);
            ShowUnitInspect(actor);
            OnStageChanged();
        }

        void AddUnitTag()
        {
            var actor = _unitActor;
            if (actor == null || !_editing)
                return;
            PanelLayer.Dismiss<PanelBlank>();
            PanelTagPick.Show(id =>
            {
                if (id == 0 || actor == null || actor.DefaultTags.Contains(id))
                    return;
                actor.DefaultTags.Add(id);
                SyncStartTags(actor);
                ShowUnitInspect(actor);
                OnStageChanged();
            });
        }

        void RemoveUnitTag(PreviewActor actor, ulong id)
        {
            if (actor == null || !_editing)
                return;
            actor.DefaultTags.Remove(id);
            actor.CastTags.Remove(id);
            SyncStartTags(actor);
            ShowUnitInspect(actor);
            OnStageChanged();
        }

        void SetUnitCast(PreviewActor actor, ulong id, bool linked)
        {
            if (_applying || actor == null || !_editing)
                return;
            if (linked)
            {
                if (!actor.CastTags.Contains(id))
                    actor.CastTags.Add(id);
            }
            else
            {
                actor.CastTags.Remove(id);
            }

            OnStageChanged();
        }

        static void SyncStartTags(PreviewActor actor)
        {
            actor.Tags.Clear();
            actor.Tags.AddRange(actor.DefaultTags);
        }

        static string TagPath(ulong id)
        {
            var path = ComTagEditor.Path(id);
            return string.IsNullOrEmpty(path) ? id.ToString() : path;
        }

        void RestoreInspectEmpty()
        {
            var empty = Label(_inspect, "inspectEmpty");
            FitEmpty(empty, 40);
            empty.text = "选中技能或 Spine 的关键帧后在这里配置。";
        }

        void FitEmpty(GTextField empty, float textHeight)
        {
            empty.SetSize(288, textHeight);
            var card = Part(_inspect, "emptyCard");
            if (card != null)
                card.SetSize(304, textHeight + 24);
        }

        static string UnitBody(PreviewActor actor)
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

        static string UnitReport(PreviewActor actor, List<ulong> tags)
        {
            var text = new StringBuilder();
            text.Append(UnitBody(actor));
            text.Append("\n标签\n");
            if (tags.Count == 0)
                text.Append("没有标签\n");
            for (var i = 0; i < tags.Count; i++)
            {
                text.Append(TagPath(tags[i]));
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

        void OnDeletePressed()
        {
            if (_cInspect != null && _cInspect.selectedIndex == InspectUnit)
            {
                var actor = _unitActor != null ? _unitActor : SpellPreviewStage.Selected;
                if (actor != null && actor.Unit == null && !actor.Locked)
                {
                    SpellPreviewStage.RemoveDummy(actor);
                    return;
                }
            }

            DeleteSelectedKey();
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
            EnsureStartKey();
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

        void DeleteSelectedKey()
        {
            if (_track < 0 || _key < 0 || _track >= _tracks.Count) return;
            var track = _tracks[_track];
            if (_key >= track.Keys.Count) return;
            var skill = track.Kind == TrackKind.Skill;
            var kind = track.Kind;
            track.Keys.RemoveAt(_key);
            _track = -1;
            _key = -1;
            if (skill)
            {
                PackSkillTracks();
                ClearSelection();
                RenderTracks();
                RecordLogic();
                return;
            }

            ClearSelection();
            RenderTracks();
            RecordRender(kind);
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

        bool EnsureStartKey()
        {
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill) continue;
                var keys = _tracks[i].Keys;
                for (var k = 0; k < keys.Count; k++)
                {
                    if (keys[k].Frame <= 0) return false;
                }
            }

            SkillTrack().Keys.Insert(0, new KeyFrame { Frame = 0 });
            return true;
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
            var can = _track >= 0
                      && _track < _tracks.Count
                      && _key >= 0
                      && _key < _tracks[_track].Keys.Count;
            button.grayed = !can;
            button.touchable = can;
        }

        void BindKey(GButton button, KeyFrame key, float px, bool skill)
        {
            var originX = 0f;
            var originW = 0f;
            var pointerX = 0f;
            var moved = false;
            var home = key.Frame;
            var edge = 0;
            var locked = skill && key.Frame <= 0 && !key.Cooldown;
            var row = new List<PlacedClip>();
            if (skill && (key.Cooldown || key.Frame <= 0))
            {
                var page = button.GetController("button");
                if (page != null)
                    page.onChanged.Add(() => PaintSkillKey(button, key));
                PaintSkillKey(button, key);
            }
            button.onTouchBegin.Add(context =>
            {
                if (locked) return;
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
                if (locked) return;
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
                    key.Frame = Mathf.Max(key.Cooldown ? 1 : 0, Mathf.RoundToInt(button.x / px));
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

        static string SkillKindLabel(KeyFrame key)
        {
            if (key.Cooldown) return "冷却帧";
            if (key.Frame <= 0) return "开始帧";
            return "事件帧";
        }

        static void PaintSkillKey(GButton button, KeyFrame key)
        {
            var bg = (button as UI_ItemKey)?.m_bg;
            if (bg == null || (!key.Cooldown && key.Frame > 0)) return;
            var page = button.GetController("button");
            var index = page == null ? 0 : page.selectedIndex;
            bg.color = key.Cooldown ? CooldownColor(index) : StartColor(index);
        }

        static Color32 StartColor(int page)
        {
            switch (page)
            {
                case 2: return new Color32(206, 132, 64, 255);
                case 1: return new Color32(224, 160, 90, 255);
                case 3: return new Color32(236, 178, 112, 255);
                default: return new Color32(184, 106, 47, 255);
            }
        }

        static Color32 CooldownColor(int page)
        {
            switch (page)
            {
                case 2: return new Color32(114, 80, 148, 255);
                case 1: return new Color32(140, 102, 184, 255);
                case 3: return new Color32(158, 122, 204, 255);
                default: return new Color32(92, 61, 120, 255);
            }
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
                Label(_inspect, "inspectKind").text = SkillKindLabel(key);
                var cast = Part(SkillRoot(), "castLink") as UI_ButtonCheck;
                if (cast != null)
                    cast.selected = key.LinkCast;
                var match = Part(SkillRoot(), "comboMatch") as GComboBox;
                if (match != null)
                    match.value = key.Match >= 1 && key.Match <= 4 ? key.Match.ToString() : "3";
                RenderTagRows();
                RenderLineRows(ListOf(SkillRoot(), "conditions"), null);
                RenderLineRows(ListOf(SkillRoot(), "buffs"), key.Buffs);
            }
            else if (track.Kind == TrackKind.Spine)
            {
                _cInspect.selectedIndex = InspectSpine;
                Label(_inspect, "inspectKind").text = "Spine 关键帧";
                _cClipName.selectedIndex = key.ClipSource == 1 ? 1 : 0;
                Radio(SpineRoot(), "clipByName").selected = key.ClipSource != 1;
                Radio(SpineRoot(), "clipByTrigger").selected = key.ClipSource == 1;
                FillClipCombos(key);
                Input(SpineRoot(), "inputSpeed").text = key.Speed;
                _cSpineEnd.selectedIndex = key.EndMode;
                Radio(SpineRoot(), "endPause").selected = key.EndMode == 0;
                Radio(SpineRoot(), "endLoop").selected = key.EndMode == 1;
                Radio(SpineRoot(), "pauseFirst").selected = key.PauseAt == 0;
                Radio(SpineRoot(), "pauseLast").selected = key.PauseAt == 1;
                (Part(SpineRoot(), "pingpong") as UI_ButtonCheck).selected = key.Pingpong;
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
            _comboAnim = Part(SpineRoot(), "comboAnim") as GComboBox;
            _comboTrigger = Part(SpineRoot(), "comboTrigger") as GComboBox;
            SyncClipBoxes();
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
            var child = Part(SpineRoot(), name);
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

        int ContentFrames()
        {
            var frames = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var keys = _tracks[i].Keys;
                var skill = _tracks[i].Kind == TrackKind.Skill;
                for (var k = 0; k < keys.Count; k++)
                {
                    if (skill && keys[k].Cooldown)
                        continue;
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

        void BindSkillFrame()
        {
            var match = Part(SkillRoot(), "comboMatch") as GComboBox;
            if (match != null)
            {
                match.items = new[] { "精确", "向上", "向下", "任意" };
                match.values = new[] { "1", "2", "3", "4" };
                match.onChanged.Add(() =>
                {
                    if (_applying || !SkillSelected()) return;
                    if (!int.TryParse(match.value, out var mode)) return;
                    Current().Match = mode;
                    RecordLogic();
                });
            }

            var cast = Part(SkillRoot(), "castLink") as UI_ButtonCheck;
            if (cast != null)
            {
                cast.onChanged.Add(() =>
                {
                    if (_applying || !SkillSelected()) return;
                    Current().LinkCast = cast.selected;
                    RecordLogic();
                });
            }

            var castTip = Btn(SkillRoot(), "btnCastTip");
            if (castTip != null)
                castTip.onClick.Set(() => PanelBlank.Show(
                    "与本次施法相关",
                    "每次施法会分配一个唯一施法识别码。目标标签中可能存储有施法识别码，勾选后只匹配识别码相同的标签。"));
            var addTag = Btn(SkillRoot(), "btnAddTag");
            if (addTag != null)
                addTag.onClick.Set(() => OpenTagPick(-1));
            var addCondition = Btn(SkillRoot(), "btnAddCondition");
            if (addCondition != null)
                addCondition.onClick.Set(() => PanelBlank.Show("添加条件", "条件功能还没定。"));
            var addBuff = Btn(SkillRoot(), "btnAddBuff");
            if (addBuff != null)
                addBuff.onClick.Set(() => PanelBlank.Show("添加 Buff", "Buff 选择还没定。"));
        }

        bool SkillSelected()
        {
            return _track >= 0
                   && _key >= 0
                   && _track < _tracks.Count
                   && _key < _tracks[_track].Keys.Count
                   && _tracks[_track].Kind == TrackKind.Skill;
        }

        void RenderTagRows()
        {
            var list = ListOf(SkillRoot(), "tags");
            if (list == null || !SkillSelected()) return;
            var tags = Current().Tags;
            list.itemRenderer = (index, obj) =>
            {
                var row = obj.asCom;
                var id = tags[index];
                var captured = index;
                row.SetSize(list.width, 72f);
                var title = row.GetChild("title") as GTextField;
                if (title != null)
                {
                    title.singleLine = false;
                    title.SetXY(0f, 0f);
                    title.SetSize(list.width, 40f);
                    title.text = TagPath(id);
                }

                var search = row.GetChild("btnSearch");
                if (search != null)
                    search.visible = false;
                var edit = row.GetChild("btnEdit") as GButton;
                if (edit != null)
                {
                    edit.SetXY(list.width - 112f, 44f);
                    edit.onClick.Set(() => EditTag(id));
                }

                var remove = row.GetChild("btnRemove") as GButton;
                if (remove != null)
                {
                    remove.SetXY(list.width - 56f, 44f);
                    remove.onClick.Set(() => RemoveTag(captured));
                }
            };
            list.numItems = tags.Count;
        }

        void RenderLineRows(GList list, List<ulong> ids)
        {
            if (list == null) return;
            if (ids == null)
            {
                list.numItems = 0;
                return;
            }

            list.itemRenderer = (index, obj) =>
            {
                var row = obj.asCom;
                var captured = index;
                var title = row.GetChild("title") as GTextField;
                if (title != null)
                    title.text = FormatId(ids[index]);
                row.SetSize(list.width, row.height);
                var remove = row.GetChild("btnRemove") as GButton;
                if (remove != null)
                    remove.onClick.Set(() => RemoveBuff(captured));
            };
            list.numItems = ids.Count;
        }

        void OpenTagPick(int index)
        {
            if (!SkillSelected()) return;
            PanelLayer.Dismiss<PanelBlank>();
            PanelTagPick.Show(id => UseTag(index, id));
        }

        void UseTag(int index, ulong id)
        {
            if (!SkillSelected() || id == 0) return;
            var tags = Current().Tags;
            if (index >= 0 && index < tags.Count)
            {
                if (tags[index] == id) return;
                for (var i = 0; i < tags.Count; i++)
                {
                    if (i != index && tags[i] == id) return;
                }

                tags[index] = id;
            }
            else if (!tags.Contains(id))
            {
                tags.Add(id);
            }
            else
            {
                return;
            }

            ApplyInspector();
            RecordLogic();
        }

        void RemoveTag(int index)
        {
            if (!SkillSelected()) return;
            var tags = Current().Tags;
            if (index < 0 || index >= tags.Count) return;
            tags.RemoveAt(index);
            ApplyInspector();
            RecordLogic();
        }

        void RemoveBuff(int index)
        {
            if (!SkillSelected()) return;
            var buffs = Current().Buffs;
            if (index < 0 || index >= buffs.Count) return;
            buffs.RemoveAt(index);
            ApplyInspector();
            RecordLogic();
        }

        void EditTag(ulong id)
        {
            if (id == 0) return;
            CloseDialogs();
            RecordLogic();
            FGUIPanelManager.Instance.ShowPanel<PanelTag>(
                UI_PanelTag.PackageName,
                UI_PanelTag.ResName,
                new object[] { id },
                ShowTypeEnum.HideCurrent);
        }

        void CloseDialogs()
        {
            PanelLayer.Dismiss<PanelTagPick>();
            PanelLayer.Dismiss<PanelBlank>();
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
            _logicDirty = true;
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
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
            if (actor != null && _cInspect != null
                && _cInspect.selectedIndex != InspectSkill
                && _cInspect.selectedIndex != InspectSpine)
                ShowUnitInspect(actor);
        }

        void RecordLogic()
        {
            if (!_entered || _spellId == 0) return;
            if (EnsureStartKey())
                RenderTracks();
            WriteLogic();
            _logicDirty = true;
            if (ProjectSession.IsOpen)
                ProjectSession.Save();
            if (!_editing)
                RunPreviewSim();
        }

        void SimulateIfNeeded()
        {
            if (_logicDirty || _samples.Count == 0)
                RunPreviewSim();
            else
                ApplyWorld();
        }

        void RunPreviewSim()
        {
            if (_spellId == 0) return;
            SpellPreviewStage.CopySeeds(_seeds);
            var frames = ContentFrames();
            var next = SpellPreviewSim.Run(_spellId, Mathf.Max(1, frames + 1), _seeds);
            _samples.Clear();
            _samples.AddRange(next);
            if (next.Count > 0)
                _logicDirty = false;
            ApplyWorld();
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
            var cooldownFrame = 0;
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Kind != TrackKind.Skill) continue;
                var keys = _tracks[i].Keys;
                for (var k = 0; k < keys.Count; k++)
                {
                    if (keys[k].Cooldown)
                    {
                        cooldownFrame = keys[k].Frame;
                        continue;
                    }

                    spell.Events.Add(ToEvent(keys[k]));
                }
            }

            spell.CooldownFrames = cooldownFrame;

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

            evt.Match = key.Match;
            evt.IgnoreCastMask = key.LinkCast ? 0 : 1;
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

                var key = new KeyFrame
                {
                    Frame = trigger == SpellEventTrigger.OnStart ? 0 : evt.Frame,
                    Match = evt.Match,
                    LinkCast = evt.IgnoreCastMask == 0,
                };
                key.Tags.AddRange(evt.Tags);
                key.Buffs.AddRange(evt.BuffIds);
                skill.Keys.Add(key);
            }

            if (spell.CooldownFrames > 0)
            {
                skill.Keys.Add(new KeyFrame
                {
                    Frame = spell.CooldownFrames,
                    Cooldown = true,
                });
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
            var seen = new HashSet<ulong>();
            for (var i = 0; i < _seeds.Count; i++)
            {
                var seed = _seeds[i];
                if (seed.StableId != 0 && !seen.Add(seed.StableId))
                    continue;
                var record = new SpellPreviewActorRecord
                {
                    StableId = seed.StableId,
                    X = seed.Position.x,
                    Y = seed.Position.y,
                    Z = seed.Position.z,
                };
                record.PresetId = seed.PresetId;
                var owner = CurrentOwner();
                if (owner != null && owner.Id == seed.StableId)
                    record.UnitId = owner.Id;
                record.Tags.AddRange(seed.Tags);
                record.CastTags.AddRange(seed.CastTags);
                record.Values.AddRange(seed.PreviewValues);
                record.HiddenAttributes.AddRange(seed.HiddenAttributes);
                record.TrackedAttributes.AddRange(seed.TrackedAttributes);
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

            SpellPreviewStage.ClearDummies();
            if (layout == null) return;
            var owner = CurrentOwner();
            var seen = new HashSet<ulong>();
            for (var i = 0; i < layout.Actors.Count; i++)
            {
                var record = layout.Actors[i];
                if (record.StableId != 0 && !seen.Add(record.StableId))
                    continue;
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
                        actors[a].CastTags.Clear();
                        actors[a].CastTags.AddRange(record.CastTags);
                        actors[a].PresetId = 0;
                        actors[a].InitialValues.Clear();
                        actors[a].InitialValues.AddRange(record.Values);
                        actors[a].HiddenAttributes.Clear();
                        actors[a].HiddenAttributes.AddRange(record.HiddenAttributes);
                        actors[a].TrackedAttributes.Clear();
                        actors[a].TrackedAttributes.AddRange(record.TrackedAttributes);
                        actors[a].RememberHome();
                        actors[a].RememberTags();
                    }

                    continue;
                }

                var dummy = SpellPreviewStage.SpawnDummy(record.StableId, new Vector3(record.X, record.Y, record.Z));
                if (dummy == null) continue;
                dummy.Tags.AddRange(record.Tags);
                dummy.CastTags.AddRange(record.CastTags);
                dummy.PresetId = record.PresetId;
                dummy.InitialValues.AddRange(record.Values);
                dummy.HiddenAttributes.Clear();
                dummy.HiddenAttributes.AddRange(record.HiddenAttributes);
                dummy.TrackedAttributes.Clear();
                dummy.TrackedAttributes.AddRange(record.TrackedAttributes);
                dummy.RememberHome();
                dummy.RememberTags();
            }
        }

        void ToggleUnits()
        {
            if (_owners.Count == 0) return;
            var keys = new List<string>();
            var titles = new List<string>();
            for (var i = 0; i < _owners.Count; i++)
            {
                keys.Add(i.ToString());
                titles.Add(new SpellId(_owners[i].Id).ToString());
            }

            PanelUnitPick.Show("预览单位", keys, titles, key =>
            {
                if (!int.TryParse(key, out var index) || index < 0 || index >= _owners.Count) return;
                _owner = index;
                ShowOwner();
            });
        }

        void RefreshPreview()
        {
            ShowOwner();
        }

        static string FormatId(ulong id)
        {
            var author = (uint)(id >> 32);
            var local = (uint)id;
            return author == 0 ? local.ToString() : author + ":" + local;
        }
    }
}
