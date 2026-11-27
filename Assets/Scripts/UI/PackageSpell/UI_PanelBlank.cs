/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_PanelBlank : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "PanelBlank";
        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public GTextField m_hint;
        public PackageAtrribute.UI_ButtonGhost m_btnCancel;
        public const string URL = "ui://skl4p8n2pblk1";

        public static UI_PanelBlank CreateInstance()
        {
            return (UI_PanelBlank)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_hint = (GTextField)GetChildAt(3);
            m_btnCancel = (PackageAtrribute.UI_ButtonGhost)GetChildAt(4);
        }
    }
}