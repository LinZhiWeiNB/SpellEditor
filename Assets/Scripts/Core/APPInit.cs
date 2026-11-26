using PackageAtrribute;
using PackageLaunch;
using PackageMain;
using PackageResource;
using PackageSpell;
using PackageUnit;
using RebornFrame.Core;
using UILogic;

public class AppInit : IDataInit
{
    public override void LoadLaunchUI(string sceneName)
    {
        PackageLaunchBinder.BindAll();
        PackageMainBinder.BindAll();
        PackageAtrributeBinder.BindAll();
        PackageSpellBinder.BindAll();
        PackageUnitBinder.BindAll();
        PackageResourceBinder.BindAll();
        FGUIPanelManager.Instance.ShowPanel<PanelLauncher>(UI_WindowLauncher.PackageName, UI_WindowLauncher.ResName);
    }
    public override void Awake()
    {
        
    }
}
