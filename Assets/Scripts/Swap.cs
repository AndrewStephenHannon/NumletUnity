using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swap : MonoBehaviour
{

    public GameObject valueObj1;        //Object of value to be swapped out
    public GameObject valueObj2;        //Object ot value to be swapped in
    private char value1;                //value to be swapped out
    private char value2;                //value to be swapped in

    // Start is called before the first frame update
    void Start()
    {
        value1 = valueObj1.GetComponent<Value>().GetValue();        //store valueObj1's value in value1
        value2 = valueObj2.GetComponent<Value>().GetValue();        //store valueObj2's value in value2
        
        valueObj1.transform.localScale = new Vector3(0.3f, 0.3f, 0.0f);     //set valueObj1's scale relative to the swap tile
        valueObj2.transform.localScale = new Vector3(0.3f, 0.3f, 0.0f);     //set valueObj2's scale relative to the swap tile
        
        valueObj1.transform.position = new Vector3(-0.25f, 0.25f, -0.5f) + transform.position;   //set valueObj1's position relative to the swap tile
        valueObj2.transform.position = new Vector3(0.25f, -0.25f, -0.5f) + transform.position;   //set valueObj2's position relative to the swap tile
    }

    //return value to be swapped out
    public char GetValue1()
    {
        return value1;
    }

    //return value to be swapped in
    public char GetValue2()
    {
        return value2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
