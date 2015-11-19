using UnityEngine;
using System.Collections;

public class UVScroller : MonoBehaviour
{
    public float speedX = 1.0f;
    public float speedY = 1.0f;

    private Material mMat;
    void Start()
    {
        mMat = renderer.material;
    }

    void Update()
    {
        Vector2 vector = mMat.mainTextureOffset;
        vector.x += speedX * Time.deltaTime;
        vector.y += speedY * Time.deltaTime;
        mMat.mainTextureOffset = vector;
    }
}
