/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_WindowPick : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "WindowPick";
        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public GGraph m_bgSearch;
        public GTextInput m_search;
        public GList m_choices;
        public GTextField m_empty;
        public PackageAtrribute.UI_ButtonGhost m_btnCancel;
        public const string URL = "ui://unt4p8n2mnu06";

        public static UI_WindowPick CreateInstance()
        {
            return (UI_WindowPick)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_bgSearch = (GGraph)GetChildAt(3);
            m_search = (GTextInput)GetChildAt(4);
            m_choices = (GList)GetChildAt(5);
            m_empty = (GTextField)GetChildAt(6);
            m_btnCancel = (PackageAtrribute.UI_ButtonGhost)GetChildAt(7);
        }
    }
}