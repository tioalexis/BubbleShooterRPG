using UnityEngine;

namespace Cyl.Common.Utils
{
    public static class DebugDraw
    {
        public static void X(Vector3 center, Color color, float size = 0.5f, float duration = 0.02f)
        {
            var topLeft = center + new Vector3(-size, size, 0f);
            var topRight = center + new Vector3(size, size, 0f);
            var bottomLeft = center + new Vector3(-size, -size, 0f);
            var bottomRight = center + new Vector3(size, -size, 0f);
            
            Debug.DrawLine(topLeft, bottomRight, color, duration);
            Debug.DrawLine(bottomLeft, topRight, color, duration);
        }

        public static void Circle(Vector3 center, float radius, int segments, Color color, float duration = 0.02f)
        {
            if (radius <= 0 || segments < 3)
            {
                Debug.LogError("Invalid parameters for Circle: radius must be > 0 and segments must be >= 3.");
                return;
            }
            
            var angleStep = 360f / segments;
            angleStep *= Mathf.Deg2Rad;
            
            var lineStart = Vector3.zero;
            var lineEnd = Vector3.zero;
            for (var i = 0; i < segments; i++)
            {
                lineStart.x = Mathf.Cos(angleStep * i);
                lineStart.y = Mathf.Sin(angleStep * i);
                
                lineEnd.x = Mathf.Cos(angleStep * (i + 1));
                lineEnd.y = Mathf.Sin(angleStep * (i + 1));
                
                lineStart *= radius;
                lineEnd *= radius;
                
                lineStart += center;
                lineEnd += center;
                
                Debug.DrawLine(lineStart, lineEnd, color, duration);
            }
        }
        
        public static void CircleCast(Vector3 origin, Vector2 direction, float radius, float distance, Color color,
            int granularity = 10, float duration = 0.02f)
        {
            if (radius <= 0 || distance <= 0)
            {
                Debug.LogError("Invalid parameters for CircleCast: radius and distance must be > 0.");
                return;
            }
            
            var endPoint = origin + (Vector3)direction.normalized * distance;
            
            for (var i = 0; i < granularity; i++)
            {
                var t = (float)i / (granularity - 1);
                var pointOnTrajectory = Vector2.Lerp(origin, endPoint, t);
                Circle(pointOnTrajectory, radius, 12, Color.yellow);
            }
        }
    }
}