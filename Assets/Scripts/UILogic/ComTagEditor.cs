using System;
using System.Collections.Generic;
using FairyGUI;
using PackageAtrribute;
using RebornFrame.Spell;
using GameContentEditor;

namespace UILogic
{
    public class ComTagEditor
    {
        struct TagRow
        {
            public ulong Id;
            public bool Draft;
            public bool Owned;
            public int Depth;
            public bool HasChildren;
            public bool Expanded;
            public string Title;
        }

        const float Margin = 16f;
        const float Top = 112f;
        const float PageWidth = 1600f;
        const float PageHeight = 900f;

        readonly UI_PanelTag view;
        readonly List<TagRow> _own = new List<TagRow>();
        readonly List<TagRow> _other = new List<TagRow>();
        readonly HashSet<ulong> _collapsed = new HashSet<ulong>();
        ulong _selectedId;
        ulong _otherHighlight;
        bool _draft;
        ulong _draftParent;
        ulong _editId;
        string _draftText = "";
        bool _focusDraft;
        bool _scrollOwn;
        bool _scrollOther;
        bool _scrollDraft;
        bool _suppress;
        GTextInput _borrowed;

        public ComTagEditor(UI_PanelTag view)
        {
            this.view = view;
            view.m_search.text = "";
            view.m_search.onChanged.Add(RefreshLists);
            view.m_toggleOthers.onChanged.Add(OnToggleOthers);
            view.m_tags.itemRenderer = RenderOwn;
            view.m_tags.onClickItem.Add(OnClickOwn);
            view.m_tags.autoResizeItem = true;
            if (view.m_others != null)
            {
                view.m_others.itemRenderer = RenderOther;
                view.m_others.onClickItem.Add(OnClickOther);
                view.m_others.autoResizeItem = true;
            }

            view.m_btnAdd.onClick.Add(AddTag);
            Layout();
            RefreshLists();
        }

        void OnToggleOthers(EventContext context)
        {
            Layout();
            RefreshLists();
        }

        void Layout()
        {
            var split = view.m_toggleOthers.selected;
            var others = view.GetController("cOthers");
            if (others != null)
            {
                others.selectedIndex = split ? (int)UI_PanelTag.EcOthers.显示 : (int)UI_PanelTag.EcOthers.隐藏;
            }

            var edit = view.GetController("cEdit");
            if (edit != null)
            {
                edit.selectedIndex = 0;
            }

            var inspect = view.GetController("cInspect");
            if (inspect != null)
            {
                inspect.selectedIndex = 1;
            }

            HideChrome();
            ParkInput();
            var width = PageWidth - Margin * 2f;
            var height = PageHeight - Top - Margin;
            var leftWidth = split ? (width - Margin) / 2f : width;
            Place(view.m_leftBg, Margin, Top, leftWidth, height);
            Place(view.m_tags, Margin, Top, leftWidth, height);
            Place(view.m_searchBg, Margin, 64f, width, 32f);
            Place(view.m_search, Margin + 12f, 64f, width - 24f, 32f);
            view.m_toggleOthers.SetXY(324f, 20f);
            var rightX = Margin + leftWidth + Margin;
            var rightBg = view.GetChild("rightBg");
            var showRight = split && view.m_others != null;
            if (rightBg != null)
            {
                rightBg.visible = showRight;
                if (showRight)
                {
                    Place(rightBg, rightX, Top, leftWidth, height);
                }
            }

            if (view.m_others != null)
            {
                view.m_others.visible = showRight;
                if (showRight)
                {
                    Place(view.m_others, rightX, Top, leftWidth, height);
                }
            }

            var group = view.GetChild("otherSources");
            if (group != null && showRight)
            {
                group.visible = true;
                view.m_others.visible = true;
            }
        }

        static void Place(GObject obj, float x, float y, float width, float height)
        {
            if (obj == null)
            {
                return;
            }

            obj.visible = true;
            obj.SetXY(x, y);
            obj.SetSize(width, height);
        }

        void HideChrome()
        {
            string[] names =
            {
                "filters", "filterAll", "filterRoot", "filterChild", "modSection", "otherSection",
                "emptyHint", "cardIdentity", "mTitle", "mDelete", "mNameLabel", "mNameBg",
                "mIdLabel", "mIdBg", "mIdInput", "cardParent", "mParentLabel", "mPath", "parents",
                "sourceLine", "nameHint", "inspector", "mWarn",
            };
            for (var i = 0; i < names.Length; i++)
            {
                var child = view.GetChild(names[i]);
                if (child != null)
                {
                    child.visible = false;
                }
            }
        }

