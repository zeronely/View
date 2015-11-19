using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
public class UListView : MonoBehaviour
{
    /// <summary>
    /// 运动方向
    /// </summary>
    public enum Movement
    {
        Horizontal,
        Vertical,
    }

    public enum DragEffect
    {
        None,
        Momentum,
        MomentumAndSpring,
    }

    public enum ShowCondition
    {
        Always,
        OnlyIfNeeded,
        WhenDragging,
    }

    public delegate void OnDragNotification();
    /// <summary>
    /// 开始拖拽
    /// </summary>
    public OnDragNotification onDragStarted;
    /// <summary>
    /// 拖拽结束
    /// </summary>
    public OnDragNotification onDragFinished;
    /// <summary>
    /// 动力移动
    /// </summary>
    public OnDragNotification onMomentumMove;
    /// <summary>
    /// 停止移动
    /// </summary>
    public OnDragNotification onStoppedMoving;
    /// <summary>
    /// 拖动到底部
    /// </summary>
    public OnDragNotification OnDragFloor;

    /// <summary>
    /// 当前UListView的运动方向
    /// </summary>
    public Movement movement = Movement.Horizontal;
    /// <summary>
    /// 当前UListView的拖拽效果
    /// </summary>
    public DragEffect dragEffect = DragEffect.MomentumAndSpring;
    /// <summary>
    /// 是否要限制内容在Bounds内.
    /// </summary>
    public bool restrictWithinPanel = true;
    /// <summary>
    /// 如果内容满了停止拖拽
    /// </summary>
    public bool disableDragIfFits = false;
    /// <summary>
    /// 是否平滑拖拽
    /// </summary>
    public bool smoothDragStart = true;
    /// <summary>
    /// Whether to use iOS drag emulation, where the content only drags at half the speed of the touch/mouse movement when the content edge is within the clipping area.
    /// </summary>	
    public bool iOSDragEmulation = true;
    /// <summary>
    /// 滚轮因子
    /// </summary>
    public float scrollWheelFactor = 0.25f;
    /// <summary>
    /// How much momentum gets applied when the press is released after dragging.
    /// </summary>
    public float momentumAmount = 35f;
    /// <summary>
    /// 水平滚动条
    /// </summary>
    public UIScrollBar horizontalScrollBar;
    /// <summary>
    /// 垂直滚动条
    /// </summary>
    public UIScrollBar verticalScrollBar;
    /// <summary>
    /// 滚动条出现的条件
    /// </summary>
    public ShowCondition showScrollBars = ShowCondition.OnlyIfNeeded;
    /// <summary>
    /// 内容的轴点
    /// </summary>
    public UIWidget.Pivot contentPivot = UIWidget.Pivot.TopLeft;
    /// <summary>
    /// 项视图的根节点
    /// </summary>
    public Transform viewRoot;
    /// <summary>
    /// 每一项的单元格数量
    /// </summary>
    public int cell = 1;

    Transform mTrans;
    UIPanel mPanel;
    Plane mPlane;
    Vector3 mLastPos;
    bool mPressed = false;
    Vector3 mMomentum = Vector3.zero;
    float mScroll = 0f;
    Bounds mBounds;
    bool mCalculatedBounds = false;
    bool mShouldMove = false;
    bool mIgnoreCallbacks = false;
    int mDragID = -10;
    Vector2 mDragStartOffset = Vector2.zero;
    bool mDragStarted = false;

    Vector2 mPanelOffset;
    Vector4 mPanelClipSize;
    Vector3 mPanelPos;

    /// <summary>
    /// Panel that's being dragged.
    /// </summary>
    public UIPanel panel { get { return mPanel; } }

    /// <summary>
    /// Calculate the bounds used by the widgets.
    /// </summary>
    public Bounds bounds
    {
        get
        {
            if (!mCalculatedBounds)
            {
                mCalculatedBounds = true;
                mBounds = CalculateRelativeWidgetBounds(mTrans);
            }
            return mBounds;
        }
    }

    /// <summary>
    /// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
    /// </summary>

    static public Bounds CalculateRelativeWidgetBounds(Transform trans)
    {
        return CalculateRelativeWidgetBounds(trans, trans, false);
    }

    /// <summary>
    /// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
    /// </summary>

    static public Bounds CalculateRelativeWidgetBounds(Transform trans, bool considerInactive)
    {
        return CalculateRelativeWidgetBounds(trans, trans, considerInactive);
    }

    /// <summary>
    /// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
    /// </summary>

