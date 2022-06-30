using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InstalledObject
{
    public Tile tile //base tile
    {
        get; protected set;
    }
    public string objectType
    {
        get; protected set;
    }

    public float movementCost
    { get; protected set; }//speed multiplayer
    public int width { get; protected set; }
public int height { get; protected set; }
    public bool linksToNeighbour
    {
        get; protected set;
    }
    public bool ghost
    { 
        get; set; 
    }
    public int woodRes;
    public int leavesRes;
    public int stonesRes;
    public int bricksRes;

    public int rotation = 0; //0 no rotation 1,2,3,4 rotation with entrance, 1 - lower right
    public int beauty;
    public bool onWater;
    public bool entry;
    public bool animator;
    public Vector2 entryPos
    {
        get
        {
            if (movementCost == 0)
            {
                if (rotation == 1) return new Vector2(this.tile.X, this.tile.Y + 0.6f);
                if (rotation == 2) return new Vector2(this.tile.X - 0.6f, this.tile.Y);
                Debug.LogError("Obiekt docelowy NOTwalkable bez ustalonego wejścia");
                return new Vector2(this.tile.X, this.tile.Y + 0.6f);
            }
            return new Vector2(this.tile.X, this.tile.Y);
        }
    }

    public int queueLenght = 0;




    public Action<InstalledObject> cbOnChanged
    { get; protected set; }

    public Action<InstalledObject> cbOnOccupied
    { get; protected set; }


    public bool Stockipile = false;
    public Character occupant;

    bool occupied=false;

    public bool Occupied {
        get { return occupied; }
        set {
            occupied = value;
            if (cbOnOccupied != null && occupied==true) 
            { 
                cbOnOccupied(this);
                //Debug.Log("wykonał callback occupied");
            }
        }
    }






    protected InstalledObject()
    {
        //Debug.Log("protectedInstalled Object");
    }

    static public InstalledObject CreatePrototype
        (
        string objectType, float movementCost=1f, int width=1,int height=1, bool linksToNeighbour=false, 
        bool ghost=false, int rotation = 0, int woodRes=0, int leavesRes = 0, int stonesRes = 0, int bricksRes = 0, 
        int beauty=0, bool onWater = false, bool entry =false, bool animator=false
        )
        {
        InstalledObject obj = new InstalledObject();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.linksToNeighbour = linksToNeighbour;
        obj.ghost = ghost;

        obj.rotation = rotation;
        obj.woodRes = woodRes;
        obj.leavesRes = leavesRes;
        obj.stonesRes = stonesRes;
        obj.bricksRes = bricksRes;
        obj.beauty = beauty;
        obj.onWater = onWater;
        obj.entry = entry;
        obj.animator = animator;


        return obj;
    }

    static public InstalledObject PlaceInstance( InstalledObject proto, Tile tile, int rotation)
    {
        InstalledObject obj = new InstalledObject();
        obj.objectType = proto.objectType;

        /*if (proto.ghost == false)*/
            obj.movementCost = proto.movementCost; //movement cost jest zczytywany z Tile w zależności czy obiekt jest fhost czy nie
        /*else obj.movementCost = 1;*/
        
        obj.width = proto.width;
        obj.height = proto.height;
        obj.linksToNeighbour = proto.linksToNeighbour;
        obj.ghost = proto.ghost;
        obj.rotation = rotation;
        if (rotation == 0) obj.rotation = proto.rotation;
        if (proto.rotation == 0) obj.rotation = 0; /*else obj.rotation = proto.rotation;*/


        obj.beauty = proto.beauty;
        obj.onWater = proto.onWater;
        obj.entry = proto.entry;
        obj.animator = proto.animator;

        //Debug.Log("Rotacja po zbudowaniu to"+obj.rotation);

        obj.tile = tile;

        if (obj.objectType != "BuildSlot01")
        {
            if (tile.PlaceObject(obj) == false)
            {                
                //we werent able to place in this tile - occupied
                return null;
            }
        }
            
        
        

        if(proto.objectType=="Stockpile")
        {
            obj.Stockipile = true;
            //WorldController.installedObjectGameObjectMap[obj].GetComponent<SpriteRenderer>().sortingOrder -= 3;
            StockpileSlot s1, s2, s3, s4;
            s1 = new StockpileSlot();
            s2 = new StockpileSlot();
            s3 = new StockpileSlot();
            s4 = new StockpileSlot();
            StockpileSlot.AddStockpileSlot(s1,tile, new Vector2(0f, 0.125f));
            StockpileSlot.AddStockpileSlot(s2,tile, new Vector2(0.25f, 0f));
            StockpileSlot.AddStockpileSlot(s3,tile, new Vector2(0f, -0.125f));
            StockpileSlot.AddStockpileSlot(s4,tile, new Vector2(-0.25f, 0f));
        }        




        //object requires building

        if (proto.ghost==true)//object requires building
        {
            //TMP
            obj.ghost = false;
            //PlaceBuildTask(obj, proto.woodRes,proto.leavesRes,proto.stonesRes,proto.bricksRes);
        }

        //if()


        //object links to neighbour and needs to update them
        if(obj.linksToNeighbour)
        {
            //if yes inform neighbours to update--------------------------------
            int x = obj.tile.X;
            int y = obj.tile.Y;

            Tile NT = tile.world.GetTileAt(x + 1, y);

            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType)
            {
                if (x + 1 <= tile.world.Width - 1)//sprawdzenie czy wychodzimy poza granice
                { 
                //Debug.Log("uaktywniam somsiada01");
                NT.installedObject.cbOnChanged(NT.installedObject);
            }
            }
            NT = tile.world.GetTileAt(x, y + 1);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType)
            {
                if (y + 1 <= tile.world.Height - 1)//sprawdzenie czy wychodzimy poza granice
                {
                    //Debug.Log("uaktywniam somsiada02");
                    NT.installedObject.cbOnChanged(NT.installedObject);
                }
            }
            NT = tile.world.GetTileAt(x - 1, y);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType)
            {
                if (x - 1 >= 0) //sprawdzenie czy wychodzimy poza granice
                {
                    //Debug.Log("uaktywniam somsiada03");
                    NT.installedObject.cbOnChanged(NT.installedObject);
                }
            }
            NT = tile.world.GetTileAt(x, y - 1);
            if (NT != null && NT.installedObject != null && NT.installedObject.objectType == obj.objectType)
            {
                if (y - 1 >= 0) //sprawdzenie czy wychodzimy poza granice
                {
                    //Debug.Log("uaktywniam somsiada04");
                    NT.installedObject.cbOnChanged(NT.installedObject);
                }
            }

        }

        return obj;
    }


    static void PlaceBuildTask(InstalledObject obj, int woodReq, int leavesReq, int stonesReq, int bricksReq)
    {
        //create stockpile for required materials
        WorldController.Instance.World.PlaceInstalledObject("BuildSlot01", obj.tile);
        obj.Stockipile = true;
        StockpileSlot s1, s2, s3, s4;
        s1 = new StockpileSlot();
        s2 = new StockpileSlot();
        s3 = new StockpileSlot();
        s4 = new StockpileSlot();

        StockpileSlot.AddBuildStockpileSlot(s1,obj.tile, new Vector2(0f, 0.125f));
        StockpileSlot.AddBuildStockpileSlot(s2,obj.tile, new Vector2(0.25f, 0f));
        StockpileSlot.AddBuildStockpileSlot(s3,obj.tile, new Vector2(0f, -0.125f));
        StockpileSlot.AddBuildStockpileSlot(s4,obj.tile, new Vector2(-0.25f, 0f));

        //create tasks for supplying build materials

        if (woodReq > 0) {
            WorldController.Instance.taskSystem.EnqueTask(() =>
            {
                StockpileSlot SS = Resource.FindResourceInStockpile(Resource.ResourceType.wood, woodReq);
                
                if (SS!=null)//condition of starting task
                {
                    SS.reservedOut += woodReq;
                    TaskSystem.Task.Supply task = new TaskSystem.Task.Supply
                    {
                        dropOffSlot = s1,
                        pickUpSlot = SS,
                        resourceType = Resource.ResourceType.wood,
                        resourceAmount = woodReq
                    }; 
                    return task;
                } else return null;
            });
        }
        if (leavesReq > 0)
        {
            WorldController.Instance.taskSystem.EnqueTask(() =>
            {
                StockpileSlot SS = Resource.FindResourceInStockpile(Resource.ResourceType.leaves, leavesReq);

                if (SS != null)//condition of starting task
                {
                    SS.reservedOut += leavesReq;
                    TaskSystem.Task.Supply task = new TaskSystem.Task.Supply
                    {
                        dropOffSlot = s2,
                        pickUpSlot = SS,
                        resourceType = Resource.ResourceType.leaves,
                        resourceAmount = leavesReq
                    };
                    return task;
                }
                else return null;
            });
        }
        if (stonesReq > 0)
        {
            WorldController.Instance.taskSystem.EnqueTask(() =>
            {
                StockpileSlot SS = Resource.FindResourceInStockpile(Resource.ResourceType.stones, stonesReq);

                if (SS != null)//condition of starting task
                {
                    SS.reservedOut += stonesReq;
                    TaskSystem.Task.Supply task = new TaskSystem.Task.Supply
                    {
                        dropOffSlot = s3,
                        pickUpSlot = SS,
                        resourceType = Resource.ResourceType.stones,
                        resourceAmount = stonesReq
                    };
                    return task;
                }
                else return null;
            });
        }
        if (bricksReq > 0)
        {
            WorldController.Instance.taskSystem.EnqueTask(() =>
            {
                StockpileSlot SS = Resource.FindResourceInStockpile(Resource.ResourceType.bricks, bricksReq);
                if (SS != null)//condition of starting task
                {
                    SS.reservedOut += bricksReq;
                    TaskSystem.Task.Supply task = new TaskSystem.Task.Supply
                    {
                        dropOffSlot = s4,
                        pickUpSlot = SS,
                        resourceType = Resource.ResourceType.bricks,
                        resourceAmount = bricksReq
                    };
                    return task;
                }
                else return null;
            });
        }

        WorldController.Instance.taskSystem.EnqueTask(() =>
        {
            if (s1.resource.amount == woodReq)
            {
                if (s2.resource.amount == leavesReq)
                {
                    if (s3.resource.amount == stonesReq)
                    {
                        if (s4.resource.amount == bricksReq)
                        {
                            TaskSystem.Task.Build task5 = new TaskSystem.Task.Build
                            {
                                targetPosition = new Vector2(obj.tile.X, obj.tile.Y),
                                buildAction = new Action(() =>
                                    {
                                        obj.ghost = false;
                                        //Debug.LogWarning(obj.movementCost);
                                        //obj.movementCost=World.
                                        GameObject GO = WorldController.installedObjectGameObjectMap[obj];
                                        SpriteRenderer SR = GO.GetComponent<SpriteRenderer>();
                                        Color tmp = SR.color;
                                        tmp.a = 1.0f;// transparency 0-1
                                        SR.color = tmp;
                                        StockpileSlot.HideBuildStockpileSlot(obj.tile);//destroy??
                                        HideBuildResources(GO);
                                        WorldController.Instance.World.OnTileChanged(obj.tile);
                                        WorldController.Instance.World.InvalidateTileGraph();

                                    })
                            };
                            return task5;
                        }
                        else { return null; }
                    }
                    return null;
                }
                return null;
            }
            return null;
        });
        //Debug.Log("Added QuedBuild Task");
    


}
    
    public bool harvestableTree(Tile t)
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


    private static void HideBuildResources(GameObject GO)
    {
                foreach (Transform child in GO.transform)
                {
                    //Debug.Log("jest dziecior i ukrywam");
                    child.gameObject.SetActive(false);
                }
    }

public static void SpawnResources(Tile t)
    {
        Debug.LogWarning("Null object!!!");
        if (t.installedObject!=null && t.installedObject.harvestableTree(t))
        {
            Resource res = new Resource();
            res.SpawnResourcesFromInstalledObject(t);
        }
    }


    public void DestoyInstalledObject(InstalledObject IO)
    {
        IO = null;
    }
    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnChanged += callbackFunc;

    }
    public void UnregisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        //Debug.Log("usunoł callback");
        cbOnChanged -= callbackFunc;
    }
    public void RegisterOnPlayAnimation(Action<InstalledObject> callbackFunc)
    {
        //Debug.Log("zarejestrował callback occupied");
        cbOnOccupied += callbackFunc;

    }
    public void UnregisterOnPlayAnimation(Action<InstalledObject> callbackFunc)
    {
        //Debug.Log("usunoł callback");
        cbOnOccupied -= callbackFunc;
    }




}
