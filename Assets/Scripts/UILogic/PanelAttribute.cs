using PackageAtrribute;

namespace UILogic
{
    public class PanelAttribute : FguiPanelBase
    {
        ComAttributeEditor _editor;

        public override void Start(object[] data)
        {
            var view = (UI_PanelAttribute)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            _editor = new ComAttributeEditor(view);
            DeleteGesture.Bind(this, _editor.DeleteSelected);
            if (data != null && data.Length > 0 && data[0] is ulong id)
            {
                _editor.Select(id);
            }
        }

        public override void OnShow()
        {
            if (_editor != null)
                DeleteGesture.Bind(this, _editor.DeleteSelected);
        }

        public override void OnClose()
        {
            DeleteGesture.Unbind(this);
        }
    }
}
