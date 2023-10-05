using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    public string startString;          //the initial string of the player when they enter the level
    public string goal;                 //the string goal that the player is trying to achieve when traversing the puzzle
    public GameObject playerObject;     //the player object in the level
    public GameObject Lock;             //the Lock object in the level (primarily used for getting the lock's position)
    private bool won;                   //a bool that indicates if the level has been successfully completed
    private Player player;              //the script component of the player object, in order to access the player variables and functions

    public GameObject[] borders;        //keeps track of a list of GameObjects that act as the border of the level for collision purposes
    public GameObject[] tiles;          //keeps track of all the tile objects in the level

    private GameObject[] goalCharacters;    //stores the character values for the Goal so that they can be disabled and enabled when player solves and unsolves the puzzle

    // Start is called before the first frame update
    void Start()
    {
        player = playerObject.GetComponent<Player>();           //Get the player script
        borders = GameObject.FindGameObjectsWithTag("Wall");    //gather all objects in the scene that contain the tag "Wall"
        tiles = GameObject.FindGameObjectsWithTag("Tile");      //gather all objects in the scene that contain the tag "Tile"

        DisplayGoal();                                          //displays the goal needed for the player to advance to next level

        goalCharacters = GameObject.FindGameObjectsWithTag("GoalCharacter");    //get all goal character objects

        won = false;                                            //initiate win state for start of level as false
    }

    //used to check if the player's string matches the goal string
    private bool CheckWin()
    {
        return goal.Equals(player.GetString());     //checks equality
    }

    //Returns the position of the lock in the level
    public Vector2 GetLockPos()
    {
        return Lock.transform.position;
    }

    //Returns value that indicates whether the lock is active or not
    public bool IsLockActive()
    {
        return Lock.activeSelf;
    }

    //Returns the list of border objects in level
    public GameObject[] GetBorders()
    {
        return borders;
    }

    //Returns value that indicates whether or not the level has been completed successfully
    public bool GetWinState()
    {
        return won;
    }

    //for each character in goal instantiate an object of that character to display the current player result
    private void DisplayGoal()
    {
        GameObject character;       //temp character gameobject for setting character tag

        //for each character in goal intantiate an object of that character to display the current player result
        for (int i = 0; i < goal.Length; i++)
        {
            //if the character is a number (using ascii range), instantiate the number by converting the ascii value to the numbers array index
            if (48 <= System.Convert.ToInt32(goal[i]) && System.Convert.ToInt32(goal[i]) <= 57)
            {
                //set the digit to a position relative to where it lies in the string
                character = Instantiate(player.numbers[System.Convert.ToInt32(goal[i]) - 48], new Vector3(Lock.transform.position.x + ((goal.Length - 1) * (-0.125f) + (i * 0.25f) + 1), Lock.transform.position.y + 1f, 0), Quaternion.identity);
                character.tag = "GoalCharacter";        //set the character tag for detecting when disabling
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
            //if the character is a letter (using ascii range), instantiate the letter by converting the ascii value to the letters array index
            else if (65 <= System.Convert.ToInt32(goal[i]) && System.Convert.ToInt32(goal[i]) <= 90)
            {
                //set the letter to a position relative to where it lies in the string
                character = Instantiate(player.letters[System.Convert.ToInt32(goal[i]) - 65], new Vector3(Lock.transform.position.x + ((goal.Length - 1) * (-0.125f) + (i * 0.3f) + 1), Lock.transform.position.y + 1f, 0), Quaternion.identity);
                character.tag = "GoalCharacter";        //set the character tag for detecting when disabling
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
            else if (System.Convert.ToInt32(goal[i]) == 46) //if the character is a decimal, instantiate the decimal object
            {
                //set the decimal to a position relative to where it lies in the string (decimal is currently 10 in the array)
                character = Instantiate(player.numbers[10], new Vector3(Lock.transform.position.x + ((goal.Length - 1) * (-0.125f) + (i * 0.25f) + 1), Lock.transform.position.y + 1f, 0), Quaternion.identity);
                character.tag = "GoalCharacter";        //set the character tag for detecting when disabling
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Update is called once per frame
    void Update()
    {
        //When player presses 'R' key in a level, it resets the level
        if (Input.GetKeyDown(KeyCode.R))
            ReloadLevel();

        //checks if goal value has been reached
        if (CheckWin())
        {
            Lock.SetActive(false);                  //if goal value has been reached, unlock the lock
            foreach (GameObject c in goalCharacters)    //if goal value has been reached, de-activate the goal characters
                c.gameObject.SetActive(false);
        }
        else
        {
            Lock.SetActive(true);                   //locks the lock if goal value is not reached or has been "unreached"
            foreach (GameObject c in goalCharacters)    //if goal value has been reached, re-activate the goal characters
                c.gameObject.SetActive(true);
        }

        //if the player is on the lock tile and the lock is not active (unlocked), then the player has won the level
        if (player.transform.position == Lock.transform.position && !Lock.activeSelf)   //***Note: might just need the first check since the player can't be on the lock if it's active***
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}