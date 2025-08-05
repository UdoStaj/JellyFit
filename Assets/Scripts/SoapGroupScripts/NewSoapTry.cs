using UnityEngine;

public class NewSoapTry : MonoBehaviour
{
    NewSoapTry Left, Right, forward, back;
    bool LeftConnect, RightConnect, ForwardConnect, BackConnect;
    public GridSystem MyGridSystem;
    private bool AmIDragged;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Drag();
    }

    public void Drag()
    {
        if (AmIDragged)
        {
            if(LeftConnect)
            {
                Left.AmIDragged = true;
            }
            if(RightConnect)
            {
                Right.AmIDragged = true;
            }
            if (ForwardConnect)
            {
                forward.AmIDragged = true;
            }
            if (BackConnect)
            {
                back.AmIDragged = true;
            }
        }
    }

    public void Cut()
    {

    }

    
}
