/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ItemAttr : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ItemAttr";
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
        public GGraph m_bar;
        public GTextField m_type;
        public GTextField m_meta;
        public const string URL = "ui://m5lrgzhjg8bt9";

        public static UI_ItemAttr CreateInstance()
        {
            return (UI_ItemAttr)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cTone = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_bar = (GGraph)GetChildAt(1);
            m_type = (GTextField)GetChildAt(3);
            m_meta = (GTextField)GetChildAt(4);
        }
    }
}