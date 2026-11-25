/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_ItemRecent : GComponent
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "ItemRecent";
        public GGraph m_bg;
        public GTextField m_title;
        public GTextField m_meta;
        public UI_ButtonSecondary m_enter;
        public const string URL = "ui://mskevjh6lpi01";

        public static UI_ItemRecent CreateInstance()
        {
            return (UI_ItemRecent)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_title = (GTextField)GetChildAt(1);
            m_meta = (GTextField)GetChildAt(2);
            m_enter = (UI_ButtonSecondary)GetChildAt(3);
        }
    }
}