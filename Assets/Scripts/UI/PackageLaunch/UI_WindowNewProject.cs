/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_WindowNewProject : GComponent
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "WindowNewProject";
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
        public UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_labelName;
        public GGraph m_bgName;
        public GTextInput m_inputName;
        public GTextField m_labelFolder;
        public GGraph m_bgFolder;
        public GTextInput m_inputFolder;
        public UI_ButtonSecondary m_btnFolder;
        public GTextField m_labelRole;
        public GList m_roles;
        public GTextField m_labelAuthor;
        public GGraph m_bgAuthor;
        public GTextInput m_inputAuthor;
        public GGraph m_calloutBg;
        public GTextField m_calloutTitle;
        public GTextField m_calloutBody;
        public GTextField m_labelSources;
        public GTextField m_sourceHint;
        public GList m_sources;
        public UI_ButtonSecondary m_btnAddSource;
        public GGroup m_modFields;
        public UI_ButtonPrimary m_btnCreate;
        public GTextField m_folderHint;
        public const string URL = "ui://mskevjh6lpw02";

        public static UI_WindowNewProject CreateInstance()
        {
            return (UI_WindowNewProject)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cRole = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_labelName = (GTextField)GetChildAt(3);
            m_bgName = (GGraph)GetChildAt(4);
            m_inputName = (GTextInput)GetChildAt(5);
            m_labelFolder = (GTextField)GetChildAt(6);
            m_bgFolder = (GGraph)GetChildAt(7);
            m_inputFolder = (GTextInput)GetChildAt(8);
            m_btnFolder = (UI_ButtonSecondary)GetChildAt(9);
            m_labelRole = (GTextField)GetChildAt(10);
            m_roles = (GList)GetChildAt(11);
            m_labelAuthor = (GTextField)GetChildAt(12);
            m_bgAuthor = (GGraph)GetChildAt(13);
            m_inputAuthor = (GTextInput)GetChildAt(14);
            m_calloutBg = (GGraph)GetChildAt(15);
            m_calloutTitle = (GTextField)GetChildAt(16);
            m_calloutBody = (GTextField)GetChildAt(17);
            m_labelSources = (GTextField)GetChildAt(18);
            m_sourceHint = (GTextField)GetChildAt(19);
            m_sources = (GList)GetChildAt(20);
            m_btnAddSource = (UI_ButtonSecondary)GetChildAt(21);
            m_modFields = (GGroup)GetChildAt(22);
            m_btnCreate = (UI_ButtonPrimary)GetChildAt(23);
            m_folderHint = (GTextField)GetChildAt(24);
        }
    }
}