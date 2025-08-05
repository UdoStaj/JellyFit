/*using NUnit.Framework;
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
    public Transform CreatePoint;
    public List<Level> allLevels;
    public List<GameObject> LoadedLevels;
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

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        if (CreatePoint == null)
        {
            CreatePoint = new GameObject("CreatePoint").transform;
            CreatePoint.position = new Vector3(50, 100, 100);
        }
        if (currentLevel == null)
        {
            currentLevel = allLevels[0];
        }

        if (LoadedLevels == null)
        {
            LoadedLevels = new List<GameObject>();
        }
    }
    private void Start()
    {

        if (LoadedLevels.Count == 0)
        { //ilk 6 lvl yükleniyor ayrý framelerde. Her yüklediðinde Load Levele yüklüyor.
            destroyInGameLevel();
            LoadedLevels.Add(Instantiate(allLevels[0].LevelPrefab));
            LoadedLevels.First().transform.position = LevelInGameParent.position;
            CreatePoint.position = NewCreatePoint(CreatePoint.position);
            StartCoroutine(PreloadNextLevelSlow(allLevels[1]));
            StartCoroutine(PreloadNextLevelSlow(allLevels[2]));
            StartCoroutine(PreloadNextLevelSlow(allLevels[3]));
            StartCoroutine(PreloadNextLevelSlow(allLevels[4]));
            StartCoroutine(PreloadNextLevelSlow(allLevels[5]));

        }
        if (currentLevelInstance == null && LoadedLevels.Count > 0)
        {
            currentLevelInstance = LoadedLevels.First();
        }
    }

    /// <summary>
    /// level yükleme fonksiyonu. suanda daha cok ilk levelden sonra sýrayla calýsýyo.
    /// </summary>
    /// <param name="LevelID"></param>
    public void LoadLevel(int LevelID) //LevelManager.Instance.LoadLevel(1) 
    {
        destroyInGameLevel();
        if (currentLevel.LevelID == LevelID)
        {
            reloadLevel();
        }
        else
        {
            if (LoadedLevels.Count > 1)
            {
                LoadedLevels.First().transform.position = LevelInGameParent.transform.position;
                currentLevelInstance = LoadedLevels.First();
                LoadedLevels.Remove(LoadedLevels.First());
                currentLevel = FoundMyLevel(LevelID);
            }
            else if (LoadedLevels.Count == 1)
            {
                LoadedLevels.First().transform.position = LevelInGameParent.transform.position;
                currentLevelInstance = LoadedLevels.First();
                LoadedLevels.Remove(LoadedLevels.First());
                currentLevel = FoundMyLevel(LevelID);
                StartCoroutine(PreloadNextLevelSlow(FoundMyLevel(LevelID + 1)));
            }
            else if (LoadedLevels.Count <= 0)
            {
                IsGameEnd = true;
            }
        }
        PlayerPrefs.SetInt("Current Level", LevelID);
    }
    public void destroyInGameLevel()
    {
        if (currentLevelInstance != null)
        {
            currentLevelInstance.transform.position = destroyArea;
            StartCoroutine(LateDestroyLevel(currentLevelInstance));
        }
    }
    public void NextLevel()
    {
        LoadLevel(currentLevel.LevelID + 1);
    }
    public void reloadLevel()
    {
        destroyInGameLevel();
        Instantiate(currentLevel.LevelPrefab, CreatePoint);
        CreatePoint.position = NewCreatePoint(CreatePoint.position);
        LoadedLevels.Last().transform.position = LevelInGameParent.position;
    }

    private IEnumerator PreloadNextLevelSlow(Level levelData)
    {
        GameObject Parent = Instantiate(levelData.LevelPrefab.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Level").gameObject);
        if (Parent == null)
        {
            Debug.LogError("ParentObjectName can not found!");
            yield break;
        }
        foreach (var obj in levelData.LevelPrefab.GetComponentsInChildren<Transform>())
        {
            Instantiate(obj.gameObject, Parent.transform);
            yield return null;
        }
        Parent.transform.position = CreatePoint.position;
        CreatePoint.position = NewCreatePoint(CreatePoint.position);
        LoadedLevels.Add(Parent);
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
}*/