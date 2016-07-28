using UnityEngine;

public class UnityHelpers
{
    public static void Destroy(UnityEngine.Object theObject)
    {
#if UNITY_EDITOR
        Object.DestroyImmediate(theObject);
#else
        Object.Destroy(theObject);
#endif
    }

/*
    /// <summary>
    /// Set the value of a vector2
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void SetVector2(ref Vector2 vector, float x, float y)
    {
        vector.x = x;
        vector.y = y;
    }

    /// <summary>
    /// Set the value of a vector2
    /// </summary>
    public static void SetVector3(ref Vector3 vector, float x, float y, float z)
    {
        vector.x = x;
        vector.y = y;
        vector.z = z;
    }

    public static void SetRect(ref Rect rect, float x, float y, float width, float height)
    {
        rect.x = x;
        rect.y = y;
        rect.width = width;
        rect.height = height;
    }

    public static void SetRectDimensionsFromNormalTex(ref GUIStyle style, ref Rect rect)
    {
        SetRectDimensionsFromTexture2D(style.normal.background, ref rect);
    }

    public static void SetRectDimensionsFromTexture2D(Texture2D tex, ref Rect rect)
    {
        rect.width = tex.width;
        rect.height = tex.height;
    }
	*/
}
