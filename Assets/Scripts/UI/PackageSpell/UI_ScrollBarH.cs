/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ScrollBarH : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ScrollBarH";
        public GGraph m_bar;
        public GGraph m_grip;
        public const string URL = "ui://skl4p8n2s0uwg";

        public static UI_ScrollBarH CreateInstance()
        {
            return (UI_ScrollBarH)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bar = (GGraph)GetChildAt(0);
            m_grip = (GGraph)GetChildAt(1);
        }
    }
}