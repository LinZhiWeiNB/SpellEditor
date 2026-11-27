using System.Collections.Generic;
using FairyGUI;
using PackageSpell;
using PackageUnit;
using RebornFrame.Spell;
using GameContentEditor;

namespace UILogic
{
    public class PanelUnitEdit : FguiPanelBase
    {
        UI_PanelUnitEdit view;
        UnitRecord _record;
        readonly List<ulong> _attributes = new List<ulong>();
        readonly List<ulong> _active = new List<ulong>();
        readonly List<ulong> _passive = new List<ulong>();
        bool _applying;

        public override void Start(object[] data)
        {
            view = (UI_PanelUnitEdit)FGUIComponent;
            view.m_btnBack.onClick.Set(() =>
            {
                PanelLayer.Dismiss<PanelUnitPick>();
                FGUIPanelManager.Instance.Back();
            });
            view.m_btnAddActive.onClick.Set(() => PickSpell(_active, Unit().ActiveSpells, view.m_activeSpells));
            view.m_btnAddPassive.onClick.Set(() => PickSpell(_passive, Unit().PassiveSpells, view.m_passiveSpells));
            view.m_btnPreset.onClick.Set(PickPreset);
            view.m_btnSpine.onClick.Set(PickSpine);
            var id = data != null && data.Length > 0 && data[0] is ulong value ? value : 0UL;
            _record = UnitLibrary.Find(id);
            view.m_cSpine.selectedIndex = UnitCatalogs.SpineEnabled()
                ? (int)UI_PanelUnitEdit.EcSpine.开
                : (int)UI_PanelUnitEdit.EcSpine.关;
            RenderAll();
        }

        public override void OnShow()
        {
            if (view == null || _record == null)
            {
                return;
            }

            var again = UnitLibrary.Find(_record.Unit.Id);
            if (again != null)
            {
                _record = again;
            }

            RenderAll();
        }

        public override void OnClose()
        {
            PanelLayer.Dismiss<PanelUnitPick>();
        }

        void RenderAll()
        {
            _attributes.Clear();
            _active.Clear();
            _passive.Clear();
            if (_record == null || _record.Unit == null)
            {
                view.m_cOrigin.selectedIndex = (int)UI_PanelUnitEdit.EcOrigin.本项目;
                view.m_valueId.text = "";
                view.m_btnSpine.title = "选择 Spine 骨骼";
                view.m_btnPreset.title = "选择属性预设";
                view.m_values.numItems = 0;
                view.m_activeSpells.numItems = 0;
                view.m_passiveSpells.numItems = 0;
                return;
            }

            view.m_cOrigin.selectedIndex = _record.Owned
                ? (int)UI_PanelUnitEdit.EcOrigin.本项目
                : (int)UI_PanelUnitEdit.EcOrigin.差异;
            if (!_record.Owned)
            {
                view.m_overrideHint.text = "这是对「" + _record.Source
                    + "」的本地差异。游戏按玩家设置的模组载入顺序覆盖，更晚载入的模组会盖过这里的修改。";
            }

            view.m_valueId.text = new SpellId(_record.Unit.Id).ToString();
            _attributes.AddRange(UnitCatalogs.Attributes());
            UnitCatalogs.Spells(_active, _passive);
            view.m_spineHint.text = UnitCatalogs.SpineKeys().Count == 0
                ? "资源列表里还没有 Spine 骨骼。"
                : "点按钮，从资源列表里选择一个 Spine 骨骼。";
            RenderSpineButton();
            KeepOnePreset();
            RenderPresetButton();
            RenderValues();
            RenderSpellRows(view.m_activeSpells, _record.Unit.ActiveSpells);
            RenderSpellRows(view.m_passiveSpells, _record.Unit.PassiveSpells);
        }

        EditorUnit Unit()
        {
            return _record == null ? null : _record.Unit;
        }

        void Commit()
        {
            UnitLibrary.Commit(_record);
        }

        void KeepOnePreset()
        {
            var unit = Unit();
            if (unit == null || unit.PresetAttributes.Count <= 1)
            {
                return;
            }

            var keep = unit.PresetAttributes[0];
            unit.PresetAttributes.Clear();
            unit.PresetAttributes.Add(keep);
            for (var i = unit.Values.Count - 1; i >= 0; i--)
            {
                if (unit.Values[i].AttributeId != keep)
                {
                    unit.Values.RemoveAt(i);
                }
            }

            Commit();
        }

