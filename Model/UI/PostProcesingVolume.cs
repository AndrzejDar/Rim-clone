using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcesingVolume : MonoBehaviour
{
    PostProcessVolume Sunset;
    PostProcessVolume NightTime;
    PostProcessVolume Sunrise;
    PostProcessVolume MidDay;



    // Start is called before the first frame update
    void Start()
    {
        PostProcessVolume[] ppArray= this.GetComponents<PostProcessVolume>();
        foreach(PostProcessVolume ppV in ppArray)
        {

            if (ppV.priority == 1) Sunset = ppV;
            if (ppV.priority == 2) NightTime = ppV;
            if (ppV.priority == 3) Sunrise = ppV;

        }
        Sunset.weight = 0;
        NightTime.weight = 0;
        Sunrise.weight = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Clock.hour >= 18 && Clock.hour <= 20)
        {
            Sunset.weight += Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 /3* Clock.Speed;
        }

        if (Clock.hour >= 21 && Clock.hour <= 23)
        {
            
            NightTime.weight += Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 /6* Clock.Speed;
        }
        if (Clock.hour >= 00 && Clock.hour <= 02)
        {
            //Sunset.weight -= Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 / 3 * Clock.Speed;
            
        }

        if (Clock.hour >= 3 && Clock.hour <= 6)
        {            
           
            Sunrise.weight += Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 /4* Clock.Speed;
        }

        if (Clock.hour > 6 && Clock.hour < 12)
        {
            Sunset.weight -= Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 / 5 * Clock.Speed;
            NightTime.weight -= Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 / 10 * Clock.Speed;
            Sunrise.weight -= Time.deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 24 /5* Clock.Speed;
        }

        if (Clock.hour == 13)
        {
            Sunset.weight = 0;
            NightTime.weight = 0;
            Sunrise.weight = 0;
        }


    }
}
