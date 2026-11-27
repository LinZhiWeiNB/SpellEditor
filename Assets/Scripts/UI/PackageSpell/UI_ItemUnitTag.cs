/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemUnitTag : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemUnitTag";
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

        public GTextField m_title;
        public PackageAtrribute.UI_ButtonCheck m_castLink;
        public PackageAtrribute.UI_ButtonGhost m_btnEdit;
        public PackageAtrribute.UI_ButtonGhost m_btnRemove;
        public const string URL = "ui://skl4p8n2utag1";

        public static UI_ItemUnitTag CreateInstance()
        {
            return (UI_ItemUnitTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cMode = GetControllerAt(0);
            m_title = (GTextField)GetChildAt(0);
            m_castLink = (PackageAtrribute.UI_ButtonCheck)GetChildAt(1);
            m_btnEdit = (PackageAtrribute.UI_ButtonGhost)GetChildAt(2);
            m_btnRemove = (PackageAtrribute.UI_ButtonGhost)GetChildAt(3);
        }
    }
}