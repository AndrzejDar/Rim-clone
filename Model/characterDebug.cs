using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterDebug : MonoBehaviour
{
    [SerializeField] private Tile prevTile;
    [SerializeField] Tile currentTile;
    [SerializeField] Tile nextTile;

    public Character t;

    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        prevTile = t.previousTile;
        currentTile = t.currentTile;
        nextTile = t.nextTile;
    }
}
