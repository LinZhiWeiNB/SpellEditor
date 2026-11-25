/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_WindowMain : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "WindowMain";
        public Controller m_cPage;

        public enum EcPage
        {
            /// <summary>
            /// null
            /// <summary>
            入口 = 0,
            /// <summary>
            /// null
            /// <summary>
            属性 = 1,
            /// <summary>
            /// null
            /// <summary>
            单位 = 2,
        }

        public EcPage GetSelectedIndexEcPage()
        {
            return (EcPage)m_cPage.selectedIndex;
        }

        public GGraph m_bg;
        public UI_ButtonModule m_unit;
        public UI_ButtonModule m_attribute;
        public UI_ButtonModule m_tag;
        public UI_ButtonModule m_spell;
        public GGroup m_entries;
        public PackageAtrribute.UI_PanelAttribute m_attributeEditor;
        public PackageAtrribute.UI_PanelUnit m_unitEditor;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public const string URL = "ui://mov3ykknmnw01";

        public static UI_WindowMain CreateInstance()
        {
            return (UI_WindowMain)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cPage = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_unit = (UI_ButtonModule)GetChildAt(1);
            m_attribute = (UI_ButtonModule)GetChildAt(2);
            m_tag = (UI_ButtonModule)GetChildAt(3);
            m_spell = (UI_ButtonModule)GetChildAt(4);
            m_entries = (GGroup)GetChildAt(5);
            m_attributeEditor = (PackageAtrribute.UI_PanelAttribute)GetChildAt(6);
            m_unitEditor = (PackageAtrribute.UI_PanelUnit)GetChildAt(7);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(8);
        }
    }
}