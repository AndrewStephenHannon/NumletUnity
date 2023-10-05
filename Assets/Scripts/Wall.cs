using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private SpriteRenderer sRenderer;

    // Start is called before the first frame update
    void Start()
    {
        sRenderer = GetComponent<SpriteRenderer>();

        if (sRenderer != null)
            sRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
