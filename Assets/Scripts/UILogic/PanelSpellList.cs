using System;
using System.Collections.Generic;
using FairyGUI;
using PackageSpell;
using RebornFrame.Spell;
using GameContentEditor;

namespace UILogic
{
    public class PanelSpellList : FguiPanelBase
    {
        sealed class Row
        {
            public SpellData Spell;
            public bool Owned;
            public string Source;
            public string Title;
            public string Meta;
        }

        UI_PanelSpellList view;
        readonly List<Row> _rows = new List<Row>();
        readonly List<Row> _shown = new List<Row>();
        SpellData _selected;
        bool _selectedOwned;

        public override void Start(object[] data)
        {
            view = (UI_PanelSpellList)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            view.m_search.text = "";
            view.m_search.onChanged.Add(Refresh);
            view.m_spells.itemRenderer = RenderRow;
            view.m_spells.onClickItem.Add(OnClick);
            view.m_btnAdd.onClick.Set(AddSpell);
            view.m_btnDelete.onClick.Set(DeleteSpell);
            view.m_btnCopy.onClick.Set(CopySpell);
            view.m_btnEdit.onClick.Set(EditSpell);
            Reload();
            ClearSelection();
        }

        void Reload()
        {
            _rows.Clear();
            var seen = new HashSet<ulong>();
            AddModule(ProjectSession.OwnModule, null, true, seen);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var source = ProjectSession.Sources[i];
                AddModule(source.Module, ProjectSession.KindLabel(source), false, seen);
            }

            Refresh();
        }

        void AddModule(SpellModuleData module, string source, bool owned, HashSet<ulong> seen)
        {
            if (module == null) return;
            for (var i = 0; i < module.Spells.Count; i++)
            {
                var spell = module.Spells[i];
                if (spell == null || !seen.Add(spell.Id)) continue;
                var id = new SpellId(spell.Id);
                _rows.Add(new Row
                {
                    Spell = spell,
                    Owned = owned,
                    Source = source ?? "",
                    Title = id.ToString(),
                    Meta = Summary(spell, owned, source),
                });
            }
        }

        void Refresh()
        {
            var query = (view.m_search.text ?? "").Trim();
            _shown.Clear();
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (query.Length > 0
                    && row.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                    && row.Meta.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                _shown.Add(row);
            }

            view.m_spells.numItems = _shown.Count;
            var still = false;
            for (var i = 0; i < _shown.Count; i++)
            {
                if (_selected == null || _shown[i].Spell.Id != _selected.Id || _shown[i].Owned != _selectedOwned)
                    continue;
                still = true;
                break;
            }

            if (_selected != null && !still)
                ClearSelection();
            else if (_selected != null)
                ShowDetail();
        }

        void RenderRow(int index, GObject obj)
        {
            var row = _shown[index];
            var item = (UI_ItemSpell)obj;
            item.title = row.Title;
            item.m_meta.text = row.Meta;
            item.m_cTone.selectedIndex = (int)UI_ItemSpell.EcTone.本源;
            item.selected = _selected != null && row.Spell.Id == _selected.Id && row.Owned == _selectedOwned;
        }

        void OnClick(EventContext context)
        {
            var index = view.m_spells.GetChildIndex((GObject)context.data);
            if (index < 0 || index >= _shown.Count) return;
            var row = _shown[index];
            _selected = row.Spell;
            _selectedOwned = row.Owned;
            ShowDetail();
            view.m_spells.numItems = _shown.Count;
        }

        void ShowDetail()
        {
            var spell = _selected;
            view.m_cDetail.selectedIndex = (int)UI_PanelSpellList.EcDetail.已选中;
            view.m_btnEdit.grayed = false;
            view.m_btnEdit.touchable = true;
            view.m_valueId.text = new SpellId(spell.Id).ToString();
            view.m_valueActivation.text = ActivationLabel(spell.Activation);
            view.m_valueDuration.text = spell.DurationFrames.ToString();
            view.m_valueCooldown.text = spell.CooldownFrames.ToString();
            view.m_valueAdmission.text = AdmissionLabel(spell.Admission);
        }

