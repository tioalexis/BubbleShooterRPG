using System;
using System.Collections.Generic;
using Cyl.Hexagons;
using UnityEngine;

namespace Cyl.BubbleShooter.Bubbles
{
    /// <summary>
    /// Represents a bubble in the Bubble Shooter game.
    /// By default, a bubble only has a collider and grid position.
    /// This class is not meant to be extended directly; instead, use the BubbleComponent class to add functionality.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Bubble : MonoBehaviour, IHexGridElement
    {
        private readonly Dictionary<Type, BubbleComponent> _components = new();
        private Collider2D _collider2D;
        
        /// <summary>
        /// The hex position of the bubble in the grid.
        /// </summary>
        public Hex GridPosition { get; set; }
        
        /// <summary>
        /// The type of the bubble, which is the name of the prefab used to create it.
        /// </summary>
        public string BubbleType { get; set; }

        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>();
        }

        /// <summary>
        /// Retrieves a bubble component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the bubble component to retrieve.</typeparam>
        /// <returns>The bubble component of the specified type, or null if not found.</returns>
        public T GetBubbleComponent<T>() where T : BubbleComponent
        {
            if (_components.TryGetValue(typeof(T), out var foundComponent))
            {
                return (T)foundComponent;
            }

            Debug.LogError($"Bubble component of type {typeof(T)} not found on bubble {name}.");
            return null;
        }
        
        /// <summary>
        /// Retrieves a bubble component of the specified type, if it exists.
        /// </summary>
        /// <param name="component">The bubble component of the specified type, if found.</param>
        /// <typeparam name="T">The type of the bubble component to retrieve.</typeparam>
        /// <returns>True if the component was found, false otherwise.</returns>
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
        
        /// <summary>
        /// Sets the enabled state of the bubble's collider.
        /// </summary>
        /// <param name="isEnabled">Whether to enable or disable the collider.</param>
        public void SetColliderEnabled(bool isEnabled)
        {
            if (_collider2D != null)
            {
                _collider2D.enabled = isEnabled;
            }
            else
            {
                Debug.LogError("Collider2D not found on bubble " + name);
            }
        }

        /// <summary>
        /// Caches all bubble components on spawn and subsequently initializes them.
        /// </summary>
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

        /// <summary>
        /// Notifies all bubble components that the bubble is being despawned.
        /// </summary>
        public void OnDespawn()
        {
            foreach (var component in _components.Values)
                component.OnDespawn();
        }

        /// <summary>
        /// Invoked when the bubble is added to a grid.
        /// </summary>
        /// <param name="grid">The grid to which the bubble is added.</param>
        public void OnAddedToGrid(HexGrid<IHexGridElement> grid)
        {
            // Do nothing by default
        }
    }

}
