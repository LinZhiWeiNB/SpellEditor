/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ButtonPill : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ButtonPill";
        public GGraph m_bgIdle;
        public GGraph m_bgOn;
        public const string URL = "ui://m5lrgzhjg8bt2";

        public static UI_ButtonPill CreateInstance()
        {
            return (UI_ButtonPill)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bgIdle = (GGraph)GetChildAt(0);
            m_bgOn = (GGraph)GetChildAt(1);
        }
    }
}