        void ClearSelection()
        {
            _selected = null;
            _selectedOwned = false;
            view.m_cDetail.selectedIndex = (int)UI_PanelSpellList.EcDetail.未选中;
        }

        void AddSpell()
        {
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null) return;
            var spell = new SpellData
            {
                Id = SpellId.Combine(ProjectSession.AuthorId, NextLocalId()).Value,
                Activation = (int)SpellActivation.Active,
            };
            ProjectSession.OwnModule.Spells.Add(spell);
            ProjectSession.Save();
            _selected = spell;
            _selectedOwned = true;
            Reload();
        }

        void DeleteSpell()
        {
            if (_selected == null || ProjectSession.OwnModule == null) return;
            var spells = ProjectSession.OwnModule.Spells;
            SpellData own = null;
            for (var i = 0; i < spells.Count; i++)
            {
                if (spells[i] != null && spells[i].Id == _selected.Id)
                {
                    own = spells[i];
                    break;
                }
            }

            if (own == null) return;
            spells.Remove(own);
            ProjectSession.Save();
            _selected = null;
            Reload();
            ClearSelection();
        }

        void CopySpell()
        {
            if (_selected == null || !ProjectSession.IsOpen || ProjectSession.OwnModule == null) return;
            var copy = _selected.Clone();
            copy.Id = SpellId.Combine(ProjectSession.AuthorId, NextLocalId()).Value;
            ProjectSession.OwnModule.Spells.Add(copy);
            ProjectSession.Save();
            _selected = copy;
            _selectedOwned = true;
            Reload();
        }

        void EditSpell()
        {
            if (_selected == null || !ProjectSession.IsOpen || ProjectSession.OwnModule == null) return;
            var spell = ProjectSession.EnsureOwnSpell(_selected);
            if (spell == null) return;
            _selected = spell;
            _selectedOwned = true;
            Reload();
            FGUIPanelManager.Instance.ShowPanel<PanelSpell>(
                UI_PanelSpell.PackageName,
                UI_PanelSpell.ResName,
                new object[] { spell.Id },
                ShowTypeEnum.HideCurrent);
        }

        uint NextLocalId()
        {
            var author = ProjectSession.AuthorId;
            var floor = author == 0 ? LibrarySpell.ReservedLocalIdCount : 1u;
            var highest = floor - 1;
            Consider(ProjectSession.OwnModule);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
                Consider(ProjectSession.Sources[i].Module);
            var next = highest + 1;
            return next < floor ? floor : next;

            void Consider(SpellModuleData module)
            {
                if (module == null) return;
                for (var i = 0; i < module.Spells.Count; i++)
                {
                    var id = new SpellId(module.Spells[i].Id);
                    if (id.AuthorId == author && id.LocalId > highest)
                        highest = id.LocalId;
                }
            }
        }

        static string Summary(SpellData spell, bool owned, string source)
        {
            var text = ActivationLabel(spell.Activation) + "  ·  持续 " + spell.DurationFrames + " 帧";
            if (!owned && !string.IsNullOrEmpty(source))
                text += "  ·  " + source;
            return text;
        }

        static string ActivationLabel(int activation)
        {
            switch ((SpellActivation)activation)
            {
                case SpellActivation.Active: return "主动";
                case SpellActivation.Passive: return "被动";
                default: return "未设置";
            }
        }

        static string AdmissionLabel(int admission)
        {
            switch ((BuffTagQuery)admission)
            {
                case BuffTagQuery.All: return "全部满足";
                case BuffTagQuery.Any: return "满足任意";
                case BuffTagQuery.NotAny: return "全部不满足";
                default: return "无";
            }
        }
    }
}
