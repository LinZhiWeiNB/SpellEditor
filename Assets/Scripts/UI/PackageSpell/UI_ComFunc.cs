/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComFunc : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComFunc";
        public Controller m_cSpine;

        public enum EcSpine
        {
            /// <summary>
            /// null
            /// <summary>
            关 = 0,
            /// <summary>
            /// null
            /// <summary>
            开 = 1,
        }

        public EcSpine GetSelectedIndexEcSpine()
        {
            return (EcSpine)m_cSpine.selectedIndex;
        }

        public GGraph m_funcPane;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_funcUnit;
        public GGraph m_bgUnit;
        public GTextField m_unitName;
        public PackageAtrribute.UI_ButtonSecondary m_btnSwitchUnit;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddDummy;
        public GTextField m_funcSkill;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddKey;
        public PackageAtrribute.UI_ButtonSecondary m_btnDeleteKey;
        public GTextField m_funcSpine;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddClip;
        public GGroup m_spineBlock;
        public const string URL = "ui://skl4p8n2func01";

        public static UI_ComFunc CreateInstance()
        {
            return (UI_ComFunc)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cSpine = GetControllerAt(0);
            m_funcPane = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_funcUnit = (GTextField)GetChildAt(3);
            m_bgUnit = (GGraph)GetChildAt(4);
            m_unitName = (GTextField)GetChildAt(5);
            m_btnSwitchUnit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(6);
            m_btnAddDummy = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(7);
            m_funcSkill = (GTextField)GetChildAt(8);
            m_btnAddKey = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(9);
            m_btnDeleteKey = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(10);
            m_funcSpine = (GTextField)GetChildAt(11);
            m_btnAddClip = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(12);
            m_spineBlock = (GGroup)GetChildAt(13);
        }
    }
}