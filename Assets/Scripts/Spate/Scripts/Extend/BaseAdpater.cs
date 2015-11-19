using UnityEngine;
using System.Collections;
//using System.Collections.Generic;

public abstract class BaseAdpater : UListView.IListViewAdapter
{
	protected GameObject goPrefab;
	private int layerMask = 1;
	protected int gcCount = 0;
	//
	protected Vector2 size;

	public BaseAdpater()
	{
        layerMask = StringUtil.GetNguiLayer();
	}

	public abstract UListView.ItemView GetView(int index, UListView.ItemView convertView);

	public Vector2 GetSize(int index)
	{
		return size;
	}
	public int GetLayer()
	{
		return layerMask;
	}
	public virtual void OnDestroy(GameObject go)
	{
		GameObject.Destroy(go);
	}
	//
	public virtual int GetCount()
	{
		return 0;
	}
	public virtual void OnRelease(int index, UListView.ItemView view)
	{
	}
	public virtual void OnGC()
	{
	}
}
