using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ChanceToSpawn : MonoBehaviour
{
    [Range(0f, 100f)]
    public float Chance = 50;
    [Tooltip("Requirements that must be met to spawn this. Look in Prefabs/LevelRequirements for all of them")]
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
        if (Random.value * 100 > Chance)
        {
            Destroy(gameObject);
            return;
        }
        foreach (var req in requirements)
        {
            if (req == null) continue;
            if (req is not IRequirement)
            {
                Debug.LogWarning($"{req.name} is not a requirement in {gameObject.GetFullPath()}");
                return;
            }
            if (req is IRequirement r )
            {
                r.OnSpawned(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Handles.Label(transform.position + new Vector3(0, 0, -3), Chance + "%");

        Transform prev = null;
        foreach (Transform child in transform)
        {
            if (child == transform)
            {
                continue;
            }
            if (prev != null)
            {
                Gizmos.DrawLine(prev.position, child.position);
            }
            prev = child;
        }
    }
}
