﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : movementManager
{
    GameObject[,,] mapRef;
    combatManager comMan;
    int x, y, z;
    //string turn;
    bool attackCheck = false;
    List<int> range = new List<int> { };

    public GameObject PathUI;
    public GameObject AttackUI;
    public GameObject selectedUnit = null;
    public int usedMove = 0;
    //int heyo = 0;

    Terrain[] thePath;

    // Use this for initialization
    void Start()
    {
        mapRef = GameObject.Find("Director").GetComponent<GridMap>().MAP;
        x = Mathf.RoundToInt(transform.position.x);
        y = Mathf.RoundToInt(transform.position.y);
        z = Mathf.RoundToInt(transform.position.z) * -1;

        comMan = GameObject.Find("Director").GetComponent<combatManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //turn = GameObject.Find("Director").GetComponent<GridMap>().currentTurn;

        if (selectedUnit != null)   //if a unit is selected,  move them around with the selector
        {
            if (!attackCheck)
            { PathIndicator(thePath); }
            else
            {
                Weapons weapon = selectedUnit.GetComponent<RPGClass>().inventory[0].GetComponent<Weapons>();

                int tx = Mathf.RoundToInt(selectedUnit.transform.position.x);
                int ty = Mathf.RoundToInt(selectedUnit.transform.position.y);
                Terrain loc = mapRef[tx, ty, 0].GetComponent<Terrain>();

                range = AttackIndicator(loc, weapon.minRange, weapon.maxRange);
            }
        }

        //this works pretty good for now. Ideally, when you select your unit it highlights all possible tiles.
        inputManagerV2();
    }


    //colours the current path to provide some feedback
    void PathIndicator (Terrain[] currentPath)
    {
        for (int i = 0; i < currentPath.Length; i++)
        {
            float px = currentPath[i].x;
            float py = currentPath[i].y;

            GameObject pUI = Instantiate(PathUI, new Vector3(px, py), transform.rotation);
            pUI.GetComponent<DestroyOnBoolNotReset>().flagChecked = true;
        }
    }

    //colours attack range
    List<int> AttackIndicator (Terrain Location, int minRange, int maxRange)
    {
        List<int> coordinates = new List<int> { };

        int x = Location.x;
        int y = Location.y;
        GameObject aUI;
        maxRange++;

        for (int i = minRange; i < maxRange; i++)
        {
            for (int j = -i; j < i; j++)
            {
                int k = Mathf.Abs(j) - i;
                aUI = Instantiate(AttackUI, new Vector3(x-j, y+k), transform.rotation);
                aUI.GetComponent<DestroyOnBoolNotReset>().flagChecked = true;

                k = ((x - j) * 1000) + (y + k);
                coordinates.Add(k);


                k = -Mathf.Abs(j) + i;
                aUI = Instantiate(AttackUI, new Vector3(x+j, y+k), transform.rotation);
                aUI.GetComponent<DestroyOnBoolNotReset>().flagChecked = true;

                k = ((x - j) * 1000) + (y + k);
                coordinates.Add(k);
            }
        }

        return coordinates;
    }

    //check range
    bool CheckRange (int x, int y, int minRange, int maxRange)
    {
        bool inRange = false;




        return inRange;
    }


    //detects inputs, then it will update thePath if a unit is selected and being moved around.
    void inputManagerV2()
    {
        Terrain nextTile;
        int maxMove;

        //move up
        if ((Input.GetKeyDown("w") || Input.GetKeyDown("up")) && y + 1 < mapRef.GetLength(1))
        {
            if (selectedUnit != null && !attackCheck)   //if a unit is currently selected
            {
                nextTile = mapRef[x, y + 1, 0].GetComponent<Terrain>();
                maxMove = selectedUnit.GetComponent<RPGClass>().Stats[11].staticValue;
                thePath = gridPathfindingArray(thePath, nextTile, maxMove);
            }

            mapRef[x, y, z] = null;
            y++;
            mapRef[x, y, z] = gameObject;
            transform.position = new Vector3(x, y, -z);
        }

        //move down
        else if ((Input.GetKeyDown("s") || Input.GetKeyDown("down")) && y-1 >= 0)
        {
            if (selectedUnit != null && !attackCheck)
            {
                nextTile = mapRef[x, y - 1, 0].GetComponent<Terrain>();
                maxMove = selectedUnit.GetComponent<RPGClass>().Stats[11].staticValue;
                thePath = gridPathfindingArray(thePath, nextTile, maxMove);
            }

            mapRef[x, y, z] = null;
            y--;
            mapRef[x, y, z] = gameObject;
            transform.position = new Vector3(x, y, -z);
        }

        //move left
        else if ((Input.GetKeyDown("a") || Input.GetKeyDown("left")) && x-1 >= 0)
        {
            if (selectedUnit != null && !attackCheck)
            {
                nextTile = mapRef[x - 1, y, 0].GetComponent<Terrain>();
                maxMove = selectedUnit.GetComponent<RPGClass>().Stats[11].staticValue;
                thePath = gridPathfindingArray(thePath, nextTile, maxMove);
            }

            mapRef[x, y, z] = null;
            x--;
            mapRef[x, y, z] = gameObject;
            transform.position = new Vector3(x, y, -z);
        }

        //move right
        else if ((Input.GetKeyDown("d") || Input.GetKeyDown("right")) && x+1 < mapRef.GetLength(0))
        {
            if (selectedUnit != null && !attackCheck)
            {
                nextTile = mapRef[x + 1, y, 0].GetComponent<Terrain>();
                maxMove = selectedUnit.GetComponent<RPGClass>().Stats[11].staticValue;
                thePath = gridPathfindingArray(thePath, nextTile, maxMove);
            }

            mapRef[x, y, z] = null;
            x++;
            mapRef[x, y, z] = gameObject;
            transform.position = new Vector3(x, y, -z);
        }

        //select object, so that only a selected unit may be moved
        else if (Input.GetKeyDown("space"))
        {
            //select or deselect depending on current state
            if (selectedUnit == null)
            {
                if (!mapRef[x, y, z - 1].GetComponent<RPGClass>().hasMoved)
                {
                    selectedUnit = mapRef[x, y, z - 1];
                    mapRef[x, y, z - 1] = null;

                    thePath = new Terrain[1];
                    thePath[0] = mapRef[x, y, z - 2].GetComponent<Terrain>();

                    //PossibleDestinations(thePath[0], 2, mapRef);
                }
            }
            else if (!attackCheck)
            {
                mapRef[thePath[thePath.Length-1].x, thePath[thePath.Length - 1].y, z - 1] = selectedUnit;
                selectedUnit.transform.position = new Vector3(thePath[thePath.Length - 1].x, thePath[thePath.Length - 1].y, -1);
                attackCheck = true;

                //selectedUnit = null;
                //usedMove = 0;
            }
            else
            {
                if (mapRef[x, y, 1] != null && mapRef[x, y, 1] != gameObject && range.Contains(x * 1000 + y))
                {
                    comMan.attackingUnit = selectedUnit;
                    comMan.attackerTerrain = mapRef[Mathf.RoundToInt(selectedUnit.transform.position.x), Mathf.RoundToInt(selectedUnit.transform.position.y), 0];

                    comMan.defendingUnit = mapRef[x, y, 1];
                    comMan.defenderTerrain = mapRef[x, y, 0];

                    comMan.runCombat = true;
                }
                selectedUnit.GetComponent<RPGClass>().hasMoved = true;
                selectedUnit = null;
                attackCheck = false;
            }
        }

        else if (Input.GetKeyDown(KeyCode.Backspace) && selectedUnit != null)
        {
            mapRef[thePath[0].x, thePath[0].y, 1] = selectedUnit;
            selectedUnit.transform.position = new Vector3(thePath[0].x, thePath[0].y, -1);
            attackCheck = false;
            selectedUnit = null;
        }


        /*
        else if (Input.GetKeyDown(KeyCode.LeftShift) && selectedUnit == null)
        {
            if (mapRef[x, y, z - 1] != null)
            {
                if (comMan.attackingUnit == null)
                {
                    comMan.attackingUnit = mapRef[x, y, z - 1];
                    comMan.attackerTerrain = mapRef[x, y, z - 2];
                }
                else
                {
                    comMan.defendingUnit = mapRef[x, y, z - 1];
                    comMan.defenderTerrain = mapRef[x, y, z - 2];
                }
            }
            else if (comMan.attackingUnit != null || comMan.defendingUnit != null)
            {
                comMan.attackingUnit = comMan.attackerTerrain = comMan.defendingUnit = comMan.defenderTerrain = null;
            }
        }

        else if (Input.GetKeyDown(KeyCode.Return) && comMan.attackingUnit != null && comMan.defendingUnit != null)
        {
            comMan.runCombat = true;
        }
        */


    }



    /*
    //detects an input, then does something depending on the input
    void inputManager()
    {
        //move up
        if (Input.GetKeyDown("w") || Input.GetKeyDown("up"))
        {
            //I can probably make this a function, especially because you can only ever move one tile at a time
            if (selectedUnit != null)   //if a unit is currently selected
            {
                Terrain nextTile = mapRef[x, y+1, 0].GetComponent<Terrain>();
                heyo = selectedUnit.GetComponent<RPGClass>().MovementLeft(usedMove, nextTile.movementCost, nextTile.tileName);
                usedMove += heyo;
            }
            
            if (heyo != 0 || selectedUnit == null)
            {
                mapRef[x, y, z] = null;
                y++;
                mapRef[x, y, z] = gameObject;
                transform.position = new Vector3(x, y, -z);
            }
        }

        //move down
        else if (Input.GetKeyDown("s") || Input.GetKeyDown("down"))
        {
            if (selectedUnit != null)
            {
                Terrain nextTile = mapRef[x, y - 1, 0].GetComponent<Terrain>();
                heyo = selectedUnit.GetComponent<RPGClass>().MovementLeft(usedMove, nextTile.movementCost, nextTile.tileName);
                usedMove += heyo;
            }

            if (heyo != 0 || selectedUnit == null)
            {
                mapRef[x, y, z] = null;
                y--;
                mapRef[x, y, z] = gameObject;
                transform.position = new Vector3(x, y, -z);
            }
        }

        //move left
        else if (Input.GetKeyDown("a") || Input.GetKeyDown("left"))
        {
            if (selectedUnit != null)
            {
                Terrain nextTile = mapRef[x-1, y, 0].GetComponent<Terrain>();
                heyo = selectedUnit.GetComponent<RPGClass>().MovementLeft(usedMove, nextTile.movementCost, nextTile.tileName);
                usedMove += heyo;
            }

            if (heyo != 0 || selectedUnit == null)
            {
                mapRef[x, y, z] = null;
                x--;
                mapRef[x, y, z] = gameObject;
                transform.position = new Vector3(x, y, -z);
            }
        }

        //move right
        else if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
        {
            if (selectedUnit != null)
            {
                Terrain nextTile = mapRef[x+1, y, 0].GetComponent<Terrain>();
                heyo = selectedUnit.GetComponent<RPGClass>().MovementLeft(usedMove, nextTile.movementCost, nextTile.tileName);
                usedMove += heyo;
            }

            if (heyo != 0 || selectedUnit == null)
            {
                mapRef[x, y, z] = null;
                x++;
                mapRef[x, y, z] = gameObject;
                transform.position = new Vector3(x, y, -z);
            }
        }

        //select object, so that only a selected unit may be moved
        else if (Input.GetKeyDown("space"))
        {
            //select or deselect depending on current state
            if (selectedUnit == null)
            {
                selectedUnit = mapRef[x, y, z-1];
                mapRef[x, y, z - 1] = null;
            }
            else
            {
                mapRef[x, y, z - 1] = selectedUnit;
                selectedUnit = null;
                usedMove = 0;
            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftShift) && selectedUnit == null)
        {
            if (mapRef[x, y, z - 1] != null)
            {
                if (comMan.attackingUnit == null)
                {
                    comMan.attackingUnit = mapRef[x, y, z - 1];
                    comMan.attackerTerrain = mapRef[x, y, z - 2];
                }
                else
                {
                    comMan.defendingUnit = mapRef[x, y, z - 1];
                    comMan.defenderTerrain = mapRef[x, y, z - 2];
                }
            }
            else if (comMan.attackingUnit != null || comMan.defendingUnit != null)
            {
                comMan.attackingUnit = comMan.attackerTerrain = comMan.defendingUnit = comMan.defenderTerrain = null;
            }
        }

        else if (Input.GetKeyDown(KeyCode.Return) && comMan.attackingUnit != null && comMan.defendingUnit != null)
        {
            comMan.runCombat = true;
        }

    }
    */

}