using System;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static float Distance(float x1, float y1, float x2, float y2)
    {
        //Euclidian Distance
        return (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }
}
