using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JetEngine
{
    public static class VectorUtils
    {
        # region Vector3 -> Vector2 conversions

        //Creates a new Vector2 with this.x as the new Vector's x and this.y
        //  as the new Vector's y
        public static Vector2 GetXY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        //Creates a new Vector2 with this.x as the new Vector's x and this.z
        //  as the new Vector's y
        public static Vector2 GetXZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        //Creates a new Vector2 with this.y as the new Vector's x and this.z
        //  as the new Vector's y
        public static Vector2 GetYZ(this Vector3 v)
        {
            return new Vector2(v.y, v.z);
        }

        #endregion

        #region Vector2 -> Vector3 conversions

        //Creates a new Vector3 with this.x as the new Vector's x and this.y
        //  as the new Vector's y. Sets the new Vector's z to 0f
        public static Vector3 ToVec3XY(this Vector2 v)
        {
            return new Vector3(v.x, v.y, 0f);
        }

        //Creates a new Vector3 with this.x as the new Vector's x and this.y
        //  as the new Vector's z. Sets the new Vector's y to 0f
        public static Vector3 ToVec3XZ(this Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }

        //Creates a new Vector3 with this.x as the new Vector's y and this.y
        //  as the new Vector's z. Sets the new Vector's x to 0f
        public static Vector3 ToVec3YZ(this Vector2 v)
        {
            return new Vector3(0f, v.x, v.y);
        }

        #endregion
    }
}
