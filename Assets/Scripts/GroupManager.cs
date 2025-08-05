using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GroupManager : MonoBehaviour
{
    [System.Serializable]
    public class Groups
    {
        public List<GameObject> SoapCubes;
    }

    public List<Groups> groups = new List<Groups>();

    //private void Start()
    //{
    //    // Event listener'ı ekle
    //    SliceManager.OnCutPerformed += OnCutPerformed;

    //    // Küplerin komşuluk tespitinden sonra grupları güncelle
    //    Invoke(nameof(UpdateGroups), 1.0f); // Daha uzun bekle
    //}

    //private void OnDestroy()
    //{
    //    // Event listener'ı temizle
    //    SliceManager.OnCutPerformed -= OnCutPerformed;
    //}

    // Kesme işlemi tamamlandığında çağrılacak fonksiyon
    private void OnCutPerformed()
    {
        Debug.Log("Kesme işlemi tamamlandı, gruplar güncelleniyor...");
        UpdateGroups();
    }

    public void CreateGroup()
    {
        Groups newGroup = new Groups();
        newGroup.SoapCubes = new List<GameObject>();
        groups.Add(newGroup);
    }

    public void AddCubeToGroup(GameObject cube, int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groups.Count)
        {
            Debug.LogError("Group index out of range.");
            return;
        }

        groups[groupIndex].SoapCubes.Add(cube);
    }

    public void RemoveCubeFromGroup(GameObject cube, int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groups.Count)
        {
            Debug.LogError("Group index out of range.");
            return;
        }

        if (groups[groupIndex].SoapCubes.Contains(cube))
        {
            groups[groupIndex].SoapCubes.Remove(cube);
        }
        else
        {
            Debug.LogWarning("Cube not found in the specified group.");
        }
    }

    // YENİ: Gruplama algoritması
    public void UpdateGroups()
    {
        Debug.Log("=== GRUP GÜNCELLEME BAŞLIYOR ===");

        // Tüm küpleri bul
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("SoapCube");
        Debug.Log($"Toplam {allCubes.Length} SoapCube bulundu.");

        // Her küpün komşuluk durumunu kontrol et
        foreach (GameObject cube in allCubes)
        {
            CubeGroupControl cubeControl = cube.GetComponent<CubeGroupControl>();
            if (cubeControl != null)
            {
                Debug.Log($"--- {cube.name} komşuluk durumu ---");
                Debug.Log($"Toplam komşu sayısı: {cubeControl.neighbors.Count}");
                Debug.Log($"Bağlı komşu sayısı: {cubeControl.neighborIsAttached.Count(kvp => kvp.Value)}");
                cubeControl.LogNeighbors();
            }
            else
            {
                Debug.LogWarning($"{cube.name} üzerinde CubeGroupControl scripti yok!");
            }
        }

        // Grupları temizle
        groups.Clear();

        // Ziyaret edilmiş küpleri takip et
        HashSet<GameObject> visited = new HashSet<GameObject>();

        foreach (GameObject cube in allCubes)
        {
            if (!visited.Contains(cube))
            {
                // Yeni grup oluştur
                Groups newGroup = new Groups();
                newGroup.SoapCubes = new List<GameObject>();
                groups.Add(newGroup);

                Debug.Log($"Yeni grup oluşturuluyor, başlangıç küpü: {cube.name}");

                // Bu küpten başlayarak bağlı tüm küpleri bul
                FindConnectedCubes(cube, newGroup, visited);

                Debug.Log($"Grup tamamlandı. Toplam küp sayısı: {newGroup.SoapCubes.Count}");
            }
        }

        // Debug: Grupları logla
        LogGroups();

        Debug.Log("=== GRUP GÜNCELLEME TAMAMLANDI ===");
    }

    private void FindConnectedCubes(GameObject startCube, Groups group, HashSet<GameObject> visited)
    {
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(startCube);
        visited.Add(startCube);
        group.SoapCubes.Add(startCube);

        Debug.Log($"BFS başlatılıyor: {startCube.name}");

        while (queue.Count > 0)
        {
            GameObject currentCube = queue.Dequeue();
            CubeGroupControl cubeControl = currentCube.GetComponent<CubeGroupControl>();

            if (cubeControl != null)
            {
                Debug.Log($"İşleniyor: {currentCube.name}, Komşu sayısı: {cubeControl.neighbors.Count}");

                // Bu küpün bağlı komşularını kontrol et
                foreach (var kvp in cubeControl.neighbors)
                {
                    Vector3Int direction = kvp.Key;
                    GameObject neighbor = kvp.Value;

                    if (neighbor == null)
                    {
                        Debug.LogWarning($"{currentCube.name} - {direction} yönünde null komşu!");
                        continue;
                    }

                    bool isAttached = cubeControl.neighborIsAttached.ContainsKey(direction) &&
                                     cubeControl.neighborIsAttached[direction];

                    Debug.Log($"{currentCube.name} -> {neighbor.name} ({direction}): Bağlı = {isAttached}, Ziyaret edildi = {visited.Contains(neighbor)}");

                    // Eğer komşu bağlıysa ve ziyaret edilmemişse
                    if (isAttached && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        group.SoapCubes.Add(neighbor);

                        Debug.Log($"Gruba eklendi: {neighbor.name}");
                    }
                }
            }
            else
            {
                Debug.LogError($"{currentCube.name} üzerinde CubeGroupControl bulunamadı!");
            }
        }
    }

    private void LogGroups()
    {
        Debug.Log($"=== TOPLAM {groups.Count} GRUP BULUNDU ===");
        for (int i = 0; i < groups.Count; i++)
        {
            Debug.Log($"Grup {i}: {groups[i].SoapCubes.Count} küp");
            foreach (var cube in groups[i].SoapCubes)
            {
                Debug.Log($"  - {cube.name}");
            }
        }
        Debug.Log("=====================================");
    }

    // Manual test için
    [ContextMenu("Force Update Groups")]
    public void ForceUpdateGroups()
    {
        UpdateGroups();
    }
}