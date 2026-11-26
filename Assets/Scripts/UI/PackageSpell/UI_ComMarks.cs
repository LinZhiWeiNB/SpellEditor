/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComMarks : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComMarks";
        public GList m_marks;
        public const string URL = "ui://skl4p8n2s0uwc";

        public static UI_ComMarks CreateInstance()
        {
            return (UI_ComMarks)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_marks = (GList)GetChildAt(0);
        }
    }
}