/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_ItemPlugin : GButton
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "ItemPlugin";
        public GGraph m_bg;
        public GGraph m_line;
        public GTextField m_meta;
        public const string URL = "ui://mov3ykknplg02";

        public static UI_ItemPlugin CreateInstance()
        {
            return (UI_ItemPlugin)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_line = (GGraph)GetChildAt(1);
            m_meta = (GTextField)GetChildAt(3);
        }
    }
}