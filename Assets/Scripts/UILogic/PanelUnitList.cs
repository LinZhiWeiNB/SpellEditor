using System;
using System.Collections.Generic;
using FairyGUI;
using PackageResource;
using PackageUnit;
using RebornFrame.Spell;
using SpellEditor;

namespace UILogic
{
    public class PanelUnitList : FguiPanelBase
    {
        UI_PanelUnitList view;
        readonly List<UnitRecord> _shown = new List<UnitRecord>();
        readonly ResourceSpinePreview _spine = new ResourceSpinePreview();
        UnitRecord _selected;
        string _spineKey = "";

        public override void Start(object[] data)
        {
            view = (UI_PanelUnitList)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            view.m_search.text = "";
            view.m_search.onChanged.Add(Refresh);
            view.m_units.itemRenderer = RenderRow;
            view.m_units.onClickItem.Add(OnClick);
            view.m_btnAdd.onClick.Set(AddUnit);
            view.m_btnDelete.onClick.Set(DeleteUnit);
            view.m_btnCopy.onClick.Set(CopyUnit);
            view.m_btnEdit.onClick.Set(EditUnit);
            view.m_btnOpenResource.onClick.Set(OpenResource);
            OpenPreviewSpace();
            view.m_cSpine.selectedIndex = UnitCatalogs.SpineEnabled()
                ? (int)UI_PanelUnitList.EcSpine.开
                : (int)UI_PanelUnitList.EcSpine.关;
            Refresh();
            ClearSelection();
        }

        public override void OnShow()
        {
            if (view != null)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            _shown.Clear();
            var records = UnitLibrary.List();
            var query = (view.m_search.text ?? "").Trim();
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var unit = record.Unit;
                var title = new SpellId(unit.Id).ToString();
                var meta = Meta(record);
                if (query.Length > 0
                    && title.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                    && meta.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                    && (unit.SpineKey ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                _shown.Add(record);
            }

            view.m_units.numItems = _shown.Count;
            if (_selected == null)
            {
                return;
            }

            var again = UnitLibrary.Find(_selected.Unit.Id);
            if (again == null)
            {
                ClearSelection();
                return;
            }

            _selected = again;
            ShowDetail();
        }

        void RenderRow(int index, GObject obj)
        {
            var record = _shown[index];
            var item = (UI_ItemUnitRow)obj;
            item.title = new SpellId(record.Unit.Id).ToString();
            item.m_meta.text = Meta(record);
            item.selected = _selected != null && record.Unit.Id == _selected.Unit.Id;
        }

        void OnClick(EventContext context)
        {
            var index = view.m_units.GetChildIndex((GObject)context.data);
            if (index < 0 || index >= _shown.Count)
            {
                return;
            }

            _selected = _shown[index];
            ShowDetail();
            view.m_units.numItems = _shown.Count;
        }

        void ShowDetail()
        {
            var record = _selected;
            var unit = record.Unit;
            view.m_cDetail.selectedIndex = (int)UI_PanelUnitList.EcDetail.已选中;
            view.m_valueId.text = new SpellId(unit.Id).ToString();
            view.m_valueSource.text = SourceLine(record);
            view.m_valueSpine.text = string.IsNullOrEmpty(unit.SpineKey) ? "未选择" : unit.SpineKey;
            var hasSpine = !string.IsNullOrEmpty(unit.SpineKey);
            view.m_btnOpenResource.grayed = !hasSpine;
            view.m_btnOpenResource.touchable = hasSpine;
            if (record.Owned)
            {
                view.m_btnDelete.title = "删除";
                view.m_btnDelete.grayed = false;
                view.m_btnDelete.touchable = true;
            }
            else if (record.Diff)
            {
                view.m_btnDelete.title = "还原";
                view.m_btnDelete.grayed = false;
                view.m_btnDelete.touchable = true;
            }
            else
            {
                view.m_btnDelete.title = "删除";
                view.m_btnDelete.grayed = true;
                view.m_btnDelete.touchable = false;
            }

            FillLines(view.m_presets, unit.PresetAttributes, UnitCatalogs.AttributeLabel);
            FillValueLines(unit);
            FillLines(view.m_activeSpells, unit.ActiveSpells, UnitCatalogs.SpellLabel);
            FillLines(view.m_passiveSpells, unit.PassiveSpells, UnitCatalogs.SpellLabel);
            ShowSpine(unit.SpineKey);
        }

        void FillLines(GList list, List<ulong> ids, Func<ulong, string> label)
        {
            list.itemRenderer = (index, obj) =>
            {
                ((UI_ItemLine)obj).m_title.text = label(ids[index]);
            };
            list.numItems = ids.Count;
        }

        void FillValueLines(EditorUnit unit)
        {
            view.m_values.itemRenderer = (index, obj) =>
            {
                var id = unit.PresetAttributes[index];
                ((UI_ItemLine)obj).m_title.text = UnitCatalogs.AttributeLabel(id) + "    " + UnitCatalogs.Raw(unit, id);
            };
            view.m_values.numItems = unit.PresetAttributes.Count;
        }

        void ClearSelection()
        {
            _selected = null;
            _spine.Clear();
            _spineKey = "";
            view.m_cDetail.selectedIndex = (int)UI_PanelUnitList.EcDetail.未选中;
            view.m_btnDelete.title = "删除";
        }

        void OpenPreviewSpace()
        {
            const float cut = 24f;
            view.m_presets.height -= cut;
            view.m_labelValues.y -= cut;
            view.m_values.y -= cut;
            view.m_values.height -= cut;
            view.m_labelActive.y -= cut * 2f;
            view.m_activeSpells.y -= cut * 2f;
            view.m_activeSpells.height -= cut;
            view.m_labelPassive.y -= cut * 3f;
            view.m_passiveSpells.y -= cut * 3f;
            view.m_passiveSpells.height -= cut;
            var lift = cut * 4f;
            view.m_labelSpine.y -= lift;
            view.m_valueSpine.y -= lift;
            view.m_previewLabel.y -= lift;
            view.m_preview.y -= lift;
            view.m_preview.height += lift;
            view.m_btnOpenResource.SetXY(
                view.m_preview.x + view.m_preview.width - view.m_btnOpenResource.width,
                view.m_labelSpine.y - 4f);
            var spinePreview = view.GetChild("spinePreview");
            if (spinePreview != null)
            {
                spinePreview.SetXY(view.m_preview.x, view.m_preview.y);
                spinePreview.SetSize(view.m_preview.width, view.m_preview.height);
            }
        }

        void ShowSpine(string key)
        {
            if (!UnitCatalogs.SpineEnabled() || string.IsNullOrEmpty(key))
            {
                _spine.Clear();
                _spineKey = "";
                return;
            }

            if (key == _spineKey)
            {
                return;
            }

            var asset = FindSpine(key);
            if (asset == null || !ResourceLibrary.TryOpenSpine(asset, out var document, out _))
            {
                _spine.Clear();
                _spineKey = "";
                return;
            }

            if (!_spine.Show(view, view.m_btnOpenResource, view.m_preview, asset, document, true, out _))
            {
                _spineKey = "";
                return;
            }

            _spineKey = key;
        }

        static ResourceAsset FindSpine(string key)
        {
            var assets = ResourceLibrary.List();
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset != null && asset.Key == key && ResourceLibrary.IsSpine(asset.Kind))
                {
                    return asset;
                }
            }

