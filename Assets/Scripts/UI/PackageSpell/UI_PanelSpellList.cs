/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_PanelSpellList : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "PanelSpellList";
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
            显示 = 0,
            /// <summary>
            /// null
            /// <summary>
            隐藏 = 1,
        }

        public EcOwn GetSelectedIndexEcOwn()
        {
            return (EcOwn)m_cOwn.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_listPane;
        public GGraph m_split;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GGraph m_bgSearch;
        public GTextInput m_search;
        public PackageAtrribute.UI_ButtonCheck m_toggleOthers;
        public GTextField m_ownSection;
        public GList m_spells;
        public GTextField m_otherSection;
        public GList m_others;
        public GGroup m_otherSources;
        public PackageAtrribute.UI_ButtonPrimary m_btnAdd;
        public PackageAtrribute.UI_ButtonSecondary m_btnDelete;
        public PackageAtrribute.UI_ButtonSecondary m_btnCopy;
        public GTextField m_emptyHint;
        public GTextField m_basicTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnEdit;
        public GTextField m_labelId;
        public GTextField m_valueId;
        public GTextField m_labelActivation;
        public GTextField m_valueActivation;
        public GTextField m_labelDuration;
        public GTextField m_valueDuration;
        public GTextField m_labelCooldown;
        public GTextField m_valueCooldown;
        public GTextField m_labelAdmission;
        public GTextField m_valueAdmission;
        public GTextField m_deriveTitle;
        public GGraph m_deriveBlank;
        public GTextField m_unitTitle;
        public GGraph m_unitBlank;
        public GGroup m_detail;
        public const string URL = "ui://skl4p8n2mns02";

        public static UI_PanelSpellList CreateInstance()
        {
            return (UI_PanelSpellList)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cDetail = GetControllerAt(0);
            m_cOwn = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_listPane = (GGraph)GetChildAt(1);
            m_split = (GGraph)GetChildAt(2);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(3);
            m_title = (GTextField)GetChildAt(4);
            m_bgSearch = (GGraph)GetChildAt(5);
            m_search = (GTextInput)GetChildAt(6);
            m_toggleOthers = (PackageAtrribute.UI_ButtonCheck)GetChildAt(7);
            m_ownSection = (GTextField)GetChildAt(8);
            m_spells = (GList)GetChildAt(9);
            m_otherSection = (GTextField)GetChildAt(10);
            m_others = (GList)GetChildAt(11);
            m_otherSources = (GGroup)GetChildAt(12);
            m_btnAdd = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(13);
            m_btnDelete = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(14);
            m_btnCopy = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(15);
            m_emptyHint = (GTextField)GetChildAt(16);
            m_basicTitle = (GTextField)GetChildAt(17);
            m_btnEdit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(18);
            m_labelId = (GTextField)GetChildAt(19);
            m_valueId = (GTextField)GetChildAt(20);
            m_labelActivation = (GTextField)GetChildAt(21);
            m_valueActivation = (GTextField)GetChildAt(22);
            m_labelDuration = (GTextField)GetChildAt(23);
            m_valueDuration = (GTextField)GetChildAt(24);
            m_labelCooldown = (GTextField)GetChildAt(25);
            m_valueCooldown = (GTextField)GetChildAt(26);
            m_labelAdmission = (GTextField)GetChildAt(27);
            m_valueAdmission = (GTextField)GetChildAt(28);
            m_deriveTitle = (GTextField)GetChildAt(29);
            m_deriveBlank = (GGraph)GetChildAt(30);
            m_unitTitle = (GTextField)GetChildAt(31);
            m_unitBlank = (GGraph)GetChildAt(32);
            m_detail = (GGroup)GetChildAt(33);
        }
    }
}