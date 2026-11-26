using System;
using System.Collections.Generic;
using FairyGUI;
using Google.Protobuf.Collections;
using PackageAtrribute;
using RebornFrame.Spell;
using SpellEditor;

namespace UILogic
{
    public class ComTagEditor
    {
        struct ShownOther
        {
            public TagDefinitionConfig Definition;
            public LoadedSource Source;
        }

        struct ParentChoice
        {
            public ulong Id;
            public string Title;
        }

        readonly UI_PanelTag view;
        readonly List<TagDefinitionConfig> _ownShown = new List<TagDefinitionConfig>();
        readonly List<ShownOther> _otherShown = new List<ShownOther>();
        readonly List<ParentChoice> _parents = new List<ParentChoice>();
        TagDefinitionConfig _selected;
        bool _selectedOwned;
        bool _suppress;

        public ComTagEditor(UI_PanelTag view)
        {
            this.view = view;
            view.m_search.text = "";
            view.m_search.onChanged.Add(RefreshLists);
            view.m_cFilter.onChanged.Add(RefreshLists);
            view.m_toggleOthers.onChanged.Add(OnToggleOthers);
            view.m_tags.itemRenderer = RenderOwn;
            view.m_others.itemRenderer = RenderOther;
            view.m_tags.onClickItem.Add(OnClickOwn);
            view.m_others.onClickItem.Add(OnClickOther);
            view.m_parents.itemRenderer = RenderParent;
            view.m_mNameInput.onFocusOut.Add(CommitName);
            view.m_mIdInput.onFocusOut.Add(CommitId);
            view.m_mDelete.onClick.Add(DeleteSelected);
            view.m_btnAdd.onClick.Add(AddTag);
            RefreshLists();
            ClearSelection();
        }

        void RefreshLists()
        {
            var keep = _selected == null ? 0UL : _selected.Id;
            var keepOwned = _selectedOwned;
            var hasSelection = _selected != null;
            _ownShown.Clear();
            _otherShown.Clear();
            Collect(ProjectSession.OwnModule, null);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                Collect(ProjectSession.Sources[i].Module, ProjectSession.Sources[i]);
            }

