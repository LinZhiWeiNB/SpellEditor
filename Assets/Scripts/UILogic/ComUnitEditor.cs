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
            var mod = ProjectSession.AuthorId != 0;
            view.m_units.numItems = 0;
            view.m_included.numItems = 0;
            view.m_pgName.text = "";
            view.m_pgOwn.text = "";
            view.m_pgCreate.visible = !mod;
            view.m_pgName.touchable = !mod;
            if (mod)
            {
                view.m_pgTitle.text = "游戏单位";
                view.m_pgDesc.text = "模组使用游戏里已经定义的单位，不能新建单位。";
                view.m_pgHint.text = "勾选只对当前模组里的属性生效。游戏和其他数据源里的属性保持原样。";
            }
        }
    }
}
