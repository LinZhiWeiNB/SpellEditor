/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemInfoAttr : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemInfoAttr";
        public GTextField m_title;
        public GTextField m_value;
        public GGraph m_track;
        public GGraph m_fill;
        public const string URL = "ui://skl4p8n2iatr1";

        public static UI_ItemInfoAttr CreateInstance()
        {
            return (UI_ItemInfoAttr)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_value = (GTextField)GetChildAt(1);
            m_track = (GGraph)GetChildAt(2);
            m_fill = (GGraph)GetChildAt(3);
        }
    }
}