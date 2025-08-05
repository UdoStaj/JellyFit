using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Level System/Level")]
public class Level : ScriptableObject
{
    [Unique]
    [SerializeField]public int LevelID;
    public List<bool> WhichNecesserySlot;
    public string LevelName;
    public string LevelDescription;
    public int SliceValue;
    public int[] JokerUsed = new int[4];
    public float SpentTime;
    public List<Vector2Int> soaps;
    public List<Vector2Int> Box;
    public List<Vector2Int> obstacles;
}
