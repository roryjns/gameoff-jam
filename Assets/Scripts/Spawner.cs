using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Range(0f, 100f)]
    public float SpawnChanceOfAll = 100;
    [Tooltip("Requirements that must be met to spawn this. Look in Prefabs/LevelRequirements for all of them")]
    public ScriptableObject[] requirements;
    [Header("Per item configuration")]
    //[Range(0f, 100f)]
    //public float SpawnChancePerItem = 100;
    public bool RandomSelectItems = true;
    public int MaxSpawnCount = 99;
    //public int MinSpawnCount = 99;
    private void Start()
    {
        foreach (var req in requirements)
        {
            if (req == null) continue;
            if (req is not IRequirement)
            {
                Debug.LogWarning($"{req.name} is not a requirement in {gameObject.GetFullPath()}");
                continue;
            }
            if (req is IRequirement r && !r.IsMet(gameObject))
            {
                Destroy(gameObject);
                return;
            }
        }
        if (SpawnChanceOfAll != 100 && UnityEngine.Random.value * 100 > SpawnChanceOfAll)
        {
            Destroy(gameObject);
            return;
        }
        List<int> indices = Enumerable.Range(0, transform.childCount).ToList();
        List<int> toKeep = new List<int>(MaxSpawnCount);
        for (int i = 0; i < MaxSpawnCount; i++)
        {
            if (indices.Count == 0)
            {
                break;
            }
            int randomIndex = !RandomSelectItems ? i : (int)System.Math.Round(UnityEngine.Random.value * (indices.Count - 1));
            int newIndex = indices[randomIndex];
            indices.RemoveAt(randomIndex);
            toKeep.Add(newIndex);
        }
        int index = 0;
        foreach (Transform child in transform)
        {
            if (!toKeep.Contains(index))
            {
                Destroy(child.gameObject);
            }
            index++;
        }
        foreach (var req in requirements)
        {
            if (req == null || req is not IRequirement) continue;
            if (req is IRequirement r )
            {
                r.OnSpawned(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.white;
        Handles.Label(transform.position + new Vector3(0, 0, -3), SpawnChanceOfAll + "%");

        Transform prev = null;
        foreach (Transform child in transform)
        {
            if (prev != null)
            {
                Gizmos.DrawLine(prev.position, child.position);
            }
            prev = child;
        }
#endif
    }

    public enum HowManyToSpawn
    {
        All,
        OneOf,
        TwoOf,
        AllExceptOne,
    }
}

