using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornMechCW : MonoBehaviour
{

    //components
    private Rigidbody2D rb;
    private EnemyDisablerCW ed;
        
    private Vector2 solidsize;
    
    //physicsdetection
    public bool isGrounded;
    //self-explanatory
    private bool isOffGround;
    //checks if one point is off the ground
    private bool prevGrounded = true;
    //for checking if you just landed
    private bool prevFullyGrounded = true;
    //for really sad new system to stop enemy from launching off into the horizon with a wink sound effect
    public bool prevIsOffGround = true;
    //sadness
    private bool offGroundLock;
    //if you landed off the platform, keep going until you're regrounded
    private bool fullyOnGround;
    //for the offgroundlock
    private GameObject groundPoint;
    //point at which raycasts originate for grounding
    public LayerMask groundMask = 8;
    //mask to detect what will make onground true in the raycast
    public LayerMask boundsMask = 12;
    private bool isWalledFront;
    
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
    //gamer
    private float prevxvelmove;
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
    public int directionOfTravel;
    //direction it moves in
    private float speed;

    public bool stunned = false;
    private float stunnedStamp = -9.0f;

    //reset enemy stuff or roombox stuff
    private Vector3 savePos;
    //enemy starting position

    //misc
    private float intangStamp;
    //stamps time for intangibility timer
    public int initialSpawnDir;

    private GameObject damagebox;

    public GameObject player;

    private BoxCollider2D trigger;
    public int mode = 0;
    private int saveMode;
    private LayerMask playerMask;
    private float playerDistance;
    public GameObject thornMechPillar1;
    public GameObject thornMechPillar2;
    public GameObject thornMechPillar3;
    public GameObject thornProjectile;
    public GameObject thornMechUppercut;
    public GameObject frostGuardAOEGraphic;
    public GameObject frostGuardSprite;
    private SpriteRenderer sr;
    private int saveDir;

    private float breakSightStamp;
    /*public GameObject guardPoint;
    private Vector3 guardPointPos;
    private BoxCollider2D guardPointHitbox;
    private CircleCollider2D guardPointDetector;
    private GameObject guardPointDetectionObj;*/
    private float attackTimestamp = -9.0f;
    private bool dashing;
    private float dashSpeed;
    public float[] prevposX;
    private bool changeDirLock;
    private float changeDirTimer = -9.0f;
    public SpriteRenderer guardSprite;
    private BoxCollider2D solid;

    public Animator anim;
    private bool idlelock;
    private bool attackFrame;
    private bool animDirLock;
    private bool inStabAnim;
    public Material frostMat;
    private int attackType;
    private int attackStopMode;
    private int goIntoMode;
    private float goIntoModeTimestamp;
    public bool dormant;
    public bool spawnInAir;
    private int repeatedAttacksCounter;
    private int prevAttackNum;

    void Start()
    {
    
        player = GameObject.FindWithTag("Player");
        if (initialSpawnDir != -1 && initialSpawnDir != 1) {
            directionOfTravel = 1;
        } else {
            directionOfTravel = initialSpawnDir;
        }
        saveDir = directionOfTravel;
        speed = 2f;
        rb = GetComponent<Rigidbody2D>();
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 21;
        ed.orbcount = 2;
        ed.health = ed.maxhealth;
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);

        //sets up child collider
        /*
        damagebox = new GameObject(this.name + " Hitbox");
        damagebox.transform.parent = this.gameObject.transform;
        damagebox.transform.localPosition = new Vector3(0f,0f,0f);
        damagebox.tag = "Enemy";
        damagebox.layer = 0;
        */
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(1.5f, 2.5f);
        solid.isTrigger = false;
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        ed.matDefault = Resources.Load("Materials/ThornMechGlow") as Material;

        CreateObjects();
        
        ed.deathTime = 1.5f;

        prevposX = new float[3];

        
    }

    public void CreateObjects() {
        dormant = true;
        thornMechPillar1 = Resources.Load<GameObject>("Prefabs/TMPillar1");
        thornMechPillar2 = Resources.Load<GameObject>("Prefabs/TMPillar2");
        thornMechPillar3 = Resources.Load<GameObject>("Prefabs/TMPillar3");
        thornProjectile = Resources.Load<GameObject>("Prefabs/ThornProjectile");
        thornMechUppercut = Resources.Load<GameObject>("Prefabs/TMUppercut");
        frostGuardAOEGraphic = Resources.Load<GameObject>("Prefabs/FrostGuardAOEGraphic");
        frostGuardSprite = Instantiate(Resources.Load<GameObject>("Prefabs/ThornMechSprite") as GameObject, gameObject.transform);
        frostGuardSprite.transform.localPosition = new Vector3(0,solid.size.y * -0.5f,0);
        sr = frostGuardSprite.GetComponent<SpriteRenderer>();
        anim = frostGuardSprite.GetComponent<Animator>();
        ed.sr = frostGuardSprite.GetComponent<SpriteRenderer>();
        ed.sr.material = ed.matDefault; //WHEEEEE
        changeDirLock = false;
        frostMat = frostGuardSprite.GetComponent<SpriteRenderer>().material;

        
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";


        ed.attachedObjects = new GameObject[2];
        ed.attachedObjects[0] = frostGuardSprite;
        ed.attachedObjects[1] = groundPoint;
        EnableReset();
    }

    public void EnableReset() {
        mode = 0;
        xvel = 0f;
        yvel = 0f;
        rb.position = savePos;
        dormant = true;
        if (initialSpawnDir != -1 && initialSpawnDir != 1) {
            directionOfTravel = 1;
        } else {
            directionOfTravel = initialSpawnDir;
        }
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        prevAttackNum = 0;
        repeatedAttacksCounter = 0;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        changeDirLock = false;
        bool left = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        bool center = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up);
        bool right = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        isGrounded = (left || right || center);
        isOffGround = center && (!left || !right);
        fullyOnGround = (center && left && right);
        isWalledFront = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, Vector2.right*directionOfTravel) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0), 0.02f, Vector2.right*directionOfTravel) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), 0.02f, Vector2.right*directionOfTravel);
        bool isPlayerAhead = CheckForPlayer((Vector2)transform.position + new Vector2(0f, 1f), 6f, directionOfTravel * Vector2.right) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,0.3f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,-0.3f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,-0.45f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,0.15f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,-0.15f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,-0.6f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 0.5f), 6f, new Vector2(directionOfTravel,0.6f).normalized);
        bool newLeft = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.6f*solidsize.x, 0), 0.02f, -Vector2.up);
        bool newRight = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.6f*solidsize.x, 0), 0.02f, -Vector2.up);
        
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));

        if (mode == 1 && isPlayerAhead) {
            breakSightStamp = StaticDataCW.Time;
        }

        if (mode == 1 && StaticDataCW.Time > breakSightStamp + 4.5f && attackType == 0) {
            mode = 1;
            attackType = 0;
        }

        if (mode == 0 && (isPlayerAhead || playerDistance < 1.5f)) {
            mode = 2;
            dormant = false;
            attackType = 0;
            goIntoModeTimestamp = StaticDataCW.Time;
        }
        if (mode == 2 && StaticDataCW.Time > goIntoModeTimestamp + 1f) {
            mode = 1;
        }

        /*
        if (ed.intangible){
            trigger.enabled = false;
        } else{
            trigger.enabled = true;
        }
        */

        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0)) {
            rb.position = savePos;
            mode = 0;
            attackStopMode = 0;
            attackType = 0;
            directionOfTravel = saveDir;
            xvel = 0;
            dashing = false;
            yvel = 0f;
            dormant = true;
            if (initialSpawnDir != -1 && initialSpawnDir != 1) {
                directionOfTravel = 1;
            } else {
                directionOfTravel = initialSpawnDir;
            }
        }

        //attacks
        
        
        if (mode == 1 && !stunned && !ed.dying) {
            if (attackType == 0) {
                if (StaticDataCW.Time > attackTimestamp + 0.75f) {
                    if (repeatedAttacksCounter < 2) {
                        if (playerDistance < 3.5f) {
                            attackType = Random.Range(1,3);
                            if (attackType == 2) {
                                attackType = 3;
                            }
                        } else if (playerDistance < 4.5f) {
                            attackType = Random.Range(2,4);
                        } else {
                            attackType = Random.Range(2,7);
                            if (attackType > 3) {
                                attackType = 2;
                            }
                        }
                    } else {
                        if (prevAttackNum == 1) {
                            attackType = Random.Range(2,4);
                        } else if (prevAttackNum == 2) {
                            attackType = Random.Range(1,3);
                            if (attackType == 2) {
                                attackType = 3;
                            }
                        } else {
                            attackType = Random.Range(1,3);
                        }
                    }
                    if (attackType == 1) {
                        attackTimestamp = StaticDataCW.Time + 0.5f;
                    } else {
                        attackTimestamp = StaticDataCW.Time;
                    }
                    if (attackType == prevAttackNum) {
                        repeatedAttacksCounter++;
                    } else {
                        repeatedAttacksCounter = 0;
                    }
                    prevAttackNum = attackType;
                    attackStopMode = 0;
                    //ed.waitDeath = true;
                    animDirLock = true;
                }
            } else {
                if (attackType == 1 && StaticDataCW.Time > attackTimestamp + 0.5f) {
                    switch (attackStopMode) {
                        case 0:
                            for (int i = -1; i < 2; i += 2) {
                                if (CheckGrounded(groundPoint.transform.position + new Vector3(-4f*i,0f,0f), 0.05f, Vector2.down) || CheckGrounded(groundPoint.transform.position + new Vector3(-4f*i,0f,0.1f), 0.05f, Vector2.up)) {
                                    GameObject attack = Instantiate(thornMechPillar1, transform) as GameObject;
                                    attack.GetComponent<EnemyAttackCW>().type = 1;
                                    attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                                    attack.GetComponent<EnemyAttackCW>().damage = 1f;
                                    attack.GetComponent<EnemyAttackCW>().dir = 2;
                                    attack.GetComponent<EnemyAttackCW>().knockbackValue = 15f;
                                    attack.GetComponent<EnemyAttackCW>().offset = new Vector3(4f*i,0f,0f);
                                    attack.GetComponent<EnemyAttackCW>().deathtime = 0.916666667f;
                                    attack.GetComponent<EnemyAttackCW>().disabletime = 0.69f;
                                    attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.66666f;
                                    attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                                    attack.GetComponent<EnemyAttackCW>().setRotationManually = true;
                                    attack.transform.rotation = Quaternion.Euler(0f,0f,0f);
                                }
                            }
                            break;
                        case 1:
                            for (int i = -1; i < 2; i += 2) {
                                if (CheckGrounded(groundPoint.transform.position + new Vector3(-2f*i,0f,0f), 0.05f, Vector2.down) || CheckGrounded(groundPoint.transform.position + new Vector3(-2f*i,0f,0.1f), 0.05f, Vector2.up)) {
                                    GameObject attack = Instantiate(thornMechPillar2, transform) as GameObject;
                                    attack.GetComponent<EnemyAttackCW>().type = 1;
                                    attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                                    attack.GetComponent<EnemyAttackCW>().damage = 1f;
                                    attack.GetComponent<EnemyAttackCW>().dir = 2;
                                    attack.GetComponent<EnemyAttackCW>().knockbackValue = 17f;
                                    attack.GetComponent<EnemyAttackCW>().offset = new Vector3(2f*i,0f,0f);
                                    attack.GetComponent<EnemyAttackCW>().deathtime = 0.916666667f;
                                    attack.GetComponent<EnemyAttackCW>().disabletime = 0.69f;
                                    attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.66666f;
                                    attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                                    attack.GetComponent<EnemyAttackCW>().setRotationManually = true;
                                    attack.transform.rotation = Quaternion.Euler(0f,0f,0f);
                                }
                            }
                            break;
                        case 2:
                            for (int i = 1; i < 2; i += 2) {
                                GameObject attack = Instantiate(thornMechPillar3, transform) as GameObject;
                                attack.GetComponent<EnemyAttackCW>().type = 1;
                                attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                                attack.GetComponent<EnemyAttackCW>().damage = 1f;
                                attack.GetComponent<EnemyAttackCW>().dir = 2;
                                attack.GetComponent<EnemyAttackCW>().knockbackValue = 19f;
                                attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0,0f,0f);
                                attack.GetComponent<EnemyAttackCW>().deathtime = 0.916666667f;
                                attack.GetComponent<EnemyAttackCW>().disabletime = 0.69f;
                                attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.66666f;
                                attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                                attack.GetComponent<EnemyAttackCW>().setRotationManually = true;
                                attack.transform.rotation = Quaternion.Euler(0f,0f,0f);
                            }
                            break;
                    }
                    attackTimestamp = StaticDataCW.Time;
                    attackStopMode++;
                    if (attackStopMode == 3) {
                        attackStopMode = 0;
                        attackType = 0;
                        attackTimestamp = StaticDataCW.Time + 0.25f;
                        animDirLock = false;
                    }
                } else if (attackType == 2 && StaticDataCW.Time > attackTimestamp + 0.003f) {
                    if (attackStopMode < 1) {
                        attackTimestamp = StaticDataCW.Time + 0.8f;
                    } else if (attackStopMode < 6) {
                        //for (int i = 0; i < 2; i++) {
                            GameObject attack = Instantiate(thornProjectile) as GameObject;
                            attack.GetComponent<EnemyAttackCW>().type = 2;
                            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                            attack.GetComponent<EnemyAttackCW>().damage = 1f;
                            attack.GetComponent<EnemyAttackCW>().dir = Mathf.PI*(0.5f - directionOfTravel*(0.75f - attackStopMode/4f));
                            if (attackStopMode == 2) {
                                attack.GetComponent<EnemyAttackCW>().dir += directionOfTravel*0.1f;
                            } else if (attackStopMode == 4) {
                                attack.GetComponent<EnemyAttackCW>().dir -= directionOfTravel*0.1f;
                            }
                            attack.GetComponent<EnemyAttackCW>().knockbackValue = 9f;
                            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,-0.4f,0f);
                            attack.GetComponent<EnemyAttackCW>().projSpeed = 5f;
                            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                            attack.GetComponent<EnemyAttackCW>().deathtime = 5f;
                            attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.1f;
                            attack.transform.rotation = Quaternion.Euler(0f,0f,attack.GetComponent<EnemyAttackCW>().dir*180f/Mathf.PI - 90f);
                            //attackStopMode++;
                            if (Random.Range(1,3) == 2) {
                                attack.GetComponent<Animator>().Play("DarkThorn");
                            }
                        //}
                        //attackStopMode--;
                        attackTimestamp = StaticDataCW.Time;
                    } else if (attackStopMode == 6) {
                        attackTimestamp = StaticDataCW.Time + 0.35f;
                    } else {
                        attackTimestamp = StaticDataCW.Time;
                        attackStopMode = -1;
                        attackType = 0;
                        animDirLock = false;
                    }
                    attackStopMode ++;
                } else if (attackType == 3 && StaticDataCW.Time > attackTimestamp + 0.84f) {
                    if (StaticDataCW.Time < attackTimestamp + 1.6f) {
                        if (attackStopMode == 0 && StaticDataCW.Time > attackTimestamp + 0.4f) {
                            GameObject attack = Instantiate(thornMechUppercut, transform) as GameObject;
                            attack.GetComponent<EnemyAttackCW>().type = 1;
                            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                            attack.GetComponent<EnemyAttackCW>().damage = 1f;
                            attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
                            attack.GetComponent<EnemyAttackCW>().knockbackValue = 14f;
                            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.5f,0f,0f);
                            attack.transform.localPosition = new Vector3(directionOfTravel, 0f, 0f);
                            attack.GetComponent<EnemyAttackCW>().deathtime = 0.25f;
                            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                            dashSpeed = 17f;
                            attackStopMode = 1;
                        }
                        xvel = dashSpeed * directionOfTravel;
                        dashSpeed -= (dashSpeed - Mathf.Lerp(dashSpeed, 0f, 0.09f))*Time.timeScale;
                        dashing = true;
                    } else {
                        xvel = 0;
                        attackStopMode = 0;
                        attackType = 0;
                        dashing = false;
                        animDirLock = false;
                        attackTimestamp = StaticDataCW.Time + 0.2f;
                    }
                }
            }
        }
        if (attackType == 0) {
            ed.waitDeath = false;
        }

        //xvel
        
        if (isWalledFront) {
            bool isWalledBottom = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,0), 0.02f, new Vector2(directionOfTravel,0)) &&
            !CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,solidsize.y), 0.02f, new Vector2(directionOfTravel,0));
            if (isWalledBottom && directionOfTravel != 0) {
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
                } else {
                    TurnAround();
                    xvel = 0f;
                    changeDirLock = true;
                }
            }
        }
        
        if (isGrounded && isOffGround && !prevGrounded && !offGroundLock)
        {
            offGroundLock = true;
        }

        if (offGroundLock && (fullyOnGround || !isGrounded)) {
            offGroundLock = false;
        }

        if (isGrounded) {
            if (!dashing) {
                xvel = 0;
            }
        }

        if (mode == 0) {
            speed = 1f;
        } else if (mode == 1) {
            if (!dashing && StaticDataCW.Time > attackTimestamp + 0.5f && !changeDirLock && StaticDataCW.Time > changeDirTimer && !animDirLock){
                directionOfTravel = (player.transform.position.x < transform.position.x)? -1:1;
                changeDirTimer = StaticDataCW.Time + 0.4f;
            }
        }
        
        xvelmove = xvelmove - (xvelmove * 0.25f * Time.timeScale);
        

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

        
        
        if(stunned || ed.dying){
            rb.velocity = new Vector2(xvelmove, rb.velocity.y);
        } else{
            rb.velocity = new Vector2(xvel + xvelmove, rb.velocity.y);
        }

        for (int i = prevposX.Length - 1; i > 0; i--){
            prevposX[i] = prevposX[i-1];
        }

        prevposX[0] = rb.position.x;
        prevxvelmove = xvelmove;
        
        //yvel

        if (isGrounded && rb.velocity.y <= 1E-03) {
            yvel = 0f;
        } else if (!isGrounded) {
            yvel -= 0.9f;
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

        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }

        /*if (guardPoint.GetComponent<FrostGuardPointCW>().health < 1) {
            ed.health = 0;
        }*/

        prevGrounded = isGrounded;
        prevFullyGrounded = fullyOnGround;
        prevIsOffGround = isOffGround;
        
        
        if (directionOfTravel == 1){
            sr.flipX = false;
        } else if (directionOfTravel == -1){
            sr.flipX = true;
        }
        frostGuardSprite.transform.localScale = Vector3.Lerp(frostGuardSprite.transform.localScale, new Vector3(1, 1, 1), 0.6f);

        anim.SetBool("Idle", attackType == 0);
        anim.SetBool("IdleLock", idlelock);
        anim.SetBool("AttackSide", attackFrame);
        anim.SetInteger("Attack", attackType);
        anim.SetBool("Dormant", dormant);
        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
        } else{
            anim.SetBool("Death", false);
        }
    }


    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("SceneBoundA") || other.CompareTag("SceneBoundB") || other.CompareTag("RoomBound")) {
            directionOfTravel = directionOfTravel * -1;
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            if (mode == 0) {
                attackType = 0;
            }
            mode = 1;
            dormant = false;
            goIntoModeTimestamp = StaticDataCW.Time;
            if (attackType == 0 && !inStabAnim && !changeDirLock && !animDirLock) {
                directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            }
            breakSightStamp = StaticDataCW.Time;
            frostMat.SetFloat("_Bool", 1);
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
        } else if (other.CompareTag("KnockbackDamage")) {
            if (other.GetComponent<Transform>().position.x < GetComponent<Transform>().position.x) {
                Xvel(7,true);
            } else {
                Xvel (-7,true);
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

    private bool CheckForPlayer(Vector2 point, float distance, Vector2 dir)
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
    private Vector2 SpawnCast(){
        RaycastHit2D spikePoint = Physics2D.Raycast(transform.position, new Vector2(0, -1), 30, groundMask | boundsMask);
        if (spikePoint.collider != null)
        {
            return spikePoint.point + new Vector2(0, (solidsize.y * 0.5f));
        } else{
            return transform.position;
        }
    }
}
