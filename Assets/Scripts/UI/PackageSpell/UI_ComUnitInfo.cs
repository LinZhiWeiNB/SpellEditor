/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComUnitInfo : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComUnitInfo";
        public GGraph m_bg;
        public GList m_tags;
        public GList m_attrs;
        public const string URL = "ui://skl4p8n2uinf1";

        public static UI_ComUnitInfo CreateInstance()
        {
            return (UI_ComUnitInfo)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_tags = (GList)GetChildAt(1);
            m_attrs = (GList)GetChildAt(2);
        }
    }
}