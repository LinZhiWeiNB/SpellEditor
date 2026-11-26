/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;

namespace PackageSpell
{
    public class PackageSpellBinder
    {
        public static void BindAll()
        {
            UIObjectFactory.SetPackageItemExtension(UI_ItemKey.URL, typeof(UI_ItemKey));
            UIObjectFactory.SetPackageItemExtension(UI_ItemTrack.URL, typeof(UI_ItemTrack));
            UIObjectFactory.SetPackageItemExtension(UI_ItemSpell.URL, typeof(UI_ItemSpell));
            UIObjectFactory.SetPackageItemExtension(UI_PanelSpell.URL, typeof(UI_PanelSpell));
            UIObjectFactory.SetPackageItemExtension(UI_PanelSpellList.URL, typeof(UI_PanelSpellList));
        }
    }
}