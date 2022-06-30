using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph 
{
    //creatce path finding graph of our world, each tile is a node. 
    //nodes are coneccted with egdes walkable

    public Dictionary<Tile, Path_Node<Tile>> nodes;
    public Path_TileGraph(World world)
    {
        nodes = new Dictionary<Tile, Path_Node<Tile>>();
        
        //loop throught all tiles of the world and create node for each tile

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile t = world.GetTileAt(x, y);

                if(t.movementCost>0)  //tiles at wchich we can walk
                {
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    n.mCost = 10 - (t.movementCost*10);
                    nodes.Add(t, n);
                }
            }
        }


// loop throught to create edges

        foreach(Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
            //get list of neightbours 
            Tile[] neighbours = t.GetNeighbours(true); //some spotes may be null
                                                       // and creat edge if walkable
            for (int i = 0; i < neighbours.Length; i++)
            {
                if(neighbours[i]!=null && neighbours[i].movementCost>0)
                {
                    if (i < 4) { 
                    //is oka nd create edge
                        Path_Edge<Tile> e = new Path_Edge<Tile>();
                        e.cost = neighbours[i].movementCost;
                        e.node = nodes[neighbours[i]];
                        //Add edge to tmp list
                        edges.Add(e);
                    }


                    if (i>=4 && i<7 &&(neighbours[i-4].movementCost > 0 && neighbours[i-3].movementCost > 0))//excluding diagonal with neighbour aka walking throught door without cuting corners
                    {
                     //is oka nd create edge
                        Path_Edge<Tile> e = new Path_Edge<Tile>();
                        e.cost = neighbours[i].movementCost;
                        e.node = nodes[neighbours[i]];
                        //Add edge to tmp list
                        edges.Add(e);
                    }

                    if (i == 7 && (neighbours[i - 4].movementCost > 0 && neighbours[i - 7].movementCost > 0))//excluding diagonal with neighbour aka walking throught door without cuting corners
                    {
                        //is oka nd create edge
                        Path_Edge<Tile> e = new Path_Edge<Tile>();
                        e.cost = neighbours[i].movementCost;
                        e.node = nodes[neighbours[i]];
                        //Add edge to tmp list
                        edges.Add(e);
                    }




                }
            }
            n.edges = edges.ToArray();

        }
    }
 
}
