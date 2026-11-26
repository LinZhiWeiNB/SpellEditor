/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemCombo : GButton
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemCombo";
        public GGraph m_bg;
        public const string URL = "ui://res4p8n2spn12";

        public static UI_ItemCombo CreateInstance()
        {
            return (UI_ItemCombo)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
        }
    }
}