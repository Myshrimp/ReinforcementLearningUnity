using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [SerializeField] GameObject[] goal;
    public void OnBegin()
    {
        foreach (GameObject go in goal)
        {
            go.SetActive(true);
        }
    }
}
