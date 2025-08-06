using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public int highlightCount,particleCount;
    public GameObject highlightPrefab;
    public GameObject particlePrefab;
    public List<GameObject> highlightList = new List<GameObject>();
    public List<GameObject> particleList = new List<GameObject>();


    Transform highlightParent;
    Transform particleContainer;

    public static ObjectPooling Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CreateParent(ref highlightParent, "HighlightContainer");
        CreateParent(ref particleContainer, "ParticleContainer");

    }
    void CreateParent(ref Transform parent, string parentName)
    {
        if (parent == null)
        {
            GameObject container = new GameObject(parentName);
            parent = container.transform;
        }
    }

    private void Start()
    {
        start(highlightCount, highlightPrefab, highlightList, highlightParent);
        start(particleCount, particlePrefab, particleList, particleContainer);
    }
    void start(int count, GameObject prefab, List<GameObject> list, Transform container)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject poolObj = Instantiate(prefab, container);
            poolObj.SetActive(false);
            list.Add(poolObj);
        }
    }
    public GameObject GetParticle()
    {
        GameObject particle = get(particleList, particlePrefab);
        return particle;
    }
    public void DisableParticle(GameObject particle)
    {
        particle.SetActive(false);
    }
    public GameObject GetHighlight()
    {
        GameObject highlight = get(highlightList, highlightPrefab);
        return highlight;
    }
    public void DisableHighlight(GameObject highlight)
    {
        highlight.SetActive(false);
    }

    GameObject get(List<GameObject> list, GameObject prefab)
    {
        foreach (GameObject poolObj in list)
        {
            if (poolObj != null)
            {
                if (!poolObj.activeInHierarchy)
                {
                    poolObj.SetActive(true);
                    return poolObj;
                }
            }
        }
        //Tüm objeler kullanýlýyorsa yeni obje oluþtur
        GameObject newPoolObj=newPoolObj = Instantiate(prefab);
        newPoolObj.SetActive(true);
        list.Add(newPoolObj);
        return newPoolObj;
    }
}
