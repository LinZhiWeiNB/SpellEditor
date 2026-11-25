/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_ButtonSecondary : GButton
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "ButtonSecondary";
        public GGraph m_bg;
        public const string URL = "ui://mskevjh6lpb02";

        public static UI_ButtonSecondary CreateInstance()
        {
            return (UI_ButtonSecondary)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
        }
    }
}