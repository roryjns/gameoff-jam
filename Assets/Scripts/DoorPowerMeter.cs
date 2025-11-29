using System;
using UnityEngine;

public class DoorPowerMeter : MonoBehaviour
{
    [SerializeField]
    int powerLevel = 0;
    readonly int numberOfLights = 5;
    int doorLevel;

    private void Start()
    {
        powerLevel = 0;
        transform.parent.GetComponent<BoxCollider2D>().enabled = false;
        GameManager.Instance.OnEnemyDeath += GameManager_OnEnemyDeath;

        doorLevel = LevelGenerator.Instance.GetChunkFromPosition(transform.position).Level;
    }

    private void OnValidate()
    {
        for (int i = 0; i < numberOfLights; i++)
        {
            transform.GetChild(i).gameObject.GetComponent<DoorLight>().Toggle(powerLevel > i);
        }
    }

    private void GameManager_OnEnemyDeath(object sender, Enemy e)
    {
        if (LevelGenerator.Instance.GetChunkFromPosition(transform.position).Level != doorLevel)
        {
            //We don't increase the enemyDeathCount from enemies in other levels.
            return;
        }

        powerLevel++;
        TurnOnLight(powerLevel);

        if (powerLevel >= numberOfLights)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        transform.parent.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void TurnOnLight(int lightCount)
    {
        if (lightCount > numberOfLights)
        {
            return;
        }
        transform.GetChild(lightCount - 1).gameObject.GetComponent<DoorLight>().Toggle(true);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDeath -= GameManager_OnEnemyDeath;
        }
    }
}
