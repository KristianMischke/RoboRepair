using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetCollider : MonoBehaviour
{
    public GameObject robotPart;

    private CircleCollider2D magnetRadius;

    public int playerID;

    // Start is called before the first frame update
    void Start()
    {
        playerID = robotPart.GetComponent<RobotPartPhysics>().playerID;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("AAAAAAAAAAAAA");
        CharacterController2D otherPlayer = collision.GetComponent<CharacterController2D>();


        if (otherPlayer != null)
        {
            int otherPlayerID = otherPlayer.playerID;
            //Debug.Log(otherPlayerID);
            if (otherPlayerID != playerID)
            {
                //todo Reverse the order of these positions, currently the pieces should repel from the robot for testing purposes
                Vector2 forceDirection = collision.transform.position - this.transform.position;
                GetComponentInParent<Rigidbody2D>().AddForce(forceDirection * 15, ForceMode2D.Force);

            }
        }
    }
}
