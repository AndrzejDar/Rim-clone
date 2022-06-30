using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;


public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }

    //public WorldGen WorldGen { get; protected set; }

    public CharController charController { get; protected set; }
    
    public TaskSystem taskSystem;

    public MouseController mouseController;

    public StockpileSlot stockpileSlot;

    UIController UIController;

    public Sprite grassSprite,groundSprite,waterSprite,beachSprite,highGrassSprite,pathSprite,waterWalkWaySprite;
    public Material waterMaterial;

    //public Sprite wallSprite;

    ObjectAnimation objectAnimation;

    public static Dictionary<Tile, GameObject> tileGameObjectMap;
    public static Dictionary<Tile, InstalledObject> tileInstalledObjectMap;
    public static Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

    public static Dictionary<Resource, StockpileSlot> ResourceStockpileSlotMap;
    public static Dictionary<StockpileSlot, Resource> StockpileSlotResourceMap;

    public static Dictionary<StockpileSlot, Resource> BuildStockpileSlotResourceMap;
    public static Dictionary<InstalledObject,GameObject> BuildStockpileGOMap;

    public static Dictionary<string, Sprite> objectsSprites;


    
    void Start()
    {
        
        if(Instance !=null)
        { Debug.LogError("There shoudl be no double world controller"); }
        Instance = this;
        //Clock clock = new Clock();
             

        World = new World();
        charController = FindObjectOfType<CharController>();/*new CharController();*///----------------------------------------------------------------
        taskSystem = new TaskSystem();
        stockpileSlot = new StockpileSlot();
        loadSprites();
        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);
        //mouseController = new MouseController();


        MouseController mouseController = GetComponentInChildren<MouseController>();
        mouseController.SetupMC(taskSystem, this, World);////TUTEJ ??????????????????????
        UIController = new UIController();
        UIController.UISetup(World, charController, mouseController);



        //Instantiate our dictionary that tracks which GameObject is rendering which tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
        tileInstalledObjectMap = new Dictionary<Tile, InstalledObject>();

        ResourceStockpileSlotMap=new Dictionary<Resource, StockpileSlot>();
        StockpileSlotResourceMap = new Dictionary<StockpileSlot, Resource>();
        BuildStockpileSlotResourceMap = new Dictionary<StockpileSlot, Resource>();
        BuildStockpileGOMap = new Dictionary<InstalledObject,GameObject>();

        WorldGen worldGen = new WorldGen();
        
        worldGen.populateWorld(World);

        //zrób GameObject dla każdego tilesa wizualnie
        for (int x = 0; x < World.Width; x++)
        {

            for (int y = World.Height-1; y >=0; y--)
            {

                Tile tile_data = World.GetTileAt(x, y);
                GameObject tile_go = new GameObject();
                tile_go.name = "tile" + x + "_" + y;
                tile_go.transform.position = CalculateIsoPosition(tile_data);
                tile_go.transform.SetParent(this.transform, true);

                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();


                if (tile_data.Type == Tile.TileType.Grass)
                {
                    tile_sr.sprite = grassSprite;
                }
                if (tile_data.Type == Tile.TileType.Water)
                {
                    tile_sr.sprite = waterSprite;
                    tile_sr.material = waterMaterial;
                    tile_sr.material.SetFloat("_DepthMultiplier", depthFromCenter(tile_data));
                }
                if (tile_data.Type == Tile.TileType.HighGrass)
                {
                    tile_sr.sprite = highGrassSprite;
                }
                if (tile_data.Type == Tile.TileType.Beach)
                {
                    tile_sr.sprite = beachSprite;
                }
                if (tile_data.Type == Tile.TileType.Path)
                {
                    tile_sr.sprite = beachSprite;
                }
                tileGameObjectMap.Add(tile_data, tile_go);
                tile_data.RegisterTileTypeChangedCallback((tile) => { OnTileTypeChanged(tile, tile_go); });
                


            }
        }

        //władowanie task systemu do MC
        World.beautyMap = new BeautyMap(World);
    }

    // Update is called once per frame
    void Update()
    {
        World.Update(Time.deltaTime);
        //taskSystem.
    }

       


    public void UpdateStockpileSlotResourceMap(StockpileSlot s, Resource resource)
    {
        //ResourceStockpileSlotMap.Add(resource, s);
        if (ResourceStockpileSlotMap.ContainsKey(resource)) { ResourceStockpileSlotMap.Remove(resource); }
        if (StockpileSlotResourceMap.ContainsKey(s)) { StockpileSlotResourceMap.Remove(s); }

        StockpileSlotResourceMap.Add(s, resource);
        ResourceStockpileSlotMap.Add(resource,s);
    }

    public void UpdateSSRM(StockpileSlot SS, Resource R)
    {
        Debug.Log("przed updejtSSRM :" + SS.resource.resourceType + "amount:" + StockpileSlotResourceMap[SS].amount);
        StockpileSlotResourceMap[SS] = R;
        //Debug.Log("updejtSSRM :" + SS.resource.resourceType + "amount:" + R.amount);
        Debug.Log("updejtSSRM2 :" + SS.resource.resourceType + "amount:" + SS.resource.amount);
    }






    public void AddBuildStockpileSlotToDict(Resource resource, StockpileSlot s)
    {
        //ResourceStockpileSlotMap.Add(resource, s);
        BuildStockpileSlotResourceMap.Add(s, resource);
    }
          




    void OnTileTypeChanged(Tile tile_data, GameObject tile_go)
    { 
        //funkcja dodana do callback uruchamianego w momencie zmiany typu tilesa!!!
        //zmieniam sprita
    if(tile_data.Type == Tile.TileType.Grass)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = grassSprite;
        }
        else if(tile_data.Type == Tile.TileType.Ground)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = groundSprite;
        }
        else if (tile_data.Type == Tile.TileType.Water)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = waterSprite;
        }
        else if (tile_data.Type == Tile.TileType.HighGrass)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = highGrassSprite;
        }
        else if (tile_data.Type == Tile.TileType.Beach)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite =beachSprite;
        }
        else if (tile_data.Type == Tile.TileType.Path)
        {
            CreatePathGO(tile_go,tile_data);
        }
        else if (tile_data.Type == Tile.TileType.WaterWalkWay)
        {
            CreateWalkWayGO(tile_go, tile_data);
        }
        else
        {
            Debug.LogError("On TileChange - nieznany typ podłoża");
        }

        //wyczyścić pathfinding
        
        //World.OnTileChanged(tile_data);
        World.InvalidateTileGraph();
        //lagggggggggg
    }


    public void CreateWalkWayGO(GameObject tile_go, Tile t)
    {
        GameObject walkWay_go = new GameObject();
        walkWay_go.name = "path_" + tile_go.name;
        walkWay_go.transform.position = tile_go.transform.position;
        walkWay_go.transform.SetParent(tile_go.transform, true);

        SpriteRenderer path_sr = walkWay_go.AddComponent<SpriteRenderer>();
        path_sr.sprite = waterWalkWaySprite;
        path_sr.sortingOrder = walkWay_go.transform.GetComponentInParent<SpriteRenderer>().sortingOrder+1;
    }


    public void CreatePathGO(GameObject tile_go,Tile t)
    {
        //World.OnTileChanged(t);

        GameObject path_go = new GameObject();
        path_go.name = "path_" + tile_go.name;
        path_go.transform.position = tile_go.transform.position;
        path_go.transform.SetParent(tile_go.transform, true);


        SpriteRenderer path_sr = path_go.AddComponent<SpriteRenderer>();
        path_sr.sprite = GetSpriteForConnectedTile(t);
        path_sr.sortingOrder = path_go.transform.GetComponentInParent<SpriteRenderer>().sortingOrder + 1;

        if (World.GetTileAt(t.X + 1, t.Y).Type == t.Type)         
        {
            Tile tmpT = World.GetTileAt(t.X + 1, t.Y);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X, t.Y+1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X, t.Y+1);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X - 1, t.Y).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X - 1, t.Y);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X, t.Y-1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X, t.Y-1);
            tileGameObjectMap[tmpT].transform./*GetChild(0).GetComponent*/GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }


        if (World.GetTileAt(t.X + 1, t.Y+1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X + 1, t.Y+1);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X-1, t.Y + 1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X-1, t.Y + 1);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X - 1, t.Y-1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X - 1, t.Y-1);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
        if (World.GetTileAt(t.X+1, t.Y - 1).Type == t.Type)
        {
            Tile tmpT = World.GetTileAt(t.X+1, t.Y - 1);
            tileGameObjectMap[tmpT].transform.GetComponentsInChildren<SpriteRenderer>()[1].sprite = GetSpriteForConnectedTile(tmpT);
        }
    }

    
    public Vector3 CalculateIsoPosition(Tile tile) //cell X,Y to xy on screen
    {
        float x,y;
        x = (tile.Y * 1f / 2f) + (tile.X * 1f / 2f);
        y = (tile.X * 0.5f / 2f) - (tile.Y * 0.5f / 2f);
        Vector3 Position = new Vector3(x, y, 0);
        return Position;
    }

    public Vector3 CalculateIsoFloatPosition(Vector2 tile) //cell X,Y to xy on screen
    {
        float x, y;
        x = (tile.y * 1f / 2f) + (tile.x * 1f / 2f);
        y = (tile.x * 0.5f / 2f) - (tile.y * 0.5f / 2f);
        Vector3 Position = new Vector3(x, y, 0);
        return Position;
    }

    public Vector2 CalculateWorldPosition(Vector3 vector) //mouse cords on screen to cell X.Y
    {
        float x, y;
        x = (vector.x / 0.5f + vector.y / 0.25f) / 2f;
        y = -(vector.y / 0.25f - (vector.x / 0.5f)) / 2f;
        Vector2 Position = new Vector2(x, y);
        return Position;
    }

    public void OnInstalledObjectCreated(InstalledObject obj)
    {
        //TMP solution - zmienić na callback w funkcji create beauty MAp
        if (World.beautyMap != null)
        {
            World.beautyMap.onTileTypeChangedUpdateBeautyLocaly(obj.tile);
        }

        //Debug.LogError("OnInstalledObjectCreated");
        //Create visuall object connected to data
        GameObject obj_go = new GameObject();
        //Add tile/GO pair to dictionary
        if (obj == null) Debug.LogError("BRAKUJE OBJJJJJJ");
        if (obj_go == null) Debug.LogError("BRAKUJE OBJ_GOOOOO");
        if (obj.objectType != "BuildSlot01")
        {

            tileInstalledObjectMap.Add(obj.tile, obj);
            installedObjectGameObjectMap.Add(obj, obj_go);
        }
        else {
            BuildStockpileGOMap.Add(obj, obj_go); 
        }

        if(obj.objectType == "Molo01") 
        {
            //tileGameObjectMap[obj.tile]
            CreateWalkWayGO(tileGameObjectMap[obj.tile], obj.tile);
            return;
            //budowanie walkway??
        }




        obj_go.name = obj.objectType + "_" + obj.tile.X + "_" + obj.tile.Y;
        obj_go.transform.position = CalculateIsoPosition(obj.tile);
        obj_go.transform.SetParent(this.transform, true);
        obj_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(obj);
        SpriteRenderer SR = obj_go.GetComponent<SpriteRenderer>();
        if (obj.rotation == 0 || obj.rotation == 1 || obj.rotation == 3)
        {
            SR.flipX = false;
        }
        else { SR.flipX = true; }


        if (obj.ghost == true) // przezroczystość dla ghosta
        {
            Color tmp = SR.color;
            tmp.a = 0.3f;// transparency 0-1
            SR.color = tmp;
            //Debug.Log("PRZEZROCZYSTY");
        }
        else
        {
            //Debug.Log("HUK");
        }
        SR.sortingLayerName = ("worldSL");
        SR.sortingOrder = Mathf.FloorToInt(obj_go.transform.position.y * -10f);

        if (obj.objectType == "BuildSlot01" || obj.objectType == "Stockpile")
        { SR.sortingOrder -= 4; }

        if (obj.objectType == "Molo01" || obj.objectType == "PlaneMolo01")
        { 
            SR.sortingOrder = 1;
            SR.sortingLayerName = "TileSL";
                }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        obj.RegisterOnChangedCallback(OnInstalledObjectChanged);

        if (obj.animator == true)
        {
            obj_go.AddComponent<Animator>();
            Animator animator = obj_go.GetComponent<Animator>();
            animator.runtimeAnimatorController = Resources.Load("animations/items/ObjectAnimator") as RuntimeAnimatorController;
            //objectAnimation = new ObjectAnimation();
            objectAnimation = obj_go.AddComponent<ObjectAnimation>();
            objectAnimation.SetUpObjectAnimation(obj, animator);
        }
        //obj.RegisterOnPlayAnimation(onPlayAnimation);
        if (obj.objectType == "PlaneMolo01")
        {
            obj_go.AddComponent<planeMolo>().Setup(obj,obj_go);

            //planeMolo(obj, obj_go);
        }
    }

    void OnInstalledObjectChanged(InstalledObject obj)
    {
        if (installedObjectGameObjectMap.ContainsKey(obj)==false)
        {
            Debug.Log("brak zmiany");
                return;
        }
        GameObject go = installedObjectGameObjectMap[obj];
        go.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(obj);
        

    }

    //nie używana funkcja
    void BuildInstalledObject(InstalledObject obj, SpriteRenderer SR) //Adding Task
    {
            TaskSystem.Task.Build task = new TaskSystem.Task.Build
            {
                targetPosition = taskSystem.NewRandomDestinationTS(),//go for material
                //targetPosition2 = new Vector2(obj.tile.X, obj.tile.Y),
                buildAction = new Action(()=> 
                {
                    obj.ghost = false;
                    Color tmp = SR.color;
                    tmp.a = 1.0f;// transparency 0-1
                    SR.color = tmp;
                    World.OnTileChanged(obj.tile);
                    World.InvalidateTileGraph();
                })          
            };
            taskSystem.AddTask(task);
        Debug.Log("Build Task added");
        }

    void loadSprites()
    {
       objectsSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("graphics/");
        foreach (Sprite s in sprites)
        {
            objectsSprites[s.name] = s;
            //Debug.Log(s);
        }        
    }

    public Sprite GetSpriteForInstalledObject(InstalledObject obj, int rotation = 0)
    {
        if (obj.linksToNeighbour == false)
        {
            //Debug.Log(obj.objectType + "_");
            if(objectsSprites[obj.objectType + "_"]==null)
            {
                Debug.LogError("Brakuje sprita o podanej nazwie! załaduj!!");
                return null;
            }
            return objectsSprites[obj.objectType+"_"];
        }
        else
        {
            int x = obj.tile.X;
            int y = obj.tile.Y;
            string spriteName = (obj.objectType + "_");
            //Debug.Log("początkowa nazwa sprita:" +spriteName);
            //Debug.Log("sprawdzam somsiada01");
            Tile NT = World.GetTileAt(x + 1, y);
            if ( NT !=null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType && x+1<=World.Width-1)
            {
                spriteName += "1";
            }
            //Debug.Log("sprawdzam somsiada02");
            NT = World.GetTileAt(x , y+1);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType && y+1<=World.Height-1)
            {
                spriteName += "2";
            }
            //Debug.Log("sprawdzam somsiada03");
            NT = World.GetTileAt(x - 1, y);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType&& x-1>=0)
            {
                spriteName += "3";
            }
            //Debug.Log("sprawdzam somsiada04");
            NT = World.GetTileAt(x, y - 1);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType&& y-1>=0)
            {
                spriteName += "4";
            }

            if (objectsSprites.ContainsKey(spriteName)==false)
            {
                Debug.Log("missing sprite for object at name " + spriteName);
                spriteName = "Wall01_";
            }

            //Debug.LogError("wybrana nazwa:" +spriteName);
            return objectsSprites[spriteName];
        }
    }




    Sprite GetSpriteForConnectedTile(Tile t)
    {
            int x = t.X;
            int y = t.Y;

        string spriteName = ("");
        if (t.Type == Tile.TileType.Path)
        {
            spriteName = ("dirt00_");
        }
            //Debug.Log("początkowa nazwa sprita:" +spriteName);
            //Debug.Log("sprawdzam somsiada01");
            Tile NT = World.GetTileAt(x + 1, y);
            if (NT != null && NT.Type == t.Type)
            {
                spriteName += "1";
            }
            //Debug.Log("sprawdzam somsiada02");
            NT = World.GetTileAt(x, y + 1);
            if (NT != null && NT.Type == t.Type)
            {
                spriteName += "2";
            }
            //Debug.Log("sprawdzam somsiada03");
            NT = World.GetTileAt(x - 1, y);
            if (NT != null && NT.Type == t.Type)
            {
                spriteName += "3";
            }
            //Debug.Log("sprawdzam somsiada04");
            NT = World.GetTileAt(x, y - 1);
            if (NT != null && NT.Type == t.Type)
            {
                spriteName += "4";
            }

        NT = World.GetTileAt(x+1, y + 1);
        if (NT != null && NT.Type == t.Type)
        {
            spriteName += "5";
        }
        NT = World.GetTileAt(x - 1, y + 1);
        if (NT != null && NT.Type == t.Type)
        {
            spriteName += "6";
        }
        NT = World.GetTileAt(x - 1, y - 1);
        if (NT != null && NT.Type == t.Type)
        {
            spriteName += "7";
        }
        NT = World.GetTileAt(x + 1, y - 1);
        if (NT != null && NT.Type == t.Type)
        {
            spriteName += "8";
        }

        if (objectsSprites.ContainsKey(spriteName) == false)
        {
            Debug.Log("missing sprite for object at name " + spriteName);
            spriteName = "dirt00_12345678";
        }
        return objectsSprites[spriteName];        
    }

    public bool RemoveInstalledObject(Tile tile)
    {
        if (WorldController.tileInstalledObjectMap.ContainsKey(tile) == true)
        {
            InstalledObject IO = WorldController.tileInstalledObjectMap[tile];
            GameObject GO = WorldController.installedObjectGameObjectMap[IO];
            Destroy(GO);
            IO = null;
            tileInstalledObjectMap.Remove(tile);
            if (tile.RemoveObject())
            {
                //spawn stuff
                //Debug.Log("usunięto skutecznie");
            }
            return true;
        }
        else return false;
    }

    public void SpawnTaskMarker(Tile t, TaskSystem.Task type)
    {
        GameObject marker = new GameObject();
        Vector3 pos = new Vector3(t.X,t.Y,0);
        marker.transform.parent = installedObjectGameObjectMap[t.installedObject].transform;
        marker.transform.position = installedObjectGameObjectMap[t.installedObject].transform.position;
        SpriteRenderer SR = marker.AddComponent<SpriteRenderer>();
        SpriteRenderer SR_parent = installedObjectGameObjectMap[t.installedObject].GetComponent<SpriteRenderer>();
        SR.sortingLayerName = SR_parent.sortingLayerName;
        SR.sortingOrder = SR_parent.sortingOrder;
        if (type is TaskSystem.Task.Harvest)
        {
            if (t.installedObject.harvestableTree(t))
            {
                SR.sprite = Resources.Load<Sprite>("graphics/00.ui/cursor/chop_marker");
                //Debug.Log("Added sprite");
            }
        }

    }





    public void onPlayAnimation(InstalledObject IO) 
    {
        Debug.Log("wykonuje OnPLayAnim");
        Animator anim = WorldController.installedObjectGameObjectMap[IO].GetComponent<Animator>();
        if(anim==null)Debug.Log("Nie mam animatora");
        anim.speed = 1f;
        anim.Play("s01",0);
        ObjectAnimation.PLayObjectAnimation(IO);
        /*AnimationClip[] animation = anim.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in animation)
        {
            if (clip.name == "s01") Debug.Log("Clip Lenght= "+clip.length);
        }*/



        //float clipLength = GetLenghtOfClip("s01", character.spriteVariation[0]);


    }




 /*   public InstalledObject FindClosestObjectOfType2(TouristAI AI, string type, Tile t,bool free)
    {

        //compares part of string "type" to dictionary, when match found chec for distance and select cloasest

        int typeLength = type.Length; //initial lenght of string to compare
        float distanceToCompare = 1000;

        InstalledObject targetIO =null;
        List<KeyValuePair<InstalledObject, GameObject>> matchingTypeList = new List<KeyValuePair<InstalledObject, GameObject>>();

        foreach (KeyValuePair<InstalledObject, GameObject> pair in installedObjectGameObjectMap)
        {
            string toCompare= null;
            StringBuilder sb = null;
            if (pair.Key.objectType.Length >= typeLength) //don't check strings witch are shorter than looked for
            {
                toCompare = pair.Key.objectType.Substring(0, typeLength);//retrive begining of string name//dobra optymalizacja

                if (type == toCompare && pair.Key.ghost == false)//found and was build
                {
                    if (free)
                    {
                        if (pair.Key.Occupied)
                        {
                            matchingTypeList.Add(pair);
                        }
                    }
                    else
                    {
                        matchingTypeList.Add(pair);
                    }
                }
            }
            
        }

        for (int i = 0; i < matchingTypeList.Count; i++)
        {
            int distance=1000;
            Debug.Log(distance +":"+ Time.time);
            StartCoroutine(_calcPathTo(t, matchingTypeList[i].Key.tile, (returnedDistance) => { distance = returnedDistance; Debug.Log("Coroutine executed"); Debug.Log(distance + ":" + Time.time); }));
            
            Debug.Log(distance +":"+ Time.time);
            if (distance < distanceToCompare)
            {
                distanceToCompare=distance;
                targetIO = matchingTypeList[i].Key;
            }
        }
        
        if (targetIO != null)
        {
            //Debug.Log("znalazłem obiekt pod tytułem: " + type +"   "+ targetIO.tile);
            AI.targetInstalledObject = targetIO;
            return targetIO;
        }
        else { 
            Debug.Log("NIE ZNALAZŁEM obiektu pod tytułem: " + type); 
        }
        return null;
    }


    IEnumerator _calcPathTo(Tile start, Tile end, Action<int> callback)
    {
        yield return null;
        Pathfinding pathAStar = new Pathfinding(World, start, end);
        callback(pathAStar.Length()); 
    }*/
    






