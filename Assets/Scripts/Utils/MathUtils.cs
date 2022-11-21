using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JetEngine
{
    public static class MathUtils
    {
        public static int GetClosestEvenDivisor(int number, int divisor)
        {
            //Get the quotient
            int quotient = (int)(number / divisor);

            //Get the remainder
            int remainder = number - (quotient * divisor);

            //How we handle the outcome changes slightly for positive and negative numbers
            if (number >= 0)
            {
                //If remainder - divisor < 0.5, then it is closer to the lower bound, else, go up one
                return ((float)remainder / divisor < 0.5) ? quotient * divisor : (quotient + 1) * divisor;
            }
            else
            {
                //If remainder - divisor < 0.5, then it is closer to the lower bound, else, go down one
                return (Math.Abs((float)remainder / divisor) < 0.5) ? quotient * divisor : (quotient - 1) * divisor;
            }
        }

        public static float GetClosestEvenDivisor(float number, float divisor)
        {
            //Get the quotient
            int quotient = (int)(number / divisor);

            //Get the remainder
            float remainder = number - (quotient * divisor);

            //How we handle the outcome changes slightly for positive and negative numbers
            if (number >= 0)
            {
                //If remainder - divisor < 0.5, then it is closer to the lower bound, else, go up one
                return (remainder / divisor < 0.5) ? quotient * divisor : (quotient + 1) * divisor;
            }
            else
            {
                //If remainder - divisor < 0.5, then it is closer to the lower bound, else, go down one
                return (Math.Abs(remainder / divisor) < 0.5) ? quotient * divisor : (quotient - 1) * divisor;
            }
        }
    }
}