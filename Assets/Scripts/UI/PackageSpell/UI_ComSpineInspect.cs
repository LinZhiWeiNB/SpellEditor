/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComSpineInspect : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComSpineInspect";
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

        public GGraph m_cardClip;
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
        public GGraph m_cardSpeed;
        public GTextField m_labelSpeed;
        public GGraph m_bgSpeed;
        public GTextInput m_inputSpeed;
        public GGraph m_cardEnd;
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
        public const string URL = "ui://skl4p8n2spin1";

        public static UI_ComSpineInspect CreateInstance()
        {
            return (UI_ComSpineInspect)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cSpineEnd = GetControllerAt(0);
            m_cClipName = GetControllerAt(1);
            m_cardClip = (GGraph)GetChildAt(0);
            m_labelClip = (GTextField)GetChildAt(1);
            m_clipByName = (PackageAtrribute.UI_ButtonRadio)GetChildAt(2);
            m_clipByTrigger = (PackageAtrribute.UI_ButtonRadio)GetChildAt(3);
            m_clipNameGroup = (GGroup)GetChildAt(4);
            m_labelAnim = (GTextField)GetChildAt(5);
            m_bgAnim = (GGraph)GetChildAt(6);
            m_inputAnim = (GTextInput)GetChildAt(7);
            m_labelTrigger = (GTextField)GetChildAt(8);
            m_bgTrigger = (GGraph)GetChildAt(9);
            m_inputTrigger = (GTextInput)GetChildAt(10);
            m_cardSpeed = (GGraph)GetChildAt(11);
            m_labelSpeed = (GTextField)GetChildAt(12);
            m_bgSpeed = (GGraph)GetChildAt(13);
            m_inputSpeed = (GTextInput)GetChildAt(14);
            m_cardEnd = (GGraph)GetChildAt(15);
            m_labelEnd = (GTextField)GetChildAt(16);
            m_endPause = (PackageAtrribute.UI_ButtonRadio)GetChildAt(17);
            m_endLoop = (PackageAtrribute.UI_ButtonRadio)GetChildAt(18);
            m_endGroup = (GGroup)GetChildAt(19);
            m_pauseFirst = (PackageAtrribute.UI_ButtonRadio)GetChildAt(20);
            m_pauseLast = (PackageAtrribute.UI_ButtonRadio)GetChildAt(21);
            m_pauseAt = (GGroup)GetChildAt(22);
            m_pingpong = (PackageAtrribute.UI_ButtonCheck)GetChildAt(23);
            m_comboAnim = (PackageResource.UI_ComboValue)GetChildAt(24);
            m_comboTrigger = (PackageResource.UI_ComboValue)GetChildAt(25);
        }
    }
}