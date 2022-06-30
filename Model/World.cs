using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World 
{
    Tile[,] tiles;
    //list of tourists in world
    static List<Character> touristsList;
    

    //pathfinding graph
    public Path_TileGraph tileGraph;

    public Dictionary<string, InstalledObject> installedObjectPrototypes { get; protected set; }

    Clock clock;


    int width;
    public int Width
    { get {return width;} }
    int height;
    public int Height
    { get { return height; } }

    public Action<InstalledObject> cbInstalledObjectCreated;
    Action<Tile> cbTileChanged;

    public BeautyMap beautyMap;

    //debugging:
    public static bool setPathfindingDebug = false;
    bool disabledPFD;


    public World(int width=80, int height=80  )
    {
        this.width = width;
        this.height = height;

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
            }

        }
        //Debug.Log("world created w:" + width + ", h:" + height + "succesfully");

        
        //beautyMap = new BeautyMap(this);
        
        CreateInstalledObjectPrototypes();
        clock = new Clock();

        touristsList = new List<Character>();
    }

    public static void addTourist(Character t)
    {
        touristsList.Add(t); 
    }

    public void Update(float deltaTime)
    {
        //Debug.Log("U1:" + deltaTime);
        if (Clock.Speed != 0)
        {
            //Debug.LogWarning( Clock.Speed);
            clock.UpdateTime(deltaTime * Clock.Speed);
            //Debug.Log("u2:" + deltaTime*Clock.Speed);
            foreach (Character t in touristsList)
            {
                t.Update(deltaTime * Clock.Speed);
            }
        }
    }


    void CreateInstalledObjectPrototypes()
    {
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        InstalledObjectsPrototypes IOP = new InstalledObjectsPrototypes();
        IOP.Prototypes(installedObjectPrototypes);
    }

    public Tile GetNearestEmptyTile(Tile t)
    {
        for (int i = 0; i < 5; i++)
        {
            int rnd = UnityEngine.Random.Range(0, 7);
            switch (rnd)
            {
                case 0:
                    if (GetTileAt(t.X - 1, t.Y).installedObject == null) return GetTileAt(t.X - 1, t.Y); break;
                case 1:
                    if (GetTileAt(t.X, t.Y + 1).installedObject == null) return GetTileAt(t.X, t.Y + 1); break;
                case 2:
                    if (GetTileAt(t.X + 1, t.Y).installedObject == null) return GetTileAt(t.X + 1, t.Y); break;
                case 3:
                    if (GetTileAt(t.X, t.Y - 1).installedObject == null) return GetTileAt(t.X, t.Y - 1); break;
                case 4:
                    if (GetTileAt(t.X + 1, t.Y + 1).installedObject == null) return GetTileAt(t.X + 1, t.Y + 1); break;
                case 5:
                    if (GetTileAt(t.X - 1, t.Y + 1).installedObject == null) return GetTileAt(t.X - 1, t.Y + 1); break;
                case 6:
                    if (GetTileAt(t.X + 1, t.Y - 1).installedObject == null) return GetTileAt(t.X + 1, t.Y - 1); break;
                case 7:
                    if (GetTileAt(t.X - 1, t.Y - 1).installedObject == null) return GetTileAt(t.X - 1, t.Y - 1); break;
            }
        }
        if (GetTileAt(t.X - 1, t.Y).installedObject == null) return GetTileAt(t.X - 1, t.Y); 
        if (GetTileAt(t.X, t.Y + 1).installedObject == null) return GetTileAt(t.X, t.Y + 1);
        if (GetTileAt(t.X + 1, t.Y).installedObject == null) return GetTileAt(t.X + 1, t.Y);
        if (GetTileAt(t.X, t.Y - 1).installedObject == null) return GetTileAt(t.X, t.Y - 1);

        if (GetTileAt(t.X + 1, t.Y + 1).installedObject == null) return GetTileAt(t.X + 1, t.Y + 1);
        if (GetTileAt(t.X - 1, t.Y + 1).installedObject == null) return GetTileAt(t.X - 1, t.Y + 1);
        if (GetTileAt(t.X + 1, t.Y - 1).installedObject == null) return GetTileAt(t.X + 1, t.Y - 1);
        if (GetTileAt(t.X - 1, t.Y - 1).installedObject == null) return GetTileAt(t.X - 1, t.Y - 1);

        if (GetTileAt(t.X - 2, t.Y).installedObject == null) return GetTileAt(t.X - 2, t.Y);
        if (GetTileAt(t.X, t.Y + 2).installedObject == null) return GetTileAt(t.X, t.Y + 2);
        if (GetTileAt(t.X + 2, t.Y).installedObject == null) return GetTileAt(t.X + 2, t.Y);
        if (GetTileAt(t.X, t.Y - 2).installedObject == null) return GetTileAt(t.X, t.Y - 2);


        else return GetTileAt(0,0);

    }



    public Tile GetRoamEmptyTile(Tile t)
    {
        int x = UnityEngine.Random.Range(-4, 4);
        int y = UnityEngine.Random.Range(-4,4);

        Tile target = GetTileAt(t.X + x, t.Y + y);
        while (target.movementCost == 0)
        {
            x = UnityEngine.Random.Range(-4, 4);
            y = UnityEngine.Random.Range(-4, 4);
            target = GetTileAt(t.X + x, t.Y + y);
        }

        return target;

    }




    public Tile GetTileAtOrNull(int x,int y)
    {
        if(x>=width || x<0 ||y>=height || y<0)
        {            
            return null;
        }
        return tiles[x, y];
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x >= width || x < 0 || y >= height || y < 0)
        {
            //Debug.LogError("Tile ("+x+","+y+") is out of range");
            if (x >= width) x = width - 1;
            if (x < 0) x = 0;
            if (y >= height) y = height - 1;
            if (y < 0) y = 0;
            return tiles[x, y];
            //return null;
        }
        return tiles[x, y];
    }


    public void PlaceInstalledObject(string objectType, Tile t,int rotation=0)
    {
        if (installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("installed object dosn't contain proper object");
            return;
        }
        InstalledObject obj = InstalledObject.PlaceInstance(installedObjectPrototypes[objectType], t,rotation);

        if (obj == null)
        {
            return;
        }

        if (cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(obj);
            //InvalidateTileGraph();
        }
        if (installedObjectPrototypes[objectType].ghost == false) InvalidateTileGraph();
    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        //Debug.Log("zmieniam świat");
        cbInstalledObjectCreated += callbackfunc;
    }
    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        cbInstalledObjectCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged += callbackfunc;
    }
    public void UnegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged -= callbackfunc;
    }


    public void OnTileChanged(Tile t)
    {
        //Debug.Log("zmieniam świat2");
        if (cbTileChanged == null)
            return;

        cbTileChanged(t);
        InvalidateTileGraph();
    }




    // function that is called when world changes to destroy old graph
    public void InvalidateTileGraph()
    {
        //Debug.Log("resetuje tile graph");

        //reset all pathfinding


        CharController.Instance.ClearAllcharactersPath();
        //Debug.Log("restartuje pathfinding!");




        /*
        //czyszcze debuging pathfindingu - powofuje LAG!!!!!!!!!!!!!!!!!!!!
        if (WorldController.tileGameObjectMap != null)
        {
            foreach (KeyValuePair<Tile,GameObject> t in WorldController.tileGameObjectMap)
            {
                if (WorldController.tileGameObjectMap[t.Key] != null
                    && WorldController.tileGameObjectMap[t.Key].GetComponentInChildren<TextMesh>() != null 
                    && WorldController.tileGameObjectMap[t.Key].GetComponentInChildren<TextMesh>().gameObject != null
                    )
                {
                    UnityEngine.Object.Destroy(WorldController.tileGameObjectMap[t.Key].GetComponentInChildren<TextMesh>().gameObject);
                }
            }
        }*/


    }


}
