using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NewCutSystem : MonoBehaviour
{
    public struct CellLine
    {
        public Vector2Int cellA;
        public Vector2Int cellB;
        public bool canCut;
    }
    public float bubbleYPosition = 0.15f;
    public int maxCutCount; // Maksimum kesme sayısı
    public int currentCutCount; // Geçerli kesme sayısı

    public Animator VisualKnifeAnimator;
    private List<SliceManager> sms;
    private Vector3 gridOrigin;
    private float cellSize;
    private GridSystem gridSystem;



    public System.Action OnCutCompleted;
    
    private void Start()
    {
        currentCutCount = maxCutCount;
        gridSystem = FindObjectOfType<GridSystem>();
        sms = FindObjectsByType<SliceManager>(FindObjectsSortMode.None).ToList();
        if (sms == null)
        {
            Debug.LogError("NewCutSystem: SliceManager bulunamadı!");
            return;
        }
        if (gridSystem == null)
        {
            Debug.LogError("NewCutSystem: GridSystem bulunamadı!");
            return;
        }
        gridOrigin = gridSystem.gridOrigin;
        cellSize = gridSystem.cellSize;
    }
    private void Update()
    {
        Mathf.Clamp(currentCutCount, 0, maxCutCount);
    }
    public void OnClickCut()
    {
        Debug.Log("tamam");
        foreach (SliceManager sm in sms)
        {
            if (sm.GetComponentInChildren<Renderer>().isVisible || currentCutCount>0)
            {
                ScanCutArea(sm);
                currentCutCount--;
            }
            else
            {
                GameHudManager.Instance.OpenLoseGamePanel();
            }
        }
    }
    private void ScanCutArea(SliceManager sm)
    {
        if (sm == null || sm.knife == null)
        {
            Debug.LogError("SliceManager veya knife null!");
            return;
        }

        Vector3 knifePosition = sm.knife.transform.position;

        Collider[] allColliders = Physics.OverlapBox(
            knifePosition,
            new Vector3(0.8f, 2f, 0.8f),
            Quaternion.identity
        );

        List<GameObject> detectedObjects = new List<GameObject>();

        foreach (var collider in allColliders)
        {
            if (collider.CompareTag("SoapCube") || 
                collider.name.Contains("boxCube") || 
                collider.name.Contains("BoxCube"))
            {
                detectedObjects.Add(collider.gameObject);
            }
        }

        if (detectedObjects.Count >= 2)
        {
            detectedObjects.Sort((a, b) => 
                Vector3.Distance(knifePosition, a.transform.position)
                .CompareTo(Vector3.Distance(knifePosition, b.transform.position))
            );

            GameObject objA = detectedObjects[0];
            GameObject objB = detectedObjects[1];

            CubeGroupControl cubeControlA = objA.GetComponent<CubeGroupControl>();
            if (cubeControlA == null)
            {
                cubeControlA = objA.AddComponent<CubeGroupControl>();
            }

            CubeGroupControl cubeControlB = objB.GetComponent<CubeGroupControl>();
            if (cubeControlB == null)
            {
                cubeControlB = objB.AddComponent<CubeGroupControl>();
            }

            Vector3 direction = objB.transform.position - objA.transform.position;
            Vector3Int directionAB = new Vector3Int(
                Mathf.RoundToInt(direction.x),
                0,
                Mathf.RoundToInt(direction.z)
            );

            cubeControlA.CutBetween(directionAB);

            currentCutCount -= 1;

            StartCoroutine(ShowBubble(knifePosition,sm.GetComponentInChildren<Animator>()));

            OnCutCompleted?.Invoke();
        }
    }
    IEnumerator ShowBubble(Vector3 position,Animator VisualKnifeAnimator)
    {
        VisualKnifeAnimator.SetBool("isSlicing", true);
        yield return new WaitForSeconds(0.5f);
        GameObject bubble = ObjectPooling.Instance.GetParticle();
        bubble.transform.position = new Vector3(
                       position.x,
                       bubbleYPosition, 
                       position.z);

        yield return new WaitForSeconds(4f);
        ObjectPooling.Instance.DisableParticle(bubble);
    }

    /*private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (sm != null && sm.knife != null)
        {
            Vector3 origin = sm.knife.transform.position;
            Vector3 halfExtents = new Vector3(0.8f, 2f, 0.8f);

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
        }
#endif
    }*/
    public void test()
    {
        Debug.Log("tm");
    }
}
