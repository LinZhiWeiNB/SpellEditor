/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_ItemSpellRow : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "ItemSpellRow";
        public GTextField m_title;
        public PackageAtrribute.UI_ButtonSecondary m_btnEdit;
        public PackageAtrribute.UI_ButtonGhost m_btnDelete;
        public const string URL = "ui://unt4p8n2mnu08";

        public static UI_ItemSpellRow CreateInstance()
        {
            return (UI_ItemSpellRow)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_btnEdit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(1);
            m_btnDelete = (PackageAtrribute.UI_ButtonGhost)GetChildAt(2);
        }
    }
}