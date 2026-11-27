using PackageLaunch;

namespace UILogic
{
    public class PanelOpenProject : FguiPanelBase
    {
        UI_WindowOpenProject view;
        string _lede;

        public override void Start(object[] data)
        {
            view = (UI_WindowOpenProject)FGUIComponent;
            _lede = view.m_lede.text;
            view.m_btnBack.onClick.Add(() => FGUIPanelManager.Instance.Back());
            view.m_btnPick.onClick.Add(Pick);
        }

        void Pick()
        {
            var path = GameContentEditor.EditorDialog.OpenByteFile("选择存档");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!GameContentEditor.ProjectSession.Open(path, out var error))
            {
                view.m_lede.text = error;
                return;
            }

            view.m_lede.text = _lede;
            FGUIPanelManager.Instance.ShowPanel<PanelMain>(
                PackageMain.UI_WindowMain.PackageName,
                PackageMain.UI_WindowMain.ResName,
                showType: ShowTypeEnum.CloseCurrent);
        }
    }
}
