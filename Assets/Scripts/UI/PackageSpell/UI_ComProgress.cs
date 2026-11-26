/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComProgress : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComProgress";
        public UI_SliderFrame m_progress;
        public const string URL = "ui://skl4p8n2s0uwd";

        public static UI_ComProgress CreateInstance()
        {
            return (UI_ComProgress)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_progress = (UI_SliderFrame)GetChildAt(0);
        }
    }
}