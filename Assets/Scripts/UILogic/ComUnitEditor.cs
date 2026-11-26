using System.Collections.Generic;
using FairyGUI;
using PackageAtrribute;
using PackageUnit;
using RebornFrame.Spell;
using SpellEditor;

namespace UILogic
{
    /// <summary>
    /// 单位预设。左边分开当前项目和其他数据源。改过其他数据源的预设后，才写进当前项目，并藏起相同标识的那一条。
    /// </summary>
    public class ComUnitEditor
    {
        readonly UI_PanelUnit view;
        readonly List<UnitPreset> _ownShown = new List<UnitPreset>();
        readonly List<UnitPreset> _otherShown = new List<UnitPreset>();
        readonly List<UnitPreset> _foreign = new List<UnitPreset>();
        readonly UnitPickDialog _pick = new UnitPickDialog();
        UnitPreset _selected;
        bool _selectedOwned;
        bool _suppress;

        public ComUnitEditor(UI_PanelUnit view)
        {
            this.view = view;
            view.m_btnBack.onClick.Set(() =>
            {
                _pick.Close();
                FGUIPanelManager.Instance.Back();
            });
            view.m_toggleOthers.onChanged.Add(OnToggleOthers);
            view.m_units.itemRenderer = RenderOwn;
            view.m_others.itemRenderer = RenderOther;
            view.m_units.onClickItem.Add(OnClickOwn);
            view.m_others.onClickItem.Add(OnClickOther);
            view.m_pgName.onFocusOut.Add(CommitName);
            view.m_btnAdd.onClick.Add(AddPreset);
            view.m_btnAddAttribute.onClick.Add(AddAttribute);
            Refresh();
        }

        public void Close()
        {
            _pick.Close();
        }

        public void Refresh()
        {
            var keep = _selected == null ? 0UL : _selected.Id;
            var keepOwned = _selectedOwned;
            var hasSelection = _selected != null;
            _ownShown.Clear();
            _otherShown.Clear();
            var archive = ProjectSession.Archive;
            if (archive != null)
            {
                for (var i = 0; i < archive.UnitPresets.Count; i++)
                {
                    if (archive.UnitPresets[i] != null)
                    {
                        _ownShown.Add(archive.UnitPresets[i]);
                    }
                }
            }

            _foreign.Clear();
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var before = _foreign.Count;
                UnitPresetPackage.Read(ProjectSession.Sources[i].AbsolutePath, _foreign);
                for (var j = before; j < _foreign.Count; j++)
                {
                    var preset = _foreign[j];
                    if (preset != null && !OwnHas(preset.Id))
                    {
                        _otherShown.Add(preset);
                    }
                }
            }

            _suppress = true;
            view.m_units.numItems = _ownShown.Count;
            view.m_others.numItems = _otherShown.Count;
            _suppress = false;
            if (!hasSelection)
            {
                ClearDetail();
                return;
            }

            if (keepOwned)
            {
                var index = IndexOf(_ownShown, keep);
                if (index >= 0)
                {
                    SelectOwn(index);
                    return;
                }
            }
            else
            {
                var index = IndexOf(_otherShown, keep);
                if (index >= 0)
                {
                    SelectOther(index);
                    return;
                }
            }

            _selected = null;
            ClearDetail();
        }

        void AddPreset()
        {
            if (!ProjectSession.IsOpen || ProjectSession.Archive == null)
            {
                return;
            }

            var preset = new UnitPreset
            {
                Id = SpellId.Combine(ProjectSession.AuthorId, NextLocalId()).Value,
            };
            ProjectSession.Archive.UnitPresets.Add(preset);
            ProjectSession.Save();
            _selected = preset;
            _selectedOwned = true;
            Refresh();
        }

        void OnToggleOthers(EventContext context)
        {
            view.m_cOthers.selectedIndex = view.m_toggleOthers.selected
                ? (int)UI_PanelUnit.EcOthers.显示
                : (int)UI_PanelUnit.EcOthers.隐藏;
        }

        void OnClickOwn(EventContext context)
        {
            if (_suppress)
            {
                return;
            }

            SelectOwn(view.m_units.GetChildIndex((GObject)context.data));
        }

        void OnClickOther(EventContext context)
        {
            if (_suppress)
            {
                return;
            }

            SelectOther(view.m_others.GetChildIndex((GObject)context.data));
        }

        void SelectOwn(int index)
        {
            if (index < 0 || index >= _ownShown.Count)
            {
                _selected = null;
                ClearDetail();
                return;
            }

            _selected = _ownShown[index];
            _selectedOwned = true;
            _suppress = true;
            view.m_units.selectedIndex = index;
            view.m_others.selectedIndex = -1;
            _suppress = false;
            BindDetail();
        }

        void SelectOther(int index)
        {
            if (index < 0 || index >= _otherShown.Count)
            {
                _selected = null;
                ClearDetail();
                return;
            }

            _selected = _otherShown[index];
            _selectedOwned = false;
            _suppress = true;
            view.m_others.selectedIndex = index;
            view.m_units.selectedIndex = -1;
            _suppress = false;
            BindDetail();
        }

        void RenderOwn(int index, GObject item)
        {
            RenderRow((UI_ItemUnit)item, _ownShown[index]);
        }

        void RenderOther(int index, GObject item)
        {
            RenderRow((UI_ItemUnit)item, _otherShown[index]);
        }

