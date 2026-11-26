/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_WindowMain : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "WindowMain";
        public Controller m_cExport;

        public enum EcExport
        {
            /// <summary>
            /// null
            /// <summary>
            空 = 0,
            /// <summary>
            /// null
            /// <summary>
            成功 = 1,
            /// <summary>
            /// null
            /// <summary>
            警告 = 2,
        }

        public EcExport GetSelectedIndexEcExport()
        {
            return (EcExport)m_cExport.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_header;
        public GTextField m_brand;
        public GTextField m_projectName;
        public GGraph m_divUnit;
        public GTextField m_unitTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnUnit;
        public PackageAtrribute.UI_ButtonSecondary m_btnUnitPreset;
        public PackageAtrribute.UI_ButtonSecondary m_btnAttribute;
        public GGraph m_divSkill;
        public GTextField m_skillTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnTag;
        public PackageAtrribute.UI_ButtonSecondary m_btnSpell;
        public PackageAtrribute.UI_ButtonSecondary m_btnBuff;
        public GGraph m_divResource;
        public GTextField m_resourceTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnResources;
        public PackageAtrribute.UI_ButtonSecondary m_btnPlugins;
        public GGraph m_divProject;
        public GTextField m_projectTitle;
        public PackageAtrribute.UI_ButtonSecondary m_btnProject;
        public PackageAtrribute.UI_ButtonSecondary m_btnExportSettings;
        public PackageAtrribute.UI_ButtonPrimary m_btnExportAll;
        public GTextField m_exportStatus;
        public GGraph m_divEnd;
        public GGroup m_hub;
        public const string URL = "ui://mov3ykknmnw01";

        public static UI_WindowMain CreateInstance()
        {
            return (UI_WindowMain)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cExport = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_header = (GGraph)GetChildAt(1);
            m_brand = (GTextField)GetChildAt(2);
            m_projectName = (GTextField)GetChildAt(3);
            m_divUnit = (GGraph)GetChildAt(4);
            m_unitTitle = (GTextField)GetChildAt(5);
            m_btnUnit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(6);
            m_btnUnitPreset = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(7);
            m_btnAttribute = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(8);
            m_divSkill = (GGraph)GetChildAt(9);
            m_skillTitle = (GTextField)GetChildAt(10);
            m_btnTag = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(11);
            m_btnSpell = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(12);
            m_btnBuff = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(13);
            m_divResource = (GGraph)GetChildAt(14);
            m_resourceTitle = (GTextField)GetChildAt(15);
            m_btnResources = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(16);
            m_btnPlugins = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(17);
            m_divProject = (GGraph)GetChildAt(18);
            m_projectTitle = (GTextField)GetChildAt(19);
            m_btnProject = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(20);
            m_btnExportSettings = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(21);
            m_btnExportAll = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(22);
            m_exportStatus = (GTextField)GetChildAt(23);
            m_divEnd = (GGraph)GetChildAt(24);
            m_hub = (GGroup)GetChildAt(25);
        }
    }
}