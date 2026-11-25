/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_ItemRole : GButton
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "ItemRole";
        public GGraph m_ring;
        public GGraph m_dot;
        public GTextField m_desc;
        public const string URL = "ui://mskevjh6lpi03";

        public static UI_ItemRole CreateInstance()
        {
            return (UI_ItemRole)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_ring = (GGraph)GetChildAt(0);
            m_dot = (GGraph)GetChildAt(1);
            m_desc = (GTextField)GetChildAt(3);
        }
    }
}