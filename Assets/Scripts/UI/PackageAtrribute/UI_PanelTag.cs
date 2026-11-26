/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_PanelTag : GComponent
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "PanelTag";
        public Controller m_cOthers;

        public enum EcOthers
        {
            /// <summary>
            /// null
            /// <summary>
            显示 = 0,
            /// <summary>
            /// null
            /// <summary>
            隐藏 = 1,
        }

        public EcOthers GetSelectedIndexEcOthers()
        {
            return (EcOthers)m_cOthers.selectedIndex;
        }

        public Controller m_cFilter;

        public enum EcFilter
        {
            /// <summary>
            /// null
            /// <summary>
            全部 = 0,
            /// <summary>
            /// null
            /// <summary>
            根节点 = 1,
            /// <summary>
            /// null
            /// <summary>
            子标签 = 2,
        }

        public EcFilter GetSelectedIndexEcFilter()
        {
            return (EcFilter)m_cFilter.selectedIndex;
        }

        public Controller m_cInspect;

        public enum EcInspect
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

        public EcInspect GetSelectedIndexEcInspect()
        {
            return (EcInspect)m_cInspect.selectedIndex;
        }

        public Controller m_cEdit;

        public enum EcEdit
        {
            /// <summary>
            /// null
            /// <summary>
            可编辑 = 0,
            /// <summary>
            /// null
            /// <summary>
            只读 = 1,
        }

        public EcEdit GetSelectedIndexEcEdit()
        {
            return (EcEdit)m_cEdit.selectedIndex;
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
            警告 = 1,
        }

        public EcMessage GetSelectedIndexEcMessage()
        {
            return (EcMessage)m_cMessage.selectedIndex;
        }

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GGraph m_leftBg;
        public GTextField m_modSource;
        public GTextField m_modSourceSub;
        public GGraph m_searchBg;
        public GTextInput m_search;
        public UI_ButtonPill m_filterAll;
        public UI_ButtonPill m_filterRoot;
        public UI_ButtonPill m_filterChild;
        public GGroup m_filters;
        public UI_ButtonCheck m_toggleOthers;
        public GTextField m_modSection;
        public GList m_tags;
        public GTextField m_otherSection;
        public GList m_others;
        public GGroup m_otherSources;
        public UI_ButtonPrimary m_btnAdd;
        public GGroup m_listMod;
        public GGraph m_split;
        public GTextField m_emptyHint;
        public GTextField m_mTitle;
        public GTextField m_mMeta;
        public UI_ButtonGhost m_mDelete;
        public GTextField m_mLoaded;
        public GTextField m_mNameLabel;
        public GGraph m_mNameBg;
        public GTextInput m_mNameInput;
        public GTextField m_mIdLabel;
        public GGraph m_mIdBg;
        public GTextInput m_mIdInput;
        public GTextField m_mIdHint;
        public GTextField m_mWarn;
        public GTextField m_mParentLabel;
        public GTextField m_mPath;
        public GList m_parents;
        public GGroup m_inspector;
        public GGroup m_catalog;
        public const string URL = "ui://m5lrgzhjg8btd";

        public static UI_PanelTag CreateInstance()
        {
            return (UI_PanelTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cOthers = GetControllerAt(0);
            m_cFilter = GetControllerAt(1);
            m_cInspect = GetControllerAt(2);
            m_cEdit = GetControllerAt(3);
            m_cMessage = GetControllerAt(4);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_leftBg = (GGraph)GetChildAt(2);
            m_modSource = (GTextField)GetChildAt(3);
            m_modSourceSub = (GTextField)GetChildAt(4);
            m_searchBg = (GGraph)GetChildAt(5);
            m_search = (GTextInput)GetChildAt(6);
            m_filterAll = (UI_ButtonPill)GetChildAt(7);
            m_filterRoot = (UI_ButtonPill)GetChildAt(8);
            m_filterChild = (UI_ButtonPill)GetChildAt(9);
            m_filters = (GGroup)GetChildAt(10);
            m_toggleOthers = (UI_ButtonCheck)GetChildAt(11);
            m_modSection = (GTextField)GetChildAt(12);
            m_tags = (GList)GetChildAt(13);
            m_otherSection = (GTextField)GetChildAt(14);
            m_others = (GList)GetChildAt(15);
            m_otherSources = (GGroup)GetChildAt(16);
            m_btnAdd = (UI_ButtonPrimary)GetChildAt(17);
            m_listMod = (GGroup)GetChildAt(18);
            m_split = (GGraph)GetChildAt(19);
            m_emptyHint = (GTextField)GetChildAt(20);
            m_mTitle = (GTextField)GetChildAt(21);
            m_mMeta = (GTextField)GetChildAt(22);
            m_mDelete = (UI_ButtonGhost)GetChildAt(23);
            m_mLoaded = (GTextField)GetChildAt(24);
            m_mNameLabel = (GTextField)GetChildAt(25);
            m_mNameBg = (GGraph)GetChildAt(26);
            m_mNameInput = (GTextInput)GetChildAt(27);
            m_mIdLabel = (GTextField)GetChildAt(28);
            m_mIdBg = (GGraph)GetChildAt(29);
            m_mIdInput = (GTextInput)GetChildAt(30);
            m_mIdHint = (GTextField)GetChildAt(31);
            m_mWarn = (GTextField)GetChildAt(32);
            m_mParentLabel = (GTextField)GetChildAt(33);
            m_mPath = (GTextField)GetChildAt(34);
            m_parents = (GList)GetChildAt(35);
            m_inspector = (GGroup)GetChildAt(36);
            m_catalog = (GGroup)GetChildAt(37);
        }
    }
}