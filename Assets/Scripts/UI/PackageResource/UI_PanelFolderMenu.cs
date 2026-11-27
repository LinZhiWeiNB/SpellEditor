/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_PanelFolderMenu : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "PanelFolderMenu";
        public GGraph m_mask;
        public GGraph m_card;
        public PackageAtrribute.UI_ButtonSecondary m_btnRename;
        public PackageAtrribute.UI_ButtonSecondary m_btnNew;
        public PackageAtrribute.UI_ButtonSecondary m_btnRemove;
        public const string URL = "ui://res4p8n2rfmn1";

        public static UI_PanelFolderMenu CreateInstance()
        {
            return (UI_PanelFolderMenu)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mask = (GGraph)GetChildAt(0);
            m_card = (GGraph)GetChildAt(1);
            m_btnRename = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(2);
            m_btnNew = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(3);
            m_btnRemove = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(4);
        }
    }
}