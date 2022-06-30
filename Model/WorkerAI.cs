using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorkerAI : MonoBehaviour
{

    public enum State
    {
        WaitingForNextTask,
        ExecutingTask,
    }

    private Character worker;
    private TaskSystem taskSystem;
    [SerializeField]private State state;

    [SerializeField]private float waitingTimer;
    [SerializeField] Character.State workerState;
    [SerializeField] public float animationTimer;
    [SerializeField] public string nextTileDebug ="dupa";

    public void Setup(Character worker, TaskSystem newTaskSystem)
    {
        this.worker = worker;
        taskSystem = newTaskSystem;
        state = State.WaitingForNextTask;
    }


    public void Update()
    {
        if (worker.nextTile != null) { 
        nextTileDebug = worker.nextTile.X.ToString();
        nextTileDebug += ":";
        nextTileDebug += worker.nextTile.Y.ToString();
    }
        switch (state)
        {
            case State.WaitingForNextTask:
                
                waitingTimer -= Time.deltaTime;
                //Debug.Log("AI:" + waitingTimer);
                if (waitingTimer <= 0)
                {
                    float waitingTimerMax = 0.2f; //500ms
                    waitingTimer = waitingTimerMax;
                    //Debug.Log("request new task");
                    RequestNextTask();
                }
                break;

            case State.ExecutingTask:
                {

                }
                break;

        }
    }

    private void RequestNextTask()
    {
        if (taskSystem == null)
        {
            Debug.Log("Brak task Systemu");
            return;
        }

        TaskSystem.Task task = taskSystem.RequestNextTask();
        if (task == null)
        {
            state = State.ExecutingTask;
            ExecuteTask_Roam(null);
 
            //state = State.WaitingForNextTask;
            //Debug.Log("Brak nowego TASKA");
        }
        else
        {
            worker.currentTask = task;
            state = State.ExecutingTask;
            if (task is TaskSystem.Task.MoveToPosition)
            {
                ExecuteTask_MoveToPosition(task as TaskSystem.Task.MoveToPosition);
                return;
            }
            if (task is TaskSystem.Task.Victory)
            {
                ExecuteTask_Victory(task as TaskSystem.Task.Victory);
                return;
            }
            if (task is TaskSystem.Task.Build)
            {
                ExecuteTask_Build(task as TaskSystem.Task.Build);
                return;
            }
            if (task is TaskSystem.Task.Harvest)
            {
                ExecuteTask_Harvest(task as TaskSystem.Task.Harvest);
                return;
            }
            if (task is TaskSystem.Task.Haul)
            {
                ExecuteTask_Haul(task as TaskSystem.Task.Haul);
                return;
            }

            if (task is TaskSystem.Task.Supply)
            {
                ExecuteTask_Supply(task as TaskSystem.Task.Supply);
                return;
            }

            Debug.LogError("Task type unknown!");
        }


    }

    private void ExecuteTask_Roam(Action onRoamEnd)
    {
        //Debug.Log("Starting roaming");
        Tile targetTile = WorldController.Instance.World.GetRoamEmptyTile(worker.currentTile);
        Vector2 pos = new Vector2(targetTile.X, targetTile.Y);
        //Debug.Log("Start roaming");
        worker.RoamTo(pos, () =>
        {
            //Debug.Log("Ending roaming");
            state = State.WaitingForNextTask;
            worker.ChangeStateToIdle(worker);

            if (onRoamEnd != null)
            {
                onRoamEnd();
                
            }
        });




    }

    private void ExecuteTask_MoveToPosition(TaskSystem.Task.MoveToPosition task)
    {
        //Debug.Log("Executing task MoveToPosition");
         worker.MoveTo(task.targetPosition, () =>
         {
             state = State.WaitingForNextTask;
         });
    }

    private void ExecuteTask_Victory(TaskSystem.Task.Victory task)
    {
        worker.PlayAnim(() => 
        { 
            state = State.WaitingForNextTask;
        });
    }
         
    private void ExecuteTask_Build(TaskSystem.Task.Build task)
    {        
                worker.MoveTo(task.targetPosition, () =>
                {
                    worker.PlayAnim(() =>
                    {
                        task.buildAction();
                        worker.nextTile = WorldController.Instance.World.GetNearestEmptyTile(worker.currentTile);
                        worker.destination = new Vector2(worker.nextTile.X, worker.nextTile.Y);
                        //worker.state = Character.State.Moving;
                        worker.MoveTo(worker.destination, () =>
                        {
                            state = State.WaitingForNextTask;
                            //Debug.Log("KONIEC BUDOWANIA");
                        });
                    });
                });
    }

    private void ExecuteTask_Harvest(TaskSystem.Task.Harvest task)
    {
        worker.MoveTo(task.targetPosition+new Vector2(0.25f,0), () =>
        {
            worker.facingDirection = 5;
            worker.PlayAnim(() =>
            {
                task.harvestAction();
                


                state = State.WaitingForNextTask;
                worker.ChangeStateToIdle(worker);
                //Debug.Log("KONIEC harvestu");
            });
        });
    }


    public void ExecuteTask_Haul(TaskSystem.Task.Haul task)
    {
        task.resource.ReserveSlotForAmount(task.resource.amount);
        worker.MoveTo(task.targetPosition, () =>
        {
            worker.PlayAnim(() =>
            {
                worker.PickupAction(worker,task.resource,task.resource.amount);
                worker.MoveTo(task.targetPosition2, () =>
                {
                    worker.PlayAnim(() =>
                    {
                    worker.DropOffAction(worker.resource,task.dropOffSlot);
                        state = State.WaitingForNextTask;
                            //Debug.Log("KONIEC HAULU");                        
                    });

                });
            });
        });

    }


    public void ExecuteTask_Supply(TaskSystem.Task.Supply task)
    {
        //no correct resourtce in slot
        Resource r = task.pickUpSlot.resource;
        Vector2 pickUP = new Vector2(task.pickUpSlot.tilePosition.X, task.pickUpSlot.tilePosition.Y);
        worker.MoveTo(pickUP, () =>
        {
            worker.PlayAnim(() =>
            {
                worker.PickupAction(worker, task.pickUpSlot.resource, task.resourceAmount);
                //Debug.Log("dropoff2 x:" + task.dropOffSlot.tilePosition.X);
                Vector2 dropOFF = new Vector2(task.dropOffSlot.tilePosition.X, task.dropOffSlot.tilePosition.Y);
                worker.MoveTo(dropOFF, () =>
                {
                    worker.PlayAnim(() =>
                    {
                        worker.DropOffAction(worker.resource, task.dropOffSlot);   
                        state = State.WaitingForNextTask;
                        //Debug.Log("KONIEC HAULU");
                    });
                });
            });
        });

    }




}
