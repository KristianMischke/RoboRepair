using UnityEngine;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
    float jumpHeight = 4;

    [SerializeField]
    bool testWithKeyboard = false;

    [SerializeField]
    public GameObject partPrefab;

    [SerializeField]
    public Transform partParentTransform;

    public Rigidbody2D robot;

    private BoxCollider2D boxCollider;

    private Vector2 velocity;

    private float moveInputX;
    private float moveInputY;

    public bool isHit = false;

    private void Awake()
    {      
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Use GetAxisRaw to ensure our input is either 0, 1 or -1.
        if (testWithKeyboard)
        {
            moveInputX = Input.GetAxisRaw("Horizontal");
            moveInputY = Input.GetAxisRaw("Vertical");
        }
        
        // Horizontal movement
        if (moveInputX != 0)
        {
            //velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInputX, acceleration * Time.deltaTime);
            velocity.x = moveInputX;
            
        }
        else
        {
            //velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            velocity.x = 0;
        }

        // Vertical movement
        if (moveInputY != 0)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed * moveInputY, walkAcceleration * Time.deltaTime);
            velocity.y = moveInputY;
        }
        else
        {
            velocity.y = Mathf.MoveTowards(velocity.y, 0, groundDeceleration * Time.deltaTime);
            velocity.y = 0;
        }

        robot.AddForce(velocity, ForceMode2D.Impulse);

        if (Input.GetAxisRaw("Jump") != 0 && !isHit) {
            //isHit = true;
            GameObject robotPart = Instantiate(partPrefab, partParentTransform);
            
        }
    }

    public void ControllerAction(string ID, JToken data)
    {
        switch (ID)
        {
            case "dpadrelative-left":
                {
                    bool pressed = (bool)data["pressed"];

                    switch ((string)(data["message"]["direction"]))
                    {
                        case "up":      moveInputY = pressed ? 1 : 0; break;
                        case "down":    moveInputY = pressed ? -1 : 0; break;
                        case "left":    moveInputX = pressed ? -1 : 0; break;
                        case "right":   moveInputX = pressed ? 1 : 0; break;
                    }
                }
                break;
        }
    }
}