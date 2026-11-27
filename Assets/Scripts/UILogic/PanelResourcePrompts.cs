using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace UILogic
{
    /// <summary>资源页弹出的列表面板。点一条就交回去并关闭。</summary>
    public class PanelPickList : FguiPanelBase
    {
        public sealed class Request
        {
            public string Title;
            public string Hint;
            public List<string> Items;
            public Action<string> OnPick;
        }

        readonly List<string> _items = new List<string>();
        Action<string> _onPick;

        public static bool Show(Request request)
        {
            if (!Ready())
            {
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelPickList>(
                "PackageResource",
                "PanelPickList",
                new object[] { request },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            var request = data != null && data.Length > 0 ? data[0] as Request : null;
            var view = FGUIComponent;
            PanelLayer.Text(view, "title", request == null ? "" : request.Title);
            PanelLayer.Text(view, "hint", request == null ? "" : request.Hint);
            if (request != null && request.Items != null)
            {
                _items.AddRange(request.Items);
            }

            _onPick = request == null ? null : request.OnPick;
            PanelLayer.Mask(view, Leave);
            (view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
            var list = view.GetChild("choices") as GList;
            list.itemRenderer = (index, obj) => ((GButton)obj).title = _items[index];
            list.numItems = _items.Count;
            list.onClickItem.Add(context =>
            {
                var index = list.GetChildIndex((GObject)context.data);
                if (index < 0 || index >= _items.Count)
                {
                    return;
                }

                var chosen = _items[index];
                var pick = _onPick;
                _onPick = null;
                FGUIPanelManager.Instance.Back();
                if (pick != null)
                {
                    pick(chosen);
                }
            });
        }

        void Leave()
        {
            _onPick = null;
            FGUIPanelManager.Instance.Back();
        }

        static bool Ready()
        {
            if (PanelLayer.Published("PackageResource", "PanelPickList"))
            {
                return true;
            }

            Debug.LogWarning("PackageResource 里还没有 PanelPickList，先在 FairyGUI 发布资源包。");
            return false;
        }
    }

    /// <summary>同名资源。点已有路径，或填写一条新路径。</summary>
    public class PanelPickName : FguiPanelBase
    {
        public sealed class Request
        {
            public string Title;
            public string Hint;
            public List<string> Items;
            public string Path;
            public string Prompt;
            public string Confirm;
            public Action<string> OnPick;
            public Func<string, string> OnPlace;
            public Action After;
            public Action OnCancel;
        }

        readonly List<string> _items = new List<string>();
        Request _request;

        public static bool Show(Request request)
        {
            if (!PanelLayer.Published("PackageResource", "PanelPickName"))
            {
                Debug.LogWarning("PackageResource 里还没有 PanelPickName，先在 FairyGUI 发布资源包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelPickName>(
                "PackageResource",
                "PanelPickName",
                new object[] { request },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            _request = data != null && data.Length > 0 ? data[0] as Request : null;
            var view = FGUIComponent;
            PanelLayer.Text(view, "title", _request == null ? "" : _request.Title);
            PanelLayer.Text(view, "hint", _request == null ? "" : _request.Hint);
            if (_request != null && _request.Items != null)
            {
                _items.AddRange(_request.Items);
            }

            var input = view.GetChild("inputPath") as GTextInput;
            input.text = _request == null ? "" : _request.Path ?? "";
            input.promptText = _request == null || string.IsNullOrEmpty(_request.Prompt)
                ? "新路径，例如 heroes/player"
                : _request.Prompt;
            var place = view.GetChild("btnPlace") as GButton;
            place.title = _request == null || string.IsNullOrEmpty(_request.Confirm) ? "使用新路径" : _request.Confirm;
            place.onClick.Set(() => Place(view, input.text));
            PanelLayer.Mask(view, Leave);
            (view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
            var list = view.GetChild("choices") as GList;
            list.itemRenderer = (index, obj) => ((GButton)obj).title = _items[index];
            list.numItems = _items.Count;
            list.onClickItem.Add(context =>
            {
                var index = list.GetChildIndex((GObject)context.data);
                if (index < 0 || index >= _items.Count)
                {
                    return;
                }

                var chosen = _items[index];
                var pick = _request == null ? null : _request.OnPick;
                _request = null;
                FGUIPanelManager.Instance.Back();
                if (pick != null)
                {
                    pick(chosen);
                }
            });
        }

        void Place(GComponent view, string path)
        {
            var request = _request;
            if (request == null || request.OnPlace == null)
            {
                return;
            }

            var error = request.OnPlace(path);
            if (!string.IsNullOrEmpty(error))
            {
                PanelLayer.Text(view, "hint", error);
                return;
            }

            var after = request.After;
            _request = null;
            FGUIPanelManager.Instance.Back();
            if (after != null)
            {
                after();
            }
        }

        void Leave()
        {
            var cancel = _request == null ? null : _request.OnCancel;
            _request = null;
            FGUIPanelManager.Instance.Back();
            if (cancel != null)
            {
                cancel();
            }
        }
    }

    /// <summary>填写一个目录名。</summary>
    public class PanelPickPath : FguiPanelBase
    {
        public sealed class Request
        {
            public string Title;
            public string Hint;
            public string Text;
            public string Prompt;
            public string Confirm;
            public Func<string, string> Commit;
        }

        public static bool Show(Request request)
        {
            if (!PanelLayer.Published("PackageResource", "PanelPickPath"))
            {
                Debug.LogWarning("PackageResource 里还没有 PanelPickPath，先在 FairyGUI 发布资源包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelPickPath>(
                "PackageResource",
                "PanelPickPath",
                new object[] { request },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            var request = data != null && data.Length > 0 ? data[0] as Request : null;
            var view = FGUIComponent;
            PanelLayer.Text(view, "title", request == null ? "" : request.Title);
            PanelLayer.Text(view, "hint", request == null ? "" : request.Hint);
            var input = view.GetChild("inputPath") as GTextInput;
            input.text = request == null ? "" : request.Text ?? "";
            input.promptText = request == null || string.IsNullOrEmpty(request.Prompt) ? "目录名" : request.Prompt;
            var place = view.GetChild("btnPlace") as GButton;
            place.title = request == null || string.IsNullOrEmpty(request.Confirm) ? "确定" : request.Confirm;
            place.onClick.Set(() =>
            {
                if (request == null || request.Commit == null)
                {
                    return;
                }

                var error = request.Commit(input.text);
                if (!string.IsNullOrEmpty(error))
                {
                    PanelLayer.Text(view, "hint", error);
                    return;
                }

                FGUIPanelManager.Instance.Back();
            });
            PanelLayer.Mask(view, Leave);
            (view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
        }

        static void Leave()
        {
            FGUIPanelManager.Instance.Back();
        }
    }

    /// <summary>确认删除。</summary>
    public class PanelPickConfirm : FguiPanelBase
    {
        public sealed class Request
        {
            public string Title;
            public string Hint;
            public string Confirm;
            public Func<string> Commit;
        }

        public static bool Show(Request request)
        {
            if (!PanelLayer.Published("PackageResource", "PanelPickConfirm"))
            {
                Debug.LogWarning("PackageResource 里还没有 PanelPickConfirm，先在 FairyGUI 发布资源包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelPickConfirm>(
                "PackageResource",
                "PanelPickConfirm",
                new object[] { request },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            var request = data != null && data.Length > 0 ? data[0] as Request : null;
            var view = FGUIComponent;
            PanelLayer.Text(view, "title", request == null ? "" : request.Title);
            PanelLayer.Text(view, "hint", request == null ? "" : request.Hint);
            var place = view.GetChild("btnPlace") as GButton;
            place.title = request == null || string.IsNullOrEmpty(request.Confirm) ? "删除" : request.Confirm;
            place.onClick.Set(() =>
            {
                if (request == null || request.Commit == null)
                {
                    return;
                }

                var error = request.Commit();
                if (!string.IsNullOrEmpty(error))
                {
                    PanelLayer.Text(view, "hint", error);
                    return;
                }

                FGUIPanelManager.Instance.Back();
            });
            PanelLayer.Mask(view, Leave);
            (view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
        }

        static void Leave()
        {
            FGUIPanelManager.Instance.Back();
        }
    }

    /// <summary>目录上的重命名、新建和删除。</summary>
    public class PanelFolderMenu : FguiPanelBase
    {
        public sealed class Request
        {
            public bool Rename;
            public bool Create;
            public bool Remove;
            public string CreateTitle;
            public string RemoveTitle;
            public Action OnRename;
            public Action OnCreate;
            public Action OnRemove;
        }

        public static bool Show(Request request)
        {
            if (!PanelLayer.Published("PackageResource", "PanelFolderMenu"))
            {
                Debug.LogWarning("PackageResource 里还没有 PanelFolderMenu，先在 FairyGUI 发布资源包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelFolderMenu>(
                "PackageResource",
                "PanelFolderMenu",
                new object[] { request },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            var request = data != null && data.Length > 0 ? data[0] as Request : null;
            var view = FGUIComponent;
            PanelLayer.Mask(view, Leave);
            var rename = view.GetChild("btnRename") as GButton;
            var create = view.GetChild("btnNew") as GButton;
            var remove = view.GetChild("btnRemove") as GButton;
            var card = view.GetChild("card");
            var showRename = request != null && request.Rename;
            var showCreate = request == null || request.Create;
            var showRemove = request != null && request.Remove;
            rename.visible = showRename;
            create.visible = showCreate;
            remove.visible = showRemove;
            if (request != null && !string.IsNullOrEmpty(request.CreateTitle))
            {
                create.title = request.CreateTitle;
            }

            if (request != null && !string.IsNullOrEmpty(request.RemoveTitle))
            {
                remove.title = request.RemoveTitle;
            }
            var y = card.y + 16f;
            if (showRename)
            {
                rename.y = y;
                y += 44f;
            }

            if (showCreate)
            {
                create.y = y;
                y += 44f;
            }

            if (showRemove)
            {
                remove.y = y;
                y += 44f;
            }

            card.SetSize(card.width, y - card.y + 8f);
            rename.onClick.Set(() => Run(request == null ? null : request.OnRename));
            create.onClick.Set(() => Run(request == null ? null : request.OnCreate));
            remove.onClick.Set(() => Run(request == null ? null : request.OnRemove));
        }

        static void Run(Action action)
        {
            FGUIPanelManager.Instance.Back();
            if (action != null)
            {
                action();
            }
        }

        static void Leave()
        {
            FGUIPanelManager.Instance.Back();
        }
    }
}
