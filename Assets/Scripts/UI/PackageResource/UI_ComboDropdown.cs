/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ComboDropdown : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ComboDropdown";
        public GList m_list;
        public const string URL = "ui://res4p8n2spn10";

        public static UI_ComboDropdown CreateInstance()
        {
            return (UI_ComboDropdown)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_list = (GList)GetChildAt(1);
        }
    }
}