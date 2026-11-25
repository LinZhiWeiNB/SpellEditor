/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_ItemDataSource : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "ItemDataSource";
        public Controller m_cRemove;

        public enum EcRemove
        {
            /// <summary>
            /// null
            /// <summary>
            可删除 = 0,
            /// <summary>
            /// null
            /// <summary>
            保留 = 1,
        }

        public EcRemove GetSelectedIndexEcRemove()
        {
            return (EcRemove)m_cRemove.selectedIndex;
        }

        public GGraph m_bg;
        public GTextField m_title;
        public GTextField m_kind;
        public GTextField m_state;
        public GTextField m_path;
        public PackageAtrribute.UI_ButtonGhost m_remove;
        public const string URL = "ui://mov3ykknmni02";

        public static UI_ItemDataSource CreateInstance()
        {
            return (UI_ItemDataSource)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cRemove = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_title = (GTextField)GetChildAt(1);
            m_kind = (GTextField)GetChildAt(2);
            m_state = (GTextField)GetChildAt(3);
            m_path = (GTextField)GetChildAt(4);
            m_remove = (PackageAtrribute.UI_ButtonGhost)GetChildAt(5);
        }
    }
}