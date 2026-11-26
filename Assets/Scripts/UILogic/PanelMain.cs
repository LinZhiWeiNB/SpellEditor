using PackageAtrribute;
using PackageMain;
using PackageResource;
using PackageSpell;
using PackageUnit;
using SpellEditor;

namespace UILogic
{
    public class PanelMain : FguiPanelBase
    {
        UI_WindowMain view;

        public override void Start(object[] data)
        {
            view = (UI_WindowMain)FGUIComponent;
            view.m_projectName.text = ProjectSession.Archive == null ? "" : ProjectSession.Archive.Name;
            view.m_exportStatus.text = "";
            view.m_btnUnit.onClick.Add(() => Open<PanelUnitList>(UI_PanelUnitList.PackageName, UI_PanelUnitList.ResName));
            view.m_btnUnitPreset.onClick.Add(() => Open<PanelUnit>(UI_PanelUnit.PackageName, UI_PanelUnit.ResName));
            view.m_btnAttribute.onClick.Add(() => Open<PanelAttribute>(UI_PanelAttribute.PackageName, UI_PanelAttribute.ResName));
            view.m_btnTag.onClick.Add(() => Open<PanelTag>(UI_PanelTag.PackageName, UI_PanelTag.ResName));
            view.m_btnSpell.onClick.Add(() => Open<PanelSpellList>(UI_PanelSpellList.PackageName, UI_PanelSpellList.ResName));
            view.m_btnResources.onClick.Add(() => Open<PanelResource>(UI_PanelResource.PackageName, UI_PanelResource.ResName));
            var plugins = view.GetChild("btnPlugins");
            if (plugins != null)
            {
                plugins.onClick.Add(() => Open<PanelPlugins>(UI_PanelPlugins.PackageName, UI_PanelPlugins.ResName));
            }
            view.m_btnProject.onClick.Add(() => Open<PanelProjectSettings>(UI_PanelProjectSettings.PackageName, UI_PanelProjectSettings.ResName));
            view.m_btnExportSettings.onClick.Add(() => Open<PanelExport>(UI_PanelExport.PackageName, UI_PanelExport.ResName));
            view.m_btnExportAll.onClick.Add(ExportAll);
        }

        void Open<T>(string packageName, string panelName) where T : FguiPanelBase, new()
        {
            FGUIPanelManager.Instance.ShowPanel<T>(
                packageName,
                panelName,
                showType: ShowTypeEnum.HideCurrent);
        }

        void ExportAll()
        {
            if (!ProjectExport.HasSettings())
            {
                view.m_cExport.selectedIndex = (int)UI_WindowMain.EcExport.空;
                view.m_exportStatus.text = "";
                Open<PanelExport>(UI_PanelExport.PackageName, UI_PanelExport.ResName);
                return;
            }

            var error = ProjectExport.ExportStored();
            view.m_cExport.selectedIndex = error == null
                ? (int)UI_WindowMain.EcExport.成功
                : (int)UI_WindowMain.EcExport.警告;
            view.m_exportStatus.text = error ?? "已写出数据包。";
        }
    }
}
