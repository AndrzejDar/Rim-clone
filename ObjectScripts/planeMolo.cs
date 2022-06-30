using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class planeMolo : MonoBehaviour
{
    InstalledObject IO;
    GameObject GO;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //animator.speed = Clock.Speed;
    }

    private void onGameSpeedChanged()
    {
        animator.speed = Clock.Speed;
    }

    public void Setup(InstalledObject IO, GameObject GO)
    {
        this.IO = IO;
        this.GO = GO;


        Tile t = WorldController.Instance.World.GetTileAt(IO.tile.X, IO.tile.Y + 1);
        if (IO.rotation == 2)
        {
            t = WorldController.Instance.World.GetTileAt(IO.tile.X - 1, IO.tile.Y);
        }

        CharSpawner.SpawnPoints.Add(IO);

        GameObject plane = new GameObject();
        plane.name = "Aeroplane01_ (1)";
        plane.transform.SetParent(GO.transform, true);

        plane.transform.position = WorldController.Instance.CalculateIsoPosition(t);
        SpriteRenderer SR = plane.AddComponent<SpriteRenderer>();
        SR.sprite = WorldController.objectsSprites["Aeroplane01_"];
        SR.sortingOrder = GO.GetComponent<SpriteRenderer>().sortingOrder + 1;
        SR.sortingLayerName = "worldSL";

        animator = GO.AddComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load("animations/items/plane/PlaneAnimator") as RuntimeAnimatorController;

        animator.speed = Clock.Speed;
        Action takeoff = new Action(() => { animator.SetTrigger("takeOff"); });
        Action land = new Action(() => { animator.SetTrigger("land"); });

        /*

        float time = (int)Math.Ceiling(Clock.gameTime) + 0.5f - Clock.gameTime;
        TropicsUtils.FunctionTimer.Create(land, time);
        TropicsUtils.FunctionTimer.Create(takeoff, time + 0.2f);*/

        Clock.RegisterOnSpeedChanged(onGameSpeedChanged);
        Clock.RegisterOnNewDay(SpawnPlaneEveryDay);
    }

    private void OnDestroy()
    {
        Clock.UnregisterOnNewDay(SpawnPlaneEveryDay);

    }


    void SpawnPlaneEveryDay()
    {
        Action takeoff = new Action(() => { animator.SetTrigger("takeOff"); });
        Action land = new Action(() => { animator.SetTrigger("land"); });
        TropicsUtils.FunctionTimer.Create(land, 0.45f);
        TropicsUtils.FunctionTimer.Create(takeoff, 0.6f);
    }



}
