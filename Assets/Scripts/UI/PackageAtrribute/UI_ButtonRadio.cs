/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ButtonRadio : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ButtonRadio";
        public GGraph m_ring;
        public GGraph m_dot;
        public const string URL = "ui://m5lrgzhjg8bt4";

        public static UI_ButtonRadio CreateInstance()
        {
            return (UI_ButtonRadio)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_ring = (GGraph)GetChildAt(0);
            m_dot = (GGraph)GetChildAt(1);
        }
    }
}