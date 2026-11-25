/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_ButtonModule : GButton
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "ButtonModule";
        public GGraph m_bg;
        public GGraph m_bgOver;
        public const string URL = "ui://mov3ykknmnb01";

        public static UI_ButtonModule CreateInstance()
        {
            return (UI_ButtonModule)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_bgOver = (GGraph)GetChildAt(1);
        }
    }
}