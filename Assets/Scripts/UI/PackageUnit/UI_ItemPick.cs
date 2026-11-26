/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_ItemPick : GButton
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "ItemPick";
        public GGraph m_bg;
        public const string URL = "ui://unt4p8n2mnu07";

        public static UI_ItemPick CreateInstance()
        {
            return (UI_ItemPick)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
        }
    }
}