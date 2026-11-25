/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ItemInclude : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ItemInclude";
        public Controller m_cLock;

        public enum EcLock
        {
            /// <summary>
            /// null
            /// <summary>
            可改 = 0,
            /// <summary>
            /// null
            /// <summary>
            锁定 = 1,
        }

        public EcLock GetSelectedIndexEcLock()
        {
            return (EcLock)m_cLock.selectedIndex;
        }

        public GGraph m_box;
        public GGraph m_mark;
        public GTextField m_kind;
        public GTextField m_source;
        public const string URL = "ui://m5lrgzhjg8btb";

        public static UI_ItemInclude CreateInstance()
        {
            return (UI_ItemInclude)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cLock = GetControllerAt(1);
            m_box = (GGraph)GetChildAt(0);
            m_mark = (GGraph)GetChildAt(1);
            m_kind = (GTextField)GetChildAt(3);
            m_source = (GTextField)GetChildAt(4);
        }
    }
}