    static public Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content)
    {
        return CalculateRelativeWidgetBounds(relativeTo, content, false);
    }

    /// <summary>
    /// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
    /// </summary>
    static public Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content, bool considerInactive)
    {
        if (content != null && relativeTo != null)
        {
            bool isSet = false;
            Matrix4x4 toLocal = relativeTo.worldToLocalMatrix;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            CalculateRelativeWidgetBounds(content, considerInactive, true, ref toLocal, ref min, ref max, ref isSet);
            if (isSet)
            {
                Bounds b = new Bounds(min, Vector3.zero);
                b.Encapsulate(max);
                return b;
            }
        }
        return new Bounds(Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// Recursive function used to calculate the widget bounds.
    /// </summary>
    [System.Diagnostics.DebuggerHidden]
    [System.Diagnostics.DebuggerStepThrough]
    static void CalculateRelativeWidgetBounds(Transform content, bool considerInactive, bool isRoot,
        ref Matrix4x4 toLocal, ref Vector3 vMin, ref Vector3 vMax, ref bool isSet)
    {
        if (content == null) return;
        if (!considerInactive && !NGUITools.GetActive(content.gameObject)) return;

        // If this isn't a root node, check to see if there is a panel present
        UIPanel p = isRoot ? null : content.GetComponent<UIPanel>();
        // Ignore disabled panels as a disabled panel means invisible children
        if (p != null && !p.enabled) return;
        // If there is a clipped panel present simply include its dimensions
        if (p != null && p.clipping != UIDrawCall.Clipping.None)
        {
            Vector3[] corners = GetUIPanelWorldCorners(p);

            for (int j = 0; j < 4; ++j)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(corners[j]);

                if (v.x > vMax.x) vMax.x = v.x;
                if (v.y > vMax.y) vMax.y = v.y;
                if (v.z > vMax.z) vMax.z = v.z;

                if (v.x < vMin.x) vMin.x = v.x;
                if (v.y < vMin.y) vMin.y = v.y;
                if (v.z < vMin.z) vMin.z = v.z;

                isSet = true;
            }
        }
        else // No panel present
        {
            // If there is a widget present, include its bounds
            URect w = content.GetComponent<URect>();

            if (w != null && w.enabled)
            {
                Vector3[] corners = w.worldCorners;

                for (int j = 0; j < 4; ++j)
                {
                    Vector3 v = toLocal.MultiplyPoint3x4(corners[j]);

                    if (v.x > vMax.x) vMax.x = v.x;
                    if (v.y > vMax.y) vMax.y = v.y;
                    if (v.z > vMax.z) vMax.z = v.z;

                    if (v.x < vMin.x) vMin.x = v.x;
                    if (v.y < vMin.y) vMin.y = v.y;
                    if (v.z < vMin.z) vMin.z = v.z;

                    isSet = true;
                }
            }

            // Iterate through children including their bounds in turn
            for (int i = 0, imax = content.childCount; i < imax; ++i)
                CalculateRelativeWidgetBounds(content.GetChild(i), considerInactive, false, ref toLocal, ref vMin, ref vMax, ref isSet);
        }
    }

    // Temporary variable to avoid GC allocation
    static Vector3[] mCorners = new Vector3[4];
    public static Vector3[] GetUIPanelWorldCorners(UIPanel panel)
    {
        if (panel.clipping != UIDrawCall.Clipping.None)
        {
            Vector4 mClipRange = panel.baseClipRegion;
            float x0 = mClipRange.x - 0.5f * mClipRange.z;
            float y0 = mClipRange.y - 0.5f * mClipRange.w;
            float x1 = x0 + mClipRange.z;
            float y1 = y0 + mClipRange.w;

            Transform wt = panel.cachedTransform;
            mCorners[0] = wt.TransformPoint(x0, y0, 0f);
            mCorners[1] = wt.TransformPoint(x0, y1, 0f);
            mCorners[2] = wt.TransformPoint(x1, y1, 0f);
            mCorners[3] = wt.TransformPoint(x1, y0, 0f);
        }
        else
        {
            Vector4 mClipRange = panel.baseClipRegion;
            float x0 = mClipRange.x - 0.5f * mClipRange.z;
            float y0 = mClipRange.y - 0.5f * mClipRange.w;
            float x1 = x0 + mClipRange.z;
            float y1 = y0 + mClipRange.w;

            mCorners[0] = new Vector3(x0, y0);
            mCorners[1] = new Vector3(x0, y1);
            mCorners[2] = new Vector3(x1, y1);
            mCorners[3] = new Vector3(x1, y0);
        }
        return mCorners;
    }

    /// <summary>
    /// Whether the panel should be able to move horizontally (contents don't fit).
    /// </summary>
    public bool shouldMoveHorizontally
    {
        get
        {
            if (Movement.Horizontal == movement)
            {
                float size = bounds.size.x;
                if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
                    size += mPanel.clipSoftness.x * 2f;
                return size > mPanel.baseClipRegion.z;
            }
            return false;
        }
    }

    /// <summary>
    /// Whether the panel should be able to move vertically (contents don't fit).
    /// </summary>
    public bool shouldMoveVertically
    {
        get
        {
            if (Movement.Vertical == movement)
            {
                float size = bounds.size.y;
                if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) size += mPanel.clipSoftness.y * 2f;
                return size > mPanel.baseClipRegion.w;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// Whether the contents of the panel should actually be draggable depends on whether they currently fit or not.
    /// </summary>
    bool shouldMove
    {
        get
        {
            if (!disableDragIfFits) return true;
            if (mPanel == null) mPanel = GetComponent<UIPanel>();
            Vector4 clip = mPanel.baseClipRegion;
            Bounds b = bounds;
            float hx = (clip.z == 0f) ? Screen.width : clip.z * 0.5f;
            float hy = (clip.w == 0f) ? Screen.height : clip.w * 0.5f;
            if (movement == Movement.Horizontal)
            {
                if (b.min.x < clip.x - hx) return true;
                if (b.max.x > clip.x + hx) return true;
            }
            else
            {
                if (b.min.y < clip.y - hy) return true;
                if (b.max.y > clip.y + hy) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Current momentum, exposed just in case it's needed.
    /// </summary>
    public Vector3 currentMomentum { get { return mMomentum; } set { mMomentum = value; mShouldMove = true; } }
    /// <summary>
    /// Cache the transform and the panel.
    /// </summary>
    void Awake()
    {
        mTrans = transform;
        mPanel = GetComponent<UIPanel>();
        mPanel.onGeometryUpdated += OnPanelChange;
        if (mPanelClipSize.x == 0 && mPanelClipSize.y == 0 && mPanelClipSize.z == 0 && mPanelClipSize.w == 0)
        {
            mPanelOffset = mPanel.clipOffset;
            mPanelClipSize = mPanel.baseClipRegion;
            mPanelPos = mPanel.transform.localPosition;
        }
        else
        {
            mPanel.clipOffset = mPanelOffset;
            mPanel.baseClipRegion = mPanelClipSize;
            mPanel.transform.localPosition = mPanelPos;
        }
    }

    /// <summary>
    /// Set the initial drag value and register the listener delegates.
    /// </summary>

    void Start()
    {
        UpdateScrollbars(true);
        if (horizontalScrollBar != null)
        {
            //horizontalScrollBar.onChange += OnHorizontalBar;
            horizontalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveHorizontally) ? 1f : 0f;
        }
        if (verticalScrollBar != null)
        {
            //verticalScrollBar.onChange += OnVerticalBar;
            verticalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveVertically) ? 1f : 0f;
        }
    }

    void OnDestroy()
    {
        if (mPanel != null)
            mPanel.onGeometryUpdated -= OnPanelChange;
    }

    void OnPanelChange() { UpdateScrollbars(true); }

    /// <summary>
    /// Restrict the panel's contents to be within the panel's bounds.
    /// </summary>
    public bool RestrictWithinBounds(bool instant)
    {
        return RestrictWithinBounds(instant, movement == Movement.Horizontal, movement == Movement.Vertical);
    }

    /// <summary>
    /// Restrict the scroll view's contents to be within the scroll view's bounds.
    /// </summary>
    public bool RestrictWithinBounds(bool instant, bool horizontal, bool vertical)
    {
        Bounds b = bounds;
        Vector3 constraint = mPanel.CalculateConstrainOffset(b.min, b.max);

        if (!horizontal) constraint.x = 0f;
        if (!vertical) constraint.y = 0f;

        if (constraint.sqrMagnitude > 0.1f)
        {
            if (!instant && dragEffect == DragEffect.MomentumAndSpring)
            {
                // Spring back into place
                Vector3 pos = mTrans.localPosition + constraint;
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                if (OnDragFloor != null && constraint.y < 0f)
                {
                    OnDragFloor();
                    OnDragFloor = null;
                }
                else
                {
                    SpringPanel.Begin(mPanel.gameObject, pos, 13f).strength = 8f;
                }
            }
            else
            {
                // Jump back into place
                MoveRelative(constraint);
                // Clear the momentum in the constrained direction
                if (Mathf.Abs(constraint.x) > 0.01f) mMomentum.x = 0;
                if (Mathf.Abs(constraint.y) > 0.01f) mMomentum.y = 0;
                if (Mathf.Abs(constraint.z) > 0.01f) mMomentum.z = 0;
                mScroll = 0f;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disable the spring movement.
    /// </summary>
    public void DisableSpring()
    {
        SpringPanel sp = GetComponent<SpringPanel>();
        if (sp != null) sp.enabled = false;
    }

    /// <summary>
    /// Update the values of the associated scroll bars.
    /// </summary>
    public void UpdateScrollbars(bool recalculateBounds)
    {
        if (mPanel == null) return;

        if (horizontalScrollBar != null || verticalScrollBar != null)
        {
            if (recalculateBounds)
            {
                mCalculatedBounds = false;
                mShouldMove = shouldMove;
            }

            Bounds b = bounds;
            Vector2 bmin = b.min;
            Vector2 bmax = b.max;

            if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
            {
                Vector2 soft = mPanel.clipSoftness;
                bmin -= soft;
                bmax += soft;
            }

            if (horizontalScrollBar != null && bmax.x > bmin.x)
            {
                Vector4 clip = mPanel.baseClipRegion;
                float extents = clip.z * 0.5f;
                float min = clip.x - extents - b.min.x;
                float max = b.max.x - extents - clip.x;

                float width = bmax.x - bmin.x;
                min = Mathf.Clamp01(min / width);
                max = Mathf.Clamp01(max / width);

                float sum = min + max;
                mIgnoreCallbacks = true;
                horizontalScrollBar.barSize = 1f - sum;
                horizontalScrollBar.value = (sum > 0.001f) ? min / sum : 0f;
                mIgnoreCallbacks = false;
            }

            if (verticalScrollBar != null && bmax.y > bmin.y)
            {
                Vector4 clip = mPanel.baseClipRegion;
                float extents = clip.w * 0.5f;
                float min = clip.y - extents - bmin.y;
                float max = bmax.y - extents - clip.y;

                float height = bmax.y - bmin.y;
                min = Mathf.Clamp01(min / height);
                max = Mathf.Clamp01(max / height);
                float sum = min + max;

                mIgnoreCallbacks = true;
                verticalScrollBar.barSize = 1f - sum;
                verticalScrollBar.value = (sum > 0.001f) ? 1f - min / sum : 0f;
                mIgnoreCallbacks = false;
            }
        }
        else if (recalculateBounds)
        {
            mCalculatedBounds = false;
        }
    }

    /// <summary>
    /// Changes the drag amount of the panel to the specified 0-1 range values.
    /// (0, 0) is the top-left corner, (1, 1) is the bottom-right.
    /// </summary>
    public void SetDragAmount(float x, float y, bool updateScrollbars)
    {
        if (mPanel == null) mPanel = GetComponent<UIPanel>();
        DisableSpring();

        Bounds b = bounds;
        if (b.min.x == b.max.x || b.min.y == b.max.y) return;
        Vector4 cr = mPanel.baseClipRegion;

        float hx = cr.z * 0.5f;
        float hy = cr.w * 0.5f;
        float left = b.min.x + hx;
        float right = b.max.x - hx;
        float bottom = b.min.y + hy;
        float top = b.max.y - hy;

        if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
        {
            left -= mPanel.clipSoftness.x;
            right += mPanel.clipSoftness.x;
            bottom -= mPanel.clipSoftness.y;
            top += mPanel.clipSoftness.y;
        }

        // Calculate the offset based on the scroll value
        float ox = Mathf.Lerp(left, right, x);
        float oy = Mathf.Lerp(top, bottom, y);

        // Update the position
        if (!updateScrollbars)
        {
            Vector3 pos = mTrans.localPosition;
            if (movement == Movement.Horizontal) pos.x += cr.x - ox;
            else pos.y += cr.y - oy;
            mTrans.localPosition = pos;
        }

        // Update the clipping offset
        cr.x = ox;
        cr.y = oy;
        mPanel.baseClipRegion = cr;

        // Update the scrollbars, reflecting this change
        if (updateScrollbars) UpdateScrollbars(false);
    }

    /// <summary>
    /// Reset the panel's position to the top-left corner.
    /// It's recommended to call this function before AND after you re-populate the panel's contents (ex: switching window tabs).
    /// Another option is to populate the panel's contents, reset its position, then call this function to reposition the clipping.
    /// </summary>
    public void ResetPosition()
    {
        if (this && this.enabled && this.gameObject.activeInHierarchy)
        {
            // Invalidate the bounds
            mCalculatedBounds = false;
            Vector2 pv = GetPivotOffset(contentPivot);
            // First move the position back to where it would be if the scroll bars got reset to zero
            SetDragAmount(pv.x, 1f - pv.y, false);
            // Next move the clipping area back and update the scroll bars
            SetDragAmount(pv.x, 1f - pv.y, true);

            mPanel.clipOffset = mPanelOffset;
            mPanel.baseClipRegion = mPanelClipSize;
            mPanel.transform.localPosition = mPanelPos;
        }
    }

    static public Vector2 GetPivotOffset(UIWidget.Pivot pv)
    {
        Vector2 v = Vector2.zero;

        if (pv == UIWidget.Pivot.Top || pv == UIWidget.Pivot.Center || pv == UIWidget.Pivot.Bottom) v.x = 0.5f;
        else if (pv == UIWidget.Pivot.TopRight || pv == UIWidget.Pivot.Right || pv == UIWidget.Pivot.BottomRight) v.x = 1f;
        else v.x = 0f;

        if (pv == UIWidget.Pivot.Left || pv == UIWidget.Pivot.Center || pv == UIWidget.Pivot.Right) v.y = 0.5f;
        else if (pv == UIWidget.Pivot.TopLeft || pv == UIWidget.Pivot.Top || pv == UIWidget.Pivot.TopRight) v.y = 1f;
        else v.y = 0f;

        return v;
    }

    /// <summary>
    /// Triggered by the horizontal scroll bar when it changes.
    /// </summary>
    void OnHorizontalBar(UIScrollBar sb)
    {
        if (!mIgnoreCallbacks && horizontalScrollBar != null)
        {
            float x = horizontalScrollBar.value;
            UpdateView(x > mLastHoriValue);
            mLastHoriValue = x;
            SetDragAmount(x, 0, false);
        }
    }

    /// <summary>
    /// Triggered by the vertical scroll bar when it changes.
    /// </summary>
    void OnVerticalBar(UIScrollBar sb)
    {
        if (!mIgnoreCallbacks)
        {
            float y = (verticalScrollBar != null) ? verticalScrollBar.value : 0f;
            SetDragAmount(0, y, false);
        }
    }

    /// <summary>
    /// Move the panel by the specified amount.
    /// </summary>
    public void MoveRelative(Vector3 relative)
    {
        mTrans.localPosition += relative;
        Vector4 cr = mPanel.baseClipRegion;
        cr.x -= relative.x;
        cr.y -= relative.y;
        mPanel.baseClipRegion = cr;
        UpdateScrollbars(false);
    }

    /// <summary>
    /// Move the panel by the specified amount.
    /// </summary>
    public void MoveAbsolute(Vector3 absolute)
    {
        Vector3 a = mTrans.InverseTransformPoint(absolute);
        Vector3 b = mTrans.InverseTransformPoint(Vector3.zero);
        MoveRelative(a - b);
    }

    /// <summary>
    /// Create a plane on which we will be performing the dragging.
    /// </summary>
    public void Press(bool pressed)
    {
        if (UICamera.currentScheme == UICamera.ControlScheme.Controller) return;
        if (smoothDragStart && pressed)
        {
            mDragStarted = false;
            mDragStartOffset = Vector2.zero;
        }

        if (enabled && NGUITools.GetActive(gameObject))
        {
            if (!pressed && mDragID == UICamera.currentTouchID) mDragID = -10;

            mCalculatedBounds = false;
            mShouldMove = shouldMove;
            if (!mShouldMove) return;
            mPressed = pressed;

            if (pressed)
            {
                // Remove all momentum on press
                mMomentum = Vector3.zero;
                mScroll = 0f;

                // Disable the spring movement
                DisableSpring();

                // Remember the hit position
                mLastPos = UICamera.lastWorldPosition;

                // Create the plane to drag along
                mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);

                // Ensure that we're working with whole numbers, keeping everything pixel-perfect
                Vector2 co = mPanel.clipOffset;
                co.x = Mathf.Round(co.x);
                co.y = Mathf.Round(co.y);
                mPanel.clipOffset = co;

                Vector3 v = mTrans.localPosition;
                v.x = Mathf.Round(v.x);
                v.y = Mathf.Round(v.y);
                mTrans.localPosition = v;

                if (!smoothDragStart)
                {
                    mDragStarted = true;
                    mDragStartOffset = Vector2.zero;
                    if (onDragStarted != null) onDragStarted();
                }
            }
            else
            {
                if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
                {
                    RestrictWithinBounds(false);
                }
                if (onDragFinished != null) onDragFinished();
                if (!mShouldMove && onStoppedMoving != null)
                    onStoppedMoving();
                if (mAdapter != null && mNeedGC)
                {
                    mAdapter.OnGC();
                    mNeedGC = false;
                }
            }
        }
    }

    /// <summary>
    /// Drag the object along the plane.
    /// </summary>

    public void Drag()
    {
        if (enabled && NGUITools.GetActive(gameObject) && mShouldMove)
        {
            if (mDragID == -10) mDragID = UICamera.currentTouchID;
            UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

            // Prevents the drag "jump". Contributed by 'mixd' from the Tasharen forums.
            if (smoothDragStart && !mDragStarted)
            {
                mDragStarted = true;
                mDragStartOffset = UICamera.currentTouch.totalDelta;
                if (onDragStarted != null) onDragStarted();
            }

            Ray ray = smoothDragStart ?
                UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset) :
                UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

            float dist = 0f;

            if (mPlane.Raycast(ray, out dist))
            {
                Vector3 currentPos = ray.GetPoint(dist);
                Vector3 offset = currentPos - mLastPos;
                mLastPos = currentPos;

                if (offset.x != 0f || offset.y != 0f || offset.z != 0f)
                {
                    offset = mTrans.InverseTransformDirection(offset);
                    if (movement == Movement.Horizontal)
                    {
                        offset.y = 0f;
                        offset.z = 0f;
                    }
                    else
                    {
                        offset.x = 0f;
                        offset.z = 0f;
                    }
                    offset = mTrans.TransformDirection(offset);
                }

                if (dragEffect == DragEffect.None) mMomentum = Vector3.zero;
                // Adjust the momentum
                else mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

                // Move the panel
                if (!iOSDragEmulation || dragEffect != DragEffect.MomentumAndSpring)
                {
                    MoveAbsolute(offset);
                }
                else
                {
                    Vector3 constraint = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);

                    if (constraint.magnitude > 0.001f)
                    {
                        MoveAbsolute(offset * 0.5f);
                        mMomentum *= 0.5f;
                    }
                    else
                    {
                        MoveAbsolute(offset);
                    }
                }

                // We want to constrain the UI to be within bounds
                if (restrictWithinPanel &&
                    mPanel.clipping != UIDrawCall.Clipping.None &&
                    dragEffect != DragEffect.MomentumAndSpring)
                {
                    RestrictWithinBounds(true);
                }
            }
        }
    }

    /// <summary>
    /// If the object should support the scroll wheel, do it.
    /// </summary>
    public void Scroll(float delta)
    {
        if (enabled && NGUITools.GetActive(gameObject) && scrollWheelFactor != 0f)
        {
            DisableSpring();
            mShouldMove = shouldMove;
            if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
            mScroll += delta * scrollWheelFactor;
        }
    }

    /// <summary>
    /// Apply the dragging momentum.
    /// </summary>
    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        float delta = UpdateRealTimeDelta();
        // Fade the scroll bars if needed
        if (showScrollBars != ShowCondition.Always && (verticalScrollBar || horizontalScrollBar))
        {
            bool vertical = false;
            bool horizontal = false;

            if (showScrollBars != ShowCondition.WhenDragging || mDragID != -10 || mMomentum.magnitude > 0.01f)
            {
                vertical = shouldMoveVertically;
                horizontal = shouldMoveHorizontally;
            }

            if (verticalScrollBar)
            {
                float alpha = verticalScrollBar.alpha;
                alpha += vertical ? delta * 6f : -delta * 3f;
                alpha = Mathf.Clamp01(alpha);
                if (verticalScrollBar.alpha != alpha) verticalScrollBar.alpha = alpha;
            }

            if (horizontalScrollBar)
            {
                float alpha = horizontalScrollBar.alpha;
                alpha += horizontal ? delta * 6f : -delta * 3f;
                alpha = Mathf.Clamp01(alpha);
                if (horizontalScrollBar.alpha != alpha) horizontalScrollBar.alpha = alpha;
            }
        }
        if (!mShouldMove) return;
        // Apply momentum
        if (!mPressed)
        {
            if (mMomentum.magnitude > 0.0001f || mScroll != 0f)
            {
                if (movement == Movement.Horizontal)
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, 0f, 0f));
                }
                else
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(0f, mScroll * 0.05f, 0f));
                }
                mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);
                // Move the panel
                Vector3 offset = NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
                MoveAbsolute(offset);

                // Restrict the contents to be within the scroll view's bounds
                if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
                    RestrictWithinBounds(false, movement == Movement.Horizontal, movement == Movement.Vertical);

                if (onMomentumMove != null)
                    onMomentumMove();
            }
            else
            {
                mScroll = 0f;
                mMomentum = Vector3.zero;

                SpringPanel sp = GetComponent<SpringPanel>();
                if (sp != null && sp.enabled) return;

                mShouldMove = false;
                if (onStoppedMoving != null)
                    onStoppedMoving();
                if (mAdapter != null && mNeedGC)
                {
                    mAdapter.OnGC();
                    mNeedGC = false;
                }

            }
        }
        else mScroll = 0f;
        // Dampen the momentum
        NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
    }



