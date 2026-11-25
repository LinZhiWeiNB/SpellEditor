using FairyGUI;
using PackageAtrribute;
using PackageMain;

namespace UILogic
{
    public class PanelMain : FguiPanelBase
    {
        UI_WindowMain view;
        GButton _exportButton;

        public override void Start(object[] data)
        {
            view = (UI_WindowMain)FGUIComponent;
            view.m_cPage.selectedIndex = (int)UI_WindowMain.EcPage.入口;
            new ComAttributeEditor(view.m_attributeEditor);
            new ComUnitEditor(view.m_unitEditor);
            _exportButton = UI_ButtonPrimary.CreateInstance();
            _exportButton.title = "导出项目";
            _exportButton.SetXY(view.width - 184, 24);
            _exportButton.SetSize(160, 40);
            _exportButton.AddRelation(view, RelationType.Right_Right);
            _exportButton.onClick.Add(OpenExport);
            view.AddChild(_exportButton);
            view.m_cPage.onChanged.Add(SyncExportButton);
            SyncExportButton();
        }

        void OpenExport()
        {
            if (view.GetChild("export") != null)
            {
                return;
            }

            new ComExport(view);
        }

        void SyncExportButton()
        {
            _exportButton.visible = view.m_cPage.selectedIndex == (int)UI_WindowMain.EcPage.入口;
        }
    }
}
