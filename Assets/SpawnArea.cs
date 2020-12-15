using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{

    public GameObject enemyToSpawn;
    public int enemiesToSpawn;
    public EnemyManager enemyManager;

    // Start is called before the first frame update
    void Start()
    {/*
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 randomCoordinate = GenerateRandomPoint();
            Instantiate(enemyToSpawn, randomCoordinate, Quaternion.identity);
        }*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Vector3 GenerateRandomPoint()
    {
        Vector3 origin = transform.position;
        Vector3 range = transform.localScale / 2.0f;
        Vector3 randomRange = new Vector3(Random.Range(-range.x, range.x),
            Random.Range(-range.y, range.y),
            Random.Range(-range.z, range.z));
        Vector3 randomCoordinate = origin + randomRange;
        return randomCoordinate;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position,transform.localScale);
    }

    public void RespawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 randomCoordinate = GenerateRandomPoint();
            Instantiate(enemyToSpawn, randomCoordinate, Quaternion.identity);
        }
    }
}