        void RefreshLists()
        {
            ParkInput();
            if (_selectedId != 0 && FindIn(ProjectSession.OwnModule, _selectedId) == null)
            {
                _selectedId = 0;
            }

            Build(true, _own);
            Build(false, _other);
            _suppress = true;
            view.m_tags.numItems = _own.Count;
            if (view.m_others != null)
            {
                view.m_others.numItems = _other.Count;
            }

            _suppress = false;
            Scroll();
        }

        void Scroll()
        {
            if (_scrollDraft)
            {
                _scrollDraft = false;
                var index = IndexOfDraft(_own);
                if (index >= 0)
                {
                    view.m_tags.ScrollToView(index);
                }
            }

            if (_scrollOwn)
            {
                _scrollOwn = false;
                var index = IndexOf(_own, _selectedId);
                if (index >= 0)
                {
                    view.m_tags.ScrollToView(index);
                }
            }

            if (_scrollOther && view.m_others != null)
            {
                _scrollOther = false;
                var index = IndexOf(_other, _otherHighlight);
                if (index >= 0)
                {
                    view.m_others.ScrollToView(index);
                }
            }
        }

        void Build(bool ownedSide, List<TagRow> into)
        {
            into.Clear();
            var visible = new HashSet<ulong>();
            EachSide(ownedSide, tag => visible.Add(tag.Id));
            var roots = new List<ulong>();
            EachSide(ownedSide, tag =>
            {
                if (tag.ParentId == 0 || !visible.Contains(tag.ParentId))
                {
                    roots.Add(tag.Id);
                }
            });
            roots.Sort(CompareTag);
            var query = (view.m_search.text ?? "").Trim();
            if (ownedSide && _draft && _editId == 0 && (_draftParent == 0 || !visible.Contains(_draftParent)))
            {
                into.Add(DraftRow(0));
            }

            for (var i = 0; i < roots.Count; i++)
            {
                Walk(roots[i], 0, query, ownedSide, visible, into);
            }
        }

        void Walk(ulong id, int depth, string query, bool ownedSide, HashSet<ulong> visible, List<TagRow> into)
        {
            if (!string.IsNullOrEmpty(query) && !BranchHits(id, query, visible) && !KeepsDraft(id))
            {
                return;
            }

            var kids = Children(id, visible);
            var draftHere = ownedSide && _draft && _editId == 0 && id == _draftParent;
            var editing = ownedSide && _editId != 0 && id == _editId;
            var hasChildren = kids.Count > 0 || draftHere;
            var expanded = hasChildren && (draftHere || !_collapsed.Contains(id) || DescendantHits(id, query, visible));
            into.Add(new TagRow
            {
                Id = id,
                Draft = editing,
                Owned = FindIn(ProjectSession.OwnModule, id) != null,
                Depth = depth,
                HasChildren = hasChildren,
                Expanded = expanded,
                Title = Leaf(id),
            });
            if (!expanded)
            {
                return;
            }

            if (draftHere)
            {
                into.Add(DraftRow(depth + 1));
            }

            for (var i = 0; i < kids.Count; i++)
            {
                Walk(kids[i], depth + 1, query, ownedSide, visible, into);
            }
        }

        bool KeepsDraft(ulong id)
        {
            if (!_draft)
            {
                return false;
            }

            var anchor = _editId != 0 ? _editId : _draftParent;
            return anchor == id || IsUnder(anchor, id);
        }

        static TagRow DraftRow(int depth)
        {
            return new TagRow
            {
                Draft = true,
                Owned = true,
                Depth = depth,
                Title = "",
            };
        }

        bool BranchHits(ulong id, string query, HashSet<ulong> visible)
        {
            if (Hits(id, query))
            {
                return true;
            }

            return DescendantHits(id, query, visible);
        }

