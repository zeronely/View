using UnityEngine;
using System.Collections;

public class URect : MonoBehaviour
{
    public enum Pivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    // Cached and saved values
    [HideInInspector]
    [SerializeField]
    protected Pivot mPivot = Pivot.Center;
    [HideInInspector]
    [SerializeField]
    protected int mWidth = 100;
    [HideInInspector]
    [SerializeField]
    protected int mHeight = 100;


    protected GameObject mGo;
    protected Transform mTrans;

    [System.NonSerialized]
    protected Vector3[] mCorners = new Vector3[4];


    public GameObject cachedGameObject { get { if (mGo == null) mGo = gameObject; return mGo; } }
    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }
    protected int minWidth { get { return 2; } }
    protected int minHeight { get { return 2; } }


    public UIPanel panel { get; set; }

    public Pivot pivot
    {
        get
        {
            return mPivot;
        }
        set
        {
            if (mPivot != value)
            {
                Vector3 before = worldCorners[0];
                mPivot = value;
                Vector3 after = worldCorners[0];

                Transform t = cachedTransform;
                Vector3 pos = t.position;
                float z = t.localPosition.z;
                pos.x += (before.x - after.x);
                pos.y += (before.y - after.y);
                cachedTransform.position = pos;

                pos = cachedTransform.localPosition;
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                pos.z = z;
                cachedTransform.localPosition = pos;
            }
        }
    }

    public Vector2 pivotOffset
    {
        get
        {
            Vector2 v = Vector2.zero;
            Pivot pv = mPivot;

            if (pv == URect.Pivot.Top || pv == URect.Pivot.Center || pv == URect.Pivot.Bottom) v.x = 0.5f;
            else if (pv == URect.Pivot.TopRight || pv == URect.Pivot.Right || pv == URect.Pivot.BottomRight) v.x = 1f;
            else v.x = 0f;

            if (pv == URect.Pivot.Left || pv == URect.Pivot.Center || pv == URect.Pivot.Right) v.y = 0.5f;
            else if (pv == URect.Pivot.TopLeft || pv == URect.Pivot.Top || pv == URect.Pivot.TopRight) v.y = 1f;
            else v.y = 0f;

            return v;
        }
    }


    public int width
    {
        get { return mWidth; }
        set
        {
            mWidth = value;
            if (mWidth < minWidth) mWidth = minWidth;
        }

    }

    public int height
    {
        get { return mHeight; }
        set
        {
            mHeight = value;
            if (mHeight < minHeight) mHeight = minHeight;
        }
    }


    public bool isVisible
    {
        get
        {
            if (panel == null) return false;
            Vector3[] corners = worldCorners;
            return panel.IsVisible(corners[0], corners[1], corners[2], corners[3]);
        }
    }


    public Vector3[] worldCorners
    {
        get
        {
            Vector2 offset = pivotOffset;
            float x0 = -offset.x * mWidth;
            float y0 = -offset.y * mHeight;
            float x1 = x0 + mWidth;
            float y1 = y0 + mHeight;

            Transform wt = cachedTransform;

            mCorners[0] = wt.TransformPoint(x0, y0, 0f);
            mCorners[1] = wt.TransformPoint(x0, y1, 0f);
            mCorners[2] = wt.TransformPoint(x1, y1, 0f);
            mCorners[3] = wt.TransformPoint(x1, y0, 0f);

            return mCorners;
        }
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        Vector2 offset = pivotOffset;
        Vector3 center = new Vector3(mWidth * (0.5f - offset.x), mHeight * (0.5f - offset.y), 0);
        Vector3 size = new Vector3(mWidth, mHeight, 1f);

        // Draw the gizmo
        Gizmos.matrix = cachedTransform.localToWorldMatrix;
        Gizmos.color = (UnityEditor.Selection.activeGameObject == cachedTransform) ? Color.white : Color.green;
        Gizmos.DrawWireCube(center, size);
    }

#endif
}