            return null;
        }

        public override void OnClose()
        {
            _spine.Clear();
            _spineKey = "";
        }

        void AddUnit()
        {
            var record = UnitLibrary.Create();
            if (record == null)
            {
                return;
            }

            _selected = record;
            Refresh();
        }

        void DeleteUnit()
        {
            if (_selected == null || !UnitLibrary.Remove(_selected))
            {
                return;
            }

            _selected = null;
            Refresh();
            ClearSelection();
        }

        void CopyUnit()
        {
            var copy = UnitLibrary.Copy(_selected);
            if (copy == null)
            {
                return;
            }

            _selected = copy;
            Refresh();
        }

        void EditUnit()
        {
            if (_selected == null)
            {
                return;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelUnitEdit>(
                UI_PanelUnitEdit.PackageName,
                UI_PanelUnitEdit.ResName,
                new object[] { _selected.Unit.Id },
                ShowTypeEnum.HideCurrent);
        }

        void OpenResource()
        {
            if (_selected == null || string.IsNullOrEmpty(_selected.Unit.SpineKey))
            {
                return;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelResource>(
                UI_PanelResource.PackageName,
                UI_PanelResource.ResName,
                new object[] { _selected.Unit.SpineKey },
                ShowTypeEnum.HideCurrent);
        }

        static string Meta(UnitRecord record)
        {
            var unit = record.Unit;
            var origin = record.Owned ? "本项目" : record.Source;
            if (record.Diff)
            {
                origin += " · 本地差异";
            }

            return origin + "  ·  属性 " + unit.PresetAttributes.Count
                + "  ·  技能 " + (unit.ActiveSpells.Count + unit.PassiveSpells.Count);
        }

        static string SourceLine(UnitRecord record)
        {
            if (record.Owned)
            {
                return "本项目";
            }

            if (record.Diff)
            {
                return "来自 " + record.Source + "  ·  本地差异，游戏按模组载入顺序覆盖";
            }

            return "来自 " + record.Source;
        }
    }
}