        bool DescendantHits(ulong id, string query, HashSet<ulong> visible)
        {
            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            var kids = Children(id, visible);
            for (var i = 0; i < kids.Count; i++)
            {
                if (BranchHits(kids[i], query, visible))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Hits(ulong id, string query)
        {
            return Path(id).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static List<ulong> Children(ulong parent, HashSet<ulong> visible)
        {
            var into = new List<ulong>();
            foreach (var id in visible)
            {
                var tag = Find(id);
                if (tag != null && tag.ParentId == parent)
                {
                    into.Add(id);
                }
            }

            into.Sort(CompareTag);
            return into;
        }

        void EachSide(bool ownedSide, Action<TagDefinitionConfig> visit)
        {
            if (ownedSide)
            {
                VisitModule(ProjectSession.OwnModule, visit);
                return;
            }

            if (!view.m_toggleOthers.selected)
            {
                return;
            }

            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                VisitModule(ProjectSession.Sources[i].Module, visit);
            }
        }

        static void VisitModule(SpellModuleData module, Action<TagDefinitionConfig> visit)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag != null && tag.Id != 0)
                {
                    visit(tag);
                }
            }
        }

        static int CompareTag(ulong left, ulong right)
        {
            var order = string.Compare(Leaf(left), Leaf(right), StringComparison.OrdinalIgnoreCase);
            if (order != 0)
            {
                return order;
            }

            return left.CompareTo(right);
        }

        void RenderOwn(int index, GObject item)
        {
            BindRow(_own[index], item, view.m_tags, true);
        }

        void RenderOther(int index, GObject item)
        {
            BindRow(_other[index], item, view.m_others, false);
        }

        void BindRow(TagRow row, GObject item, GList list, bool ownedSide)
        {
            var button = (UI_ItemTag)item;
            button.SetSize(list.width, button.height);
            var id = row.Id;
            var legacyType = button.GetChild("type");
            if (legacyType != null)
            {
                legacyType.visible = false;
            }

            var legacyMeta = button.GetChild("meta");
            if (legacyMeta != null)
            {
                legacyMeta.visible = false;
            }

            var indent = 8f + row.Depth * 16f;
            var titleX = indent + 22f;
            var mode = button.GetController("cInput");
            if (mode != null)
            {
                mode.selectedIndex = row.Draft ? 1 : 0;
            }

            var fold = button.GetChild("fold") as GButton;
            if (fold != null)
            {
                fold.SetXY(indent, 4f);
                fold.visible = !row.Draft && row.HasChildren;
                fold.touchable = fold.visible;
                fold.title = row.Expanded ? "▾" : "▸";
                fold.onClick.Set(context =>
                {
                    context.StopPropagation();
                    if (ownedSide)
                    {
                        _selectedId = id;
                    }
                    else
                    {
                        _otherHighlight = id;
                    }

                    Toggle(id);
                });
            }

            var title = button.GetChild("title");
            if (title != null)
            {
                title.visible = !row.Draft;
                title.SetXY(titleX, title.y);
                title.width = Math.Max(40f, button.width - titleX - 8f);
            }

            button.title = row.Draft ? "" : row.Title;
            button.m_cTone.selectedIndex = row.Owned ? 0 : 1;
            button.selected = row.Draft || (ownedSide ? id == _selectedId && id != 0 : id == _otherHighlight && id != 0);
            button.data = id;
            button.draggable = ownedSide && !row.Draft;
            button.onDragStart.Clear();
            button.onDrop.Clear();
            button.onRightClick.Clear();
            if (row.Draft)
            {
                BindDraft(button, titleX);
                return;
            }

            if (!ownedSide)
            {
                return;
            }

            button.onDragStart.Set(context => OnDragStart(context, id));
            button.onDrop.Set(context => OnDrop(context, id));
            button.onRightClick.Set(context =>
            {
                context.StopPropagation();
                var tag = FindIn(ProjectSession.OwnModule, id);
                if (tag == null)
                {
                    return;
                }

                _selectedId = id;
                _suppress = true;
                view.m_tags.numItems = _own.Count;
                _suppress = false;
                if (!PanelFolderMenu.Show(new PanelFolderMenu.Request
                {
                    Create = true,
                    Remove = true,
                    CreateTitle = "新增",
                    RemoveTitle = "删除",
                    OnCreate = () => AddUnder(id),
                    OnRemove = () => AskDelete(tag),
                }))
                {
                    Warn("请重新发布 PackageResource 后再管理标签。");
                }
            });
        }

