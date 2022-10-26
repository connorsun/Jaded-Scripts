using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealineCW : MonoBehaviour
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
    public LayerMask waterMask;
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
    public int directionOfTravel;
    //direction it moves in
    private float speed;

    private int platformDir;

    public GameObject sealineAttack;
    public GameObject sealineLaser;
    
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
    public int attackStopMode;
    private bool zoomLock;
    private int zoomSaveDir;
    private GameObject edgePoint1;
    private GameObject edgePoint2;
    private float turnAroundStamp;
    private float leapingStamp;
    public int saveDir;
    private bool leapAttackable;
    private float jumpBackStamp = -9.0f;
    private float unlockSpeedStamp = -9.0f;
    private GameObject sealineSprite;
    public int initialSpawnDir;
    private Animator anim;
    private bool attackFrame;
    private bool circleAttackFrame;
    private BoxCollider2D solid;
    public bool spawnInAir;
    private bool playerOnRight;
    private Vector2 centerPos;
    private GameObject currentAttack;
    private bool flipping;
    private bool reswitch;
    private bool flipSr;
    private bool teleporting;
    private Vector2 teleportPos;
    private float teleportStamp = -9.0f;
    private bool teleportFrame;
    private LayerMask enemyMask;
    private float randWaitTime;

    void Start()
    {
        sealineAttack = Resources.Load<GameObject>("Prefabs/SealineAttack");
        sealineLaser = Resources.Load<GameObject>("Prefabs/SealineLaser");
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
        ed.health = ed.maxhealth;
        savePos = rb.position;
        ed.savePos = rb.position;
        ed.maxhealth = 13;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;

        //sets up child collider
        /*
        damagebox = new GameObject(this.name + " Hitbox");
        damagebox.transform.parent = this.gameObject.transform;
        damagebox.transform.localPosition = new Vector3(0f,0f,0f);
        damagebox.tag = "Enemy";
        damagebox.layer = 0;
        */
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(1f, 2.5f);
        solid.offset = new Vector2(0f, -0.25f);
        solid.isTrigger = false;
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        

        

        ed.deathTime = 0.6f;
        ed.matDefault = Resources.Load("Materials/ProtoGlow") as Material;

        groundMask |= (1 << 8);
        boundsMask |= (1 << 12);
        boundsMask |= (1 << 12);
        playerMask |= (1 << 11);
        waterMask |= (1 << 24);
        enemyMask |= (1 << 10);
        mode = 0;
        stop = false;

        CreateObjects();
    }

    public void EnableReset() {
        mode = 0;
        attackMode = 0;
        attackStopMode = 0;
        attackTimestamp = StaticDataCW.Time;
        leaping = false;
        directionOfTravel = saveDir;
        jumpBackStamp = -9.0f;
        unlockSpeedStamp = -9.0f;
        flipping = false;
        reswitch = false;
        flipSr = false;
        teleporting = false;
        teleportStamp = -9.0f;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        randWaitTime = Random.Range(0f, 0.3f);
        solid.enabled = true;
        teleportFrame = false;
    }

    public void CreateObjects() {
        sealineSprite = Instantiate(Resources.Load<GameObject>("Prefabs/SealineSprite") as GameObject);
        sealineSprite.transform.SetParent(transform);
        sealineSprite.transform.localPosition = Vector3.zero;
        anim = sealineSprite.GetComponent<Animator>();
        ed.sr = sealineSprite.GetComponent<SpriteRenderer>();
        ed.sr.material = ed.matDefault; //WHEEEEE
        //gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/cabbage-Sheet_1");
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
        ed.attachedObjects[2] = sealineSprite;
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
        isWalled = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,0), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), 0.02f, Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0.5f*solidsize.y), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,0), 0.02f, -Vector2.right) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), 0.02f, -Vector2.right);
        playerOnRight = (player.transform.position.x > ed.sr.transform.position.x)? true:false;
        //sightCast = SightCast((Vector2)gameObject.transform.position, 10f, Vector2.right) || SightCast((Vector2)gameObject.transform.position, 10f, -Vector2.right);
        sightCast = SightCast((Vector2)transform.position, 10f, ((Vector2) player.transform.position - rb.position).normalized);
        if (sightCast && mode == 0) {
            mode = 1;
            stopStamp = StaticDataCW.Time;
            turnAroundStamp = StaticDataCW.Time;
            leapingStamp = StaticDataCW.Time;
            attackTimestamp = StaticDataCW.Time;
            Teleport();
        }

        if (mode == 1 && prevmode == 0){
            sightCastStamp = StaticDataCW.Time;
            sightCastTime = 0.1f;
        }
        prevmode = mode;

        if (mode == 1 && sightCast) {
            breakSightStamp = StaticDataCW.Time;
        }

        if (mode == 0) {
            attackStopMode = 0;
        }

        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0 && !ed.dying && !ed.dead)) {
            rb.position = ed.savePos;
            mode = 0;
            attackMode = 0;
            attackStopMode = 0;
            leaping = false;
            directionOfTravel = saveDir;
            jumpBackStamp = -9.0f;
            unlockSpeedStamp = -9.0f;
            flipping = false;
            reswitch = false;
            flipSr = false;
            attackTimestamp = StaticDataCW.Time;
            teleporting = false;
            teleportStamp = -9.0f;
            randWaitTime = Random.Range(0f, 0.3f);
            solid.enabled = true;
            teleportFrame = false;
        }

        //attacks
        if (!ed.dying) {
            if (mode == 1 && StaticDataCW.Time > attackTimestamp + 2.1f + randWaitTime && attackStopMode == 0) {
                randWaitTime = Random.Range(0f, 0.3f);
                attackTimestamp = StaticDataCW.Time;
                attackFrame = true;
                if (Random.Range(0, 3) == 0) {
                    attackStopMode = 5;
                } else {
                    attackStopMode = 1;
                }
            } else if (attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 0.8f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 2;
            } else if (attackStopMode == 2 && StaticDataCW.Time > attackTimestamp + 0.3f) {
                SpawnAttack(/*VecToDeg(((Vector2) player.transform.position - rb.position).normalized), 0.8f, 0.65f*/);
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 3;
            } else if (attackStopMode == 3 && StaticDataCW.Time > attackTimestamp + 0.5f) {
                SpawnLaser();
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 4;
            } else if (attackStopMode == 4 && StaticDataCW.Time > attackTimestamp + 0.7f) {
                Teleport();
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 0;
            } else if (attackStopMode == 5 && StaticDataCW.Time > attackTimestamp + 0.8f) {
                //SpawnAttack(/*VecToDeg(((Vector2) player.transform.position - rb.position).normalized), 0.3f, 0.65f*/);
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 6;
            } else if (attackStopMode == 6 && StaticDataCW.Time > attackTimestamp + 0.3f) {
                Teleport();
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 7;
            } else if (attackStopMode == 7 && StaticDataCW.Time > attackTimestamp + 1.5f) {
                SpawnAttack(/*VecToDeg(((Vector2) player.transform.position - rb.position).normalized), 0.4f, 3f*/);
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 8;
                teleportFrame = false;
            } else if (attackStopMode == 8 && StaticDataCW.Time > attackTimestamp + 0.5f) {
                SpawnLaser();
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 9;
            } else if (attackStopMode == 9 && StaticDataCW.Time > attackTimestamp + 0.7f) {
                attackTimestamp = StaticDataCW.Time;
                attackStopMode = 0;
            }
        }

        anim.SetBool("Shooting", attackStopMode == 2 || attackStopMode == 3 || attackStopMode == 4 || attackStopMode == 6 || (attackStopMode == 7 && StaticDataCW.Time > attackTimestamp + 1.2f) || attackStopMode == 8 || attackStopMode == 9);

        if (teleporting && StaticDataCW.Time > teleportStamp + 0.8f) {
            float minDist = 1000f;
            Vector2 minPoint = rb.position;
            for (int i = 0; i < 4; i++) {
                Vector2 newPoint = ReturnGrounded((Vector2) groundPoint.transform.position + i*0.25f*Vector2.up + solidsize.x*0.55f*(playerOnRight?Vector2.right:Vector2.left), Random.Range(2f, 5f) + Mathf.Abs(player.transform.position.x - transform.position.x), playerOnRight?Vector2.right: Vector2.left);
                newPoint.y = transform.position.y;
                if ((newPoint - rb.position).magnitude < minDist) {
                    minPoint = newPoint;
                    minDist = (newPoint - rb.position).magnitude;
                }
            }
            
            if (playerOnRight) {
                while (!CheckGrounded(new Vector2(minPoint.x, groundPoint.transform.position.y), 10f, -Vector2.up) && minPoint.x > transform.position.x) {
                    print(new Vector2(minPoint.x, groundPoint.transform.position.y));
                    minPoint.x -= (playerOnRight?1f: -1f)*0.25f;
                }
            } else {
                while (!CheckGrounded(new Vector2(minPoint.x, groundPoint.transform.position.y), 10f, -Vector2.up) && minPoint.x < transform.position.x) {
                    print(new Vector2(minPoint.x, groundPoint.transform.position.y));
                    minPoint.x -= (playerOnRight?1f: -1f)*0.25f;
                }
            }

            if (playerOnRight) {
                if (minPoint.x > transform.position.x) {
                    rb.position = minPoint;
                }
            } else {
                if (minPoint.x < transform.position.x) {
                    rb.position = minPoint;
                }
            }

            directionOfTravel = (player.transform.position.x > minPoint.x)? -1: 1;
            teleportFrame = true;
            teleporting = false;
            solid.enabled = true;
        }
        if (teleporting) {
            ed.hideLock = true;
        } else {
            ed.hideLock = false;
        }

        if (attackStopMode == 0 || attackStopMode == 1 || attackStopMode == 5) {
            directionOfTravel = playerOnRight? -1: 1;
        }

        ed.sr.flipX = directionOfTravel >= 0? true: false;

        //yvel
        //no

        //xvel
        
        
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

        xvel = 0f;

        
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
        prevGrounded = isGrounded;
        prevIsOffGround = isOffGround;
        prevSightCast = sightCast;

        
        anim.SetBool("Idle", attackStopMode == 0);
        anim.SetBool("Charging", attackStopMode == 1 || attackStopMode == 5);
        anim.SetBool("Teleporting", teleporting);
        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
        } else{
            anim.SetBool("Death", false);
        }
        anim.SetBool("TeleportFrame", teleportFrame);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Teleport Out") || anim.GetCurrentAnimatorStateInfo(0).IsName("Placeholder") || anim.GetCurrentAnimatorStateInfo(0).IsName("Shoot")){
            teleportFrame = false;
        }
        
    }


    void OnTriggerEnter2D(Collider2D other) {

        if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
        } else if (other.CompareTag("KnockbackDamage")) {
            ed.health = 0;
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

    private Vector2 ReturnGrounded(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, distance + 2f, groundMask | boundsMask | waterMask | enemyMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance)
            {
                return groundPoint.point - dir*new Vector2(0.5f, 0);
            }
        }
        return point + dir*distance;
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


    void SpawnAttack(/*float dirShoot, float deathtime, float initSpeed*/)
    {
        currentAttack = Instantiate(sealineAttack, transform) as GameObject;
        currentAttack.GetComponent<EnemyAttackCW>().type = 1;
        currentAttack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
        currentAttack.GetComponent<EnemyAttackCW>().damage = 1f;
        currentAttack.GetComponent<EnemyAttackCW>().knockbackValue = 8f;
        currentAttack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.7f,0.3f,0f);
        currentAttack.transform.localPosition = new Vector3(0, 0f, 0f);
        currentAttack.GetComponent<EnemyAttackCW>().deathtime = 0.2f;
        currentAttack.GetComponent<EnemyAttackCW>().dir = directionOfTravel;
        //currentAttack.GetComponent<EnemyAttackCW>().setRotationManually = true;
        //currentAttack.GetComponent<SealineAttackCW>().dir = dirShoot;
        //currentAttack.transform.rotation = Quaternion.Euler(0f, 0f, dirShoot);
        //currentAttack.GetComponent<SealineAttackCW>().initialSpeed = initSpeed;
    }

    void SpawnLaser()
    {
        float minDist = 1000f;
        Vector2 minPoint = rb.position;
        for (int i = 0; i < 4; i++) {
            Vector2 newPoint = ReturnGrounded((Vector2) groundPoint.transform.position + i*0.25f*Vector2.up + solidsize.x*0.55f*(directionOfTravel == -1?Vector2.right:Vector2.left), Random.Range(2f, 5f) + Mathf.Abs(player.transform.position.x - transform.position.x), directionOfTravel == -1?Vector2.right: Vector2.left);
            newPoint.y = transform.position.y;
            if ((newPoint - rb.position).magnitude < minDist) {
                minPoint = newPoint;
                minDist = (newPoint - rb.position).magnitude;
            }
        }

        if (minDist > 2f) {
            print(minDist);
            GameObject attack = Instantiate(sealineLaser) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 2;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = directionOfTravel == 1? Mathf.PI:0;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 20f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(directionOfTravel == 1? -0.34385f:0.34385f,-0.015024f,0f);
            attack.GetComponent<EnemyAttackCW>().projSpeed = 31f;
            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
            attack.GetComponent<EnemyAttackCW>().deathtime = 4f;
            attack.GetComponent<EnemyAttackCW>().reenableBoxColliderTime = 0.05f;
            attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0f;
            attack.GetComponent<EnemyAttackCW>().dontDestroyOnPlayer = true;
            attack.GetComponent<SpriteRenderer>().flipX = directionOfTravel == 1;
        }
    }
    private float VecToDeg(Vector2 vec)
     {
         if (vec.x < 0)
         {
             return 360 - (Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg * -1);
         }
         else
         {
             return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
         }
     }

    void Teleport() {
        teleporting = true;
        teleportFrame = true;
        teleportStamp = StaticDataCW.Time;
        
        solid.enabled = false;
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
