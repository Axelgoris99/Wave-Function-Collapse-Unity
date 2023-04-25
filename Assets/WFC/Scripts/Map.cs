using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public class Map : MonoBehaviour
    {
        // Size of the map is an array of two integers
        [SerializeField] Vector2Int mapSize = new Vector2Int(5, 5);
        [SerializeField] float cellSize;
        [SerializeField] Tile[] possibleStates; 
        public MapCell[,] mapCellsMatrix;
        public int RowsCount => mapCellsMatrix.GetLength(0);
        public int ColumnsCount => mapCellsMatrix.GetLength(1);
        private MapCell[] mapCellsArray;

        private void Start()
        {
            Initialize();
            FillCells();
            CreateMap();
        }

        void Initialize()
        {
            mapCellsMatrix = new MapCell[mapSize.x, mapSize.y];
            for(int i =0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    mapCellsMatrix[i, j] = new MapCell(this, new Vector2Int(i, j), new List<Tile>(possibleStates));
                }
            }
            mapCellsArray = mapCellsMatrix.Cast<MapCell>().ToArray();
        }

        void FillCells()
        {
            MapCell cell = null;
            do
            {
                var cellsWithoutDefinedState = mapCellsArray.Where(c => c.possibleStatesOnThisCell.Count > 1).ToArray();
                if (cellsWithoutDefinedState.Length < 1) return;
                var minimumNumberOfStateOnACell = cellsWithoutDefinedState.Min(c => c.possibleStatesOnThisCell.Count);
                cell = cellsWithoutDefinedState.First(c => c.possibleStatesOnThisCell.Count == minimumNumberOfStateOnACell);
            }
            // Using a delegate does it so that the function passed will be called in the tryselectstate function, meaning we can iterate once step deeper instead of doing it here!
            while (cell.TrySelectState(possibleStatesOnThisCell => possibleStatesOnThisCell[Random.Range(0, possibleStatesOnThisCell.Count)]));
        }
        
        void CreateMap()
        {
            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    var localPosition = new Vector3(i * cellSize, 0, j * cellSize);
                    mapCellsMatrix[i, j].possibleStatesOnThisCell[0].InstantiatePrefab(this, localPosition);
                }
            }
        }
    }
}
