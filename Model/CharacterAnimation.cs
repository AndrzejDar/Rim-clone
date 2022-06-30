using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator anim;
    public string[] standDirections = { "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7" };
    public string[] runDirections = { "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7" };
    public string[] victoryDirections = { "v0", "v1", "v2", "v3", "v4", "v5", "v6", "v7" };
    public string[] layingDirections = { "l3","l5" };
    public string[] harvestDirections = { "h5" };
    public string[] waitingDirections = { "w4" };
    Tile previousTile;
    Tile nextTile;
    int direction;
    float waitTime;
   // Animator anim;

    private void Awake()
    {
 
    }

    public void SetAnimation(Character character, Animator anim)
    {
        string[] directionArray = null;
        if (character == null) return;
        bool changeAnimation = false;
        int prevDirection = direction;
        Tile cT = character.currentTile;
        Tile nT = character.nextTile;
        //Debug.Log("prev direction is:" + prevDirection);
        setAnimationSpeed(character, anim);

        if (cT != nT && nT !=null)
        {
            directionArray = runDirections;
            anim.Play(directionArray[character.facingDirection], 0);
            //Debug.Log("zmiana animacji na: " + directionArray[character.facingDirection]);
            return;
        }

        if (nT==null || nT==cT)
        {
            directionArray = standDirections;
            //Debug.Log("zmiana animacji STÓJJ na: " + directionArray[4]);
            //anim.Play(directionArray[character.facingDirection], 0);
            anim.Play(directionArray[character.facingDirection], 0);
            return;
        }
    }

    public float SetAndPlayAnimation_Tourist(Character character, Animator anim,TouristAI.Activity activity)
    {
        string[] directionArray;
        float clipLength;
        this.anim = anim;
        Debug.Log("activity przed wyborem animacji jest:" + activity);
        if (activity == TouristAI.Activity.Sunbathing)
        {
            directionArray = layingDirections;
            anim.speed = 0.5f*Clock.Speed;
            int a = checkIODirection(character.currentTile);
            anim.Play(directionArray[a], 0);
            clipLength = GetLenghtOfClip(directionArray[0], character.spriteVariation[0]);

        }
        else if (activity == TouristAI.Activity.Waiting)
        {
            directionArray = waitingDirections;
            anim.speed = 1f * Clock.Speed;
            anim.Play(directionArray[0], 0);
            clipLength = GetLenghtOfClip(directionArray[0], character.spriteVariation[0]);
        }
        else //domyślna animka Victory
        {
            directionArray = victoryDirections;
            anim.speed = 2f * Clock.Speed;
            anim.Play(directionArray[4], 0);
            clipLength = GetLenghtOfClip(directionArray[4],character.spriteVariation[0]);
        }

        waitTime = clipLength / anim.speed * Clock.Speed;
        //Debug.Log("odtwarzam animacje "+anim.GetCurrentAnimatorClipInfo(0) +" o długości"+ waitTime);
        return waitTime;       
    }


    int checkIODirection(Tile t)
    {
        int index=0;
        if (t.installedObject != null)
        {
            if (t.installedObject.rotation == 1) index = 0;
            if (t.installedObject.rotation == 2) index = 1;
        }
        return index;
    }


    public float SetAnimation_Worker(Character character, Animator anim, TaskSystem.Task task)
    {
        string[] directionArray;
        float clipLength;
        this.anim = anim;
        if (task is TaskSystem.Task.Harvest)
        {
            directionArray = harvestDirections;
            anim.speed = 1f * Clock.Speed;
            anim.Play(directionArray[0], 0);
            clipLength = (GetLenghtOfClip(directionArray[0], character.spriteVariation[0])*3f+0.4f);

        }
        else //domyślna animka Victory
        {
            directionArray = victoryDirections;
            anim.speed = 2f * Clock.Speed;
            //Debug.Log("odtwarzam animacje " + directionArray[4]);
            anim.Play(directionArray[4], 0);
            clipLength = GetLenghtOfClip(directionArray[4], character.spriteVariation[0]);
        }
        if(task==null)Debug.Log("Task is null");else
        //Debug.Log("Task is: "+task);

        waitTime = clipLength / anim.speed * Clock.Speed;
        //Debug.Log("Animacja o długości" + waitTime);
        return waitTime;
    }



    float GetLenghtOfClip(string clipName, int characterType)
    {
        AnimationClip[] animation = anim.runtimeAnimatorController.animationClips;
        string newName = clipName[0] + characterType.ToString() + clipName[1];
        foreach (AnimationClip clip in animation)
        {
            if (clip.name == newName) return clip.length;
        }
        return 1;
    }

    IEnumerator ShowCurrentClipLength()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();
        //print("current clip length = " + anim.GetCurrentAnimatorStateInfo(0).length);
        waitTime = anim.GetCurrentAnimatorClipInfo(0).Length;
        //Debug.Log(anim.GetCurrentAnimatorClipInfo(0).Length);
        //Debug.Log(anim.GetCurrentAnimatorStateInfo(0).length);
    }


    void setAnimationSpeed(Character character,Animator anim)
    {
        anim.speed = character.movementSpeed * 2.3f * Clock.Speed;
        if (character.state == Character.State.Roaming) anim.speed = anim.speed * character.idleSpeedM;
        // idealny realistyczny mnożnik to 5.5
    }

    public int directionIndex(Tile start, Tile end)
    {
        if (start.X < end.X && start.Y > end.Y) {return 0; }

        if (start.X < end.X && start.Y == end.Y) { return 1; }
        
        if (start.X < end.X && start.Y < end.Y) {return 2; }

        if (start.X == end.X && start.Y < end.Y) {return 3; }

        if (start.X > end.X && start.Y < end.Y) { return 4; }

        if (start.X > end.X && start.Y == end.Y) {  return 5; }

        if (start.X > end.X && start.Y > end.Y) { return 6; }

        if (start.X == end.X && start.Y > end.Y) {  return 7; }
        else
        {
            //Debug.LogWarning("kierunek nieokreslony!!!!!!");
            return 4;
        }
    }

}
