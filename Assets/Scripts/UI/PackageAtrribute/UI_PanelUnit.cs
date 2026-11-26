/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_PanelUnit : GComponent
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "PanelUnit";
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

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_pgTitle;
        public UI_ButtonPrimary m_btnAdd;
        public GGraph m_pLeft;
        public UI_ButtonCheck m_toggleOthers;
        public GTextField m_modSection;
        public GList m_units;
        public GTextField m_otherSection;
        public GList m_others;
        public GGroup m_otherSources;
        public GTextField m_emptyHint;
        public GGraph m_cardName;
        public GTextField m_pgNameLabel;
        public GGraph m_pgNameBg;
        public GTextInput m_pgName;
        public GGraph m_cardAttrs;
        public GTextField m_pgHead;
        public UI_ButtonSecondary m_btnAddAttribute;
        public GList m_included;
        public GGroup m_detail;
        public const string URL = "ui://m5lrgzhjg8btc";

        public static UI_PanelUnit CreateInstance()
        {
            return (UI_PanelUnit)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cOthers = GetControllerAt(0);
            m_cInspect = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_pgTitle = (GTextField)GetChildAt(2);
            m_btnAdd = (UI_ButtonPrimary)GetChildAt(3);
            m_pLeft = (GGraph)GetChildAt(4);
            m_toggleOthers = (UI_ButtonCheck)GetChildAt(5);
            m_modSection = (GTextField)GetChildAt(6);
            m_units = (GList)GetChildAt(7);
            m_otherSection = (GTextField)GetChildAt(8);
            m_others = (GList)GetChildAt(9);
            m_otherSources = (GGroup)GetChildAt(10);
            m_emptyHint = (GTextField)GetChildAt(11);
            m_cardName = (GGraph)GetChildAt(12);
            m_pgNameLabel = (GTextField)GetChildAt(13);
            m_pgNameBg = (GGraph)GetChildAt(14);
            m_pgName = (GTextInput)GetChildAt(15);
            m_cardAttrs = (GGraph)GetChildAt(16);
            m_pgHead = (GTextField)GetChildAt(17);
            m_btnAddAttribute = (UI_ButtonSecondary)GetChildAt(18);
            m_included = (GList)GetChildAt(19);
            m_detail = (GGroup)GetChildAt(20);
        }
    }
}