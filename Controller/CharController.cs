using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TropicsUtils;

public class CharController : MonoBehaviour
{
    public static CharController Instance { get; protected set; }
    public World World { get; protected set; }

    WorldController worldController /*= new WorldController()*/;//---------------------------------------
    TaskSystem taskSystem;
    CharacterAnimation characterAnimation;
    Dictionary<Vector2, GameObject> characterPositionMap;
    Dictionary<string, Character> characterPrototypes;
    public Dictionary<Character, GameObject> characterGameObjectMap;

    private Dictionary<string, Sprite> characterSprites;

    [SerializeField]float delay;

    List<Character> allTouristList = new List<Character>();

    Action<Character> cbTouristCreated;

    TouristAI touristAI;

    [SerializeField] private float waitingTimer;
    private Character tourist;

    public int charCount = 0;


    public void CreateCharacterPrototypes()
    {
        characterPrototypes = new Dictionary<string, Character>();

        characterPrototypes.Add("Tourist00", Character.CreateCharacterPrototype(
            "Tourist00", // cahracter type name 

            0.5f, //speed
            0 // potwierdzenie że jest turystą
            )
            );

        characterPrototypes.Add("Worker10", Character.CreateCharacterPrototype
            (
            "Worker10", // kobieta -character type name, 1 - worker,0-4 wariacja kobiety  
            1.1f, //speed
            1 // potwierdzenie że jest workerem
            )
        );
        characterPrototypes.Add("Worker11", Character.CreateCharacterPrototype
    (
    "Worker11", // kobieta -character type name, 1 - worker,0-4 wariacja kobiety  
    1.1f, //speed
    1 // potwierdzenie że jest workerem
    )
);
        characterPrototypes.Add("Worker15", Character.CreateCharacterPrototype
            (
            "Worker15", // facet -character type name, 1 - worker,5-9wariacja faceta 
            1.0f, //speed
            1 // potwierdzenie że jest workerem
            )
        );

    }


    // Start is called before the first frame update
    void Start()
    {

        if (Instance != null)
        { Debug.LogError("There shoudl be no double char controller"); }
        Instance = this;//------------------------------------------------------------------------
        CreateCharacterPrototypes();
        RegisterTouristCreated(OnCharacterCreated);
        characterGameObjectMap = new Dictionary<Character, GameObject>();
        
    }

    void Update()
    {
        //Pathfinding.Update(); //inicjalizacja pathfindingu 
    }

    private void LateUpdate()
    {
        Pathfinding.LateUpdate();
    }

    public GameObject GetCharacterGOFromMap(Character character)
    {
        //Debug.Log(characterGameObjectMap.Count);
        return characterGameObjectMap[character];
    }


    void CreateRandomTasks(int x, TaskSystem taskSystem2)
    {
        for (int i = 0; i < x; i++)
        {
            TaskSystem.Task.MoveToPosition task = new TaskSystem.Task.MoveToPosition { targetPosition = taskSystem2.NewRandomDestinationTS() };
            taskSystem2.AddTask(task);
            Debug.Log("created" + x + " tasks");
            TaskSystem.Task.Victory task2 = new TaskSystem.Task.Victory { };
            taskSystem2.AddTask(task2);
        }
    }



    public void PlaceCharacter(string charType, Tile t, int[] variation)
    {
        if (characterPrototypes.ContainsKey(charType) == false)
        {
            Debug.LogError("place character dosn't contain proper character");
            return;
        }
        Character character = Character.PlaceCharacter(characterPrototypes[charType], t);
        


        character.spriteVariation = variation;


        character.destination.x = t.X;
        character.destination.y = t.Y;
        //Debug.Log("destynacja po stworzeniu =" + tourist.destination);
        World.addTourist(character);


        if (character == null)
        {
            return;
        }
        allTouristList.Add(character);
        //Debug.LogError("turysta utworzony");
        if (cbTouristCreated != null)
        {
            cbTouristCreated(character);
        }


    }


