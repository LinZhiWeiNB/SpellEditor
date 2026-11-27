using PackageAtrribute;

namespace UILogic
{
    public class PanelTag : FguiPanelBase
    {
        public override void Start(object[] data)
        {
            var view = (UI_PanelTag)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            var editor = new ComTagEditor(view);
            if (data != null && data.Length > 0 && data[0] is ulong id && id != 0)
            {
                editor.SelectId(id);
            }
        }
    }
}
