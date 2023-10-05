using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An enum for setting the operator type
public enum OpType { none, add, sub, mult, div, mod };

public class Operator : MonoBehaviour
{
    public OpType opType;       //operator type
    private Player player;      //player object

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();     //get the player object in the scene
    }

    //return the OpType
    public OpType GetOpType()
    {
        return opType;
    }
    
}
