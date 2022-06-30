using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TropicsUtils;

public class TouristAI : MonoBehaviour
{

    public enum State
    {
        WaitingForNextActivity,
        ExecutingActivity,
    }
    public enum Activity
    {
        Roaming,
        Talking,
        Sleeping,
        Eating,
        Drinking,
        Swiming,
        Sunbathing,
        PlayingVoleyball,
        Diving,
        Waiting,
        Moving,
        Entering

    }

    Action<Activity> cbActivityChanged;

    public Character tourist;
    private TaskSystem taskSystem;
    private CharacterStatistics characterStatistics;
    public InstalledObject targetInstalledObject;
    private bool wasSearching;
    [SerializeField]private State state;
    [SerializeField] private Character.State charState;
    Activity activity = TouristAI.Activity.Waiting;
    [SerializeField] public Activity publicActivity{ 
        get{return activity;} 
        set{
            activity = value;
            if (cbActivityChanged != null)
            {
                //Debug.Log("Zmieniam activity na"+ value);
                String str = this.activity.ToString();
                TropicsUtilsClass.CreateWorldTextPopup(str, this.tourist.position);
                cbActivityChanged(activity);
                //Debug.Log("końcez CALLBACK");
            }
        }
    }

    [SerializeField]private float waitingTimer;
    [SerializeField] private float waitingTimerCharacteru;
    //[SerializeField] Character.State workerState;
    [SerializeField] public float animationTimer;
    //[SerializeField] public string nextTileDebug ="dupa";



    [SerializeField] string DLastActivity;
    [SerializeField] State DTaskState;
    [SerializeField] Character.State DState;
    [SerializeField] Activity DActivity;
    [SerializeField] Vector2 DNextT;
    [SerializeField] Vector2 DCurrentT;
    [SerializeField] Vector2 DDestinationT;
    [SerializeField] int DFacingDirection;
    [SerializeField] Vector2 DPosition;
    [SerializeField] Vector2 DDestination;




    public void Setup(Character tourist, TaskSystem TaskSystem, CharacterStatistics characterStatistics)
    {
        this.tourist = tourist;
        taskSystem = TaskSystem;
        state = State.WaitingForNextActivity;
        this.characterStatistics = characterStatistics;
        
    }


    public void Update()
    {
        DebugStats();


        charState = tourist.state;
        waitingTimerCharacteru = tourist.waitingTimer;
        if (tourist.state == Character.State.Idle) //zakończył cokolwiek robił wcześniej
        {
            switch (state)
            {
                case State.WaitingForNextActivity:

                    waitingTimer -= Time.deltaTime;
                    if (waitingTimer <= 0)
                    {
                        float waitingTimerMax = 0.5f; //500ms
                        waitingTimer = waitingTimerMax;
                        //Debug.Log("request new task");
                        RequestNextActivity();
                    }
                    break;

                case State.ExecutingActivity:
                    {

                    }
                    break;

            }
        }
    }

    private void RequestNextActivity()
    {
        state = State.ExecutingActivity;
        if (characterStatistics.accommodation==null) {
            DLastActivity = "Going to Checking IN";
            CheckIn();
            return;
        }

        characterStatistics.Sleepy();
        if (characterStatistics.sleepy)
        {
            DLastActivity = "Going to Sleep";
            if (tourist.naked)// dress up then go sleep
            {
                GoToAndDisappear("Changing", 0.25f,()=> {
                    tourist.naked = !tourist.naked;
                }, () =>
                {
                    GoToAndDisappear(characterStatistics.accommodation, characterStatistics.sleepRequired, () => {
                        publicActivity = Activity.Sleeping;
                    }, () => {
                        characterStatistics.sleepy = false;
                        publicActivity = Activity.Waiting;
                        state = State.WaitingForNextActivity;
                    });
                });
                
                return;
            }
            else
            {
                //go sleep
                //characterStatistics.sleepy = false;
                GoToAndDisappear(characterStatistics.accommodation, characterStatistics.sleepRequired,()=> {
                    publicActivity = Activity.Sleeping;
                }, () => {
                    characterStatistics.sleepy = false;
                    publicActivity = Activity.Waiting;
                    state = State.WaitingForNextActivity;
                    });

                return;
            }
        }

        if (characterStatistics.needToliet && WorldController.Instance.FindClosestObjectOfType("Toilet", tourist.currentTile, false) != null)
        {
            DLastActivity = "Going to Toilet";
            GoToAndDisappear("Toilet", 0.5f,()=> { }, () => { characterStatistics.needToliet = false; state = State.WaitingForNextActivity; });

            return;
        }


        if (characterStatistics.hunger< 30 && WorldController.Instance.FindClosestObjectOfType("Food", tourist.currentTile, false) != null)
        {
            DLastActivity = "Going to FoodStall";
            GoToAndPlayAnim("FoodStall", () => { },() => { characterStatistics.Feed(50); state = State.WaitingForNextActivity; });

            return;
        }


        //HAVE FUN
        //Debug.Log("starting to have fun");



        if (characterStatistics.recreation < 90)
        {
            //sunbathing-------------------------------------------------------------------------------------------------------------


            //check if activity is viable
            if (
                WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, false) != null &&
                Clock.hour >= 8 &&
                Clock.hour <= 18
                )
            {
                    sunbathing2();
                return;
            }
        }   
            
