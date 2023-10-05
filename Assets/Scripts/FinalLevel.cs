using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLevel : MonoBehaviour
{
    public GameObject playerObj;        //Player GameObject
    private PlayerFinalLevel player;              //Player script object
    public GameObject Lock;             //the Lock object in the level (primarily used for getting the lock's position)
    private bool won;                   //a bool that indicates if the level has been successfully completed
    private string goal;                //the string goal that the player is trying to achieve when traversing the puzzle

    public GameObject[] borders;        //keeps track of a list of GameObjects that act as the border of the level for collision purposes
    public GameObject[] yous;
    public GameObject[] iss;

    // Start is called before the first frame update
    void Start()
    {
        player = playerObj.GetComponent<PlayerFinalLevel>();    //get player script
        borders = GameObject.FindGameObjectsWithTag("Wall");    //gather all objects in the scene that contain the tag "Wall"
        
        player.SetString("");           //set the player's initial string to an empty string

        //set the goal string
        goal = "YOU IS NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING NOTHING ";
    }

    //Returns the list of border objects in level
    public GameObject[] GetBorders()
    {
        return borders;
    }

    //used to check if the player's string matches the goal string
    private bool CheckWin()
    {
        return goal.Equals(player.GetString());     //checks equality
    }

    //Returns value that indicates whether or not the level has been completed successfully
    public bool GetWinState()
    {
        return won;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if(playerObj.transform.position.y == 1 && player.GetMoved())
        {
            player.SetString(player.GetString() + "YOU ");
            player.SetMoved(false);
        }
        else if(playerObj.transform.position.y == 2 && player.GetMoved())
        {
            player.SetString(player.GetString() + "IS ");
            player.SetMoved(false);

            foreach(GameObject you in yous)
            {
                you.SetActive(false);
            }
        }
        else if(player.transform.position.y > 2 && player.transform.position.y < 25 && player.GetMoved())
        {
            player.SetString(player.GetString() + "NOTHING ");
            player.SetMoved(false);

            if(player.transform.position.y == 3)
            {
                player.laughTrack.volume = 1.0f / 22.0f;
                player.laughTrack.Play();
            }
            else
                player.laughTrack.volume += 1.0f / 22.0f;

            foreach (GameObject is1 in iss)
            {
                is1.SetActive(false);
            }
        }

        foreach(GameObject border in borders)
        {
            if (border.transform.position.y == player.transform.position.y - 1)
                border.SetActive(false);
        }

        //checks if goal value has been reached
        if (CheckWin())
            Lock.SetActive(false);   //if goal value has been reached, unlock the lock
        else
            Lock.SetActive(true);   //locks the lock if goal value is not reached or has been "unreached"

        //if the player is on the lock tile and the lock is not active (unlocked), then the player has won the level
        if (player.transform.position == Lock.transform.position && !Lock.activeSelf)   //***Note: might just need the first check since the player can't be on the lock if it's active***
            Application.Quit();
    }
}
