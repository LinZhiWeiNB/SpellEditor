/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_PanelResource : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "PanelResource";
        public Controller m_cDetail;

        public enum EcDetail
        {
            /// <summary>
            /// null
            /// <summary>
            未选中 = 0,
            /// <summary>
            /// null
            /// <summary>
            已选中 = 1,
        }

        public EcDetail GetSelectedIndexEcDetail()
        {
            return (EcDetail)m_cDetail.selectedIndex;
        }

        public Controller m_cOwn;

        public enum EcOwn
        {
            /// <summary>
            /// null
            /// <summary>
            本源 = 0,
            /// <summary>
            /// null
            /// <summary>
            只读 = 1,
        }

        public EcOwn GetSelectedIndexEcOwn()
        {
            return (EcOwn)m_cOwn.selectedIndex;
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
            提示 = 1,
            /// <summary>
            /// null
            /// <summary>
            警告 = 2,
        }

        public EcMessage GetSelectedIndexEcMessage()
        {
            return (EcMessage)m_cMessage.selectedIndex;
        }

        public Controller m_cPreview;

        public enum EcPreview
        {
            /// <summary>
            /// null
            /// <summary>
            无 = 0,
            /// <summary>
            /// null
            /// <summary>
            有 = 1,
        }

        public EcPreview GetSelectedIndexEcPreview()
        {
            return (EcPreview)m_cPreview.selectedIndex;
        }

        public Controller m_cPick;

        public enum EcPick
        {
            /// <summary>
            /// null
            /// <summary>
            关 = 0,
            /// <summary>
            /// null
            /// <summary>
            类型 = 1,
            /// <summary>
            /// null
            /// <summary>
            同名 = 2,
            /// <summary>
            /// null
            /// <summary>
            目录 = 3,
            /// <summary>
            /// null
            /// <summary>
            删除 = 4,
        }

        public EcPick GetSelectedIndexEcPick()
        {
            return (EcPick)m_cPick.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_listPane;
        public GGraph m_split;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GGraph m_bgSearch;
        public GTextInput m_search;
        public GList m_resources;
        public GTextField m_status;
        public PackageAtrribute.UI_ButtonPrimary m_btnImport;
        public PackageAtrribute.UI_ButtonSecondary m_btnDelete;
        public GTextField m_emptyHint;
        public GTextField m_basicTitle;
        public GTextField m_labelName;
        public GTextField m_valueName;
        public GTextField m_labelKind;
        public GTextField m_valueKind;
        public GTextField m_labelSource;
        public GTextField m_valueSource;
        public GTextField m_labelFolder;
        public GTextField m_valueFolder;
        public GGraph m_folderBg;
        public GTextInput m_inputFolder;
        public PackageAtrribute.UI_ButtonSecondary m_btnMove;
        public GTextField m_folderHint;
        public GGroup m_ownFolder;
        public GTextField m_labelFiles;
        public GList m_files;
        public GTextField m_labelRefs;
        public GList m_refs;
        public GTextField m_labelPreview;
        public GGraph m_previewFrame;
        public GTextField m_previewHint;
        public GLoader m_preview;
        public GTextField m_previewNote;
        public GGroup m_detail;
        public GGraph m_pickMask;
        public GGraph m_pickCard;
        public GTextField m_pickTitle;
        public GTextField m_pickHint;
        public GList m_kinds;
        public PackageAtrribute.UI_ButtonSecondary m_btnCancel;
        public GGraph m_pathBg;
        public GTextInput m_inputPath;
        public PackageAtrribute.UI_ButtonPrimary m_btnPlace;
        public GGroup m_pick;
        public const string URL = "ui://res4p8n2mnr01";

        public static UI_PanelResource CreateInstance()
        {
            return (UI_PanelResource)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cDetail = GetControllerAt(0);
            m_cOwn = GetControllerAt(1);
            m_cMessage = GetControllerAt(2);
            m_cPreview = GetControllerAt(3);
            m_cPick = GetControllerAt(4);
            m_bg = (GGraph)GetChildAt(0);
            m_listPane = (GGraph)GetChildAt(1);
            m_split = (GGraph)GetChildAt(2);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(3);
            m_title = (GTextField)GetChildAt(4);
            m_bgSearch = (GGraph)GetChildAt(5);
            m_search = (GTextInput)GetChildAt(6);
            m_resources = (GList)GetChildAt(7);
            m_status = (GTextField)GetChildAt(8);
            m_btnImport = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(9);
            m_btnDelete = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(10);
            m_emptyHint = (GTextField)GetChildAt(11);
            m_basicTitle = (GTextField)GetChildAt(12);
            m_labelName = (GTextField)GetChildAt(13);
            m_valueName = (GTextField)GetChildAt(14);
            m_labelKind = (GTextField)GetChildAt(15);
            m_valueKind = (GTextField)GetChildAt(16);
            m_labelSource = (GTextField)GetChildAt(17);
            m_valueSource = (GTextField)GetChildAt(18);
            m_labelFolder = (GTextField)GetChildAt(19);
            m_valueFolder = (GTextField)GetChildAt(20);
            m_folderBg = (GGraph)GetChildAt(21);
            m_inputFolder = (GTextInput)GetChildAt(22);
            m_btnMove = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(23);
            m_folderHint = (GTextField)GetChildAt(24);
            m_ownFolder = (GGroup)GetChildAt(25);
            m_labelFiles = (GTextField)GetChildAt(26);
            m_files = (GList)GetChildAt(27);
            m_labelRefs = (GTextField)GetChildAt(28);
            m_refs = (GList)GetChildAt(29);
            m_labelPreview = (GTextField)GetChildAt(30);
            m_previewFrame = (GGraph)GetChildAt(31);
            m_previewHint = (GTextField)GetChildAt(32);
            m_preview = (GLoader)GetChildAt(33);
            m_previewNote = (GTextField)GetChildAt(34);
            m_detail = (GGroup)GetChildAt(35);
            m_pickMask = (GGraph)GetChildAt(36);
            m_pickCard = (GGraph)GetChildAt(37);
            m_pickTitle = (GTextField)GetChildAt(38);
            m_pickHint = (GTextField)GetChildAt(39);
            m_kinds = (GList)GetChildAt(40);
            m_btnCancel = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(41);
            m_pathBg = (GGraph)GetChildAt(42);
            m_inputPath = (GTextInput)GetChildAt(43);
            m_btnPlace = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(44);
            m_pick = (GGroup)GetChildAt(45);
        }
    }
}