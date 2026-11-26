/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_PanelProjectSettings : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "PanelProjectSettings";
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

        public Controller m_cMessage;

        public enum EcMessage
        {
            /// <summary>
            /// null
            /// <summary>
            空 = 0,
            /// <summary>
            /// null
            /// <summary>
            错误 = 1,
            /// <summary>
            /// null
            /// <summary>
            警告 = 2,
            /// <summary>
            /// null
            /// <summary>
            成功 = 3,
        }

        public EcMessage GetSelectedIndexEcMessage()
        {
            return (EcMessage)m_cMessage.selectedIndex;
        }

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_message;
        public GTextField m_labelName;
        public GGraph m_bgName;
        public GTextInput m_inputName;
        public GTextField m_labelFolder;
        public GGraph m_bgFolder;
        public GTextField m_folder;
        public PackageAtrribute.UI_ButtonSecondary m_btnMigrate;
        public GTextField m_folderHint;
        public GTextField m_labelLogic;
        public GGraph m_bgLogic;
        public GTextInput m_inputLogicFps;
        public GTextField m_logicHint;
        public GTextField m_pluginTitle;
        public GTextField m_pluginHint;
        public PackageAtrribute.UI_ButtonCheck m_checkParticles;
        public PackageAtrribute.UI_ButtonCheck m_checkVfx;
        public PackageAtrribute.UI_ButtonCheck m_checkSpine;
        public PackageAtrribute.UI_ButtonCheck m_checkAnimation;
        public GTextField m_labelSources;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddSource;
        public GTextField m_sourceHint;
        public GList m_sources;
        public GGroup m_modFields;
        public PackageAtrribute.UI_ButtonPrimary m_btnSave;
        public const string URL = "ui://mov3ykknmnp01";

        public static UI_PanelProjectSettings CreateInstance()
        {
            return (UI_PanelProjectSettings)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cRole = GetControllerAt(0);
            m_cMessage = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_message = (GTextField)GetChildAt(3);
            m_labelName = (GTextField)GetChildAt(4);
            m_bgName = (GGraph)GetChildAt(5);
            m_inputName = (GTextInput)GetChildAt(6);
            m_labelFolder = (GTextField)GetChildAt(7);
            m_bgFolder = (GGraph)GetChildAt(8);
            m_folder = (GTextField)GetChildAt(9);
            m_btnMigrate = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(10);
            m_folderHint = (GTextField)GetChildAt(11);
            m_labelLogic = (GTextField)GetChildAt(12);
            m_bgLogic = (GGraph)GetChildAt(13);
            m_inputLogicFps = (GTextInput)GetChildAt(14);
            m_logicHint = (GTextField)GetChildAt(15);
            m_pluginTitle = (GTextField)GetChildAt(16);
            m_pluginHint = (GTextField)GetChildAt(17);
            m_checkParticles = (PackageAtrribute.UI_ButtonCheck)GetChildAt(18);
            m_checkVfx = (PackageAtrribute.UI_ButtonCheck)GetChildAt(19);
            m_checkSpine = (PackageAtrribute.UI_ButtonCheck)GetChildAt(20);
            m_checkAnimation = (PackageAtrribute.UI_ButtonCheck)GetChildAt(21);
            m_labelSources = (GTextField)GetChildAt(22);
            m_btnAddSource = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(23);
            m_sourceHint = (GTextField)GetChildAt(24);
            m_sources = (GList)GetChildAt(25);
            m_modFields = (GGroup)GetChildAt(26);
            m_btnSave = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(27);
        }
    }
}