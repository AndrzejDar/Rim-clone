using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using CodeMonkey.Utils;

public class Resource

{
    public enum ResourceType { empty, wood, leaves, stones, bricks, twigs, /*stone,*/ water, coconuts };

    public ResourceType resourceType;

    public Tile tilePosition;
    public int amount{get; protected set; }
    public GameObject resourceGO;
    public GameObject parent;

    public Action<Resource> cbOnChanged
    { get; protected set; }


    public Resource SpawnResource(ResourceType resourceType, int amount, Tile t)
    {

        Resource resource = new Resource();
        //resource.RegisterOnChangedCallback(resource.onResourceChanged);
        resource.RegisterOnChangedCallback(onResourceChanged);
        resource.resourceType = resourceType;
        resource.amount = amount;
        resource.tilePosition = t;
        t.layingResource = resource;
        resource.resourceGO = new GameObject();
        resource.resourceGO.name = resource.resourceType.ToString();
        resource.resourceGO.transform.position = WorldController.Instance.CalculateIsoPosition(t);
        //WorldController.Instance.World.GetTileAt(t).;
        SpriteRenderer SR = resource.resourceGO.AddComponent<SpriteRenderer>(); 
        SR.sortingLayerName = ("worldSL");
        SR.sortingOrder = Mathf.FloorToInt(SR.transform.position.y * -10f);

        CreateWorldText(resource.resourceGO.transform,amount.ToString(), resource.resourceGO.transform.position);
        if(resourceType == ResourceType.wood)SR.sprite = Resources.Load<Sprite>("graphics/05.resources/wood");
        if (resourceType == ResourceType.twigs) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/twigs");
        if (resourceType == ResourceType.leaves) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/leaves");
        if (resourceType == ResourceType.stones) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/stone");
        if (resourceType == ResourceType.water) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/water");
        if (resourceType == ResourceType.coconuts) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/coconuts");

        return resource;
    }


    public GameObject CreateResourceGO(Resource resource, GameObject parent)
    {
        resource.RegisterOnChangedCallback(onResourceChanged);
        resourceGO = new GameObject();
        resourceGO.name = resource.resourceType.ToString();
        resourceGO.transform.parent = parent.transform;
        resourceGO.transform.position = parent.transform.position;
        SpriteRenderer SR = resourceGO.AddComponent<SpriteRenderer>();
        SR.sortingLayerName = ("worldSL");
        SR.sortingOrder = Mathf.FloorToInt(SR.transform.position.y * -10f);

        CreateWorldText(resourceGO.transform, amount.ToString(), resourceGO.transform.position);
        if (resource.resourceType == ResourceType.wood) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/wood");
        if (resource.resourceType == ResourceType.twigs) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/twigs");
        if (resource.resourceType == ResourceType.leaves) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/leaves");
        if (resource.resourceType == ResourceType.stones) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/stone");
        if (resource.resourceType == ResourceType.water) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/water");
        if (resource.resourceType == ResourceType.coconuts) SR.sprite = Resources.Load<Sprite>("graphics/05.resources/coconuts");
        //Debug.Log("Resource new GO created");
        //Debug.Log("resource.type" + resource.resourceType);
        return resourceGO;
    }


    public void onResourceChanged(Resource resource)
    {
        //resource.resourceGO.transform.GetComponentInChildren<MeshRenderer>().te;
        //Debug.LogError("Resurcesc changed on callback");

        if (resource.amount == 0)//destroy resource
        {
            /*if (resource.resourceGO != null) Debug.Log("destroyed resource GO");*/
            UnityEngine.Object.Destroy(resource.resourceGO);

        }
        else
        {
            resource.resourceGO.transform.GetComponentInChildren<TextMesh>().text = resource.amount.ToString();
            resource.resourceGO.GetComponentInChildren<MeshRenderer>().sortingOrder = Mathf.FloorToInt(resource.resourceGO.transform.position.y * -10f) + 4;
            //resource.resourceGO.GetComponent<TextMesh>().GetComponent<MeshRenderer>().sortingOrder = Mathf.FloorToInt(resource.resourceGO.transform.position.y * -10f) + 4;



        }


    }




    public void SpawnResourcesFromInstalledObject(Tile t)
    {
        if (t.installedObject.harvestableTree(t))
        {
            Tile tmpT = neighbourTile(t);
            Resource r1= SpawnResource(ResourceType.wood, 10,tmpT );
            HaulResource(r1, tmpT);
            tmpT = neighbourTile(t);
            Resource r2 = SpawnResource(ResourceType.coconuts, 2,tmpT);
            HaulResource(r2, tmpT);
            tmpT = neighbourTile(t);
            Resource r3 = SpawnResource(ResourceType.leaves, 20, tmpT);
            HaulResource(r3, tmpT);
        }
    }


    public void HaulResource(Resource r, Tile t, StockpileSlot slot=null) 
    {


        WorldController.Instance.taskSystem.EnqueTask(() =>
        {
            StockpileSlot s = WorldController.Instance.stockpileSlot.FindStockpileSlot(r);
            if (s != null)
            {
                //s.reserved = true;
                Tile tmp = t;

                TaskSystem.Task.Haul task = new TaskSystem.Task.Haul
                {
                    targetPosition = new Vector2(t.X, t.Y),
                    targetPosition2 = new Vector2(s.tilePosition.X, s.tilePosition.Y),
                    dropOffSlot = s,
                    resource = r,
                };
                //Debug.Log("Added Haul Task");
                return task;
            }
            else {
                Debug.Log("no free stockpile slot - enquing task");
                return null;
            }
        });

    }


