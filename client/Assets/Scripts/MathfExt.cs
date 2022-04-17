using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AntonsCameraController
{
    public static class MathfExt
    {
        public static float OutOfRangeDistance(float min, float max, float value)
        {
            if (min > max)
                Debug.LogError("MathfExt: min can not be higher then max");

            if (value < min)
                return value - min;
            else if (value > max)
                return value - max;
            else
                return 0f;
        }
    }
}
