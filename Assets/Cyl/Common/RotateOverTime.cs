using UnityEngine;

namespace Cyl.Common
{
    /// <summary>
    /// Rotates the GameObject over time based on the specified rotation speed.
    /// </summary>
    public class RotateOverTime : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 0f, 30f);
        
        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}