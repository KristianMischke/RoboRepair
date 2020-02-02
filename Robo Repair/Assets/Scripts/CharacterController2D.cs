using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;


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
    public GameObject dustPrefab;

    public Rigidbody2D robot;

    // Movement params
    private Vector2 velocity;
    private Vector2 moveDir = Vector2.zero;
    private bool movePressed = false;

    // Attack params
    [SerializeField]
    private float meleeDistance = 2f;
    private float meleeAttackTimer = 0;
    private float meleeAttackDelay { get { return 0.8f; } }
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


    bool isBlinking = false;
    float blinkTimer = 0;

    bool hasCharged = false, stopWalkNoise = false;
    
    // health/gamelogic
    public int playerID = -1;

    int hitpoints = 12;
    public const int MAX_HP = 20;

    HashSet<int> capturedPlayers = new HashSet<int>();

    [SerializeField]
    HPScript myHPBar;

    [SerializeField]
    TMP_Text myText;

    // Visuals
    List<Sprite> bodySprites;
    List<Sprite> legSprites;
    List<Sprite> wireSprites;
    List<Sprite> partSprites;
    List<Sprite> shinySprites;
    List<Sprite> chargeSprites;
    int bodyIndex;
    int legIndex;
    int wireIndex;
    int shinyIndex;
    SpriteRenderer bodyRenderer;
    SpriteRenderer legRenderer;
    SpriteRenderer wireRenderer;
    SpriteRenderer shinyRenderer;
    SpriteRenderer chargeRenderer;
    LineRenderer crosshair;
    float legFrameTimer = 0;
    float wireFrameTimer = 0;
    float shinyTimer = 0;
    Texture2D mainColorSwapTex;
    Color[] spriteColors;
    Texture2D justRobotSwapTex;
    public enum SwapIndex
    {
        Metal0 = 70,
        Metal1 = 83,
        Metal2 = 115,
        Metal3 = 140,
        Metal4 = 185,
        Metal5 = 221,

        Glow0 = 254,
        Glow1 = 255,
        Glow2 = 252,
        Glow3 = 209,
        Glow4 = 153,

        WireA = 1,
        WireB = 2
    }

    void SwapColor(SwapIndex index, Color color)
    {
        spriteColors[(int)index] = color;
        mainColorSwapTex.SetPixel((int)index, 0, color);
    }

    void SetRobotColor(Color color, bool useBase)
    {
        for (int i = 0; i < 255; i++)
        {
            if (useBase)
            {
                justRobotSwapTex.SetPixel(i, 0, spriteColors[i]);
            }
            else
            {
                justRobotSwapTex.SetPixel(i, 0, color);
            }
        }

        justRobotSwapTex.Apply();
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
        chargeSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.CHARGE_SPRITES);

        bodyRenderer = transform.Find("RobotBody").GetComponent<SpriteRenderer>();
        legRenderer = transform.Find("RobotLegs").GetComponent<SpriteRenderer>();
        wireRenderer = transform.Find("RobotWires").GetComponent<SpriteRenderer>();
        shinyRenderer = transform.Find("RobotShiny").GetComponent<SpriteRenderer>();
        chargeRenderer = transform.Find("ChargeFlare").GetComponent<SpriteRenderer>();
        crosshair = transform.Find("Crosshair").GetComponent<LineRenderer>();


        SpriteManager.InitColorSwapTex(out mainColorSwapTex, out spriteColors, bodySprites[0].texture);
        SpriteManager.InitColorSwapTex(out justRobotSwapTex, out _, bodySprites[0].texture);
        bodyRenderer.material.SetTexture("_SwapTex", justRobotSwapTex);
        legRenderer.material.SetTexture("_SwapTex", justRobotSwapTex);
        wireRenderer.material.SetTexture("_SwapTex", justRobotSwapTex);
        shinyRenderer.material.SetTexture("_SwapTex", justRobotSwapTex);

        //DO SWAP
        SwapColor(SwapIndex.WireA, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
        SwapColor(SwapIndex.WireB, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));

        float metalHue = Random.Range(0f, 1f);
        float glowHue = Random.Range(0f, 1f);
        
        for (SwapIndex i = 0; i < (SwapIndex)256; i++)
        {
            Color refColor = spriteColors[(int)i];
            if (i == SwapIndex.Metal0
                || i == SwapIndex.Metal1
                || i == SwapIndex.Metal2
                || i == SwapIndex.Metal3
                || i == SwapIndex.Metal4
                || i == SwapIndex.Metal5)
            {
                Color.RGBToHSV(refColor, out float H, out float S, out float V);
                H = metalHue;
                Color newColor = Color.HSVToRGB(H, S, V);
                SwapColor(i, newColor);
            }
            if (i == SwapIndex.Glow0
                || i == SwapIndex.Glow1
                || i == SwapIndex.Glow2
                || i == SwapIndex.Glow3
                || i == SwapIndex.Glow4)
            {
                Color.RGBToHSV(refColor, out float H, out float S, out float V);
                H = glowHue;
                Color newColor = Color.HSVToRGB(H, S, V);
                SwapColor(i, newColor);

                if (i == SwapIndex.Glow4)
                {
                    crosshair.startColor = newColor;
                    chargeRenderer.material.color = newColor;
                }
                if (i == SwapIndex.Glow0)
                {
                    crosshair.endColor = newColor;
                }
            }
        }
        mainColorSwapTex.Apply();
        SetRobotColor(Color.clear, true);

        legFrameTimer = LegFrameInterval;
        wireFrameTimer = WireFrameInterval;

        myHPBar.UpdateHP(hitpoints);

        //myText.text = "Player " + playerID;

        myText.SetText("Player " + playerID);
    }

    private void Update()
    {

        crosshair.enabled = attackPressed;
        chargeRenderer.gameObject.SetActive(attackPressed);
        // charge attack
        if (attackPressed)
        {
            attackCharge = Mathf.Min(attackCharge + Time.deltaTime * attackChargeRate, attackMaxCharge);

            crosshair.SetPosition(0, transform.position + (Vector3)attackDir.normalized * 0.4f);
            crosshair.SetPosition(1, transform.position + (Vector3)attackDir.normalized * 1f);
            crosshair.startWidth = crosshair.endWidth = Mathf.Clamp01(attackCharge / attackMaxCharge) * 0.3f;

            chargeRenderer.sprite = chargeSprites[Mathf.FloorToInt(Mathf.Clamp01(attackCharge / attackMaxCharge) * (chargeSprites.Count - 1))];
        }
        else
        {
            // not pressing attack joystick

            if (attackCharge > 0)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, attackDir);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != this.gameObject && hit.collider.GetComponent<RobotPartPhysics>() == null && hit.collider.GetComponent<MagnetCollider>() == null)
                    {
                        // first hit other than me
                        
                        // create laser
                        GameObject newLaser = Instantiate(laserPrefab);
                        newLaser.transform.position = new Vector2(transform.position.x, transform.position.y);
                        SpriteRenderer sr = newLaser.GetComponent<SpriteRenderer>();
                        newLaser.transform.Rotate(Vector3.forward, Vector2.SignedAngle(Vector2.right, attackDir.normalized));
                        sr.size = new Vector2(hit.distance, 1);
                        sr.material.SetTexture("_SwapTex", mainColorSwapTex);
                        DespawnAnimation dAnim = newLaser.GetComponent<DespawnAnimation>();
                        dAnim.incrementIndex = false;
                        dAnim.FrameTimer = 0.2f;
                        dAnim.spriteList = SpriteManager.instance.GetModifiedSprites(SpriteManager.LASER_SPRITES);
                        dAnim.spriteIndex = Mathf.FloorToInt(Mathf.Clamp01(attackCharge/attackMaxCharge) * (dAnim.spriteList.Count-1));

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

            if (legIndex == 0 || legIndex == 5)
            {
                GameObject dust = Instantiate(dustPrefab, GameLogic.instance.playerParentTransform);
                dust.transform.position = transform.position + new Vector3(legIndex == 0 ? -0.45f : 0.45f, -0.35f);
                DespawnAnimation dAnim = dust.GetComponent<DespawnAnimation>();
                dAnim.spriteList = SpriteManager.instance.GetModifiedSprites(SpriteManager.WALK_DUST_SPRITES);
                dAnim.incrementIndex = true;
                dAnim.spriteIndex = 0;
                dAnim.FrameTimer = 0.3f;
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

        velocity = direction;
        velocity.Normalize();
        robot.AddForce(velocity * speed, ForceMode2D.Impulse);

        //if (Input.GetAxisRaw("Jump") != 0 && !isHit) {
        //    //isHit = true;
        //    GameObject robotPart = Instantiate(partPrefab, partParentTransform);

        //}

        bodyIndex = Mathf.Clamp(bodySprites.Count - Mathf.RoundToInt(bodySprites.Count * ((float)hitpoints / MAX_HP)), 0, bodySprites.Count-1);

        bodyRenderer.sprite = bodySprites[bodyIndex];
        shinyRenderer.GetComponent<SpriteMask>().sprite = bodySprites[bodyIndex];
        legRenderer.sprite = legSprites[legIndex];
        wireRenderer.sprite = wireSprites[wireIndex];
        wireRenderer.gameObject.SetActive(bodyIndex >= bodySprites.Count - 2);

        Vector3 scale = Vector3.one;
        scale.x = moveDir.x < 0 ? -1 : 1;
        bodyRenderer.transform.localScale = scale;
        legRenderer.transform.localScale = scale;
        wireRenderer.transform.localScale = scale;
        shinyRenderer.transform.localScale = scale;

        if (hitpoints == 0)
        {
            isBlinking = true;
            if (blinkTimer <= -0.2f)
            {
                blinkTimer = 0.2f;
                SetRobotColor(Color.red, false);
            }
        }

        if (isBlinking)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0)
            {
                SetRobotColor(Color.clear, true);
                isBlinking = false;
            }
        }

        if (meleeAttackTimer > 0f)
            meleeAttackTimer -= Time.deltaTime;
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
        if (meleeAttackTimer > 0f)
            return;

        meleeAttackTimer = meleeAttackDelay;

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
        newMelee.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, moveDir.normalized)));
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

                sr.material.SetTexture("_SwapTex", mainColorSwapTex);
                robotPart.GetComponentInChildren<RobotPartPhysics>().playerID = this.playerID;
            }
        }

        hitpoints -= pointsLost;

        if (hitpoints < 0)
            hitpoints = 0;

        myHPBar.UpdateHP(hitpoints);

        SetRobotColor(Color.white, false);
        isBlinking = true;
        blinkTimer = 0.4f;
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