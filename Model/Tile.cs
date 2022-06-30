using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile
{

    public enum TileType { Ground, Path, WalkWay, WaterWalkWay, Grass, HighGrass, Sand, Water, Floor, Wall, Beach, Lawn, Empty };

    Action<Tile> cbTileTypeChanged;
    TileType type = TileType.Grass;
    public TileType Type {
        get { return type; }
        set {
            TileType oldType = type;
            type = value;
            //updejt zmiany typu płytek
            if (cbTileTypeChanged != null)
            {
                if (cbTileTypeChanged != null && oldType != type)
                    cbTileTypeChanged(this);

            }
        }

    }

    //LooseObject looseObject;
    public InstalledObject installedObject
    { get; protected set; }

    public Resource layingResource;
    public int tileWalked
    { get; protected set; }





    public World world
    { get; protected set; }
    int x;
    public int X { get { return x; } }
    int y;
    public int Y { get { return y; } }

    public float movementCost
    { get
        {
            if (type == TileType.Empty)
                return 0;

            if (installedObject != null)
            {
                if (installedObject.ghost == false)
                    return installedObject.movementCost;
                if (installedObject.ghost == true)
                    return 0.1f;
            }

            if (type == TileType.Path)
                return 0.8f;
            if (type == TileType.Grass)
                return 0.4f;
            if (type == TileType.HighGrass)
                return 0.2f;
            if (type == TileType.Water)
                return 0;
            if (type == TileType.WaterWalkWay)
                return 0.8f;
            return 0.1f;
            
        } }

    public Tile(World world,int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }


    public void tileWalkedOn()
    {
        this.tileWalked += 8;
        if (this.tileWalked > 100 && this.type==TileType.Grass) 
        {
            this.type = TileType.Path;
            this.tileWalked = 0;
            //do callback;
            //Debug.Log("wlazł i zdeptał");
            if (cbTileTypeChanged != null)
                cbTileTypeChanged(this);
        }

        if (this.tileWalked > 100 && this.type == TileType.HighGrass)
        {
            this.type = TileType.Grass;
            //do callback;
            this.tileWalked = 0;
            //Debug.Log("wlazł i zdeptał");
            if (cbTileTypeChanged != null)
                cbTileTypeChanged(this);
        }
    }




    public bool PlaceObject(InstalledObject objInstance)
    {
        if (objInstance == null) // uninstalling wat was before
        {
            installedObject = null;
            return true;
        }


        //special cases------------------------------------------------
        //when building molo or pathWay over entrance slot
        if((objInstance.objectType == "Molo01" || objInstance.objectType == "PathWay01" )&& this.installedObject!=null && this.installedObject.objectType == "EntranceSlot01")
        {
            return true;
        }


        //check space for landing zone---------------------------------------------------------------------------
        if (objInstance.objectType == "PlaneMolo01")
        {
            int ZWidth = 40;
            int ZHeight = 3;

            if (WorldController.Instance.World.GetTileAtOrNull(this.X - (ZWidth / 2 + 5), this.Y + 1) != null)
            {
                Tile t = WorldController.Instance.World.GetTileAt(this.X - (ZWidth / 2 + 5), this.Y + 1);

                for (int i = 0; i < ZWidth; i++)
                {
                    for (int j = 0; j < ZHeight; j++)
                    {
                        Tile t5 = WorldController.Instance.World.GetTileAtOrNull(t.X + i, t.Y + j);
                        if (t5 == null || t5.installedObject != null || t5.type != TileType.Water) return false;


                    }
                }
            }
            else return false;
        }


        //end of special cases ----------------------------------------









        int width;
        int height;
        if (objInstance.rotation == 0 || objInstance.rotation == 1 || objInstance.rotation == 3)
        {
            width = objInstance.width;
            height = objInstance.height;
        }
        else 
        { 
            width = objInstance.height;
            height = objInstance.width;
        }


        for (int i = 0; i < width; i++)//check for large object
        {
            for (int j = 0; j < height; j++)
            {
                Tile t = WorldController.Instance.World.GetTileAt(this.X + i, this.Y - j);
                if (t.installedObject != null)
                {                    
                    Debug.LogWarning("trying to assing an installed object to a tile that already has one!");
                    return false;
                }
                if (objInstance.onWater == true && t.type != TileType.Water)
                {
                    Debug.LogWarning("trying to build water object on land!!!");
                    return false;
                }
                if (objInstance.onWater == false && t.type == TileType.Water)
                {
                    Debug.LogWarning("trying to build on water what is not allowed!!!");
                    return false;
                }
            }
        }

        Tile t1 = WorldController.Instance.World.GetTileAt(this.X , this.Y+1);
        Tile t2 = WorldController.Instance.World.GetTileAt(this.X-1, this.Y);
        Tile t3 = WorldController.Instance.World.GetTileAt(this.X , this.Y-1);
        Tile t4 = WorldController.Instance.World.GetTileAt(this.X+1, this.Y);

        if (objInstance.rotation > 0 && objInstance.entry)//if object has entrance check if entry tile is empty
        {

            if (objInstance.rotation == 1 && t1.installedObject != null)
            {
                if (t1.installedObject.objectType != "EntranceSlot01" && t1.installedObject.objectType != "Molo01" && t1.installedObject.objectType != "PathWay01")
                    return false;
            }
            if (objInstance.rotation == 2 && t2.installedObject != null)
            {
                if (t2.installedObject.objectType != "EntranceSlot01" && t2.installedObject.objectType != "Molo01" && t2.installedObject.objectType != "PathWay01")
                    return false;
            }
            if (objInstance.rotation == 3 && t3.installedObject != null)
            {
                if (t3.installedObject.objectType != "EntranceSlot01" && t3.installedObject.objectType != "Molo01" && t3.installedObject.objectType != "PathWay01")
                    return false;
            }
            if (objInstance.rotation == 4 && t4.installedObject != null)
            {
                if (t4.installedObject.objectType != "EntranceSlot01" && t4.installedObject.objectType != "Molo01" && t4.installedObject.objectType != "PathWay01")
                    return false;
            }
        }





            for (int i = 0; i < objInstance.width; i++)//occupy space for large object
            {
                for (int j = 0; j < objInstance.height; j++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(this.X + i, this.Y - j);
                    t.installedObject = objInstance;
                }
            }
        if (objInstance.rotation > 0 && objInstance.entry)//occupy space for entry
        {
            if (objInstance.rotation == 1) t1.installedObject= world.installedObjectPrototypes["EntranceSlot01"];
            if (objInstance.rotation == 2) t2.installedObject = world.installedObjectPrototypes["EntranceSlot01"];
            if (objInstance.rotation == 3) t3.installedObject = world.installedObjectPrototypes["EntranceSlot01"];
            if (objInstance.rotation == 4) t4.installedObject = world.installedObjectPrototypes["EntranceSlot01"];
        }


        //occupy space for landing zone---------------------------------------------------------------------------
        if (objInstance.objectType == "PlaneMolo01")
        {
            int ZWidth = 40;
            int ZHeight = 3;

            Tile t = WorldController.Instance.World.GetTileAt(objInstance.tile.x - (ZWidth/2+5), objInstance.tile.y + 1);

            for (int i = 0; i < ZWidth; i++)
            {
                for (int j = 0; j < ZHeight; j++)
                {
                    Tile t5 = WorldController.Instance.World.GetTileAt(t.x + i, t.y + j);
                    t5.installedObject = world.installedObjectPrototypes["LandingSlot01"];
                }
            }
        }



            return true;
    }









    public bool PlaceObjectIsValid(InstalledObject objInstance,int rotation =0)
    {

        //special cases------------------------------------------------
        //when building molo or pathWay over entrance slot
        if ((objInstance.objectType == "Molo01" || objInstance.objectType == "PathWay01") && this.installedObject != null && this.installedObject.objectType == "EntranceSlot01")
        {
            return true;
        }


        //check space for landing zone---------------------------------------------------------------------------
        if (objInstance.objectType == "PlaneMolo01")
        {
            int ZWidth = 40;
            int ZHeight = 3;

            if (WorldController.Instance.World.GetTileAtOrNull(this.X - (ZWidth / 2+5), this.Y + 1) != null)
            {
                Tile t = WorldController.Instance.World.GetTileAt(this.X - (ZWidth / 2+5), this.Y + 1);

                for (int i = 0; i < ZWidth; i++)
                {
                    for (int j = 0; j < ZHeight; j++)
                    {                        
                            Tile t5 = WorldController.Instance.World.GetTileAtOrNull(t.X + i, t.Y + j);
                            if (t5 == null || t5.installedObject != null || t5.type != TileType.Water) return false;
                        

                    }
                }
            }
            else return false;
        }





                                    

        //end of special cases ----------------------------------------









        if (rotation == 0 || rotation == 1 || rotation == 3)
        {
            for (int i = 0; i < objInstance.width; i++)//check for large object
            {
                for (int j = 0; j < objInstance.height; j++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(this.X + i, this.Y - j);
                    if (t.installedObject != null)
                    {
                        return false;
                    }
                    if (objInstance.onWater == true && t.type != TileType.Water) return false;//check for water object
                    if (objInstance.onWater == false && t.type == TileType.Water) return false;//check for not water object
                }
            }
        }

        if (rotation == 2 || rotation == 4)
        {
            for (int i = 0; i < objInstance.height; i++)//check for large object
            {
                for (int j = 0; j < objInstance.width; j++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(this.X + i, this.Y - j);
                    if (t.installedObject != null)
                    {
                        return false;
                    }
                    if (objInstance.onWater == true && t.type != TileType.Water) return false;//check for water object
                    if (objInstance.onWater == false && t.type == TileType.Water) return false;//check for not water object
                }
            }
        }


        Tile t1 = WorldController.Instance.World.GetTileAt(this.X, this.Y + 1);
        Tile t2 = WorldController.Instance.World.GetTileAt(this.X - 1, this.Y);
        Tile t3 = WorldController.Instance.World.GetTileAt(this.X, this.Y - 1);
        Tile t4 = WorldController.Instance.World.GetTileAt(this.X + 1, this.Y);

        if (objInstance.rotation > 0 && objInstance.entry)//check if entrance is empty
        {

            if (rotation == 1 && t1.installedObject != null) { 
                if (t1.installedObject.objectType != "EntranceSlot01" && t1.installedObject.objectType != "Molo01" && t1.installedObject.objectType != "PathWay01")
                    return false; }
            if (rotation == 2 && t2.installedObject != null) {
                if (t2.installedObject.objectType != "EntranceSlot01" && t2.installedObject.objectType != "Molo01" && t2.installedObject.objectType != "PathWay01")
                    return false;
            }
            if (rotation == 3 && t3.installedObject != null){
                if (t3.installedObject.objectType != "EntranceSlot01" && t3.installedObject.objectType != "Molo01" && t3.installedObject.objectType != "PathWay01")
                return false;
            }
            if (rotation == 4 && t4.installedObject != null){
                if (t4.installedObject.objectType != "EntranceSlot01" && t4.installedObject.objectType != "Molo01" && t4.installedObject.objectType != "PathWay01")
                    return false; 
            }

        }
        return true;
    }



    public bool RemoveObject()
    {
        if (installedObject != null) // uninstalling wat was before
        {
            installedObject = null;
            //Debug.Log("usunięto IO");
            return true;
        }
        if (installedObject == null)
        {
            Debug.LogError("trying to remove something that dosent exist!");
            return false;
        }
        return false;
    }



    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;
        if (diagOkay == false)
        {
            ns = new Tile[4]; //clockwise from north
        }
        else
        {
            ns = new Tile[8];
        }
        Tile n;
        n = world.GetTileAt(X, Y + 1);
        ns[0] = n;
        n = world.GetTileAt(X+1, Y);
        ns[1] = n;
        n = world.GetTileAt(X, Y - 1);
        ns[2] = n;
        n = world.GetTileAt(X-1, Y);
        ns[3] = n;
        
        if(diagOkay==true) 
        {
            n = world.GetTileAt(X+1, Y + 1);
            ns[4] = n;
            n = world.GetTileAt(X + 1, Y-1);
            ns[5] = n;
            n = world.GetTileAt(X-1, Y - 1);
            ns[6] = n;
            n = world.GetTileAt(X - 1, Y+1);
            ns[7] = n;
        }
        return ns;
    }


    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged += callback;
    }




}
