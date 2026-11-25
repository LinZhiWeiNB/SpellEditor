/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ButtonToggle : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ButtonToggle";
        public GGraph m_trackOff;
        public GGraph m_trackOn;
        public GGraph m_knobOff;
        public GGraph m_knobOn;
        public const string URL = "ui://m5lrgzhjg8bt6";

        public static UI_ButtonToggle CreateInstance()
        {
            return (UI_ButtonToggle)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_trackOff = (GGraph)GetChildAt(0);
            m_trackOn = (GGraph)GetChildAt(1);
            m_knobOff = (GGraph)GetChildAt(2);
            m_knobOn = (GGraph)GetChildAt(3);
        }
    }
}