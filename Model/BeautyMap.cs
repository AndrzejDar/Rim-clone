using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;

public class BeautyMap
{
    BeautyTile[,] beautyTiles;
    private int width;
    private int height;
    private World world;
    private int radius = 3;

    public BeautyMap(World world)
    {
        this.world = world;
        this.width = world.Width;
        this.height = world.Height;
        beautyTiles = createBeautyMap();
        CalculateCombinedBeautyInArea(0,0,width-1,height-1);
    }


    private BeautyTile[,] createBeautyMap()
    {

        BeautyTile[,] beautyTiles = new BeautyTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                beautyTiles[x, y] = new BeautyTile(x, y);
                Tile t = world.GetTileAt(x, y);                
                t.RegisterTileTypeChangedCallback((tile)=>onTileTypeChangedUpdateBeautyLocaly(t));
                                               
                //world.RegisterInstalledObjectCreated((tile)=>onTileTypeChangedUpdateBeautyLocaly(t));
                //world.RegisterInstalledObjectCreated(onTileTypeChangedUpdateBeautyLocaly);
                //world.RegisterInstalledObjectCreated((tile) => CalculateCombinedBeautyInArea(0, 0, width - 1, height - 1));
                beautyTiles[x, y].SetLocalBeauty(CalculateLocalBeauty(t));

            }
        }

        return beautyTiles;
    }


    void onTileTypeChangedUpdateBeautyLocaly(InstalledObject IO)
    {
        onTileTypeChangedUpdateBeautyLocaly(IO.tile);
    }

    [BurstCompile]
    public void onTileTypeChangedUpdateBeautyLocaly(Tile t)
    {
        //Debug.Log("Updateing beauty map for tile"+t.X+":"+t.Y);
        beautyTiles[t.X,t.Y].SetLocalBeauty(CalculateLocalBeauty(t));
        int tX = t.X;
        int tY = t.Y;
        CalculateCombinedBeautyInArea(tX - radius, tY - radius, tX + radius, tY + radius);

    }


    public int GetBeautyAt(Tile t)
    {
        int x=t.X;
        int y=t.Y;
        return beautyTiles[x, y].combinedBeauty;
    }

    int CalculateLocalBeauty(Tile t)
    {
        int beauty = 0;
        switch (t.Type) {
            case Tile.TileType.Path:
                beauty = 0;
                break;
            case Tile.TileType.Grass:
                beauty = 20;
                break;
            case Tile.TileType.HighGrass:
                beauty = 80;
                break;
            case Tile.TileType.Lawn:
                beauty = 50;
                break;
            case Tile.TileType.Water:
                beauty = 100;
                break;
            case Tile.TileType.Beach:
                beauty = 80;
                //Debug.Log("beach");
                break;
            case Tile.TileType.WalkWay:
                beauty = 50;
                break;                                                                    
        }
        if (t.installedObject != null)
        {
            beauty = t.installedObject.beauty;
            //Debug.Log(beauty);
        }
        //Debug.Log(beauty);
        return beauty;
    }


    private void CalculateCombinedBeautyInArea(int startX,  int startY, int endX, int endY)
    {

        for (int x = startX; x < endX+1; x++)
        {
            for (int y = startY; y < endY+1; y++)
            {
                if ((x >= 0 && x < width) && (y >= 0 && y < height))
                {

                    CalculateCombinedBeauty(x, y);
                }  
            }
        }
    }



    void CalculateCombinedBeauty(int x, int y)
    {
        int count = 0;
        int combinedBeauty = 0;
        for (int i = x - radius; i < x + radius+1; i++)
        {
            for (int j = y - radius; j < y + radius+1; j++)
            {

                if ((i >= 0 && i < width) && (j >= 0 && j < height))
                {
                    count++;
                    combinedBeauty += beautyTiles[i, j].localBeauty;
                }
            }
        }
        if (combinedBeauty != 0)
        {
            beautyTiles[x, y].SetCombinedBeauty(combinedBeauty / count);
            if (world.GetTileAt(x, y).installedObject != null)
            {
                beautyTiles[x, y].SetCombinedBeauty(world.GetTileAt(x, y).installedObject.beauty);
            }
        }
    }







}


public class BeautyTile
{
    public int localBeauty{get; protected set; }

    public int combinedBeauty { get; protected set; }

    int x;
    public int X { get { return x; } }
    int y;
    public int Y { get { return y; } }

    public BeautyTile(int x,int y)
    {
        this.x = x;
        this.y = y;
    }


    public void SetLocalBeauty(int beauty)
    {
        localBeauty = beauty;
    }
    public void SetCombinedBeauty(int beauty)
    {
        combinedBeauty = beauty;
    }


}