using Cyl.BubbleShooter.Bubbles;
using UnityEngine;

namespace Cyl.BubbleShooter.BubbleComponents
{
    public class HealthComponent : BubbleComponent
    {
        public int Value { get; private set; } = 1;
        
        public void ApplyDamage(int damage)
        {
            Value -= damage;
            Value = Mathf.Clamp(Value, 0, int.MaxValue);
        }
    }
}