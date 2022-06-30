using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;



public class UIController : MonoBehaviour
{
    public static UIController Instance { get; protected set; }

    World world;
    CharController charController;
    MouseController mouseController;

    public GameObject Canvas;
    public GameObject Top_Panel;
    public GameObject Action_Panel;
    public GameObject Info_Panel;
    public GameObject IPtext;

    Tile tile;

    RectTransform Info_Panel2;


    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        { Debug.LogError("There shoudl be no double world controller"); }
        Canvas = GameObject.Find("Canvas");
        Top_Panel = GameObject.Find("Canvas");
        Action_Panel = GameObject.Find("Canvas");
        Info_Panel = Canvas.gameObject.transform.GetChild(2).gameObject;
        //Debug.Log(Canvas.gameObject.transform.childCount);

    }

    public void UISetup(World world, CharController charController, MouseController mouseController)
    {
        this.world = world;
        this.charController = charController;
        this.mouseController = mouseController;
        mouseController.RegisterCursorMoved(OnCursorMoved);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCursorMoved(Tile tile)
    {
        InfoPanelUpdate(tile);
    }

    void InfoPanelUpdate(Tile tile) 
    {
        this.tile = tile;
        Info_Panel = GameObject.Find("Info_Panel");
        TextMeshProUGUI text = Info_Panel.GetComponentInChildren<TextMeshProUGUI>();//nagłowek  - do poprawki jak niżej
        TextMeshProUGUI text2 = Info_Panel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI text3 = Info_Panel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        if (text2 == null)
        {
            Debug.Log("GÓWNO!!!!!");
        }

        if(tile.installedObject==null)
        text2.text = (
                "Tile "+tile.X+":"+tile.Y+ 
                "\n Ground:" + tile.Type + 
                "\n IO: empty"+
                "\n mCost" + tile.movementCost+
                "\n beauty" + world.beautyMap.GetBeautyAt(tile)
                );
        else
        text2.text = (
                "Tile " + tile.X + ":" + tile.Y + 
                "\n Ground: " + tile.Type + 
                "\n IO: " + tile.installedObject.objectType +
                "\n IO.occupied: " + tile.installedObject.Occupied +
                "\n mCost" + tile.movementCost+
                "\n beauty" + world.beautyMap.GetBeautyAt(tile)
                );
        text3.text = ("Mouse mode:" + mouseController.mode +
            "\n BuildMde=" + mouseController.buildModeIsObjects +
            "\n HarvestMode=" + mouseController.buildModeIsObjects +
            "\n GroundMode=" + mouseController.buildModeIsObjects
            );
    }

    public void setPathfindingDebug(bool value)
    {
        if (value == true) { 
            World.setPathfindingDebug = true; 
            WorldController.Instance.World.InvalidateTileGraph(); 
        }
        if(value==false) 
        { 
            World.setPathfindingDebug = false;
            WorldController.Instance.World.InvalidateTileGraph(); 
        }
        Debug.Log("seting path debug to:"+World.setPathfindingDebug);
    }


    public void setBeautyMap(bool value)
    {
        if (value == true)
        {

            WorldController.Instance.EnableBeautyHeatMap(true);
        }
        if (value == false)
        {
            WorldController.Instance.EnableBeautyHeatMap(false);
        }
        Debug.Log("enabling/disabling Heat MAP");
    }


}
