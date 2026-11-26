/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_PanelExport : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "PanelExport";
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

        public Controller m_cResult;

        public enum EcResult
        {
            /// <summary>
            /// null
            /// <summary>
            空 = 0,
            /// <summary>
            /// null
            /// <summary>
            成功 = 1,
            /// <summary>
            /// null
            /// <summary>
            警告 = 2,
        }

        public EcResult GetSelectedIndexEcResult()
        {
            return (EcResult)m_cResult.selectedIndex;
        }

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_version;
        public GTextField m_error;
        public GTextField m_labelLocation;
        public GGraph m_bgLocation;
        public GTextInput m_inputLocation;
        public PackageAtrribute.UI_ButtonSecondary m_btnLocation;
        public GTextField m_labelModDir;
        public GGraph m_bgModDir;
        public GTextInput m_inputModDir;
        public GGroup m_gameFields;
        public GTextField m_labelName;
        public GGraph m_bgName;
        public GTextInput m_inputName;
        public GTextField m_labelIntro;
        public GGraph m_bgIntro;
        public GTextInput m_inputIntro;
        public GTextField m_labelDir;
        public GGraph m_bgDir;
        public GTextInput m_inputDir;
        public GGroup m_modFields;
        public PackageAtrribute.UI_ButtonPrimary m_btnExport;
        public const string URL = "ui://mov3ykknmne01";

        public static UI_PanelExport CreateInstance()
        {
            return (UI_PanelExport)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cRole = GetControllerAt(0);
            m_cResult = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_version = (GTextField)GetChildAt(3);
            m_error = (GTextField)GetChildAt(4);
            m_labelLocation = (GTextField)GetChildAt(5);
            m_bgLocation = (GGraph)GetChildAt(6);
            m_inputLocation = (GTextInput)GetChildAt(7);
            m_btnLocation = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(8);
            m_labelModDir = (GTextField)GetChildAt(9);
            m_bgModDir = (GGraph)GetChildAt(10);
            m_inputModDir = (GTextInput)GetChildAt(11);
            m_gameFields = (GGroup)GetChildAt(12);
            m_labelName = (GTextField)GetChildAt(13);
            m_bgName = (GGraph)GetChildAt(14);
            m_inputName = (GTextInput)GetChildAt(15);
            m_labelIntro = (GTextField)GetChildAt(16);
            m_bgIntro = (GGraph)GetChildAt(17);
            m_inputIntro = (GTextInput)GetChildAt(18);
            m_labelDir = (GTextField)GetChildAt(19);
            m_bgDir = (GGraph)GetChildAt(20);
            m_inputDir = (GTextInput)GetChildAt(21);
            m_modFields = (GGroup)GetChildAt(22);
            m_btnExport = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(23);
        }
    }
}