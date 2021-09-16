using System.Collections.Generic;
using Attributes;
using Core.Builder;
using UnityEngine;

[CreateAssetMenu(fileName = "UVDataScriptable", menuName = "Scriptable Objects/UVData")]
public class UVDataScriptable : ScriptableObject
{
    [ArrayElementTitle("EnumType")]
    public List<BlockInformation> blockInformation;
}
