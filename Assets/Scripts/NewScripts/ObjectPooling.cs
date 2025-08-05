using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public int bubbleParticleCount;
    public GameObject bubbleParticlePrefab;
    public List<GameObject> bubbleList = new List<GameObject>();


    Transform bubbleParent;

    public static ObjectPooling Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CreateParent(ref bubbleParent, "ParticlesContainer");


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
        startBubble(bubbleParticleCount, bubbleParticlePrefab, bubbleList, bubbleParent,Quaternion.Euler(-90,0,0));
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
    void startBubble(int count, GameObject prefab, List<GameObject> list, Transform container,Quaternion eulers)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject poolObj = Instantiate(prefab, transform.position, eulers, container);
            poolObj.SetActive(false);
            list.Add(poolObj);
        }
    }
    public GameObject GetParticle()
    {
        GameObject particle = get(bubbleList, bubbleParticlePrefab);
        return particle;
    }
    public void DisableParticle(GameObject particle)
    {
        particle.SetActive(false);
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
        GameObject newPoolObj;
        if (prefab==bubbleParticlePrefab)
            newPoolObj = Instantiate(prefab, transform.position, Quaternion.Euler(90,0,0), bubbleParent);
        else
            newPoolObj = Instantiate(prefab);
        newPoolObj.SetActive(true);
        list.Add(newPoolObj);
        return newPoolObj;
    }
}
