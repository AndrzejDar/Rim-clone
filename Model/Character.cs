using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TropicsUtils;

public class Character
{

    public string characterType { get; protected set; }
    public Vector2 position { get; protected set; }
    public float movementSpeed;
    public float idleSpeedM = 0.4f;
    public Vector2 destination;

    public Tile destinationTile;
    Tile NextTile;
    [SerializeField] public Tile nextTile;
   /* {
        get { return NextTile; }
        set {
            NextTile = value;
            /*if(NextTile!=null&&this.spriteVariation[0] == 0)
            Debug.Log("Next tile is: " + NextTile.X + ":" + NextTile.Y);*/
       /* }

    }*/
    [SerializeField] public Tile previousTile;
    public Vector2 nextTilePosition;
    [SerializeField] public Tile currentTile;
    //public Path_AStar pathAStar;
    public Pathfinding pathAStar;
    private Pathfinding pathfinding;
    public int facingDirection;
    public bool visible = true;
    public float animationTimer;

    CharacterStatistics characterStatistics;

    public Resource resource;



    public float waitingTimer;
    public TaskSystem.Task currentTask;

    private float deltaTime;
    public bool isMoving;
    public bool waitingForNewTask;

    public TouristAI touristAI;

    public bool naked = false;

    public enum State
    {
        Idle, //deafult state
        Roaming,//to move half speed 
        Moving,//to move
        Animating,//when playing special animation
    }

    public enum Gender
    {
        Male,
        Female,
    }
    public Gender gender { get; protected set; }

    public int[] spriteVariation = new int[] { 0, 0, 0 };

    public State state
    {get; protected set;}
    private Action onArrivedAtPosition;
    private Action tmpAction;

    public Action<Character> cbOnChanged
    { get; protected set; }
    public Action<Character> cbOnNextTilleArrival
    { get; protected set; }

    public Action<Character> cbOnPlayAnimation
    { get; protected set;}

    State oldState= State.Idle; //debuging

    public Path_TileGraph tileGraph;

    static public Character CreateCharacterPrototype(string charType, /*Vector2 position, */float movementSpeed = 1f,int worker = 1)
    {
        Character character = new Character();
        character.characterType = charType;
        Debug.Log("added type: " + charType);
        character.movementSpeed = movementSpeed;
        character.spriteVariation[0] = worker;
        return character;
    }

    static public Character PlaceCharacter(Character characterPrototype, Tile tile)
    {
        Character character = new Character();
        character.characterType = characterPrototype.characterType;
        character.movementSpeed = characterPrototype.movementSpeed;
        int x = tile.X;
        int y = tile.Y;
        character.position = new Vector2(x,y);
        character.currentTile = tile;
        character.spriteVariation[0] = characterPrototype.spriteVariation[0];
        return character;
    }


    public void Setup(int[] variation,CharacterStatistics characterStatistics)
    {
        this.characterStatistics = characterStatistics;
        this.characterStatistics.setTouristMultipliers(variation);
        
        //characterStatistics = WorldController.Instance.charController.characterGameObjectMap[this].GetComponent<CharacterStatistics>();
    }



    public void Update(float deltaTime)
    {
        //Debug.Log("char:" + deltaTime);
        //Debug.Log(state);
        if (this.spriteVariation[0] == 0)//tylko turyści mają statystyki
        {
            characterStatistics.manualUpdate(deltaTime);
        }
        if (state != oldState) {
            //Debug.Log("Zmienilł się stejt na: " + state); 
            oldState = state; }
        switch (state)
        {
            case State.Idle:
                break;

            case State.Moving:
                Update_DoMovement(deltaTime);
                break;

            case State.Roaming:
                Update_DoMovement(deltaTime*idleSpeedM);
                break;

            case State.Animating:
                Update_DoAnimation(deltaTime);        
                break;
        }
    }

   public void ChangeStateToIdle(Character character) 
    {
        character.state = Character.State.Idle;

        //Debug.Log("Zmieniam stejt przez funkcje na IDLE");
    }


