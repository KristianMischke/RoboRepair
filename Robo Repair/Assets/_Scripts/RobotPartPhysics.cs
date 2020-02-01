using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RobotPartPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    
    public Rigidbody2D robotPart;

    private BoxCollider2D boxCollider;

    private bool addedForce = false;

    public int playerID;

    // Update is called once per frame
    void Update()
    {
        if (!addedForce) {
            robotPart.AddForce(new Vector2(1f, 0f), ForceMode2D.Impulse);
            addedForce = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController2D otherPlayer = collision.GetComponent<CharacterController2D>();
        int otherPlayerID = otherPlayer.playerID;

        if (otherPlayer != null) {
            if (otherPlayerID != playerID) {

            }
        }
    }
}
