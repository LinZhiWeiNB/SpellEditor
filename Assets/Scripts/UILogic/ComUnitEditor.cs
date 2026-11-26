using PackageAtrribute;
using SpellEditor;

namespace UILogic
{
    /// <summary>
    /// 单位预设还没有写入 SpellModuleData，这里只按作者身份摆界面。
    /// 游戏开发者看得到新建按钮，模组作者不能新建单位。
    /// </summary>
    public class ComUnitEditor
    {
        public ComUnitEditor(UI_PanelUnit view)
        {
            view.m_cRole.selectedIndex = ProjectSession.AuthorId == 0
                ? (int)UI_PanelUnit.EcRole.游戏开发者
                : (int)UI_PanelUnit.EcRole.模组作者;
            view.m_units.numItems = 0;
            view.m_included.numItems = 0;
            view.m_pgName.text = "";
            view.m_pgOwn.text = "";
        }
    }
}
