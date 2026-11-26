using PackageAtrribute;

namespace UILogic
{
    public class PanelUnit : FguiPanelBase
    {
        public override void Start(object[] data)
        {
            var view = (UI_PanelUnit)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            new ComUnitEditor(view);
        }
    }
}
