/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ModeHit : GButton
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ModeHit";
        public GGraph m_hit;
        public const string URL = "ui://skl4p8n2mode01";

        public static UI_ModeHit CreateInstance()
        {
            return (UI_ModeHit)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_hit = (GGraph)GetChildAt(0);
        }
    }
}