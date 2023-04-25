using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

namespace WFC
{
    public class MapCell
    {
        public Vector2Int positionInMap { get; private set; }
        public List<Tile> possibleStatesOnThisCell { get; private set; }
        public List<Vector2Int> adjacentCellsPosition { get; private set; }
        public Map map;
        private Dictionary<MapCell, Tile[]> mapCellCache = new Dictionary<MapCell, Tile[]>();

        public MapCell(Map mapCurrent, Vector2Int positionOnTheMap, List<Tile> tiles)
        {
            possibleStatesOnThisCell = tiles;
            positionInMap = positionOnTheMap;
            adjacentCellsPosition = GetAdjacentCellsPositions(mapCurrent);
            map = mapCurrent;
        }
        List<Vector2Int> GetAdjacentCellsPositions(Map map)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            if (positionInMap.x - 1 >= 0) cells.Add(new Vector2Int(positionInMap.x - 1, positionInMap.y));
            if (positionInMap.x + 1 < map.RowsCount) cells.Add(new Vector2Int(positionInMap.x + 1, positionInMap.y));
            if (positionInMap.y - 1 >= 0) cells.Add(new Vector2Int(positionInMap.x, positionInMap.y - 1));
            if (positionInMap.y + 1 < map.ColumnsCount) cells.Add(new Vector2Int(positionInMap.x, positionInMap.y + 1));
            return cells;
        }

        public delegate Tile GetCellAction(List<Tile> possibleStates);

        public bool TrySelectState(GetCellAction getCellAction)
        {
            AddOrUpdateToMapCellCashe(this);
            var states = new List<Tile>(possibleStatesOnThisCell);
            while(states.Count > 0)
            {
                // getCellAction will return with what we passed in the map function !
                var selectState = getCellAction(states);
                possibleStatesOnThisCell = new List<Tile>() { selectState };
                if (!TryUpdateAdjacentCells(this))
                {
                    states.Remove(selectState);
                }
                else return true;
            }
            return false;
        }

    
        delegate bool TryUpdateAction();
        bool TryUpdateAdjacentCells(MapCell cellWithSelectedState)
        {
            List<TryUpdateAction> updateAdjacentCellsActions = new List<TryUpdateAction>();
            bool updateSuccess = adjacentCellsPosition.All(cellPos =>
            {
                return map.mapCellsMatrix[cellPos.x, cellPos.y].TryUpdateStates(this, cellWithSelectedState, updateAdjacentCellsActions);
            });
            if (!updateSuccess)
            {
                ReverseStates(cellWithSelectedState);
                return false;
            }
            else
                return updateAdjacentCellsActions.All(action => action.Invoke());
        }

        bool TryUpdateStates(MapCell otherCell, MapCell cellWithSelectedState, List<TryUpdateAction> updateAdjacentCellsActions)
        {
            AddOrUpdateToMapCellCashe(cellWithSelectedState);

            int removeModuleCount = possibleStatesOnThisCell.RemoveAll(thisState =>
            {
                var directionToPreviousCell = otherCell.positionInMap - positionInMap;
                return !otherCell.possibleStatesOnThisCell.Any(otherState => thisState.IsMatchingTile(otherState, directionToPreviousCell));
            });

            if (possibleStatesOnThisCell.Count == 0)
                return false;

            if (removeModuleCount > 0)
                updateAdjacentCellsActions.Add(() => TryUpdateAdjacentCells(cellWithSelectedState));

            return true;
        }

        public void ReverseStates(MapCell originallyUpdatedCell)
        {
            if (mapCellCache.ContainsKey(originallyUpdatedCell))
            {
                possibleStatesOnThisCell = new List<Tile>(mapCellCache[originallyUpdatedCell]);
                mapCellCache.Remove(originallyUpdatedCell);
                foreach (var cellPos in adjacentCellsPosition)
                {
                    map.mapCellsMatrix[cellPos.x, cellPos.y].ReverseStates(originallyUpdatedCell);
                }
            }
        }
        void AddOrUpdateToMapCellCashe(MapCell originallyUpdatedCell)
        {
            if (mapCellCache.ContainsKey(originallyUpdatedCell)) mapCellCache[originallyUpdatedCell] = possibleStatesOnThisCell.ToArray();
            else mapCellCache.Add(originallyUpdatedCell, possibleStatesOnThisCell.ToArray());
        }
    }
}