        void RenderPresetButton()
        {
            var unit = Unit();
            var chosen = unit != null && unit.PresetAttributes.Count > 0 ? unit.PresetAttributes[0] : 0UL;
            view.m_btnPreset.title = unit == null || unit.PresetAttributes.Count == 0
                ? "选择属性预设"
                : UnitCatalogs.AttributeLabel(chosen);
        }

        void PickPreset()
        {
            var unit = Unit();
            if (unit == null)
            {
                return;
            }

            var keys = new List<string>();
            var titles = new List<string>();
            for (var i = 0; i < _attributes.Count; i++)
            {
                keys.Add(_attributes[i].ToString());
                titles.Add(UnitCatalogs.AttributeLabel(_attributes[i]));
            }

            PanelUnitPick.Show("选择属性预设", keys, titles, key =>
            {
                if (!ulong.TryParse(key, out var id))
                {
                    return;
                }

                unit.PresetAttributes.Clear();
                unit.PresetAttributes.Add(id);
                for (var i = unit.Values.Count - 1; i >= 0; i--)
                {
                    if (unit.Values[i].AttributeId != id)
                    {
                        unit.Values.RemoveAt(i);
                    }
                }

                UnitCatalogs.SetRaw(unit, id, UnitCatalogs.Raw(unit, id));
                Commit();
                RenderPresetButton();
                RenderValues();
            });
        }

        void RenderValues()
        {
            var unit = Unit();
            _applying = true;
            view.m_values.itemRenderer = (index, obj) =>
            {
                var id = unit.PresetAttributes[index];
                var row = (UI_ItemValue)obj;
                row.m_title.text = UnitCatalogs.AttributeLabel(id);
                row.m_input.text = UnitCatalogs.Raw(unit, id).ToString();
                row.m_input.onChanged.Set(() =>
                {
                    if (_applying)
                    {
                        return;
                    }

                    long.TryParse(row.m_input.text, out var raw);
                    UnitCatalogs.SetRaw(unit, id, raw);
                    Commit();
                });
            };
            view.m_values.numItems = unit.PresetAttributes.Count;
            _applying = false;
        }

        void RenderSpellRows(GList list, List<ulong> chosen)
        {
            list.itemRenderer = (index, obj) =>
            {
                var id = chosen[index];
                var row = (UI_ItemSpellRow)obj;
                row.m_title.text = UnitCatalogs.SpellLabel(id);
                row.SetSize(list.width, row.height);
                row.m_btnEdit.onClick.Set(() => OpenSpell(id));
                row.m_btnDelete.onClick.Set(() =>
                {
                    chosen.Remove(id);
                    Commit();
                    RenderSpellRows(list, chosen);
                });
            };
            list.numItems = chosen.Count;
        }

        void PickSpell(List<ulong> catalog, List<ulong> chosen, GList list)
        {
            var unit = Unit();
            if (unit == null || chosen == null)
            {
                return;
            }

            var keys = new List<string>();
            var titles = new List<string>();
            for (var i = 0; i < catalog.Count; i++)
            {
                if (chosen.Contains(catalog[i]))
                {
                    continue;
                }

                keys.Add(catalog[i].ToString());
                titles.Add(UnitCatalogs.SpellLabel(catalog[i]));
            }

            PanelUnitPick.Show("新增技能", keys, titles, key =>
            {
                if (!ulong.TryParse(key, out var id) || chosen.Contains(id))
                {
                    return;
                }

                chosen.Add(id);
                Commit();
                RenderSpellRows(list, chosen);
            });
        }

        void OpenSpell(ulong spellId)
        {
            var unit = Unit();
            if (unit == null)
            {
                return;
            }

            PanelLayer.Dismiss<PanelUnitPick>();
            FGUIPanelManager.Instance.ShowPanel<PanelSpell>(
                UI_PanelSpell.PackageName,
                UI_PanelSpell.ResName,
                new object[] { spellId, unit.Id },
                ShowTypeEnum.HideCurrent);
        }

        void RenderSpineButton()
        {
            var key = Unit().SpineKey ?? "";
            view.m_btnSpine.title = key.Length == 0 ? "选择 Spine 骨骼" : key;
        }

        void PickSpine()
        {
            var unit = Unit();
            if (unit == null)
            {
                return;
            }

            var keys = UnitCatalogs.SpineKeys();
            PanelUnitPick.Show("选择 Spine 骨骼", keys, keys, key =>
            {
                unit.SpineKey = key ?? "";
                ResourceReferences.BindUnitSpine(unit.Id, unit.SpineKey);
                Commit();
                RenderSpineButton();
            });
        }
    }
}
