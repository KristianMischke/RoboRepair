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

    int hitpoints = 12;
    public const int MAX_HP = 20;

    HashSet<int> capturedPlayers = new HashSet<int>();

    [SerializeField]
    HPScript myHPBar;

    // Visuals
    List<Sprite> bodySprites;
    List<Sprite> legSprites;
    List<Sprite> wireSprites;
    List<Sprite> partSprites;
    List<Sprite> shinySprites;
    int bodyIndex;
    int legIndex;
    int wireIndex;
    int shinyIndex;
    SpriteRenderer bodyRenderer;
    SpriteRenderer legRenderer;
    SpriteRenderer wireRenderer;
    SpriteRenderer shinyRenderer;
    float legFrameTimer = 0;
    float wireFrameTimer = 0;
    float shinyTimer = 0;
    Texture2D colorSwapTex;
    Color[] spriteColors;
    public enum SwapIndex
    {
        Metal0 = 70,
        Metal1 = 83,
        Metal2 = 115,
        Metal3 = 140,
        Metal4 = 185,
        Metal5 = 221,

        Glow0 = 255,
        Glow1 = 252,
        Glow2 = 209,
        Glow3 = 153,

        WireA = 1,
        WireB = 2
    }

    void SwapColor(SwapIndex index, Color color)
    {
        spriteColors[(int)index] = color;
        colorSwapTex.SetPixel((int)index, 0, color);
    }

    public float LegFrameInterval { get { return 0.03f; } }
    public float WireFrameInterval { get { return 0.25f; } }

    private void Start()
    {
        bodySprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.ROBOT_SPRITES + Random.Range(0, SpriteManager.NUM_ROBOTS));
        legSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.LEG_SPRITES);
        wireSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.WIRE_SPRITES);
        partSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.PART_SPRITES);
        shinySprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.SHINY_SPRITES);

        bodyRenderer = transform.Find("RobotBody").GetComponent<SpriteRenderer>();
        legRenderer = transform.Find("RobotLegs").GetComponent<SpriteRenderer>();
        wireRenderer = transform.Find("RobotWires").GetComponent<SpriteRenderer>();
        shinyRenderer = transform.Find("RobotShiny").GetComponent<SpriteRenderer>();


        SpriteManager.InitColorSwapTex(out colorSwapTex, out spriteColors, bodySprites[0].texture);
        bodyRenderer.material.SetTexture("_SwapTex", colorSwapTex);
        legRenderer.material.SetTexture("_SwapTex", colorSwapTex);
        wireRenderer.material.SetTexture("_SwapTex", colorSwapTex);
        shinyRenderer.material.SetTexture("_SwapTex", colorSwapTex);

        //DO SWAP
        SwapColor(SwapIndex.WireA, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
        SwapColor(SwapIndex.WireB, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));

        Vector3 metalOffset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        Color glowOffset = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f));
        Color baseGlow = spriteColors[(int)SwapIndex.Glow3];
        
        for (SwapIndex i = 0; i < (SwapIndex)256; i++)
        {
            Color refColor = colorSwapTex.GetPixel((int)i, 0);
            if (i == SwapIndex.Metal0
                || i == SwapIndex.Metal1
                || i == SwapIndex.Metal2
                || i == SwapIndex.Metal3
                || i == SwapIndex.Metal4
                || i == SwapIndex.Metal5)
            {
                Color newColor = new Color(Mathf.Clamp(refColor.r + metalOffset.x, 0, 1), Mathf.Clamp(refColor.g + metalOffset.y, 0, 1), Mathf.Clamp(refColor.b + metalOffset.z, 0, 1));
                SwapColor(i, newColor);
            }
            if (i == SwapIndex.Glow0
                || i == SwapIndex.Glow1
                || i == SwapIndex.Glow2
                || i == SwapIndex.Glow3)
            {
                Color newColor = glowOffset + (refColor - baseGlow);
                newColor.a = 1f;
                SwapColor(i, newColor);
            }
        }
        colorSwapTex.Apply();

        legFrameTimer = LegFrameInterval;
        wireFrameTimer = WireFrameInterval;

        myHPBar.UpdateHP(hitpoints);
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
                    if (hit.collider.gameObject != this.gameObject && hit.collider.GetComponent<RobotPartPhysics>() == null)
                    {
                        // first hit other than me
                        
                        // create laser
                        GameObject newLaser = Instantiate(laserPrefab);
                        newLaser.transform.position = new Vector2(transform.position.x, transform.position.y);
                        SpriteRenderer sr = newLaser.GetComponent<SpriteRenderer>();
                        newLaser.transform.Rotate(Vector3.forward, Mathf.Tan(attackDir.y / attackDir.x) * 180 / Mathf.PI + (attackDir.x < 0 ? 180 : 0));
                        sr.size = new Vector2(hit.distance, 1);
                        DespawnAnimation dAnim = newLaser.GetComponent<DespawnAnimation>();
                        dAnim.FrameTimer = 0.5f;
                        dAnim.spriteList = SpriteManager.instance.GetModifiedSprites(SpriteManager.LASER_SPRITES);
                        dAnim.spriteIndex = dAnim.spriteList.Count - 1;

                        // deal damage to player
                        CharacterController2D otherPlayer = hit.collider.GetComponent<CharacterController2D>();
                        if (otherPlayer != null)
                        {
                            otherPlayer.Hit(attackCharge);
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

            wireFrameTimer -= Time.deltaTime; // double up on wire frame time when walking
        }
        else if (stopWalkNoise == false) { GameLogic.instance.soundGenerator.StopWalkSound(); stopWalkNoise = true; }

        wireFrameTimer -= Time.deltaTime;
        if (wireFrameTimer <= 0)
        {
            wireFrameTimer += WireFrameInterval;
            wireIndex = ++wireIndex % wireSprites.Count;
        }

        shinyRenderer.gameObject.SetActive(false);
        if (hitpoints == MAX_HP)
        {
            shinyTimer -= Time.deltaTime;

            if (shinyTimer <= 0)
            {
                shinyIndex = Mathf.FloorToInt(Mathf.Abs(shinyTimer) * 16);

                if (shinyIndex >= shinySprites.Count)
                {
                    shinyTimer = Random.Range(2f, 3f);
                }
                else
                {
                    shinyRenderer.sprite = shinySprites[shinyIndex];
                    shinyRenderer.gameObject.SetActive(true);
                }
            }
        }

        velocity.Normalize();
        robot.AddForce(velocity, ForceMode2D.Impulse);

        //if (Input.GetAxisRaw("Jump") != 0 && !isHit) {
        //    //isHit = true;
        //    GameObject robotPart = Instantiate(partPrefab, partParentTransform);

        //}

        bodyIndex = Mathf.Clamp(bodySprites.Count - Mathf.RoundToInt(bodySprites.Count * ((float)hitpoints / MAX_HP)), 0, bodySprites.Count-1);

        bodyRenderer.sprite = bodySprites[bodyIndex];
        legRenderer.sprite = legSprites[legIndex];
        wireRenderer.sprite = wireSprites[wireIndex];
        wireRenderer.gameObject.SetActive(bodyIndex >= bodySprites.Count - 2);

        Vector3 scale = Vector3.one;
        scale.x = moveDir.x < 0 ? -1 : 1;
        bodyRenderer.transform.localScale = scale;
        legRenderer.transform.localScale = scale;
        wireRenderer.transform.localScale = scale;
        shinyRenderer.transform.localScale = scale;
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

                // deal damage to player
                CharacterController2D otherPlayer = hit.collider.GetComponent<CharacterController2D>();
                if (otherPlayer != null)
                {
                    didHit = true;
                    otherPlayer.Hit(3);

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

        // create melee
        GameObject newMelee = Instantiate(meleePrefab);
        newMelee.transform.position = transform.position + (Vector3)moveDir.normalized*0.5f;
        newMelee.transform.localScale = Vector3.one;
        newMelee.transform.Rotate(new Vector3(0, 0, Mathf.Tan(moveDir.normalized.y / moveDir.normalized.x) * 180 / Mathf.PI + (moveDir.normalized.x < 0 ? 180 : 0)));
        DespawnAnimation dAnim = newMelee.GetComponent<DespawnAnimation>();
        dAnim.spriteList = SpriteManager.instance.GetModifiedSprites(SpriteManager.MELEE_SPRITES);
        dAnim.incrementIndex = true;
        dAnim.FrameTimer = 1 / 20f;

        GameLogic.instance.soundGenerator.AddSwordSound();
    }

    public void Hit(float laserPower)
    {
        int pointsLost = Mathf.CeilToInt(laserPower);

        if (hitpoints > 0 && Random.Range(0f, 1f) < 0.45f)
        {
            for (int i = 0; i < pointsLost; i++)
            {
                GameObject robotPart = Instantiate(partPrefab, GameLogic.instance.playerParentTransform);
                robotPart.transform.position = transform.position;
                SpriteRenderer sr = robotPart.GetComponent<SpriteRenderer>();
                sr.sprite = partSprites[Random.Range(0, partSprites.Count)];
                sr.material.SetTexture("_SwapTex", colorSwapTex);
                robotPart.GetComponent<RobotPartPhysics>().playerID = this.playerID;
            }
        }

        hitpoints -= pointsLost;

        if (hitpoints < 0)
            hitpoints = 0;

        myHPBar.UpdateHP(hitpoints);
    }

    public void Heal(int amount = 1)
    {
        hitpoints += amount;
        if (hitpoints > MAX_HP)
            hitpoints = MAX_HP;
        myHPBar.UpdateHP(hitpoints);
    }

    public void ReleaseCapturedPlayers()
    {
        foreach (int playerID in capturedPlayers)
        {
            CharacterController2D capturedPlayer = GameLogic.instance.GetPlayerByID(playerID);
            if (capturedPlayer != null)
            {
                capturedPlayer.gameObject.SetActive(true);
                capturedPlayer.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                capturedPlayer.Heal(8);
            }
        }

        GameLogic.instance.soundGenerator.AddRandomSound();
    }
}