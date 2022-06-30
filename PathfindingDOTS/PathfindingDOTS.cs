
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using CodeMonkey.Utils;
using System.Linq;

public class Pathfinding {
    //Queue<Tile> path2;
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private static NativeArray<int> movementCostArray2;
    private static Dictionary<int, int> movementCostTmpDic;
    private Queue<Tile> pathQueue;
    static World localWorld;
    static float lastTime;
    static int taskCount;
    static int lastFrameTaskCount;




    public static void CreateTempDic()
    {
        if (movementCostArray2.IsCreated)
        {
            movementCostArray2.Dispose(); //disposes array created in previous frame - causes leak on inerupted
        }
        movementCostArray2 = new NativeArray<int>(localWorld.Width * localWorld.Height, Allocator.TempJob);
        for (int i = 0; i < localWorld.Width; i++)
        {
            for (int j = 0; j < localWorld.Height; j++)
            {
                int movementCost = (int)(WorldController.Instance.World.GetTileAt(i, j).movementCost * 10);
                int index = i + j * localWorld.Height;
                movementCostArray2[index] = movementCost;
            }
        }
        lastTime = Time.time;
    }
    

     public static void LateUpdate() {
        //movementCostArray2.Dispose();
    }


    public Pathfinding(World world, Tile tileStart, Tile tileEnd)
    {
        localWorld = world;
        if (lastTime != Time.time)
        {
            CreateTempDic();//update array of movement costs every frame
        }

        NativeList<int2> path = new NativeList<int2>(world.Width+world.Height, Allocator.TempJob);

        //tworze nowe zadanie (jako kopie)
        FindPathJob findPathJob = new FindPathJob 
        {
            startPosition = new int2(tileStart.X, tileStart.Y),
            endPosition = new int2(tileEnd.X, tileEnd.Y),
            gridSize = new int2(world.Width, world.Height),
            movementCostArray = movementCostArray2,
            path = path
        };


    JobHandle jobHandle = findPathJob.Schedule();
        jobHandle.Complete();
        pathQueue = new Queue<Tile>();
        //print zwrotu
        foreach (int2 i in findPathJob.path)
        {
            pathQueue.Enqueue(world.GetTileAt(i.x, i.y));
            //debuging
            if (World.setPathfindingDebug == true)
            {
                CreatePathTextF(WorldController.tileGameObjectMap[world.GetTileAt(i.x, i.y)].transform, "0", new Vector3(i.x, i.y, 0));
            }
        }            
        pathQueue= new Queue<Tile>(pathQueue.Reverse());
        path.Dispose();
    }


public Tile GetNextTile()
    {
        //Debug
        if (World.setPathfindingDebug == true)
        {
            Tile t = pathQueue.First();
            if (WorldController.tileGameObjectMap[t].GetComponentInChildren<TextMesh>() != null && WorldController.tileGameObjectMap[t].GetComponentInChildren<TextMesh>().gameObject != null)
            {
                UnityEngine.Object.Destroy(WorldController.tileGameObjectMap[t].GetComponentInChildren<TextMesh>().gameObject);
            }
        }
    return pathQueue.Dequeue();
    }



    public int Length()
    {
        if (pathQueue == null)
            return 0;
        return pathQueue.Count;
    }



    public static TextMesh CreatePathTextF(Transform parent, string text, Vector3 localPosition/*, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder*/)
    {
        if (parent.GetComponentInChildren<TextMesh>() == null /*parent.childCount == 0*/)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(-0.1f, -0.1f)/*localPosition*/;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.LowerLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.text = text;
            textMesh.characterSize = .02f;
            textMesh.fontSize = 128;
            textMesh.color = Color.red;
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











    [BurstCompile]
    private struct FindPathJob : IJob
    {

        public int2 startPosition;
        public int2 endPosition;
        public int2 gridSize;
        public NativeArray<int> movementCostArray;
        public NativeList<int2> path;


        public void Execute() {
            

            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridSize.x);

                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.mCost = movementCostArray[pathNode.index];
                    pathNode.CalculateFCost();
                    if (pathNode.mCost != 0)
                    {
                        pathNode.isWalkable = true;
                    }
                    else pathNode.isWalkable = false;
                    pathNode.cameFromNodeIndex = -1;

                    pathNodeArray[pathNode.index] = pathNode;
                }
            }


            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
            neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            //Debug.Log("strarting Pathfind");

