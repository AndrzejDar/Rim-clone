using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatistics : MonoBehaviour
{
    [SerializeField] public string name { get; protected set; }
    [SerializeField] public float rested { get; protected set; }//ogólny poziom wypoczynku na wakacjach
    private int restedM;//0-1 - mnożnik wzrostu
    [SerializeField] public int beauty { get; protected set; }//poziom satysfakcji z otaczającej aury
    private int beautyT;//0-100 - wymagany próg
    [SerializeField] public float hunger { get; protected set; }//poziom najedzenia
    private int hungerD;//0-1 - tempo opadania, 10-20% na godzinę
    [SerializeField] public float recreation { get; protected set; }//poziom zadowolenia z aktywności
    private Dictionary<string, int> recreationM; //mnożniki dla różnych aktywności
    private int recreationD; //0-1 tempo opdania

    public int tier { get; protected set; } = 0;//pozim wymagań z variacji tworzenia 4 pozycja 1, 2 lub 3
    public float sleepRequired; //random 6-12
    public bool sleepy = false;
    public bool needToliet;


    public bool checkedIn = false;
    [SerializeField] public InstalledObject accommodation;

    private bool active = false;
    private TouristAI.Activity activity;
    private int activityQuality;

    int tmp;

    public void Start()
    {
        name = "Mściwój";
        rested = 0;
        beauty = 0;
        hunger = 100;
        recreation = 100;

        accommodation = null;

        recreationM = new Dictionary<string, int>();
        recreationM.Add("aktwywnosc", 90);
        tmp = (int)(Mathf.Floor(Clock.gameTime % 1f * 24)) + 2;

    }


    public void manualUpdate(float deltaTime)
    {
        //Debug.Log("mu:" + deltaTime);
        Decay(deltaTime);
        if (hunger > 33 && beauty > beautyT && recreation > 33)
        {
            IncreaseRested(deltaTime);
        }

        /*   if (active == true)
           {*/
        switch (activity)
        {
            case TouristAI.Activity.Sunbathing:
                recreation += deltaTime * 4;
                //Debug.Log(recreation);
                if (recreation > 100) recreation = 100;
                break;
        }

        /*  }*/
    }

    public void OnActivityChanged(TouristAI.Activity activity)
    {

        //Debug.Log("Ustawiam activity w statystykach");
        this.activity = activity;
        //Debug.Log("Ustawiłem activity w statystykach");
    }


    private void Decay(float deltaTime)
    {
        if (hunger > 0)
        {
            hunger -= deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 10 * 24 / 8 * hungerD; //do 100 do 0 w 8h *mnożnik
        }

        if (recreation > 0)
        {
            recreation -= deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 10 * 24 / 12 * recreationD; //do 100 do 0 w 12h *mnożnik
        }
        if (tmp == (Mathf.Floor(Clock.gameTime % 1f * 24)))
        { tmp += 1;
            if (tmp == 24) tmp = 0;
            if (UnityEngine.Random.Range(1, 12) == 1&&!sleepy) needToliet = true;
        }



    }



    public void setTouristMultipliers(int[] variation)
    {
        tier = variation[3];
        if (tier == 1 || tier == 2 || tier == 3)
        {
            restedM = 1;
            beautyT = 50;
            hungerD = UnityEngine.Random.Range(8, 12);
            //Debug.Log("hunger" + hungerD);
            recreationD = UnityEngine.Random.Range(7, 13);
            sleepRequired = UnityEngine.Random.Range(8, 11);

        }
        else Debug.LogWarning("BRAK TIERU TURYSTY");

    }

    public void startedActivity(TouristAI.State activity, int quality)
    {
        active = true;
        activity = activity;
        activityQuality = quality;
    }
    public void stopedActivity()
    {
        active = false;
    }

    private void IncreaseRested(float deltaTime)
    {
        rested += deltaTime / Clock.REAL_SECOUNDS_PER_INGAME_DAY * 10 * 24 / 48 * restedM;
    }

    public void Sleepy()
    {
        if (Clock.hour >= 22 || Clock.hour < 3)
        {            
                sleepy = true;
            
        }
    }

    public void Feed(int amount)
    {
        hunger += amount;
        if (hunger != 100) hunger = 100;//TMP zwiększa do maksa
    }


}