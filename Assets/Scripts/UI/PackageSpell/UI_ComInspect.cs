/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComInspect : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComInspect";
        public Controller m_cInspect;

        public enum EcInspect
        {
            /// <summary>
            /// null
            /// <summary>
            空 = 0,
            /// <summary>
            /// null
            /// <summary>
            技能关键帧 = 1,
            /// <summary>
            /// null
            /// <summary>
            Spine关键帧 = 2,
        }

        public EcInspect GetSelectedIndexEcInspect()
        {
            return (EcInspect)m_cInspect.selectedIndex;
        }

        public GGraph m_inspectPane;
        public GTextField m_inspectTitle;
        public GTextField m_inspectKind;
        public GGraph m_emptyCard;
        public GTextField m_inspectEmpty;
        public UI_ComSkillInspect m_skillInspect;
        public UI_ComSpineInspect m_spineInspect;
        public const string URL = "ui://skl4p8n2insp01";

        public static UI_ComInspect CreateInstance()
        {
            return (UI_ComInspect)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cInspect = GetControllerAt(0);
            m_inspectPane = (GGraph)GetChildAt(0);
            m_inspectTitle = (GTextField)GetChildAt(1);
            m_inspectKind = (GTextField)GetChildAt(2);
            m_emptyCard = (GGraph)GetChildAt(3);
            m_inspectEmpty = (GTextField)GetChildAt(4);
            m_skillInspect = (UI_ComSkillInspect)GetChildAt(5);
            m_spineInspect = (UI_ComSpineInspect)GetChildAt(6);
        }
    }
}