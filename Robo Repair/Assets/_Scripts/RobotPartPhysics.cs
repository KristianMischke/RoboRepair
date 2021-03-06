﻿using System.Collections;
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

    public int playerID = -2;

    // Update is called once per frame
    void Update()
    {
        if (!addedForce) {
            Debug.Log("Piece Spawned" + playerID);
            float intensity = Random.Range(0.1f, 1f);

            robotPart.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * intensity, ForceMode2D.Impulse);
            robotPart.AddTorque(Random.Range(-5f, 5f)*intensity, ForceMode2D.Impulse);
            addedForce = true;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerID == -2)
        {
            return;
        }
        //Debug.Log("AAAAAAAAAAAAA");
        CharacterController2D otherPlayer = collision.GetComponent<CharacterController2D>();


        if (otherPlayer != null) {
            int otherPlayerID = otherPlayer.playerID;
            Debug.Log(otherPlayerID);
            if (otherPlayerID != playerID) {
                //Debug.Log("Did it");
                otherPlayer.Heal();
                //this.gameObject.SetActive(false);
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
