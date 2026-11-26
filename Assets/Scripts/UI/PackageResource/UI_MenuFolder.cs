/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_MenuFolder : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "MenuFolder";
        public GGraph m_bg;
        public PackageAtrribute.UI_ButtonSecondary m_btnRename;
        public PackageAtrribute.UI_ButtonSecondary m_btnNew;
        public PackageAtrribute.UI_ButtonSecondary m_btnRemove;
        public const string URL = "ui://res4p8n2mnr06";

        public static UI_MenuFolder CreateInstance()
        {
            return (UI_MenuFolder)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChildAt(0);
            m_btnRename = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(1);
            m_btnNew = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(2);
            m_btnRemove = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(3);
        }
    }
}