        void BindDraft(GComponent row, float titleX)
        {
            var input = row.GetChild("input") as GTextInput;
            var inputBg = row.GetChild("inputBg");
            if (input == null)
            {
                input = BorrowInput(row, titleX);
            }
            else
            {
                input.visible = true;
                if (inputBg != null)
                {
                    inputBg.visible = true;
                    inputBg.SetXY(titleX, 4f);
                    inputBg.SetSize(Math.Max(40f, row.width - titleX - 8f), 28f);
                }

                input.SetXY(titleX + 8f, 4f);
                input.SetSize(Math.Max(40f, row.width - titleX - 16f), 28f);
            }

            if (input == null)
            {
                return;
            }

            input.singleLine = true;
            input.promptText = "名称，/ 表示下一层";
            input.touchable = true;
            input.grayed = false;
            input.onChanged.Set(() => _draftText = input.text);
            input.onFocusOut.Set(() => CommitDraft(input));
            input.onSubmit.Set(() => CommitDraft(input));
            if (!_focusDraft)
            {
                return;
            }

            _focusDraft = false;
            var initial = _editId != 0 ? Leaf(_editId) : "";
            input.text = initial;
            _draftText = initial;
            var field = input;
            Timers.inst.CallLater(param =>
            {
                if (field == null || field.isDisposed)
                {
                    return;
                }

                field.RequestFocus();
                if (initial.Length > 0)
                {
                    field.SetSelection(0, initial.Length);
                }
            });
        }

        GTextInput BorrowInput(GComponent row, float titleX)
        {
            if (_borrowed == null || _borrowed.isDisposed)
            {
                _borrowed = view.GetChild("mNameInput") as GTextInput;
            }

            if (_borrowed == null)
            {
                return null;
            }

            row.AddChild(_borrowed);
            _borrowed.visible = true;
            _borrowed.SetXY(titleX, 4f);
            _borrowed.SetSize(Math.Max(40f, row.width - titleX - 8f), row.height - 8f);
            return _borrowed;
        }

        void ParkInput()
        {
            if (_borrowed == null || _borrowed.isDisposed)
            {
                _borrowed = view.GetChild("mNameInput") as GTextInput;
            }

            if (_borrowed == null || _borrowed.isDisposed)
            {
                return;
            }

            if (_borrowed.parent != view)
            {
                view.AddChild(_borrowed);
            }

            _borrowed.visible = false;
        }

        void OnClickOwn(EventContext context)
        {
            if (_suppress || (context.inputEvent != null && context.inputEvent.button == 1))
            {
                return;
            }

            var index = view.m_tags.GetChildIndex((GObject)context.data);
            if (index < 0 || index >= _own.Count || _own[index].Draft)
            {
                return;
            }

            var id = _own[index].Id;
            if (context.inputEvent != null && context.inputEvent.isDoubleClick)
            {
                BeginEdit(id);
                return;
            }

            _selectedId = id;
            _suppress = true;
            view.m_tags.numItems = _own.Count;
            _suppress = false;
        }

        void OnClickOther(EventContext context)
        {
            if (_suppress || (context.inputEvent != null && context.inputEvent.button == 1))
            {
                return;
            }

            var index = view.m_others.GetChildIndex((GObject)context.data);
            if (index < 0 || index >= _other.Count)
            {
                return;
            }

            _otherHighlight = _other[index].Id;
            _suppress = true;
            view.m_others.numItems = _other.Count;
            _suppress = false;
        }

        void Toggle(ulong id)
        {
            if (!_collapsed.Add(id))
            {
                _collapsed.Remove(id);
            }

            RefreshLists();
        }

        void OnDragStart(EventContext context, ulong id)
        {
            context.PreventDefault();
            var touch = context.inputEvent == null ? -1 : context.inputEvent.touchId;
            DragDropManager.inst.StartDrag((GObject)context.sender, "", id, touch);
        }

        void OnDrop(EventContext context, ulong parentId)
        {
            if (!(context.data is ulong dragged) || dragged == 0 || dragged == parentId)
            {
                return;
            }

            var tag = FindIn(ProjectSession.OwnModule, dragged);
            if (tag == null || tag.ParentId == parentId)
            {
                return;
            }

            if (parentId != 0 && !Exists(parentId))
            {
                Warn("这个父标签不存在。");
                return;
            }

            if (parentId == tag.Id || IsUnder(parentId, tag.Id))
            {
                Warn("不能拖到自己或自己的子层级下。");
                return;
            }

            if (TooDeep(parentId, tag.Id))
            {
                Warn("父链不能超过 " + TagCatalog.MaxHierarchyDepth + " 层。");
                return;
            }

            tag.ParentId = parentId;
            ProjectSession.Save();
            if (parentId != 0)
            {
                _collapsed.Remove(parentId);
            }

            _selectedId = tag.Id;
            Warn("");
            _scrollOwn = true;
            RefreshLists();
        }

