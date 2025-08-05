using UnityEngine;
using System.Collections.Generic;

public class HighlightSpawner : MonoBehaviour
{
    public GameObject highlightPrefab;

    [Header("Spawn Settings")]
    public float spawnHeight = 1.02f; // Y ekseninde sabit yükseklik
    public float cellSizeOffset = 0f; // Gerekirse pozisyonu ileri geri ayarlamak için

    private List<GameObject> highlights = new List<GameObject>();
    private float cellSize;
    public GameObject knife;
    private SliceManager sliceManager;

    GridSystem grid;

    void Start()
    {
        grid = FindObjectOfType<GridSystem>();
        if (grid == null)
        {
            Debug.LogError("HighlightVisualizer: GridSystem bulunamadı.");
            enabled = false;
            return;
        }

        cellSize = grid.cellSize;

        

        sliceManager = FindObjectOfType<SliceManager>();
        if (sliceManager == null)
        {
            Debug.LogError("HighlightVisualizer: SliceManager bulunamadı.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        int count = sliceManager.lineCount;

        while (highlights.Count > count)
        {
            Destroy(highlights[highlights.Count - 1]);
            highlights.RemoveAt(highlights.Count - 1);
        }

        while (highlights.Count < count)
        {
            GameObject g = Instantiate(highlightPrefab, transform);
            highlights.Add(g);
        }

        Vector3 basePos = knife.transform.position;
        Vector3 right = knife.transform.right;
        Vector3 rotation = knife.transform.rotation.eulerAngles;

        basePos.y = spawnHeight; // ← Artık sabit y değeri
        basePos += right * cellSizeOffset; // İstersen ileri/geri kaydırmak için (opsiyonel)

        for (int i = 0; i < count; i++)
        {
            int offsetIndex = (i + 1) / 2;
            float offset = offsetIndex * cellSize;
            offset *= (i % 2 == 0) ? 0 : (i % 4 == 1 ? 1 : -1); // 0, +1, -1, +2, -2...

            Vector3 pos = basePos + right * offset;
            highlights[i].transform.position = pos;
            if(Mathf.Abs(knife.transform.rotation.eulerAngles.y)==90|| Mathf.Abs(knife.transform.rotation.eulerAngles.y) == 270)
                highlights[i].transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                highlights[i].transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        CalculateGridField();
    }

    void CalculateGridField()
    {
        foreach (GameObject highlight in highlights)
        {
            Vector3 pos = knife.transform.position;
            Vector2Int gridIndex = grid.GetGridIndexFromWorld(pos);

            if (grid.IsValidCell(gridIndex))
            {
                highlight.SetActive(true);
            }
            else
            {
                highlight.SetActive(false);
            }
        }
    }
}
