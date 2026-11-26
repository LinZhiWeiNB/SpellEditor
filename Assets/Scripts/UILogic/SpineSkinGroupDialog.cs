using System;
using System.Collections.Generic;
using System.Text;
using FairyGUI;
using PackageResource;

namespace UILogic
{
    /// <summary>预览里编辑的群组、触发器和外观槽位。播放时再按当前群组取片段和贴图。</summary>
    sealed class SpinePreviewSetup
    {
        public readonly List<SpineNamed> Groups = new List<SpineNamed>();
        public readonly List<SpineNamed> Triggers = new List<SpineNamed>();
        public readonly List<SpineSlotSetup> Slots = new List<SpineSlotSetup>();
        readonly Dictionary<string, Dictionary<string, string>> _clips = new Dictionary<string, Dictionary<string, string>>();
        readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _textures =
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        int _seq;

        public string GroupId = "";
        public string TriggerId = "";
        public string Skeleton = "";

        public void Clear()
        {
            Groups.Clear();
            Triggers.Clear();
            Slots.Clear();
            _clips.Clear();
            _textures.Clear();
            GroupId = "";
            TriggerId = "";
            _seq = 0;
        }

        public SpinePreviewSetup Clone()
        {
            var copy = new SpinePreviewSetup();
            copy._seq = _seq;
            for (var i = 0; i < Groups.Count; i++) copy.Groups.Add(Groups[i].Copy());
            for (var i = 0; i < Triggers.Count; i++) copy.Triggers.Add(Triggers[i].Copy());
            for (var i = 0; i < Slots.Count; i++) copy.Slots.Add(Slots[i].Copy());
            foreach (var trigger in _clips)
            {
                var row = new Dictionary<string, string>();
                foreach (var cell in trigger.Value) row[cell.Key] = cell.Value;
                copy._clips[trigger.Key] = row;
            }

            foreach (var slot in _textures)
            {
                var skins = new Dictionary<string, Dictionary<string, string>>();
                foreach (var skin in slot.Value)
                {
                    var row = new Dictionary<string, string>();
                    foreach (var cell in skin.Value) row[cell.Key] = cell.Value;
                    skins[skin.Key] = row;
                }

                copy._textures[slot.Key] = skins;
            }

            return copy;
        }

        public void ApplyDraft(SpinePreviewSetup draft)
        {
            var chosen = new Dictionary<string, string>();
            for (var i = 0; i < Slots.Count; i++) chosen[Slots[i].Id] = Slots[i].Chosen ?? "";
            var groupId = GroupId;
            var triggerId = TriggerId;
            Groups.Clear();
            Triggers.Clear();
            Slots.Clear();
            _clips.Clear();
            _textures.Clear();
            _seq = draft._seq;
            for (var i = 0; i < draft.Groups.Count; i++) Groups.Add(draft.Groups[i].Copy());
            for (var i = 0; i < draft.Triggers.Count; i++) Triggers.Add(draft.Triggers[i].Copy());
            for (var i = 0; i < draft.Slots.Count; i++)
            {
                var slot = draft.Slots[i].Copy();
                if (chosen.TryGetValue(slot.Id, out var previous) && ContainsSkin(slot, previous))
                    slot.Chosen = previous;
                else if (!ContainsSkin(slot, slot.Chosen))
                    slot.Chosen = slot.Skins.Count > 0 ? slot.Skins[0].Id : "";
                Slots.Add(slot);
            }

            foreach (var trigger in draft._clips)
            {
                var row = new Dictionary<string, string>();
                foreach (var cell in trigger.Value) row[cell.Key] = cell.Value;
                _clips[trigger.Key] = row;
            }

            foreach (var slot in draft._textures)
            {
                var skins = new Dictionary<string, Dictionary<string, string>>();
                foreach (var skin in slot.Value)
                {
                    var row = new Dictionary<string, string>();
                    foreach (var cell in skin.Value) row[cell.Key] = cell.Value;
                    skins[skin.Key] = row;
                }

                _textures[slot.Key] = skins;
            }

            GroupId = Contains(Groups, groupId) ? groupId : (Groups.Count > 0 ? Groups[0].Id : "");
            TriggerId = Contains(Triggers, triggerId) ? triggerId : (Triggers.Count > 0 ? Triggers[0].Id : "");
        }

