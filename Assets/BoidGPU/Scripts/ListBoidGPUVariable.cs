using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListBoidGPUVariable", menuName = "Scriptable Objects/ListBoidGPUVariable")]
public class ListBoidGPUVariable : ScriptableObject
{
    public List<BoidGPUTransform> boidTransform = new List<BoidGPUTransform>();
    public List<BoidGPUMaterial> boidMaterial = new List<BoidGPUMaterial>();
}
