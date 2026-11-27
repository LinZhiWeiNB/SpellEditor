/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_PanelPickList : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "PanelPickList";
        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public GTextField m_hint;
        public GList m_choices;
        public PackageAtrribute.UI_ButtonSecondary m_btnCancel;
        public const string URL = "ui://res4p8n2rlst1";

        public static UI_PanelPickList CreateInstance()
        {
            return (UI_PanelPickList)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_hint = (GTextField)GetChildAt(3);
            m_choices = (GList)GetChildAt(4);
            m_btnCancel = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(5);
        }
    }
}