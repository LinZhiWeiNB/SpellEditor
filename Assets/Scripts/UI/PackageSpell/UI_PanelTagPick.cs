/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_PanelTagPick : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "PanelTagPick";
        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public GGraph m_bgSearch;
        public GTextInput m_search;
        public GList m_choices;
        public GTextField m_empty;
        public GGraph m_split;
        public GTextField m_createTitle;
        public GTextField m_labelName;
        public GGraph m_bgName;
        public GTextInput m_inputName;
        public GTextField m_createHint;
        public GTextField m_createWarn;
        public PackageAtrribute.UI_ButtonPrimary m_btnCreate;
        public PackageAtrribute.UI_ButtonGhost m_btnCancel;
        public const string URL = "ui://skl4p8n2ptag1";

        public static UI_PanelTagPick CreateInstance()
        {
            return (UI_PanelTagPick)UIPackage.CreateObject(PackageName, ResName);
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
            m_split = (GGraph)GetChildAt(7);
            m_createTitle = (GTextField)GetChildAt(8);
            m_labelName = (GTextField)GetChildAt(9);
            m_bgName = (GGraph)GetChildAt(10);
            m_inputName = (GTextInput)GetChildAt(11);
            m_createHint = (GTextField)GetChildAt(12);
            m_createWarn = (GTextField)GetChildAt(13);
            m_btnCreate = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(14);
            m_btnCancel = (PackageAtrribute.UI_ButtonGhost)GetChildAt(15);
        }
    }
}