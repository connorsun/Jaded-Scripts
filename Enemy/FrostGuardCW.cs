using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostGuardCW : MonoBehaviour
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
    public GameObject frostGuardAttack;
    public GameObject frostGuardAttack2;
    public GameObject frostGuardUpAttack;
    public GameObject frostGuardAOE;
    public GameObject frostGuardAOEGraphic;
    public GameObject frostGuardSprite;
    public GameObject frostGuardIcicle;
    public GameObject frostGuardShockwave;
    private SpriteRenderer sr;
    private bool jumpCompleted;
    private int saveDir;

    private float breakSightStamp;
    /*public GameObject guardPoint;
    private Vector3 guardPointPos;
    private BoxCollider2D guardPointHitbox;
    private CircleCollider2D guardPointDetector;
    private GameObject guardPointDetectionObj;*/
    private float playerInRadiusTimestamp = -9.0f;
    private float attackTimestamp = -9.0f;
    public bool stop;
    public float stopStamp;
    private float stopTime;
    private Vector2 endOfStopLock = new Vector2(0f,0f);
    public int attackStopMode;
    private float stopCooldownStamp = -9.0f;
    public float[] prevposX;
    private bool zoomLock;
    private int zoomSaveDir;
    private bool changeDirLock;
    private GameObject edgePoint1;
    private GameObject edgePoint2;
    public SpriteRenderer guardSprite;
    private BoxCollider2D solid;

    public Animator anim;
    private bool idlelock;
    private bool attackFrame;
    private bool edgeStop;
    private bool animDirLock;
    private bool inStabAnim;
    public Material frostMat;
    private int attackCounter;
    public bool spawnInAir;

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
        ed.maxhealth = 25;
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
        solid.size = new Vector2(1.25f, 3.5f);
        solid.isTrigger = false;
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        ed.matDefault = Resources.Load("Materials/FrostGuardGlow") as Material;
        
        CreateObjects();
        
        ed.deathTime = 0.66666666f;
        

        prevposX = new float[3];

        
    }

    public void EnableReset() {
        rb.position = savePos;
        mode = 0;
        stop = false;
        endOfStopLock = new Vector2(0f,0f);
        attackStopMode = 0;
        stopCooldownStamp = -9.0f;
        zoomLock = false;
        directionOfTravel = saveDir;
        edgeStop = false;
        attackCounter = 1;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
    }

    public void CreateObjects() {
        stopStamp = StaticDataCW.Time;
        stopTime = Random.Range(3f,16f);
        frostGuardAttack = Resources.Load<GameObject>("Prefabs/FrostGuardAttack");
        frostGuardAttack2 = Resources.Load<GameObject>("Prefabs/FrostGuardAttack2");
        frostGuardUpAttack = Resources.Load<GameObject>("Prefabs/FrostGuardUpAttack");
        frostGuardIcicle = Resources.Load<GameObject>("Prefabs/FrostGuardIcicle");
        frostGuardAOE = Resources.Load<GameObject>("Prefabs/FrostGuardAOE");
        frostGuardAOEGraphic = Resources.Load<GameObject>("Prefabs/FrostGuardAOEGraphic");
        frostGuardShockwave = Resources.Load<GameObject>("Prefabs/FrostGuardShockwave");
        frostGuardSprite = Instantiate(Resources.Load<GameObject>("Prefabs/FrostGuardSprite") as GameObject, gameObject.transform);
        frostGuardSprite.transform.localPosition = new Vector3(0,solid.size.y * -0.5f,0);
        sr = frostGuardSprite.GetComponent<SpriteRenderer>();
        anim = frostGuardSprite.GetComponent<Animator>();
        ed.sr = frostGuardSprite.GetComponent<SpriteRenderer>();
        ed.sr.material = ed.matDefault; //WHEEEEE
        zoomLock = false;
        changeDirLock = false;
        frostMat = frostGuardSprite.GetComponent<SpriteRenderer>().material;

        
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";

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
        

        //sets up guard point
        /*guardPoint = new GameObject(this.name + "GuardPoint");
        guardPoint.transform.position = new Vector2(transform.position.x,transform.position.y);
        guardPointPos = this.transform.position;
        guardPoint.tag = "GuardPoint";
        guardPoint.layer = 15;*/
        //guardSprite = guardPoint.AddComponent<SpriteRenderer>();
        //guardSprite.sprite = Resources.Load<Sprite>("Textures/frostpoint");
        /*guardPoint.AddComponent<FrostGuardPointCW>();
        guardPointHitbox = guardPoint.AddComponent<BoxCollider2D>();
        guardPointHitbox.size = new Vector2(0.5f,1.5f);
        guardPointHitbox.offset = new Vector2(0f,0.75f);
        guardPointHitbox.isTrigger = true;
        guardPointDetectionObj = new GameObject(this.name + "GuardPointDetection");
        guardPointDetectionObj.transform.position = new Vector2(transform.position.x,transform.position.y);
        guardPointDetectionObj.AddComponent<FrostGuardDetectorCW>();
        guardPointDetector = guardPointDetectionObj.AddComponent<CircleCollider2D>();
        guardPointDetector.radius = 1.25f;
        guardPointDetector.offset = new Vector2(0f,0f);
        guardPointDetector.isTrigger = true;
        guardPoint.GetComponent<FrostGuardPointCW>().health = 10;*/

        ed.attachedObjects = new GameObject[4];
        /*ed.attachedObjects[0] = guardPoint;
        ed.attachedObjects[1] = guardPointDetectionObj;*/
        ed.attachedObjects[0] = edgePoint1;
        ed.attachedObjects[1] = edgePoint2;
        ed.attachedObjects[2] = frostGuardSprite;
        ed.attachedObjects[3] = groundPoint;
        EnableReset();
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
        isWalled = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), 0.02f, -Vector2.right);
        bool isPlayerAhead = CheckForPlayer((Vector2)transform.position + new Vector2(0f, 1f), 6f, directionOfTravel * Vector2.right) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 1f), 6f, new Vector2(directionOfTravel,0.3f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 1f), 6f, new Vector2(directionOfTravel,-0.3f).normalized) ||
        CheckForPlayer((Vector2)transform.position + new Vector2(0f, 1f), 6f, new Vector2(directionOfTravel,-0.45f).normalized);
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
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        //point detection
        /*if (guardPointDetectionObj.GetComponent<FrostGuardDetectorCW>().playerInside) {
            mode = 2;
            playerInRadiusTimestamp = StaticDataCW.Time;
        }*/
        
        if (mode == 2 && StaticDataCW.Time > playerInRadiusTimestamp + 4f) {
            if (playerDistance < 6f) {
                mode = 1;
            } else {
                mode = 0;
                EndStop();
            }
        }

        if (mode == 1 && isPlayerAhead) {
            breakSightStamp = StaticDataCW.Time;
        }

        if (mode == 1 && StaticDataCW.Time > breakSightStamp + 4.5f && attackStopMode == 0 && !stop) {
            mode = 0;
            EndStop();
        }

        if (mode == 0 && (isPlayerAhead || playerDistance < 1.5f)) {
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
            stop = false;
            endOfStopLock = new Vector2(0f,0f);
            attackStopMode = 0;
            stopCooldownStamp = -9.0f;
            zoomLock = false;
            directionOfTravel = saveDir;
            edgeStop = false;
            attackCounter = 1;
        }

        //attacks
        
        
        if ((mode == 1 || mode == 2) && !stunned && !ed.dying) {
            if (StaticDataCW.Time > attackTimestamp + 1.5f && attackStopMode == 0) {
                if (Random.Range(1,5) != 1 && attackCounter > 1){
                    //SpawnAOEGraphic();
                    attackTimestamp = StaticDataCW.Time;
                    attackStopMode = 4;
                    stop = true;
                    stopStamp = StaticDataCW.Time;
                    stopTime = 9f;
                    attackCounter = 0;
                    //ed.waitDeath = true;
                } else {
                    attackTimestamp = StaticDataCW.Time;
                    attackStopMode = 1;
                    attackFrame = true;
                    stop = true;
                    stopStamp = StaticDataCW.Time;
                    stopTime = 3f;
                    attackCounter++;
                }
            } else if (attackStopMode == 4 && StaticDataCW.Time > attackTimestamp + 1f){
                SpawnAOE();
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 5;
            } else if (attackStopMode > 4 && StaticDataCW.Time > attackTimestamp + 2f) {
                if (attackStopMode == 8) {
                    attackStopMode = -1;
                    ed.waitDeath = false;
                    attackTimestamp = StaticDataCW.Time + 1f;
                } else {
                    attackTimestamp = StaticDataCW.Time;
                    PolygonCollider2D area = ed.roomBox.GetComponent<PolygonCollider2D>();
                    float x1 = area.points[0].x;
                    float y1 = area.points[0].y;
                    float areasize = 0;
                    float centerpos = 0;
                    foreach (Vector2 i in area.points) {
                        if (i.x != x1) {
                            areasize = Mathf.Abs(i.x - x1);
                            centerpos = ed.roomBox.transform.position.x + (i.x + x1)/2f;
                        }
                        if (i.y > y1) {
                            y1 = i.y;
                        }
                    }
                    int intsize = (int) areasize;
                    if (intsize > 0) {
                        intsize--;
                    }
                    float xspawn = (centerpos - intsize/2f);
                    int numgaps = (int) (intsize/3f);
                    List<int> gaps = new List<int>();
                    while (gaps.Count < numgaps) {
                        int rand = Random.Range(0, intsize);
                        if (!(gaps.Contains(rand))) {
                            gaps.Add(rand);
                        }
                    }
                    int gapcount = 0;
                    for (int i = 0; i < intsize; i++) {
                        if (!gaps.Contains(i) && gapcount < 3) {
                            gapcount++;
                            GameObject attack = Instantiate(frostGuardIcicle) as GameObject;
                            attack.GetComponent<EnemyAttackCW>().type = 2;
                            attack.transform.position = new Vector3(xspawn, transform.position.y + 5f, 0f);
                            attack.GetComponent<EnemyAttackCW>().damage = 1f;
                            attack.GetComponent<EnemyAttackCW>().dir = 3f*Mathf.PI/2f;
                            attack.GetComponent<EnemyAttackCW>().knockbackValue = 10f;
                            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,0f,0f);
                            attack.GetComponent<EnemyAttackCW>().projSpeed = 0f;
                            //attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.7f;
                            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                            attack.GetComponent<EnemyAttackCW>().deathtime = 10f;
                            attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.417f;
                        } else {
                            gapcount = 0;
                        }
                        xspawn += 1f;
                    }
                }
                attackStopMode++;
            /*} else if (StaticDataCW.Time > attackTimestamp + 0.5f && player.GetComponent<Transform>().position.y - transform.position.y < 5f && player.GetComponent<Transform>().position.y - transform.position.y > 1.5f && Mathf.Abs(player.GetComponent<Transform>().position.x - transform.position.x) < 2f && attackStopMode == 0) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 3;
                stop = true;
                stopStamp = StaticDataCW.Time;
                stopTime = 0.6f;
            } else if (attackStopMode == 3 && StaticDataCW.Time > attackTimestamp + 0.3f) { 
                SpawnUpAttack();
                attackTimestamp = StaticDataCW.Time + 0.5f;
                attackStopMode = 0;
                attackCounter++;
            } */
            //else if (attackStopMode == 0 && playerDistance < 2.5f && directionOfTravel == (player.transform.position.x > transform.position.x? 1:-1) && Mathf.Abs(player.GetComponent<Transform>().position.y - transform.position.y) < 2f) {
            } else if (attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 0.95f) {
                SpawnAttack();
                attackFrame = true;
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 2;
                /*if (!edgeStop) {
                    Xvel(10f * directionOfTravel, true);
                }*/
            } else if (attackStopMode == 2 && StaticDataCW.Time > attackTimestamp + 1.35f) {
                SpawnAttack();
                attackTimestamp = StaticDataCW.Time + 1.5f;
                attackStopMode = 0;
            }
        } else {
            if ((attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 1f) || (attackStopMode == 2 && StaticDataCW.Time > attackTimestamp + 0.95f)) {
                attackStopMode = 0;
                attackTimestamp = StaticDataCW.Time;
            }
        }

        if (mode == 2 && playerDistance > 6.0f && !stop) {
            mode = 0;
            EndStop();
            attackTimestamp = StaticDataCW.Time + 0.5f;
        }

        

        //xvel
        
        if (isWalled && !stop) {
            TurnAround();
            xvel = 0f;
            changeDirLock = true;
        }
        
        if (!stop && mode == 0 && StaticDataCW.Time > stopStamp + stopTime) {
            stop = true;
            stopStamp = StaticDataCW.Time;
            stopTime = Random.Range(1f,3f);
        } else if (stop && StaticDataCW.Time > stopStamp + stopTime) {
            EndStop();
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
            if (!stop) {
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

        if (mode == 0) {
            speed = 1f;
            anim.SetFloat("WalkMult", 1.0f);
        } else if (mode == 1) {
            if (!stop && StaticDataCW.Time > attackTimestamp + 0.5f && !changeDirLock){
                directionOfTravel = (player.transform.position.x < transform.position.x)? -1:1;
            }
            speed = 1.75f;
            anim.SetFloat("WalkMult", 1.25f);
            
        } else if (mode == 2) {
            if (!stop && StaticDataCW.Time > attackTimestamp + 0.5f && !changeDirLock){
                directionOfTravel = (player.transform.position.x < transform.position.x)? -1:1;
            }
            speed = 2.25f;
            anim.SetFloat("WalkMult", 1.3f);
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
        if (fullyOnGround) {
            /*if (!stop && zoomLock) {
                directionOfTravel = zoomSaveDir;
                mode = 0;
            }*/
            zoomLock = false;
        }

        if (((StaticDataCW.Time < attackTimestamp + 0.5f) || (xvelmove != 0)) && (isOffGround && !fullyOnGround)) {
            zoomLock = true;            
            /*for (int i = 0; i < prevposX.Length; i++){
                transform.position = new Vector2(prevposX[i], rb.position.y);
                left = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
                center = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up);
                right = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
                fullyOnGround = (center && left && right);
                if (fullyOnGround){
                    break;
                }
            }
            xvelmove = prevxvelmove * -1;
            */
            xvelmove = 0f;
            
            /*TurnAround();
            float turnAroundXvel = xvelmove;
            if (turnAroundXvel < 5f){
                turnAroundXvel = 5f;
            }
            Xvel(turnAroundXvel * directionOfTravel, true);*/
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

        anim.SetBool("Idle", stop);
        anim.SetBool("IdleLock", idlelock);
        anim.SetBool("Hurt", false);
        anim.SetBool("AttackSide", attackFrame);
        anim.SetInteger("Attack", attackStopMode);
        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
        } else{
            anim.SetBool("Death", false);
        }
        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Stab1") && attackStopMode == 1) || (anim.GetCurrentAnimatorStateInfo(0).IsName("Stab2") && attackStopMode == 2)){
            attackFrame = false;
        }
        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Stab2"))) {
            inStabAnim = true;
        } else {
            inStabAnim = false;
        }
        idlelock = stop;
    }


    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("SceneBoundA") || other.CompareTag("SceneBoundB") || other.CompareTag("RoomBound")) {
            directionOfTravel = directionOfTravel * -1;
            mode = 0;
            //EndStop();
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            mode = 1;
            breakSightStamp = StaticDataCW.Time;
            if (attackStopMode == 0 && !inStabAnim) {
                directionOfTravel = directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            }
            frostMat.SetFloat("_Bool", 1);
            //frostGuardSprite.transform.localScale = new Vector3 (0.8f,1.25f,1);
        } else if ((other.gameObject == edgePoint1 || other.gameObject == edgePoint2)) {
            stop = true;
            endOfStopLock = new Vector2(1f,directionOfTravel*-1f);
            stopStamp = StaticDataCW.Time;
            stopTime = 1.3f;
            Xvel(0, true);
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

    private Vector2 SpawnCast(){
        RaycastHit2D spikePoint = Physics2D.Raycast(transform.position, new Vector2(0, -1), 30, groundMask | boundsMask);
        if (spikePoint.collider != null)
        {
            return spikePoint.point + new Vector2(0, (solidsize.y * 0.5f));
        } else{
            return transform.position;
        }
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

    void SpawnAttack()
    {
        GameObject attack = Instantiate(frostGuardAttack, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 22f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3(2.125f,0f,0f);
        attack.transform.localPosition = new Vector3(2.125f * directionOfTravel, 0f, 0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 0.1f;
        attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
        attack = Instantiate(frostGuardAttack2, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 22f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,2.5f,0f);
        attack.transform.localPosition = new Vector3(2.125f * directionOfTravel, 0f, 0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 0.1f;
        attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
        for (int i = 0; i < 2; i ++) {
            GameObject shockwave = Instantiate(frostGuardShockwave, transform) as GameObject;
            if (i == 1) {
                shockwave.GetComponent<SpriteRenderer>().flipX = true;
            }
            shockwave.GetComponent<EnemyAttackCW>().type = 2;
            shockwave.transform.position = transform.position + new Vector3(2.125f * directionOfTravel, -solidsize.y/2 + 0.5f, 0f);
            shockwave.GetComponent<EnemyAttackCW>().damage = 1f;
            shockwave.GetComponent<EnemyAttackCW>().dir = i*Mathf.PI;
            shockwave.GetComponent<EnemyAttackCW>().knockbackValue = 10f;
            shockwave.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,0f,0f);
            shockwave.GetComponent<EnemyAttackCW>().projSpeed = 0.5f;
            shockwave.GetComponent<EnemyAttackCW>().additivevelocity = false;
            shockwave.GetComponent<EnemyAttackCW>().deathtime = 10f;
            shockwave.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.05f;
            shockwave.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.1f;
            shockwave.GetComponent<FrostGuardIcicleCW>().isntIcicle = true;
        }
    }

    void SpawnUpAttack()
    {
        GameObject attack = Instantiate(frostGuardUpAttack, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = 2;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 10f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3(-0.125f,2.3f,0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 0.175f;
        attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
    }

    void SpawnAOE()
    {
        GameObject attack = Instantiate(frostGuardAOE, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 30f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.55f,0.45f,0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 5.8f;
        attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
    }

    void SpawnAOEGraphic()
    {
        GameObject attack = Instantiate(frostGuardAOEGraphic, transform) as GameObject;
        attack.transform.position = gameObject.transform.position;
        attack.transform.parent = gameObject.transform;
        attack.GetComponent<AnimationCW>().parent = gameObject;
    }

    void EndStop() 
    {
        //float distToPoint = ((Vector2) transform.position - (Vector2) guardPointPos).magnitude;
        if (endOfStopLock.x == 1f) {
            directionOfTravel = (int) endOfStopLock.y;
            mode = 0;
            changeDirLock = true;
        } else {
            /*if (Random.Range(0f,1f) < distToPoint/8f && directionOfTravel == (transform.position.x > guardPointPos.x? 1:-1)) {
                //TurnAround();
            } */
        }
        endOfStopLock = new Vector2(0f,0f);
        stop = false;
        edgeStop = false;
        stopStamp = StaticDataCW.Time;
        stopTime = Random.Range(10f,20f);
        stopCooldownStamp = StaticDataCW.Time;
    }
}
