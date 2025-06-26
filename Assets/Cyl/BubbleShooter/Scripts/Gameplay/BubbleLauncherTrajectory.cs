using UnityEngine;

namespace Cyl.BubbleShooter.Gameplay
{
    /// <summary>
    /// This class is responsible for rendering the trajectory of the bubble launcher.
    /// A LineRenderer is used to visualize the trajectory, and a landing indicator is shown at the end of the trajectory.
    /// </summary>
    public class BubbleLauncherTrajectory : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject landingIndicator;

        private void Awake()
        {
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 1;
            landingIndicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the visibility of the line renderer.
        /// </summary>
        /// <param name="visible">True to make the line renderer visible, false to hide it.</param>
        public void SetLineRendererVisible(bool visible)
        {
            lineRenderer.enabled = visible;
        }
        
        /// <summary>
        /// Sets the trajectory of the line renderer.
        /// </summary>
        /// <param name="trajectory">An array of Vector2 points representing the trajectory.</param>
        /// <param name="length">The number of points in the trajectory.</param>
        public void SetTrajectory(Vector2[] trajectory, int length)
        {
            if (length < 2)
            {
                SetLineRendererVisible(false);
                return;
            }
            
            lineRenderer.positionCount = length;
            for (var i = 0; i < length; i++)
                lineRenderer.SetPosition(i, trajectory[i]);
            SetLineRendererVisible(true);
        }

        /// <summary>
        /// Set the angle of the trajectory.
        /// This is used to align other visual effects with the trajectory.
        /// </summary>
        /// <param name="angle">The angle in degrees to set for the trajectory.</param>
        public void SetAngle(float angle)
        {
            // TODO: Adjust VFX to match the angle
        }
        
        /// <summary>
        /// Sets the visibility of the landing indicator.
        /// </summary>
        /// <param name="active">True to make the landing indicator active, false to deactivate it.</param>
        public void SetLandingIndicatorActive(bool active)
        {
            landingIndicator.SetActive(active);
        }

        /// <summary>
        /// Sets the position of the landing indicator in world space.
        /// </summary>
        /// <param name="worldPosition">The world position to set for the landing indicator.</param>
        public void SetLandingIndicatorPosition(Vector3 worldPosition)
        {
            landingIndicator.transform.position = worldPosition;
        }
    }
}