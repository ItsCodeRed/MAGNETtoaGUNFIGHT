using System.Collections;
using UnityEngine;

public static class Utils
{
    public static Vector2 WorldToGameSpace(Vector3 point)
    {
        return new Vector2(point.x, point.z);
    }

    public static Vector3 GameToWorldSpace(Vector2 point)
    {
        return new Vector3(point.x, 0, point.y);
    }
}