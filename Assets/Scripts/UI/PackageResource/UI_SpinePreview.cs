/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_SpinePreview : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "SpinePreview";
        public GGraph m_holder;
        public PackageAtrribute.UI_ButtonSecondary m_btnEdit;
        public UI_ItemChoiceRow m_rowGroup;
        public UI_ItemChoiceRow m_rowTrigger;
        public GList m_slots;
        public const string URL = "ui://res4p8n2spn17";

        public static UI_SpinePreview CreateInstance()
        {
            return (UI_SpinePreview)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_holder = (GGraph)GetChildAt(0);
            m_btnEdit = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(1);
            m_rowGroup = (UI_ItemChoiceRow)GetChildAt(2);
            m_rowTrigger = (UI_ItemChoiceRow)GetChildAt(3);
            m_slots = (GList)GetChildAt(4);
        }
    }
}