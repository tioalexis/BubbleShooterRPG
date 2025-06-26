using TMPro;
using UnityEngine;

namespace Cyl.BubbleShooter.Grid
{
    /// <summary>
    /// This class is responsible for creating a visual representation of the bubble grid slots.
    /// Used for debugging and visualizing the grid layout in the Unity editor.
    /// </summary>
    public class BubbleGridSlotView : MonoBehaviour
    {
        [SerializeField] private BubbleGrid grid;
        [SerializeField] private GameObject oddRowPrefab;
        [SerializeField] private GameObject evenRowPrefab;

        private void Start()
        {
            if (grid == null)
                return;
            
            for (var col = 0; col < grid.Width; col++)
            for (var row = 0; row < grid.Height; row++)
            {
                var prefab = (row % 2 == 0) ? evenRowPrefab : oddRowPrefab;
                var instance = Instantiate(prefab, transform);
                instance.transform.position = grid.GetWorldPosition(col, row);
                instance.name = $"Slot ({col}, {row})";
                
                var text = instance.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = $"{col}, {row}";
            }
        }
    }
}