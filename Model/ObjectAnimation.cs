using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAnimation : MonoBehaviour
{
    InstalledObject IO;
    Animator anim;
    TouristAI occupantAI;
    bool triggered;
    private void Update()
    {
        //on entry triger shake
        if (IO.entry == true)
        {
            if (IO.Occupied == true && triggered == false) { anim.SetTrigger("entered"); triggered = true; }
            if (IO.Occupied == false) triggered = false;
        }

        


        // on occupant sleeping snore
        if (IO.occupant!=null && WorldController.Instance.charController.characterGameObjectMap[IO.occupant].GetComponent<TouristAI>() != null)
        {
            occupantAI = WorldController.Instance.charController.characterGameObjectMap[IO.occupant].GetComponent<TouristAI>();
            if (occupantAI.publicActivity == TouristAI.Activity.Sleeping && anim.GetBool("sleeping")==false)
            {
                anim.SetBool("sleeping", true);
            }else
                anim.SetBool("sleeping", false);

        }
    }




    void setObjectAnimation()
    {

    }

    public void SetUpObjectAnimation(InstalledObject installedObject, Animator anim)
    {
        this.IO = installedObject;
        this.anim = anim;
    } 

    public static void PLayObjectAnimation(InstalledObject IO)
    {
        Animator anim = WorldController.installedObjectGameObjectMap[IO].GetComponent<Animator>();

        
        string SwitchName = IO.objectType.Substring(0, 4);

        switch (SwitchName)
        {
            case "acco": //accomodation
                
                anim.speed = 1 * Clock.Speed;
                anim.Play("snoring", 0);
                break;
        }
    }

}
