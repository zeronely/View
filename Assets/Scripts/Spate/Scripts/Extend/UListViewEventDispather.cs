using UnityEngine;
using System.Collections;

public class UListViewEventDispather : MonoBehaviour
{
    /// <summary>
    /// Reference to the scroll view that will be dragged by the script.
    /// </summary>
    public UListView listView;
    // Legacy functionality, kept for backwards compatibility. Use 'scrollView' instead.
    [HideInInspector]
    [SerializeField]
    UListView draggablePanel;
    GameObject mGo;
    UListView mListView;
    bool mAutoFind = false;
    bool mStarted = false;

    /// <summary>
    /// Automatically find the scroll view if possible.
    /// </summary>
    void OnEnable()
    {
        mGo = gameObject;
        // Auto-upgrade
        if (listView == null && draggablePanel != null)
        {
            listView = draggablePanel;
            draggablePanel = null;
        }

        if (mStarted && (mAutoFind || mListView == null))
            FindScrollView();
    }

    /// <summary>
    /// Find the scroll view.
    /// </summary>
    void Start()
    {
        mStarted = true;
        FindScrollView();
    }

    /// <summary>
    /// Find the scroll view to work with.
    /// </summary>
    void FindScrollView()
    {
        // If the scroll view is on a parent, don't try to remember it (as we want it to be dynamic in case of re-parenting)
        UListView sv = NGUITools.FindInParents<UListView>(mGo);

        if (listView == null)
        {
            listView = sv;
            mAutoFind = true;
        }
        else if (listView == sv)
        {
            mAutoFind = true;
        }
        mListView = listView;
    }

    /// <summary>
    /// Create a plane on which we will be performing the dragging.
    /// </summary>
    void OnPress(bool pressed)
    {
        // If the scroll view has been set manually, don't try to find it again
        if (mAutoFind && mListView != listView)
        {
            mListView = listView;
            mAutoFind = false;
        }

        if (listView != null && enabled && NGUITools.GetActive(gameObject))
        {
            listView.Press(pressed);

            if (!pressed && mAutoFind)
            {
                listView = NGUITools.FindInParents<UListView>(mGo);
                mListView = listView;
            }
        }
    }

    /// <summary>
    /// Drag the object along the plane.
    /// </summary>
    void OnDrag(Vector2 delta)
    {
        if (listView != null && GetActive(this))
            listView.Drag();
    }

    /// <summary>
    /// If the object should support the scroll wheel, do it.
    /// </summary>
    void OnScroll(float delta)
    {
        if (listView && GetActive(this))
            listView.Scroll(delta);
    }

    static public bool GetActive(Behaviour mb)
    {
        return mb && mb.enabled && mb.gameObject.activeInHierarchy;
    }
}
