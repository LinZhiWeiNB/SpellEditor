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

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_modSource;
        public UI_ButtonPrimary m_btnAdd;
        public UI_ButtonCheck m_toggleOthers;
        public GTextField m_status;
        public GGraph m_searchBg;
        public GTextInput m_search;
        public GGraph m_leftBg;
        public GList m_tags;
        public GGraph m_rightBg;
        public GList m_others;
        public const string URL = "ui://m5lrgzhjg8btd";

        public static UI_PanelTag CreateInstance()
        {
            return (UI_PanelTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cOthers = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_modSource = (GTextField)GetChildAt(2);
            m_btnAdd = (UI_ButtonPrimary)GetChildAt(3);
            m_toggleOthers = (UI_ButtonCheck)GetChildAt(4);
            m_status = (GTextField)GetChildAt(5);
            m_searchBg = (GGraph)GetChildAt(6);
            m_search = (GTextInput)GetChildAt(7);
            m_leftBg = (GGraph)GetChildAt(8);
            m_tags = (GList)GetChildAt(9);
            m_rightBg = (GGraph)GetChildAt(10);
            m_others = (GList)GetChildAt(11);
        }
    }
}