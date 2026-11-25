using System;
using System.Collections.Generic;
using FairyGUI;
using PackageAtrribute;
using RebornFrame.Spell;
using SpellEditor;

namespace UILogic
{
    public class ComAttributeEditor
    {
        struct ShownOther
        {
            public AttributeDefinitionConfig Definition;
            public LoadedSource Source;
        }

        readonly UI_PanelAttribute view;
        readonly List<AttributeDefinitionConfig> _ownShown = new List<AttributeDefinitionConfig>();
        readonly List<ShownOther> _otherShown = new List<ShownOther>();
        AttributeDefinitionConfig _selected;
        bool _selectedOwned;
        bool _suppress;

        public ComAttributeEditor(UI_PanelAttribute view)
        {
            this.view = view;
            view.m_search.text = "";
            view.m_search.onChanged.Add(RefreshLists);
            view.m_cFilter.onChanged.Add(RefreshLists);
            view.m_toggleOthers.onChanged.Add(OnToggleOthers);
            view.m_attrs.itemRenderer = RenderOwn;
            view.m_others.itemRenderer = RenderOther;
            view.m_attrs.onClickItem.Add(OnClickOwn);
            view.m_others.onClickItem.Add(OnClickOther);
            view.m_cType.onChanged.Add(OnTypeChanged);
            view.m_mNameInput.onFocusOut.Add(CommitName);
            view.m_mIdInput.onFocusOut.Add(CommitId);
            view.m_mIdInput.restrict = "[0-9]";
            view.m_mScale.onChanged.Add(OnScaleChanged);
            view.m_mNonNeg.onChanged.Add(OnNonNegativeChanged);
            view.m_mDelete.onClick.Add(DeleteSelected);
            BindHeader();
            RefreshLists();
            ClearSelection();
        }

        void BindHeader()
        {
            var archive = ProjectSession.Archive;
            view.m_modSource.text = archive.Name;
            view.m_modSourceSub.text = archive.AuthorId == 0
                ? "当前数据源 · 游戏项目"
                : "当前数据源 · 作者 " + archive.AuthorId;
            var names = archive.Name;
            for (int i = 0; i < ProjectSession.Sources.Count; i++)
            {
                names += "、" + ProjectSession.Sources[i].Title;
            }

            view.m_mLoaded.text = "已载入 " + names + "。只有" + archive.Name + "里的属性可以改。";
        }

        void RefreshLists()
        {
            var hasSelection = _selected != null;
            var keep = hasSelection ? _selected.Id : 0UL;
            var keepOwned = _selectedOwned;
            _ownShown.Clear();
            _otherShown.Clear();
            Collect(ProjectSession.OwnModule, null, _ownShown, _otherShown);
            for (int i = 0; i < ProjectSession.Sources.Count; i++)
            {
                Collect(ProjectSession.Sources[i].Module, ProjectSession.Sources[i], _ownShown, _otherShown);
            }

            _suppress = true;
            view.m_attrs.numItems = _ownShown.Count;
            view.m_others.numItems = _otherShown.Count;
            _suppress = false;
            if (!hasSelection)
            {
                ClearSelection();
                return;
            }

            if (keepOwned)
            {
                var index = IndexOf(_ownShown, keep);
                if (index >= 0)
                {
                    _suppress = true;
                    view.m_attrs.selectedIndex = index;
                    view.m_others.selectedIndex = -1;
                    _suppress = false;
                    SelectOwn(index);
                    return;
                }
            }
            else
            {
                for (int i = 0; i < _otherShown.Count; i++)
                {
                    if (_otherShown[i].Definition.Id != keep)
                    {
                        continue;
                    }

                    _suppress = true;
                    view.m_others.selectedIndex = i;
                    view.m_attrs.selectedIndex = -1;
                    _suppress = false;
                    SelectOther(i);
                    return;
                }
            }

            ClearSelection();
        }

        void Collect(
            SpellModuleData module,
            LoadedSource source,
            List<AttributeDefinitionConfig> own,
            List<ShownOther> other)
        {
            if (module == null)
            {
                return;
            }

            var attributes = module.Attributes;
            for (int i = 0; i < attributes.Count; i++)
            {
                var definition = attributes[i];
                if (!Match(definition, source))
                {
                    continue;
                }

                if (source == null)
                {
                    own.Add(definition);
                }
                else
                {
                    other.Add(new ShownOther { Definition = definition, Source = source });
                }
            }
        }