    public void OnCharacterCreated(Character character)
    {
        //Create visuall object connected to data
        GameObject obj_go = new GameObject();
        
        //Attach AI to GO
        taskSystem = WorldController.Instance.taskSystem; 

        //Add touris/GO pair to dictionary
        characterGameObjectMap.Add(character, obj_go);
        
        obj_go.name = character.characterType + "_";
        obj_go.transform.position = WorldController.Instance.CalculateIsoFloatPosition(character.position);//zmiana char controlera------------------------------
        obj_go.transform.SetParent(this.transform, true);
        createGraphicsStructure(obj_go,character.spriteVariation);        

        obj_go.AddComponent<Animator>();
        Animator animator = obj_go.GetComponent<Animator>();
        if (character.spriteVariation[0] == 0)//jest turystą
        {
            CharacterStatistics characterStatistics = obj_go.AddComponent<CharacterStatistics>();
            //characterStatistics.setTouristMultipliers(character.spriteVariation);
            character.Setup(character.spriteVariation,characterStatistics);
            
            touristAI = obj_go.AddComponent<TouristAI>();
            character.touristAI = touristAI;
            if (taskSystem == null) Debug.Log("brak task systema");
            touristAI.Setup(character, taskSystem,characterStatistics);
            touristAI.RegisterActivityChangedCallback((activity) => { characterStatistics.OnActivityChanged(activity); });
            //characterStatistics.OnActivityChanged(touristAI.activity));
            //tile_data.RegisterTileTypeChangedCallback((tile) => { OnTileTypeChanged(tile, tile_go); });


            animator.runtimeAnimatorController = Resources.Load("animations/characters/01/TouristAnimator") as RuntimeAnimatorController;
        }
        else //jest pracownikiem
        {
            WorkerAI workerAI = obj_go.AddComponent<WorkerAI>();
            if (taskSystem == null) Debug.Log("brak task systema");
            workerAI.Setup(character, taskSystem);

            animator.runtimeAnimatorController = Resources.Load("animations/characters/01/WorkerAnimator") as RuntimeAnimatorController;
        }

        characterAnimation = obj_go.AddComponent<CharacterAnimation>();
        // Register our callback so that our GameObject gets updated whenever
        // the object's info changes.
        character.RegisterOnChangedCallback(OnTouristChangedPosition);
        character.RegisterOnNextTilleArrivalCallback(OnCharacterNextTileArrival);
        character.RegisterOnPlayAnimation(OnPlayAnimation);

        charCount += 1;

    }

