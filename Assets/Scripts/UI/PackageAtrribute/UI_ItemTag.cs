/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ItemTag : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ItemTag";
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

        public Controller m_cInput;

        public enum EcInput
        {
            /// <summary>
            /// null
            /// <summary>
            文本 = 0,
            /// <summary>
            /// null
            /// <summary>
            输入 = 1,
        }

        public EcInput GetSelectedIndexEcInput()
        {
            return (EcInput)m_cInput.selectedIndex;
        }

        public GGraph m_bg;
        public GGraph m_bar;
        public UI_ButtonGhost m_fold;
        public GGraph m_inputBg;
        public GTextInput m_input;
        public const string URL = "ui://m5lrgzhjg8bte";

        public static UI_ItemTag CreateInstance()
        {
            return (UI_ItemTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cTone = GetControllerAt(1);
            m_cInput = GetControllerAt(2);
            m_bg = (GGraph)GetChildAt(0);
            m_bar = (GGraph)GetChildAt(1);
            m_fold = (UI_ButtonGhost)GetChildAt(2);
            m_inputBg = (GGraph)GetChildAt(4);
            m_input = (GTextInput)GetChildAt(5);
        }
    }
}