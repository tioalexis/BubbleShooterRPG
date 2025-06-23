using UnityEngine;

namespace Cyl.BubbleShooter.Gameplay
{
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

        public void SetLineRendererVisible(bool visible)
        {
            lineRenderer.enabled = visible;
        }
        
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

        public void SetAngle(float angle)
        {
            // TODO: Adjust VFX to match the angle
        }
        
        public void SetLandingIndicatorActive(bool active)
        {
            landingIndicator.SetActive(active);
        }

        public void SetLandingIndicatorPosition(Vector3 worldPosition)
        {
            landingIndicator.transform.position = worldPosition;
        }
    }
}