        public void SelectId(ulong id)
        {
            Reveal(id);
            if (FindIn(ProjectSession.OwnModule, id) != null)
            {
                _selectedId = id;
                _scrollOwn = true;
            }
            else if (Find(id) != null)
            {
                _otherHighlight = id;
                _scrollOther = true;
                if (!view.m_toggleOthers.selected)
                {
                    view.m_toggleOthers.selected = true;
                }
            }

            Layout();
            RefreshLists();
        }

        void Reveal(ulong id)
        {
            var current = id;
            for (var depth = 0; depth <= TagCatalog.MaxHierarchyDepth && current != 0; depth++)
            {
                _collapsed.Remove(current);
                if (!TryParent(current, out var parent))
                {
                    break;
                }

                current = parent;
            }
        }

        void AddTag()
        {
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null || _draft)
            {
                return;
            }

            var parent = _selectedId != 0 && FindIn(ProjectSession.OwnModule, _selectedId) != null
                ? _selectedId
                : 0UL;
            AddUnder(parent);
        }

        void AddUnder(ulong parent)
        {
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null)
            {
                return;
            }

            _draft = false;
            _editId = 0;
            _selectedId = parent;
            if (parent != 0)
            {
                _collapsed.Remove(parent);
            }

            _draft = true;
            _draftParent = parent;
            _editId = 0;
            _draftText = "";
            _focusDraft = true;
            _scrollDraft = true;
            Warn("");
            RefreshLists();
        }

        void CommitDraft(GTextInput input)
        {
            if (_suppress || !_draft)
            {
                return;
            }

            var text = input == null || input.isDisposed ? _draftText : input.text;
            var parent = _draftParent;
            var editing = _editId;
            _draft = false;
            _editId = 0;
            _draftText = "";
            _suppress = true;
            var leaf = editing != 0 ? ApplyEdit(editing, text) : CreateUnder(parent, text);
            _suppress = false;
            if (leaf != 0)
            {
                _selectedId = leaf;
                Reveal(leaf);
                _scrollOwn = true;
            }

            ParkInput();
            RefreshLists();
        }

        void BeginEdit(ulong id)
        {
            if (_draft || FindIn(ProjectSession.OwnModule, id) == null)
            {
                return;
            }

            _selectedId = id;
            _draft = true;
            _editId = id;
            _draftText = Leaf(id);
            _focusDraft = true;
            _scrollDraft = true;
            Warn("");
            RefreshLists();
        }

        ulong ApplyEdit(ulong id, string text)
        {
            var tag = FindIn(ProjectSession.OwnModule, id);
            if (tag == null)
            {
                return 0;
            }

            var parts = SplitPath(text);
            if (parts.Count == 0)
            {
                return id;
            }

            if (!string.Equals(Leaf(id), parts[0], StringComparison.Ordinal))
            {
                ProjectSession.SetTagName(id, parts[0]);
            }

            if (parts.Count == 1)
            {
                Warn("");
                return id;
            }

            var rest = string.Join("/", parts.GetRange(1, parts.Count - 1).ToArray());
            var leaf = CreateUnder(id, rest);
            return leaf != 0 ? leaf : id;
        }

        ulong CreateUnder(ulong parent, string text)
        {
            var parts = SplitPath(text);
            if (parts.Count == 0)
            {
                return 0;
            }

            var current = parent;
            ulong leaf = 0;
            for (var i = 0; i < parts.Count; i++)
            {
                var found = FindOwnChild(current, parts[i]);
                if (found == null)
                {
                    var depth = current == 0 ? 1 : ChainLength(current) + 1;
                    if (depth > TagCatalog.MaxHierarchyDepth)
                    {
                        Warn("父链不能超过 " + TagCatalog.MaxHierarchyDepth + " 层。");
                        return leaf;
                    }

                    found = CreateOwned(current, parts[i]);
                    if (found == null)
                    {
                        Warn("没能创建标签。");
                        return leaf;
                    }
                }

                current = found.Id;
                leaf = found.Id;
                _collapsed.Remove(found.Id);
            }

            Warn("");
            return leaf;
        }

        public void DeleteCurrent()
        {
            if (_selectedId == 0 || _draft)
                return;
            AskDelete(FindIn(ProjectSession.OwnModule, _selectedId));
        }

        void AskDelete(TagDefinitionConfig tag)
        {
            if (tag == null || FindIn(ProjectSession.OwnModule, tag.Id) == null)
            {
                return;
            }

            var doomed = new List<ulong>();
            CollectOwnedSubtree(tag.Id, doomed);
            if (OtherDepends(doomed))
            {
                _selectedId = tag.Id;
                Warn("其他数据源的标签还挂在这个层级下。");
                RefreshLists();
                return;
            }

            var leaf = Leaf(tag.Id);
            var hint = doomed.Count <= 1
                ? "会删掉「" + leaf + "」。"
                : "会删掉「" + leaf + "」和它下面的 " + (doomed.Count - 1) + " 个标签。";
            var id = tag.Id;
            if (!PanelPickConfirm.Show(new PanelPickConfirm.Request
            {
                Title = "删除层级",
                Hint = hint,
                Confirm = "删除",
                Commit = () => DeleteTree(id),
            }))
            {
                Warn("请重新发布 PackageResource 后再删除层级。");
            }
        }

        string DeleteTree(ulong id)
        {
            var tag = FindIn(ProjectSession.OwnModule, id);
            if (tag == null)
            {
                return "这个标签已经不在了。";
            }

            var doomed = new List<ulong>();
            CollectOwnedSubtree(id, doomed);
            if (OtherDepends(doomed))
            {
                return "其他数据源的标签还挂在这个层级下。";
            }

            var module = ProjectSession.OwnModule;
            for (var i = 0; i < doomed.Count; i++)
            {
                RemoveOwned(module, doomed[i]);
                if (ProjectSession.Archive != null)
                {
                    ProjectSession.Archive.TagNames.Remove(doomed[i]);
                }
            }

            ProjectSession.Save();
            if (doomed.Contains(_selectedId))
            {
                _selectedId = 0;
            }

            Warn("");
            RefreshLists();
            return null;
        }

        void Warn(string text)
        {
            var status = view.GetChild("status") as GTextField ?? view.GetChild("mWarn") as GTextField;
            if (status == null)
            {
                return;
            }

            status.text = text ?? "";
            status.visible = !string.IsNullOrEmpty(text);
            if (status.name == "mWarn")
            {
                status.SetXY(560f, 20f);
                status.SetSize(1020f, 28f);
            }
        }

        public static ulong CreateRoot(string name)
        {
            var parts = SplitPath(name);
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null || parts.Count == 0
                || parts.Count > TagCatalog.MaxHierarchyDepth)
            {
                return 0;
            }

            if (parts.Count == 1)
            {
                var created = CreateOwned(0, parts[0]);
                return created == null ? 0UL : created.Id;
            }

            ulong parent = 0;
            TagDefinitionConfig leaf = null;
            for (var i = 0; i < parts.Count; i++)
            {
                var found = FindChild(parent, parts[i], 0);
                if (found == null)
                {
                    var depth = parent == 0 ? 1 : ChainLength(parent) + 1;
                    if (depth > TagCatalog.MaxHierarchyDepth)
                    {
                        return 0;
                    }

                    found = CreateOwned(parent, parts[i]);
                    if (found == null)
                    {
                        return 0;
                    }
                }

                parent = found.Id;
                leaf = found;
            }

            return leaf == null ? 0UL : leaf.Id;
        }

        public static string Path(ulong id)
        {
            var names = new List<string>();
            var current = id;
            for (var depth = 0; depth <= TagCatalog.MaxHierarchyDepth && current != 0; depth++)
            {
                names.Add(Leaf(current));
                if (!TryParent(current, out var parent))
                {
                    break;
                }

                current = parent;
            }

            names.Reverse();
            return names.Count == 0 ? "" : string.Join("/", names);
        }

        static List<string> SplitPath(string text)
        {
            var parts = new List<string>();
            var raw = (text ?? "").Split('/');
            for (var i = 0; i < raw.Length; i++)
            {
                var part = raw[i].Trim();
                if (part.Length > 0)
                {
                    parts.Add(part);
                }
            }

            return parts;
        }

        static TagDefinitionConfig CreateOwned(ulong parent, string name)
        {
            if (!ProjectSession.IsOpen || ProjectSession.OwnModule == null)
            {
                return null;
            }

            name = (name ?? "").Trim();
            if (name.Length == 0)
            {
                return null;
            }

            var id = TagId.Combine(ProjectSession.AuthorId, NextLocalId());
            if (!LibraryTags.CanBeCreatedByConfiguration(id))
            {
                return null;
            }

            var tag = new TagDefinitionConfig
            {
                Id = id.Value,
                ParentId = parent,
            };
            ProjectSession.OwnModule.Tags.Add(tag);
            ProjectSession.SetTagName(id.Value, name);
            return tag;
        }

        static TagDefinitionConfig FindOwnChild(ulong parent, string name)
        {
            return FindChildIn(ProjectSession.OwnModule, parent, name, 0);
        }

        static TagDefinitionConfig FindChild(ulong parent, string name, ulong except)
        {
            var found = FindChildIn(ProjectSession.OwnModule, parent, name, except);
            if (found != null)
            {
                return found;
            }

            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                found = FindChildIn(ProjectSession.Sources[i].Module, parent, name, except);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        static TagDefinitionConfig FindChildIn(SpellModuleData module, ulong parent, string name, ulong except)
        {
            if (module == null)
            {
                return null;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag == null || tag.Id == except || tag.ParentId != parent)
                {
                    continue;
                }

                if (except != 0 && IsUnder(tag.Id, except))
                {
                    continue;
                }

                if (string.Equals(Leaf(tag.Id), name, StringComparison.OrdinalIgnoreCase))
                {
                    return tag;
                }
            }

            return null;
        }

        static string Leaf(ulong id)
        {
            return ProjectSession.TagName(id, new TagId(id).LocalId);
        }

        static bool TooDeep(ulong parent, ulong moving, int extra = 0)
        {
            var depth = parent == 0 ? 0 : ChainLength(parent);
            depth += extra + 1 + DepthBelow(moving, new HashSet<ulong>());
            return depth > TagCatalog.MaxHierarchyDepth;
        }

        static int ChainLength(ulong id)
        {
            var count = 0;
            var current = id;
            for (var i = 0; i < TagCatalog.MaxHierarchyDepth && current != 0; i++)
            {
                count++;
                if (!TryParent(current, out var parent) || parent == 0)
                {
                    return count;
                }

                current = parent;
            }

            return count;
        }

        static int DepthBelow(ulong id, HashSet<ulong> stack)
        {
            if (!stack.Add(id))
            {
                return 0;
            }

            var best = 0;
            EachTag(tag =>
            {
                if (tag.ParentId != id)
                {
                    return;
                }

                var below = 1 + DepthBelow(tag.Id, stack);
                if (below > best)
                {
                    best = below;
                }
            });
            stack.Remove(id);
            return best;
        }

        static void EachTag(Action<TagDefinitionConfig> visit)
        {
            VisitModule(ProjectSession.OwnModule, visit);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                VisitModule(ProjectSession.Sources[i].Module, visit);
            }
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
                if (module.Tags[i] == null)
                {
                    continue;
                }

                var id = new TagId(module.Tags[i].Id);
                if (id.AuthorId == author && id.LocalId > highest)
                {
                    highest = id.LocalId;
                }
            }
        }

        static bool Exists(ulong id)
        {
            return Find(id) != null;
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

        static void RemoveOwned(SpellModuleData module, ulong id)
        {
            if (module == null)
            {
                return;
            }

            for (var i = module.Tags.Count - 1; i >= 0; i--)
            {
                if (module.Tags[i] != null && module.Tags[i].Id == id)
                {
                    module.Tags.RemoveAt(i);
                }
            }
        }

        static void CollectOwnedSubtree(ulong id, List<ulong> into)
        {
            into.Add(id);
            var module = ProjectSession.OwnModule;
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag != null && tag.ParentId == id && !into.Contains(tag.Id))
                {
                    CollectOwnedSubtree(tag.Id, into);
                }
            }
        }

        static bool OtherDepends(List<ulong> ids)
        {
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var module = ProjectSession.Sources[i].Module;
                if (module == null)
                {
                    continue;
                }

                for (var t = 0; t < module.Tags.Count; t++)
                {
                    var tag = module.Tags[t];
                    if (tag != null && ids.Contains(tag.ParentId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static int IndexOf(List<TagRow> rows, ulong id)
        {
            if (id == 0)
            {
                return -1;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                if (!rows[i].Draft && rows[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        static int IndexOfDraft(List<TagRow> rows)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                if (rows[i].Draft)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
