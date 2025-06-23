using UnityEngine;

namespace Cyl.Common.Utils
{
    public static class DebugDraw
    {
        public static void X(Vector3 center, Color color, float size = 0.5f, float duration = 0.1f)
        {
            var topLeft = center + new Vector3(-size, size, 0f);
            var topRight = center + new Vector3(size, size, 0f);
            var bottomLeft = center + new Vector3(-size, -size, 0f);
            var bottomRight = center + new Vector3(size, -size, 0f);
            
            Debug.DrawLine(topLeft, bottomRight, color, duration);
            Debug.DrawLine(bottomLeft, topRight, color, duration);
        }
    }
}