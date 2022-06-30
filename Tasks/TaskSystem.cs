using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TaskSystem
{

    public class QuedTask
    {
        private Func<Task> tryGetTaskFunc;

        public QuedTask(Func<Task> tryGetTaskFunc)
        {
            this.tryGetTaskFunc = tryGetTaskFunc;
        }

        public Task TryDequeTask()
        {
            return tryGetTaskFunc();
        }
    }

    public abstract class Task
    {

        public class Roam : Task
        {            
        }

        public class MoveToPosition : Task
        {
            public Vector2 targetPosition;
        }

        public class Victory : Task
        {
        }

        public class Build : Task
        {
            public Vector2 targetPosition;
            public Action buildAction;
        }

        public class Harvest : Task
        {
            public Vector2 targetPosition;
            public Action harvestAction;
            public Vector2 targetPosition2;
        }

        public class Haul : Task
        {
            public Vector2 targetPosition;
            /*public Action pickupAction(Tourist t);*/
            public Vector2 targetPosition2;
            public StockpileSlot dropOffSlot;
            public Resource resource;
            public Action dropOffAction;
        }

        public class Supply : Task
        {
            public StockpileSlot dropOffSlot;
            public StockpileSlot pickUpSlot;
            public Resource.ResourceType resourceType;
            public int resourceAmount;
        }
    }


    private List<Task> taskList;
    private List<QuedTask> quedTaskList;

    public TaskSystem()
    {
            taskList = new List<Task>();
        quedTaskList = new List<QuedTask>();
    }

    public Task RequestNextTask()
    {
        DequeTask();
        if(taskList.Count>0)
        {
            Task task = taskList[0];
            taskList.RemoveAt(0);
            return task;
        }
        else
        {
            return null;
        }
    }

    public void AddTask(Task task)
    {
            taskList.Add(task);
    }

    public void EnqueTask(QuedTask quedTask)
    {
        quedTaskList.Add(quedTask);
    }

    public void EnqueTask(Func<Task> tryGetFunc)
    {
        QuedTask quedTask = new QuedTask(tryGetFunc);
        quedTaskList.Add(quedTask);
    }

    private void DequeTask()
    {
        //Debug.Log("Trying to")
        for (int i = 0; i < quedTaskList.Count; i++)
        {
            QuedTask quedTask = quedTaskList[i];
            Task task = quedTask.TryDequeTask(); //Debug.Log("Trying to deque" + quedTaskList.Count);
            if (task != null)
            {
                AddTask(task);
                quedTaskList.RemoveAt(i);
                //Debug.Log("Task dequed");
                i--;
            }
            else { }
        }
    }


    public Vector2 NewRandomDestinationTS()
    {
        int dx = UnityEngine.Random.Range(0, WorldController.Instance.World.Width);
        int dy = UnityEngine.Random.Range(0, WorldController.Instance.World.Height);
        Vector2 destination = new Vector2(dx, dy);
        //Tile destinationTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.y));
        return destination;
    }
}







