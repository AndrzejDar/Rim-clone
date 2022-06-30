using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class Clock :MonoBehaviour
{
    public const float REAL_SECOUNDS_PER_INGAME_DAY = 600f; //5 min
    public static float gameTime { get; protected set; }
    string dayS;
    string hourS;
    string minutesS;
    public static int day;
    public static int hour;
    public static int minutes;
    public static int Speed  //0,1x,2x,8x
    { get; protected set; }

    private GameObject timer;
    private int previousDay;

    private static Action cbNewDay;
    private static Action cbSpeedChange;


    public Clock()
    {
        Speed = 1;
        gameTime = 1.5f; //time at new game start
    }    

    // Update is called once per frame
    public void UpdateTime(float deltaTime)
    {
        if (timer == null) { timer = GameObject.Find("Timer"); }


        
        gameTime += deltaTime / REAL_SECOUNDS_PER_INGAME_DAY;
        //Debug.Log("gt"+gameTime);

        day = (int)(Mathf.Floor(gameTime));

        if (day != previousDay && cbNewDay != null) { cbNewDay(); }



        previousDay = day;
        hour = (int)(Mathf.Floor((gameTime % 1f) * 24f));// % - mod, zwraca część dziesiętną
        minutes = (int)((((gameTime % 1f) * 24f) % 1f) * 60f);
        dayS = day.ToString();
        hourS = hour.ToString("00");
        minutesS = minutes.ToString("00");

        if (timer.GetComponent<TextMeshProUGUI>() != null) 
        { 
        timer.GetComponent<TextMeshProUGUI>().text = "Day " + dayS + "    " + hourS + ":" + minutesS;
    }
    else{
            Debug.LogWarning("Brakuje obiektu zegara!");
            Debug.LogWarning(timer.name);
        }

    }

    public static void SetSpeed(int speed)
    {
        Speed = speed;
        if (cbSpeedChange != null) cbSpeedChange();
        //Debug.Log(Speed);
    }

    public void SetSpeedMenu(int speed) //used by Speed menu
    {
        Speed = speed;
        if(cbSpeedChange!=null)cbSpeedChange();
        //Debug.Log(Speed);
    }

    public static void RegisterOnSpeedChanged(Action callbackfunc)
    {
        cbSpeedChange += callbackfunc;
    }
    public static void UnRegisterOnSpeedChanged(Action callbackfunc)
    {
        cbSpeedChange += callbackfunc;
    }

    public static void RegisterOnNewDay(Action callbackfunc)
    {
        cbNewDay += callbackfunc;
    }
    public static void UnregisterOnNewDay(Action callbackfunc)
    {
        cbNewDay -= callbackfunc;
    }

}
