﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShooterAgent : Unity.MLAgents.Agent
{

    public EnemyManager enemyManager; 
    public Health healthManager;
   // public AutonomusPlayerController autonomusPlayerController;
    public PlayerWeaponsManager playerWeaponsManager;
   // public AutonomusInputController autonomusInputController;
    public Jetpack jetpack;
    public ObjectiveManager objectiveManager;
    public SpawnArea spawnArea;
    public GameObject playerSpawn;
    public GameObject bossSpawn;
    public GameObject bossObjectToSpawn;
    public float maxEpisodeTime = 40;
    public GameObject healthPickupObject;
    private float currentTime = 0;
    public GameObject[] healthPickUpSpawn;
    public PickupManager pickupManager;
    public double killHeight = -10;
    public GameObject objectiveKillAll;
    public ObjectiveHUDManger objectiveHudManger;
    public WeaponController m_activeWeapon;
    public Camera playerCamera;
    
    private PlayerCharacterController m_playerCharacterController;
    private GameObject currentObjective;
    private  StatsRecorder statsRecorder;
    private List<Health> currentEnemyHealthArray;

    private int episodeCount = 0;
    private int enemeyHitCount = 0;
    private int enemyKillCount = 0;
    private int objectiveCompletedCount = 0;
    //public WeaponController weaponController;
    // Start is called before the first frame update
    void Start()
    {
        m_playerCharacterController = GetComponent<PlayerCharacterController>();
        healthManager.onDie += OnDie;
        statsRecorder = Academy.Instance.StatsRecorder;
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Targets --> May be replaced with seeking in future iterations?
        /*foreach (var enemy in enemyManager.enemies)
        {
            sensor.AddObservation(enemy.transform.localPosition);
            sensor.AddObservation(enemy.GetComponent<Health>().currentHealth);
            sensor.AddObservation(enemy.GetComponent<Health>().maxHealth);
        }*/
        
        //Agent velocity
        //sensor.AddObservation(autonomusPlayerController.characterVelocity);
        var localPosition = gameObject.transform.localPosition;
        sensor.AddObservation(localPosition.x);
        sensor.AddObservation(localPosition.y);
        sensor.AddObservation(localPosition.z);
        sensor.AddObservation(gameObject.transform.rotation.y);
        sensor.AddObservation(m_playerCharacterController.playerCamera.transform.rotation.x);
        sensor.AddObservation(playerWeaponsManager.GetWeaponAtSlotIndex(playerWeaponsManager.activeWeaponIndex).m_CurrentAmmo / playerWeaponsManager.GetWeaponAtSlotIndex(playerWeaponsManager.activeWeaponIndex).maxAmmo); //Added as of 19.1.2021: Normalised Input
        sensor.AddObservation(healthManager.currentHealth / healthManager.maxHealth); //Added as of 19.1.2021: Normalised Input
        sensor.AddObservation(playerWeaponsManager.isPointingAtEnemy); //Added as of 18.01.2021 to help the AI detect if they will hit an enemy
        //Monitor Health
        //sensor.AddObservation(healthManager.currentHealth);
        //sensor.AddObservation(healthManager.maxHealth);
        
        //Weapon monitoring
        //sensor.AddObservation(playerWeaponsManager.isPointingAtEnemy);
        //sensor.AddObservation(weaponController.maxAmmo);
        //sensor.AddObservation(weaponController.m_CurrentAmmo);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float distanceToSpawn = Vector3.Distance(playerSpawn.transform.position, transform.position);
        //Debug.Log(distanceToSpawn);
        if (distanceToSpawn >= 40f)
        {
            //Debug.Log("Furthest Reward.");
            AddReward(0.0008f);
        }
        else if (distanceToSpawn >= 25f)
        {
            //Debug.Log("3nd Nearest Reward.");
            AddReward(0.0006f);
        }
        else if (distanceToSpawn >= 10f)
        {
            //Debug.Log("2nd Nearest Reward.");
            AddReward(0.0004f);
        }
        else if (distanceToSpawn >= 4)
        {
            //Debug.Log("Nearest Reward.");
            AddReward(0.0002f);
        }
        //Debug.Log("Current Reward:" +GetCumulativeReward());
        if (playerWeaponsManager.isPointingAtEnemy)
        {
            AddReward(0.001f); //Added if 19.1.2020: Keeping aim at enemy --> good
        }
        if (currentEnemyHealthArray.Count == 0)
        {
            int i = 0;
            foreach (var enemy in enemyManager.enemies)
            {
                i = i + 1;
                Health tmpHealth = enemy.GetComponent<Health>();
                currentEnemyHealthArray.Add(tmpHealth);
                tmpHealth.onDie += OnEnemyDeath;
                tmpHealth.onDamaged += RewardForHit;
            }
            //Debug.Log("Added " + i + " enemies");
        }
        
        currentTime += Time.deltaTime;
        AddReward(-0.001f);
        //If all objectives are done the episode ends
        if (objectiveManager.AreAllObjectivesCompleted())
        {
            objectiveCompletedCount += 1;
            statsRecorder.Add("Agent/objectiveCompleted", objectiveCompletedCount);
            AddReward(3f);
            //Debug.Log("Current Reward:" +GetCumulativeReward());
            EndEpisode();
        }

        if (currentTime >= maxEpisodeTime)
        {
            EndEpisode();
        }


        if(transform.position.y < killHeight)
        {
            OnDie(gameObject);
        }
        
    }

    private void RewardForHit(float damage, GameObject damageSource)
    {
        //Debug.Log("Hit reward!");
        if (damageSource == gameObject)
        {
            //Debug.Log("Enemy hit by player!");
            enemeyHitCount += 1;
            statsRecorder.Add("Agent/EnemeiesHit", enemeyHitCount);
            AddReward(0.1f);
        }

        //Debug.Log("Current Reward:" +GetCumulativeReward());
    }

    private void OnDie(GameObject damageSource)
    {
        //Debug.Log("Player Died!");
        AddReward(-1f);
        //Debug.Log("Current Reward:" +GetCumulativeReward());
        EndEpisode();
    }

    private void OnEnemyDeath(GameObject damageSource)
    {
        if (damageSource == gameObject)
        {
            //Debug.Log("Player killed an enemy!");
            enemyKillCount += 1;
            statsRecorder.Add("Agent/EnemiesKilled", enemyKillCount);
            SetReward(1f);
        }

        //Debug.Log("Current Reward:" +GetCumulativeReward());
    }


    public override void OnEpisodeBegin()
    {
        episodeCount += 1;
        Debug.Log(episodeCount);
        currentTime = 0f;
        //Reset Player --> Health, Position, Velocity
        healthManager.currentHealth = healthManager.maxHealth;
        healthManager.m_IsDead = false;
        m_playerCharacterController.isDead = false;

        //Debug.Log(" Poistion vor respawn: " + gameObject.transform.localPosition);
        var localPosition = playerSpawn.transform.localPosition;
        m_playerCharacterController.transform.localPosition = localPosition;
        gameObject.transform.localPosition = localPosition;
        //Debug.Log("Poistion vor respawn: " + gameObject.transform.localPosition);
        
        transform.rotation = Quaternion.Euler(0,436,0);
        m_playerCharacterController.m_CameraVerticalAngle = 0f;
        //playerCamera.transform.rotation = Quaternion.Euler(0,0,0);
        
        
        m_playerCharacterController.SetCrouchingState(false, false);
        //m_playerCharacterController.characterVelocity = Vector3.zero;
        //End reset player

        //Debug.Log("New Episode!");
        //Reset enemies
        currentEnemyHealthArray = new List<Health>();
        foreach (var enemy in enemyManager.enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemyManager.ResetEnemyList(); //Reset the Text in the list
        //objectiveManager.ResetObjective(); //Reset the objectives
        spawnArea.RespawnEnemies();
        Instantiate(bossObjectToSpawn, bossSpawn.transform.position, Quaternion.identity);
        objectiveManager.ResetObjective();
        currentObjective = Instantiate(objectiveKillAll); // Spawn new Objective
        
        
        //End reset enemies
        
        //Reset Pickups
        pickupManager.ClearAllPickups();
        foreach (var healthSpawn in healthPickUpSpawn)
        {
            Instantiate(healthPickupObject, healthSpawn.transform.position, Quaternion.identity);
        }
        //End Reset Pickups
        
        //Debug.Log("Switch to weapon!");
        playerWeaponsManager.SwitchToWeaponIndex(0, true);
        //Reset Weapon State
        playerWeaponsManager.GetWeaponAtSlotIndex(playerWeaponsManager.activeWeaponIndex).m_CurrentAmmo = playerWeaponsManager.GetWeaponAtSlotIndex(playerWeaponsManager.activeWeaponIndex).maxAmmo;

        //End reset weapon state
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        //****Continous********
        var continousActions = actionsOut.ContinuousActions;
        continousActions[0] = 0;
        continousActions[1] = 0;
        //continousActions[2] = 0; Commented out as of 18.01.2020 to reduce inputs
        //continousActions[3] = 0; Commented out as of 18.01.2020 to reduce inputs
        continousActions[2] = 0;
        //continousActions[5] = 0; Commented out as of 18.01.2020 to reduce inputs
        //continousActions[6] = 0; Commented out as of 18.01.2020 to reduce inputs
        continousActions[3] = 0;
        continousActions[4] = 0;
        
        //Move Horizontal
        if (Input.GetKey(KeyCode.D))
        {
            continousActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            continousActions[0] = -1;
        }
        else
        {
            continousActions[0] = 0;
        }
        
        //Move forward and backwards (Vertically)
        if (Input.GetKey(KeyCode.W))
        {
            continousActions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            continousActions[1] = -1;
            
        }
        else
        {
            continousActions[1] = 0;
        }
        
        /* Commented out as of 18.01.2020 to reduce inputs //Jump
        if (Input.GetKey(KeyCode.Space))
        {
            continousActions[2] = 1;
        }
        else
        {
            continousActions[2] = -1;
        }*/

        /* Commented out as of 18.01.2020 to reduce inputs//Crouch
        if (Input.GetKey(KeyCode.C))
        {
            continousActions[3] = 1;
        }else
        {
            continousActions[3] = -1;
        }*/

        //Fire
        if (Input.GetKey(KeyCode.Mouse0))
        {
            continousActions[2] = 1;
        }else
        {
            continousActions[2] = -1;
        }
        
        /* Commented out as of 18.01.2020 to reduce inputs //Aim
        if (Input.GetKey(KeyCode.Mouse1))
        {
            continousActions[5] = 1;
        }else
        {
            continousActions[5] = -1;
        }*/
        
        /* Commented out as of 18.01.2020 to reduce inputs //Sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            continousActions[6] = 1;
        }else
        {
            continousActions[6] = -1;
        }*/
        continousActions[3] = Mathf.Clamp(Input.GetAxisRaw("Mouse X"), -1f, 1f);
        continousActions[4] = Mathf.Clamp(Input.GetAxisRaw("Mouse Y"), -1f, 1f);
        //continousActions[3] = Input.GetAxisRaw("Mouse X");
        //continousActions[4] = Input.GetAxisRaw("Mouse Y");

        //****Continous End******

        /*
        //****DISCRETE******
        //var discreteActions = actionsOut.DiscreteActions;
        //discreteActions[0] = 0;
        //discreteActions[1] = 0;
        /*discreteActions[2] = 0;
        discreteActions[3] = 0;
        discreteActions[4] = 0;
        discreteActions[5] = 0;
        discreteActions[6] = 0;
        discreteActions[7] = 0;
        discreteActions[8] = 0;
        
        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
        }
        else
        {
            discreteActions[0] = 0;
        }
        
        
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[1] = 2;
            
        }
        else
        {
            discreteActions[1] = 0;
        }
        
        
        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[2] = 1;
        }
        else
        {
            discreteActions[2] = 0;
        }

        if (Input.GetKey(KeyCode.C))
        {
            discreteActions[3] = 1;
        }else
        {
            discreteActions[3] = 0;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            discreteActions[4] = 1;
        }else
        {
            discreteActions[4] = 0;
        }
        
        if (Input.GetKey(KeyCode.Mouse1))
        {
            discreteActions[5] = 1;
        }else
        {
            discreteActions[5] = 0;
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            discreteActions[6] = 1;
        }else
        {
            discreteActions[6] = 0;
        }
        
        
        Debug.Log(Input.GetAxisRaw("Mouse X")); 
        if (Input.GetAxisRaw("Mouse X") == 0f)
        {
            discreteActions[7] = 0;
        }
        else if (Input.GetAxisRaw("Mouse X") > 0.8f)
        {
            discreteActions[7] = 1;
        }
        else if (Input.GetAxisRaw("Mouse X") > 0.6f)
        {
            discreteActions[7] = 2;
        }
        else if (Input.GetAxisRaw("Mouse X") > 0.4f)
        {
            discreteActions[7] = 3;
        }
        else if (Input.GetAxisRaw("Mouse X") > 0.2f)
        {
            discreteActions[7] = 4;
        }
        else if (Input.GetAxisRaw("Mouse X") > 0f)
        {
            discreteActions[7] = 5;
        }
        else if (Input.GetAxisRaw("Mouse X") > -0.2f)
        {
            discreteActions[7] = 6;
        }
        else if (Input.GetAxisRaw("Mouse X") > -0.4f)
        {
            discreteActions[7] = 7;
        }
        else if (Input.GetAxisRaw("Mouse X") > -0.6f)
        {
            discreteActions[7] = 8;
        }
        else if (Input.GetAxisRaw("Mouse X") > -0.8f)
        {
            discreteActions[7] = 9;
        }
        else if (Input.GetAxisRaw("Mouse X") <= -0.8f)
        {
            discreteActions[7] = 10;
        }

        Debug.Log(Input.GetAxisRaw("Mouse Y"));
        if (Input.GetAxisRaw("Mouse Y") == 0f)
        {
            discreteActions[8] = 0;
        }
        else if (Input.GetAxisRaw("Mouse Y") > 0.8f)
        {
            discreteActions[8] = 1;
        }
        else if (Input.GetAxisRaw("Mouse Y") > 0.6f)
        {
            discreteActions[8] = 2;
        }
        else if (Input.GetAxisRaw("Mouse Y") > 0.4f)
        {
            discreteActions[8] = 3;
        }
        else if (Input.GetAxisRaw("Mouse Y") > 0.2f)
        {
            discreteActions[8] = 4;
        }
        else if (Input.GetAxisRaw("Mouse Y") > 0f)
        {
            discreteActions[8] = 5;
        }
        else if (Input.GetAxisRaw("Mouse Y") > -0.2f)
        {
            discreteActions[8] = 6;
        }
        else if (Input.GetAxisRaw("Mouse Y") > -0.4f)
        {
            discreteActions[8] = 7;
        }
        else if (Input.GetAxisRaw("Mouse Y") > -0.6f)
        {
            discreteActions[8] = 8;
        }
        else if (Input.GetAxisRaw("Mouse Y") > -0.8f)
        {
            discreteActions[8] = 9;
        }
        else if (Input.GetAxisRaw("Mouse Y") <= -0.8f)
        {
            discreteActions[8] = 10;
        }
        //****DISCRETE END*****
        */

    }



         void OnTriggerStay(Collider other)
    {
        //if player is on a gameObject with the tag "DeathZone", substract 1 health per frame 
        if (other.gameObject.CompareTag("RewardZone"))
        {
            SetReward(0.2f);
        }
    }
}
