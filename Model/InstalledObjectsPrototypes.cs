using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObjectsPrototypes
{
    public void Prototypes(Dictionary<string, InstalledObject> installedObjectPrototypes)
    {


        installedObjectPrototypes.Add("Wall01", InstalledObject.CreatePrototype(
            "Wall01",
            0.01f, //movement cost 0 - 0% of speed
            1,
            1,
            true,
            true, //ghosting
            0,//rotation
            5,//wood
            5,//leaves
            0,//stone
            0,//bricks
            0,//beauty
            false,//required to be build on water
            false, //have entry tile
            false //animator required
            )
            );

        installedObjectPrototypes.Add("Foliage01", InstalledObject.CreatePrototype(
            "Foliage01",//objest type
            0.1f,//movement cost 1- 100%of speed 
            1,//base width
            1,//base height
            false,// connectst to neighbours?
            false,
            0,//rotation
                        0,//wood
            0,//leaves
            0,//stone
            0,//bricks
            100,//beauty
            false,//required to be build on water
            false, //have entry tile
            false //animator required
            )
            );

        installedObjectPrototypes.Add("Foliage02", InstalledObject.CreatePrototype(
    "Foliage02",//objest type
    0.1f,//movement cost
    1,//base width
    1,//base height
    false,// connectst to neighbours?
    false,
    0,//rotation
               0,//wood
           0,//leaves
            0,//stone
            0,//bricks
            100,//beauty
            false,//required to be build on water
            false, //have entry tile
            false //animator required
    )
    );


        installedObjectPrototypes.Add("Stockpile", InstalledObject.CreatePrototype(
        "Stockpile",//object type
        1f,//movement cost
        1,//base width
        1,//base height
        false,// connectst to neighbours?
        false

        )
        );

        installedObjectPrototypes.Add("Accommodation01", InstalledObject.CreatePrototype(
        "Accommodation01",//object type
        0,//movement cost
        2,//base width
        2,//base height
        false,// connectst to neighbours?
        true,  //ghosting
        1,//rotation
                    10,//wood
            10,//leaves
            0,//stone
            0,//bricks
            0,//beauty
            false,//required to be build on water
            true, //have entry tile
            true //animator required
        )
        );


        installedObjectPrototypes.Add("AccommodationWater01", InstalledObject.CreatePrototype(
"AccommodationWater01",//object type
0,//movement cost
2,//base width
2,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
            30,//wood
    10,//leaves
    0,//stone
    0,//bricks
    0,//beauty
    true,//required to be build on water
    true, //have entry tile
    true //animator required
)
);


        installedObjectPrototypes.Add("BuildSlot01", InstalledObject.CreatePrototype(
        "BuildSlot01",//obiekt do zbierania surowców przed budową obiektu
        1f,//movement cost
        1,//base width
        1,//base height
        false,// connectst to neighbours?
        false,  //ghosting
        0,//rotation
        0,//wood
        0,//leaves
        0,//stone
        0,//bricks
        0,//beauty
        false,//required to be build on water
        false, //have entry tile
        false //animator required
        )
        );

        installedObjectPrototypes.Add("EntranceSlot01", InstalledObject.CreatePrototype(
"EntranceSlot01",//obiekt z którego się wchodzi do głównego
1f,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
false,  //ghosting
0,//rotation
0,//wood
0,//leaves
0,//stone
0,//bricks
0,//beauty
false,//required to be build on water
false, //have entry tile
false //animator required
)
);


        installedObjectPrototypes.Add("LandingSlot01", InstalledObject.CreatePrototype(
"LandingSlot01",//
0f,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
false,  //ghosting
0,//rotation
0,//wood
0,//leaves
0,//stone
0,//bricks
0,//beauty
true,//required to be build on water
false, //have entry tile
false //animator required
)
);




        installedObjectPrototypes.Add("SunBed01", InstalledObject.CreatePrototype(
"SunBed01",//leżanka drewniana
0.1f,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
5,//wood
5,//leaves
0,//stone
0,//bricks
50,//beauty
false,//required to be build on water
true, //have entry tile
false //animator required

)
);

        installedObjectPrototypes.Add("CheckIn01", InstalledObject.CreatePrototype(
"CheckIn01",//checkin
0.2f,//movement cost
2,//base width
3,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
10,//wood
0,//leaves
0,//stone
0,//bricks
50,//beauty
false,//required to be build on water
false, //have entry tile
false //animator required
)
);

        installedObjectPrototypes.Add("ChangingRoom01", InstalledObject.CreatePrototype(
"ChangingRoom01",//przebieralnia
0,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
10,//wood
0,//leaves
0,//stone
0,//bricks
0,//beauty
false,//required to be build on water
true, //have entry tile
true //animator required
)
);



        installedObjectPrototypes.Add("ToiletStall01", InstalledObject.CreatePrototype(
"ToiletStall01",//przebieralnia
0,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
10,//wood
10,//leaves
0,//stone
0,//bricks
0,//beauty
false,//required to be build on water
true, //have entry tile
true //animator required
)
);


        installedObjectPrototypes.Add("FoodStall01", InstalledObject.CreatePrototype(
"FoodStall01",//stoisko z żarciem
0f,//movement cost
2,//base width
3,//base height
false,// connectst to neighbours?
true,  //ghosting
1,//rotation
10,//wood
10,//leaves
0,//stone
0,//bricks
50,//beauty
false,//required to be build on water
false, //have entry tile
false //animator required
)
);

        installedObjectPrototypes.Add("Molo01", InstalledObject.CreatePrototype(
"Molo01",//stoisko z żarciem
0.8f,//movement cost
1,//base width
1,//base height
false,// connectst to neighbours?
true,  //ghosting
0,//rotation , 1 allow, 0 not allowed
20,//wood
0,//leaves
0,//stone
0,//bricks
50,//beauty
true,//required to be build on water
false, //have entry tile
false //animator required
)
);



        installedObjectPrototypes.Add("PlaneMolo01", InstalledObject.CreatePrototype(
"PlaneMolo01",//lądowisko hydroplanów
0.1f,//movement cost
3,//base width
1,//base height
false,// connectst to neighbours?
true,  //ghosting
0,//rotation , 1 allow, 0 not allowed
100,//wood
0,//leaves
0,//stone
0,//bricks
0,//beauty
true,//required to be build on water
false, //have entry tile
false //animator required
)
);




    }
}