        public SpineNamed AddGroup(string name)
        {
            var item = new SpineNamed { Id = Next("g"), Name = name };
            Groups.Add(item);
            return item;
        }

        public SpineNamed AddTrigger(string name)
        {
            var item = new SpineNamed { Id = Next("t"), Name = name };
            Triggers.Add(item);
            return item;
        }

        public SpineSlotSetup AddSlot(string name)
        {
            var slot = new SpineSlotSetup { Id = Next("slot"), Name = name };
            var skin = new SpineNamed { Id = Next("skin"), Name = "新皮肤" };
            slot.Skins.Add(skin);
            slot.Chosen = skin.Id;
            Slots.Add(slot);
            return slot;
        }

        public SpineNamed AddSkin(SpineSlotSetup slot, string name)
        {
            var skin = new SpineNamed { Id = Next("skin"), Name = name };
            slot.Skins.Add(skin);
            if (string.IsNullOrEmpty(slot.Chosen)) slot.Chosen = skin.Id;
            return skin;
        }

        public void RemoveGroup(string id)
        {
            RemoveNamed(Groups, id);
            foreach (var row in _clips.Values)
                row.Remove(id);
            foreach (var skins in _textures.Values)
            {
                foreach (var row in skins.Values)
                    row.Remove(id);
            }
        }

        public void RemoveTrigger(string id)
        {
            RemoveNamed(Triggers, id);
            _clips.Remove(id);
        }

