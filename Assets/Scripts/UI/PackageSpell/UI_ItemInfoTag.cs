/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemInfoTag : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemInfoTag";
        public GGraph m_bg;
        public GTextField m_title;
        public const string URL = "ui://skl4p8n2itag1";

        public static UI_ItemInfoTag CreateInstance()
        {
            return (UI_ItemInfoTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_title = (GTextField)GetChildAt(1);
        }
    }
}