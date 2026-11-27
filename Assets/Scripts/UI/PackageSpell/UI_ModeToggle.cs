/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ModeToggle : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ModeToggle";
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

        public GGraph m_bg;
        public GGraph m_editOn;
        public GGraph m_previewOn;
        public UI_ModeHit m_edit;
        public UI_ModeHit m_preview;
        public GGraph m_dotEdit;
        public GTextField m_editLabel;
        public GGraph m_dotPreview;
        public GTextField m_previewLabel;
        public const string URL = "ui://skl4p8n2mode02";

        public static UI_ModeToggle CreateInstance()
        {
            return (UI_ModeToggle)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cMode = GetControllerAt(0);
            m_bg = (GGraph)GetChildAt(0);
            m_editOn = (GGraph)GetChildAt(1);
            m_previewOn = (GGraph)GetChildAt(2);
            m_edit = (UI_ModeHit)GetChildAt(3);
            m_preview = (UI_ModeHit)GetChildAt(4);
            m_dotEdit = (GGraph)GetChildAt(5);
            m_editLabel = (GTextField)GetChildAt(6);
            m_dotPreview = (GGraph)GetChildAt(7);
            m_previewLabel = (GTextField)GetChildAt(8);
        }
    }
}