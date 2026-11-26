/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemSlot : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemSlot";
        public Controller m_cOn;

        public enum EcOn
        {
            /// <summary>
            /// null
            /// <summary>
            否 = 0,
            /// <summary>
            /// null
            /// <summary>
            是 = 1,
        }

        public EcOn GetSelectedIndexEcOn()
        {
            return (EcOn)m_cOn.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_mark;
        public GTextField m_label;
        public GGraph m_inputBg;
        public GTextInput m_input;
        public PackageAtrribute.UI_ButtonGhost m_btnDelete;
        public const string URL = "ui://res4p8n2spn15";

        public static UI_ItemSlot CreateInstance()
        {
            return (UI_ItemSlot)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cOn = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_mark = (GGraph)GetChildAt(1);
            m_label = (GTextField)GetChildAt(2);
            m_inputBg = (GGraph)GetChildAt(3);
            m_input = (GTextInput)GetChildAt(4);
            m_btnDelete = (PackageAtrribute.UI_ButtonGhost)GetChildAt(5);
        }
    }
}