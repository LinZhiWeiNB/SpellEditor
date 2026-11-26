using PackageAtrribute;

namespace UILogic
{
    public class PanelUnit : FguiPanelBase
    {
        ComUnitEditor _editor;

        public override void Start(object[] data)
        {
            var view = (UI_PanelUnit)FGUIComponent;
            _editor = new ComUnitEditor(view);
        }

        public override void OnShow()
        {
            if (_editor != null)
            {
                _editor.Refresh();
            }
        }

        public override void OnClose()
        {
            if (_editor != null)
            {
                _editor.Close();
            }
        }
    }
}
