using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhyxnicSentryCW : MonoBehaviour
{

    //components
    private Rigidbody2D rb;
    private EnemyDisablerCW ed;
        
    private Vector2 solidsize;
    
    //physicsdetection
    private bool isGrounded;
    //self-explanatory
    private bool isOffGround;

    //checks if one point is off the ground
    private bool prevGrounded = true;
    private bool prevIsOffGround;
    //for checking if you just landed
    private bool offGroundLock;
    //if you landed off the platform, keep going until you're regrounded
    private bool fullyOnGround;
    //for the offgroundlock
    private GameObject groundPoint;
    //point at which raycasts originate for grounding
    public LayerMask groundMask;
    //mask to detect what will make onground true in the raycast
    public LayerMask boundsMask;
    //playermask
    public LayerMask playerMask;
    private bool isWalled;
    
    //physics
    private float yvel;
    //downwards velocity applied every fixedframe
    public float yveladd;
    //adds to yvel
    public float yvelset;
    //sets yvel to new number
    public bool canyveladd;
    //adds to yvel
    public bool canyvelset;
    //sets yvel to new number
    private float xvel;
    //walking each frame
    public float xveladd;
    //adds to yvel
    public float xvelset;
    //sets yvel to new number
    public float xvelmove;
    //own physics xvel
    public bool canxveladd;
    //adds to yvel
    public bool canxvelset;
    //sets yvel to new number
    private int directionOfTravel;
    //direction it moves in
    private float speed;

    private int platformDir;

    public GameObject discAttack;
    public GameObject blastAttack;
    
    public bool stop;
    public bool sightCast;
    public float sightCastTime;
    public float sightCastStamp;
    public bool prevSightCast;

    public bool stunned = false;
    private float stunnedStamp = -9.0f;

    public float stopStamp;
    private float playerDistance;
    public float stopTime;

    //reset enemy stuff or roombox stuff
    private Vector3 savePos;
    //enemy starting position

    //misc
    private float intangStamp;
    //stamps time for intangibility timer

    private GameObject damagebox;

    public GameObject player;

    private BoxCollider2D trigger;

    public int mode;
    private int prevmode;
    private float attackStamp = -9.0f;
    private int attackMode = 0;
    private float breakSightStamp;
    private bool changeDirLock;
    private Vector2 endOfStopLock;
    private float attackTimestamp;
    private float stopCooldownStamp;
    public int attackStopMode;
    private bool zoomLock;
    private int zoomSaveDir;
    private GameObject edgePoint1;
    private GameObject edgePoint2;
    private float turnAroundStamp;
    public int saveDir;
    private float jumpBackStamp = -9.0f;
    private float unlockSpeedStamp = -9.0f;
    private GameObject phyxnicSprite;
    public int initialSpawnDir;
    private Animator anim;
    private bool attackFrame;
    private bool circleAttackFrame;
    private BoxCollider2D solid;
    public bool spawnInAir;
    public GameObject shield;
    private Rigidbody2D shieldrb;
    private int shieldSide;
    private int currentPlayerRegion;
    private float shieldTurnAroundStamp;
    public bool shieldAway;
    private float savePlayerDir;
    private Vector2 savePlayerVec;
    private GameObject frontArm;
    private GameObject backArm;
    private GameObject anchor;
    private GameObject shoulderPad;
    private GameObject beneath;
    private float playerDir;
    private Vector2 playerVec;
    private GameObject deathShield;
    private bool hasDroppedShield;

    void Start()
    {
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(1f, 2.5f);
        solid.isTrigger = false;
        groundMask |= (1 << 8);
        boundsMask |= (1 << 12);
        boundsMask |= (1 << 12);
        playerMask |= (1 << 11);
        discAttack = Resources.Load<GameObject>("Prefabs/DiscAttack");
        blastAttack = Resources.Load<GameObject>("Prefabs/PhyxnicSentryBlast");
        deathShield = Resources.Load<GameObject>("Prefabs/ShieldWhee");
        player = GameObject.FindWithTag("Player");
        if (initialSpawnDir != -1 && initialSpawnDir != 1) {
            directionOfTravel = 1;
        } else {
            directionOfTravel = initialSpawnDir;
        }
        shieldSide = directionOfTravel;
        saveDir = directionOfTravel;
        speed = 3f;
        rb = GetComponent<Rigidbody2D>();
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 18;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        savePos = rb.position;
        ed.savePos = rb.position;

        //sets up child collider
        /*
        damagebox = new GameObject(this.name + " Hitbox");
        damagebox.transform.parent = this.gameObject.transform;
        damagebox.transform.localPosition = new Vector3(0f,0f,0f);
        damagebox.tag = "Enemy";
        damagebox.layer = 0;
        */
        
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        

        

        ed.deathTime = 1.417f;
        ed.matDefault = Resources.Load("Materials/PhyxnicGlow") as Material;

        
        mode = 0;
        stop = false;

        CreateObjects();
    }

    public void EnableReset() {
        rb.position = savePos;
        mode = 0;
        attackMode = 0;
        attackStopMode = 0;
        directionOfTravel = saveDir;
        jumpBackStamp = -9.0f;
        unlockSpeedStamp = -9.0f;
        /*if (!spawnInAir) {
            rb.position = SpawnCast();
        }*/
        shieldSide = directionOfTravel;
        shieldAway = false;
        hasDroppedShield = false;
    }

    public void CreateObjects() {
        phyxnicSprite = Instantiate(Resources.Load<GameObject>("Prefabs/PhyxnicSentrySprite") as GameObject, gameObject.transform);
        phyxnicSprite.transform.localPosition = new Vector3(0,solid.size.y * -0.5f,0);
        frontArm = phyxnicSprite.transform.Find("PhyxnicSentrySprite (1)").gameObject;
        backArm = phyxnicSprite.transform.Find("PhyxnicSentrySprite (2)").gameObject;
        anchor = frontArm.transform.Find("Anchor").gameObject;
        shoulderPad = phyxnicSprite.transform.Find("GameObject").gameObject;
        beneath = frontArm.transform.Find("GameObject (1)").gameObject;
        anim = phyxnicSprite.transform.Find("SpriteObj").GetComponent<Animator>();
        if (phyxnicSprite.transform.Find("SpriteObj") != null) {
            ed.sr = phyxnicSprite.transform.Find("SpriteObj").GetComponent<SpriteRenderer>();
        }
        ed.sr.material = ed.matDefault; //WHEEEEE
        //sets up edgepoints
        edgePoint1 = new GameObject(this.name + " EdgePoint");
        edgePoint1.transform.parent = this.gameObject.transform;
        edgePoint1.transform.localPosition = new Vector3(-0.6f*solidsize.x,(-0.5f*solidsize.y),0f);
        BoxCollider2D edgeTrigger1 = edgePoint1.AddComponent<BoxCollider2D>();
        edgeTrigger1.size = new Vector2(0.1f, solidsize.y);
        edgeTrigger1.offset = new Vector2(0f, solidsize.y * 0.5f);
        edgeTrigger1.isTrigger = true;
        edgeTrigger1.enabled = false;
        edgePoint2 = new GameObject(this.name + " EdgePoint");
        edgePoint2.transform.parent = this.gameObject.transform;
        edgePoint2.transform.localPosition = new Vector3(0.6f*solidsize.x,(-0.5f*solidsize.y),0f);
        BoxCollider2D edgeTrigger2 = edgePoint2.AddComponent<BoxCollider2D>();
        edgeTrigger2.size = new Vector2(0.1f, solidsize.y);
        edgeTrigger2.offset = new Vector2(0f, solidsize.y * 0.5f);
        edgeTrigger2.isTrigger = true;
        edgeTrigger2.enabled = false;

        shield = Instantiate(Resources.Load<GameObject>("Prefabs/PhyxnicSentryShield") as GameObject);
        shield.transform.SetParent(transform);
        shield.transform.localPosition = new Vector2(1.5f*saveDir, 0f);
        shieldrb = shield.GetComponent<Rigidbody2D>();

        
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";
        ed.attachedObjects = new GameObject[6];
        ed.attachedObjects[0] = edgePoint1;
        ed.attachedObjects[1] = edgePoint2;
        ed.attachedObjects[2] = phyxnicSprite;
        ed.attachedObjects[3] = groundPoint;
        ed.attachedObjects[4] = shield;
        ed.attachedObjects[5] = shield.transform.GetChild(0).gameObject;
        EnableReset();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        bool left = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        bool center = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up);
        bool right = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        isGrounded = (left || right || center);
        isOffGround = (!left || !right || !center);
        fullyOnGround = (center && left && right);
        isWalled = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), 0.02f, -Vector2.right);
        //sightCast = SightCast((Vector2)gameObject.transform.position, 10f, Vector2.right) || SightCast((Vector2)gameObject.transform.position, 10f, -Vector2.right);
        sightCast = SightCast((Vector2)transform.position, 6f, (player.transform.position - transform.position).normalized);
        playerDir = Mathf.Atan2((anchor.transform.position.y - player.transform.position.y), (anchor.transform.position.x - player.transform.position.x));
        playerVec = (anchor.transform.position - player.transform.position).normalized;
        if (sightCast && mode == 0 && !ed.dying) {
            mode = 1;
            attackTimestamp = StaticDataCW.Time;
            stopStamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
            shieldTurnAroundStamp = StaticDataCW.Time;
            stopTime = 0.15f;
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
        }

        if (player.transform.position.x < transform.position.x - 0.75f) {
            currentPlayerRegion = -1;
        } else if (player.transform.position.x < transform.position.x - 0.75f) {
            currentPlayerRegion = 0;
        } else {
            currentPlayerRegion = 1;
        }
        
        bool newLeft = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.6f*solidsize.x, 0), 0.02f, -Vector2.up);
        bool newRight = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.6f*solidsize.x, 0), 0.02f, -Vector2.up);
        if (!newLeft) {
            edgePoint1.GetComponent<BoxCollider2D>().enabled = true;
            edgePoint1.transform.parent = null;
        } else {
            edgePoint1.GetComponent<BoxCollider2D>().enabled = false;
            edgePoint1.transform.parent = this.gameObject.transform;
            edgePoint1.transform.localPosition = new Vector3(-0.6f*solidsize.x,(-0.5f*solidsize.y),0f);
        }
        if (!newRight) {
            edgePoint2.GetComponent<BoxCollider2D>().enabled = true;
            edgePoint2.transform.parent = null;
        } else {
            edgePoint2.GetComponent<BoxCollider2D>().enabled = false;
            edgePoint2.transform.parent = this.gameObject.transform;
            edgePoint2.transform.localPosition = new Vector3(0.6f*solidsize.x,(-0.5f*solidsize.y),0f);
        }

        if (mode == 1 && prevmode == 0){
            sightCastStamp = StaticDataCW.Time;
            sightCastTime = 0.1f;
        }
        prevmode = mode;

        if (mode == 1 && sightCast) {
            breakSightStamp = StaticDataCW.Time;
        }

        if (mode == 1 && StaticDataCW.Time > breakSightStamp + 6f && fullyOnGround) {
            mode = 0;
        }

        if (((mode == 1 && StaticDataCW.Time > turnAroundStamp + 0.5f && attackStopMode == 0 || attackStopMode == 2 || attackStopMode == 8)) && !ed.dying) {
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            if (shieldSide != 0) {
                shieldSide = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            }
            turnAroundStamp = StaticDataCW.Time;
        }

        if (mode == 1 && shieldSide != currentPlayerRegion && StaticDataCW.Time > shieldTurnAroundStamp + 1f) {
            if (shieldSide == 2) {
                shieldSide = currentPlayerRegion;
                shieldTurnAroundStamp = StaticDataCW.Time;
                
            } else {
                shieldSide = 2;
                shieldTurnAroundStamp = StaticDataCW.Time;
            }
        }
        


        /*
        if (ed.intangible){
            trigger.enabled = false;
        } else{
            trigger.enabled = true;
        }
        */

        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0 && !ed.dying && !ed.dead)) {
            rb.position = savePos;
            mode = 0;
            attackMode = 0;
            attackStopMode = 0;
            directionOfTravel = saveDir;
            jumpBackStamp = -9.0f;
            unlockSpeedStamp = -9.0f;
            shieldSide = directionOfTravel;
            shieldAway = false;
            hasDroppedShield = false;
        }

        //attacks
        if (!ed.dying) {
            if (isGrounded && mode == 1 && playerDistance < 7f && !stunned && StaticDataCW.Time > attackTimestamp + 1.5f && directionOfTravel == (player.transform.position.x > transform.position.x? 1:-1) && attackStopMode == 0) {
                attackTimestamp = StaticDataCW.Time;
                attackFrame = true;
                stop = true;
                stopStamp = StaticDataCW.Time;
                stopTime = 1f;
                attackStopMode = 1;
                shieldAway = true;
                shield.GetComponent<BoxCollider2D>().enabled = false;
                shield.GetComponentsInChildren<BoxCollider2D>()[1].enabled = false;
                shield.GetComponent<SpriteRenderer>().enabled = false;
            } else if (attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 0.4f) {
                attackTimestamp = StaticDataCW.Time;
                if (Random.Range(1, 3) == 1) {
                    stopStamp = StaticDataCW.Time;
                    stopTime = 2.3f;
                    attackStopMode = 2;
                } else {
                    stopStamp = StaticDataCW.Time;
                    stopTime = 1.3f;
                    attackStopMode = 8;
                }
            } else if (attackStopMode > 1 && attackStopMode < 5 && StaticDataCW.Time > attackTimestamp + 0.8f) {
                if (attackStopMode == 2) {
                    savePlayerDir = Mathf.PI+playerDir;
                    savePlayerVec = -playerVec;
                }
                SpawnDiscAttack();
                attackTimestamp = StaticDataCW.Time - 0.4f;
                attackStopMode++;
            } else if (attackStopMode == 5 && StaticDataCW.Time > attackTimestamp + 0.2f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 6;
            } else if (attackStopMode == 6 && StaticDataCW.Time > attackTimestamp + 0.4f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 7;
            } else if (attackStopMode == 7 && StaticDataCW.Time > attackTimestamp + 0.4f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 0;
                shieldAway = false;
                shield.GetComponent<BoxCollider2D>().enabled = true;
                shield.GetComponentsInChildren<BoxCollider2D>()[1].enabled = true;
                shield.GetComponent<SpriteRenderer>().enabled = true;
            } else if (attackStopMode == 8 && StaticDataCW.Time > attackTimestamp + 1.3f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 9;
                savePlayerDir = Mathf.PI+playerDir;
                savePlayerVec = -playerVec;
                SpawnBlast();
            } else if (attackStopMode == 9 && StaticDataCW.Time > attackTimestamp + 0.2f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 10;
            } else if (attackStopMode == 10 && StaticDataCW.Time > attackTimestamp + 0.2f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 11;
            } else if (attackStopMode == 11 && StaticDataCW.Time > attackTimestamp + 0.4f) {
                attackStopMode = 0;
                shieldAway = false;
                shield.GetComponent<BoxCollider2D>().enabled = true;
                shield.GetComponentsInChildren<BoxCollider2D>()[1].enabled = true;
                shield.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        if (StaticDataCW.Time < jumpBackStamp + 0.6f) {
            Xvel(-4f*directionOfTravel, true);
        }
        

        //yvel

        if (isGrounded && rb.velocity.y <= 1E-03) {
            yvel = 0f;
        } else if (!isGrounded) {
            if (StaticDataCW.Time < jumpBackStamp + 0.3f)
                yvel -= 0.6f;
            else {
                yvel -= 0.9f;
            }
        }
        
        if (yvel <= -15f) {yvel = -15f;}

        if (canyvelset == true){
            yvel = yvelset;
            canyvelset = false;
        }

        if (canyveladd == true){
            yvel += yveladd;
            canyveladd = false;
        }
        
        rb.velocity = new Vector2(rb.velocity.x, yvel);

        //xvel
        
        if (isWalled && !ed.dying) {
            /*TurnAround();
            xvel = 0f;
            unlockSpeedStamp = StaticDataCW.Time;
            changeDirLock = true;*/
            bool isWalledBottom = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,0), 0.02f, new Vector2(directionOfTravel,0)) &&
            !CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,solidsize.y*0.3f), 0.02f, new Vector2(directionOfTravel,0));
            if (mode == 0) {
                xvel = 0f;
            } else if (isWalledBottom && directionOfTravel != 0) {
                float height;
                bool steppable = false;
                for (height = 0; height < 0.51f; height += 0.02f) {
                    if (!CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,height), 0.02f, new Vector2(directionOfTravel,0))) {
                        steppable = true;
                        break;
                    }
                }
                if (steppable && !(CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), height, Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0f,solidsize.y), height, Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), height, Vector2.up))) {
                    rb.position = new Vector2(rb.position.x, rb.position.y + height);
                }
            }
        }
        
        
        if (stop && StaticDataCW.Time > stopStamp + stopTime) {
            changeDirLock = true;
            stop = false;
        }
        if (isGrounded && isOffGround && !prevGrounded && !offGroundLock)
        {
            offGroundLock = true;
        }

        if (offGroundLock && (fullyOnGround || !isGrounded)) {
            offGroundLock = false;
        }

        if (isGrounded) {
            if (!zoomLock) {
                zoomSaveDir = directionOfTravel;
            }
            if (!stop && !stunned && !ed.dying) {
                if (xvel < speed && directionOfTravel == 1) {
                    xvel += 0.75f;
                } else if (xvel > -speed && directionOfTravel == -1) {
                    xvel -= 0.75f;
                }
                if (xvel > speed && directionOfTravel == 1 || xvel < -speed && directionOfTravel == -1) {
                    xvel = directionOfTravel * speed;
                }
            } else {
                xvel = 0f;
            }
        }

        if (attackStopMode == 2 || attackStopMode == 8) {
            Quaternion saverot = frontArm.transform.rotation;
            Quaternion targetdir = Quaternion.Euler(0, 0, playerDir * (180 / Mathf.PI) - 90);
            frontArm.transform.rotation = targetdir;
            frontArm.transform.rotation = Quaternion.Slerp(saverot, targetdir,  0.3f);
        }
        if (attackStopMode == 3 || attackStopMode == 9) {
            Quaternion saverot = frontArm.transform.rotation;
            Quaternion targetdir = Quaternion.Euler(0, 0,  (savePlayerDir-Mathf.PI) * (180 / Mathf.PI) - 90);
            frontArm.transform.rotation = targetdir;
            frontArm.transform.rotation = Quaternion.Slerp(saverot, targetdir,  0.3f);
        }
        if (attackStopMode == 5 || (attackStopMode == 10) ) {
            Quaternion saverot = frontArm.transform.rotation;
            Quaternion targetdir = Quaternion.Euler(0, 0,  0);
            frontArm.transform.rotation = targetdir;
            frontArm.transform.rotation = Quaternion.Slerp(saverot, targetdir,  0.3f);
        }

        if (shieldSide == 1 || shieldSide == -1) {
            shield.transform.localPosition = new Vector2(1f/*shieldSide*/, 0f);
            shield.GetComponentsInChildren<EnemyAttackCW>()[0].dir = shieldSide;
            shield.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            shield.GetComponent<SpriteRenderer>().flipX = true/*shieldSide == 1? false:true*/;
            shield.GetComponentsInChildren<BoxCollider2D>()[1].size = new Vector2(0.7f, 1.5f);
            shield.GetComponentsInChildren<BoxCollider2D>()[1].offset = new Vector2(0f, 0f);
            directionOfTravel = shieldSide;
            backArm.transform.rotation = Quaternion.Euler(0f, 0f, 41.7f * shieldSide);
        } else {
            shield.transform.localPosition = new Vector2(0f, 1.8f);
            shield.GetComponentsInChildren<EnemyAttackCW>()[0].dir = 2;
            shield.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            shield.GetComponent<SpriteRenderer>().flipX = false;
            shield.GetComponentsInChildren<BoxCollider2D>()[1].size = new Vector2(1f, 1.5f);
            shield.GetComponentsInChildren<BoxCollider2D>()[1].offset = new Vector2(-0.15f, 0f);
            backArm.transform.rotation = Quaternion.Euler(0f, 0f, 155f);
        }

        if (mode == 0) {
            if (StaticDataCW.Time < unlockSpeedStamp + 0.1f) {
                speed = 1f;
            } else {
                speed = 0f;
            }
        } else {
            speed = 2.3f;
        }
        
        xvelmove = xvelmove - (xvelmove * 0.3f * Time.timeScale);

        if (canxvelset == true){
            xvelmove = xvelset;
            canxvelset = false;
        }

        if (canxveladd == true){
            xvelmove = xvelmove + xveladd;
            canxveladd = false;
        }

        if (xvelmove < 0.005f && xvelmove > -0.005f){
            xvelmove = 0;
        }

        if (rb.velocity.x > -0.2f && rb.velocity.x < 0.2f){
            anim.SetBool("Walking", false);
        } else{
            anim.SetBool("Walking", true);
        }
        
        if(stunned || ed.dying){
            rb.velocity = new Vector2(xvelmove, rb.velocity.y);
        } else{
            rb.velocity = new Vector2(xvel + xvelmove, rb.velocity.y);
        }
        
        
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }
        if (stunned && StaticDataCW.Time > stunnedStamp + 0.4f){
            stunned = false;
            mode = 1;
            attackTimestamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            breakSightStamp = StaticDataCW.Time;
        }
        if (directionOfTravel == 1){
            ed.transform.localScale = new Vector3(1f, 1f, 1f);
        } else if (directionOfTravel == -1){
            ed.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        prevGrounded = isGrounded;
        prevIsOffGround = isOffGround;
        prevSightCast = sightCast;

        if (attackStopMode == 0){
            anim.SetBool("Attacking", false);
        } else{
            anim.SetBool("Attacking", true);
        }
        if (shieldAway) {
            backArm.GetComponent<SpriteRenderer>().enabled = false;
        } else {
            backArm.GetComponent<SpriteRenderer>().enabled = true;
        }
        if (mode == 0 || attackStopMode == 0 || attackStopMode == 1 || attackStopMode == 7 || attackStopMode == 11) {
            frontArm.GetComponent<SpriteRenderer>().enabled = false;
            shoulderPad.GetComponent<SpriteRenderer>().enabled = false;
            beneath.GetComponent<SpriteRenderer>().enabled = false;
            frontArm.transform.Find("GameObject (1)").GetComponent<SpriteRenderer>().enabled = false;
        } else {
            frontArm.GetComponent<SpriteRenderer>().enabled = true;
            shoulderPad.GetComponent<SpriteRenderer>().enabled = true;
            beneath.GetComponent<SpriteRenderer>().enabled = true;
            frontArm.transform.Find("GameObject (1)").GetComponent<SpriteRenderer>().enabled = true;
        }
        anim.SetInteger("AttackStopMode", attackStopMode);
        frontArm.GetComponent<Animator>().SetInteger("AttackStopMode", attackStopMode);
        frontArm.GetComponent<Animator>().SetBool("Idle", (attackStopMode == 8 && StaticDataCW.Time < attackTimestamp + 0.5f));
        frontArm.GetComponent<Animator>().SetBool("Shoot", (attackStopMode >= 2 && attackStopMode <= 6 && StaticDataCW.Time < attackTimestamp + 0.525f));
        anim.SetBool("TakeOut", attackStopMode == 7 || attackStopMode == 11);
        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
            backArm.GetComponent<SpriteRenderer>().enabled = false;
            frontArm.GetComponent<SpriteRenderer>().enabled = false;
            shoulderPad.GetComponent<SpriteRenderer>().enabled = false;
            beneath.GetComponent<SpriteRenderer>().enabled = false;
            shield.GetComponent<SpriteRenderer>().enabled = false;
            phyxnicSprite.transform.Find("GameObject").GetComponent<SpriteRenderer>().enabled = false;
            frontArm.transform.Find("GameObject (1)").GetComponent<SpriteRenderer>().enabled = false;
            if (!hasDroppedShield) {
                shieldAway = true;
                GameObject dShield = Instantiate(deathShield as GameObject);
                dShield.transform.position = shield.transform.position;
                hasDroppedShield = true;
            }
        } else{
            anim.SetBool("Death", false);
        }
    }


    void OnTriggerEnter2D(Collider2D other) {

        if (other.CompareTag("RoomBound") || other.gameObject.layer == 14){
            directionOfTravel = directionOfTravel * -1;
            //rb.position = new Vector3(rb.position.x + 0.5f*dir,rb.position.y,0f);
            
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            mode = 1;
            if (attackStopMode == 0 || mode == 0) {
                directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
                attackTimestamp = StaticDataCW.Time;
            }
            turnAroundStamp = StaticDataCW.Time;
        } else if (other.CompareTag("EnemyAttack") && !ed.intangible && other.GetComponent<SentryDiscCW>() != null && other.GetComponent<SentryDiscCW>().parent == gameObject && other.GetComponent<SentryDiscCW>().reflected) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= 1;
            mode = 1;
            turnAroundStamp = StaticDataCW.Time;
            SceneManagerCW.PlaySound("Impact", 1);
            other.gameObject.GetComponent<EnemyAttackCW>().projDying = true;
            other.gameObject.GetComponent<EnemyAttackCW>().projectileDeathstamp = StaticDataCW.Time;
            other.gameObject.GetComponent<Animator>().SetBool("Dying", true);
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
        } else if (other.CompareTag("KnockbackDamage")) {
            if (other.GetComponent<Transform>().position.x < GetComponent<Transform>().position.x) {
                Xvel(20,true);
            } else {
                Xvel (-20,true);
            }
        }
    }


    private bool CheckGrounded(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask | boundsMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance)
            {
                return true;
            }
        }
        return false;
    }

    private bool SightCast(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D check = Physics2D.Raycast(point, dir, distance, playerMask | groundMask | boundsMask);
        if (check.collider != null && check.transform.gameObject.layer == 11)
        {
            Vector2 checkVec = check.point - point;
            float checkDistance = checkVec.magnitude;
            if (checkDistance < distance)
            {
                return true;
            }
        }
        return false;
    }

    void Yvel(float velocity, bool setadd){
        if (setadd){
            canyvelset = true;
            yvelset = velocity;
        } else{
            canyveladd = true;
            yveladd = velocity;
        }
    }

    void Xvel(float velocity, bool setadd){
        if (setadd){
            canxvelset = true;
            xvelset = velocity;
        } else{
            canxveladd = true;
            xveladd = velocity;
        }
    }

    void TurnAround()
    {
        directionOfTravel = directionOfTravel * -1;
    }

    void SpawnDiscAttack()
    {
        GameObject attack = Instantiate(discAttack) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 2;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = savePlayerDir;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 5f;
        attack.GetComponent<EnemyAttackCW>().offset = anchor.transform.position-transform.position;
        attack.GetComponent<EnemyAttackCW>().projSpeed = 10f;
        attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
        attack.GetComponent<EnemyAttackCW>().deathtime = 4f;
        attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.1f;
        attack.transform.rotation = Quaternion.Euler(0f,0f,savePlayerDir*180f/Mathf.PI - 90f);
        attack.GetComponent<SentryDiscCW>().parent = gameObject;
        attack.GetComponent<SentryDiscCW>().shield = shield;
    }

    void SpawnBlast()
    {
        for (float i = -0.25f; i < 0.26f; i += 0.125f) {
            GameObject attack = Instantiate(discAttack) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 2;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = (savePlayerDir+i);
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 7f;
            attack.GetComponent<EnemyAttackCW>().offset = anchor.transform.position-transform.position;
            attack.GetComponent<EnemyAttackCW>().projSpeed = 12f;
            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.5f;
            attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.1f;
            attack.transform.rotation = Quaternion.Euler(0f,0f,(savePlayerDir+i)*180f/Mathf.PI - 90f);
            attack.GetComponent<SentryDiscCW>().parent = gameObject;
            attack.GetComponent<SentryDiscCW>().shield = shield;
        }
    /*
        GameObject attack = Instantiate(blastAttack, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 2f;
        if (savePlayerVec.normalized.y > 1f/Mathf.Sqrt(2f)) {
            attack.GetComponent<EnemyAttackCW>().dir = 2;
        } else if (savePlayerVec.normalized.y < -1f/Mathf.Sqrt(2f)) {
            attack.GetComponent<EnemyAttackCW>().dir = -2;
        } else if (savePlayerVec.normalized.x < 0f) {
            attack.GetComponent<EnemyAttackCW>().dir = -1;
        } else {
            attack.GetComponent<EnemyAttackCW>().dir = 1;
        }
        attack.transform.rotation = Quaternion.Euler(0f, 0f, savePlayerDir*180f/Mathf.PI);
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 12f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3((savePlayerVec.normalized * 2f).x,(savePlayerVec.normalized * 2f).y,0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 0.1f;
        attack.GetComponent<EnemyAttackCW>().setRotationManually = true;
        attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.1f;
    */
    }

    private Vector2 SpawnCast(){
        RaycastHit2D spikePoint = Physics2D.Raycast(transform.position, new Vector2(0, -1), 30, groundMask | boundsMask);
        if (spikePoint.collider != null)
        {
            return spikePoint.point + new Vector2(0, (solid.size.y * 0.5f));
        } else{
            return transform.position;
        }
    }
}
