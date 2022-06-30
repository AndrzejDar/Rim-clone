using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StockpileSlot 
{
    public int Slots;
    public int SlotCapacity=100;
    public Tile tilePosition;
    public Resource resource;
    public bool occupied;
    public int reservedOut;
    public int reservedIn;
    public Vector2 positionModifier;

    public static Dictionary<Resource, Tile > ResourceSlotTileMap;


    public static void AddStockpileSlot(StockpileSlot s, Tile t,Vector2 positionModifier)
    {
        //StockpileSlot s = new StockpileSlot();
        s.tilePosition = t;
        s.occupied = false;
        s.reservedIn = 0;
        s.reservedOut = 0;
        s.positionModifier = positionModifier;
        s.resource = new Resource();
        s.resource.RegisterOnChangedCallback(s.resource.onResourceChanged);
        s.resource.resourceType = Resource.ResourceType.empty;
        //s.resource.resourceType = Resource.ResourceType.empty;
        WorldController.Instance.UpdateStockpileSlotResourceMap(s, s.resource); //zrobić updejt WC na callbacku
        //Debug.Log("StockpileSlot SLOT ADDED");      
    }

    public static void AddBuildStockpileSlot(StockpileSlot s, Tile t, Vector2 positionModifier)
    {
        //StockpileSlot s = new StockpileSlot();
        s.tilePosition = t;
        s.occupied = false;
        s.reservedIn = 0;
        s.reservedOut = 0;
        s.positionModifier = positionModifier;
        s.resource = new Resource();
        s.resource.resourceType = Resource.ResourceType.empty;
        //s.resource.resourceType = Resource.ResourceType.empty;
        WorldController.Instance.AddBuildStockpileSlotToDict(s.resource, s); //zrobić updejt WC na callbacku
        //Debug.Log("BuildStockpileSlot SLOT ADDED");
    }

    public static void HideBuildStockpileSlot(Tile t)
    {
        foreach (KeyValuePair<InstalledObject,GameObject> IOGOM in WorldController.BuildStockpileGOMap)
        {
            if (IOGOM.Key.tile == t )
            {
                IOGOM.Value.SetActive(false);
            }
        }
    }


    public StockpileSlot FindStockpileSlot(Resource r)//for droping resource
    {
        Dictionary<StockpileSlot, Resource> SSRM = WorldController.StockpileSlotResourceMap;
        StockpileSlot s;
        foreach (StockpileSlot SS in SSRM.Keys)
        {
            if (SS.resource.resourceType == r.resourceType) //sprawdzenie do takiego samego slotu
            {
                //Debug.Log("do slotu z takim samym resourcem");
                if (SS.resource.amount+SS.reservedIn < SS.SlotCapacity)//sprawdzenie pojemności
                {
                    StockpileSlot tmpS = SS;
                    //TO DO szukać gdzie trafi nadmiar?
                    s = tmpS;
                    s.reservedIn += r.amount;
                    return s;
                }
                
            }
        }
        foreach (StockpileSlot SS in SSRM.Keys)
        {
            if (SS.resource.resourceType == Resource.ResourceType.empty)//sprawdzenie do nowego slotu
            {
                //Debug.Log("do Slotu pustego");
                StockpileSlot tmpS = SS;
                if (!tmpS.occupied && tmpS.reservedIn==0)
                {
                    s = tmpS;

                    s.resource.resourceType = r.resourceType;
                    //s.resource.amount = 0;
                    s.reservedIn = r.amount;
                    return s;
                }
            }
        }

        s = null;
        Debug.Log("No stockpile Slot Found");
        return s;
    }


    public static StockpileSlot FindStockpileSlotWithResource(Resource.ResourceType rt, int amount)
    {
        Dictionary<StockpileSlot, Resource> SSRM = WorldController.StockpileSlotResourceMap;
        Debug.Log("need:" + rt + " w ilości:" + amount);
        foreach (KeyValuePair<StockpileSlot,Resource> SSR in WorldController.StockpileSlotResourceMap)
        {
            //Debug.Log("PARA");
            StockpileSlot SS = SSR.Key;
            //Resource R = SSR.Value;
            Debug.Log("jest:" + SS.resource.resourceType + " w ilości SS.res:" +SS.resource.amount+ "w ilości R.a" + SS.resource.amount);
            if (SS.resource.resourceType == rt && SS.resource.amount >= amount)
            {               
                return SS;
            }

        }
        Debug.Log("nie znalazłem slotu z poszukiwanym surowcem");
        return null;
    }




}
