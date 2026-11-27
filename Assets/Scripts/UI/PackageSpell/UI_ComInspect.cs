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

        public Controller m_cSpineEnd;

        public enum EcSpineEnd
        {
            /// <summary>
            /// null
            /// <summary>
            暂停 = 0,
            /// <summary>
            /// null
            /// <summary>
            无限循环 = 1,
        }

        public EcSpineEnd GetSelectedIndexEcSpineEnd()
        {
            return (EcSpineEnd)m_cSpineEnd.selectedIndex;
        }

        public Controller m_cClipName;

        public enum EcClipName
        {
            /// <summary>
            /// null
            /// <summary>
            动画名 = 0,
            /// <summary>
            /// null
            /// <summary>
            触发器名 = 1,
        }

        public EcClipName GetSelectedIndexEcClipName()
        {
            return (EcClipName)m_cClipName.selectedIndex;
        }

        public GGraph m_inspectPane;
        public GTextField m_inspectTitle;
        public GTextField m_inspectKind;
        public GTextField m_inspectEmpty;
        public GTextField m_labelPick;
        public PackageAtrribute.UI_ButtonRadio m_pickOwner;
        public PackageAtrribute.UI_ButtonRadio m_pickPrimary;
        public PackageAtrribute.UI_ButtonRadio m_pickSubject;
        public PackageAtrribute.UI_ButtonRadio m_pickSource;
        public PackageAtrribute.UI_ButtonRadio m_pickExplicit;
        public GGroup m_pickGroup;
        public GTextField m_labelCondition;
        public PackageAtrribute.UI_ButtonRadio m_condAll;
        public PackageAtrribute.UI_ButtonRadio m_condAny;
        public PackageAtrribute.UI_ButtonRadio m_condNotAny;
        public GGroup m_condGroup;
        public GTextField m_labelTags;
        public GList m_tags;
        public GTextField m_tagHint;
        public GTextField m_labelBuffs;
        public GList m_buffs;
        public GTextField m_buffHint;
        public GGroup m_skillInspect;
        public GTextField m_labelClip;
        public PackageAtrribute.UI_ButtonRadio m_clipByName;
        public PackageAtrribute.UI_ButtonRadio m_clipByTrigger;
        public GGroup m_clipNameGroup;
        public GTextField m_labelAnim;
        public GGraph m_bgAnim;
        public GTextInput m_inputAnim;
        public GTextField m_labelTrigger;
        public GGraph m_bgTrigger;
        public GTextInput m_inputTrigger;
        public GTextField m_labelSpeed;
        public GGraph m_bgSpeed;
        public GTextInput m_inputSpeed;
        public GTextField m_labelEnd;
        public PackageAtrribute.UI_ButtonRadio m_endPause;
        public PackageAtrribute.UI_ButtonRadio m_endLoop;
        public GGroup m_endGroup;
        public PackageAtrribute.UI_ButtonRadio m_pauseFirst;
        public PackageAtrribute.UI_ButtonRadio m_pauseLast;
        public GGroup m_pauseAt;
        public PackageAtrribute.UI_ButtonCheck m_pingpong;
        public PackageResource.UI_ComboValue m_comboAnim;
        public PackageResource.UI_ComboValue m_comboTrigger;
        public GGroup m_spineInspect;
        public const string URL = "ui://skl4p8n2insp01";

        public static UI_ComInspect CreateInstance()
        {
            return (UI_ComInspect)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cInspect = GetControllerAt(0);
            m_cSpineEnd = GetControllerAt(1);
            m_cClipName = GetControllerAt(2);
            m_inspectPane = (GGraph)GetChildAt(0);
            m_inspectTitle = (GTextField)GetChildAt(1);
            m_inspectKind = (GTextField)GetChildAt(2);
            m_inspectEmpty = (GTextField)GetChildAt(3);
            m_labelPick = (GTextField)GetChildAt(4);
            m_pickOwner = (PackageAtrribute.UI_ButtonRadio)GetChildAt(5);
            m_pickPrimary = (PackageAtrribute.UI_ButtonRadio)GetChildAt(6);
            m_pickSubject = (PackageAtrribute.UI_ButtonRadio)GetChildAt(7);
            m_pickSource = (PackageAtrribute.UI_ButtonRadio)GetChildAt(8);
            m_pickExplicit = (PackageAtrribute.UI_ButtonRadio)GetChildAt(9);
            m_pickGroup = (GGroup)GetChildAt(10);
            m_labelCondition = (GTextField)GetChildAt(11);
            m_condAll = (PackageAtrribute.UI_ButtonRadio)GetChildAt(12);
            m_condAny = (PackageAtrribute.UI_ButtonRadio)GetChildAt(13);
            m_condNotAny = (PackageAtrribute.UI_ButtonRadio)GetChildAt(14);
            m_condGroup = (GGroup)GetChildAt(15);
            m_labelTags = (GTextField)GetChildAt(16);
            m_tags = (GList)GetChildAt(17);
            m_tagHint = (GTextField)GetChildAt(18);
            m_labelBuffs = (GTextField)GetChildAt(19);
            m_buffs = (GList)GetChildAt(20);
            m_buffHint = (GTextField)GetChildAt(21);
            m_skillInspect = (GGroup)GetChildAt(22);
            m_labelClip = (GTextField)GetChildAt(23);
            m_clipByName = (PackageAtrribute.UI_ButtonRadio)GetChildAt(24);
            m_clipByTrigger = (PackageAtrribute.UI_ButtonRadio)GetChildAt(25);
            m_clipNameGroup = (GGroup)GetChildAt(26);
            m_labelAnim = (GTextField)GetChildAt(27);
            m_bgAnim = (GGraph)GetChildAt(28);
            m_inputAnim = (GTextInput)GetChildAt(29);
            m_labelTrigger = (GTextField)GetChildAt(30);
            m_bgTrigger = (GGraph)GetChildAt(31);
            m_inputTrigger = (GTextInput)GetChildAt(32);
            m_labelSpeed = (GTextField)GetChildAt(33);
            m_bgSpeed = (GGraph)GetChildAt(34);
            m_inputSpeed = (GTextInput)GetChildAt(35);
            m_labelEnd = (GTextField)GetChildAt(36);
            m_endPause = (PackageAtrribute.UI_ButtonRadio)GetChildAt(37);
            m_endLoop = (PackageAtrribute.UI_ButtonRadio)GetChildAt(38);
            m_endGroup = (GGroup)GetChildAt(39);
            m_pauseFirst = (PackageAtrribute.UI_ButtonRadio)GetChildAt(40);
            m_pauseLast = (PackageAtrribute.UI_ButtonRadio)GetChildAt(41);
            m_pauseAt = (GGroup)GetChildAt(42);
            m_pingpong = (PackageAtrribute.UI_ButtonCheck)GetChildAt(43);
            m_comboAnim = (PackageResource.UI_ComboValue)GetChildAt(44);
            m_comboTrigger = (PackageResource.UI_ComboValue)GetChildAt(45);
            m_spineInspect = (GGroup)GetChildAt(46);
        }
    }
}