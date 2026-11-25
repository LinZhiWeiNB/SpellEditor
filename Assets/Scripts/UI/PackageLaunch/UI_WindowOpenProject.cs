/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_WindowOpenProject : GComponent
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "WindowOpenProject";
        public GGraph m_bg;
        public UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_lede;
        public UI_ButtonPrimary m_btnPick;
        public const string URL = "ui://mskevjh6lpw03";

        public static UI_WindowOpenProject CreateInstance()
        {
            return (UI_WindowOpenProject)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_lede = (GTextField)GetChildAt(3);
            m_btnPick = (UI_ButtonPrimary)GetChildAt(4);
        }
    }
}