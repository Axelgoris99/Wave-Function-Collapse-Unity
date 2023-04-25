using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Tile", order = 1)]
    public class Tile : ScriptableObject
    {
        [SerializeField] List<Tile> Up = new List<Tile>();
        [SerializeField] List<Tile> Down = new List<Tile>();
        [SerializeField] List<Tile> Left = new List<Tile>();
        [SerializeField] List<Tile> Right = new List<Tile>();
        [SerializeField] GameObject objectToInstantiate;

        /// <summary>
        /// Check that the suggested tile can fit where it should
        /// Also check that the other tile accepts the first one!
        /// </summary>
        /// <param name="other">The tile that will be the neighbor</param>
        /// <param name="direction">Where is the other tile relative to this one</param>
        /// <returns></returns>
        public bool IsMatchingTile(Tile other, Vector2 direction)
        {
            if (direction == Vector2.up)
            {
                return Up.Contains(other) && other.Down.Contains(this);
            }
            else if (direction == Vector2.down)
            {
                return Down.Contains(other) && other.Up.Contains(this);
            }
            else if (direction == Vector2.left)
            {
                return Left.Contains(other) && other.Right.Contains(this);
            }
            else if (direction == Vector2.right)
            {
                return Right.Contains(other) && other.Left.Contains(this);
            }
            return true;
        }
        
        public void InstantiatePrefab(Map map, Vector3 localPosition)
        {
            var GO = MonoBehaviour.Instantiate(objectToInstantiate);
            GO.transform.parent = map.transform;
            GO.transform.localPosition = localPosition;
        }
    }
}