            openList.Add(startNode.index);

            while (openList.Length > 0) {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex) {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++) {
                    if (openList[i] == currentNodeIndex) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++) {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, gridSize)) {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex)) {
                        // Already searched this node
                        continue;
                    }

                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable) {
                        // Not walkable
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

	                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition)+(20 - neighbourNode.mCost*2);//muliplier

                    


                    if (tentativeGCost < neighbourNode.gCost) {
		                neighbourNode.cameFromNodeIndex = currentNodeIndex;
		                neighbourNode.gCost = tentativeGCost;
		                neighbourNode.CalculateFCost();
		                pathNodeArray[neighbourNodeIndex] = neighbourNode;

		                if (!openList.Contains(neighbourNode.index)) {
			                openList.Add(neighbourNode.index);
		                }
	                }

                }
            }

            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1) {
                // Didn't find a path!
            } else {
                CalculatePath(pathNodeArray, endNode, path);

            }

            pathNodeArray.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();

        }
        
        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, NativeList<int2> path) {
            if (endNode.cameFromNodeIndex == -1) {
                // Couldn't find a path!
                return new NativeList<int2>(Allocator.Temp);
            } else {
                // Found a path
                NativeList<int2> path2 = path;  /* = new NativeList<int2>(Allocator.Temp)*/;
                path2.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1) {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path2.Add(new int2(cameFromNode.x, cameFromNode.y));
                    //Debug.Log(cameFromNode.x + ":" + cameFromNode.y);
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
            return
                gridPosition.x >= 0 && 
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private int CalculateIndex(int x, int y, int gridWidth) {
            return x + y * gridWidth;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition) {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

    
        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++) {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost) {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }

        private struct PathNode {
            public int x;
            public int y;

            public int index;

            public int gCost;
            public int hCost;
            public int fCost;
            public int mCost;

            public bool isWalkable;

            public int cameFromNodeIndex;

            public void CalculateFCost() {
                fCost = gCost + hCost;
            }

            public void SetIsWalkable(bool isWalkable) {
                this.isWalkable = isWalkable;
            }
        }

    }



}



/*

public Pathfinding(World world, Tile tileStart, Tile tileEnd)
{
    localWorld = world;
    //new array to pass movement cost

    NativeArray<int> movementCostArray = new NativeArray<int>(world.Width * world.Height, Allocator.TempJob);
    if (lastTime != Time.deltaTime) CreateTempDic();

    for (int i = 0; i < world.Width; i++)
    {
        for (int j = 0; j < world.Height; j++)
        {
            int movementCost = (int)(world.GetTileAt(i, j).movementCost * 10);
            int index = i + j * world.Height;


            movementCostArray[index] = movementCost;

        }
    }



    NativeList<int2> path = new NativeList<int2>(world.Width + world.Height, Allocator.TempJob);

    //tworze nowe zadanie (jako kopie)
    FindPathJob findPathJob = new FindPathJob
    {
        startPosition = new int2(tileStart.X, tileStart.Y),
        endPosition = new int2(tileEnd.X, tileEnd.Y),
        gridSize = new int2(world.Width, world.Height),
        movementCostArray = movementCostArray,
        path = path
    };

    JobHandle jobHandle = findPathJob.Schedule();
    jobHandle.Complete();

    pathQueue = new Queue<Tile>();
    //print zwrotu
    foreach (int2 i in findPathJob.path)
    {
        pathQueue.Enqueue(world.GetTileAt(i.x, i.y));




        //debuging
        if (World.setPathfindingDebug == true)
        {
            CreatePathTextF(WorldController.tileGameObjectMap[world.GetTileAt(i.x, i.y)].transform, "0", new Vector3(i.x, i.y, 0));
        }
    }
    pathQueue = new Queue<Tile>(pathQueue.Reverse());
    path.Dispose();
    movementCostArray.Dispose();
}*/