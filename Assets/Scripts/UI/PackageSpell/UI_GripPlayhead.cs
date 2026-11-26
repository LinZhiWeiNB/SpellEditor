/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_GripPlayhead : GButton
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "GripPlayhead";
        public GGraph m_head;
        public const string URL = "ui://skl4p8n2mns03";

        public static UI_GripPlayhead CreateInstance()
        {
            return (UI_GripPlayhead)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_head = (GGraph)GetChildAt(0);
        }
    }
}