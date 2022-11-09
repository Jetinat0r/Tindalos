using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jagged2DArraySerializer
{
    [System.Serializable]
    public struct Jagged2DArrayPackage<TElement>
    {
        public int HorizontalSize;
        public List<int> VerticalSizes;
        public List<Jagged2DArrayElementPackage<TElement>> Array;
        public Jagged2DArrayPackage(int hSize, List<int> vSizes, List<Jagged2DArrayElementPackage<TElement>> array)
        {
            HorizontalSize = hSize;
            VerticalSizes = vSizes;
            Array = array;
        }
    }

    [System.Serializable]
    public struct Jagged2DArrayElementPackage<TElement>
    {
        public int Index0;
        public int Index1;
        public TElement Element;
        public Jagged2DArrayElementPackage(int idx0, int idx1, TElement element)
        {
            Index0 = idx0;
            Index1 = idx1;
            Element = element;
        }
    }
}