    public void DropResource(Resource resource)
    {
        Debug.LogError("DROPING RESOURCE!");
    }


    private Tile neighbourTile(Tile t)
    {
        Tile freeTile;
        int rnd = 0;
        if (WorldController.Instance.World.GetTileAt(t.X + 1, t.Y).layingResource == null
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y).installedObject == null
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X + 1, t.Y);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X, t.Y + 1).layingResource == null
    && WorldController.Instance.World.GetTileAt(t.X, t.Y + 1).installedObject == null
    && WorldController.Instance.World.GetTileAt(t.X, t.Y + 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X, t.Y + 1);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X - 1, t.Y).layingResource == null
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y).installedObject == null
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X - 1, t.Y);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X, t.Y - 1).layingResource == null
                 && WorldController.Instance.World.GetTileAt(t.X, t.Y - 1).installedObject == null
                 && WorldController.Instance.World.GetTileAt(t.X, t.Y - 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X, t.Y - 1);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X + 1, t.Y + 1).layingResource == null
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y + 1).installedObject == null 
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y + 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X + 1, t.Y + 1);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X - 1, t.Y + 1).layingResource == null
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y + 1).installedObject == null 
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y + 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X - 1, t.Y + 1);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X - 1, t.Y - 1).layingResource == null 
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y - 1).installedObject != null
            && WorldController.Instance.World.GetTileAt(t.X - 1, t.Y - 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X - 1, t.Y - 1);
            return freeTile;
        }
        if (WorldController.Instance.World.GetTileAt(t.X + 1, t.Y - 1).layingResource == null
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y - 1).installedObject == null
            && WorldController.Instance.World.GetTileAt(t.X + 1, t.Y - 1) != null)
        {
            freeTile = WorldController.Instance.World.GetTileAt(t.X + 1, t.Y - 1);
            return freeTile;
        }        
        return t;
    }


        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition/*, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder*/)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(0.1f,0.1f)/*localPosition*/;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor =  TextAnchor.LowerLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.text = text;
            textMesh.characterSize = .01f;
            textMesh.fontSize = 128;
            textMesh.color = Color.white;
        textMesh.GetComponent<MeshRenderer>().sortingLayerName = parent.GetComponentInParent<SpriteRenderer>().sortingLayerName;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = parent.GetComponentInParent<SpriteRenderer>().sortingOrder+4;
            return textMesh;
        }

    public static int GetSortingLayerWhenCarrying(Character t)
    {
        int SL;
        GameObject GO = WorldController.Instance.charController.GetCharacterGOFromMap(t);
        if (t.facingDirection==0|| t.facingDirection == 1||t.facingDirection == 7)
        {            
            SL=GO.transform.GetChild(0).GetChild(0).transform.GetComponent<SpriteRenderer>().sortingOrder-1;
            return SL;
        }
        SL = GO.transform.GetChild(0).GetChild(0).transform.GetComponent<SpriteRenderer>().sortingOrder;
        //Debug.Log("SR for"+ GO.transform.GetChild(0).GetChild(0).name + ":" + SL );
        SL += 1;
        //Debug.Log(GO.transform.GetChild(0).GetChild(0).name);
        //Debug.Log("new SR for res:" + SL );
        return SL;
    }

    

    public void TransferResources(Resource r1, Resource r2, int amountTaken, GameObject charGO)
    {
        //Resource r2 = new Resource();
        if (r1.resourceType != r2.resourceType && r2.amount != 0) 
        {
            Debug.LogWarning("Ciągle mam inny surowiec"); 
            return;  
        }
        r2.resourceType = r1.resourceType;
        //Debug.Log("r1.type" + r1.resourceType + "  ,r2.type" + r2.resourceType);
        r2.amount += amountTaken;
        //r2.resourceGO
        r2.parent = charGO;
        r1.amount -= amountTaken;
        //Debug.Log("parent name:" + r2.parent.name);
        UnityEngine.Object.Destroy(r2.resourceGO);
        r2.resourceGO = CreateResourceGO(r2, r2.parent);
        //Debug.Log("r1.type" + r1.resourceType + "  ,r2.type" + r2.resourceType);
        return;
    }

       

    public static StockpileSlot FindResourceInStockpile(ResourceType rt, int amount)
    { 
        foreach(KeyValuePair<StockpileSlot,Resource> SSR in WorldController.StockpileSlotResourceMap)
        {
            if (SSR.Key.resource.resourceType == rt)
            {
                if (SSR.Key.resource.amount- SSR.Key.reservedOut >= amount)
                {
                    return SSR.Key;
                }
            }
        }
        return null;
    }




    public void RegisterOnChangedCallback(Action<Resource> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnChanged += callbackFunc;

    }
    public void UnregisterOnChangedCallback(Action<Resource> callbackFunc)
    {
        //Debug.Log("usunoł callback");
        cbOnChanged -= callbackFunc;
    }

    public bool ReserveSlotForAmount(int amount)
    {
        //FindStockpileSlot(Resource r)


        return false;
    }




}
