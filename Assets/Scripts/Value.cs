using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Value : MonoBehaviour
{
    public char value;            //The value of the tile that this script is attached to

    //Returns the value of this tile
    public char GetValue()
    {
        return value;
    }
}
