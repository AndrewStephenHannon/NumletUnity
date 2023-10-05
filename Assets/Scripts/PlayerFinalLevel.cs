using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFinalLevel : MonoBehaviour
{
    private string currentString;   //current string value for player's puzzle progess
    public Text currentStringText;  //variable used for the visual representation of the player's current string
    private bool isMovingX;         //a boolean that determines whether or not the player is currently moving in the X direction
    private bool isMovingY;         //a boolean that determines whether or not the player is currently moving in the Y direction    

    private float startValue;       //variable for indicating the start value of player movement for lerping purposes (can be used for either x or y)
    private float endValue;         //variable for indicating the end value of player movement for lerping purposes (can be used for either x or y)

    private float timeElapsed;      //current amount of time that has elapsed during player movement (lerping)
    public float lerpDuration;      //the amount of time the lerp for player movement takes

    private float lerpValue;        //keeps track of the lerp progress

    public GameObject levelObject;  //gets a reference to the current level object
    private FinalLevel level;            //gets a reference to the current level script

    private OpType currOperator;    //keeps track of the current operator the player is standing on, if any
    private char currValue;         //keeps track of current value the player is standing on, if any

    private bool moved;

    public AudioSource laughTrack;

    // Start is called before the first frame update
    void Start()
    {
        level = levelObject.GetComponent<FinalLevel>();      //gets the level object's script
        currentStringText.text = "Current Result: " + currentString;    //initializing the visual string result

        laughTrack = GetComponent<AudioSource>();

        //instantiate move booleans
        isMovingX = false;
        isMovingY = false;

        moved = false;
    }

    //Used for when a player is solving the puzzle and the player string needs to be altered
    public void SetString(string value)
    {
        currentString = value;
    }

    //Used for checking the player's current string value (possibly used for checking if win state is reached)
    public string GetString()
    {
        return currentString;
    }

    //Function for handling Player the movement lerp
    private void LerpMove()
    {
        if (timeElapsed < lerpDuration)      //if the current time elapsed is less than the lerp duration, then continue executing lerp movement
        {
            lerpValue = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);   //set the lerp value using the start value end value and curent time elapsed over the lerp duration

            if (isMovingX)
                transform.position = new Vector2(lerpValue, transform.position.y);      //set the current X value to the new lerp value to show movement progress
            else if (isMovingY)
                transform.position = new Vector2(transform.position.x, lerpValue);      //set the current Y value to the new lerp value to show movement progress

            timeElapsed += Time.deltaTime;  //update time elapsed value
        }
        else
        {
            lerpValue = endValue;   //once the lerp has reach the lerp duration, set the lerp value to the end value so that the position is rounded (snapped into place)

            if (isMovingX)
                transform.position = new Vector2(lerpValue, transform.position.y);      //set the current X value to the new lerp value to show movement progress
            else if (isMovingY)
                transform.position = new Vector2(transform.position.x, lerpValue);      //set the current Y value to the new lerp value to show movement progress
            
            timeElapsed = 0;        //reset elapsed time for next lerp movement

            //now that lerp is completed, reset move booleans
            isMovingX = false;
            isMovingY = false;

            moved = true;
        }
    }

    public bool GetMoved()
    {
        return moved;
    }

    public void SetMoved(bool value)
    {
        moved = value;
    }

    //Returns true if the position passed through is equal to any of the level's border positions, otherwise returns false
    private bool IsBorderCollision(Vector2 pos)
    {
        foreach (GameObject border in level.GetBorders())    //loops through the levels borders
            if (border.transform.position.x == pos.x && border.transform.position.y == pos.y)   //check is position passed through is equal to borders' poistions
                return true;        //if equal to one of the border's positions, returns true

        return false;               //if no borders' positions match the position passed through, then returns false
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 targetPos = new Vector2();      //used for determining the move the player is attempting to make before they make it

        if (!isMovingX && !isMovingY && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))            //if player isn't already moving in X or Y get input
        {
            targetPos = new Vector2(transform.position.x, transform.position.y + 1);        //Gets target position

            //if the player is trying to walk into the borders or the player is attempting to move onto the lock and it's active, the attempted move will not happen
            if (!IsBorderCollision(targetPos) || (targetPos == level.GetLockPos() && !level.IsLockActive()))
            {
                isMovingY = true;   //indicates that the player is started to move in the Y axis

                startValue = transform.position.y;      //set the start value for the lerp movement process
                endValue = transform.position.y + 1;    //set the end value for the lerp movement process
            }
        }

        if (isMovingX || isMovingY)     //if the player is currently moving, execute lerp
            LerpMove();     //calls move lerp method

        currentStringText.text = "Current Result: " + currentString;        //updates current string UI text;
    }
}
