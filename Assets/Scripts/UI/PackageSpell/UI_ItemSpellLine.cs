/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemSpellLine : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemSpellLine";
        public GTextField m_title;
        public PackageAtrribute.UI_ButtonGhost m_btnRemove;
        public const string URL = "ui://skl4p8n2linr1";

        public static UI_ItemSpellLine CreateInstance()
        {
            return (UI_ItemSpellLine)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_btnRemove = (PackageAtrribute.UI_ButtonGhost)GetChildAt(1);
        }
    }
}