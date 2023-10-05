using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private string currentString;   //current string value for player's puzzle progess
    private string prevString;      //previous string
    private bool isMovingX;         //a boolean that determines whether or not the player is currently moving in the X direction
    private bool isMovingY;         //a boolean that determines whether or not the player is currently moving in the Y direction    

    private float startValue;       //variable for indicating the start value of player movement for lerping purposes (can be used for either x or y)
    private float endValue;         //variable for indicating the end value of player movement for lerping purposes (can be used for either x or y)
    private float timeElapsed;      //current amount of time that has elapsed during player movement (lerping)
    public float lerpDuration;      //the amount of time the lerp for player movement takes
    private float lerpValue;        //keeps track of the lerp progress

    public GameObject levelObject;  //gets a reference to the current level object
    private Level level;            //gets a reference to the current level script

    private OpType currOperator;    //keeps track of the current operator the player is standing on, if any
    private char currValue;         //keeps track of current value the player is standing on, if any

    public GameObject[] numbers;    //a list of the numbers prefabs
    public GameObject[] letters;    //a list of the letter prefabs

    //public double maxValue;         //max value the player's result can be
    //public double minValue;         //max value the player's result can be
    public int maxStringLength;     //max length the player result can be
    public float ShakeSpeedInterval;    //interval that controls how often the player result characters orientation updates (essentially the speed at which the characters shake)
    public float maxShakeAmount;        //how much the player result characters shake around their central position

    private bool playerMoved;           //keep track of whether the player has made their first move or not

    public GameObject line;             //visual indicator for player to let them know they can't move back to the starting position of the level

    private List<Vector3> characterPositions;

    // Start is called before the first frame update
    void Start()
    {
        level = levelObject.GetComponent<Level>();      //gets the level object's script
        currentString = level.startString;              //instantiate player string value for the current level

        characterPositions = new List<Vector3>();       //stores the central positions of each character in the player result

        DisplayResult();        //Display Player's current result

        prevString = currentString;     //set prevString to currentString

        //instantiate move booleans
        isMovingX = false;
        isMovingY = false;

        playerMoved = false;
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
   
    //Function for executing the behaviour of the tile the player just moved to
    private void OnTileAction()
    {
        //check the level's list of tiles
        foreach(GameObject tile in level.tiles)
        {
            //if the tile in the list's position is equal to that of the player's, then the player is standing on this tile and action can be applied
            if (transform.position == tile.transform.position)
            {
                //if the tile's layer is SwapL# (layer 8), then apply the swap action that swaps letters to numbers and vice versa
                if(tile.layer == 8)
                {
                    currentString = ApplyLetterNumberSwapAction();      //swap the letters to numbers and numbers to letters in currentString
                    currOperator = OpType.none;                         //clear the operator
                    currValue = '!';                                    //clear value to be an unusable character
                }
                //if the tile is an operator type, then apply the operator
                else if(tile.GetComponent<Operator>() != null)
                {
                    currOperator = tile.GetComponent<Operator>().GetOpType();
                }
                //if the tile is a swap type, then apply the swap
                else if(tile.GetComponent<Swap>() != null)
                {
                    Swap swapObj = tile.GetComponent<Swap>();       //store the Swap object
                    currentString = ApplySwapAction(swapObj);       //call ApplySwapAction to perform swap action and store result in currentString

                    currOperator = OpType.none;                     //clear the operator
                    currValue = '!';                                //clear value to be an unusable character
                }
                //if the player string is empty and they land on a value tile, their string becomes that value
                else if(tile.GetComponent<Value>() != null && currentString.Length == 0)
                {
                    string tileValue = "" + tile.GetComponent<Value>().GetValue();  //store tile value as a string

                    //if the tile's value is '0', then it is a 10 tile
                    if (tileValue == "0")
                        tileValue = "10";           //store tile value as 10

                    //if the player has moved or the operator is add (for the sake of empty strings in letter levels), make the string equal the tile value
                    if(!playerMoved || currOperator ==  OpType.add)
                        currentString = tileValue;      //set player's current string to tileValue;
                }
                //if the tile is a value type and there is an operator, then apply the value and operator
                else if(tile.GetComponent<Value>() != null && currOperator != OpType.none)
                {
                    currValue = tile.GetComponent<Value>().GetValue();      //get value from tile

                    int digCounter = 0;             //initialize a counter to keep track of how many digits in a row for currString, to be used for calculations
                    bool isString = false;          //initialize a bool that determines if there is a letter in the string or not and therefore string manipulation needs to take place
                    //int letCounter = 0;             //initilize a counter to keep track of how many letters in a row ***review how calculations work on letter puzzles, might not need this counter ***

                    string tempString = "";         //keeps track of the player's string as it's being manipulated
                    
                    //take currString and apply currOperator and currValue to get new currString (new result)
                    for(int i = 0; i < currentString.Length; i++)
                    {
                        //if the current character in the string is a digit, increment the digit counter and reset the letter counter
                        if (char.IsDigit(currentString[i]))
                        {
                            digCounter++;       //increment digit counter
                        }
                        //if the current character in the string is a decimal and the next character in the string (if there is one) is a digit
                        //and the previous character in the string (if there is one) is a digit, increment the digit counter and reset the letter
                        //counter, otherwise it will not be a number
                        else if (currentString[i] == '.')// && i > 0 && char.IsDigit(currentString[i - 1]) && i < currentString.Length - 1 && char.IsDigit(currentString[i+1]))
                        {
                            //if there is a digit before the decimal and no digit after, count the decimal as a number
                            if (i > 0 && char.IsDigit(currentString[i - 1]) && i < currentString.Length - 1 && !char.IsDigit(currentString[i + 1]))
                                digCounter++;       //increment digit counter
                            //if there is a digit before and after the decimal, then count the decimal as a number
                            else if (i > 0 && char.IsDigit(currentString[i - 1]) && i < currentString.Length - 1 && char.IsDigit(currentString[i + 1]))
                                digCounter++;       //increment digit counter
                            //if there is a digit after the decimal and no digit before, count the decimal as a number
                            else if (i > 0 && !char.IsDigit(currentString[i - 1]) && i < currentString.Length - 1 && char.IsDigit(currentString[i + 1]))
                                digCounter++;       //increment digit counter
                            //if the decimal is the last character in the string and there is a digit before it, count the decimal as a number
                            else if (i > 0 && char.IsDigit(currentString[i - 1]) && i == currentString.Length - 1)
                                digCounter++;       //increment digit counter
                            //if the decimal is the first character and there is a digit after is, then count the decimal as a number
                            else if (i == 0 && i < currentString.Length - 1 && char.IsDigit(currentString[i + 1]))
                                digCounter++;       //increment digit counter
                            //for all other scenarios (non-digit before and after decimal, non-digit before and decimal is last character, or the
                            //decimal is the first character in the string and there is a non-digit after it) don't count it as a number
                            else
                                tempString += currentString[i];     //store character to tempString as is ('.')
                        }
                        //if the current character in the string is a letter, apply the calculations to the string of previous digits, if there is one and increment the letter counter
                        else if(char.IsLetter(currentString[i]) || currentString[i] == ' ')
                        {
                            //if there was a string of digits, apply calculations and store the result in tempString
                            tempString += ApplyCalculation(i, digCounter);
                            tempString += currentString[i];
                            digCounter = 0;     //reset digit counter
                            isString = true;
                        }
                        //if the current character is a negative, apply calculation as we reached the end of a number potentially
                        else if(currentString[i] == '-')
                        {
                            //if there was a string of digits, apply calculations and store the result in tempString
                            tempString += ApplyCalculation(i, digCounter);

                            //if there is a digit after the negative, set the number counter to 1
                            if(i < currentString.Length - 1 && char.IsDigit(currentString[i + 1]))
                                digCounter = 1;     //set the digit counter to 1
                            //if there is no digit following the negative, then store the negative in tempString and set the number counter to 0
                            else
                            {
                                tempString += currentString[i];     //stoer the negative as is
                                digCounter = 0;                     //reset the number counter to 0 as there is no digit detected in this scenario
                            }
                        }
                    }

                    //if there was a string of digits, apply calculations and store the result in tempString
                    //this call is for strings that are all digits or end in digits
                    tempString += ApplyCalculation(currentString.Length, digCounter);
                    currentString = tempString;     //store the resulting tempString as the new currString
                    //if there was a letter or space in currentString, apply manipulation and store the result in tempString
                    tempString = ApplyStringManipulation(isString);
                    currentString = tempString;     //store the resulting tempString as the new currString

                    currOperator = OpType.none;     //clears the operator after the value has been applied
                    playerMoved = true; //player has moved
                }
            }
        }
    }

    //Function for swapping letter to numbers and vice versa in the player's current string
    private string ApplyLetterNumberSwapAction()
    {
        string tempString = "";     //initialize a temporary string

        //cycle through the current string to see if the character is a letter or a number and convert it
        for (int i = 0; i < currentString.Length; i++)
        {
            //if the char is a letter, convert it to its corresponding number (i.e. A = 0, B = 1, c = 2, etc.)
            if (char.IsLetter(currentString[i]))
                tempString += (int)(currentString[i] - 65);        //store the result to the end of tempString
            //if the char is a digit check if the next char is a digit and attempt converting to corresponding letter
            else if (char.IsDigit(currentString[i]))
            {
                //if the digit isn't 0 and there's another character in the string and that next character is also a digit and the two
                //characters together as digits is less than 26, then convert them into the corresponding letter
                if (currentString[i] != '0' && i < currentString.Length - 1 && char.IsDigit(currentString[i+1]) &&
                    (((int)currentString[i] - 48) * 10 + (int)currentString[i+1] - 48) <= 25)
                {
                    tempString += (char)((((int)currentString[i] - 48) * 10 + (int)currentString[i + 1] - 48)+65);  //convert digits to corresponding letter
                    i++;        //since two digits were converted at once, increment i before current loop increments it again
                }
                //if two digits won't convert to a letter, try the one digit
                else
                {
                    tempString += (char)((currentString[i]) + 17);      //convert digit to corresponding letter
                }
            }
            //if the char is neither a letter nor a digit (i.e. a '-' or '.'), then just store it as is to the end of tempString
            else
                tempString += currentString[i];        //store as is to the end of tempString
        }

        return tempString;      //return the result
    }

    //Function for applying the swap action to the current string
    private string ApplySwapAction(Swap swapObj)
    {
        string tempString = "";         //initiate a temporary string

        //cycle through the characters in the currentString
        foreach(char c in currentString)
        {
            //if the current character in the string is equal the first swap value, then replace it with the second swap value
            if (c.Equals(swapObj.GetValue1()))
                tempString += swapObj.GetValue2();
            //if the current character in the string is equal the second swap value, then replace it with the first swap value
            else if (c.Equals(swapObj.GetValue2()))
                tempString += swapObj.GetValue1();
            //if the current character is the string is not equal to the swap out value, then just store it as is to the tempString
            else
                tempString += c;
        }
        return tempString;      //return the result
    }

    //Function for applying the string manipulation on the current string of characters when an operation is applied
    private string ApplyStringManipulation(bool isString)
    {
        string tempString = "";             //initialize a string 

        //if there was a letter and the currValue is not a number, then apply the string manipulation
        if (isString && !int.TryParse("" + currValue, out int intVersion))
        {
            if (currOperator == OpType.add)
            {        //if the operator is add, then add the currValue at the end of the string
                if (currentString.Length >= maxStringLength)
                    tempString = currentString;
                else
                    tempString = currentString + currValue;     //store result to tempString
            }
            else if (currOperator == OpType.sub)    //if the operator is sub, then remove the last occurrence of currValue from the string
            {
                int letterIndex = -1;           //initialize an int that will be used to determine the index of currValue in the string if it exists (-1 if it doesn't)

                //cycle through the current string
                for (int i = 0; i < currentString.Length; i++)
                {
                    //if char at i is equal to currValue, record it's index
                    if (currentString[i] == currValue)
                        letterIndex = i;
                }

                //if letterIndex >= 0 (currValue is in string), remove it from the string
                if (letterIndex >= 0)
                    tempString = currentString.Substring(0, letterIndex) + currentString.Substring(letterIndex + 1);
                //if letterIndex < 0 (currValue is not in string), leave string as is
                else
                    tempString = currentString;
            }
            else if (currOperator == OpType.mult)   //if the operator is mult, then add currValue to the end of each word in currrentString (each word is separated by a space)
            {
                if (currentString.Length >= maxStringLength)
                {
                    tempString = currentString;
                }
                else
                {

                    bool addMore = true;        //A boolean for letting the multiplication add the letter to as many words in currentString as character limit will allow

                    //cycle through the current string
                    for (int i = 0; i < currentString.Length; i++)
                    {
                        //if a space is detected in the string, then add the currValue to the end of the previous word
                        if (currentString[i] == ' ' && addMore)
                        {
                            //a currValue before the space
                            tempString += currValue;

                            //if the remainder of the string's length plus the tempString's length is 15 in length or greater, we can't keep adding letters to the string
                            if ((tempString.Length + currentString.Substring(i, currentString.Length - i).Length) >= maxStringLength)
                                addMore = false;
                            //Continue to add the next character in the String
                            tempString += currentString[i];

                        }
                        //if not a space, just keep adding each letter to the tempString
                        else
                        {
                            tempString += currentString[i];
                        }
                    }

                    //add the currValue to the end of the string (if it doesn't end in a space), to account for the last word in the string
                    if (currentString[currentString.Length - 1] != ' ' && tempString.Length < maxStringLength)
                        tempString += currValue;
                }
            }
            else if (currOperator == OpType.div)     //if the operator is a div, then remove every occurence of currValue
            {
                //cycle through the current string
                for (int i = 0; i < currentString.Length; i++)
                {
                    //if the current character in the string is the same as currValue, don't store in tempString
                    if (currentString[i] != currValue)
                        tempString += currentString[i];
                }
            }
            else if (currOperator == OpType.mod)     //if the operator is a mod, then get the mod of each letter with currValue as if the letters were their number counterparts
            {
                //cycle through each character in the currentString
                foreach (char c in currentString)
                {
                    //if the character is a letter, mod it with currValue
                    if (char.IsLetter(c))
                    {
                        int charValue;
                        //this takes the letter and turns it into a corresponding int (A = 0, B = 1, C = 2 etc.)
                        if(currValue - 65 == 0)
                            charValue = ((c - 65) % 1 + 65);
                        else
                            charValue = ((c - 65) % (currValue - 65) + 65);

                        //if the charValue is a letter, turn back into a char and store it in tempString
                        if (charValue > 64 && charValue < 91)
                            tempString += (char)charValue;     //add the result to the end of tempString
                    }
                    else
                        //if the character is not a letter, then just add it to the end of the tempString
                        tempString += c;
                }
            }
            
            return tempString;              //return the string
        }
        else
            return currentString;           //if there are no letters in the string, then return the string as is
    }

    //Function for calculating a string of digits as an int (change to float later) and storing result back as a string
    private string ApplyCalculation(int index, int digCounter)
    {
        //if there were digits in the string and the tile value is a number and there is a current operator to apply, then apply the calculation
        if (digCounter > 0 && int.TryParse("" + currValue, out int intVersion))// && currOperator != OpType.none)  //intVersion is an int used for a TryParse that tries to parse the tile value into number
        {
            //no 0 tiles in the game, but there are 10 tiles, so to work around this (since currValue is a single character) have 0 digit represent 10
            if (intVersion == 0)
                intVersion = 10;

            double tempNumber = double.Parse(currentString);     //turns the detected substring into a number

            if (currOperator == OpType.add)             //if the operator is add, then add the tile value to the substring of digits
                tempNumber = tempNumber + (double)intVersion;   //store result to tempNumber
            else if (currOperator == OpType.sub)        //if the operator is sub, then subtract the tile value from the substring of digits
                tempNumber = tempNumber - (double)intVersion;   //store result to tempNumber
            else if (currOperator == OpType.mult)       //if the operator is mult, then multiply the tile value with the substring of digits
                tempNumber = tempNumber * (double)intVersion;   //store result to tempNumber
            else if (currOperator == OpType.div)        //if the operator is div, then divide the substring of digits by the tile value
                tempNumber = tempNumber / (double)intVersion;   //store result to tempNumber
            else if (currOperator == OpType.mod)        //if the operator is mod, then get the remainder of the tile value when divided into the substring of digits
                tempNumber = tempNumber % (double)intVersion;   //store result to tempNumber

            int accNegative = 0;        //set a variable that will accommodate string size when the number is negative

            if (tempNumber < 0)
                accNegative = 1;

            //if player result is below 1, concat value to max string length minus 1 (to accommodate for leading 0)
            if (Mathf.Abs((float)tempNumber) < 1)
                return tempNumber.ToString("0." + new string('#', maxStringLength - 1));
            //if player result is greater in length than max string length, cut last decimal digits
            else if (tempNumber.ToString().Length > maxStringLength)
            {
                if (((long)tempNumber).ToString().Length > maxStringLength + accNegative)
                    return currentString;
                else if (((long)tempNumber).ToString().Length == maxStringLength + accNegative)
                    return ((long)tempNumber).ToString();
                else
                    return tempNumber.ToString().Substring(0, maxStringLength + 1 + accNegative);
            }
            else
                return tempNumber.ToString();       //return the resulting number as a string
        }
        else
        {
            print(index);
            print(digCounter);
            return currentString.Substring(index - digCounter, digCounter);     //return the substring as is as the calculation could not take place
        }
    }

    //Function for handling Player the movement lerp
    private void LerpMove()
    {
        if (timeElapsed < lerpDuration)      //if the current time elapsed is less than the lerp duration, then continue executing lerp movement
        {
            lerpValue = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);   //set the lerp value using the start value end value and current time elapsed over the lerp duration

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

            if(level != null)
                OnTileAction();         //execute the action that should take place once movement is complete

            timeElapsed = 0;        //reset elapsed time for next lerp movement

            //now that lerp is completed, reset move booleans
            isMovingX = false;
            isMovingY = false;

            //once the player moves their first move, the line is set to active
            if (!line.active)
                line.active = true;
        }
    }

    //Returns true if the position passed through is equal to any of the level's border positions, otherwise returns false
    private bool IsBorderCollision(Vector2 pos)
    {
        foreach (GameObject border in level.GetBorders())    //loops through the levels borders
            if (border.transform.position.x == pos.x && border.transform.position.y == pos.y)   //check is position passed through is equal to borders' poistions
                return true;        //if equal to one of the border's positions, returns true

        return false;               //if no borders' positions match the position passed through, then returns false
    }

    //function that handles displaying the player's current result using animated characters
    private void DisplayResult()
    {
        GameObject[] currChars = GameObject.FindGameObjectsWithTag("Clone");    //get all currently displayed characters
        GameObject character;       //temp character gameobject for setting character tag

        //destroy all currently displayed characters
        foreach (GameObject c in currChars)
        {
            DestroyImmediate(c);
            characterPositions = new List<Vector3>();
        }

        //for each character in currentString intantiate an object of that character to display the current player result
        for(int i=0; i<currentString.Length; i++)
        {
            //if the currenString is is a negative number, display the first character as a negative symbol
            if (currentString[i] == '-')
            {
                //set the negative symbol to a position relative to where it lies in the string
                character = Instantiate(numbers[11], new Vector3((currentString.Length - 1) * (-0.125f) + (i * 0.25f), -3, 0), Quaternion.identity);
                character.tag = "Clone";        //set the character tag for detecting when deleting
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
            //if the character is a number (using ascii range), instantiate the number by converting the ascii value to the numbers array index
            else if (48 <= System.Convert.ToInt32(currentString[i]) && System.Convert.ToInt32(currentString[i]) <= 57)
            {
                //set the digit to a position relative to where it lies in the string
                character = Instantiate(numbers[System.Convert.ToInt32(currentString[i]) - 48], new Vector3((currentString.Length-1)*(-0.125f)+(i*0.25f), -3, 0), Quaternion.identity);
                character.tag = "Clone";        //set the character tag for detecting when deleting
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
            //if the character is a letter (using ascii range), instantiate the letter by converting the ascii value to the letters array index
            else if(65 <= System.Convert.ToInt32(currentString[i]) && System.Convert.ToInt32(currentString[i]) <= 90)
            {
                //set the letter to a position relative to where it lies in the string
                character = Instantiate(letters[System.Convert.ToInt32(currentString[i]) - 65], new Vector3((currentString.Length - 1) * (-0.125f) + (i * 0.3f), -3, 0), Quaternion.identity);
                character.tag = "Clone";        //set the character tag for detecting when deleting
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
            else if(System.Convert.ToInt32(currentString[i]) == 46) //if the character is a decimal, instantiate the decimal object
            {
                //set the decimal to a position relative to where it lies in the string (decimal is currently 10 in the array)
                character = Instantiate(numbers[10], new Vector3((currentString.Length - 1) * (-0.125f) + (i * 0.25f), -3, 0), Quaternion.identity);
                character.tag = "Clone";        //set the character tag for detecting when deleting
                character.transform.localScale = new Vector3(0.4f, 0.4f, 1);    //set scale for character for display purposes
            }
        }

        currChars = GameObject.FindGameObjectsWithTag("Clone");    //get all currently displayed characters

        //store each player result character's central position for reference when character shake occurs
        foreach (GameObject c in currChars)
            characterPositions.Add(c.transform.position);
    }

    //A function that checks if the Player's current result has only digits
    private (bool, double) IsNumber()
    {
        //Tries to convert the player's current string to a double. If it can, then the player string contains all digits and function returns true with result, otherwise function returns false
        if (double.TryParse(currentString, out double result))
            return (true, result);
        else
            return (false, 9999999999999999);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 targetPos = new Vector2();      //used for determining the move the player is attempting to make before they make it

        if(!isMovingX && !isMovingY && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))            //if player isn't already moving in X or Y get input
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
        else if (!isMovingX && !isMovingY && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)))    //if player isn't already moving in X or Y get input
        {
            targetPos = new Vector2(transform.position.x - 1, transform.position.y);        //Gets target position

            //if the player is trying to walk into the borders or the player is attempting to move onto the lock and it's active, the attempted move will not happen
            if (!IsBorderCollision(targetPos) || (targetPos == level.GetLockPos() && !level.IsLockActive()))
            {
                isMovingX = true;   //indicates that the player is started to move in the X axis

                startValue = transform.position.x;      //set the start value for the lerp movement process
                endValue = transform.position.x - 1;    //set the end value for the lerp movement process
            }
        }
        else if (!isMovingX && !isMovingY && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))    //if player isn't already moving in X or Y get input
        {
            targetPos = new Vector2(transform.position.x, transform.position.y - 1);        //Gets target position

            //if the player is trying to walk into the borders or the player is attempting to move onto the lock and it's active, the attempted move will not happen
            if (!IsBorderCollision(targetPos) || (targetPos == level.GetLockPos() && !level.IsLockActive()))
            {
                isMovingY = true;   //indicates that the player is started to move in the Y axis

                startValue = transform.position.y;      //set the start value for the lerp movement process
                endValue = transform.position.y - 1;    //set the end value for the lerp movement process
            }
        }
        else if (!isMovingX && !isMovingY && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)))   //if player isn't already moving in X or Y get input
        {
            targetPos = new Vector2(transform.position.x + 1, transform.position.y);        //Get target position

            //if the player is trying to walk into the borders or the player is attempting to move onto the lock and it's active, the attempted move will not happen
            if (!IsBorderCollision(targetPos) || (targetPos == level.GetLockPos() && !level.IsLockActive()))
            {
                isMovingX = true;   //indicates that the player is started to move in the X axis

                startValue = transform.position.x;      //set the start value for the lerp movement process
                endValue = transform.position.x + 1;    //set the end value for the lerp movement process
            }
        }

        if (isMovingX || isMovingY)    //if the player is currently moving, execute lerp
            LerpMove();     //calls move lerp method

        //if prevString isn't equal to currentString, then the player string was updated and the display string needs to be updated
        if(prevString != currentString)
            DisplayResult();        //Display Player's current result
        
        //ShakeCharacters();             //Update characters

        prevString = currentString;     //set prevString to currentString
    }
}