using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPartPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    
    public Rigidbody2D robotPart;

    private BoxCollider2D boxCollider;

    private bool addedForce = false;

    // Update is called once per frame
    void Update()
    {
        if (!addedForce) {
            robotPart.AddForce(new Vector2(1f, 0f), ForceMode2D.Impulse);
            addedForce = true;
        }



    }
}
