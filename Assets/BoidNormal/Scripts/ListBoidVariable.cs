using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListBoidVariable", menuName = "Scriptable Objects/ListBoidVariable")]
public class ListBoidVariable : ScriptableObject
{
    public List<Transform> boidTransform = new List<Transform>();
}
