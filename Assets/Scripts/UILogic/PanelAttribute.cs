using PackageAtrribute;

namespace UILogic
{
    public class PanelAttribute : FguiPanelBase
    {
        public override void Start(object[] data)
        {
            var view = (UI_PanelAttribute)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            new ComAttributeEditor(view);
        }
    }
}
