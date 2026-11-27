using PackageAtrribute;

namespace UILogic
{
    public class PanelTag : FguiPanelBase
    {
        ComTagEditor _editor;

        public override void Start(object[] data)
        {
            var view = (UI_PanelTag)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            _editor = new ComTagEditor(view);
            DeleteGesture.Bind(this, _editor.DeleteCurrent);
            if (data != null && data.Length > 0 && data[0] is ulong id && id != 0)
            {
                _editor.SelectId(id);
            }
        }

        public override void OnShow()
        {
            if (_editor != null)
                DeleteGesture.Bind(this, _editor.DeleteCurrent);
        }

        public override void OnClose()
        {
            DeleteGesture.Unbind(this);
        }
    }
}
