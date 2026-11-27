/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemUnitValue : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemUnitValue";
        public GTextField m_title;
        public PackageAtrribute.UI_ButtonCheck m_track;
        public GTextField m_currentLabel;
        public GGraph m_bg;
        public GTextInput m_input;
        public GTextField m_maxLabel;
        public GGraph m_maxBg;
        public GTextInput m_maxInput;
        public const string URL = "ui://skl4p8n2uval1";

        public static UI_ItemUnitValue CreateInstance()
        {
            return (UI_ItemUnitValue)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_track = (PackageAtrribute.UI_ButtonCheck)GetChildAt(1);
            m_currentLabel = (GTextField)GetChildAt(2);
            m_bg = (GGraph)GetChildAt(3);
            m_input = (GTextInput)GetChildAt(4);
            m_maxLabel = (GTextField)GetChildAt(5);
            m_maxBg = (GGraph)GetChildAt(6);
            m_maxInput = (GTextInput)GetChildAt(7);
        }
    }
}