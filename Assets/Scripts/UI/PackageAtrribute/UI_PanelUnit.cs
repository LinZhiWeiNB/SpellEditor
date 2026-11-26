/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_PanelUnit : GComponent
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "PanelUnit";
        public Controller m_cRole;

        public enum EcRole
        {
            /// <summary>
            /// null
            /// <summary>
            游戏开发者 = 0,
            /// <summary>
            /// null
            /// <summary>
            模组作者 = 1,
        }

        public EcRole GetSelectedIndexEcRole()
        {
            return (EcRole)m_cRole.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_pLeft;
        public GGraph m_pSplit;
        public GTextField m_pgTitle;
        public GTextField m_pgDesc;
        public GList m_units;
        public UI_ButtonSecondary m_pgCreate;
        public GTextField m_pgNameLabel;
        public GGraph m_pgNameBg;
        public GTextInput m_pgName;
        public GTextField m_pgOwn;
        public GTextField m_pgHint;
        public GTextField m_pgHead;
        public GList m_included;
        public GGroup m_preset;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public const string URL = "ui://m5lrgzhjg8btc";

        public static UI_PanelUnit CreateInstance()
        {
            return (UI_PanelUnit)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cRole = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_pLeft = (GGraph)GetChildAt(1);
            m_pSplit = (GGraph)GetChildAt(2);
            m_pgTitle = (GTextField)GetChildAt(3);
            m_pgDesc = (GTextField)GetChildAt(4);
            m_units = (GList)GetChildAt(5);
            m_pgCreate = (UI_ButtonSecondary)GetChildAt(6);
            m_pgNameLabel = (GTextField)GetChildAt(7);
            m_pgNameBg = (GGraph)GetChildAt(8);
            m_pgName = (GTextInput)GetChildAt(9);
            m_pgOwn = (GTextField)GetChildAt(10);
            m_pgHint = (GTextField)GetChildAt(11);
            m_pgHead = (GTextField)GetChildAt(12);
            m_included = (GList)GetChildAt(13);
            m_preset = (GGroup)GetChildAt(14);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(15);
        }
    }
}