            _suppress = true;
            view.m_tags.numItems = _ownShown.Count;
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
                    SelectOwn(index);
                    return;
                }
            }
            else
            {
                for (var i = 0; i < _otherShown.Count; i++)
                {
                    if (_otherShown[i].Definition.Id != keep)
                    {
                        continue;
                    }

                    SelectOther(i);
                    return;
                }
            }

            ClearSelection();
        }

        void Collect(SpellModuleData module, LoadedSource source)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var definition = module.Tags[i];
                if (definition == null || !Match(definition))
                {
                    continue;
                }

                if (source == null)
                {
                    _ownShown.Add(definition);
                }
                else
                {
                    _otherShown.Add(new ShownOther { Definition = definition, Source = source });
                }
            }
        }

        bool Match(TagDefinitionConfig definition)
        {
            var filter = (UI_PanelTag.EcFilter)view.m_cFilter.selectedIndex;
            var rooted = definition.ParentId == 0;
            if (filter == UI_PanelTag.EcFilter.根节点 && !rooted)
            {
                return false;
            }

            if (filter == UI_PanelTag.EcFilter.子标签 && rooted)
            {
                return false;
            }

            var query = view.m_search.text;
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            var id = new TagId(definition.Id);
            var name = ProjectSession.TagName(definition.Id, id.LocalId);
            var parent = definition.ParentId == 0 ? "根" : ProjectSession.TagName(definition.ParentId, new TagId(definition.ParentId).LocalId);
            return name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                   || id.LocalId.ToString().IndexOf(query, StringComparison.Ordinal) >= 0
                   || id.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                   || parent.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void RenderOwn(int index, GObject item)
        {
            var definition = _ownShown[index];
            var row = (UI_ItemTag)item;
            var id = new TagId(definition.Id);
            row.title = ProjectSession.TagName(definition.Id, id.LocalId);
            row.m_type.text = definition.ParentId == 0 ? "根" : "子";
            row.m_meta.text = "标签 ID " + id.LocalId + " · " + ParentMeta(definition);
            row.m_cTone.selectedIndex = 0;
            row.selected = _selectedOwned && _selected != null && _selected.Id == definition.Id;
        }

        void RenderOther(int index, GObject item)
        {
            var shown = _otherShown[index];
            var row = (UI_ItemTag)item;
            var id = new TagId(shown.Definition.Id);
            row.title = ProjectSession.TagName(shown.Definition.Id, id.LocalId);
            row.m_type.text = shown.Definition.ParentId == 0 ? "根" : "子";
            row.m_meta.text = "标签 ID " + id.LocalId + " · " + ParentMeta(shown.Definition);
            row.m_cTone.selectedIndex = 0;
            row.selected = !_selectedOwned && _selected != null && _selected.Id == shown.Definition.Id;
        }

        void OnClickOwn(EventContext context)
        {
            if (_suppress)
            {
                return;
            }

            SelectOwn(view.m_tags.GetChildIndex((GObject)context.data));
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
                ClearSelection();
                return;
            }

            _selected = _ownShown[index];
            _selectedOwned = true;
            BindInspector();
            view.m_tags.numItems = _ownShown.Count;
            view.m_others.numItems = _otherShown.Count;
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
            BindInspector();
            view.m_tags.numItems = _ownShown.Count;
            view.m_others.numItems = _otherShown.Count;
        }

        void ClearSelection()
        {
            _selected = null;
            _selectedOwned = false;
            view.m_cInspect.selectedIndex = (int)UI_PanelTag.EcInspect.未选中;
            Warn("");
        }

        void BindInspector()
        {
            var definition = _selected;
            var id = new TagId(definition.Id);
            _suppress = true;
            view.m_cInspect.selectedIndex = (int)UI_PanelTag.EcInspect.已选中;
            view.m_cEdit.selectedIndex = _selectedOwned
                ? (int)UI_PanelTag.EcEdit.可编辑
                : (int)UI_PanelTag.EcEdit.只读;
            view.m_mTitle.text = ProjectSession.TagName(definition.Id, id.LocalId);
            view.m_mNameInput.text = view.m_mTitle.text;
            view.m_mIdInput.text = id.LocalId.ToString();
            view.m_mPath.text = PathOf(definition.Id);
            BuildParents(definition);
            view.m_parents.numItems = _parents.Count;
            _suppress = false;
        }

        void BuildParents(TagDefinitionConfig definition)
        {
            _parents.Clear();
            _parents.Add(new ParentChoice { Id = 0, Title = "根节点" });
            AddParentChoices(ProjectSession.OwnModule, definition.Id);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                AddParentChoices(ProjectSession.Sources[i].Module, definition.Id);
            }
        }

        void AddParentChoices(SpellModuleData module, ulong self)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag == null || tag.Id == self || IsUnder(tag.Id, self))
                {
                    continue;
                }

                var id = new TagId(tag.Id);
                _parents.Add(new ParentChoice
                {
                    Id = tag.Id,
                    Title = ProjectSession.TagName(tag.Id, id.LocalId) + "  " + id,
                });
            }
        }

        void RenderParent(int index, GObject item)
        {
            var choice = _parents[index];
            var button = (UI_ButtonRadio)item;
            button.title = choice.Title;
            button.data = choice.Id;
            button.selected = _selected != null && _selected.ParentId == choice.Id;
            button.onChanged.Set(() =>
            {
                if (_suppress || !button.selected)
                {
                    return;
                }

                CommitParent(choice.Id);
            });
        }

        void OnToggleOthers(EventContext context)
        {
            view.m_cOthers.selectedIndex = view.m_toggleOthers.selected
                ? (int)UI_PanelTag.EcOthers.显示
                : (int)UI_PanelTag.EcOthers.隐藏;
        }

        void CommitName(EventContext context)
        {
            if (_suppress || !_selectedOwned || _selected == null)
            {
                return;
            }

            ProjectSession.SetTagName(_selected.Id, view.m_mNameInput.text);
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
                Warn("先填写标签 ID。");
                return;
            }

            var next = TagId.Combine(ProjectSession.AuthorId, localId);
            if (!LibraryTags.CanBeCreatedByConfiguration(next))
            {
                Warn("0 到 99 是库保留编号。");
                return;
            }

            if (next.Value != _selected.Id && IdTaken(next.Value, _selected))
            {
                Warn("已经有这个标签 ID。");
                return;
            }

            if (next.Value != _selected.Id)
            {
                ReplaceId(_selected.Id, next.Value);
                ProjectSession.MoveTagName(_selected.Id, next.Value);
                _selected.Id = next.Value;
                ProjectSession.Save();
            }

            Warn("");
            RefreshLists();
        }

        void CommitParent(ulong parentId)
        {
            if (_suppress || !_selectedOwned || _selected == null || _selected.ParentId == parentId)
            {
                return;
            }

            if (parentId != 0 && !Exists(parentId))
            {
                Warn("这个父标签不存在。");
                BindInspector();
                return;
            }

            if (parentId == _selected.Id || IsUnder(parentId, _selected.Id))
            {
                Warn("父标签不能是自己，也不能是自己的子标签。");
                BindInspector();
                return;
            }

            if (ChainTooDeep(parentId))
            {
                Warn("父链不能超过 " + TagCatalog.MaxHierarchyDepth + " 层。");
                BindInspector();
                return;
            }

            _selected.ParentId = parentId;
            ProjectSession.Save();
            Warn("");
            RefreshLists();
        }

        void AddTag()
        {
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null)
            {
                return;
            }

            var localId = NextLocalId();
            var tag = new TagDefinitionConfig
            {
                Id = TagId.Combine(ProjectSession.AuthorId, localId).Value,
                ParentId = 0,
            };
            ProjectSession.OwnModule.Tags.Add(tag);
            ProjectSession.Save();
            _selected = tag;
            _selectedOwned = true;
            Warn("");
            RefreshLists();
        }

        void DeleteSelected()
        {
            if (!_selectedOwned || _selected == null)
            {
                return;
            }

            if (UsedAsParent(_selected.Id))
            {
                Warn("还有标签挂在它下面。");
                return;
            }

            ProjectSession.OwnModule.Tags.Remove(_selected);
            if (ProjectSession.Archive != null)
            {
                ProjectSession.Archive.TagNames.Remove(_selected.Id);
            }

            ProjectSession.Save();
            _selected = null;
            Warn("");
            RefreshLists();
        }

        static uint NextLocalId()
        {
            var author = ProjectSession.AuthorId;
            var floor = author == 0 ? LibraryTags.ReservedLocalIdCount : 1u;
            var highest = floor - 1;
            NoteHighest(ProjectSession.OwnModule, author, ref highest);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                NoteHighest(ProjectSession.Sources[i].Module, author, ref highest);
            }

            var next = highest + 1;
            return next < floor ? floor : next;
        }

        static void NoteHighest(SpellModuleData module, uint author, ref uint highest)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var id = new TagId(module.Tags[i].Id);
                if (id.AuthorId == author && id.LocalId > highest)
                {
                    highest = id.LocalId;
                }
            }
        }

        static bool IdTaken(ulong id, TagDefinitionConfig except)
        {
            return Contains(ProjectSession.OwnModule, id, except) || SourceContains(id);
        }

        static bool SourceContains(ulong id)
        {
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                if (Contains(ProjectSession.Sources[i].Module, id, null))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Contains(SpellModuleData module, ulong id, TagDefinitionConfig except)
        {
            if (module == null)
            {
                return false;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                if (module.Tags[i] != null && module.Tags[i].Id == id && !ReferenceEquals(module.Tags[i], except))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Exists(ulong id)
        {
            return Contains(ProjectSession.OwnModule, id, null) || SourceContains(id);
        }

        static bool UsedAsParent(ulong id)
        {
            if (ChildOf(ProjectSession.OwnModule, id))
            {
                return true;
            }

            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                if (ChildOf(ProjectSession.Sources[i].Module, id))
                {
                    return true;
                }
            }

            return false;
        }

        static bool ChildOf(SpellModuleData module, ulong id)
        {
            if (module == null)
            {
                return false;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                if (module.Tags[i] != null && module.Tags[i].ParentId == id)
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsUnder(ulong id, ulong ancestor)
        {
            var current = id;
            for (var depth = 0; depth <= TagCatalog.MaxHierarchyDepth; depth++)
            {
                if (!TryParent(current, out var parent) || parent == 0)
                {
                    return false;
                }

                if (parent == ancestor)
                {
                    return true;
                }

                current = parent;
            }

            return false;
        }

        static bool ChainTooDeep(ulong parentId)
        {
            var depth = 1;
            var current = parentId;
            for (var i = 0; i < TagCatalog.MaxHierarchyDepth; i++)
            {
                if (current == 0)
                {
                    return false;
                }

                if (!TryParent(current, out var parent))
                {
                    return true;
                }

                current = parent;
                depth++;
                if (depth > TagCatalog.MaxHierarchyDepth)
                {
                    return true;
                }
            }

            return current != 0;
        }

        static bool TryParent(ulong id, out ulong parent)
        {
            parent = 0;
            var tag = Find(id);
            if (tag == null)
            {
                return false;
            }

            parent = tag.ParentId;
            return true;
        }

        static TagDefinitionConfig Find(ulong id)
        {
            var own = FindIn(ProjectSession.OwnModule, id);
            if (own != null)
            {
                return own;
            }

            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var tag = FindIn(ProjectSession.Sources[i].Module, id);
                if (tag != null)
                {
                    return tag;
                }
            }

            return null;
        }

        static TagDefinitionConfig FindIn(SpellModuleData module, ulong id)
        {
            if (module == null)
            {
                return null;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                if (module.Tags[i] != null && module.Tags[i].Id == id)
                {
                    return module.Tags[i];
                }
            }

            return null;
        }

        static void ReplaceId(ulong from, ulong to)
        {
            var module = ProjectSession.OwnModule;
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                if (module.Tags[i] != null && module.Tags[i].ParentId == from)
                {
                    module.Tags[i].ParentId = to;
                }
            }

            for (var i = 0; i < module.Buffs.Count; i++)
            {
                var buff = module.Buffs[i];
                if (buff == null)
                {
                    continue;
                }

                Replace(buff.RequiredTags, from, to);
                Replace(buff.ImmunityTags, from, to);
                Replace(buff.GrantedTags, from, to);
            }

            for (var i = 0; i < module.Spells.Count; i++)
            {
                var spell = module.Spells[i];
                if (spell != null)
                {
                    Replace(spell.AdmissionTags, from, to);
                }
            }
        }

        static void Replace(RepeatedField<ulong> ids, ulong from, ulong to)
        {
            if (ids == null)
            {
                return;
            }

            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == from)
                {
                    ids[i] = to;
                }
            }
        }

        string ParentMeta(TagDefinitionConfig definition)
        {
            if (definition.ParentId == 0)
            {
                return "根";
            }

            return "父 " + ProjectSession.TagName(definition.ParentId, new TagId(definition.ParentId).LocalId);
        }

        string PathOf(ulong id)
        {
            var names = new List<string>();
            var current = id;
            for (var depth = 0; depth <= TagCatalog.MaxHierarchyDepth && current != 0; depth++)
            {
                var parsed = new TagId(current);
                names.Add(ProjectSession.TagName(current, parsed.LocalId));
                if (!TryParent(current, out var parent))
                {
                    break;
                }

                current = parent;
            }

            names.Reverse();
            return names.Count == 0 ? "根" : string.Join(" / ", names);
        }

        void Warn(string text)
        {
            view.m_mWarn.text = text ?? "";
            view.m_cMessage.selectedIndex = string.IsNullOrEmpty(text)
                ? (int)UI_PanelTag.EcMessage.空
                : (int)UI_PanelTag.EcMessage.警告;
        }

        static int IndexOf(List<TagDefinitionConfig> list, ulong id)
        {
            for (var i = 0; i < list.Count; i++)
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
