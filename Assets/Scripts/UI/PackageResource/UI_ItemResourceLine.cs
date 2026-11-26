/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageResource
{
    public partial class UI_ItemResourceLine : GComponent
    {
        public const string PackageName = "PackageResource";
        public const string ResName = "ItemResourceLine";
        public GTextField m_title;
        public const string URL = "ui://res4p8n2mnr04";

        public static UI_ItemResourceLine CreateInstance()
        {
            return (UI_ItemResourceLine)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
        }
    }
}