    void createGraphicsStructure(GameObject character,int[] spriteVariation)
    {
        /*Dictionary<string, Sprite>*/ characterSprites =loadCharacterSprites();
        int direction=4;/* = characterAnimation.direction;*/

        string sV = spriteVariation[0].ToString() + spriteVariation[1].ToString();
        //Debug.Log("sprite variation" +sV);
        Sprite sprite;
        

        GameObject base_go = new GameObject();
        base_go.name = "Base";
        base_go.transform.position = character.transform.position;
        base_go.transform.SetParent(character.transform, true);
        if (characterSprites.ContainsKey(sV+"f_b")) { sprite = characterSprites[sV + "f_b"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        base_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer base_sr = base_go.GetComponent<SpriteRenderer>();
        base_sr.sortingLayerName = ("worldSL");

        GameObject torso_go = new GameObject();
        torso_go.name = "Torso";
        Vector3 pos = base_go.transform.position;
        pos.y += 0.04f;
        torso_go.transform.position = pos;
        torso_go.transform.SetParent(base_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_t")) { sprite = characterSprites[sV + "f_t"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        torso_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer torso_sr = torso_go.GetComponent<SpriteRenderer>();
        torso_sr.sortingLayerName = ("worldSL");

        GameObject head_go = new GameObject();
        head_go.name = "Head";
        pos = torso_go.transform.position;
        pos.y += 0.17f;
        head_go.transform.position = pos;
        head_go.transform.SetParent(torso_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_h")) { sprite = characterSprites[sV + "f_h"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        head_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer head_sr = head_go.GetComponent<SpriteRenderer>();
        head_sr.sortingLayerName = ("worldSL");
        
        GameObject lHand_go = new GameObject();
        lHand_go.name = "Left Hand";
        pos = torso_go.transform.position;
        pos.y += 0.15f;
        pos.x += 0.09f;
        lHand_go.transform.position = pos;
        lHand_go.transform.SetParent(torso_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_lh")) { sprite = characterSprites[sV + "f_lh"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        lHand_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer lHand_sr = lHand_go.GetComponent<SpriteRenderer>();
        lHand_sr.sortingLayerName = ("worldSL");

        GameObject rHand_go = new GameObject();
        rHand_go.name = "Right Hand";
        pos = torso_go.transform.position;
        pos.y += 0.15f;
        pos.x -= 0.09f;
        rHand_go.transform.position = pos;
        rHand_go.transform.SetParent(torso_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_rh")) { sprite = characterSprites[sV + "f_rh"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        rHand_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer rHand_sr = rHand_go.GetComponent<SpriteRenderer>();
        rHand_sr.sortingLayerName = ("worldSL");

        GameObject lFoot_go = new GameObject();
        lFoot_go.name = "Left Foot";
        pos = base_go.transform.position;
        pos.y += 0.09f;
        pos.x += 0.06f;
        lFoot_go.transform.position = pos;
        lFoot_go.transform.SetParent(base_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_lf")) { sprite = characterSprites[sV + "f_lf"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        lFoot_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer lFoot_sr = lFoot_go.GetComponent<SpriteRenderer>();
        lFoot_sr.sortingLayerName = ("worldSL");

        GameObject rFoot_go = new GameObject();
        rFoot_go.name = "Right Foot";
        pos = base_go.transform.position;
        pos.y += 0.09f;
        pos.x += -0.06f;
        rFoot_go.transform.position = pos;
        rFoot_go.transform.SetParent(base_go.transform, true);
        if (characterSprites.ContainsKey(sV + "f_rf")) { sprite = characterSprites[sV + "f_rf"]; }
        else { sprite = null; Debug.Log("missing sprite"); }
        rFoot_go.AddComponent<SpriteRenderer>().sprite = sprite;
        SpriteRenderer rFoot_sr = rFoot_go.GetComponent<SpriteRenderer>();
        rFoot_sr.sortingLayerName = ("worldSL");  
    }

    Dictionary<string, Sprite> loadCharacterSprites()
    {
        Sprite[] SpritesData;
        Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < 10; i++)
            {
                string name1 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "f";
                SpritesData = Resources.LoadAll<Sprite>(name1);
                for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); /*Debug.Log(SpritesData[j].name);*/ }

                string name2 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "fl";
                SpritesData = Resources.LoadAll<Sprite>(name2);
                for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }

                string name3 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "b";
                SpritesData = Resources.LoadAll<Sprite>(name3);
                for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }

                string name4 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "br";
                SpritesData = Resources.LoadAll<Sprite>(name4);
                for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }


                if (k == 0)//load naked variation
                {
                    string name5 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "fn";
                    SpritesData = Resources.LoadAll<Sprite>(name5);
                    for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); /*Debug.Log(SpritesData[j].name);*/ }

                    string name6 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "fln";
                    SpritesData = Resources.LoadAll<Sprite>(name6);
                    for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }

                    string name7 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "bn";
                    SpritesData = Resources.LoadAll<Sprite>(name7);
                    for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }

