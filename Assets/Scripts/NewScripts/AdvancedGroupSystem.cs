using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class AdvancedGroupManager : MonoBehaviour
{
    [System.Serializable]
    public class CubeGroup
    {
        public int groupId;
        public List<GameObject> cubes = new List<GameObject>();

        public CubeGroup(int id)
        {
            groupId = id;
            
        }
    }
    public List<CubeGroup> groups = new List<CubeGroup>();
    private Dictionary<GameObject, int> cubeToGroupMap = new Dictionary<GameObject, int>();
    private int nextGroupId = 0;

    // Events
    public System.Action<List<CubeGroup>> OnGroupsUpdated;

    private void Awake()
    {
        NewCutSystem cutSystem = FindObjectOfType<NewCutSystem>();
        if (cutSystem != null)
        {
            cutSystem.OnCutCompleted += () => StartCoroutine(UpdateGroups());
        }
    }
    private void Start()
    {
        // Başlangıçta grupları güncelle
        StartCoroutine(UpdateGroups());
    }

    /// <summary>
    /// BFS Algoritması ile bağlı küpleri gruplar.
    /// </summary>
    /// <param name="startCube"></param>
    /// <param name="visitedCubes"></param>
    private void CreateGroupFromCube(GameObject startCube, HashSet<GameObject> visitedCubes)
    {
        Debug.Log("BFS fonksiyonu başlama yerine girdi");
        CubeGroup newGroup = new CubeGroup(nextGroupId++);
        Queue<GameObject> explorationQueue = new Queue<GameObject>();
        Debug.Log("Enqueue olacak");
        // Başlangıç küpünü kuyruğa ekle
        explorationQueue.Enqueue(startCube);
        visitedCubes.Add(startCube);
        newGroup.cubes.Add(startCube);
        cubeToGroupMap[startCube] = newGroup.groupId;

        Debug.Log($"Enqueue yapıldı.");
        // BFS ile bağlı küpleri bul
        while (explorationQueue.Count > 0)
        {
            GameObject currentCube = explorationQueue.Dequeue();
            CubeGroupControl cubeControl = currentCube.GetComponent<CubeGroupControl>();

            if (cubeControl == null)
            {
                Debug.LogError($"{currentCube.name} üzerinde CubeGroupControl bulunamadı!");
                continue;
            }


            // Bu küpün tüm komşularını kontrol et
            foreach (var neighborPair in cubeControl.neighbors)
            {
                Vector3Int direction = neighborPair.Key;
                GameObject neighborCube = neighborPair.Value;

                // Null kontrol
                if (neighborCube == null)
                {
                    continue;
                }

                // Bu komşu daha önce ziyaret edildi mi?
                if (visitedCubes.Contains(neighborCube))
                {
                    continue;
                }

                // Bu yöndeki bağlantı var mı kontrol et
                // Başlangıçta tüm bağlantılar açık olmalı, kesim sonrası kapanır
                bool isAttached = true; // Default olarak bağlı kabul et

                if (cubeControl.neighborIsAttached.ContainsKey(direction))
                {
                    isAttached = cubeControl.neighborIsAttached[direction];
                }

                

                // Eğer bağlantı kesik değilse bu komşuyu da gruba ekle
                if (isAttached)
                {
                    explorationQueue.Enqueue(neighborCube);
                    visitedCubes.Add(neighborCube);
                    newGroup.cubes.Add(neighborCube);
                    cubeToGroupMap[neighborCube] = newGroup.groupId;

                }
                else
                {
                }
            }
        }

        groups.Add(newGroup);

        Debug.Log($"Grup {newGroup.groupId} tamamlandı: {newGroup.cubes.Count} küp");
    }

    public IEnumerator UpdateGroups()
    {
        yield return new WaitForSeconds(0.7f);

        groups.Clear();
        cubeToGroupMap.Clear();
        nextGroupId = 0;

        HashSet<GameObject> visitedCubes = new HashSet<GameObject>();
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("SoapCube");

        Debug.Log("1 saniye beklendi.");
        foreach (GameObject cube in allCubes)
        {
            if (!visitedCubes.Contains(cube))
            {
                CreateGroupFromCube(cube, visitedCubes);
            }
        }

        //  Burada event tetikle
        OnGroupsUpdated?.Invoke(groups);
    }

    public int GetGroupIdForCube(GameObject cube)
    {
        if (cubeToGroupMap.TryGetValue(cube, out int groupId))
            return groupId;
        return -1;
    }

    public List<GameObject> GetGroupCubes(int groupId)
    {
        return groups.FirstOrDefault(g => g.groupId == groupId)?.cubes;
    }
}