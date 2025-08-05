using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RadioScript : MonoBehaviour
{
    AudioSource Source;
    [SerializeField] private List<AudioClip> clips;
    public int _i;
    int i 
    {  
        get { return _i; }
        set 
        {
            if (i >= clips.Count)
            {
                value = 0;
            }
            else
            {
                _i = value;
            }
        }
    }
    private void Start()
    {
        Source = GetComponent<AudioSource>();
        Source.loop = true;
        Source.clip = clips[0];
        Source.Play();
        i = 0;
    }
    private void OnMouseDown()
    {
        NextOne();
    }
    public void NextOne()
    {
        Source.clip = clips[i+1];
        i++;
        Source.Play();
    }
}
