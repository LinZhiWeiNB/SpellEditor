/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComKeys : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComKeys";
        public GGraph m_extent;
        public const string URL = "ui://skl4p8n2s0uwf";

        public static UI_ComKeys CreateInstance()
        {
            return (UI_ComKeys)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_extent = (GGraph)GetChildAt(0);
        }
    }
}