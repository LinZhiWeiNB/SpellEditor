/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_ItemContent : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "ItemContent";
        public GGraph m_bg;
        public GTextField m_title;
        public GTextField m_count;
        public GTextField m_desc;
        public const string URL = "ui://mov3ykknmni01";

        public static UI_ItemContent CreateInstance()
        {
            return (UI_ItemContent)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_title = (GTextField)GetChildAt(1);
            m_count = (GTextField)GetChildAt(2);
            m_desc = (GTextField)GetChildAt(3);
        }
    }
}