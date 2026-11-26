/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageUnit
{
    public partial class UI_ItemValue : GComponent
    {
        public const string PackageName = "PackageUnit";
        public const string ResName = "ItemValue";
        public GTextField m_title;
        public GGraph m_bg;
        public GTextInput m_input;
        public const string URL = "ui://unt4p8n2mnu05";

        public static UI_ItemValue CreateInstance()
        {
            return (UI_ItemValue)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_bg = (GGraph)GetChildAt(1);
            m_input = (GTextInput)GetChildAt(2);
        }
    }
}