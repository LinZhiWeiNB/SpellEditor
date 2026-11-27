using FairyGUI;
using UnityEngine;

namespace UILogic
{
    /// <summary>还没设计内容的说明面板。只显示标题和一句说明。</summary>
    public class PanelBlank : FguiPanelBase
    {
        const string Package = "PackageSpell";
        const string Panel = "PanelBlank";

        public static bool Show(string title, string hint)
        {
            if (!PanelLayer.Published(Package, Panel))
            {
                Debug.LogWarning("PackageSpell 里还没有 PanelBlank，先在 FairyGUI 发布技能包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelBlank>(
                Package,
                Panel,
                new object[] { title, hint },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            var view = FGUIComponent;
            PanelLayer.Text(view, "title", data != null && data.Length > 0 ? data[0] as string : "");
            PanelLayer.Text(view, "hint", data != null && data.Length > 1 ? data[1] as string : "");
            PanelLayer.Mask(view, Leave);
            (view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
        }

        static void Leave()
        {
            FGUIPanelManager.Instance.Back();
        }
    }
}
