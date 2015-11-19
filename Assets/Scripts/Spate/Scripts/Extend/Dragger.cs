using UnityEngine;
using System.Collections;
using System;
using Spate;

public class Dragger : BaseBehaviour
{
    public enum eRole
    {
        // 拖动方
        Drager,
        // 接受方
        Droper,
        // 两者都
        Both
    }

    public eRole role;
    public BoxCollider2D areaCollider;
    private Camera mCamera;
    private Transform mItemTransform;

    private static Transform mDragDropRoot;

    void Start()
    {
        if (mDragDropRoot == null)
            mDragDropRoot = GameObject.Find("GUI/Anchor/Dialog").transform;

        if (mCamera == null)
            mCamera = Camera.allCameras[0];
        if (areaCollider == null)
            areaCollider = GetComponent<BoxCollider2D>();
    }

    // 开始拖拽
    private void OnDragStart()
    {
        if (role == eRole.Droper)
            return;
        if (OnDragStartHandler == null)
            return;
        GameObject orgGo = OnDragStartHandler(this);
        Transform orgTf = orgGo.transform;
        // 生成
        GameObject go = GameObject.Instantiate(orgGo) as GameObject;
        mItemTransform = go.transform;
        mItemTransform.SetParent(mDragDropRoot);
        // 矫正位置
        mItemTransform.position = orgTf.position;
        mItemTransform.localRotation = orgTf.localRotation;
        mItemTransform.localScale = orgTf.localScale;
    }

    // 拖拽中
    private void OnDrag(Vector2 delta)
    {
        if (role == eRole.Droper)
            return;
        if (mItemTransform != null)
        {
            // 基于偏移量方式进行位移
            mItemTransform.localPosition += new Vector3(delta.x, delta.y);
        }
    }

    // 拖拽时经过
    private void OnDragOver(GameObject go)
    {
        if (role == eRole.Droper)
            return;
        // 是否需要变大处理,用于提示可放下
        if (OnDragOverHandler != null)
            OnDragOverHandler(this, true);
    }

    // 退出有效区域时
    private void OnDragOut(GameObject go)
    {
        if (role == eRole.Droper)
            return;
        if (OnDragOverHandler != null)
            OnDragOverHandler(this, false);
    }

    // 拖拽时经过
    private void OnDragEnd()
    {
        if (role == eRole.Droper)
            return;
        // 需要判定当前的点是否也在一个Dragger实例的有效范围中
        Collider2D[] hits = Physics2D.OverlapPointAll(mItemTransform.position, LayerUtil.UIMask);
        if (hits == null)
            return;
        foreach (Collider2D hit in hits)
        {
            Dragger droper = hit.collider2D.GetComponent<Dragger>();
            if (droper != null && droper.role != eRole.Drager && droper.OnDragDropHandler != null)
            {
                droper.OnDragDropHandler(droper);
                break;
            }
        }
        if (mItemTransform != null)
            Destroy(mItemTransform.gameObject);
    }

    // 启动的时候管逻辑端要Transform对象
    public Func<Dragger, GameObject> OnDragStartHandler;
    // 拖拽在上方区域或者退出区域
    public Action<Dragger, bool> OnDragOverHandler;
    // 放下拖拽
    public Action<Dragger> OnDragDropHandler;
}
