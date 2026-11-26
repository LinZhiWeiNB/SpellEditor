using PackageAtrribute;

namespace UILogic
{
    public class PanelTag : FguiPanelBase
    {
        public override void Start(object[] data)
        {
            var view = (UI_PanelTag)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            new ComTagEditor(view);
        }
    }
}
