using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhyxnicSoldierCW : MonoBehaviour
{

    //components
    public int type;
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
    private bool isWalledRight;
    public bool isWalledLeft;
    
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

    public GameObject phyxnicSoldierAttack;
    public GameObject circleAttack;
    
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
    public Vector3 savePos;
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
    private bool leaping;
    private float breakSightStamp;
    private bool changeDirLock;
    private Vector2 endOfStopLock;
    private float attackTimestamp;
    private float stopCooldownStamp;
    private int attackStopMode;
    private bool zoomLock;
    private int zoomSaveDir;
    private GameObject edgePoint1;
    private GameObject edgePoint2;
    private float turnAroundStamp;
    private float leapingStamp = -9.0f;
    public int saveDir;
    private bool leapAttackable;
    private float jumpBackStamp = -9.0f;
    private float unlockSpeedStamp = -9.0f;
    private GameObject phyxnicSprite;
    public int initialSpawnDir;
    private Animator anim;
    private bool attackFrame;
    private bool circleAttackFrame;
    private BoxCollider2D solid;
    public bool spawnInAir;
    private GameObject phyxnicSoldierClubAttack;
    void Start()
    {
        groundMask |= (1 << 8);
        boundsMask |= (1 << 12);
        boundsMask |= (1 << 12);
        playerMask |= (1 << 11);
        phyxnicSoldierAttack = Resources.Load<GameObject>("Prefabs/PhyxnicSoldierAttack");
        phyxnicSoldierClubAttack = Resources.Load<GameObject>("Prefabs/PhyxnicSoldierClubAttack");
        circleAttack = Resources.Load<GameObject>("Prefabs/PhyxnicSoldierCircleattack");
        player = GameObject.FindWithTag("Player");
        if (initialSpawnDir != -1 && initialSpawnDir != 1) {
            directionOfTravel = 1;
        } else {
            directionOfTravel = initialSpawnDir;
        }
        saveDir = directionOfTravel;
        speed = 3f;
        rb = GetComponent<Rigidbody2D>();
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 8;
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
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.9f, 1.25f);
        solid.isTrigger = false;
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        

        

        ed.deathTime = 0.66666666f;
        ed.matDefault = Resources.Load("Materials/PhyxnicGlow") as Material;

        
        mode = 0;
        stop = false;

        CreateObjects();
    }

    public void EnableReset() {
        mode = 0;
        attackMode = 0;
        attackStopMode = 0;
        leaping = false;
        directionOfTravel = saveDir;
        jumpBackStamp = -9.0f;
        unlockSpeedStamp = -9.0f;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
    }

    public void CreateObjects() {
        if (type == 0) {
            phyxnicSprite = Instantiate(Resources.Load<GameObject>("Prefabs/PhyxnicSoldierSprite") as GameObject, gameObject.transform);
        } else {
            phyxnicSprite = Instantiate(Resources.Load<GameObject>("Prefabs/PhyxnicSoldierClubSprite") as GameObject, gameObject.transform);
        }
        phyxnicSprite.transform.localPosition = new Vector3(0,solid.size.y * -0.5f,0);
        anim = phyxnicSprite.GetComponent<Animator>();
        ed.sr = phyxnicSprite.GetComponent<SpriteRenderer>();
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

        
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";
        ed.attachedObjects = new GameObject[4];
        ed.attachedObjects[0] = edgePoint1;
        ed.attachedObjects[1] = edgePoint2;
        ed.attachedObjects[2] = phyxnicSprite;
        ed.attachedObjects[3] = groundPoint;
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
        isWalledRight = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), 0.02f, Vector2.right);
        isWalledLeft = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), 0.02f, -Vector2.right);
        //sightCast = SightCast((Vector2)gameObject.transform.position, 10f, Vector2.right) || SightCast((Vector2)gameObject.transform.position, 10f, -Vector2.right);
        sightCast = SightCast((Vector2)transform.position, 6f, ((Vector2) player.transform.position - rb.position).normalized);
        if (sightCast && mode == 0 && !ed.dying) {
            mode = 1;
            stopStamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
            //leapingStamp = StaticDataCW.Time;
            stopTime = 0.15f;
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
        }

        if (isGrounded && leaping) {
            leaping = false;
        }
        /*
            leapingStamp = StaticDataCW.Time;
            leapAttackable = true;
            attackTimestamp = StaticDataCW.Time;
            circleAttackFrame = false;
            
        }*/


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

        if (mode == 1 && StaticDataCW.Time > breakSightStamp + 4.5f && fullyOnGround) {
            mode = 0;
        }

        if ((leaping || (mode == 1 && StaticDataCW.Time > turnAroundStamp + 0.5f && attackStopMode == 0)) && !ed.dying) {
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            turnAroundStamp = StaticDataCW.Time;
        }

        /*if (attackStopMode < 2 && mode == 1 && isGrounded && StaticDataCW.Time > leapingStamp + 1.4f && player.transform.position.y >= transform.position.y + 0.5f && playerDistance < 4f) {
            Leap();
        }*/

        /*
        if (ed.intangible){
            trigger.enabled = false;
        } else{
            trigger.enabled = true;
        }
        */

        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0 && !ed.dying && !ed.dead)) {
            rb.position = ed.savePos;
            mode = 0;
            attackMode = 0;
            attackStopMode = 0;
            leaping = false;
            directionOfTravel = saveDir;
            jumpBackStamp = -9.0f;
            unlockSpeedStamp = -9.0f;
        
        }

        //attacks
        if (!leaping && !ed.dying) {
            if (isGrounded && mode == 1 && playerDistance < 1.7f && !stunned && StaticDataCW.Time > attackTimestamp + 0.5f && directionOfTravel == (player.transform.position.x > transform.position.x? 1:-1) && attackStopMode == 0) {
                if (type == 0) {
                    attackTimestamp = StaticDataCW.Time;
                } else if (type == 1) {
                    attackTimestamp = StaticDataCW.Time + 0.25f;
                }
                attackFrame = true;
                attackStopMode = 1;
                if (!stop) {
                    stop = true;
                    stopStamp = StaticDataCW.Time;
                    if (type == 0) {
                        stopTime = 1.2f;
                    } else if (type == 1) {
                        stopTime = 1.4f;
                    }
                }
            } else if (attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 0.7f) {
                SpawnAttack();
                if (type == 0) {
                    attackTimestamp = StaticDataCW.Time;
                    attackStopMode = 2;
                } else if (type == 1) {
                    attackTimestamp = StaticDataCW.Time + 0.5f;
                    attackStopMode = 3;
                }
            } else if (attackStopMode == 2 && StaticDataCW.Time > attackTimestamp + 0.3f) {
                SpawnAttack();
                attackTimestamp = StaticDataCW.Time + 0.5f;
                attackStopMode = 3;
            } else if (attackStopMode == 3 && StaticDataCW.Time > attackTimestamp - 0.2f) {
                attackTimestamp = StaticDataCW.Time + 0.5f;
                attackStopMode = 0;
                directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
                Yvel(8f, true);
                Xvel(-4f*directionOfTravel, true);
                mode = 0;
                stop = true;
                stopStamp = StaticDataCW.Time;
                stopTime = 0.7f;
                jumpBackStamp = StaticDataCW.Time;
                leapAttackable = true;
            }
        }/* else {
            Vector3 playerDir = (Vector2) player.transform.position - (Vector2) transform.position;
            playerDir.Normalize();
            if (playerDistance < 3f && !stunned && !ed.dying && StaticDataCW.Time > attackTimestamp + 0.2f && leapAttackable && SightCast(transform.position, 6f, (playerDir))) {
                circleAttackFrame = true;
                anim.SetBool("CircleAttack", true);
                attackTimestamp = StaticDataCW.Time;
                SpawnCircleAttack();
                leapAttackable = false;
            }
        }*/

        if (mode == 1 && playerDistance > 6.0f && !stop) {
            mode = 0;
            stop = true;
        }

        if (StaticDataCW.Time < jumpBackStamp + 0.6f) {
            Xvel(-4f*directionOfTravel, true);
        }
        
        //circle attack code
        if (StaticDataCW.Time > jumpBackStamp + 0.2f && !ed.dying && leapAttackable) {
            circleAttackFrame = true;
            anim.SetBool("CircleAttack", true);
            attackTimestamp = StaticDataCW.Time;
            SpawnCircleAttack();
            leapAttackable = false;
            attackStopMode = 0;
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
        
        if (((isWalledRight && directionOfTravel == 1) || (isWalledLeft && directionOfTravel == -1)) && mode != 0 && isGrounded && !ed.dying) {
            if (StaticDataCW.Time > leapingStamp + 0.3f) {
                leaping = true;
                Yvel(10f, true);
                attackStopMode = 0;
                attackTimestamp = StaticDataCW.Time;
                turnAroundStamp = StaticDataCW.Time;
                leapingStamp = StaticDataCW.Time;
                stop = false;
            } else {
                mode = 0;
                leapingStamp = -9.0f;
            }
            /*TurnAround();
            xvel = 0f;
            unlockSpeedStamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
            changeDirLock = true;*/
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

        if (mode == 0 && !leaping) {
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
        
        if(stunned || ed.dying){
            rb.velocity = new Vector2(xvelmove, rb.velocity.y);
        } else{
            rb.velocity = new Vector2(xvel + xvelmove, rb.velocity.y);
        }
        
        
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }
        if (stunned && StaticDataCW.Time > stunnedStamp + 0.4f && !ed.dying){
            stunned = false;
            mode = 1;
            turnAroundStamp = StaticDataCW.Time;
            directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
            breakSightStamp = StaticDataCW.Time;
        }
        if (directionOfTravel == 1){
            phyxnicSprite.GetComponent<SpriteRenderer>().flipX = false;
        } else if (directionOfTravel == -1){
            phyxnicSprite.GetComponent<SpriteRenderer>().flipX = true;
        }
        prevGrounded = isGrounded;
        prevIsOffGround = isOffGround;
        prevSightCast = sightCast;

        if (rb.velocity.x > -0.2 && rb.velocity.x < 0.2){
            anim.SetBool("Idle", true);
        } else{
            anim.SetBool("Idle", false);
        }
        anim.SetBool("Grounded", isGrounded);
        if (yvel < 0){
            anim.SetBool("Falling", true);
        } else{
            anim.SetBool("Falling", false);
        }
        anim.SetBool("Hurt", ed.intangible);
        anim.SetBool("Forwards", leaping);
        if (attackStopMode == 0){
            anim.SetBool("Attacking", false);
        } else{
            anim.SetBool("Attacking", true);
        }
        if (attackStopMode == 0){
            anim.SetBool("Attacking", false);
        } else{
            anim.SetBool("Attacking", true);
        }
        anim.SetBool("CircleFrame", circleAttackFrame);
        anim.SetBool("AttackFrame", attackFrame);
        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Stabs") && attackFrame)){
            attackFrame = false;
        }
        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("CircleAttack") && circleAttackFrame)){
            circleAttackFrame = false;
        }
        if (/*isGrounded || */StaticDataCW.Time > jumpBackStamp + 0.533f/* || StaticDataCW.Time > attackTimestamp + 0.33333333f*/){
            anim.SetBool("CircleAttack", false);
        }

        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
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
            if (attackStopMode == 0) {
                directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
                attackTimestamp = StaticDataCW.Time;
            }
            turnAroundStamp = StaticDataCW.Time;
            //leapingStamp = StaticDataCW.Time;
            if (other.GetComponent<AttackCW>().dir == 2 || other.GetComponent<AttackCW>().crouching) {
                Yvel(3,true);
            } else if (other.GetComponent<AttackCW>().dir == -1) {
                Xvel(4,true);
            } else if (other.GetComponent<AttackCW>().dir == 1) {
                Xvel(-4,true);
            }
        }  else if (mode == 1 && (other.gameObject == edgePoint1 || other.gameObject == edgePoint2) && isGrounded && attackStopMode == 0) {
            leaping = true;
            Yvel(10f, true);
            attackStopMode = 0;
            attackTimestamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
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

    void SpawnAttack()
    {
        if (type == 0) {
            GameObject attack = Instantiate(phyxnicSoldierAttack, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 7f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(1f,0f,0f);
            attack.transform.localPosition = new Vector3(directionOfTravel, 0f, 0f);
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.08f;
        } else {
            GameObject attack = Instantiate(phyxnicSoldierClubAttack, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 10f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.5f,0.45f,0f);
            attack.transform.localPosition = new Vector3(directionOfTravel, 0f, 0f);
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.2f;
        }
    }

    void SpawnCircleAttack()
    {
        GameObject attack = Instantiate(circleAttack, transform) as GameObject;
        attack.GetComponent<EnemyAttackCW>().type = 1;
        attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        attack.GetComponent<EnemyAttackCW>().damage = 1f;
        attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
        attack.GetComponent<EnemyAttackCW>().knockbackValue = 1f;
        attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.04f,0f,0f);
        attack.GetComponent<EnemyAttackCW>().deathtime = 0.12f;
        attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.1f;
    }

    void Leap()
    {
        leaping = true;
        Yvel(14f, true);
        attackStopMode = 0;
        attackTimestamp = StaticDataCW.Time;
        directionOfTravel = player.transform.position.x > gameObject.transform.position.x? 1:-1;
        turnAroundStamp = StaticDataCW.Time;
        leapAttackable = true;
    }
    private Vector2 SpawnCast(){
        RaycastHit2D spikePoint = Physics2D.Raycast(transform.position, new Vector2(0, -1), 30, groundMask | boundsMask);
        if (spikePoint.collider != null)
        {
            return spikePoint.point + new Vector2(0, (solidsize.y/2f));
        } else{
            return transform.position;
        }
    }
}