        bool Match(AttributeDefinitionConfig definition, LoadedSource source)
        {
            var filter = (UI_PanelAttribute.EcFilter)view.m_cFilter.selectedIndex;
            if (filter != UI_PanelAttribute.EcFilter.全部 && BehaviorOf(definition) != filter)
            {
                return false;
            }

            var query = view.m_search.text;
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            var id = new AttributeId(definition.Id);
            var name = ProjectSession.AttributeName(definition.Id, id.LocalId);
            var sourceName = source == null ? ProjectSession.Archive.Name : source.Title;
            return name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                   || id.LocalId.ToString().IndexOf(query, StringComparison.Ordinal) >= 0
                   || id.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                   || sourceName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static UI_PanelAttribute.EcFilter BehaviorOf(AttributeDefinitionConfig definition)
        {
            switch ((AttributeBehavior)definition.Behavior)
            {
                case AttributeBehavior.Resource:
                    return UI_PanelAttribute.EcFilter.资源;
                case AttributeBehavior.Numeric:
                    return UI_PanelAttribute.EcFilter.数值;
                case AttributeBehavior.Marker:
                    return UI_PanelAttribute.EcFilter.标记;
                default:
                    return UI_PanelAttribute.EcFilter.全部;
            }
        }

        static string BehaviorLabel(AttributeDefinitionConfig definition)
        {
            switch ((AttributeBehavior)definition.Behavior)
            {
                case AttributeBehavior.Resource:
                    return "资源";
                case AttributeBehavior.Numeric:
                    return "数值";
                case AttributeBehavior.Marker:
                    return "标记";
                default:
                    return "未设置";
            }
        }

        void RenderOwn(int index, GObject item)
        {
            var definition = _ownShown[index];
            var id = new AttributeId(definition.Id);
            var row = (UI_ItemAttr)item;
            row.title = ProjectSession.AttributeName(definition.Id, id.LocalId);
            row.m_type.text = BehaviorLabel(definition);
            row.m_meta.text = "属性 ID " + id.LocalId + " · " + ProjectSession.Archive.Name;
            row.m_cTone.selectedIndex = (int)UI_ItemAttr.EcTone.本源;
        }

        void RenderOther(int index, GObject item)
        {
            var shown = _otherShown[index];
            var id = new AttributeId(shown.Definition.Id);
            var row = (UI_ItemAttr)item;
            row.title = ProjectSession.AttributeName(shown.Definition.Id, id.LocalId);
            row.m_type.text = BehaviorLabel(shown.Definition);
            row.m_meta.text = "属性 ID " + id.LocalId + " · " + shown.Source.Title;
            row.m_cTone.selectedIndex = (int)UI_ItemAttr.EcTone.只读;
        }

        void OnClickOwn(EventContext context)
        {
            if (_suppress)
            {
                return;
            }

            SelectOwn(view.m_attrs.selectedIndex);
        }

        void OnClickOther(EventContext context)
        {
            if (_suppress)
            {
                return;
            }

            SelectOther(view.m_others.selectedIndex);
        }

        void SelectOwn(int index)
        {
            if (index < 0 || index >= _ownShown.Count)
            {
                ClearSelection();
                return;
            }

            _selected = _ownShown[index];
            _selectedOwned = true;
            if (view.m_others.selectedIndex != -1)
            {
                _suppress = true;
                view.m_others.selectedIndex = -1;
                _suppress = false;
            }

            BindInspector();
        }

        void SelectOther(int index)
        {
            if (index < 0 || index >= _otherShown.Count)
            {
                ClearSelection();
                return;
            }

            _selected = _otherShown[index].Definition;
            _selectedOwned = false;
            if (view.m_attrs.selectedIndex != -1)
            {
                _suppress = true;
                view.m_attrs.selectedIndex = -1;
                _suppress = false;
            }

            BindInspector();
        }

        void ClearSelection()
        {
            _selected = null;
            _selectedOwned = false;
            view.m_inspectorMod.visible = false;
        }

        void BindInspector()
        {
            var definition = _selected;
            var id = new AttributeId(definition.Id);
            var sourceName = _selectedOwned ? ProjectSession.Archive.Name : SourceTitle(definition.Id);
            view.m_inspectorMod.visible = true;
            view.m_mTitle.text = ProjectSession.AttributeName(definition.Id, id.LocalId);
            view.m_mMeta.text = sourceName + "    " + (_selectedOwned ? "可编辑" : "只读");
            _suppress = true;
            view.m_mNameInput.text = ProjectSession.Archive.AttributeNames.TryGetValue(definition.Id, out var name) ? name : "";
            view.m_mIdInput.text = id.LocalId.ToString();
            view.m_mIdHint.text = "完整标识是 " + id + "。作者身份在创建项目时确定，这里只改属性 ID。";
            var behavior = (AttributeBehavior)definition.Behavior;
            var page = behavior == AttributeBehavior.Resource || behavior == AttributeBehavior.Numeric || behavior == AttributeBehavior.Marker
                ? (int)behavior - 1
                : (int)UI_PanelAttribute.EcType.数值;
            view.m_cType.selectedIndex = page;
            view.m_mScale.selected = definition.ScaleCurrentWithMax;
            view.m_mNonNeg.selected = definition.NonNegative;
            SetEditable(_selectedOwned);
            _suppress = false;
        }

        string SourceTitle(ulong id)
        {
            for (int i = 0; i < _otherShown.Count; i++)
            {
                if (_otherShown[i].Definition.Id == id)
                {
                    return _otherShown[i].Source.Title;
                }
            }

            return "";
        }

        void SetEditable(bool editable)
        {
            view.m_mNameInput.touchable = editable;
            view.m_mIdInput.touchable = editable;
            view.m_mNameInput.grayed = !editable;
            view.m_mIdInput.grayed = !editable;
            view.m_mTypeResource.touchable = editable;
            view.m_mTypeNumeric.touchable = editable;
            view.m_mTypeMarker.touchable = editable;
            view.m_mTypeResource.grayed = !editable;
            view.m_mTypeNumeric.grayed = !editable;
            view.m_mTypeMarker.grayed = !editable;
            view.m_mScale.touchable = editable;
            view.m_mNonNeg.touchable = editable;
            view.m_mDelete.visible = editable;
        }

        void CommitName(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            ProjectSession.SetAttributeName(_selected.Id, view.m_mNameInput.text);
            var id = new AttributeId(_selected.Id);
            view.m_mTitle.text = ProjectSession.AttributeName(_selected.Id, id.LocalId);
            RefreshLists();
        }

        void CommitId(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            if (!uint.TryParse(view.m_mIdInput.text, out var localId))
            {
                view.m_mIdInput.text = new AttributeId(_selected.Id).LocalId.ToString();
                return;
            }

            if (!ProjectSession.TryChangeLocalId(_selected, localId, out var error))
            {
                view.m_mIdHint.text = error;
                view.m_mIdInput.text = new AttributeId(_selected.Id).LocalId.ToString();
                return;
            }

            BindInspector();
            RefreshLists();
        }

        void OnToggleOthers(EventContext context)
        {
            view.m_cOthers.selectedIndex = view.m_toggleOthers.selected
                ? (int)UI_PanelAttribute.EcOthers.显示
                : (int)UI_PanelAttribute.EcOthers.隐藏;
        }

        void OnTypeChanged(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            var behavior = (AttributeBehavior)(view.m_cType.selectedIndex + 1);
            var kind = (AttributeValueKind)_selected.ValueKind;
            var numeric = kind == AttributeValueKind.Int32 || kind == AttributeValueKind.Int64 || kind == AttributeValueKind.Fixed64;
            switch (behavior)
            {
                case AttributeBehavior.Resource:
                    if (!numeric)
                    {
                        _selected.ValueKind = (int)AttributeValueKind.Fixed64;
                    }

                    _selected.NonNegative = true;
                    break;
                case AttributeBehavior.Numeric:
                    if (!numeric)
                    {
                        _selected.ValueKind = (int)AttributeValueKind.Fixed64;
                    }

                    _selected.ScaleCurrentWithMax = false;
                    break;
                case AttributeBehavior.Marker:
                    if (!numeric && kind != AttributeValueKind.Bool)
                    {
                        _selected.ValueKind = (int)AttributeValueKind.Bool;
                    }

                    _selected.ScaleCurrentWithMax = false;
                    if ((AttributeValueKind)_selected.ValueKind == AttributeValueKind.Bool)
                    {
                        _selected.NonNegative = false;
                    }

                    break;
            }

            _selected.Behavior = (int)behavior;
            ProjectSession.Save();
            _suppress = true;
            view.m_mScale.selected = _selected.ScaleCurrentWithMax;
            view.m_mNonNeg.selected = _selected.NonNegative;
            _suppress = false;
            RefreshLists();
        }

        void OnScaleChanged(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            if ((AttributeBehavior)_selected.Behavior != AttributeBehavior.Resource)
            {
                return;
            }

            _selected.ScaleCurrentWithMax = view.m_mScale.selected;
            _selected.NonNegative = true;
            ProjectSession.Save();
        }

        void OnNonNegativeChanged(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            if ((AttributeBehavior)_selected.Behavior != AttributeBehavior.Numeric)
            {
                return;
            }

            _selected.NonNegative = view.m_mNonNeg.selected;
            _selected.ScaleCurrentWithMax = false;
            ProjectSession.Save();
        }

        void DeleteSelected()
        {
            if (!_selectedOwned || _selected == null)
            {
                return;
            }

            ProjectSession.DeleteAttribute(_selected);
            _selected = null;
            RefreshLists();
        }

        static int IndexOf(List<AttributeDefinitionConfig> list, ulong id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
