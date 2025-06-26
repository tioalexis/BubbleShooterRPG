using UnityEngine;

namespace Cyl.BubbleShooter.Bubbles
{
    [RequireComponent(typeof(Bubble))]
    public abstract class BubbleComponent : MonoBehaviour
    {
        public Bubble Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponent<Bubble>();
        }

        public virtual void Initialize()
        {

        }

        public virtual void OnSpawn()
        {

        }

        public virtual void OnDespawn()
        {
            
        }
    }
}