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

        public abstract void Initialize();
        
        public abstract void OnSpawn();
        
        public abstract void OnDespawn();
    }
}