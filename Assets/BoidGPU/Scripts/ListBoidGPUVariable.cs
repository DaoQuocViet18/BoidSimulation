using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListBoidGPUVariable", menuName = "Scriptable Objects/ListBoidGPUVariable")]
public class ListBoidGPUVariable : ScriptableObject
{
    public List<BoidGPUVariable> boidVariable = new List<BoidGPUVariable>();
}
