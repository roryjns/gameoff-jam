using UnityEngine;

public class SpawnerOneOf : MonoBehaviour
{
    [Tooltip("Requirements that must be met to spawn this")]
    public ScriptableObject[] requirements;
    private void Start()
    {
        foreach (var req in requirements)
        {
            if (req == null) continue;
            if (req is not IRequirement)
            {
                Debug.LogWarning($"{req.name} is not a requirement in {gameObject.GetFullPath()}");
                return;
            }
            if (req is IRequirement r && !r.IsMet(gameObject))
            {
                Destroy(gameObject);
                return;
            }
        }
        int index = (int)System.Math.Round(Random.value * (transform.childCount - 1));
        Transform theOneThatStays = null;
        if (index >= 0)
        {
            theOneThatStays = transform.GetChild(index);
        }
        foreach (Transform child in transform)
        {
            if (child != theOneThatStays)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void Update()
    {
        
    }
}