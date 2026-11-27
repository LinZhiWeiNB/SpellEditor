/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemSpellTag : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemSpellTag";
        public GTextField m_title;
        public PackageAtrribute.UI_ButtonGhost m_btnEdit;
        public PackageAtrribute.UI_ButtonGhost m_btnRemove;
        public const string URL = "ui://skl4p8n2tagr1";

        public static UI_ItemSpellTag CreateInstance()
        {
            return (UI_ItemSpellTag)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_btnEdit = (PackageAtrribute.UI_ButtonGhost)GetChildAt(1);
            m_btnRemove = (PackageAtrribute.UI_ButtonGhost)GetChildAt(2);
        }
    }
}