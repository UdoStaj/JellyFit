using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find existing instance in scene
                _instance = FindAnyObjectByType<LevelManager>();
                if (_instance == null)
                {
                    // Create new GameObject if none exists
                    GameObject singletonObj = new GameObject("LevelManager");
                    _instance = singletonObj.AddComponent<LevelManager>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    public bool IsGameEnd;
    public Transform LevelInGameParent;
    private Level _currentLevel;
    public Level currentLevel
    {
        get
        {
            return _currentLevel;
        }
        set
        {
            _currentLevel = value;
        }
    }
    private GameObject currentLevelInstance;
    private float NewEpisodeDistance = 50f;
    
    private Vector3 destroyArea = new Vector3(0, 1000, 1000);
    private List<Level> allLevels;
    public GridSystem MyGrid;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        MyGrid = GetComponent<GridSystem>();
    }
    private void Start()
    {
        
        
    }

    /// <summary>
    /// level yükleme fonksiyonu. suanda daha cok ilk levelden sonra sýrayla calýsýyo.
    /// </summary>
    /// <param name="LevelID"></param>
    public void LoadLevel(int LevelID) //LevelManager.Instance.LoadLevel(1) 
    {
        
    }
    public void destroyInGameLevel()
    {
        if(MyGrid != null)
        {
            MyGrid.soapCells.Clear();
            MyGrid.boxCells.Clear();
            MyGrid.obstacleCells.Clear();
        }
    }
    public void NextLevel()
    {
        LoadLevel(currentLevel.LevelID + 1);
    }
    public void reloadLevel()
    {
        destroyInGameLevel();
    }

    private IEnumerator LateDestroyLevel(GameObject parent)
    {
        foreach (Transform level in parent.GetComponentsInChildren<Transform>())
        {
            Destroy(level.gameObject);
            yield return null;
        }
        Destroy(parent);
    }
    private Vector3 NewCreatePoint(Vector3 point)
    {
        point += new Vector3(NewEpisodeDistance, 0, 0);
        return point;
    }
    private Level FoundMyLevel(int LevelID)
    {
        foreach (var level in allLevels)
        {
            if (level.LevelID == LevelID)
            {
                return level;
            }
        }
        return null;
    }
}