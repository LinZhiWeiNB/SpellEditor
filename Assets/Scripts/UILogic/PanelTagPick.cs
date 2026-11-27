using System;
using System.Collections.Generic;
using FairyGUI;
using PackageAtrribute;
using RebornFrame.Spell;
using GameContentEditor;
using UnityEngine;

namespace UILogic
{
    /// <summary>层级列出已有标签。点一条就使用；底部可以按路径新建并使用。</summary>
    public class PanelTagPick : FguiPanelBase
    {
        struct Row
        {
            public ulong Id;
            public int Depth;
            public bool HasChildren;
            public bool Expanded;
            public bool Owned;
            public string Title;
        }

        const string Package = "PackageSpell";
        const string Panel = "PanelTagPick";

        readonly Dictionary<ulong, ulong> _parent = new Dictionary<ulong, ulong>();
        readonly Dictionary<ulong, string> _names = new Dictionary<ulong, string>();
        readonly Dictionary<ulong, bool> _owned = new Dictionary<ulong, bool>();
        readonly List<Row> _rows = new List<Row>();
        readonly HashSet<ulong> _collapsed = new HashSet<ulong>();
        GComponent _view;
        Action<ulong> _onPick;

        public static bool Show(Action<ulong> onPick)
        {
            if (!PanelLayer.Published(Package, Panel))
            {
                Debug.LogWarning("PackageSpell 里还没有 PanelTagPick，先在 FairyGUI 发布技能包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelTagPick>(
                Package,
                Panel,
                new object[] { onPick },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            _view = FGUIComponent;
            _onPick = data != null && data.Length > 0 ? data[0] as Action<ulong> : null;
            Arrange();
            Collect();
            var search = Input("search");
            var name = Input("inputName");
            search.text = "";
            name.text = "";
            Label("createWarn").text = "";
            search.onChanged.Set(Fill);
            name.onSubmit.Set(Create);
            PanelLayer.Mask(_view, Leave);
            (_view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
            (_view.GetChild("btnCreate") as GButton).onClick.Set(Create);
            var list = _view.GetChild("choices") as GList;
            list.defaultItem = UI_ItemTag.URL;
            list.itemRenderer = Render;
            Fill();
        }

        public override void OnClose()
        {
            _onPick = null;
            _view = null;
        }

        void Arrange()
        {
            Place("card", 480f, 120f, 640f, 660f);
            Place("title", 504f, 140f, 480f, 28f);
            Place("bgSearch", 504f, 180f, 592f, 36f);
            Place("search", 516f, 180f, 568f, 36f);
            Place("choices", 504f, 228f, 592f, 400f);
            Place("empty", 504f, 228f, 592f, 36f);
            Place("createWarn", 504f, 636f, 592f, 22f);
            Place("bgName", 504f, 668f, 420f, 36f);
            Place("inputName", 516f, 668f, 396f, 36f);
            Place("btnCreate", 936f, 668f, 160f, 36f);
            Place("btnCancel", 1008f, 720f, 88f, 32f);
            Hide("split");
            Hide("createTitle");
            Hide("labelName");
            Hide("createHint");
        }

        void Place(string name, float x, float y, float width, float height)
        {
            var child = _view.GetChild(name);
            if (child == null)
            {
                return;
            }

            child.visible = true;
            child.SetXY(x, y);
            child.SetSize(width, height);
        }

        void Hide(string name)
        {
            var child = _view.GetChild(name);
            if (child != null)
            {
                child.visible = false;
            }
        }

        void Leave()
        {
            _onPick = null;
            FGUIPanelManager.Instance.Back();
        }

        void Collect()
        {
            _parent.Clear();
            _names.Clear();
            _owned.Clear();
            Note(ProjectSession.OwnModule, true);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                Note(ProjectSession.Sources[i].Module, false);
            }
        }

        void Note(SpellModuleData module, bool owned)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag == null || tag.Id == 0 || _parent.ContainsKey(tag.Id))
                {
                    continue;
                }

                var id = new TagId(tag.Id);
                _parent.Add(tag.Id, tag.ParentId);
                _names.Add(tag.Id, ProjectSession.TagName(tag.Id, id.LocalId));
                _owned.Add(tag.Id, owned);
            }
        }

        void Fill()
        {
            if (_view == null)
            {
                return;
            }

            var query = (Input("search").text ?? "").Trim();
            _rows.Clear();
            var roots = Kids(0);
            for (var i = 0; i < roots.Count; i++)
            {
                Walk(roots[i], 0, query);
            }

            var list = _view.GetChild("choices") as GList;
            list.numItems = _rows.Count;
            _view.GetChild("empty").visible = _rows.Count == 0;
        }

