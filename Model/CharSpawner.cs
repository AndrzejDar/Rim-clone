using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharSpawner : MonoBehaviour
{
    public static List<InstalledObject> SpawnPoints = new List<InstalledObject>();
    int level=1;
    int count=10;
    bool spawned = false;
    SpawnVariant spawnVariant; 

    void Start()
    {
    }

    void Update()
    {
        if (SpawnPoints.Count > 0 && spawned ==false) {
            if ((Clock.gameTime) % 1f >= 0.5f  )//
            {
                Debug.Log("spawning tourists by spawner");
                spawnVariant = UI_arrival_panel.selectedVariant;
                Action spawn = new Action(() => { SpawnTourist(SpawnPoints[0].tile); });
                for (int i = 0; i < spawnVariant.count; i++)
                {
                    int rnd = UnityEngine.Random.Range(0, 19);
                    float delay = (i + (rnd / 10f)) / 500f;
                    TropicsUtils.FunctionTimer.Create(spawn, delay);
                }

                spawned = true;
            } 
        }



        if((Clock.gameTime) % 1f >= 0.1f && (Clock.gameTime) % 1f < 0.2f)
        {
            spawned = false;
        }
    }


    void SpawnTourist(Tile t)
    {
        int[] variation = { 0, 0, 0, 0 };
        variation[0] = 0; //0 - tourist, 1- worker
        variation[1] = UnityEngine.Random.Range(0, 0); //head / base choice 
        variation[2] = UnityEngine.Random.Range(0, 4); // body choice
        variation[3] = spawnVariant.templateNo; //tourist tier/dificulty
        string name = "Tourist0" + variation[1].ToString();
        CharController.Instance.PlaceCharacter(name, t, variation);
    }

}

