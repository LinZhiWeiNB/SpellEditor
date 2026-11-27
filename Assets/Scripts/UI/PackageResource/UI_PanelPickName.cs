/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_PanelPickName : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "PanelPickName";
        public GGraph m_mask;
        public GGraph m_card;
        public GTextField m_title;
        public GTextField m_hint;
        public GList m_choices;
        public GGraph m_pathBg;
        public GTextInput m_inputPath;
        public PackageAtrribute.UI_ButtonPrimary m_btnPlace;
        public PackageAtrribute.UI_ButtonSecondary m_btnCancel;
        public const string URL = "ui://res4p8n2rnam1";

        public static UI_PanelPickName CreateInstance()
        {
            return (UI_PanelPickName)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_hint = (GTextField)GetChildAt(3);
            m_choices = (GList)GetChildAt(4);
            m_pathBg = (GGraph)GetChildAt(5);
            m_inputPath = (GTextInput)GetChildAt(6);
            m_btnPlace = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(7);
            m_btnCancel = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(8);
        }
    }
}