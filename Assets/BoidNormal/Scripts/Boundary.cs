using UnityEngine;

[CreateAssetMenu(fileName = "Boundary", menuName = "Scriptable Objects/Boundary")]
public class Boundary : ScriptableObject
{
    private float xLimit;
    private float yLimit;

    public float XLimit { get { CaculationLimit(); return xLimit; } }
    public float YLimit { get { CaculationLimit();  return yLimit; } }

    private void CaculationLimit()
    {
        yLimit = Camera.main.orthographicSize + 1f;
        xLimit = yLimit * Screen.width / Screen.height + 1f;
    }
}
