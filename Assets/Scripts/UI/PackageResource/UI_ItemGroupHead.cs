/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemGroupHead : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemGroupHead";
        public Controller m_cEdit;

        public enum EcEdit
        {
            /// <summary>
            /// null
            /// <summary>
            可改 = 0,
            /// <summary>
            /// null
            /// <summary>
            只读 = 1,
        }

        public EcEdit GetSelectedIndexEcEdit()
        {
            return (EcEdit)m_cEdit.selectedIndex;
        }

        public GGraph m_inputBg;
        public GTextInput m_input;
        public GTextField m_label;
        public PackageAtrribute.UI_ButtonGhost m_btnDelete;
        public const string URL = "ui://res4p8n2spn14";

        public static UI_ItemGroupHead CreateInstance()
        {
            return (UI_ItemGroupHead)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cEdit = GetControllerAt(0);
            m_inputBg = (GGraph)GetChildAt(0);
            m_input = (GTextInput)GetChildAt(1);
            m_label = (GTextField)GetChildAt(2);
            m_btnDelete = (PackageAtrribute.UI_ButtonGhost)GetChildAt(3);
        }
    }
}