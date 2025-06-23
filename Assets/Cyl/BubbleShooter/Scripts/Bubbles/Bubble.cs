using System;
using System.Collections.Generic;
using Cyl.Hexagons;
using UnityEngine;

namespace Cyl.BubbleShooter.Bubbles
{
    public class Bubble : MonoBehaviour, IHexGridElement
    {
        private readonly Dictionary<Type, BubbleComponent> _components = new();
        
        public Hex GridPosition { get; set; }
        
        public T GetBubbleComponent<T>() where T : BubbleComponent
        {
            if (_components.TryGetValue(typeof(T), out var foundComponent))
            {
                return (T)foundComponent;
            }

            Debug.LogError($"Bubble component of type {typeof(T)} not found on bubble {name}.");
            return null;
        }
        
        public bool TryGetBubbleComponent<T>(out T component) where T : BubbleComponent
        {
            if (_components.TryGetValue(typeof(T), out var foundComponent))
            {
                component = (T)foundComponent;
                return true;
            }

            component = null;
            return false;
        }

        public void OnSpawn()
        {
            _components.Clear();
            
            var bubbleComponents = GetComponents<BubbleComponent>();
            foreach (var component in bubbleComponents)
            {
                if (component == null) continue;
                component.Initialize();
                _components[component.GetType()] = component;
            }
            
            foreach (var component in _components.Values)
                component.OnSpawn();
        }

        public void OnDespawn()
        {
            foreach (var component in _components.Values)
                component.OnDespawn();
        }

        public void OnAddedToGrid(HexGrid<IHexGridElement> grid)
        {
            
        }
    }

}
