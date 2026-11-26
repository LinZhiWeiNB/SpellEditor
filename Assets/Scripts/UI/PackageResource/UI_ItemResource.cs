/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemResource : GButton
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemResource";
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

        public GGraph m_bg;
        public GTextField m_meta;
        public const string URL = "ui://res4p8n2mnr02";

        public static UI_ItemResource CreateInstance()
        {
            return (UI_ItemResource)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cTone = GetControllerAt(1);
            m_bg = (GGraph)GetChildAt(0);
            m_meta = (GTextField)GetChildAt(2);
        }
    }
}