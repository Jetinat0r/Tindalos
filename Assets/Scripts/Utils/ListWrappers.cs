using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListWrappers
{
    [Serializable]
    public class ListWrapper<T>
    {
        public ListWrapper()
        {
            list = new List<T>();
        }

        public List<T> list;
    }

    [Serializable]
    public class Vector2ListWrapper : ListWrapper<Vector2> { }
}
