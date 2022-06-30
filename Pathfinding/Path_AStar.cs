using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;

public class Path_AStar
{
    Queue<Tile> path;
    public Path_AStar(World world, Tile tileStart, Tile tileEnd)
    {
        // chec to see if tile graph is ok
        if (world.tileGraph == null)
        {
            world.tileGraph = new Path_TileGraph(world);
        }




        //A dictionary of all walkable nodes
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;


        if (nodes.ContainsKey(tileEnd) == false)
        {
            Debug.Log("path AStar, ending tile isnt in the lis of corect tileslabla");
            return;
        }


        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.Log("path AStar, starting tile isnt in the lis of blabla");//zrobić unstuck z tego
            return;
        }




        Path_Node<Tile> start = nodes[tileStart];
        //what if goal is no walkable?
        Path_Node<Tile> goal = nodes[tileEnd];
        
        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        /* List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
         OpenSet.Add(start);*/


        SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();

        foreach(Path_Node<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }
        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();

        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }
        f_score[start] = heuristic_cost_estimate(start,goal);

        while (OpenSet.Count > 0)
        {
            Path_Node<Tile> current = OpenSet.Dequeue();

            if(current==goal)
            {
                reconstruct_path(Came_From, current);
                return;  //REACHED GOAL!!!!!

            }
            ClosedSet.Add(current);
            foreach(Path_Edge<Tile> edge_neighbor in current.edges)
            {
                Path_Node<Tile> neighbor = edge_neighbor.node;
                if (ClosedSet.Contains(neighbor) == true)
                    continue;
                float tentative_g_score = (g_score[current] + dist_between(current, neighbor))+(neighbor.mCost)*1f;//movement cost to neighbour

                if (OpenSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                    continue;

                Came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + heuristic_cost_estimate(neighbor, goal);

                if (World.setPathfindingDebug==true)
                {
                    CreatePathTextF(WorldController.tileGameObjectMap[neighbor.data].transform, f_score[neighbor].ToString(), new Vector3(neighbor.data.X, neighbor.data.Y, 0));
                    //CreatePathTextG(WorldController.tileGameObjectMap[neighbor.data].transform, g_score[neighbor].ToString(), new Vector3(neighbor.data.X, neighbor.data.Y, 0));
                    //CreatePathTextMC(WorldController.tileGameObjectMap[neighbor.data].transform, (neighbor.mCost*10f).ToString(), new Vector3(neighbor.data.X, neighbor.data.Y, 0));
                }

                if (OpenSet.Contains(neighbor)==false)
                {
                    OpenSet.Enqueue(neighbor, f_score[neighbor]);
                }

            }
        }
        Debug.Log("no path to goal");

    }

    float heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2));

    }



    float dist_between (Path_Node<Tile> a, Path_Node<Tile>b)
    {
        if(Mathf.Abs(a.data.X-b.data.X)+Mathf.Abs(a.data.Y-b.data.Y)==1)
            return 1f;

        if(Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y)==1)
        return 1.41f;

        return Mathf.Sqrt(
    Mathf.Pow(a.data.X - b.data.X, 2) +
    Mathf.Pow(a.data.Y - b.data.Y, 2));

    }

    void reconstruct_path(
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From,
        Path_Node<Tile> current
        )
    {
        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data);

        while (Came_From.ContainsKey(current))
        {
            current = Came_From[current];
            total_path.Enqueue(current.data);


            //debuging
            GameObject GO = WorldController.tileGameObjectMap[current.data];
            //GO.transform.GetChild(1).GetComponent<TextMesh>().color = Color.red;
            if(GO.GetComponentInChildren<TextMesh>()!=null)
            GO.GetComponentInChildren<TextMesh>().color = Color.red;
        }

        path=new Queue<Tile>( total_path.Reverse());
    }

    public Tile GetNextTile()
    {
        return path.Dequeue();
    }

    public int Length()
    {
        if (path == null)
            return 0;
        return path.Count;
    }


    public static TextMesh CreatePathTextF(Transform parent, string text, Vector3 localPosition/*, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder*/)
    {

        if (parent.GetComponentInChildren<TextMesh>() == null /*parent.childCount == 0*/)
        {

            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(-0.3f, -0.1f)/*localPosition*/;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.LowerLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.text = text;
            textMesh.characterSize = .01f;
            textMesh.fontSize = 128;
            textMesh.color = Color.white;
            textMesh.GetComponent<MeshRenderer>().sortingLayerName = parent.GetComponentInParent<SpriteRenderer>().sortingLayerName;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = parent.GetComponentInParent<SpriteRenderer>().sortingOrder + 4;
            return textMesh;
        }
        else
        {
            TextMesh textMesh = parent.GetComponentInChildren<TextMesh>();//
            textMesh.text = text;
            return textMesh;
        }

    }
    public static TextMesh CreatePathTextG(Transform parent, string text, Vector3 localPosition/*, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder*/)
    {
        if (parent.childCount == 1)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(-0.3f, 0.1f)/*localPosition*/;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.LowerLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.text = text;
            textMesh.characterSize = .005f;
            textMesh.fontSize = 128;
            textMesh.color = Color.white;
            textMesh.GetComponent<MeshRenderer>().sortingLayerName = parent.GetComponentInParent<SpriteRenderer>().sortingLayerName;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = parent.GetComponentInParent<SpriteRenderer>().sortingOrder + 4;
            return textMesh;
        }
        else
        {
            TextMesh textMesh = parent.GetChild(1).GetComponent<TextMesh>();

            textMesh.text = text;
            return textMesh;
        }
    }

    public static TextMesh CreatePathTextMC(Transform parent, string text, Vector3 localPosition/*, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder*/)
    {
        if (parent.childCount == 2)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(0.0f, 0.1f)/*localPosition*/;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.LowerLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.text = text;
            textMesh.characterSize = .005f;
            textMesh.fontSize = 128;
            textMesh.color = Color.white;
            textMesh.GetComponent<MeshRenderer>().sortingLayerName = parent.GetComponentInParent<SpriteRenderer>().sortingLayerName;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = parent.GetComponentInParent<SpriteRenderer>().sortingOrder + 4;
            return textMesh;
        }

        else
        {
            TextMesh textMesh = parent.GetChild(2).GetComponent<TextMesh>();

            textMesh.text = text;
            return textMesh;
        }
    }
}
