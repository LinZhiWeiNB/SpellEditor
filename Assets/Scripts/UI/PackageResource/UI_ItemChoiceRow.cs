/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemChoiceRow : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemChoiceRow";
        public GTextField m_title;
        public UI_ComboValue m_value;
        public const string URL = "ui://res4p8n2spn16";

        public static UI_ItemChoiceRow CreateInstance()
        {
            return (UI_ItemChoiceRow)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_value = (UI_ComboValue)GetChildAt(1);
        }
    }
}