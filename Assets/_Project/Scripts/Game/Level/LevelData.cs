using _Project.Scripts.Game.Tile;
using UnityEngine;

namespace _Project.Scripts.Game.Level
{

    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        [Header("Rules")]
        [SerializeField] internal bool dropBombAfterMatch = false;
        [SerializeField] internal int matchCountForBombDrop = 4;
        [SerializeField] internal int matchCountForColorBombDrop = 5;
        [SerializeField] internal int maxCollectibleCount = 3;
        [SerializeField] internal int collectibleCount = 0;
        [Range(0, 1)]
        [SerializeField] internal float chanceForCollectible = 0.1f;
        
        [Space(10)]
        [Header("Pre-Defined Pieces")]
        [SerializeField] internal StartingTile[] startingTiles;
        [SerializeField] internal StartingTile[] startingGamePieces;

    }

}