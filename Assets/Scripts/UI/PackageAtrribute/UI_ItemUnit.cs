/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ItemUnit : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ItemUnit";
        public GGraph m_bg;
        public GGraph m_bar;
        public GTextField m_meta;
        public const string URL = "ui://m5lrgzhjg8bta";

        public static UI_ItemUnit CreateInstance()
        {
            return (UI_ItemUnit)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_bar = (GGraph)GetChildAt(1);
            m_meta = (GTextField)GetChildAt(3);
        }
    }
}