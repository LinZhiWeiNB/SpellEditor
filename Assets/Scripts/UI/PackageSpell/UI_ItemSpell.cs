/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemSpell : GButton
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemSpell";
        public Controller m_cTone;

        public enum EcTone
        {
            /// <summary>
            /// null
            /// <summary>
            本源 = 0,
            /// <summary>
            /// null
            /// <summary>
            只读 = 1,
        }

        public EcTone GetSelectedIndexEcTone()
        {
            return (EcTone)m_cTone.selectedIndex;
        }

        public GGraph m_bg;
        public GTextField m_meta;
        public const string URL = "ui://skl4p8n2mni06";

        public static UI_ItemSpell CreateInstance()
        {
            return (UI_ItemSpell)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cTone = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_meta = (GTextField)GetChildAt(2);
        }
    }
}