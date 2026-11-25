/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ButtonPrimary : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ButtonPrimary";
        public GGraph m_bg;
        public const string URL = "ui://m5lrgzhjg8bt3";

        public static UI_ButtonPrimary CreateInstance()
        {
            return (UI_ButtonPrimary)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
        }
    }
}