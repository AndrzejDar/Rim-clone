using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGen
{
    int wX;
    int wY;
    World world;
    int seaMulti;


 public void populateWorld(World world)
    {
        this.world = world;
        wX = world.Width;
         wY = world.Height;
        seaMulti = 15;

        createSea(seaMulti);//10-30%
        //createBeach(); - created inside sea
        createRuins();
        createFoliage(2);
        createGroundVariation();
    }

    //poprawić!!!!! wypytuje o nie istniejace tilesy
    void createSea( int amount)
    {
        //amount = amount;
        Tile.TileType type = Tile.TileType.Water;
        for (int j = wY; j > 0; j--)
        {
            for (int i = 0; i < wX; i++)
            {
                if (j == wY)
                {
                    Tile changeTile = world.GetTileAt(i, j);
                    changeTile.Type = type;
                }
                else
                {
                    int random = UnityEngine.Random.Range(1, 100);
                    if (random < j-50+amount)
                    {
                        if ((world.GetTileAt(i - 1, j + 1).Type == Tile.TileType.Water) ||
                                (world.GetTileAt(i, j + 1).Type == Tile.TileType.Water) ||
                                (world.GetTileAt(i - 1, j + 1).Type == Tile.TileType.Water))
                        {
                            Tile changeTile = world.GetTileAt(i, j);
                            changeTile.Type = type;
                            changeTile = world.GetTileAt(i-1, j);
                            changeTile.Type = type;
                            changeTile = world.GetTileAt(i+1, j);
                            changeTile.Type = type;
                            changeTile = world.GetTileAt(i, j+1);
                            changeTile.Type = type;
                            changeTile = world.GetTileAt(i+1, j+1);
                            changeTile.Type = type;
                            changeTile = world.GetTileAt(i-1, j+1);
                            changeTile.Type = type;
                        }
                    }
                }
            }
        }
        for (int j = 0; j < wY; j++)
        {
            for (int i = 0; i < wX; i++)
            {
                if (world.GetTileAt(i, j).Type == Tile.TileType.Water)
                {
                    Tile changeTile = world.GetTileAt(i-1, j+1);
                    changeTile.Type = type;
                    changeTile = world.GetTileAt(i+1, j+1);
                    changeTile.Type = type;
                    createBeach(i, j);

                }
            }
        }
        int x;
        int y;
    }



    void createBeach(int x, int y)
    {
        Tile.TileType type = Tile.TileType.Beach;
        int random = UnityEngine.Random.Range(3, 8);
        if (world.GetTileAt(x, y - 1).Type != Tile.TileType.Water)
        {
            for (int i = random; i > 0; i--)
            {
                Tile changeTile = world.GetTileAt(x, y - i);
                changeTile.Type = type;
            }
        }
        // wyłagadzanie
        for (int j = 0; j < 6; j++)
        {
            for ( int i = -2; i < 3; i++)
            {            
                int random2 = UnityEngine.Random.Range(1, 2);
                if (Mathf.Abs(i) == 1 || i == 0 || (Mathf.Abs(i) == 2 && random2 == 1 && j>0))
                {
                    if (world.GetTileAt(x + i, y -random + j).Type!=Tile.TileType.Water)
                    {
                        Tile changeTile = world.GetTileAt(x + i, y - random + j);
                        changeTile.Type = type;
                    }
                }
            }            
        }
    }

    void createRuins()
    {}

    void createFoliage(float density)
    {
        string buildModeObjectType = "Foliage01";
        float d = world.Width * world.Height * (density/4) / 100;
        for (int i = 0; i < d; i++)
        {
            int x = UnityEngine.Random.Range(0, wX);
            int y = UnityEngine.Random.Range(0, wY);
            Tile t = world.GetTileAt(x, y);
            if (t.Type == Tile.TileType.Grass)
            {
                //Debug.Log("Ładuje drzewo");
                world.PlaceInstalledObject(buildModeObjectType, t);
                int rnd = 10;
                createTilePatch(t, rnd, Tile.TileType.HighGrass);
            }
        }
        createFoliage2(15);
    }

    void createTilePatch(Tile tile, int radius, Tile.TileType type)
    {
        int rnd;
        Tile N, S, E, W;
        int x = tile.X;
        int y = tile.Y;
        int rnd1 = UnityEngine.Random.Range(2, radius);
        N = world.GetTileAt(x,y-rnd1);
        int rnd2 = UnityEngine.Random.Range(2, radius);
        S = world.GetTileAt(x, y + rnd2);
        int rnd3 = UnityEngine.Random.Range(2, radius);
        E = world.GetTileAt(x +rnd3, y);
        int rnd4 = UnityEngine.Random.Range(2, radius);
        W = world.GetTileAt(x- rnd4, y );

        if(tile.Type==Tile.TileType.Grass)tile.Type = type;
        if (N.Type == Tile.TileType.Grass) N.Type = type;
        if (S.Type == Tile.TileType.Grass) S.Type = type;
        if (E.Type == Tile.TileType.Grass) E.Type = type;
        if (W.Type == Tile.TileType.Grass) W.Type = type;
        //Debug.Log("nowy typ podłoża" + type);

        for (int i = 0; i <= rnd1; i++) //Y loop
        {
            for (int j = 0; j <= rnd1/2; j++)
            {
                int rnd5 = UnityEngine.Random.Range(0, 3);
                int rnd6 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd5 == 0 && j >= i - 1) || (j<=1))
                { 
                    
                    Tile t = world.GetTileAt(N.X + j, N.Y + i);
                    if(t.Type==Tile.TileType.Grass)
                    t.Type = type;
                }
                else break;
            }
            for (int j = 0; j <= rnd1 / 2; j++)
            {
                int rnd5 = UnityEngine.Random.Range(0, 3);
                int rnd6 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd6 == 0 && j >= i - 1) || (j <= 1))
                {

                    Tile t = world.GetTileAt(N.X - j, N.Y + i);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
        }

        for (int i = 0; i <= rnd2; i++) //Y loop
        {
            for (int j = 0; j <= rnd2 / 2; j++)
            {
                int rnd5 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd5 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(S.X + j, S.Y - i);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
            for (int j = 0; j <= rnd2 / 2; j++)
            {
                int rnd6 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd6 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(S.X - j, S.Y - i);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
        }


        for (int i = 0; i <= rnd3; i++) //X loop
        {
            for (int j = 0; j <= rnd3 / 2; j++)
            {
                int rnd5 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd5 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(E.X + -i, E.Y +j);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
            for (int j = 0; j <= rnd3 / 2; j++)
            {
                int rnd6 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd6 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(E.X - i, E.Y -j );
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
        }


        for (int i = 0; i <= rnd4; i++) //X loop
        {
            for (int j = 0; j <= rnd4 / 2; j++)
            {
                int rnd5 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd5 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(W.X + i, W.Y + j);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
            for (int j = 0; j <= rnd4 / 2; j++)
            {
                int rnd6 = UnityEngine.Random.Range(0, 3);
                if ((i > 0 && j < i - 1) || (i > 0 && rnd6 == 0 && j >= i - 1) || j <= 1)
                {
                    Tile t = world.GetTileAt(W.X + i, W.Y - j);
                    if (t.Type == Tile.TileType.Grass)
                        t.Type = type;
                }
                else break;
            }
        }
    }


    void createFoliage2(float density)
    {
        int offsetFromWater = Mathf.FloorToInt (wY * 0.1f); //offset od lini brzegowej

        for (int j = wY; j > 0; j--) {
            float densityMultiplayer = ((wY - j) - (wY * seaMulti * 2.5f / 100)) / (wY - wY * seaMulti * 2.5f / 100)*2;
            if (densityMultiplayer < 0) densityMultiplayer = 0;
            //Debug.Log(densityMultiplayer);
            for (int i = 0; i < wX; i++)
            {            
                Tile t = world.GetTileAt(i, j);
                if (t.Type == Tile.TileType.HighGrass)
                {
                    if (UnityEngine.Random.Range(0, 100) <= density/4 +density*densityMultiplayer)
                    {
                    Tile t1 = world.GetTileAt(i, j + offsetFromWater);
                    if (t1.Type != Tile.TileType.Water && t1.Type != Tile.TileType.Beach)
                        {
                            string buildModeObjectType = "Foliage02";
                            if(UnityEngine.Random.Range(0,3)==0) buildModeObjectType = "Foliage01";
                            world.PlaceInstalledObject(buildModeObjectType, t);
                            int rnd = 3;
                            createTilePatch(t, rnd, Tile.TileType.HighGrass);
                        }
                    }
                }
            }
        }    
    }


    void createGroundVariation()
    { }



}
