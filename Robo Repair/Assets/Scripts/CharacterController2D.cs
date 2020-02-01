﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(Collider2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField]
    bool testWithKeyboard = false;

    [SerializeField]
    public GameObject partPrefab;

    [SerializeField]
    public Transform partParentTransform;

    public Rigidbody2D robot;

    // Movement params
    private Vector2 velocity;
    private Vector2 moveDir = Vector2.zero;
    private bool movePressed = false;

    // Attack params
    [SerializeField]
    private float meleeDistance = 2f;
    [SerializeField, Tooltip("amount of charge gained per second")]
    private float attackChargeRate = 0.5f;
    [SerializeField]
    private float attackMaxCharge = 3f;
    private float attackCharge = 0;
    private Vector2 attackDir = Vector2.zero;
    private bool attackPressed = false;

    public GameObject laserPrefab;
    public GameObject meleePrefab;
    public GameObject meleeMissPrefab;


    public bool isHit = false;
    bool hasCharged = false, stopWalkNoise = false;
    
    // health/gamelogic
    public int playerID = -1;

    // Changed to public so RobotPart can modify it
    public int hitpoints = 10;
    public const int MAX_HP = 20;

    HashSet<int> capturedPlayers = new HashSet<int>();

    // Visuals
    List<Sprite> bodySprites;
    List<Sprite> legSprites;
    List<Sprite> partSprites;
    int bodyIndex;
    int legIndex;
    SpriteRenderer bodyRenderer;
    SpriteRenderer legRenderer;
    float legFrameTimer = 0;
    Texture2D colorSwapTex;
    Color[] spriteColors;
    enum SwapIndex
    {
        
    }

    void SwapColor(SwapIndex index, Color color)
    {
        spriteColors[(int)index] = color;
        colorSwapTex.SetPixel((int)index, 0, color);
    }

    public float LegFrameInterval { get { return 0.1f; } }

    private void Start()
    {
        bodySprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.ROBOT_SPRITES + Random.Range(0, SpriteManager.NUM_ROBOTS));
        legSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.LEG_SPRITES);
        partSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.PART_SPRITES);

        bodyRenderer = transform.Find("RobotBody").GetComponent<SpriteRenderer>();
        legRenderer = transform.Find("RobotLegs").GetComponent<SpriteRenderer>();

        SpriteManager.InitColorSwapTex(out colorSwapTex, out spriteColors);
        bodyRenderer.material.SetTexture("_SwapTex", colorSwapTex);
        legRenderer.material.SetTexture("_SwapTex", colorSwapTex);

        legFrameTimer = LegFrameInterval;
    }

    private void Update()
    {

        // charge attack
        if (attackPressed)
        {
            attackCharge = Mathf.Min(attackCharge + Time.deltaTime * attackChargeRate, attackMaxCharge);
        }
        else
        {
            // not pressing attack joystick

            if (attackCharge > 0)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, attackDir);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != this.gameObject)
                    {
                        // first hit other than me
                        
                        // create laser
                        GameObject newLaser = Instantiate(laserPrefab);
                        newLaser.transform.position = Vector3.zero;
                        LineRenderer lineRenderer = newLaser.GetComponent<LineRenderer>();
                        lineRenderer.SetPosition(0, new Vector2(transform.position.x, transform.position.y));
                        lineRenderer.SetPosition(1, hit.point);
                        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f + attackCharge*0.5f;

                        // deal damage to player
                        CharacterController2D otherPlayer = hit.collider.GetComponent<CharacterController2D>();
                        if (otherPlayer != null)
                        {
                            otherPlayer.LaserHit(attackCharge);
                        }

                        //Debug.Log(hit.collider.gameObject.name);
                        break;
                    }
                }

                GameLogic.instance.soundGenerator.AddLaserSound();

                attackCharge = 0;
            }
        }

        if (attackCharge > 0 && !hasCharged) { GameLogic.instance.soundGenerator.AddLaserChargeSound(); hasCharged = true; }
        else if (attackCharge == 0) { hasCharged = false; }

        Vector2 direction = movePressed ? moveDir : Vector2.zero;

        // Use GetAxisRaw to ensure our input is either 0, 1 or -1.
        if (testWithKeyboard)
        {
            direction.x = Input.GetAxisRaw("Horizontal");
            direction.y = Input.GetAxisRaw("Vertical");
            moveDir = direction;

            
            attackPressed = Input.GetMouseButton(0);
            
            attackDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            attackDir.Normalize();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                DoMelee();
            }
        }
        
        // Horizontal movement
        if (direction.x != 0)
        {
            //velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInputX, acceleration * Time.deltaTime);
            velocity.x = direction.x;
            
        }
        else
        {
            //velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            velocity.x = 0;
        }

        // Vertical movement
        if (direction.y != 0)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed * direction.y, walkAcceleration * Time.deltaTime);
            velocity.y = direction.y;
        }
        else
        {
            velocity.y = Mathf.MoveTowards(velocity.y, 0, groundDeceleration * Time.deltaTime);
            velocity.y = 0;
        }

        if (direction != Vector2.zero)
        {
            stopWalkNoise = false;
            GameLogic.instance.soundGenerator.AddWalkSound();

            legFrameTimer -= Time.deltaTime;
            if (legFrameTimer <= 0)
            {
                legFrameTimer += LegFrameInterval;
                legIndex = ++legIndex % legSprites.Count;
            }
        }
        else if (stopWalkNoise == false) { GameLogic.instance.soundGenerator.StopWalkSound(); stopWalkNoise = true; }

        velocity.Normalize();
        robot.AddForce(velocity, ForceMode2D.Impulse);

        //if (Input.GetAxisRaw("Jump") != 0 && !isHit) {
        //    //isHit = true;
        //    GameObject robotPart = Instantiate(partPrefab, partParentTransform);

        //}

        bodyIndex = bodySprites.Count - Mathf.RoundToInt(bodySprites.Count * ((float)hitpoints / MAX_HP));

        bodyRenderer.sprite = bodySprites[bodyIndex];
        legRenderer.sprite = legSprites[legIndex];
    }

    public void ControllerAction(string ID, JToken data)
    {
        switch (ID)
        {
            case "joystick-left": // movement d-pad
                {
                    movePressed = (bool)data["pressed"];

                    if (movePressed)
                    {
                        moveDir.x = (float)(data["message"]["x"]);
                        moveDir.y = -(float)(data["message"]["y"]);
                    }
                }
                break;
            case "joystick-right": // shooting d-pad
                {
                    attackPressed = (bool)data["pressed"];

                    if (attackPressed)
                    {
                        attackDir.x = (float)(data["message"]["x"]);
                        attackDir.y = -(float)(data["message"]["y"]);
                    }
                }
                break;
            case "melee-attack-button":
                {
                    bool pressed = (bool)data["pressed"];
                    if (pressed)
                    {
                        DoMelee();
                    }
                }
                break;
        }
    }

    private void DoMelee()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, moveDir, meleeDistance);

        bool didHit = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != this.gameObject)
            {
                // first hit other than me

                // create laser
                GameObject newMelee = Instantiate(meleePrefab);
                newMelee.transform.position = hit.point;
                newMelee.transform.localScale = Vector3.one * 4 * hit.distance;

                // deal damage to player
                CharacterController2D otherPlayer = hit.collider.GetComponent<CharacterController2D>();
                if (otherPlayer != null)
                {
                    didHit = true;

                    if (otherPlayer.hitpoints == 0)
                    {
                        otherPlayer.gameObject.SetActive(false);
                        otherPlayer.ReleaseCapturedPlayers();
                        capturedPlayers.Add(otherPlayer.playerID);
                    }
                }

                //Debug.Log(hit.collider.gameObject.name);
                break;
            }
        }

        if (!didHit)
        {
            GameObject newMelee = Instantiate(meleeMissPrefab);
            newMelee.transform.position = transform.position + new Vector3(moveDir.x, moveDir.y);
        }

        GameLogic.instance.soundGenerator.AddSwordSound();
    }

    public void LaserHit(float laserPower)
    {
        int pointsLost = Mathf.CeilToInt(laserPower);
        hitpoints -= pointsLost;

        if (hitpoints < 0)
            hitpoints = 0;

        //todo spawn parts
        GameObject robotPart = Instantiate(partPrefab, partParentTransform);
        robotPart.GetComponent<RobotPartPhysics>().playerID = this.playerID;
    }

    public void ReleaseCapturedPlayers()
    {
        foreach (int playerID in capturedPlayers)
        {
            CharacterController2D capturedPlayer = GameLogic.instance.GetPlayerByID(playerID);
            if (capturedPlayer != null)
            {
                capturedPlayer.gameObject.SetActive(true);
                capturedPlayer.transform.position = transform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            }
        }

        GameLogic.instance.soundGenerator.AddRandomSound();
    }
}