        void Walk(ulong id, int depth, string query)
        {
            if (!string.IsNullOrEmpty(query) && !BranchHits(id, query))
            {
                return;
            }

            var kids = Kids(id);
            var hasChildren = kids.Count > 0;
            var expanded = hasChildren && (!_collapsed.Contains(id) || DescendantHits(id, query));
            _rows.Add(new Row
            {
                Id = id,
                Depth = depth,
                HasChildren = hasChildren,
                Expanded = expanded,
                Owned = _owned.ContainsKey(id) && _owned[id],
                Title = _names.ContainsKey(id) ? _names[id] : "",
            });
            if (!expanded)
            {
                return;
            }

            for (var i = 0; i < kids.Count; i++)
            {
                Walk(kids[i], depth + 1, query);
            }
        }

        bool BranchHits(ulong id, string query)
        {
            var current = id;
            for (var depth = 0; depth <= TagCatalog.MaxHierarchyDepth && current != 0; depth++)
            {
                if (_names.ContainsKey(current)
                    && _names[current].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (!_parent.TryGetValue(current, out var parent) || parent == 0 || parent == current)
                {
                    break;
                }

                current = parent;
            }

            return DescendantHits(id, query);
        }

        bool DescendantHits(ulong id, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            var kids = Kids(id);
            for (var i = 0; i < kids.Count; i++)
            {
                if (BranchHits(kids[i], query))
                {
                    return true;
                }
            }

            return false;
        }

        List<ulong> Kids(ulong parent)
        {
            var kids = new List<ulong>();
            foreach (var pair in _parent)
            {
                var root = pair.Value == 0 || !_parent.ContainsKey(pair.Value);
                if (parent == 0 ? root : pair.Value == parent)
                {
                    kids.Add(pair.Key);
                }
            }

            kids.Sort(CompareName);
            return kids;
        }

        int CompareName(ulong left, ulong right)
        {
            var leftName = _names.ContainsKey(left) ? _names[left] : "";
            var rightName = _names.ContainsKey(right) ? _names[right] : "";
            var order = string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
            return order != 0 ? order : left.CompareTo(right);
        }

        void Render(int index, GObject obj)
        {
            var row = _rows[index];
            var button = (GButton)obj;
            var list = _view.GetChild("choices") as GList;
            button.SetSize(list.width, button.height);
            var id = row.Id;
            var type = button.GetChild("type");
            if (type != null)
            {
                type.visible = false;
            }

            var meta = button.GetChild("meta");
            if (meta != null)
            {
                meta.visible = false;
            }

            var indent = 8f + row.Depth * 16f;
            var titleX = indent + 22f;
            var fold = button.GetChild("fold") as GButton;
            if (fold != null)
            {
                fold.SetXY(indent, 4f);
                fold.visible = row.HasChildren;
                fold.touchable = row.HasChildren;
                fold.title = row.Expanded ? "▾" : "▸";
                fold.onClick.Set(context =>
                {
                    context.StopPropagation();
                    if (!_collapsed.Add(id))
                    {
                        _collapsed.Remove(id);
                    }

                    Fill();
                });
            }

            var title = button.GetChild("title");
            if (title != null)
            {
                title.SetXY(titleX, title.y);
                title.width = Math.Max(40f, button.width - titleX - 8f);
            }

            button.title = row.HasChildren && fold == null
                ? (row.Expanded ? "▾ " : "▸ ") + row.Title
                : row.Title;
            if (button is UI_ItemTag item)
            {
                item.m_cTone.selectedIndex = row.Owned ? 0 : 1;
            }

            button.onClick.Set(() => Use(id));
        }

        void Create()
        {
            var name = (Input("inputName").text ?? "").Trim();
            if (name.Length == 0)
            {
                Label("createWarn").text = "先填写名称。";
                return;
            }

            var id = ComTagEditor.CreateRoot(name);
            if (id == 0)
            {
                Label("createWarn").text = "没能创建标签。";
                return;
            }

            Use(id);
        }

        void Use(ulong id)
        {
            var pick = _onPick;
            _onPick = null;
            FGUIPanelManager.Instance.Back();
            if (pick != null)
            {
                pick(id);
            }
        }

        GTextInput Input(string name)
        {
            return _view.GetChild(name) as GTextInput;
        }

        GTextField Label(string name)
        {
            return _view.GetChild(name) as GTextField;
        }
    }
}
