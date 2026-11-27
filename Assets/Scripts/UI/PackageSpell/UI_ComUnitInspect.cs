/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComUnitInspect : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComUnitInspect";
        public Controller m_cMode;

        public enum EcMode
        {
            /// <summary>
            /// null
            /// <summary>
            编辑 = 0,
            /// <summary>
            /// null
            /// <summary>
            预览 = 1,
        }

        public EcMode GetSelectedIndexEcMode()
        {
            return (EcMode)m_cMode.selectedIndex;
        }

        public GGraph m_card;
        public GTextField m_labelTags;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddTag;
        public GTextField m_empty;
        public GList m_tags;
        public GTextField m_report;
        public const string URL = "ui://skl4p8n2unit1";

        public static UI_ComUnitInspect CreateInstance()
        {
            return (UI_ComUnitInspect)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cMode = GetControllerAt(0);
            m_card = (GGraph)GetChildAt(0);
            m_labelTags = (GTextField)GetChildAt(1);
            m_btnAddTag = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(2);
            m_empty = (GTextField)GetChildAt(3);
            m_tags = (GList)GetChildAt(4);
            m_report = (GTextField)GetChildAt(5);
        }
    }
}