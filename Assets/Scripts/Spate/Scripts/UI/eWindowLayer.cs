using System;

namespace Spate
{
    public enum eWindowLayer
    {
        /// <summary>
        /// 壁纸层,处于最低
        /// </summary>
        Wallpaper,
        /// <summary>
        /// 普通UI层
        /// </summary>
        Normal,
        /// <summary>
        /// 普通对话框,弹出框层
        /// </summary>
        Dialog,
        /// <summary>
        /// 系统警告层，最顶层
        /// </summary>
        Alert
    }
}
