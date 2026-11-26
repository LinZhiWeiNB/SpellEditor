/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemResourceHead : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemResourceHead";
        public GTextField m_title;
        public const string URL = "ui://res4p8n2mnr03";

        public static UI_ItemResourceHead CreateInstance()
        {
            return (UI_ItemResourceHead)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
        }
    }
}