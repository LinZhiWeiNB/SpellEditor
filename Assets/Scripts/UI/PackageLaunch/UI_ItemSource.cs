/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageLaunch
{
    public partial class UI_ItemSource : GComponent
    {
        public const string PackageName = "PackageLaunch";
        public const string ResName = "ItemSource";
        public GGraph m_bg;
        public GTextField m_title;
        public GTextField m_kind;
        public GTextField m_path;
        public UI_ButtonGhost m_remove;
        public const string URL = "ui://mskevjh6lpi04";

        public static UI_ItemSource CreateInstance()
        {
            return (UI_ItemSource)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_title = (GTextField)GetChildAt(1);
            m_kind = (GTextField)GetChildAt(2);
            m_path = (GTextField)GetChildAt(3);
            m_remove = (UI_ButtonGhost)GetChildAt(4);
        }
    }
}