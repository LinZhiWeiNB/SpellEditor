using FairyGUI;

namespace UILogic
{
    /// <summary>弹出的面板盖在当前页上，关闭时回到原来的页。</summary>
    public static class PanelLayer
    {
        public static bool Published(string package, string component)
        {
            var found = UIPackage.GetByName(package);
            return found != null && found.GetItemByName(component) != null;
        }

        public static void Dismiss<T>() where T : FguiPanelBase
        {
            if (FGUIPanelManager.Instance.GetCurrentPanel() is T)
            {
                FGUIPanelManager.Instance.Back();
            }
        }

        public static void Text(GComponent view, string name, string value)
        {
            if (view != null && view.GetChild(name) is GTextField field)
            {
                field.text = value ?? "";
            }
        }

        public static GGraph Mask(GComponent view, EventCallback0 onClick)
        {
            var mask = view.GetChild("mask") as GGraph;
            if (mask != null)
            {
                mask.touchable = true;
                mask.onClick.Set(onClick);
            }

            return mask;
        }
    }
}