        TropicsUtilsClass.CreateWorldTextPopup("There is nothing to do...", this.transform.position);
            
        
        Debug.LogError("ROAMING! - najpewniej brakuje gdzieś returna");
        DLastActivity = "Going to Roam";
        ExecuteTask_Roam(()=> { state = State.WaitingForNextActivity; });
        return;
    }


    void Sunbathing()
    {
        DLastActivity = "Going to SunBathing";
        //do activity
        state = State.ExecutingActivity;
        if (tourist.naked == false && WorldController.Instance.FindClosestObjectOfType("ChangingRoom", tourist.currentTile, false) != null) //jeśli ubrany i jest przebieralnia
        {
            GoToAndDisappear("ChangingRoom", 0.25f, () => { tourist.naked = !tourist.naked; }, () =>
            {
                if (WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true) != null)
                {
                    InstalledObject IO = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true);


                    GoToAndPlayAnim(IO, () =>
                    {
                        publicActivity = Activity.Sunbathing;
                    }, () =>
                    {
                        publicActivity = Activity.Waiting;
                        state = State.WaitingForNextActivity;
                    });
                }
                else
                {
                    //No emopty sun bed!!!

                    GoToAndPlayAnim("SunBed", () => { }, () =>
                    {
                        TropicsUtilsClass.CreateWorldTextPopup("There is no sunbed for me...", this.transform.position);
                        state = State.WaitingForNextActivity;
                    });
                }
            });

            return;
        }
        else //jeśli nie ma przebieralni


        {
            if (WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true) != null)
            {
                InstalledObject IO = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true);

                GoToAnd(getEntryTilePosition(IO), () =>
                {

                    GoToAndPlayAnim(IO, () =>
                    {
                        publicActivity = Activity.Sunbathing;
                        Debug.LogError("Seting activity to sunbathing from tourist AI");
                    }, () =>
                    {
                        publicActivity = Activity.Waiting;
                        state = State.WaitingForNextActivity;
                    });
                });


            }
            else
            {
                //No emopty sun bed!!!
                GoToAndPlayAnim("SunBed", () => { }, () =>
                {
                    TropicsUtilsClass.CreateWorldTextPopup("There is no sunbed for me...", this.transform.position);
                    state = State.WaitingForNextActivity;
                });
            }
            
        }

       /* {
                 if (WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true) != null)
                 {
                     InstalledObject IO = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true);
                     GoToAndPlayAnim(IO,()=> {
                         publicActivity = Activity.Sunbathing;
                         Debug.LogError("Seting activity to sunbathing from tourist AI");
                     }, () =>
                     {
                         publicActivity = Activity.Waiting;
                         state = State.WaitingForNextActivity;
                     });
                 }
                 else
                 {
                     //No emopty sun bed!!!
                     GoToAndPlayAnim("SunBed", () => { },() =>
                     {
                         TropicsUtilsClass.CreateWorldTextPopup("There is no sunbed for me...", this.transform.position);
                         state = State.WaitingForNextActivity;
                     });
                 }
                 return;
             }*/
        

    }

    //GoToAndDisappear("Toilet", 5f, () => { characterStatistics.needToliet = false; });

    private void GoToAndDisappear(string typeName, float forTime, Action onDissapearAction,Action onEndAction)
    {
        InstalledObject targetIO = WorldController.Instance.FindClosestObjectOfType(typeName, tourist.currentTile, false);
        if (targetIO == null)
        {
            TropicsUtilsClass.CreateWorldTextPopup("Can't find" + typeName, this.transform.position);
            ExecuteTask_Roam(() =>
            {
                tourist.ChangeStateToIdle(tourist);
            });
            return;
        }
        GoToAndDisappear(targetIO, forTime, onDissapearAction,onEndAction);
    }


    private void GoToAndDisappear(InstalledObject targetObject, float forTime,Action onDissapearAction, Action onEndAction)
    {
        InstalledObject targetIO = targetObject;
        //Vector2 targetPosition = new Vector2(targetIO.tile.X-0.6f, targetIO.tile.Y);//shity fix entry position
        Vector2 targetPosition = targetObject.entryPos;
        publicActivity = Activity.Moving;
        GoToAnd(getEntryTilePosition(targetIO), () =>
        {
        if (targetIO != characterStatistics.accommodation && targetIO.Occupied == true)
        {
                if (WorldController.Instance.FindClosestObjectOfType(targetObject.objectType, tourist.currentTile, true) != null)
                {
                    targetIO = WorldController.Instance.FindClosestObjectOfType(targetObject.objectType, tourist.currentTile, true);
                    GoToAnd(getEntryTilePosition(targetIO), () =>{ state = State.WaitingForNextActivity; });
                }
                else
                {
                    DLastActivity = "Going to Roam as escape from overcrovded task";
                    ExecuteTask_Roam(() => { state = State.WaitingForNextActivity; });
                }
        }
        else
        {
                targetIO.Occupied = true;//zajmuje obiekt
                GoToAnd(targetObject.entryPos, () =>
                {
                    //targetIO.Occupied = true; //zajmuje obiekt
                    onDissapearAction();
                    tourist.Disappear(targetIO, forTime, () =>
                    {
                        //Vector2 exitPosition = new Vector2(tourist.nextTile.X, tourist.nextTile.Y);
                        //Debug.Log("exit position is " + tourist.nextTile.X + ":" + tourist.nextTile.Y);
                        Vector2 exitPosition = new Vector2(Mathf.RoundToInt(targetObject.entryPos.x), Mathf.RoundToInt(targetObject.entryPos.y));
                        //Debug.Log("entry position is " + targetObject.entryPos.x + ":" + targetObject.entryPos.y);
                        //Debug.Log("exit position is " + exitPosition.x + ":" + exitPosition.y);
                        targetIO.Occupied = false;
                        //trzeba przejść na środek tila - wyjście z obiektu z ustawieniem kierunku przejścia
                        if (targetIO.rotation == 1) tourist.facingDirection = 3;
                        if (targetIO.rotation == 2) tourist.facingDirection = 5;
                        if (targetIO.rotation == 3) tourist.facingDirection = 7;
                        if (targetIO.rotation == 4) tourist.facingDirection = 1;
                        publicActivity = Activity.Moving;
                        tourist.MoveTo(exitPosition, () =>
                         {
                             publicActivity = Activity.Waiting;
                             //Debug.Log("Zakończyłem Go to And Dissapear!");
                             onEndAction();
                         });
                    });
                });
            }
        }); 
    }


    private void GoToAndPlayAnim(string typeName, Action onPlayAnimationAction, Action onEndAction)
    {
        InstalledObject targetIO = WorldController.Instance.FindClosestObjectOfType(typeName, tourist.currentTile, false);
        if (targetIO == null)
        {
            TropicsUtilsClass.CreateWorldTextPopup("Can't find" + typeName, this.transform.position);
            ExecuteTask_Roam(() =>
            {
                tourist.ChangeStateToIdle(tourist);
            });
            return;
        }
        else
        {
            GoToAndPlayAnim(targetIO, onPlayAnimationAction, onEndAction);
        }
    }


    private void GoToAndPlayAnim(InstalledObject targetObject, Action onPlayAnimationAction, Action onEndAction)
    {

        //InstalledObject targetIO = targetObject;
        Vector2 targetPosition = targetObject.entryPos;

        if (!targetObject.Occupied)//nie zajęty
        {
            //Debug.Log("nie zajęty więc zajmuje");
            targetObject.Occupied = true;
            //Debug.Log(targetObject.entryPos);
            //tourist.MoveTo(targetObject.entryPos, () =>
            //onPlayAnimationAction();
            GoToAnd(targetObject.entryPos, () =>
            {

                Debug.Log("doszedł przed animacją");
                onPlayAnimationAction();


                tourist.PlayAnim(() =>
                    {
                        Debug.Log("zakończył animacje");


                        targetObject.Occupied = false;

                        tourist.ChangeStateToIdle(tourist);
                        onEndAction();
                        return;
                    });


            });
        }


        else
        {
            Debug.Log("obiekt zajęty więc gramy waiting anim");
            tourist.MoveTo(getEntryTilePosition(targetObject), () =>
            {
                publicActivity = Activity.Waiting;
                tourist.PlayAnim(() =>
                {
                    publicActivity = Activity.Waiting;
                    tourist.ChangeStateToIdle(tourist);
                    onEndAction();
                });

            });
        }
    }
    
          
    private void CheckIn()
    {
        InstalledObject targetIO = WorldController.Instance.FindClosestObjectOfType("CheckIn", tourist.currentTile,false);


        if(targetIO==null)//komunikat o braku i wyjście z checkin z ciągłym sprawdzenirm czy jest
        {
            TropicsUtilsClass.CreateWorldTextPopup("Where to check in??", this.transform.position);            
            ExecuteTask_Roam(()=>
            {
                //targetIO = WorldController.Instance.FindClosestObjectOfType("CheckIn", tourist.currentTile,false);
                tourist.ChangeStateToIdle(tourist);
                state = State.WaitingForNextActivity;
            });
            return;
        }
        //Debug.Log("znalazłem recepcję");
        Vector2 targetPosition = new Vector2(targetIO.tile.X, targetIO.tile.Y);
        WorldController.Instance.World.GetTileAt((int)targetPosition.x, (int)targetPosition.y);
        //Debug.Log("ide do recepcji");
        tourist.MoveTo(targetIO.entryPos, () =>
        {
            if (targetIO.Occupied == true)
            {
                //Debug.Log("zajęta recepcja");
                publicActivity = Activity.Waiting;
                tourist.Wait(targetIO, () =>
                {
                    tourist.ChangeStateToIdle(tourist);
                    state = State.WaitingForNextActivity;
                });
            }
            else
            {
                targetIO.Occupied = true; //zajmuje recepcję
                publicActivity = Activity.Talking;
                tourist.PlayAnim(()=> 
                { 
                    targetIO.Occupied = false;
                    //Debug.Log("skończył grać animacje i zwalnia recepcje");
                
                    characterStatistics.accommodation = WorldController.Instance.FindEmptyAccomodation(1, tourist.currentTile);

                    if (characterStatistics.accommodation == null)
                    {
                        TropicsUtilsClass.CreateWorldTextPopup("How there is no room for me?!", this.transform.position);
                        ExecuteTask_Roam(() =>
                        {
                            tourist.ChangeStateToIdle(tourist);
                            state = State.WaitingForNextActivity;
                            return;
                        });
                    }
                    else
                    {

                        //Debug.Log("mam cel");
                        characterStatistics.accommodation.occupant = tourist; //rezerwuje noclego GO
                        TropicsUtilsClass.CreateWorldTextPopup("Finally!!", this.transform.position);


                        //move to house
                        //Vector2 target2 = new Vector2(characterStatistics.accommodation.tile.X - 1, characterStatistics.accommodation.tile.Y);
                        Vector2 target2 = new Vector2(characterStatistics.accommodation.entryPos.x, characterStatistics.accommodation.entryPos.y);
                        //Vector2 target2 = new Vector2(0, 0);

                        publicActivity = Activity.Moving;
                        //Debug.Log("Destination is: " + target2 + " ,dest rotation is: " + characterStatistics.accommodation.rotation);
                        GoToAndDisappear(characterStatistics.accommodation, 0.25f,()=> { },() => {
                            state = State.WaitingForNextActivity;
                        });

                    }
                });

            }
        });
    }
       

    private void ExecuteTask_Roam(Action onRoamEnd)
    {
        //Debug.Log("Start roaming");
        publicActivity = Activity.Roaming;
        Tile targetTile = WorldController.Instance.World.GetRoamEmptyTile(tourist.currentTile);
        Vector2 pos = new Vector2(targetTile.X,targetTile.Y);
        
        tourist.RoamTo(pos, () =>
        {
            //Debug.Log("Ending roaming");
            state = State.WaitingForNextActivity;
            publicActivity = Activity.Waiting;
            tourist.ChangeStateToIdle(tourist);
            if (onRoamEnd != null)
            {
                onRoamEnd();
            }
        });
    }

    private void GoToAnd(Tile t, Action onGoToEnd)
    {        
        Vector2 pos = new Vector2(t.X, t.Y);
            GoToAnd(pos, onGoToEnd);
    }

    private void GoToAnd(Vector2 v2, Action onGoToEnd)
    {
        //Debug.Log("Start moviing");
        publicActivity = Activity.Moving;
        tourist.MoveTo(v2, () =>
        {
            //publicActivity = Activity.Waiting;
            tourist.ChangeStateToIdle(tourist);
            if (onGoToEnd != null)
            {
                onGoToEnd();
                //return;
            }
        });
    }





    private void ExecuteTask_SunBathing(/*Action onSunBathingEnd*/)
    {
        state = State.ExecutingActivity;
        publicActivity = Activity.Moving;
        tourist.nextTile = null;//zapobiega unstakowi - nieskutecznie
        tourist.previousTile = null;//zapobiega unstakowi - nieskutecznie
        InstalledObject sunBed = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile,true);//loking for free object
        sunBed.Occupied = true;
        Vector2 pos = new Vector2(sunBed.tile.X, sunBed.tile.Y);
        if (sunBed.tile != tourist.currentTile) //sprawdzam czy musze dojść
        {
            tourist.MoveTo(pos, () =>
            {
                publicActivity = Activity.Sunbathing;
                //tourist.facingDirection = UnityEngine.Random.Range(0, 7);//obracanie kotleta
                tourist.PlayAnim(() =>
                {
                    state = State.WaitingForNextActivity;
                    
                    tourist.ChangeStateToIdle(tourist);
                    sunBed.Occupied = false;
                    //publicActivity = Activity.Waiting;
                    //onSunBathingEnd();
                });

            });
        }
        else //kontynuje czynność w tym samym miejscu
        {
            publicActivity = Activity.Sunbathing;
            tourist.facingDirection = UnityEngine.Random.Range(0, 7);//obracanie kotleta
            tourist.PlayAnim(() =>
            {
                state = State.WaitingForNextActivity;
                
                tourist.ChangeStateToIdle(tourist);
                sunBed.Occupied = false; 
                publicActivity = Activity.Waiting;
                //onSunBathingEnd();
            });
        }
    }

          

    private void ExecuteTask_MoveToPosition(TaskSystem.Task.MoveToPosition task)
    {
        //Debug.Log("Executing task MoveToPosition");
         tourist.MoveTo(task.targetPosition, () =>
         {
             state = State.WaitingForNextActivity;
         });
    }

    private void ExecuteTask_Victory(TaskSystem.Task.Victory task)
    {
        tourist.PlayAnim(() => 
        { 
            state = State.WaitingForNextActivity;
        });
    }


    private void ExecuteTask_ChangeCloths(Action onChangedCloths)
    {
        //Debug.Log("Starting changing cloths");

        
            state = State.ExecutingActivity;
            InstalledObject targetIO = WorldController.Instance.FindClosestObjectOfType("ChangingRoom", tourist.currentTile, false);

            Vector2 targetPosition = new Vector2(targetIO.tile.X, targetIO.tile.Y);
            WorldController.Instance.World.GetTileAt((int)targetPosition.x, (int)targetPosition.y);
            //Debug.Log("ide do przebieralni");
            tourist.MoveTo(targetPosition, () =>
            {
                if (targetIO.Occupied == true)
                {
                    TropicsUtilsClass.CreateWorldTextPopup("It is taken...", this.transform.position);
                    publicActivity = Activity.Waiting;
                    tourist.Wait(targetIO, () =>
                    {
                        tourist.ChangeStateToIdle(tourist);
                        state = State.WaitingForNextActivity;
                        return;
                    });
                }

                else
                {
                    targetIO.Occupied = true; //zajmuje przebieralnie
                    //publicActivity = Activity.Sleeping;
                    tourist.Sleep(targetIO, 1, () =>
                    {
                        targetIO.Occupied = false;

                        if (tourist.naked)
                            tourist.naked = false;
                        else
                            tourist.naked = true;

                        TropicsUtilsClass.CreateWorldTextPopup("przebrany!!", this.transform.position);
                        onChangedCloths();
                        //ExecuteTask_SunBathing();
                    });
                }

            });
        
    }

    Vector2 getEntryTilePosition(InstalledObject IO)
    {
        if (IO==null||IO.rotation == null) return new Vector2(IO.tile.X, IO.tile.Y);
        else
        {
            //if (IO.rotation != 0)
            //{
            Debug.Log("rotacja jest:" + IO.rotation);
            if (IO.rotation == 1) return new Vector2(IO.tile.X, IO.tile.Y + 1);
            if (IO.rotation == 2) return new Vector2(IO.tile.X - 1, IO.tile.Y);
            if (IO.rotation == 3) return new Vector2(IO.tile.X, IO.tile.Y - 1);
            if (IO.rotation == 4) return new Vector2(IO.tile.X + 1, IO.tile.Y);
            //}
            return new Vector2(IO.tile.X, IO.tile.Y);
        }
    }


    public void DebugStats()
    {
        //LastAction 
       DTaskState = state;
        DState = tourist.state;
        DActivity = activity;
        if (tourist.nextTile != null)
            DNextT = new Vector2(tourist.nextTile.X, tourist.nextTile.Y);
        else DNextT = new Vector2(0, 0);
        if (tourist.nextTile != null)
            if(tourist.currentTile!=null)
            DCurrentT = new Vector2 (tourist.currentTile.X,tourist.currentTile.Y);
            else
            {
                DCurrentT = new Vector2(999, 999);
                Debug.LogWarning("CurrentTile nie istnieje ?!");
            }
        else DCurrentT = new Vector2(0, 0);
        if (tourist.nextTile != null)
            DDestinationT = new Vector2(tourist.destinationTile.X,tourist.destinationTile.Y);
        else DDestinationT = new Vector2(0, 0);
        DPosition = tourist.position;
        DDestination = tourist.destination;
        DFacingDirection = tourist.facingDirection;
}



    public void RegisterActivityChangedCallback(Action<Activity> callback)
    {
        cbActivityChanged += callback;
    }
    public void UnRegisterTileTypeChangedCallback(Action<Activity> callback)
    {
        cbActivityChanged += callback;
    }


    void changeClothes(Action onEndAction)
    {

        DLastActivity = "changing clothes";
        if (WorldController.Instance.FindClosestObjectOfType("ChangingRoom", tourist.currentTile, false) != null) //jeśli jest przebieralnia
        {
            GoToAndDisappear("ChangingRoom", 0.25f, () => { tourist.naked = !tourist.naked; }, () =>
            {
                onEndAction();
            });
        }
    }

   /* bool lookingForObjectOfType(string type, Tile t, bool free)
    {
        if (wasSearching == false)//start searching
        {

            targetInstalledObject = WorldController.Instance.FindClosestObjectOfType(this, "CheckIn", tourist.currentTile, false);
            wasSearching = true;
            return false;
        }
        if (wasSearching == true)
        {
            
            return true;
        }

    }*/



    void sunbathing2()    
    {


            if (WorldController.Instance.FindClosestObjectOfType("ChangingRoom", tourist.currentTile, false) != null && tourist.naked==false)
            {

            changeClothes(() => { state = State.WaitingForNextActivity; });
            DLastActivity = "clothes changed";

            }
            else
            {
                DLastActivity = "sunbathing2";
                InstalledObject IO = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, false);
                GoToAnd(getEntryTilePosition(IO), () =>
                {
                    if (IO.Occupied == false)
                    {
                        GoToAndPlayAnim(IO, () => { this.publicActivity = Activity.Sunbathing; }, () => { state = State.WaitingForNextActivity; });
                        return;
                    }
                    else
                    {
                        IO = WorldController.Instance.FindClosestObjectOfType("SunBed", tourist.currentTile, true);
                        if (IO != null)
                        {

                            GoToAnd(getEntryTilePosition(IO), () =>
                            {
                                Debug.LogWarning("dotarł dio drugiej pozycji i będzie leciał dalej");
                                state = State.WaitingForNextActivity;
                                return;
                            //to do counter of failed repetitions
                        });
                        }
                        else
                        {
                            DLastActivity = "Going to Roam as escape from overcrovded task";
                            ExecuteTask_Roam(() => { state = State.WaitingForNextActivity; });
                        }
                    }
                });
            }

    }





}
