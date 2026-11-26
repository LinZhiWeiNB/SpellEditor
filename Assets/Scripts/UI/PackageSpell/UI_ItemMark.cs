/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemMark : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemMark";
        public GTextField m_mark0;
        public const string URL = "ui://skl4p8n2s0uwb";

        public static UI_ItemMark CreateInstance()
        {
            return (UI_ItemMark)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mark0 = (GTextField)GetChildAt(0);
        }
    }
}