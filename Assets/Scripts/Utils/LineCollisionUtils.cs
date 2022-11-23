using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public static class LineCollisionUtils
{
    public static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rect r)
    {
        return LineIntersectsLine(p1, p2, new Vector2(r.x, r.y), new Vector2(r.x + r.width, r.y)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.x + r.width, r.y), new Vector2(r.x + r.width, r.y + r.height)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.x + r.width, r.y + r.height), new Vector2(r.x, r.y + r.height)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.x, r.y + r.height), new Vector2(r.x, r.y)) ||
               (r.Contains(p1) && r.Contains(p2));
    }

    public static bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
    {
        float q = (l1p1.y - l2p1.y) * (l2p2.x - l2p1.x) - (l1p1.x - l2p1.x) * (l2p2.y - l2p1.y);
        float d = (l1p2.x - l1p1.x) * (l2p2.y - l2p1.y) - (l1p2.y - l1p1.y) * (l2p2.x - l2p1.x);

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = (l1p1.y - l2p1.y) * (l1p2.x - l1p1.x) - (l1p1.x - l2p1.x) * (l1p2.y - l1p1.y);
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }
}