    void Update_DoAnimation(float deltaTime)
    {
        //Debug.Log(waitingTimer);
        waitingTimer -= deltaTime;
        if (waitingTimer <= 0)
        {
            waitingTimer = 0;
            if (tmpAction == null) 
            { 
                //Debug.Log("TMPAction = null"); 
            }
            else
            {
                tmpAction();
                tmpAction = null;
            }
            //this.state = Character.State.Idle;
            //Debug.Log("zmieniam na stejt Idle po Animating");

        }
    }


    void Update_DoMovement2(float deltaTime)
    {
        if (cbOnChanged != null)
            cbOnChanged(this);


        currentTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
       
        if (nextTile == null)//trzeba wyciągnąć z Astara nowy
        {
            if (pathAStar == null) //nie ma z czego wyciągać potrzebujemy nową ścieżkę
            {
                //Debug.Log("potrzebuje nową ścieżkę!");
                pathAStar = new Pathfinding(currentTile.world, currentTile, destinationTile);                
                if (pathAStar.Length() == 0)
                {
                    Debug.LogError("brak nowej ścieżki / jestem w miejscu docelowym - przechodzę na najbliższą pustą pozycję");
                     
                    
                    
                    if (currentTile == destinationTile) // jestem w miejscu docelowym od początku
                    {
                        if (onArrivedAtPosition != null)
                        {
                            Action tmpAction = onArrivedAtPosition;
                            onArrivedAtPosition = null;
                            nextTile = null;
                            if (currentTile.movementCost != 0)
                            {
                                //Debug.Log("Doszedł na bezpieczny koniec");
                                isMoving = false;
                                nextTile = null;
                            }
                            else //escape after finish from unwalkable tile
                            {
                                nextTile = WorldController.Instance.World.GetNearestEmptyTile(currentTile);
                                if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }
                            }
                            tmpAction();
                            return;
                        }






                    }
                    //Sprawdzić czy punkt początkowy jest walkable!!!!!!!!!!!!!!!!!!!!!!!!
                    //Sukam najbliższej pustej pozycji
                    //nextTile = WorldController.Instance.World.GetNearestEmptyTile(currentTile);
                    if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }
                    pathAStar = null;
                    return; //cancel destination
                }
            }            
            if (pathAStar != null && pathAStar.Length() > 0) //i jest z czego wyciągać
            {
                nextTile = pathAStar.GetNextTile(); //znaczy że dotarł do nowego tilesa i trzeba ogarnąć updejt grafiki
                if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }
                if (nextTile != currentTile)
                    previousTile = currentTile;
            }
            else pathAStar = null;
        }

        

        //---------------------------------
        if (currentTile.movementCost == 0 /*&& previousTile!=currentTile*/) //UNSTUCK po budowie
        {
            nextTile = previousTile;
            if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }
            pathAStar = null;
        }

                
        if (nextTile.movementCost == 0) //UNSTUCK poprawić
        {
            nextTile = WorldController.Instance.World.GetNearestEmptyTile(currentTile);
            if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }
            pathAStar = null;
            Update_DoMovement(deltaTime);
            Debug.LogWarning("current Tile:" + currentTile.X +":"+currentTile.Y+ "current Tile:" + nextTile.X + ":" + nextTile.Y);

            Debug.LogError("STUCK!!");
            return;
        }
        //---------------------------------------------
        // What's the total distance from point A to point B?
        float distToTravel = Mathf.Sqrt(Mathf.Pow(position.x - nextTile.X, 2) + Mathf.Pow(position.y - nextTile.Y, 2));

        // How much distance can be travel this Update?
        float distThisFrame = movementSpeed * deltaTime;
        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;
        if (percThisFrame < 1)
        {
            float xMovement;
            float yMovement;
            if (position.x > nextTile.X)
            {
                xMovement = percThisFrame * -(position.x - nextTile.X);
            }
            else { xMovement = percThisFrame * (nextTile.X - position.x); }

            if (position.y > nextTile.Y) { yMovement = percThisFrame * -(position.y - nextTile.Y); }
            else { yMovement = percThisFrame * (nextTile.Y - position.y); }

            position = position + new Vector2(xMovement, yMovement);
        }
        else 
        { 
            position = new Vector2 (nextTile.X,nextTile.Y);
            nextTile = null; //doszedł na następnego tilesa więc go czyścimy            
            //Debug.Log("doszedł na następny tiles - doMovement");
            if (position == destination)//doszedł do celu
            {
                if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this); }//aktualizacja animacji po zakończeniu przejścia

                if (previousTile != null)
                {
                    destination = new Vector2(previousTile.X, previousTile.Y);
                }
                pathAStar = null;
                //Debug.Log("DOTARŁ DO CELU!!!!!!");

                if (onArrivedAtPosition != null)
                {
                    Action tmpAction = onArrivedAtPosition;
                    onArrivedAtPosition = null;
                    nextTile = null;

                    if (currentTile.movementCost != 0)
                    {
                        //Debug.Log("Doszedł na bezpieczny koniec");
                        isMoving = false;
                        nextTile = null;
                    }
                    else //escape after finish from unwalkable tile
                    {
                        nextTile = previousTile;                     
                    }
                    tmpAction();

                }
            }

        }


    }


    public void Update_DoMovement(float deltaTime) 
    {
        if (currentTile == null)//inicjacja
        {
            currentTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        }

        if (destination == position)//arrived at exact destination so update graphics
        {
            //Debug.Log("arrived at exact destination so update graphics");
            //this.state = State.Idle;//nowy dodatek
            pathAStar = null;
            if (cbOnNextTilleArrival != null) { cbOnNextTilleArrival(this);
                //Debug.LogWarning("wołam nextTille arrival na końcu ścieżki");
            }



            if (onArrivedAtPosition != null) 
            {
                //Debug.Log("odpala funkcje po dojściu");
                onArrivedAtPosition();
                //onArrivedAtPosition = null;//       <----------------------------------
                //currentTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
                //Debug.LogWarning(" odpalone onArrivedAtPosition!!!!"); 
            } 
            else 
            { 
                //Debug.LogWarning("onArrivedAt position is empty!!!!");
                state = State.Idle;
                //Debug.Log("zmieniam na stejt Idle po Update do movemen");
            }

            return;
        }

        if (nextTile != null)
        {
            //przesuwam w ramach jednego odcinka
            // What's the total distance from point A to point B?
            float distToTravel = Mathf.Sqrt(Mathf.Pow(position.x - nextTilePosition.x, 2) + Mathf.Pow(position.y - nextTilePosition.y, 2));
            if (this.spriteVariation[0] == 0)
            {
                //Debug.Log("Distans to travel" + distToTravel);
                //Debug.Log("Pos" + position);
            }
            // How much distance can be travel this Update?
            float distThisFrame = movementSpeed * deltaTime;
            // How much is that in terms of percentage to our destination?
            float percThisFrame = distThisFrame / distToTravel;
            if (percThisFrame < 1)
            {
                float xMovement;
                float yMovement;
                if (position.x > nextTilePosition.x)
                {
                    xMovement = percThisFrame * -(position.x - nextTilePosition.x);
                }
                else { xMovement = percThisFrame * (nextTilePosition.x - position.x); }

                if (position.y > nextTilePosition.y) { yMovement = percThisFrame * -(position.y - nextTilePosition.y); }
                else { yMovement = percThisFrame * (nextTilePosition.y - position.y); }

                position = position + new Vector2(xMovement, yMovement);
            }
            else
            {
                if(position!=destination)//zabezpieczenie przed sztuczną zmianą pozycji po dojściu do celu innego niż środek tilesa
                position = new Vector2(nextTilePosition.x, nextTilePosition.y);



                currentTile = nextTile;
                nextTile = null; //doszedł na następnego tilesa lub koniec - czysćimy next tile                  
                /*if (this.spriteVariation[0] == 0)
                    Debug.Log("Doszedł na NT i go kasuje");*/
            }
            //wykonał małe przesunięcie więc aktualizujemy pozycję sprita
            if (cbOnChanged != null) { cbOnChanged(this); }
        }
        else
        {
            nextTile = GetNextTile();
            if (cbOnNextTilleArrival != null) 
            { 
                cbOnNextTilleArrival(this);
            }

            if (nextTile != null)
            {
                nextTilePosition = new Vector2(nextTile.X, nextTile.Y);
                //Weź pod uwagą offset gdy jesteś na poprzedzającej pozycji
                if (nextTile == destinationTile) { nextTilePosition = destination; }
            }
            if (nextTile == null) 
            { 
                Debug.LogError("Brak kontynuacji ścieżki");
                TropicsUtilsClass.CreateWorldTextPopup("STUCK!!!", WorldController.Instance.charController.characterGameObjectMap[this].transform.position);
                ChangeStateToIdle(this);
                return; 
            }
        }

    }




    private Tile GetNextTile()
    {
        Tile NT = null;

        if (pathAStar==null)//w momencie gdy zaczynam / stracił możliwość dojścia /start nie chodliwy/koniec nie chodliwy 
        {
            //Debug.Log("Szukam nowej ścieżki");
            

            if (currentTile!=null && currentTile.movementCost == 0) Debug.LogError("Punkt startowy uwalkable");
            if (destinationTile.movementCost == 0) Debug.LogError("Punkt końcowy uwalkable:"+destinationTile.X+":"+destinationTile.Y);
            currentTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            pathAStar = new Pathfinding(currentTile.world, currentTile, destinationTile);
            if (pathAStar == null) 
            {
                Debug.Log("BRAK możliwego dojścia");

            }
            else //znalazłem ścieżkę
            {

                if (pathAStar.Length() > 0)
                {
                    NT = pathAStar.GetNextTile();
                }
            }
        }
        else //w przypadku gdy była ścieżka
        {
            if (pathAStar.Length() > 0)
            {
                
                NT = pathAStar.GetNextTile();
            }
        }

        //Debug.Log("NT z astara to:" + NT.X + ":" + NT.Y);
        return NT;
    }





    public void RoamTo(Vector2 newDestination, Action onArrivedAtPosition)
    {
        state = State.Roaming;
        Debug.Log("Zmieniam stejt na Roam w character.roamTo");
        destination = newDestination;
        destinationTile = WorldController.Instance.World.GetTileAt(Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.y));
        //this.onArrivedAtPosition += new Action(() => { this.state = State.Idle; });//WTF???????????????
        this.onArrivedAtPosition = onArrivedAtPosition;

        if (cbOnNextTilleArrival != null){cbOnNextTilleArrival(this);}
    }



    public void MoveTo(Vector2 newDestination, Action onArrivedAtPosition)
    {
        this.state = State.Moving;
        currentTile = null;
        //Debug.Log("Zmieniam stejt na Move w character.moveTo");
        destination = newDestination;
        destinationTile = WorldController.Instance.World.GetTileAt(Mathf.RoundToInt(destination.x), Mathf.RoundToInt(destination.y));

        if (destination!=position && destinationTile == WorldController.Instance.World.GetTileAt(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y))) //ruch w ramach jednego tilesa
        {
            //Debug.Log("Wykonuje moveTo w ramach jednego tilesa");
            nextTile = destinationTile;
            nextTilePosition = newDestination; 
        }

        //Debug.Log("DESTINATION: "+destination);

        /*this.onArrivedAtPosition += new Action(() => { this.state = State.Idle;
            Debug.Log("Zmieniam stejt na Idle w akcji po dojściu do celu");
        });
        this.onArrivedAtPosition += onArrivedAtPosition;
        if (this.onArrivedAtPosition == null) Debug.Log("Pusty on arrived at position"); else Debug.Log("NIE Pusty on arrived at position");*/

        this.onArrivedAtPosition = new Action (()=>{
            this.state = State.Idle;
            onArrivedAtPosition();

        });

        if (cbOnNextTilleArrival != null){cbOnNextTilleArrival(this);}
    }


    public void Wait(InstalledObject IO, Action onWaitEnd)
    {
        //Debug.LogWarning("WAITING FUNCTION");
        if (cbOnNextTilleArrival != null)
        {
            cbOnNextTilleArrival(this);
        }
            onWaitEnd();
        
    }

    public void Sleep(InstalledObject IO, int hours, Action onSleepEnd)
    {

        /*Debug.LogWarning("Sleep FUNCTION");
        if (cbOnNextTilleArrival != null)
        {
            cbOnNextTilleArrival(this);
        }*/

        float RTDuration = hours * Clock.REAL_SECOUNDS_PER_INGAME_DAY / 24; //changes in game time to wait time in real secounds
        TropicsUtils.FunctionTimer.Create(onSleepEnd, RTDuration);
           // onSleepEnd();
    }


    public void Disappear(InstalledObject IO, float hours, Action onEnd)
    {
        this.visible = false;
        if (cbOnNextTilleArrival != null)
        {
            cbOnNextTilleArrival(this);
        }
        Action baseAction = new Action(() => {
            this.visible = true;
            this.state = State.Moving;
            if (cbOnNextTilleArrival != null)
            {
                cbOnNextTilleArrival(this);
            }
        });

        baseAction += onEnd;
        //onEnd += addAction;
        float GTDuration = hours  /* *Clock.REAL_SECOUNDS_PER_INGAME_DAY*/ / 24; //in Game time dissapear//changes in game time to wait time in real secounds
        //force 


        this.currentTile = IO.tile;
        this.nextTile = WorldController.Instance.World.GetTileAt(IO.tile.X - 1, IO.tile.Y);//entry tile!!!

        TropicsUtils.FunctionTimer.Create(baseAction, GTDuration);
        //Debug.LogWarning("Disappear FUNCTION");



    }





    public void PlayAnim(Action onPlayAnimEnd)
    {

            state = State.Animating;
            //Debug.Log("stejt Animating w play anim");
            if (cbOnPlayAnimation != null)
            {
                cbOnPlayAnimation(this); //odpala animacje
            }
            //globalna akcja odpalana przez stejt w updacie o ile State.Animating

        tmpAction=onPlayAnimEnd;//tmp action is trigered after current animation stops playing

        //Action cbAction = new Action(cbA);
      
        //tmpAction = tmpAction /*+ cbAction*/; //nie moge dokładać on next Tile
        //Debug.Log("dodałe akcje do TMP Action");
    }


    public void PickupAction(Character t, Resource pickedUpResource, int amount)
    {
        //extra check and zeroing

        if (t.resource!=null && t.resource.resourceType != pickedUpResource.resourceType && t.resource.amount != 0) 
        {
            Debug.LogWarning("Nie podnies - MAM inny surowiec");
            return; 
        }
        if (t.resource == null) 
        { 
            t.resource = new Resource(); 
            t.resource.RegisterOnChangedCallback(t.resource.onResourceChanged);
        }
        if (t.resource.resourceType != pickedUpResource.resourceType && t.resource.amount == 0) t.resource.resourceType = pickedUpResource.resourceType;//zmieniam typ na ten który podnosze
               
        
        
        GameObject carrierGO = WorldController.Instance.charController.characterGameObjectMap[t];//nosiciel
        t.resource.TransferResources(pickedUpResource, t.resource, amount, carrierGO);



                t.resource.resourceGO.transform.localPosition = new Vector3(0, 0.1f, 0);

        //updejt map i zdjęcie rezerw
        if (WorldController.ResourceStockpileSlotMap.ContainsKey(pickedUpResource))
        {
            WorldController.ResourceStockpileSlotMap[pickedUpResource].reservedOut -= amount;
            StockpileSlot s = WorldController.ResourceStockpileSlotMap[pickedUpResource];
            WorldController.Instance.UpdateStockpileSlotResourceMap(s, s.resource);
        }

        //czyszczenie i update GO
        if (pickedUpResource.cbOnChanged != null) pickedUpResource.cbOnChanged(pickedUpResource);
        if (t.resource.cbOnChanged != null) t.resource.cbOnChanged(t.resource);

    }


    public void DropOffAction(Resource droppingResource, StockpileSlot stockpileSlot)
    {
        Tile t = WorldController.Instance.World.GetTileAt(stockpileSlot.tilePosition.X, stockpileSlot.tilePosition.Y);
        InstalledObject parentIO = WorldController.tileInstalledObjectMap[t];//target of droping
        GameObject parentGO = WorldController.installedObjectGameObjectMap[parentIO];
        
 
        stockpileSlot.reservedIn -= resource.amount;//zdejmuje rezerwę na przychodzący towar
        stockpileSlot.resource.TransferResources(droppingResource, stockpileSlot.resource, droppingResource.amount, parentGO);
        
        stockpileSlot.resource.resourceGO.transform.position = parentGO.transform.position + new Vector3(stockpileSlot.positionModifier.x, stockpileSlot.positionModifier.y, 0);
        
        if (stockpileSlot.resource.amount > stockpileSlot.SlotCapacity)//odnoszę dalej nadmiar
        {
            //zwracam nadmiar nad pojemność
            droppingResource.TransferResources(stockpileSlot.resource, droppingResource, stockpileSlot.resource.amount - stockpileSlot.SlotCapacity, droppingResource.parent);

            //ADD haul task or drop
            StockpileSlot s = WorldController.Instance.stockpileSlot.FindStockpileSlot(resource);
            if (s != null)
            {
                TaskSystem.Task.Haul task = new TaskSystem.Task.Haul
                {
                    targetPosition = new Vector2(this.currentTile.X + 1, this.currentTile.Y),
                    targetPosition2 = new Vector2(s.tilePosition.X, s.tilePosition.Y),
                    dropOffSlot = s,
                    resource = droppingResource,
                };

                GameObject GO = CharController.Instance.GetCharacterGOFromMap(this);
                WorkerAI wAI = GO.GetComponent<WorkerAI>();
                wAI.ExecuteTask_Haul(task);
                Debug.Log("Added self Haul Task");
            }
            else resource.DropResource(resource);
        }
        else { }



        // updejt map 

        if (WorldController.StockpileSlotResourceMap.ContainsKey(stockpileSlot))
        { 
            WorldController.Instance.UpdateStockpileSlotResourceMap(stockpileSlot, stockpileSlot.resource);
        }

        //czyszczenie i updajt GO
        if (stockpileSlot.resource.cbOnChanged != null) stockpileSlot.resource.cbOnChanged(stockpileSlot.resource);
        if (droppingResource.cbOnChanged != null) droppingResource.cbOnChanged(droppingResource);
               
    }


          

    private void cbA()
    {
        if (cbOnNextTilleArrival != null)
        cbOnNextTilleArrival(this); 
    }
    
       

    public void RegisterOnChangedCallback(Action<Character> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback(Action<Character> callbackFunc)
    {
        //Debug.Log("usunoł callback");
        cbOnChanged -= callbackFunc;
    }

    public void RegisterOnNextTilleArrivalCallback(Action<Character> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnNextTilleArrival += callbackFunc;
    }
    public void UnregisterOnNextTilleArrivalCallback(Action<Character> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnNextTilleArrival -= callbackFunc;
    }


    public void RegisterOnPlayAnimation(Action<Character> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnPlayAnimation += callbackFunc;
    }
    public void UnregisterOnPlayAnimation(Action<Character> callbackFunc)
    {
        //Debug.Log("zarejestrował callback");
        cbOnPlayAnimation -= callbackFunc;
    }

}
