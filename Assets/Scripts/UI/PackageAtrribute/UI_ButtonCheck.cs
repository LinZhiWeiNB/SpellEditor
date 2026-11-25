/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageAtrribute
{
    public partial class UI_ButtonCheck : GButton
    {
        public const string PackageName = "PackageAtrribute";
        public const string ResName = "ButtonCheck";
        public GGraph m_box;
        public GGraph m_mark;
        public const string URL = "ui://m5lrgzhjg8bt0";

        public static UI_ButtonCheck CreateInstance()
        {
            return (UI_ButtonCheck)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_box = (GGraph)GetChildAt(0);
            m_mark = (GGraph)GetChildAt(1);
        }
    }
}