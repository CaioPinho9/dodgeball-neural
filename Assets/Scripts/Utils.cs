using System;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static float Distance(float x1, float y1, float x2, float y2)
    {
        //Euclidian Distance
        return (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }

    public static bool IsPrime(int number)
    {
        //Check if a number is prime
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0) return false;
        }

        return true;
    }
}