        public void RemoveSlot(string id)
        {
            for (var i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].Id != id) continue;
                Slots.RemoveAt(i);
                _textures.Remove(id);
                return;
            }
        }

        public void RemoveSkin(SpineSlotSetup slot, string id)
        {
            if (slot == null) return;
            for (var i = 0; i < slot.Skins.Count; i++)
            {
                if (slot.Skins[i].Id != id) continue;
                slot.Skins.RemoveAt(i);
                break;
            }

            if (slot.Chosen == id)
                slot.Chosen = slot.Skins.Count > 0 ? slot.Skins[0].Id : "";
            if (_textures.TryGetValue(slot.Id, out var skins) && skins != null)
                skins.Remove(id);
        }

        public string Clip(string trigger, string group)
        {
            if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(group)) return "";
            if (!_clips.TryGetValue(trigger, out var row) || row == null) return "";
            return row.TryGetValue(group, out var value) ? value ?? "" : "";
        }

        public void SetClip(string trigger, string group, string value)
        {
            if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(group)) return;
            if (!_clips.TryGetValue(trigger, out var row) || row == null)
            {
                row = new Dictionary<string, string>();
                _clips[trigger] = row;
            }

            row[group] = value ?? "";
        }

        public string Texture(string slot, string skin, string group)
        {
            if (string.IsNullOrEmpty(slot) || string.IsNullOrEmpty(skin) || string.IsNullOrEmpty(group)) return "";
            if (!_textures.TryGetValue(slot, out var skins) || skins == null) return "";
            if (!skins.TryGetValue(skin, out var row) || row == null) return "";
            return row.TryGetValue(group, out var value) ? value ?? "" : "";
        }

        public void SetTexture(string slot, string skin, string group, string value)
        {
            if (string.IsNullOrEmpty(slot) || string.IsNullOrEmpty(skin) || string.IsNullOrEmpty(group)) return;
            if (!_textures.TryGetValue(slot, out var skins) || skins == null)
            {
                skins = new Dictionary<string, Dictionary<string, string>>();
                _textures[slot] = skins;
            }

            if (!skins.TryGetValue(skin, out var row) || row == null)
            {
                row = new Dictionary<string, string>();
                skins[skin] = row;
            }

            row[group] = value ?? "";
        }

        public string ResolveClip()
        {
            return Clip(TriggerId, GroupId);
        }

        public void ResolveSkins(string defaultSkin, List<string> into)
        {
            into.Clear();
            for (var i = 0; i < Slots.Count; i++)
            {
                var slot = Slots[i];
                var skin = ChosenSkin(slot);
                if (skin == null) continue;
                var name = Texture(slot.Id, skin.Id, GroupId);
                if (string.IsNullOrEmpty(name)) continue;
                if (!string.IsNullOrEmpty(defaultSkin) && string.Equals(name, defaultSkin, StringComparison.OrdinalIgnoreCase))
                    continue;
                into.Add(name);
            }
        }

        static SpineNamed ChosenSkin(SpineSlotSetup slot)
        {
            for (var i = 0; i < slot.Skins.Count; i++)
            {
                if (slot.Skins[i].Id == slot.Chosen) return slot.Skins[i];
            }

            return slot.Skins.Count > 0 ? slot.Skins[0] : null;
        }

        static bool ContainsSkin(SpineSlotSetup slot, string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (var i = 0; i < slot.Skins.Count; i++)
            {
                if (slot.Skins[i].Id == id) return true;
            }

            return false;
        }

        static bool Contains(List<SpineNamed> items, string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id) return true;
            }

            return false;
        }

        static void RemoveNamed(List<SpineNamed> items, string id)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Id != id) continue;
                items.RemoveAt(i);
                return;
            }
        }

        string Next(string prefix)
        {
            _seq++;
            return prefix + _seq;
        }

        public string Write()
        {
            var text = new StringBuilder();
            text.Append("v1\n");
            text.Append("seq\t").Append(_seq).Append('\n');
            text.Append("play\t").Append(Esc(GroupId)).Append('\t').Append(Esc(TriggerId)).Append('\n');
            for (var i = 0; i < Groups.Count; i++)
            {
                text.Append("group\t").Append(Esc(Groups[i].Id)).Append('\t').Append(Esc(Groups[i].Name)).Append('\n');
            }

            for (var i = 0; i < Triggers.Count; i++)
            {
                text.Append("trigger\t").Append(Esc(Triggers[i].Id)).Append('\t').Append(Esc(Triggers[i].Name)).Append('\n');
            }

            for (var i = 0; i < Slots.Count; i++)
            {
                var slot = Slots[i];
                text.Append("slot\t").Append(Esc(slot.Id)).Append('\t').Append(Esc(slot.Name)).Append('\t').Append(Esc(slot.Chosen)).Append('\n');
                for (var s = 0; s < slot.Skins.Count; s++)
                {
                    text.Append("skin\t").Append(Esc(slot.Id)).Append('\t').Append(Esc(slot.Skins[s].Id)).Append('\t').Append(Esc(slot.Skins[s].Name)).Append('\n');
                }
            }

            foreach (var trigger in _clips)
            {
                foreach (var cell in trigger.Value)
                {
                    text.Append("clip\t").Append(Esc(trigger.Key)).Append('\t').Append(Esc(cell.Key)).Append('\t').Append(Esc(cell.Value)).Append('\n');
                }
            }

            foreach (var slot in _textures)
            {
                foreach (var skin in slot.Value)
                {
                    foreach (var cell in skin.Value)
                    {
                        text.Append("tex\t").Append(Esc(slot.Key)).Append('\t').Append(Esc(skin.Key)).Append('\t').Append(Esc(cell.Key)).Append('\t').Append(Esc(cell.Value)).Append('\n');
                    }
                }
            }

            return text.ToString();
        }

        public void Read(string text)
        {
            Clear();
            if (string.IsNullOrEmpty(text)) return;
            var lines = text.Split('\n');
            SpineSlotSetup slot = null;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r');
                if (line.Length == 0 || line == "v1") continue;
                var parts = line.Split('\t');
                for (var p = 0; p < parts.Length; p++) parts[p] = Unesc(parts[p]);
                if (parts.Length < 2) continue;
                switch (parts[0])
                {
                    case "seq":
                        int.TryParse(parts[1], out _seq);
                        break;
                    case "play":
                        GroupId = parts[1];
                        TriggerId = parts.Length > 2 ? parts[2] : "";
                        break;
                    case "group":
                        if (parts.Length < 3 || parts[1].Length == 0) break;
                        Groups.Add(new SpineNamed { Id = parts[1], Name = parts[2] });
                        break;
                    case "trigger":
                        if (parts.Length < 3 || parts[1].Length == 0) break;
                        Triggers.Add(new SpineNamed { Id = parts[1], Name = parts[2] });
                        break;
                    case "slot":
                        if (parts.Length < 3 || parts[1].Length == 0) break;
                        slot = new SpineSlotSetup { Id = parts[1], Name = parts[2], Chosen = parts.Length > 3 ? parts[3] : "" };
                        Slots.Add(slot);
                        break;
                    case "skin":
                        if (parts.Length < 4 || parts[2].Length == 0) break;
                        slot = FindSlot(parts[1]);
                        if (slot == null) break;
                        slot.Skins.Add(new SpineNamed { Id = parts[2], Name = parts[3] });
                        break;
                    case "clip":
                        if (parts.Length < 3) break;
                        SetClip(parts[1], parts[2], parts.Length > 3 ? parts[3] : "");
                        break;
                    case "tex":
                        if (parts.Length < 4) break;
                        SetTexture(parts[1], parts[2], parts[3], parts.Length > 4 ? parts[4] : "");
                        break;
                }
            }

            if (!Contains(Groups, GroupId)) GroupId = Groups.Count > 0 ? Groups[0].Id : "";
            if (!Contains(Triggers, TriggerId)) TriggerId = Triggers.Count > 0 ? Triggers[0].Id : "";
            for (var i = 0; i < Slots.Count; i++)
            {
                if (!ContainsSkin(Slots[i], Slots[i].Chosen))
                    Slots[i].Chosen = Slots[i].Skins.Count > 0 ? Slots[i].Skins[0].Id : "";
            }
        }

        SpineSlotSetup FindSlot(string id)
        {
            for (var i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].Id == id) return Slots[i];
            }

            return null;
        }

        static string Esc(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        static string Unesc(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('\\') < 0) return value ?? "";
            var text = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch != '\\' || i + 1 >= value.Length)
                {
                    text.Append(ch);
                    continue;
                }

                i++;
                switch (value[i])
                {
                    case '\\': text.Append('\\'); break;
                    case 't': text.Append('\t'); break;
                    case 'r': text.Append('\r'); break;
                    case 'n': text.Append('\n'); break;
                    default: text.Append(value[i]); break;
                }
            }

            return text.ToString();
        }
    }

    sealed class SpineNamed
    {
        public string Id = "";
        public string Name = "";

        public SpineNamed Copy()
        {
            return new SpineNamed { Id = Id, Name = Name };
        }
    }

    sealed class SpineSlotSetup
    {
        public string Id = "";
        public string Name = "";
        public string Chosen = "";
        public readonly List<SpineNamed> Skins = new List<SpineNamed>();

        public SpineSlotSetup Copy()
        {
            var copy = new SpineSlotSetup { Id = Id, Name = Name, Chosen = Chosen };
            for (var i = 0; i < Skins.Count; i++) copy.Skins.Add(Skins[i].Copy());
            return copy;
        }
    }

    /// <summary>编辑详情。窗口、下拉框和行都是 FairyGUI 工程里的组件。</summary>
    sealed class SpineDetailDialog
    {
        readonly List<string> _clips = new List<string>();
        readonly List<string> _skins = new List<string>();
        SpinePreviewSetup _live;
        SpinePreviewSetup _draft;
        Action _onApply;
        UI_WindowSpineDetail _window;
        string _slotId = "";
        bool _filling;
        bool _fillPosted;
        TimerCallback _later;

        public bool Open(SpinePreviewSetup setup, IReadOnlyList<string> clips, IReadOnlyList<string> skins, string defaultSkin, Action onApply)
        {
            Close();
            _window = UI_WindowSpineDetail.CreateInstance();
            if (_window == null) return false;
            _live = setup ?? new SpinePreviewSetup();
            _draft = _live.Clone();
            _onApply = onApply;
            _clips.Clear();
            _skins.Clear();
            CopyNames(clips, _clips);
            CopyNames(skins, _skins);
            if (!string.IsNullOrEmpty(defaultSkin))
                _skins.RemoveAll(name => string.Equals(name, defaultSkin, StringComparison.OrdinalIgnoreCase));
            _slotId = _draft.Slots.Count > 0 ? _draft.Slots[0].Id : "";

            var root = GRoot.inst;
            _window.SetSize(root.width, root.height);
            _window.AddRelation(root, RelationType.Size);
            _window.sortingOrder = 1000;
            root.AddChild(_window);
            _window.m_mask.onClick.Set(Close);
            _window.m_btnCancel.onClick.Set(Close);
            _window.m_btnDone.onClick.Set(Apply);
            _window.m_btnAddTrigger.onClick.Set(() =>
            {
                _draft.AddTrigger("新触发器");
                Fill();
            });
            _window.m_btnAddGroup.onClick.Set(() =>
            {
                _draft.AddGroup("新群组");
                Fill();
            });
            _window.m_btnAddSlot.onClick.Set(() =>
            {
                var slot = _draft.AddSlot("新槽位");
                _slotId = slot.Id;
                Fill();
            });
            _window.m_btnAddSkin.onClick.Set(() =>
            {
                var slot = FindSlot(_slotId);
                if (slot == null) return;
                _draft.AddSkin(slot, "新皮肤");
                Fill();
            });
            _window.m_cPage.onChanged.Set(Fill);
            Fill();
            return true;
        }

        public void Close()
        {
            if (_later != null) Timers.inst.Remove(_later);
            _fillPosted = false;
            if (GRoot.inst != null) GRoot.inst.HidePopup();
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }

            _live = null;
            _draft = null;
            _onApply = null;
        }

        void Apply()
        {
            var live = _live;
            var draft = _draft;
            var apply = _onApply;
            if (live != null && draft != null) live.ApplyDraft(draft);
            Close();
            if (apply != null) apply();
        }

        void Fill()
        {
            if (_window == null || _draft == null || _filling) return;
            _filling = true;
            if (GRoot.inst != null) GRoot.inst.HidePopup();
            var editing = _window.m_cPage.selectedIndex == 0;
            BindHeads(_window.m_groupHeads, editing);
            BindHeads(_window.m_groupHeadsLook, false);
            BindNames(_window.m_triggers, true);
            BindNames(_window.m_skins, false);
            BindSlots(_window.m_slots);
            _filling = false;
        }

        void BindHeads(GList list, bool editing)
        {
            list.itemRenderer = (index, obj) =>
            {
                var group = _draft.Groups[index];
                var head = (UI_ItemGroupHead)obj;
                head.m_cEdit.selectedIndex = editing ? 0 : 1;
                var shown = string.IsNullOrEmpty(group.Name) ? "未命名" : group.Name;
                head.m_label.text = shown;
                var input = head.m_input;
                input.onChanged.Set(() => group.Name = input.text ?? "");
                input.text = group.Name ?? "";
                var id = group.Id;
                head.m_btnDelete.onClick.Set(() =>
                {
                    _draft.RemoveGroup(id);
                    FillSoon();
                });
            };
            list.numItems = _draft.Groups.Count;
        }

        void BindNames(GList list, bool triggers)
        {
            list.autoResizeItem = true;
            list.itemRenderer = (index, obj) =>
            {
                var row = (UI_ItemNameRow)obj;
                if (triggers)
                {
                    var trigger = _draft.Triggers[index];
                    BindName(row, trigger.Name, value => trigger.Name = value ?? "", () =>
                    {
                        _draft.RemoveTrigger(trigger.Id);
                        FillSoon();
                    });
                    BindCells(row.m_cells, (column, box) =>
                    {
                        var group = _draft.Groups[column];
                        FillCombo(box, _draft.Clip(trigger.Id, group.Id), "这一组不播", _clips, value => _draft.SetClip(trigger.Id, group.Id, value));
                    });
                }
                else
                {
                    var slot = FindSlot(_slotId);
                    if (slot == null) return;
                    var skin = slot.Skins[index];
                    BindName(row, skin.Name, value => skin.Name = value ?? "", () =>
                    {
                        _draft.RemoveSkin(slot, skin.Id);
                        FillSoon();
                    });
                    BindCells(row.m_cells, (column, box) =>
                    {
                        var group = _draft.Groups[column];
                        FillCombo(box, _draft.Texture(slot.Id, skin.Id, group.Id), "这一组不换", _skins, value => _draft.SetTexture(slot.Id, skin.Id, group.Id, value));
                    });
                }
            };
            var slotShown = FindSlot(_slotId);
            list.numItems = triggers ? _draft.Triggers.Count : (slotShown == null ? 0 : slotShown.Skins.Count);
        }

        void BindSlots(GList list)
        {
            list.autoResizeItem = true;
            if (!ContainsSlot(_slotId) && _draft.Slots.Count > 0) _slotId = _draft.Slots[0].Id;
            list.itemRenderer = (index, obj) =>
            {
                var slot = _draft.Slots[index];
                var row = (UI_ItemSlot)obj;
                var selected = slot.Id == _slotId;
                row.opaque = true;
                row.m_cOn.selectedIndex = selected ? 1 : 0;
                row.m_label.text = string.IsNullOrEmpty(slot.Name) ? "未命名" : slot.Name;
                var input = row.m_input;
                input.onChanged.Set(() => slot.Name = input.text ?? "");
                input.text = slot.Name ?? "";
                var id = slot.Id;
                row.onClick.Set(() =>
                {
                    if (_slotId == id) return;
                    _slotId = id;
                    FillSoon();
                });
                row.m_btnDelete.onClick.Set(context =>
                {
                    context.StopPropagation();
                    _draft.RemoveSlot(id);
                    if (_slotId == id)
                        _slotId = _draft.Slots.Count > 0 ? _draft.Slots[0].Id : "";
                    FillSoon();
                });
            };
            list.numItems = _draft.Slots.Count;
        }

        void BindName(UI_ItemNameRow row, string name, Action<string> rename, Action remove)
        {
            var input = row.m_input;
            input.onChanged.Set(() => rename(input.text));
            input.text = name ?? "";
            row.m_btnDelete.onClick.Set(() => remove());
        }

        void BindCells(GList list, Action<int, GComboBox> bind)
        {
            list.itemRenderer = (index, obj) => bind(index, (UI_ComboValue)obj);
            list.numItems = _draft.Groups.Count;
        }

        static void FillCombo(GComboBox box, string current, string empty, List<string> options, Action<string> pick)
        {
            var labels = new string[options.Count + 1];
            var keys = new string[options.Count + 1];
            labels[0] = empty;
            keys[0] = "";
            for (var i = 0; i < options.Count; i++)
            {
                labels[i + 1] = options[i];
                keys[i + 1] = options[i];
            }

            box.onChanged.Set(() => { });
            box.items = labels;
            box.values = keys;
            box.value = current ?? "";
            box.onChanged.Set(() =>
            {
                if (pick != null) pick(box.value ?? "");
            });
        }

        void FillSoon()
        {
            if (_later == null)
            {
                _later = _ =>
                {
                    _fillPosted = false;
                    Fill();
                };
            }

            if (_fillPosted || _window == null) return;
            _fillPosted = true;
            Timers.inst.CallLater(_later);
        }

        SpineSlotSetup FindSlot(string id)
        {
            if (_draft == null) return null;
            for (var i = 0; i < _draft.Slots.Count; i++)
            {
                if (_draft.Slots[i].Id == id) return _draft.Slots[i];
            }

            return null;
        }

        bool ContainsSlot(string id)
        {
            return FindSlot(id) != null;
        }

        static void CopyNames(IReadOnlyList<string> source, List<string> into)
        {
            if (source == null) return;
            for (var i = 0; i < source.Count; i++)
            {
                var name = source[i];
                if (!string.IsNullOrEmpty(name)) into.Add(name);
            }
        }
    }
}

