/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_PanelUnitEdit : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "PanelUnitEdit";
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

        public Controller m_cOrigin;

        public enum EcOrigin
        {
            /// <summary>
            /// null
            /// <summary>
            本项目 = 0,
            /// <summary>
            /// null
            /// <summary>
            差异 = 1,
        }

        public EcOrigin GetSelectedIndexEcOrigin()
        {
            return (EcOrigin)m_cOrigin.selectedIndex;
        }

        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_overrideHint;
        public GTextField m_labelId;
        public GTextField m_valueId;
        public GTextField m_labelPreset;
        public GList m_presets;
        public GTextField m_labelValues;
        public GList m_values;
        public GTextField m_labelActive;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddActive;
        public GList m_activeSpells;
        public GTextField m_labelPassive;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddPassive;
        public GList m_passiveSpells;
        public GTextField m_labelSpine;
        public GTextField m_spineHint;
        public PackageAtrribute.UI_ButtonSecondary m_btnSpine;
        public GGroup m_spineEdit;
        public const string URL = "ui://unt4p8n2mnu02";

        public static UI_PanelUnitEdit CreateInstance()
        {
            return (UI_PanelUnitEdit)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cSpine = GetControllerAt(0);
            m_cOrigin = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_overrideHint = (GTextField)GetChildAt(3);
            m_labelId = (GTextField)GetChildAt(4);
            m_valueId = (GTextField)GetChildAt(5);
            m_labelPreset = (GTextField)GetChildAt(6);
            m_presets = (GList)GetChildAt(7);
            m_labelValues = (GTextField)GetChildAt(8);
            m_values = (GList)GetChildAt(9);
            m_labelActive = (GTextField)GetChildAt(10);
            m_btnAddActive = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(11);
            m_activeSpells = (GList)GetChildAt(12);
            m_labelPassive = (GTextField)GetChildAt(13);
            m_btnAddPassive = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(14);
            m_passiveSpells = (GList)GetChildAt(15);
            m_labelSpine = (GTextField)GetChildAt(16);
            m_spineHint = (GTextField)GetChildAt(17);
            m_btnSpine = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(18);
            m_spineEdit = (GGroup)GetChildAt(19);
        }
    }
}