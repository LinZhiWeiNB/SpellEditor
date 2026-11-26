/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_PanelSpell : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "PanelSpell";
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

        public Controller m_cSpine;

        public enum EcSpine
        {
            /// <summary>
            /// null
            /// <summary>
            关 = 0,
            /// <summary>
            /// null
            /// <summary>
            开 = 1,
        }

        public EcSpine GetSelectedIndexEcSpine()
        {
            return (EcSpine)m_cSpine.selectedIndex;
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

        public Controller m_cUnits;

        public enum EcUnits
        {
            /// <summary>
            /// null
            /// <summary>
            关 = 0,
            /// <summary>
            /// null
            /// <summary>
            开 = 1,
        }

        public EcUnits GetSelectedIndexEcUnits()
        {
            return (EcUnits)m_cUnits.selectedIndex;
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

        public GGraph m_funcPane;
        public GGraph m_preview;
        public GGraph m_inspectPane;
        public GGraph m_timePane;
        public GGraph m_vSplitL;
        public GGraph m_vSplitR;
        public GGraph m_hSplit;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_funcUnit;
        public GGraph m_bgUnit;
        public GTextField m_unitName;
        public PackageAtrribute.UI_ButtonSecondary m_btnSwitchUnit;
        public GTextField m_funcSkill;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddKey;
        public PackageAtrribute.UI_ButtonSecondary m_btnDeleteKey;
        public GTextField m_funcSpine;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddClip;
        public GGroup m_spineBlock;
        public GTextField m_previewLabel;
        public GGraph m_floor;
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
        public GGroup m_spineInspect;
        public GList m_units;
        public GTextField m_previewCast;
        public GList m_dummies;
        public UI_ComMarks m_marks;
        public UI_ComTracks m_tracks;
        public UI_ComProgress m_progress;
        public UI_ScrollBarH m_scrollH;
        public UI_ScrollBarV m_scrollV;
        public const string URL = "ui://skl4p8n2mns01";

        public static UI_PanelSpell CreateInstance()
        {
            return (UI_PanelSpell)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cInspect = GetControllerAt(0);
            m_cSpine = GetControllerAt(1);
            m_cSpineEnd = GetControllerAt(2);
            m_cUnits = GetControllerAt(3);
            m_cClipName = GetControllerAt(4);
            m_funcPane = (GGraph)GetChildAt(0);
            m_preview = (GGraph)GetChildAt(1);
            m_inspectPane = (GGraph)GetChildAt(2);
            m_timePane = (GGraph)GetChildAt(3);
            m_vSplitL = (GGraph)GetChildAt(4);
            m_vSplitR = (GGraph)GetChildAt(5);
            m_hSplit = (GGraph)GetChildAt(6);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(7);
            m_title = (GTextField)GetChildAt(8);
            m_funcUnit = (GTextField)GetChildAt(9);
            m_bgUnit = (GGraph)GetChildAt(10);
            m_unitName = (GTextField)GetChildAt(11);
            m_btnSwitchUnit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(12);
            m_funcSkill = (GTextField)GetChildAt(13);
            m_btnAddKey = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(14);
            m_btnDeleteKey = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(15);
            m_funcSpine = (GTextField)GetChildAt(16);
            m_btnAddClip = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(17);
            m_spineBlock = (GGroup)GetChildAt(18);
            m_previewLabel = (GTextField)GetChildAt(19);
            m_floor = (GGraph)GetChildAt(20);
            m_inspectTitle = (GTextField)GetChildAt(21);
            m_inspectKind = (GTextField)GetChildAt(22);
            m_inspectEmpty = (GTextField)GetChildAt(23);
            m_labelPick = (GTextField)GetChildAt(24);
            m_pickOwner = (PackageAtrribute.UI_ButtonRadio)GetChildAt(25);
            m_pickPrimary = (PackageAtrribute.UI_ButtonRadio)GetChildAt(26);
            m_pickSubject = (PackageAtrribute.UI_ButtonRadio)GetChildAt(27);
            m_pickSource = (PackageAtrribute.UI_ButtonRadio)GetChildAt(28);
            m_pickExplicit = (PackageAtrribute.UI_ButtonRadio)GetChildAt(29);
            m_pickGroup = (GGroup)GetChildAt(30);
            m_labelCondition = (GTextField)GetChildAt(31);
            m_condAll = (PackageAtrribute.UI_ButtonRadio)GetChildAt(32);
            m_condAny = (PackageAtrribute.UI_ButtonRadio)GetChildAt(33);
            m_condNotAny = (PackageAtrribute.UI_ButtonRadio)GetChildAt(34);
            m_condGroup = (GGroup)GetChildAt(35);
            m_labelTags = (GTextField)GetChildAt(36);
            m_tags = (GList)GetChildAt(37);
            m_tagHint = (GTextField)GetChildAt(38);
            m_labelBuffs = (GTextField)GetChildAt(39);
            m_buffs = (GList)GetChildAt(40);
            m_buffHint = (GTextField)GetChildAt(41);
            m_skillInspect = (GGroup)GetChildAt(42);
            m_labelClip = (GTextField)GetChildAt(43);
            m_clipByName = (PackageAtrribute.UI_ButtonRadio)GetChildAt(44);
            m_clipByTrigger = (PackageAtrribute.UI_ButtonRadio)GetChildAt(45);
            m_clipNameGroup = (GGroup)GetChildAt(46);
            m_labelAnim = (GTextField)GetChildAt(47);
            m_bgAnim = (GGraph)GetChildAt(48);
            m_inputAnim = (GTextInput)GetChildAt(49);
            m_labelTrigger = (GTextField)GetChildAt(50);
            m_bgTrigger = (GGraph)GetChildAt(51);
            m_inputTrigger = (GTextInput)GetChildAt(52);
            m_labelSpeed = (GTextField)GetChildAt(53);
            m_bgSpeed = (GGraph)GetChildAt(54);
            m_inputSpeed = (GTextInput)GetChildAt(55);
            m_labelEnd = (GTextField)GetChildAt(56);
            m_endPause = (PackageAtrribute.UI_ButtonRadio)GetChildAt(57);
            m_endLoop = (PackageAtrribute.UI_ButtonRadio)GetChildAt(58);
            m_endGroup = (GGroup)GetChildAt(59);
            m_pauseFirst = (PackageAtrribute.UI_ButtonRadio)GetChildAt(60);
            m_pauseLast = (PackageAtrribute.UI_ButtonRadio)GetChildAt(61);
            m_pauseAt = (GGroup)GetChildAt(62);
            m_pingpong = (PackageAtrribute.UI_ButtonCheck)GetChildAt(63);
            m_spineInspect = (GGroup)GetChildAt(64);
            m_units = (GList)GetChildAt(65);
            m_previewCast = (GTextField)GetChildAt(66);
            m_dummies = (GList)GetChildAt(67);
            m_marks = (UI_ComMarks)GetChildAt(68);
            m_tracks = (UI_ComTracks)GetChildAt(69);
            m_progress = (UI_ComProgress)GetChildAt(70);
            m_scrollH = (UI_ScrollBarH)GetChildAt(71);
            m_scrollV = (UI_ScrollBarV)GetChildAt(72);
        }
    }
}