public InstalledObject FindEmptyAccomodation(int quality, Tile t)
    {
        InstalledObject targetIO = null;
        string toCompare = "Accommodation";
        foreach (KeyValuePair<InstalledObject, GameObject> pair in installedObjectGameObjectMap)
        {
            if (pair.Key.objectType.Length >=toCompare.Length && toCompare == pair.Key.objectType.Substring(0, toCompare.Length))
            {
                if (pair.Key.occupant == null&&pair.Key.ghost==false) { targetIO = pair.Key; }
            }
        }
        if (targetIO != null)
        {
            Debug.Log("znalazłem nocleg");
            return targetIO;
        }
        else Debug.Log("NIE ZNALAZŁEM NOCLEGU");
        return null;
    }


    private float depthFromCenter(Tile t){
        float Wx = World.Width / 2;
        float Wy = World.Height / 2;
        float dist = Mathf.Sqrt(Mathf.Pow(Wx - t.X,2)+Mathf.Pow(Wy - t.Y, 2));

        float lerpDist = (dist-(Wx/2f))/(Wx/2f);

        if (dist > Wx) return 0.5f;
        if (dist < Wx/2) return 0.8f;

        //return 0;
        //Debug.Log(dist + " : " + Wx + " : "+lerpDist+" : " + Mathf.Lerp(1f, 0.5f, lerpDist));
        return Mathf.Lerp(0.8f, 0.5f, lerpDist);



    }


    Tile getEntryTile(InstalledObject IO)
    {
        if (IO == null || IO.rotation == null) return IO.tile;
        else
        {
            //if (IO.rotation != 0)
            //{
            //Debug.Log("rotacja jest:" + IO.rotation);
            if (IO.rotation == 1) return World.GetTileAt(IO.tile.X, IO.tile.Y + 1);
            if (IO.rotation == 2) return World.GetTileAt(IO.tile.X - 1, IO.tile.Y);
            if (IO.rotation == 3) return World.GetTileAt(IO.tile.X, IO.tile.Y - 1);
            if (IO.rotation == 4) return World.GetTileAt(IO.tile.X + 1, IO.tile.Y);
            //}
            return IO.tile;
        }
    }





    public void EnableBeautyHeatMap(bool enable)
    {

        for (int i = 0; i < World.Width; i++)
        {
            for (int j = 0; j < World.Height; j++)
            {
                Tile t = World.GetTileAt(i, j);
                GameObject GO = tileGameObjectMap[t];
                SpriteRenderer SR = GO.GetComponent<SpriteRenderer>();

                if (enable)
                {
                    int beauty = World.beautyMap.GetBeautyAt(t);
                    float f = (float)beauty / 100f;
                    //Debug.Log(f);
                    SR.color = Color.Lerp(Color.red, Color.green, f);
                    //SR.color = new Color(beauty/100,1,1,1);
                }
                else
                {
                    SR.color = Color.white;
                }
            }
        }

        
    }


    
    public InstalledObject FindClosestObjectOfType(string type, Tile t, bool free)
    {

        //compares part of string "type" to dictionary, when match found chec for distance and select cloasest

        int typeLength = type.Length; //initial lenght of string to compare
        float distanceToCompare = 1000;

        InstalledObject targetIO = null;

        foreach (KeyValuePair<InstalledObject, GameObject> pair in installedObjectGameObjectMap)
        {
            string toCompare = null;
            StringBuilder sb = null;
            if (pair.Key.objectType.Length >= typeLength) //don't check strings witch are shorter than looked for
            {
                toCompare = pair.Key.objectType.Substring(0, typeLength);//retrive begining of string name//dobra optymalizacja
                //sb = new StringBuilder(pair.Key.objectType);
                //toCompare = sb.ToString(0, typeLength);
                if (type == toCompare && pair.Key.ghost == false)//found and was build
                {
                    if (free) //searching for free and cloasest object of matching type
                    {
                        if (pair.Key.Occupied == false)
                        {
                            //check is targei is reachable
                            //Pathfinding pathAStar = new Pathfinding(World, t, getEntryTile(pair.Key));
                            float distance = Mathf.Sqrt(Mathf.Pow(t.X - getEntryTile(pair.Key).X, 2) + Mathf.Pow(t.Y - getEntryTile(pair.Key).Y, 2));
                            if (distance < distanceToCompare)
                            {
                                Pathfinding pathAStar = new Pathfinding(World, t, getEntryTile(pair.Key));

                                if (pathAStar.Length() >= 0)
                                {
                                    distanceToCompare = distance;
                                    targetIO = pair.Key;
                                }
                            }
                        }
                    }
                    else //searching for cloasest object of matching type
                    {
                        //Pathfinding pathAStar = new Pathfinding(World, t, getEntryTile(pair.Key));

                        float distance = Mathf.Sqrt(Mathf.Pow(t.X - getEntryTile(pair.Key).X, 2) + Mathf.Pow(t.Y - getEntryTile(pair.Key).Y, 2));
                        if (distance < distanceToCompare)
                        {
                            Pathfinding pathAStar = new Pathfinding(World, t, getEntryTile(pair.Key));

                            if (pathAStar.Length() >= 0)
                            {
                                distanceToCompare = distance;
                                targetIO = pair.Key;
                            }
                        }
                    }
                }
            }

        }
        if (targetIO != null)
        {
            //Debug.Log("znalazłem obiekt pod tytułem: " + type +"   "+ targetIO.tile);
            return targetIO;
        }
        else
        {
            Debug.Log("NIE ZNALAZŁEM obiektu pod tytułem: " + type);
        }
        return null;
    }
    










}