                    string name8 = "graphics/03.characters/01/" + k.ToString() + i.ToString() + "brn";
                    SpritesData = Resources.LoadAll<Sprite>(name8);
                    for (int j = 0; j < SpritesData.Length; j++) { Sprites.Add(SpritesData[j].name, SpritesData[j]); }

                }




            }
        }
        return Sprites;
    }

    Dictionary<string, Sprite> changeCharacterSprites(int direction)
    {
        Sprite[] spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/empty");
        if (direction == 0) spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/01b");
        if (direction == 4) spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/01f");
        if (direction == 5 || direction == 6) spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/01fl");
        if (direction == 2 || direction == 3) spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/01fl");
        if (direction == 1 || direction==7) spritesData = Resources.LoadAll<Sprite>("graphics/03.characters/01/01br");

        //Debug.Log(direction);

        Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        for (int i = 0; i < spritesData.Length; i++)
        {
            Sprites.Add(spritesData[i].name, spritesData[i]);
        }
        return Sprites;

    }



    void OnTouristChangedPosition(Character tourist)//updejt pozyscji i sorting layerów wzgledem otoczenia
    {
        if (characterGameObjectMap.ContainsKey(tourist) == false)
        {
            Debug.Log("brak zmiany");
            return;
        }
        //Debug.Log("CBping");
                
        GameObject char_go = characterGameObjectMap[tourist];

        //createGraphicsStructure(char_go);
        if (tourist.nextTile != null)
        {
            Vector2 newPos = WorldController.Instance.CalculateIsoFloatPosition(tourist.position);
            char_go.transform.position = new Vector3(newPos.x, newPos.y, 0);
            float x = tourist.nextTile.X;
            float y = tourist.nextTile.Y;
            Vector2 nex_positionIZO = WorldController.Instance.CalculateIsoFloatPosition(new Vector2(x, y));

            //wczytuje strukture postaci
            GameObject child1_go = char_go.transform.GetChild(0).gameObject;
            GameObject child21_go = child1_go.transform.GetChild(0).gameObject;
            GameObject child31_go = child21_go.transform.GetChild(0).gameObject;
            GameObject child32_go = child21_go.transform.GetChild(1).gameObject;
            GameObject child33_go = child21_go.transform.GetChild(2).gameObject;
            GameObject child22_go = child1_go.transform.GetChild(1).gameObject;
            GameObject child23_go = child1_go.transform.GetChild(2).gameObject;

            SpriteRenderer SR1 = child1_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR21 = child21_go.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer SR31 = child31_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR32 = child32_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR33 = child33_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR22 = child22_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR23 = child23_go.GetComponent<SpriteRenderer>();

            // i ustawia sorting order spritów
            SR1.sortingOrder = Mathf.FloorToInt(char_go.transform.position.y * -10f);
            SR21.sortingOrder = SR22.sortingOrder = SR23.sortingOrder = SR1.sortingOrder + 1;
            SR31.sortingOrder = SR32.sortingOrder = SR33.sortingOrder = SR21.sortingOrder + 1;

        }
        //if has item/resource update sprite SR
        if (tourist.resource != null && tourist.resource.resourceGO!=null)
        {
            tourist.resource.resourceGO.GetComponentInChildren<MeshRenderer>().sortingOrder = tourist.resource.resourceGO.GetComponent<SpriteRenderer>().sortingOrder = Resource.GetSortingLayerWhenCarrying(tourist);
            //tourist.resource.resourceGO.GetComponentInChildren<MeshRenderer>().sortingOrder = Resource.GetSortingLayerWhenCarrying(tourist);
        }
    }



     
    void OnCharacterNextTileArrival(Character character)//zmienia sprity i animację po dojściu
    {
        //Debug.Log("onNext Tile Arrival -char controler");
        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.Log("brak zmiany");
            return;
        }

        //if(!character.visible)

        //tourist steped on tile so mark it as walked
        if(character.currentTile!=null)
        character.currentTile.tileWalkedOn();

               //Debug.Log("On nextTile arrival odpalenie przez callbak");

        //wczytuje strukture postaci
        GameObject char_go = characterGameObjectMap[character];

        if (!character.visible)
        {
            char_go.SetActive(false);
            return;
        }
        else char_go.SetActive(true);


        GameObject child1_go = char_go.transform.GetChild(0).gameObject;
        GameObject child21_go = child1_go.transform.GetChild(0).gameObject;
        GameObject child31_go = child21_go.transform.GetChild(0).gameObject;
        GameObject child32_go = child21_go.transform.GetChild(1).gameObject;
        GameObject child33_go = child21_go.transform.GetChild(2).gameObject;
        GameObject child22_go = child1_go.transform.GetChild(1).gameObject;
        GameObject child23_go = child1_go.transform.GetChild(2).gameObject;

        SpriteRenderer SR1 = child1_go.GetComponent<SpriteRenderer>();
        SpriteRenderer SR21 = child21_go.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer SR31 = child31_go.GetComponent<SpriteRenderer>();
        SpriteRenderer SR32 = child32_go.GetComponent<SpriteRenderer>();
        SpriteRenderer SR33 = child33_go.GetComponent<SpriteRenderer>();
        SpriteRenderer SR22 = child22_go.GetComponent<SpriteRenderer>();
        SpriteRenderer SR23 = child23_go.GetComponent<SpriteRenderer>();

        //zmiana sprita---------------------------------------------------------------------------------

        Dictionary<string, Sprite> sprites;
        sprites = null;

        if (character.nextTile != null && character.currentTile != null)
        {
            character.facingDirection = characterAnimation.directionIndex(character.currentTile, character.nextTile);//ustawiam kierunek dalszego marszu
            //Debug.Log("Facing direction is: " + character.facingDirection);
        }

        if (character.currentTile != character.nextTile)//idzie dalej
        {
            if (character.facingDirection <= 7 || character.facingDirection >= 0)
            {
                sprites = characterSprites;
                //sprites = changeCharacterSprites(tourist.facingDirection);
                string name = character.spriteVariation[0].ToString() + character.spriteVariation[1].ToString();
                if (character.facingDirection == 4) name += "f";
                if (character.facingDirection == 0) name += "b";
                if (character.facingDirection == 2||character.facingDirection == 3|| character.facingDirection == 5|| character.facingDirection == 6) name += "fl";
                if (character.facingDirection == 1 || character.facingDirection == 7) name += "br";
                if (character.naked) { name += "n"; }
                /*else name = "11f";*/
                //Debug.Log(name);  
                if (sprites != null)
                {
                    SR1.sprite = sprites[name+"_b"];
                    SR21.sprite = sprites[name + "_t"];
                    SR31.sprite = sprites[name + "_h"];
                    //Debug.LogWarning(name + "_h");if(sprites[name + "_h"]==null) Debug.LogWarning("brakuje sprita");
                    SR32.sprite = sprites[name + "_lh"];
                    SR33.sprite = sprites[name + "_rh"];
                    SR22.sprite = sprites[name + "_lf"];
                    SR23.sprite = sprites[name + "_rf"];
                }
                else {
                    //Debug.LogWarning("brakuje listy spritów");
                }

                //zmiana animacji-----------------------------------------------------------------------------
                Animator animator = char_go.GetComponent<Animator>();
                characterAnimation.SetAnimation(character, animator);
            }
        }
        else if (character.nextTile==null || character.nextTile==character.currentTile)//doszedł do celu lub nie ma dalszej pozycji
        {

            Animator animator = char_go.GetComponent<Animator>();
            characterAnimation.SetAnimation(character, animator);

            //Debug.Log("Stoje w miejscu i ustawiam animacje");
        }

    }

    void OnPlayAnimation(Character character)
    {

        
        //Debug.Log("onPlayAnim -char controler");
        this.tourist = character;
        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.Log("brak turysty");
            return;
        }
        else
        {

            //wczytuje strukture postaci
            GameObject char_go = characterGameObjectMap[character];

            GameObject child1_go = char_go.transform.GetChild(0).gameObject;
            GameObject child21_go = child1_go.transform.GetChild(0).gameObject;
            GameObject child31_go = child21_go.transform.GetChild(0).gameObject;
            GameObject child32_go = child21_go.transform.GetChild(1).gameObject;
            GameObject child33_go = child21_go.transform.GetChild(2).gameObject;
            GameObject child22_go = child1_go.transform.GetChild(1).gameObject;
            GameObject child23_go = child1_go.transform.GetChild(2).gameObject;

            SpriteRenderer SR1 = child1_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR21 = child21_go.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer SR31 = child31_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR32 = child32_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR33 = child33_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR22 = child22_go.GetComponent<SpriteRenderer>();
            SpriteRenderer SR23 = child23_go.GetComponent<SpriteRenderer>();


            //aktualizuje sprity
            Dictionary<string, Sprite> sprites;
            sprites = characterSprites;

            string name = character.spriteVariation[0].ToString() + character.spriteVariation[1].ToString();
            if (character.facingDirection == 4) name += "f";
            if (character.facingDirection == 0) name += "b";
            if (character.facingDirection == 2 || character.facingDirection == 3 || character.facingDirection == 5 || character.facingDirection == 6) name += "fl";
            if (character.facingDirection == 1 || character.facingDirection == 7) name += "br";

            if (character.naked) { name += "n"; }

            /*else name = "11f";*/
            //Debug.Log("Zmieniam zestaw spritów w OnPlay animation na: "+ name);  
            if (sprites != null)
            {
                SR1.sprite = sprites[name + "_b"];
                SR21.sprite = sprites[name + "_t"];
                SR31.sprite = sprites[name + "_h"];
                //Debug.LogWarning(name + "_h");if(sprites[name + "_h"]==null) Debug.LogWarning("brakuje sprita");
                SR32.sprite = sprites[name + "_lh"];
                SR33.sprite = sprites[name + "_rh"];
                SR22.sprite = sprites[name + "_lf"];
                SR23.sprite = sprites[name + "_rf"];
            }
            else
            {
                //Debug.LogWarning("brakuje listy spritów");
            }





            //if (tourist.currentTask is TaskSystem.Task.Victory)

            // i ustawia sorting order spritów
            SR1.sortingOrder = Mathf.FloorToInt(char_go.transform.position.y * -10f);
            SR21.sortingOrder = SR22.sortingOrder = SR23.sortingOrder = SR1.sortingOrder + 1;
            SR31.sortingOrder = SR32.sortingOrder = SR33.sortingOrder = SR21.sortingOrder + 1;

            
            //tourist.state = Character.State.Animating;

            Animator animator = char_go.GetComponent<Animator>();
            if (character.spriteVariation[0] == 0)
            {
                delay = characterAnimation.SetAndPlayAnimation_Tourist(character, animator, character.touristAI.publicActivity);

                //TropicsUtilsClass.CreateWorldTextPopup("Odpalam animacje z " + character.touristAI.publicActivity+"    pos:"+ character.position, characterGameObjectMap[character].transform.position);
            }
            else
            {
                if (character.currentTask == null) Debug.Log("nie mam taska w kontrolerze");
                //Debug.Log("task: " + character.currentTask);
                delay = characterAnimation.SetAnimation_Worker(character, animator, character.currentTask);
            }
            character.waitingTimer = delay;
            character.animationTimer = delay;
            //Debug.Log("onPlayAnim - jako callback z funkcji PlaAnim postaci, ustawia timer animacji i stan animating");

        }
    }

        public void ClearAllcharactersPath()
    {
        
        foreach (Character t in allTouristList)
        {
            t.pathAStar=null;
            //Debug.Log("cleared path of cahracter");
        }

    }


    public void RegisterTouristCreated(Action<Character> callbackfunc)
    {
        cbTouristCreated += callbackfunc;
    }
    public void UnregisterTouristCreated(Action<Character> callbackfunc)
    {
        cbTouristCreated -= callbackfunc;
    }



}
