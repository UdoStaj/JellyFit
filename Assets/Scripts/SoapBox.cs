using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SoapBox : MonoBehaviour
{
    public List<Vector2Int> box;
    public List<bool> controls;
    //public bool IsFinish;
    public GameObject MyGrid;
    public int i;

    void Start()    
    {
        if(box == null)
        {
            box=new List<Vector2Int>();
        }
        if(controls == null)
        {
            controls=new List<bool>();
        }
    }

    void Update()
    {
        control();
        if(Input.GetKeyDown(KeyCode.V))
        {
            IsFinish();
        }
    }

    public void control()
    {
        foreach(var item in box)
        {
            
            if (MyGrid.GetComponent<GridSystem>().IsCellOccupied(item))
            {
                controls[box.IndexOf(item)] = true;
            }
        }
        i = controls.Count;
        foreach(var item in controls)
        {
            if (item)
            {
                i--;
            }
            if(i <= 0)
            {
                IsFinish();
            }
        }
    }
    public void IsFinish()
    {
        Debug.Log("hi");
        GameHudManager.Instance.OpenWinGamePanel();
    }

    public void Add(Vector2Int item)
    {
        box.Add(item);
        controls.Add(false);
    }
}