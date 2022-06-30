using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class MouseController : MonoBehaviour
{
    private Camera myCamera;
    public WorldController worldController { get; protected set; }

    public GameObject tileCursor;
    public bool buildModeIsObjects { get; protected set; }
    public bool harvestMode { get; protected set; }
    public bool groundMode { get; protected set; }

    bool spawnWorker, spawnTourist;
    TaskSystem TaskSystem;
    World world;
    string buildModeObjectType;
    public enum Mode
    {
        Build, //build mode also changing ground tiles
        Manage, //managing tasks like harvest, clean, transport
        Info, //basic, with character selection 
    }

    public Mode mode { get; protected set; }
    Vector3 lastFramePosition;
    Tile tileDownStart;
    Tile tileDownStop;
    Tile tileDown;
    Tile tileUnderMouse;
    Vector3 currFramePosition;
    Color highlightColor;
    int rotation=0; //for rotating instaled objects


    Action<Tile> cbCursorMoved;

    float cameraZoom;

    // Start is called before the first frame update


    private void Start()
    {
        myCamera = Camera.main;
        mode = Mode.Info;
        highlightColor = new Color(0.8f, 0.8f, 0.8f);
        cameraZoom = 4f;
        myCamera.orthographicSize = cameraZoom;
        //find starting pos for camera
        Tile t = world.GetTileAt(world.Width / 2, world.Height / 2);
        Vector3 centerPos = new Vector3(worldController.CalculateIsoPosition(t).x , 0f);
        myCamera.transform.position = centerPos;
        //set starting modes
        harvestMode = false;
        groundMode = false;
        buildModeIsObjects = false;
        //initiate starting cursor
        tileActionIcon();


    }

    public void SetupMC(TaskSystem newTaskSystem, WorldController worldController, World world)
    {
        this.worldController = worldController;
        TaskSystem = newTaskSystem;
        this.world = world; 
    }


    // Update is called once per frame
    void Update()
    {
        if (myCamera.orthographicSize != cameraZoom)
            handleZoom(cameraZoom);
        checkMousePosition();
        checkForMouseInput();   
    }


    void checkMousePosition()
    {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        if (tileUnderMouse != GetTileAtWorldCoord(currFramePosition))//sprawdzenie czy wskazuje nowego tilesa
        {
            //ClearHighlight(tileUnderMouse);
            tileUnderMouse = GetTileAtWorldCoord(currFramePosition); //returns cell XY under mouse
            tileActionIcon();
            if (cbCursorMoved != null) //CB wchich activates UI change
            {
                cbCursorMoved(tileUnderMouse);
            }
        }
    }

     void checkForMouseInput()
    {
        //scrollwheel input
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && !IsMouseOverUI() && cameraZoom>=4)
        {
            cameraZoom = cameraZoom / 2f;
            handleZoom(cameraZoom);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && !IsMouseOverUI() && cameraZoom <= 8)
        {
            cameraZoom = cameraZoom * 2f;
            handleZoom(cameraZoom);
        }

        //left mouse button-------------------------------------------------------------
        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())         //left click
        {
            tileDownStart = tileUnderMouse;
            tileDown = tileUnderMouse;
        }
        if (Input.GetMouseButton(0) && !IsMouseOverUI()) //left hold
        {
            if (tileDown != tileUnderMouse)
            {
                marqueTileHighlight(tileDownStart, tileDown, Color.white);
                tileDown = tileUnderMouse;
                marqueTileHighlight(tileDownStart, tileDown, highlightColor);
            }
            else { return; }
        }

        if (Input.GetMouseButtonUp(0) && !IsMouseOverUI()) //left clickup
        {
            marqueTileHighlight(tileDownStart, tileDown, Color.white);// unhiglight marque
            tileDownStop = tileUnderMouse;
            Tile t = tileUnderMouse;
            if (t != null)
            {
                checkMode(t);


                if (buildModeIsObjects == true)//budujemy obiekty
                {
                    if (tileDownStart != tileDownStop)//building with marque
                    {
                        marqueObjectBuild(tileDownStart, tileDownStop);
                        marqueTileHighlight(tileDownStart, tileDownStop, Color.white);
                    }
                    else
                    {
                        WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, t,rotation);
                    }
                }
                if (harvestMode == true)
                {
                }

                if (groundMode == true) //zmieniamy tilesy
                {
                    tileDownStop = tileUnderMouse;
                    marqueTileHighlight(tileDownStart, tileDown, Color.white);
                    marqueTileChange(tileDownStart, tileDownStop, Tile.TileType.Water);
                }
            }
        }


        if (Input.GetMouseButtonUp(1))
        {
            if (mode == Mode.Build)
            {
                if (rotation != 2) rotation = 2;
                else rotation = 1;
                tileActionIcon();
            }
        }

        //middle mouse button-------------------------
        //Handle screen draging
        if (Input.GetMouseButton(2) && !IsMouseOverUI())
        {
            Vector3 diff = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(diff);
        }
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;

    }
    void checkMode(Tile t)
    {
        switch (mode)
        {
            case Mode.Build:
                break;
            case Mode.Manage:
                if (harvestMode == true)
                {
                    if (harvestable(t)) { Harvest(t); }
                }
                if (spawnWorker)
                {
                    int[] variation = { 1, 0, 0, 0 };
                    variation[0] = 1; //0 - tourist, 1- worker
                    variation[1] = UnityEngine.Random.Range(0, 9); //head / base choice 
                    variation[2] = UnityEngine.Random.Range(0, 4); // body choice
                    //variation[3] = UnityEngine.Random.Range(0, 2); //tourist tier/dificulty

                    string name = "Worker1" + variation[1].ToString();
                    //Debug.Log(name);
                    CharController.Instance.PlaceCharacter(name, tileDownStop,variation);
                }
                if (spawnTourist)
                {
                    int[] variation= { 0, 0, 0, 0 };
                    variation[0] = 0; //0 - tourist, 1- worker
                    variation[1] = UnityEngine.Random.Range(0, 0); //head / base choice 
                    variation[2] = UnityEngine.Random.Range(0, 4); // body choice
                    variation[3] = UnityEngine.Random.Range(1, 3); //tourist tier/dificulty
                    string name = "Tourist0" + variation[1].ToString();
                    CharController.Instance.PlaceCharacter(name, tileDownStop,variation);
                }
                break;
            case Mode.Info:
                break;
        }
    }


    void Harvest(Tile t) //adding harvest task;
    {

        //Tile tmp = t;
        
        
        TaskSystem.Task.Harvest task = new TaskSystem.Task.Harvest
        {
            targetPosition = new Vector2(tileUnderMouse.X, tileUnderMouse.Y),
            harvestAction = new Action(() =>
            {
                InstalledObject.SpawnResources(t);//TO DO spawn things according to harvested Game Object;
                worldController.RemoveInstalledObject(t);
            })


        };
        TaskSystem.AddTask(task);
        worldController.SpawnTaskMarker(t,task);
        //Debug.Log("Added harvest Task");

    }

    void tileActionIcon()
    {

        //Update cursor position
        if (tileUnderMouse != null)
        {
            tileCursor.SetActive(true);
            Vector3 currsorPosition = worldController.CalculateIsoPosition(tileUnderMouse);
            tileCursor.transform.position = currsorPosition;
            SpriteRenderer tileMarkerSR = tileCursor.GetComponent<SpriteRenderer>();

            if (mode == Mode.Manage)
            {
                Color tmp;
                if (harvestMode == true)
                {
                    tileMarkerSR = tileCursor.GetComponent<SpriteRenderer>();
                    tileMarkerSR.sprite = Resources.Load<Sprite>("graphics/00.UI/cursor/chop");
                    tmp = tileMarkerSR.color;
                    if (harvestable(tileUnderMouse) == true) { tmp.a = 1f; }
                    else { tmp.a = 0.5f; }
                    tileMarkerSR.color = tmp;
                    return;
                }                               
                //deafult manage state
                GameObject tileToHighlight = WorldController.tileGameObjectMap[tileUnderMouse];
                SpriteRenderer tileSR = tileToHighlight.GetComponent<SpriteRenderer>();
                //tileSR.color = highlightColor; 
                return;
            }

            if (mode == Mode.Info)
            {
                Color tmp;
                //deafult manage state
                GameObject tileToHighlight = WorldController.tileGameObjectMap[tileUnderMouse];

                //Tile t = WorldController.Instance.World.GetTileAt(x, y);

                SpriteRenderer tileSR = tileToHighlight.GetComponent<SpriteRenderer>();
                //tileSR.color = highlightColor;
                tileMarkerSR.sprite = Resources.Load<Sprite>("graphics/00.UI/cursor/cursor01");

                return;
            }

            if (mode == Mode.Build)
            {
                //Tile t = WorldController.Instance.World.GetTileAt(x, y);

                GameObject tileToHighlight = WorldController.tileGameObjectMap[tileUnderMouse];
                tileMarkerSR = tileCursor.GetComponent<SpriteRenderer>();
                tileMarkerSR.sprite = WorldController.Instance.GetSpriteForInstalledObject(world.installedObjectPrototypes[buildModeObjectType]);
                if (rotation == 2 || rotation == 4)
                {
                    tileMarkerSR.flipX = true;
                }
                else tileMarkerSR.flipX = false;
                Color tmp = tileMarkerSR.color;

                
                if (tileUnderMouse.PlaceObjectIsValid(world.installedObjectPrototypes[buildModeObjectType],rotation) == false)
                {
                    tmp = Color.red;
                    tmp.a = 0.7f; }
                else
                {
                    tmp = Color.white;
                    tmp.a = 0.7f; }
                tileMarkerSR.color = tmp;
                return;
            }


            tileMarkerSR.sprite = Resources.Load<Sprite>("graphics/00.UI/cursor/cursor01");
        }
        else
        {
            tileCursor.SetActive(false);
        }
    }

    void ClearHighlight(Tile t)
    {
        if (t == null) return;
        GameObject tileGOToClear = WorldController.tileGameObjectMap[t];
        SpriteRenderer sr = tileGOToClear.GetComponent<SpriteRenderer>();
        sr.color = Color.white;
    }

    bool harvestable(Tile t)
    {
        //Debug.Log("harvestable");
        if (t.installedObject != null && (t.installedObject.objectType == "Foliage01" ||
            t.installedObject.objectType == "Foliage02"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void marqueTileChange(Tile tileStart, Tile tileEnd, Tile.TileType type)
    {
        int start_X = tileStart.X;
        int start_Y = tileStart.Y;
        int end_X = tileEnd.X;
        int end_Y = tileEnd.Y;

        if (start_X > end_X)
        {
            int tmp = start_X;
            start_X = end_X;
            end_X = tmp;
        }
        if (start_Y > end_Y)
        {
            int tmp = start_Y;
            start_Y = end_Y;
            end_Y = tmp;
        }
        for (int x = start_X; x <= end_X; x++)
        {
            for (int y = start_Y; y <= end_Y; y++)
            {
                Tile changeTile = world.GetTileAt(x, y); // złego tilesa biere???
                //Tile changeTile = WorldController.Instance.World.GetTileAt(x, y); // złego tilesa biere???
                changeTile.Type = type;
            }
        }
    }


    void marqueTileHighlight(Tile tileStart, Tile tileEnd, Color highlightColor)
    {
        int start_X = tileStart.X;
        int start_Y = tileStart.Y;
        int end_X = tileEnd.X;
        int end_Y = tileEnd.Y;

        if (start_X > end_X)
        {
            int tmp = start_X;
            start_X = end_X;
            end_X = tmp;
        }
        if (start_Y > end_Y)
        {
            int tmp = start_Y;
            start_Y = end_Y;
            end_Y = tmp;
        }
        for (int x = start_X; x <= end_X; x++)
        {
            for (int y = start_Y; y <= end_Y; y++)
            {
                Tile t = WorldController.Instance.World.GetTileAt(x, y);
                GameObject tileToHighlight = WorldController.tileGameObjectMap[t];
                SpriteRenderer sr = tileToHighlight.GetComponent<SpriteRenderer>();
                sr.color = highlightColor;
            }
        }
    }


    void marqueObjectBuild(Tile tileStart, Tile tileEnd)
    {
        int start_X = tileStart.X;
        int start_Y = tileStart.Y;
        int end_X = tileEnd.X;
        int end_Y = tileEnd.Y;

        if (start_X > end_X)
        {
            int tmp = start_X;
            start_X = end_X;
            end_X = tmp;
        }
        if (start_Y > end_Y)
        {
            int tmp = start_Y;
            start_Y = end_Y;
            end_Y = tmp;
        }
        for (int x = start_X; x <= end_X; x++)
        {
            for (int y = start_Y; y <= end_Y; y++)
            {
                if ((x == start_X) || (x == end_X) || (y == start_Y) || (y == end_Y))
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, t);
                }
            }
        }
    }


    private void handleZoom(float cZ) //zooming when mouse scroll is used
    {
        float cameraZoom =cZ;

        float cameraZoomDifference = (float)System.Math.Round(cameraZoom, 1) - (float)System.Math.Round(myCamera.orthographicSize, 3);
        float cameraZoomSpeed = 10f;

        if (cameraZoom < 1) { cameraZoom = 0.5f; }
        if (cameraZoom > 16) { cameraZoom = 16; }

        myCamera.orthographicSize += cameraZoomDifference * cameraZoomSpeed * Time.deltaTime;

        //if (cameraZoom + 0.001f < myCamera.orthographicSize) myCamera.orthographicSize = cameraZoom;

        if (cameraZoomDifference > 0)
        {
            if (myCamera.orthographicSize > cameraZoom-0.001f)
            {
                myCamera.orthographicSize = cameraZoom;
            }
        }
        else
        {
            if (myCamera.orthographicSize < cameraZoom+0.001f)
            {
                myCamera.orthographicSize = cameraZoom;
            }
        }
    }


    Tile GetTileAtWorldCoord(Vector3 coord)
    {
        Vector2 currsorPosition = worldController.CalculateWorldPosition(coord);
        int x = Mathf.FloorToInt(currsorPosition.x + 0.5f);
        int y = Mathf.FloorToInt(currsorPosition.y + 0.5f);
        return WorldController.Instance.World.GetTileAt(x, y);
    }


    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }


    public void SetMode_BuildFloor()
    {
        ClearModes();
    }

    public void SetMode_Bulldoze()
    {
        ClearModes();
    }

    public void SetMode_BuildInstalledObject(string objectType)
    {
        ClearModes();
        buildModeIsObjects = true;
        buildModeObjectType = objectType;

        mode = Mode.Build;
    }




        public void SetMode_Harvest()
    {
        ClearModes();
        harvestMode = true;
        mode = Mode.Manage;
    }
    public void SetMode_Ground()
    {
        ClearModes();
        groundMode = true;
        mode = Mode.Build;
    }

    public void SetMode_Deafult()
    {
        ClearModes();
    }

    public void SpawnWorker()
    {
        ClearModes();
        spawnWorker = true;
        mode = Mode.Manage;
    }

    public void SpawnTourist()
    {
        ClearModes();
        spawnTourist = true;
        mode = Mode.Manage;
    }

    private void ClearModes()
    {
        buildModeIsObjects = false;
        harvestMode = false;
        groundMode = false;
        spawnTourist = false;
        spawnWorker = false;
        mode = Mode.Info;
    }


    public void RegisterCursorMoved(Action<Tile> callbackfunc)
    {
        cbCursorMoved += callbackfunc;
    }
    public void UnegisterCursorMoved(Action<Tile> callbackfunc)
    {
        cbCursorMoved -= callbackfunc;
    }

}