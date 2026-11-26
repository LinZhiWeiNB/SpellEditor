/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_ItemUnitRow : GButton
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "ItemUnitRow";
        public GGraph m_bg;
        public GTextField m_meta;
        public const string URL = "ui://unt4p8n2mnu03";

        public static UI_ItemUnitRow CreateInstance()
        {
            return (UI_ItemUnitRow)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_meta = (GTextField)GetChildAt(2);
        }
    }
}