/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_PanelAttribute : GComponent
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "PanelAttribute";
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
            资源 = 1,
            /// <summary>
            /// null
            /// <summary>
            数值 = 2,
            /// <summary>
            /// null
            /// <summary>
            标记 = 3,
        }

        public EcFilter GetSelectedIndexEcFilter()
        {
            return (EcFilter)m_cFilter.selectedIndex;
        }

        public Controller m_cType;

        public enum EcType
        {
            /// <summary>
            /// null
            /// <summary>
            资源 = 0,
            /// <summary>
            /// null
            /// <summary>
            数值 = 1,
            /// <summary>
            /// null
            /// <summary>
            标记 = 2,
        }

        public EcType GetSelectedIndexEcType()
        {
            return (EcType)m_cType.selectedIndex;
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

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_modSource;
        public UI_ButtonPrimary m_btnAdd;
        public GGraph m_leftBg;
        public GGraph m_searchBg;
        public GTextInput m_search;
        public UI_ButtonPill m_filterAll;
        public UI_ButtonPill m_filterResource;
        public UI_ButtonPill m_filterNumeric;
        public UI_ButtonPill m_filterMarker;
        public UI_ButtonCheck m_toggleOthers;
        public GTextField m_modSection;
        public GList m_attrs;
        public GTextField m_otherSection;
        public GList m_others;
        public GGroup m_otherSources;
        public GGraph m_cardIdentity;
        public GTextField m_mTitle;
        public UI_ButtonGhost m_mDelete;
        public GTextField m_mNameLabel;
        public GGraph m_mNameBg;
        public GTextInput m_mNameInput;
        public GTextField m_mIdLabel;
        public GGraph m_mIdBg;
        public GTextInput m_mIdInput;
        public GTextField m_mIdHint;
        public GGraph m_cardType;
        public GTextField m_mTypeLabel;
        public UI_ButtonRadio m_mTypeResource;
        public GTextField m_mTypeResourceDesc;
        public UI_ButtonRadio m_mTypeNumeric;
        public GTextField m_mTypeNumericDesc;
        public UI_ButtonRadio m_mTypeMarker;
        public GTextField m_mTypeMarkerDesc;
        public GGraph m_cardScale;
        public GTextField m_mConstraint;
        public UI_ButtonToggle m_mScale;
        public GTextField m_mScaleLabel;
        public GTextField m_mScaleDesc;
        public GGroup m_modScale;
        public GGraph m_cardNonNeg;
        public GTextField m_mConstraint2;
        public UI_ButtonToggle m_mNonNeg;
        public GTextField m_mNonNegLabel;
        public GTextField m_mNonNegDesc;
        public GGroup m_modNonNeg;
        public GGroup m_inspector;
        public const string URL = "ui://m5lrgzhjg8bt7";

        public static UI_PanelAttribute CreateInstance()
        {
            return (UI_PanelAttribute)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cOthers = GetControllerAt(0);
            m_cFilter = GetControllerAt(1);
            m_cType = GetControllerAt(2);
            m_cInspect = GetControllerAt(3);
            m_cEdit = GetControllerAt(4);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_modSource = (GTextField)GetChildAt(2);
            m_btnAdd = (UI_ButtonPrimary)GetChildAt(3);
            m_leftBg = (GGraph)GetChildAt(4);
            m_searchBg = (GGraph)GetChildAt(5);
            m_search = (GTextInput)GetChildAt(6);
            m_filterAll = (UI_ButtonPill)GetChildAt(7);
            m_filterResource = (UI_ButtonPill)GetChildAt(8);
            m_filterNumeric = (UI_ButtonPill)GetChildAt(9);
            m_filterMarker = (UI_ButtonPill)GetChildAt(10);
            m_toggleOthers = (UI_ButtonCheck)GetChildAt(11);
            m_modSection = (GTextField)GetChildAt(12);
            m_attrs = (GList)GetChildAt(13);
            m_otherSection = (GTextField)GetChildAt(14);
            m_others = (GList)GetChildAt(15);
            m_otherSources = (GGroup)GetChildAt(16);
            m_cardIdentity = (GGraph)GetChildAt(17);
            m_mTitle = (GTextField)GetChildAt(18);
            m_mDelete = (UI_ButtonGhost)GetChildAt(19);
            m_mNameLabel = (GTextField)GetChildAt(20);
            m_mNameBg = (GGraph)GetChildAt(21);
            m_mNameInput = (GTextInput)GetChildAt(22);
            m_mIdLabel = (GTextField)GetChildAt(23);
            m_mIdBg = (GGraph)GetChildAt(24);
            m_mIdInput = (GTextInput)GetChildAt(25);
            m_mIdHint = (GTextField)GetChildAt(26);
            m_cardType = (GGraph)GetChildAt(27);
            m_mTypeLabel = (GTextField)GetChildAt(28);
            m_mTypeResource = (UI_ButtonRadio)GetChildAt(29);
            m_mTypeResourceDesc = (GTextField)GetChildAt(30);
            m_mTypeNumeric = (UI_ButtonRadio)GetChildAt(31);
            m_mTypeNumericDesc = (GTextField)GetChildAt(32);
            m_mTypeMarker = (UI_ButtonRadio)GetChildAt(33);
            m_mTypeMarkerDesc = (GTextField)GetChildAt(34);
            m_cardScale = (GGraph)GetChildAt(35);
            m_mConstraint = (GTextField)GetChildAt(36);
            m_mScale = (UI_ButtonToggle)GetChildAt(37);
            m_mScaleLabel = (GTextField)GetChildAt(38);
            m_mScaleDesc = (GTextField)GetChildAt(39);
            m_modScale = (GGroup)GetChildAt(40);
            m_cardNonNeg = (GGraph)GetChildAt(41);
            m_mConstraint2 = (GTextField)GetChildAt(42);
            m_mNonNeg = (UI_ButtonToggle)GetChildAt(43);
            m_mNonNegLabel = (GTextField)GetChildAt(44);
            m_mNonNegDesc = (GTextField)GetChildAt(45);
            m_modNonNeg = (GGroup)GetChildAt(46);
            m_inspector = (GGroup)GetChildAt(47);
        }
    }
}