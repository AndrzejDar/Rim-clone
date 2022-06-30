using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mainClouds : MonoBehaviour
{
    RectTransform RT;
    public Material mat;
    float off = 0;
    AspectRatioFitter ARF;
    Camera camera;
    float variation;
    float maskOffset=0.3f;
    float cloudsDisplayWidth;


    // Start is called before the first frame update
    void Start()
    {
        RT = this.GetComponent<RectTransform>();
        ARF = this.GetComponent<AspectRatioFitter>();
        ARF.aspectRatio = mat.GetTexture("_Clouds").width/ mat.GetTexture("_Clouds").height;
        camera = Camera.main;
        variation = UnityEngine.Random.Range(0, 9)/100+1;
        off = variation;


    }

    // Update is called once per frame
    void Update()
    {
        //Set movement speed translated to text offset
        off -= Time.deltaTime/100*Clock.Speed/2*variation;
        mat.SetFloat("_MatOffset", off);

        //Calculate moving texture aspect ratio and asign it
        ARF.aspectRatio = mat.GetTexture("_Clouds").width / mat.GetTexture("_Clouds").height;


        //calculate clouds Display width wchich determines tiling for mask
        float a =  mat.GetTexture("_Clouds").width / mat.GetTexture("_Clouds").height * camera.pixelHeight; //displayed mask width
        float b = camera.pixelWidth;
        float tiling = a/b;
        mat.SetFloat("_MaskTileX", tiling);

        // and UV offset on X for mask



        maskOffset = ((a - camera.pixelWidth) / 2) / camera.pixelWidth;
        //Debug.Log("clouds width" + mat.GetTexture("_Clouds").width +"     mask width"+ mat.GetTexture("_CloudsMask").width);
        mat.SetFloat("_MaskTileXOffset", -maskOffset);

        //Set visibility of clouds depending on camera zoom
        mat.SetFloat("_CloudsVisibility", camera.orthographicSize);
 

    }
}
