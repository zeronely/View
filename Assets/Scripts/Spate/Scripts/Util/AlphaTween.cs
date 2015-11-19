using UnityEngine;
using Spate;

public class AlphaTween : MonoBehaviour
{
    private Material mMaterial;

    void Start()
    {
        mMaterial = renderer.material;
        if (mMaterial == null)
        {
            Debug.LogError("No renderer.material!");
            Destroy(this);
        }
    }

    void Update()
    {
        Color color = CurrentColor;
        color.a -= (0.5f * Time.deltaTime);
        CurrentColor = color;
        if (color.a <= 0)
        {
            Destroy(this);
        }
    }

    private Color CurrentColor
    {
        get
        {
            return mMaterial.GetColor("_TintColor");
        }
        set
        {
            mMaterial.SetColor("_TintColor", value);
        }
    }
}
