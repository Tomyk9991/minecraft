﻿using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute
{
    public string varName;

    public ArrayElementTitleAttribute(string elementTitleName)
    {
        this.varName = elementTitleName;
    }
}
