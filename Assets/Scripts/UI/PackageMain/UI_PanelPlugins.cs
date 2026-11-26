/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageMain
{
    public partial class UI_PanelPlugins : GComponent
    {
        public const string PackageName = "PackageMain";
        public const string ResName = "PanelPlugins";
        public GGraph m_bg;
        public PackageLaunch.UI_ButtonGhost m_btnBack;
        public GTextField m_title;
        public GTextField m_message;
        public GList m_list;
        public GTextField m_hint;
        public PackageAtrribute.UI_ButtonPrimary m_btnImport;
        public PackageAtrribute.UI_ButtonSecondary m_btnDelete;
        public const string URL = "ui://mov3ykknplg01";

        public static UI_PanelPlugins CreateInstance()
        {
            return (UI_PanelPlugins)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_btnBack = (PackageLaunch.UI_ButtonGhost)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_message = (GTextField)GetChildAt(3);
            m_list = (GList)GetChildAt(4);
            m_hint = (GTextField)GetChildAt(5);
            m_btnImport = (PackageAtrribute.UI_ButtonPrimary)GetChildAt(6);
            m_btnDelete = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(7);
        }
    }
}