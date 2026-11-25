/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_WindowLauncher : GComponent
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "WindowLauncher";
        public GGraph m_bg;
        public GGraph m_side;
        public GTextField m_brand;
        public GTextField m_brandDesc;
        public UI_ButtonPrimary m_btnNew;
        public UI_ButtonSecondary m_btnOpen;
        public GTextField m_recentTitle;
        public GList m_recent;
        public const string URL = "ui://mskevjh6lpw01";

        public static UI_WindowLauncher CreateInstance()
        {
            return (UI_WindowLauncher)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_side = (GGraph)GetChildAt(1);
            m_brand = (GTextField)GetChildAt(2);
            m_brandDesc = (GTextField)GetChildAt(3);
            m_btnNew = (UI_ButtonPrimary)GetChildAt(4);
            m_btnOpen = (UI_ButtonSecondary)GetChildAt(5);
            m_recentTitle = (GTextField)GetChildAt(6);
            m_recent = (GList)GetChildAt(7);
        }
    }
}