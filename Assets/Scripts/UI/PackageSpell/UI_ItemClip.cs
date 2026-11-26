/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemClip : GButton
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemClip";
        public GGraph m_bg;
        public const string URL = "ui://skl4p8n2mns05";

        public static UI_ItemClip CreateInstance()
        {
            return (UI_ItemClip)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
        }
    }
}