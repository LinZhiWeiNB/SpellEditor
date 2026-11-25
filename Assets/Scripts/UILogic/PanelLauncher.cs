using System;
using FairyGUI;
using PackageLaunch;
using PackageMain;
using SpellEditor;

namespace UILogic
{
    public class PanelLauncher : FguiPanelBase
    {
        UI_WindowLauncher view;
        string _brandDesc;
        System.Collections.Generic.List<RecentProject> _recents;

        public override void Start(object[] data)
        {
            view = (UI_WindowLauncher)FGUIComponent;
            _brandDesc = view.m_brandDesc.text;
            view.m_btnNew.onClick.Add(OpenNewProject);
            view.m_btnOpen.onClick.Add(OpenProject);
            view.m_recent.itemRenderer = RenderRecent;
            RefreshRecents();
        }

        public override void OnShow()
        {
            if (view != null)
            {
                view.m_brandDesc.text = _brandDesc;
                RefreshRecents();
            }
        }

        void RefreshRecents()
        {
            _recents = RecentProjects.Load();
            view.m_recent.numItems = _recents.Count;
        }

        void RenderRecent(int index, GObject item)
        {
            var recent = _recents[index];
            var row = (UI_ItemRecent)item;
            row.m_title.text = recent.Name;
            var role = recent.AuthorId == 0 ? "游戏开发者" : "模组作者";
            var time = new DateTime(recent.OpenedAt).ToString("yyyy-MM-dd HH:mm");
            row.m_meta.text = role + "  ·  " + time + "  ·  " + recent.Path;
            row.m_enter.data = recent.Path;
            row.m_enter.onClick.Set(EnterRecent);
        }

        void EnterRecent(EventContext context)
        {
            var path = (string)((GObject)context.sender).data;
            if (!Enter(path, out var error))
            {
                view.m_brandDesc.text = error;
            }
        }

        void OpenNewProject()
        {
            FGUIPanelManager.Instance.ShowPanel<PanelNewProject>(
                UI_WindowNewProject.PackageName,
                UI_WindowNewProject.ResName,
                showType: ShowTypeEnum.HideCurrent);
        }

        void OpenProject()
        {
            FGUIPanelManager.Instance.ShowPanel<PanelOpenProject>(
                UI_WindowOpenProject.PackageName,
                UI_WindowOpenProject.ResName,
                showType: ShowTypeEnum.HideCurrent);
        }

        public static bool Enter(string path, out string error)
        {
            if (!ProjectSession.Open(path, out error))
            {
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelMain>(
                UI_WindowMain.PackageName,
                UI_WindowMain.ResName,
                showType: ShowTypeEnum.CloseCurrent);
            return true;
        }
    }
}
