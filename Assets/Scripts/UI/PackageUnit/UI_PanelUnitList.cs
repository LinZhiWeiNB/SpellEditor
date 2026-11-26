/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_PanelUnitList : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "PanelUnitList";
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

        public GGraph m_bg;
        public GGraph m_listPane;
        public GGraph m_split;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GGraph m_bgSearch;
        public GTextInput m_search;
        public GList m_units;
        public PackageAtrribute.UI_ButtonPrimary m_btnAdd;
        public PackageAtrribute.UI_ButtonSecondary m_btnDelete;
        public PackageAtrribute.UI_ButtonSecondary m_btnCopy;
        public GTextField m_emptyHint;
        public GTextField m_basicTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnEdit;
        public GTextField m_labelId;
        public GTextField m_valueId;
        public GTextField m_valueSource;
        public GTextField m_labelPreset;
        public GList m_presets;
        public GTextField m_labelValues;
        public GList m_values;
        public GTextField m_labelActive;
        public GList m_activeSpells;
        public GTextField m_labelPassive;
        public GList m_passiveSpells;
        public GTextField m_labelSpine;
        public GTextField m_valueSpine;
        public GTextField m_previewLabel;
        public GGraph m_preview;
        public PackageAtrribute.UI_ButtonSecondary m_btnOpenResource;
        public GGroup m_spineInfo;
        public GGroup m_detail;
        public const string URL = "ui://unt4p8n2mnu01";

        public static UI_PanelUnitList CreateInstance()
        {
            return (UI_PanelUnitList)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cDetail = GetControllerAt(0);
            m_cSpine = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_listPane = (GGraph)GetChildAt(1);
            m_split = (GGraph)GetChildAt(2);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(3);
            m_title = (GTextField)GetChildAt(4);
            m_bgSearch = (GGraph)GetChildAt(5);
            m_search = (GTextInput)GetChildAt(6);
            m_units = (GList)GetChildAt(7);
            m_btnAdd = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(8);
            m_btnDelete = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(9);
            m_btnCopy = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(10);
            m_emptyHint = (GTextField)GetChildAt(11);
            m_basicTitle = (GTextField)GetChildAt(12);
            m_btnEdit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(13);
            m_labelId = (GTextField)GetChildAt(14);
            m_valueId = (GTextField)GetChildAt(15);
            m_valueSource = (GTextField)GetChildAt(16);
            m_labelPreset = (GTextField)GetChildAt(17);
            m_presets = (GList)GetChildAt(18);
            m_labelValues = (GTextField)GetChildAt(19);
            m_values = (GList)GetChildAt(20);
            m_labelActive = (GTextField)GetChildAt(21);
            m_activeSpells = (GList)GetChildAt(22);
            m_labelPassive = (GTextField)GetChildAt(23);
            m_passiveSpells = (GList)GetChildAt(24);
            m_labelSpine = (GTextField)GetChildAt(25);
            m_valueSpine = (GTextField)GetChildAt(26);
            m_previewLabel = (GTextField)GetChildAt(27);
            m_preview = (GGraph)GetChildAt(28);
            m_btnOpenResource = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(29);
            m_spineInfo = (GGroup)GetChildAt(30);
            m_detail = (GGroup)GetChildAt(31);
        }
    }
}