        static void RenderRow(UI_ItemUnit row, UnitPreset preset)
        {
            var id = new SpellId(preset.Id);
            row.title = string.IsNullOrEmpty(preset.Name) ? "预设 " + id.LocalId : preset.Name;
            row.m_meta.text = "预设 ID " + id.LocalId;
        }

        void BindDetail()
        {
            var preset = _selected;
            _suppress = true;
            view.m_cInspect.selectedIndex = (int)UI_PanelUnit.EcInspect.已选中;
            view.m_pgName.text = preset.Name ?? "";
            _suppress = false;
            RenderAttributes();
        }

        void ClearDetail()
        {
            _suppress = true;
            view.m_cInspect.selectedIndex = (int)UI_PanelUnit.EcInspect.未选中;
            view.m_units.selectedIndex = -1;
            view.m_others.selectedIndex = -1;
            view.m_pgName.text = "";
            view.m_included.numItems = 0;
            _suppress = false;
        }

        void CommitName(EventContext context)
        {
            if (_suppress || _selected == null)
            {
                return;
            }

            var name = (view.m_pgName.text ?? "").Trim();
            if (name == (_selected.Name ?? ""))
            {
                return;
            }

            if (!EnsureOwned())
            {
                return;
            }

            _selected.Name = name;
            ProjectSession.Save();
            Refresh();
        }

        void RenderAttributes()
        {
            var preset = _selected;
            var chosen = preset == null ? null : preset.Attributes;
            view.m_included.itemRenderer = (index, obj) =>
            {
                var id = chosen[index];
                var row = (UI_ItemSpellRow)obj;
                row.m_title.text = UnitCatalogs.AttributeLabel(id);
                row.SetSize(view.m_included.width, row.height);
                row.m_btnEdit.onClick.Set(() => OpenAttribute(id));
                row.m_btnDelete.onClick.Set(() =>
                {
                    if (_selected == null || !EnsureOwned())
                    {
                        return;
                    }

                    _selected.Attributes.Remove(id);
                    ProjectSession.Save();
                    Refresh();
                });
            };
            view.m_included.numItems = chosen == null ? 0 : chosen.Count;
        }

        void AddAttribute()
        {
            if (_selected == null)
            {
                return;
            }

            var catalog = UnitCatalogs.Attributes();
            var keys = new List<string>();
            var titles = new List<string>();
            for (var i = 0; i < catalog.Count; i++)
            {
                if (_selected.Attributes.Contains(catalog[i]))
                {
                    continue;
                }

                keys.Add(catalog[i].ToString());
                titles.Add(UnitCatalogs.AttributeLabel(catalog[i]));
            }

            _pick.Open("新增属性", keys, titles, key =>
            {
                if (_selected == null || !ulong.TryParse(key, out var id) || _selected.Attributes.Contains(id) || !EnsureOwned())
                {
                    return;
                }

                _selected.Attributes.Add(id);
                ProjectSession.Save();
                Refresh();
            });
        }

        void OpenAttribute(ulong id)
        {
            _pick.Close();
            FGUIPanelManager.Instance.ShowPanel<PanelAttribute>(
                UI_PanelAttribute.PackageName,
                UI_PanelAttribute.ResName,
                new object[] { id },
                ShowTypeEnum.HideCurrent);
        }

        bool EnsureOwned()
        {
            if (_selected == null || !ProjectSession.IsOpen || ProjectSession.Archive == null)
            {
                return false;
            }

            if (_selectedOwned)
            {
                return true;
            }

            var presets = ProjectSession.Archive.UnitPresets;
            for (var i = 0; i < presets.Count; i++)
            {
                if (presets[i] != null && presets[i].Id == _selected.Id)
                {
                    _selected = presets[i];
                    _selectedOwned = true;
                    return true;
                }
            }

            var copy = Clone(_selected);
            presets.Add(copy);
            _selected = copy;
            _selectedOwned = true;
            return true;
        }

        static UnitPreset Clone(UnitPreset source)
        {
            var copy = new UnitPreset
            {
                Id = source.Id,
                Name = source.Name ?? "",
            };
            for (var i = 0; i < source.Attributes.Count; i++)
            {
                copy.Attributes.Add(source.Attributes[i]);
            }

            return copy;
        }

        bool OwnHas(ulong id)
        {
            var presets = ProjectSession.Archive == null ? null : ProjectSession.Archive.UnitPresets;
            if (presets == null)
            {
                return false;
            }

            for (var i = 0; i < presets.Count; i++)
            {
                if (presets[i] != null && presets[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        uint NextLocalId()
        {
            var author = ProjectSession.AuthorId;
            var highest = 0u;
            Note(_ownShown, author, ref highest);
            Note(_otherShown, author, ref highest);
            var next = highest + 1;
            return next == 0 ? 1u : next;
        }

        static void Note(List<UnitPreset> presets, uint author, ref uint highest)
        {
            for (var i = 0; i < presets.Count; i++)
            {
                var id = new SpellId(presets[i].Id);
                if (id.AuthorId == author && id.LocalId > highest)
                {
                    highest = id.LocalId;
                }
            }
        }

        static int IndexOf(List<UnitPreset> presets, ulong id)
        {
            for (var i = 0; i < presets.Count; i++)
            {
                if (presets[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
