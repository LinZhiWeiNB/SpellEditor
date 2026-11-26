using PackageAtrribute;

namespace UILogic
{
    public class PanelAttribute : FguiPanelBase
    {
        public override void Start(object[] data)
        {
            var view = (UI_PanelAttribute)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            var editor = new ComAttributeEditor(view);
            if (data != null && data.Length > 0 && data[0] is ulong id)
            {
                editor.Select(id);
            }
        }
    }
}
