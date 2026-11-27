using FairyGUI;
using PackageAtrribute;
using PackageLaunch;
using PackageMain;
using PackageResource;
using PackageSpell;
using PackageUnit;
using RebornFrame.Core;
using UILogic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        var layer = LayerMask.NameToLayer(StageCamera.LayerName);
        var mask = layer < 0 ? 0 : 1 << layer;
        var uiCamera = FGUIPanelManager.Instance.CreateCamera(StageCamera.Name, mask);
        var main = Camera.main;
        if (main != null && uiCamera != null)
        {
            var mainData = main.GetUniversalAdditionalCameraData();
            var uiData = uiCamera.GetUniversalAdditionalCameraData();
            uiData.renderType = CameraRenderType.Overlay;
            if (!mainData.cameraStack.Contains(uiCamera))
                mainData.cameraStack.Add(uiCamera);
            if (layer >= 0)
                main.cullingMask &= ~mask;
        }

        FGUIPanelManager.Instance.ShowPanel<PanelLauncher>(UI_WindowLauncher.PackageName, UI_WindowLauncher.ResName);
    }
    public override void Awake()
    {
        
    }
}
