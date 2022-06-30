using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_arrival_panel : MonoBehaviour
{
    bool active;
    bool open;
    [SerializeField]Animator anim;
    [SerializeField] GameObject button1;
    [SerializeField] GameObject button2;
    [SerializeField] GameObject button3;
    [SerializeField] GameObject option1;
    [SerializeField] GameObject option2;
    [SerializeField] GameObject option3;
    [SerializeField] GameObject timer1;
    [SerializeField] GameObject timer2;
    [SerializeField] GameObject container;

    TextMeshProUGUI timer1_text;
    TextMeshProUGUI timer2_text;


    private SpawnVariant variant1;
    private SpawnVariant variant2;
    private SpawnVariant variant3;
    static public SpawnVariant selectedVariant;

    private void Start()
    {
        active = true;//false
        //anim.SetBool("active", active);
        onNewDay();//debug
        open = true;
        Clock.RegisterOnNewDay(onNewDay);
        timer1_text = timer1.GetComponent<TextMeshProUGUI>();
        timer2_text = timer2.GetComponent<TextMeshProUGUI>();

        container.SetActive(false);

        //this.enabled = false;
    }

    private void Update()
    {

        if (Clock.gameTime % 1f < 0.501f && Clock.gameTime % 1f > 0.25f)//between 6-12
        {
            if (active == false)
            {
                container.SetActive(true);
                Debug.Log("opening arrival panel");
                //container.SetActive(true); 
                active = true; 
                open = true;
                anim.SetBool("active", active);
                anim.SetBool("open", open);
                selectedVariant = null;

            }
            timer1_text.text = "Guest arraving in: 0" + (11 - Clock.hour) + "h " + (60 - Clock.minutes) + "m";
            timer2_text.text = "0"+(11 - Clock.hour) + ":" + (60 - Clock.minutes) + "m";
        }
        else if (active==true)
        {
            if (selectedVariant == null) SelectRandomOption();
            Debug.Log("closing arrival panel");
            active = false;
            open = false;
            anim.SetBool("active", active);
            anim.SetBool("open", open);
            button1.GetComponent<Button>().interactable = true;
            button2.GetComponent<Button>().interactable = true;
            button3.GetComponent<Button>().interactable = true;
            

            //container.SetActive(false);
        }
    }

    void SelectRandomOption()
    {
        int rnd = UnityEngine.Random.Range(1, 3);
        if (rnd == 1)
            selectedVariant = variant1;
        if (rnd == 2)
            selectedVariant = variant2; 
        if (rnd == 3)
            selectedVariant = variant3;
    }

    public void Open()
    {
        open = !open;
        anim.SetBool("open",open);
    }

    public void TouristSpawnOption(int option)
    {
        if (option == 1)
        { selectedVariant = variant1;
            button2.GetComponent<Button>().interactable = false;
            button3.GetComponent<Button>().interactable = false;
        }
        if (option == 2)
        {
            selectedVariant = variant2;
            button1.GetComponent<Button>().interactable = false;
            button3.GetComponent<Button>().interactable = false;
        }
        if (option == 3)
        {
            selectedVariant = variant3;
            button1.GetComponent<Button>().interactable = false;
            button2.GetComponent<Button>().interactable = false;
        }

    }

    string generateSpawnMessage(SpawnVariant SV)
    {
        string text="x " + SV.count + " " + SV.template + " tourists will arrive on Island";
        return text;
    }


    void onNewDay()
    {
        //active = true;
        variant1 = new SpawnVariant(1);
        option1.GetComponent<TextMeshProUGUI>().text = generateSpawnMessage(variant1);
        variant2 = new SpawnVariant(2);
        option2.GetComponent<TextMeshProUGUI>().text = generateSpawnMessage(variant2);
        variant3 = new SpawnVariant(3);
        option3.GetComponent<TextMeshProUGUI>().text = generateSpawnMessage(variant3);
    }

}

public class SpawnVariant
{
    int level;//dificulty 1-3; affect multiplier and others
    public int count;//number of spawned tourist
    public int templateNo; //1-asian, 2-american, 3europe

    public enum Template
    {
        Asian,
        American,
        European
    }
    public Template template;

    float spawnRate = 0.1f;
    float maxSpawnRateBounds = 1;//%of deviation

     public SpawnVariant(int level)
    {
        this.level = level;
        float rnd = UnityEngine.Random.Range(1, 1 + maxSpawnRateBounds);
        if (WorldController.Instance!=null && WorldController.Instance.charController!=null && WorldController.Instance.charController.characterGameObjectMap != null)
        {
            count = (int)Mathf.Floor(WorldController.Instance.charController.characterGameObjectMap.Count * spawnRate * rnd);
            if (count < 1) count = 2;
        }
        else count = 2;


        templateNo = UnityEngine.Random.Range(1, 3);
        if (templateNo == 1) template = Template.Asian;
        else if (templateNo == 2) template = Template.American;
        else template = Template.European;
    }

}