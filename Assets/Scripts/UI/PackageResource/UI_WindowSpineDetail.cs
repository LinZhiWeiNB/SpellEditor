/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_WindowSpineDetail : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "WindowSpineDetail";
        public Controller m_cPage;

        public enum EcPage
        {
            /// <summary>
            /// null
            /// <summary>
            动作 = 0,
            /// <summary>
            /// null
            /// <summary>
            外观 = 1,
        }

        public EcPage GetSelectedIndexEcPage()
        {
            return (EcPage)m_cPage.selectedIndex;
        }

        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public PackageAtrribute.UI_ButtonPill m_btnMotion;
        public PackageAtrribute.UI_ButtonPill m_btnLook;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddTrigger;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddGroup;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddSlot;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddSkin;
        public GList m_groupHeads;
        public GList m_triggers;
        public GList m_slots;
        public GList m_groupHeadsLook;
        public GList m_skins;
        public PackageAtrribute.UI_ButtonGhost m_btnCancel;
        public PackageAtrribute.UI_ButtonPrimary m_btnDone;
        public const string URL = "ui://res4p8n2spn18";

        public static UI_WindowSpineDetail CreateInstance()
        {
            return (UI_WindowSpineDetail)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cPage = GetControllerAt(0);
            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_btnMotion = (PackageAtrribute.UI_ButtonPill)GetChildAt(3);
            m_btnLook = (PackageAtrribute.UI_ButtonPill)GetChildAt(4);
            m_btnAddTrigger = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(5);
            m_btnAddGroup = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(6);
            m_btnAddSlot = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(7);
            m_btnAddSkin = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(8);
            m_groupHeads = (GList)GetChildAt(9);
            m_triggers = (GList)GetChildAt(10);
            m_slots = (GList)GetChildAt(11);
            m_groupHeadsLook = (GList)GetChildAt(12);
            m_skins = (GList)GetChildAt(13);
            m_btnCancel = (PackageAtrribute.UI_ButtonGhost)GetChildAt(14);
            m_btnDone = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(15);
        }
    }
}