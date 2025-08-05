using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGridSystem : GridSystem
{
    public static new BoxGridSystem instance;

    public override void Awake()
    {
        base.Awake();
        instance = this;
    }
}

