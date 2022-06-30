using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class seaDepth : MonoBehaviour
{
    public Material mat;

    AspectRatioFitter ARF;
    Camera camera;
    float variation;


    // Start is called before the first frame update
    void Start()
    {
        //Material mat = FindObjectOfType<Material>("");
        Material mat = this.GetComponent<SpriteRenderer>().material;
        if (mat.name == "WaterMat") {
           // mat.SetFloat("_DepthMultiplier", distFromCenter());

        }

       /* RT = this.GetComponent<RectTransform>();
        ARF = this.GetComponent<AspectRatioFitter>();
        ARF.aspectRatio = mat.GetTexture("_Clouds").width/ mat.GetTexture("_Clouds").height;
        camera = Camera.main;
        variation = UnityEngine.Random.Range(0, 9)/100+1;
        off = variation;*/


    }

    // Update is called once per frame
    void Update()
    {
       /* //Set movement speed translated to text offset
        off -= Time.deltaTime/100*Clock.Speed/2*variation;
        mat.SetFloat("_MatOffset", off);

        //Calculate moving texture aspect ratio and asign it
        ARF.aspectRatio = mat.GetTexture("_Clouds").width / mat.GetTexture("_Clouds").height;


        //calculate real texture width wchich determines tiling for mask
        float a =  mat.GetTexture("_Clouds").width / mat.GetTexture("_Clouds").height * camera.pixelHeight;
        float b = camera.pixelWidth;
        float tiling = a/b;
        mat.SetFloat("_MaskTileX", tiling);

        //Set visibility of clouds depending on camera zoom
        mat.SetFloat("_CloudsVisibility", camera.orthographicSize);*/
 

    }
    /*private float distFromCenter()
    {
        GameObject parentGO = this.transform.gameObject;
        foreach (KeyValuePair<Tile, GameObject> pair in WorldController.tileGameObjectMap)
        { 
        pair.Value
        }
        WorldController.tileGameObjectMap
    }*/
}
