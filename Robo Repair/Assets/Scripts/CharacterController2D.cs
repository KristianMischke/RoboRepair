using System.Collections;
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


    public bool isHit = false;
    // health/gamelogic
    public int playerID = -1;

    // Changed to public so RobotPart can modify it
    public int hitpoints = 10;

    HashSet<int> capturedPlayers = new HashSet<int>();

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

                attackCharge = 0;
            }
        }

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

        velocity.Normalize();
        robot.AddForce(velocity, ForceMode2D.Impulse);

        //if (Input.GetAxisRaw("Jump") != 0 && !isHit) {
        //    //isHit = true;
        //    GameObject robotPart = Instantiate(partPrefab, partParentTransform);
            
        //}
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
    }
}