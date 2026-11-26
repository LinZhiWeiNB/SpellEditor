/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_ItemLine : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "ItemLine";
        public GTextField m_title;
        public const string URL = "ui://unt4p8n2mnu04";

        public static UI_ItemLine CreateInstance()
        {
            return (UI_ItemLine)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
        }
    }
}