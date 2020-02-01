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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("AAAAAAAAAAAAA");
        CharacterController2D otherPlayer = collision.GetComponent<CharacterController2D>();


        if (otherPlayer != null) {
            int otherPlayerID = otherPlayer.playerID;
            Debug.Log(otherPlayerID);
            if (otherPlayerID != playerID) {
                //Debug.Log("Did it");
                otherPlayer.Heal();
                this.gameObject.SetActive(false);
            }
        }
    }
}
