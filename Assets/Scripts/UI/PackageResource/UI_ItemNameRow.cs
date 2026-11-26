/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemNameRow : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemNameRow";
        public GGraph m_inputBg;
        public GTextInput m_input;
        public PackageAtrribute.UI_ButtonGhost m_btnDelete;
        public GList m_cells;
        public const string URL = "ui://res4p8n2spn13";

        public static UI_ItemNameRow CreateInstance()
        {
            return (UI_ItemNameRow)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_inputBg = (GGraph)GetChildAt(0);
            m_input = (GTextInput)GetChildAt(1);
            m_btnDelete = (PackageAtrribute.UI_ButtonGhost)GetChildAt(2);
            m_cells = (GList)GetChildAt(3);
        }
    }
}