#if UNITY_EDITOR
    /// <summary>
    /// Draw a visible orange outline of the bounds.
    /// </summary>
    void OnDrawGizmos()
    {
        if (mPanel != null)// && mPanel.debugInfo == UIPanel.DebugInfo.Gizmos)
        {
            Bounds b = bounds;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.min.z), new Vector3(b.size.x, b.size.y, 0f));
        }
    }
#endif

    void Update()
    {
        UpdateView(mMomentum.x > 0 || mMomentum.y > 0);
    }

    private void UpdateView(bool desc)
    {
        if (mAdapter == null || mChildren == null) return;
        if (desc)
        {
            for (int i = mChildren.Count - 1; i >= 0; i--)
            {
                ItemContainer container = mChildren[i];
                if (container.visible == container.widget.isVisible) continue;

                container.visible = container.widget.isVisible;
                if (container.visible)
                {
                    // 获取可用的缓存项视图
                    ItemView itemView = mAdapter.GetView(i, FindItemView(container));
                    if (itemView.View != null)
                    {
                        Transform tf = itemView.View.transform;
                        tf.name = "lingyi" + i;
                        tf.parent = container.trans;
                        tf.localPosition = Vector3.zero;
                        tf.localScale = Vector3.one;
                        tf.localRotation = Quaternion.Euler(Vector3.zero);
                    }
                }
                else
                {
                    // 释放视图
                    mAdapter.OnRelease(i, container.view);
                    RecycleItemView(container);
                }
                mNeedGC = true;
            }
            if (mIsFirst)
            {
                mIsFirst = false;
                //mAdapter.OnViewCreated();
                if (OnViewCreated != null)
                {
                    OnViewCreated();
                }
            }
        }
        else
        {
            for (int i = 0, len = mChildren.Count; i < len; i++)
            {
                ItemContainer container = mChildren[i];
                if (container.visible == container.widget.isVisible) continue;

                container.visible = container.widget.isVisible;
                if (container.visible)
                {
                    // 获取可用的缓存项视图
                    ItemView itemView = mAdapter.GetView(i, FindItemView(container));
                    if (itemView.View != null)
                    {
                        Transform tf = itemView.View.transform;
                        tf.name = "lingyi" + i;
                        tf.parent = container.trans;
                        tf.localPosition = Vector3.zero;
                        tf.localScale = Vector3.one;
                        tf.localRotation = Quaternion.Euler(Vector3.zero);
                    }
                }
                else
                {
                    // 释放视图
                    mAdapter.OnRelease(i, container.view);
                    RecycleItemView(container);
                }
                mNeedGC = true;
            }
            if (mIsFirst)
            {
                mIsFirst = false;
                //mAdapter.OnViewCreated();
                if (OnViewCreated != null) OnViewCreated();
            }
        }
    }

    public void SetAdapter(IListViewAdapter adapter)
    {
        if (mAdapter != null)
        {
            int k = 0;
            for (int i = 0; i != mChildren.Count; i++)
            {
                if (mChildren[i] == null) continue;
                if (adapter == null && mChildren[i].widget.isVisible)
                {
                    mAdapter.OnRelease(k, mChildren[i].view);
                    k += 1;
                }
                mAdapter.OnDestroy(mChildren[i].go);
                mChildren[i].Destroy();
            }
            mChildren.Clear();
            viewList.Clear();
            mChildren = null;
            mAdapter = null;
        }
        mIsFirst = true;
        mLastViewPos = Vector2.zero;
        mAdapter = adapter;
        if (mAdapter == null) return;
        int count = mAdapter.GetCount();
        mChildren = new List<ItemContainer>(count);
        viewList = new List<ItemView>();

        for (int i = 0; i < count; i++)
        {
            ItemContainer holder = new ItemContainer();
            // go
            holder.go = new GameObject("Rect");
            holder.go.layer = mAdapter.GetLayer();
            holder.trans = holder.go.transform;
            holder.trans.parent = viewRoot;
            holder.trans.localPosition = holder.localPos = mLastViewPos;
            holder.trans.localScale = Vector3.one;
            holder.trans.localRotation = Quaternion.Euler(Vector3.zero);
            // widget
            holder.widget = holder.go.AddComponent<URect>();
            Vector2 size = mAdapter.GetSize(i);
            holder.widget.width = (int)size.x;
            holder.widget.height = (int)size.y;
            holder.widget.panel = mPanel;
            if (movement == Movement.Horizontal)
            {
                if ((i + 1) % cell == 0)
                {
                    mLastViewPos.x += size.x;
                    mLastViewPos.y = 0;
                }
                else mLastViewPos.y -= size.y;
            }
            else
            {
                if ((i + 1) % cell == 0)
                {
                    mLastViewPos.x = 0;
                    mLastViewPos.y -= size.y;
                }
                else mLastViewPos.x += size.x;
            }
            mChildren.Add(holder);
        }
        UpdateScrollbars(true);
        ResetPosition();
    }



    /// <summary>
    /// 项容器
    /// </summary>
    public class ItemContainer
    {
        public ItemContainer()
        {
            Destroy();
            visible = false;
        }

        public void Destroy()
        {
            GameObject.Destroy(go);
            go = null;
            trans = null;
            widget = null;
            view = null;
        }

        public GameObject go;
        public Transform trans;
        public Vector3 localPos;
        public URect widget;
        public bool visible;

        // 视图
        public ItemView view;
    }

    /// <summary>
    /// 项视图
    /// </summary>
    public class ItemView
    {
        /// <summary>
        /// Item视图
        /// </summary>
        public GameObject View { get; set; }
        /// <summary>
        /// 数据体，便于用Holder机制来环节数据绑定的压力
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// 当前视图归属,默认为null,表示无归属
        /// </summary>
        public ItemContainer Container { get; set; }
    }

    public interface IListViewAdapter
    {
        /// <summary>
        /// 获取总数量
        /// </summary>
        /// <returns></returns>
        int GetCount();
        /// <summary>
        /// 获取节点所在的LayerMask
        /// </summary>
        /// <returns></returns>
        int GetLayer();
        /// <summary>
        /// 获取指定项的Widget Size
        /// </summary>
        Vector2 GetSize(int index);
        /// <summary>
        /// 获取指定索引处的视图
        /// </summary>
        /// <returns></returns>
        ItemView GetView(int index, ItemView convertView);
        /// <summary>
        /// 第一屏的视图已全部创建完毕
        /// </summary>
        //void OnViewCreated();
        /// <summary>
        /// 释放此处的资源
        /// </summary>
        void OnRelease(int index, ItemView view);
        /// <summary>
        /// 视图被销毁
        /// </summary>
        void OnDestroy(GameObject go);
        /// <summary>
        /// 可能需要GC了，发生在滚动完毕后，如果没有滚动，是不需要GC的
        /// </summary>
        void OnGC();
    }

    /// <summary>
    /// 获取ItemView
    /// </summary>
    private ItemView FindItemView(ItemContainer container)
    {
        ItemView result = container.view;
        if (result == null)
        {
            bool hasChild = container.trans.childCount > 0;
            GameObject child = null;
            if (hasChild) child = container.trans.GetChild(0).gameObject;

            for (int i = 0, len = viewList.Count; i < len; i++)
            {
                ItemView item = viewList[i];
                if (hasChild)
                {
                    // 让子节点为我所用
                    if (item.View == child)
                    {
                        result = item;
                        break;
                    }
                }
                else
                {
                    // 从缓存列表中查找到一个可用的视图项
                    if (item.View != null && item.Container == null)
                    {
                        result = item;
                        break;
                    }
                }
            }
        }
        if (result == null)
        {
            // 第二步，如果缓存中没有就构建一个视图项出来
            result = new ItemView();
            viewList.Add(result);
        }
        // 和container互相绑定
        container.view = result;
        result.Container = container;
        return result;
    }

    /// <summary>
    /// 回收ItemView
    /// </summary>
    private void RecycleItemView(ItemContainer container)
    {
        if (container.view != null) container.view.Container = null;
        container.view = null;
    }

    private List<ItemView> viewList = null;
    private List<ItemContainer> mChildren = null;
    private IListViewAdapter mAdapter;
    private Vector2 mLastViewPos = Vector2.zero;
    private float mLastHoriValue = 0f;
    private bool mIsFirst = false;
    private bool mNeedGC = false;
    public System.Action OnViewCreated;

    float mRt = 0f;
    float mTimeStart = 0f;
    float mTimeDelta = 0f;
    float mActual = 0f;
    bool mTimeStarted = false;

    protected float UpdateRealTimeDelta()
    {
        mRt = Time.realtimeSinceStartup;

        if (mTimeStarted)
        {
            float delta = mRt - mTimeStart;
            mActual += Mathf.Max(0f, delta);
            mTimeDelta = 0.001f * Mathf.Round(mActual * 1000f);
            mActual -= mTimeDelta;
            if (mTimeDelta > 1f) mTimeDelta = 1f;
            mTimeStart = mRt;
        }
        else
        {
            mTimeStarted = true;
            mTimeStart = mRt;
            mTimeDelta = 0f;
        }
        return mTimeDelta;
    }
}
