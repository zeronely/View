using System;
using UnityEditor;

/// <summary>
/// 所有编辑器窗口的基类,便于对所有的编辑器进行归档
/// </summary>
public abstract class BaseEditorWindow
{
    public UnityEditor.EditorWindow aaaaa;

    /// <summary>
    /// 本窗口的名称
    /// </summary>
    public abstract string Name
    {
        get;
    }
    /// <summary>
    /// 是否自动销毁,自动销毁会在OnHide时自动发生OnDestory
    /// </summary>
    public virtual bool AutoDestroy
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// 创建
    /// </summary>
    public abstract void OnCreate();
    /// <summary>
    /// 打开
    /// </summary>
    public virtual void OnOpen()
    {

    }
    /// <summary>
    /// 绘制
    /// </summary>
    public abstract void OnGUI();
    /// <summary>
    /// 隐藏
    /// </summary>
    public virtual void OnHide()
    {

    }
    /// <summary>
    /// 销毁窗口资源
    /// </summary>
    public abstract void OnDestroy();

    protected void Repaint()
    {
        aaaaa.Repaint();
    }
}
