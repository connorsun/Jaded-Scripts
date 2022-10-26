using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrrmiteCW : MonoBehaviour
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
    private bool offGroundLock;
    //if you landed off the platform, keep going until you're regrounded
    private bool fullyOnGround;
    //for the offgroundlock
    private GameObject groundPoint;
    //point at which raycasts originate for grounding
    public LayerMask groundMask = 8;
    //mask to detect what will make onground true in the raycast
    public LayerMask boundsMask = 12;
    public bool isWalled;
    
    //physics
    public float yvel;
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

    public bool stunned = false;
    private float stunnedStamp = -9.0f;

    //reset enemy stuff or roombox stuff
    private Vector3 savePos;
    //enemy starting position

    //misc
    private float intangStamp;
    //stamps time for intangibility timer

    private GameObject damagebox;

    public GameObject player;

    private BoxCollider2D trigger;
    public int mode = 0;
    private int saveMode;
    private LayerMask playerMask;
    private float playerDistance;
    public GameObject brrmiteAttack;
    public GameObject brrmiteSprite;
    public GameObject brrmiteAttackSprite;
    private SpriteRenderer sr;
    private bool jumpCompleted;
    private int saveDir;
    private Animator anim;
    private bool biteFrame;
    private bool attackAnimLock;
    private float biteStamp;

    private float breakSightStamp;
    private int framesSinceJump = 2;
    private int framesOnEdge = 0;
    private bool stop;
    private float stopTime;
    private int attackStopMode;
    private float attackTimestamp;
    private BoxCollider2D solid;
    public bool spawnInAir;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        directionOfTravel = 1;
        saveDir = directionOfTravel;
        speed = 2f;
        rb = GetComponent<Rigidbody2D>();
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 5;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);
        brrmiteAttack = Resources.Load<GameObject>("Prefabs/BrrmiteAttack");
        brrmiteAttackSprite = Resources.Load<GameObject>("Prefabs/BrrmiteAOEGraphic");
        
        biteFrame = false;

        //sets up child collider
        /*
        damagebox = new GameObject(this.name + " Hitbox");
        damagebox.transform.parent = this.gameObject.transform;
        damagebox.transform.localPosition = new Vector3(0f,0f,0f);
        damagebox.tag = "Enemy";
        damagebox.layer = 0;
        */
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.5f, 0.3f);
        solid.offset = new Vector2(0f, -0.1f);
        solid.isTrigger = false;
        CreateObjects();
        /*
        trigger = damagebox.AddComponent<BoxCollider2D>();
        trigger.size = solid.size;
        trigger.offset = solid.offset;
        trigger.isTrigger = true;
        */

        //sets up groundpoint
        
    }
    
    public void EnableReset() {
        rb.position = savePos;
        mode = 0;
        directionOfTravel = saveDir;
        stop = false;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
    }

    public void CreateObjects() {
        brrmiteSprite = Instantiate(Resources.Load<GameObject>("Prefabs/BrrmiteSprite") as GameObject, gameObject.transform);
        brrmiteSprite.transform.localPosition = new Vector3 (0,0,0);
        sr = brrmiteSprite.GetComponent<SpriteRenderer>();
        anim = brrmiteSprite.GetComponent<Animator>();
        ed.sr = brrmiteSprite.GetComponent<SpriteRenderer>();
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(0.5f*-solidsize.y) + solid.offset.y,0f);
        groundPoint.tag = "Groundpoint";
        ed.deathTime = 0.1f;
        ed.attachedObjects = new GameObject[2];
        ed.attachedObjects[0] = brrmiteSprite;
        ed.attachedObjects[1] = groundPoint;
        EnableReset();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        bool left = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        bool center = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up);
        bool right = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 0.02f, -Vector2.up);
        isGrounded = (left || right || center);
        isOffGround = center && (!left || !right);
        fullyOnGround = (center && left && right);
        isWalled = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,0.5f*solidsize.y), 0.03f, Vector2.right * directionOfTravel) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,0), 0.03f, Vector2.right * directionOfTravel) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,solidsize.y), 0.03f, Vector2.right * directionOfTravel);
        bool isPlayerAhead = CheckForPlayer((Vector2)transform.position, 3f, directionOfTravel * Vector2.right) ||
            CheckForPlayer((Vector2)transform.position, 3f, new Vector2(directionOfTravel,0.2f).normalized) ||
            CheckForPlayer((Vector2)transform.position, 3f, new Vector2(directionOfTravel,-0.2f).normalized);
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));

        if (mode == 1 && isPlayerAhead) {
            breakSightStamp = StaticDataCW.Time;
        }
        
        if (framesSinceJump < 2) {
            framesSinceJump++;
        }

        if (mode == 1 && StaticDataCW.Time > breakSightStamp + 4.5f && fullyOnGround) {
            mode = 0;
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
            directionOfTravel = saveDir;
            stop = false;
        }

        //attacks

        if (playerDistance <= 1.5f && !ed.dying && attackStopMode == 0 && StaticDataCW.Time > biteStamp + 0.4f) {
            biteFrame = true;
            stop = true;
            stopTime = StaticDataCW.Time + 1.2f;
            attackStopMode = 1;
            attackTimestamp = StaticDataCW.Time;
            GameObject attackSprite = Instantiate(brrmiteAttackSprite, transform) as GameObject;
            attackSprite.transform.position = gameObject.transform.position;
            attackSprite.transform.parent = gameObject.transform;
            attackSprite.GetComponent<AnimationCW>().parent = gameObject;
        } else if (attackStopMode == 1 && StaticDataCW.Time > attackTimestamp + 1f && !ed.dying) {
            biteStamp = StaticDataCW.Time;
            GameObject attack = Instantiate(brrmiteAttack, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = (player.transform.position.x - transform.position.x >= 0f)? 1:-1;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 17f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,0f,0f);
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.1f;
            attackStopMode = 0;
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Up")) {
            biteFrame = false;
        }
        /*
        if (!isWalled && mode == 3) {
            jumpCompleted = true;
        }

        if (isGrounded && mode == 3 && framesSinceJump > 1) {
            if (jumpCompleted) {
                mode = saveMode;
            } else {
                mode = 0;
                TurnAround();
                xvel = 0f;
            }
        }
        */
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


        //xvel
        
        if (isWalled && !ed.dead && !ed.dying) {
            bool isWalledBottom = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,0), 0.02f, new Vector2(directionOfTravel,0)) &&
            !CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * directionOfTravel,solidsize.y), 0.02f, new Vector2(directionOfTravel,0));
            if (mode == 0) {
                TurnAround();
                xvel = 0f;
            }/* else if (isWalledBottom && directionOfTravel != 0) {
                float height;
                bool steppable = false;
                for (height = 0; height < 0.26f; height += 0.02f) {
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
            }*//* else if ((mode == 1 || mode == 2) && isGrounded) {
                saveMode = mode;
                mode = 3;
                saveDir = directionOfTravel;
                Yvel(8, true);
                brrmiteSprite.transform.localScale = new Vector3 (0.7f,1.5f,1);
                jumpCompleted = false;
                framesSinceJump = 0;
                xvel = speed * directionOfTravel;
            }*/
        }
        if (fullyOnGround) {
            offGroundLock = false;
        }
        if (isOffGround && isGrounded && prevGrounded && !offGroundLock && !ed.dead && !ed.dying) {
            TurnAround();
            xvel = -xvel;
            rb.position = new Vector3(rb.position.x + directionOfTravel * 0.1f, rb.position.y, 0f);
            offGroundLock = true;
        }

        if (isGrounded && isOffGround && !prevGrounded && !offGroundLock)
        {
            offGroundLock = true;
        }

        if (offGroundLock && (fullyOnGround || !isGrounded)) {
            offGroundLock = false;
        }
        if (stop && StaticDataCW.Time > stopTime) {
            stop = false;
        }

        if (StaticDataCW.Time > biteStamp + 0.3f) {
            attackAnimLock = false;
        } else {
            attackAnimLock = true;
        }

        if (stop) {
            speed = 0f;
        } else if (mode == 0) {
            speed = 2f;
        }/* else if (mode == 1) {
            directionOfTravel = (player.transform.position.x < transform.position.x)? -1:1;
            speed = 3f;
        } else if (mode == 2 && !attackAnimLock) {
            directionOfTravel = (player.transform.position.x < transform.position.x)? 1:-1;
            speed = 4f;
        } else if (mode == 3) {
            directionOfTravel = saveDir;
            speed = 1f;
        }*/

        if (isGrounded) {
            if (xvel < speed && directionOfTravel == 1) {
                xvel += 0.75f;
            } else if (xvel > -speed && directionOfTravel == -1) {
                xvel -= 0.75f;
            }
            if (xvel > speed && directionOfTravel == 1 || xvel < -speed && directionOfTravel == -1 || mode == 3) {
                xvel = directionOfTravel * speed;
            }
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
        if (stunned && StaticDataCW.Time > stunnedStamp + 0.4f){
            stunned = false;
            breakSightStamp = StaticDataCW.Time;
        }

        prevGrounded = isGrounded;

        brrmiteSprite.transform.localScale = Vector3.Lerp(brrmiteSprite.transform.localScale, new Vector3(1, 1, 1), 0.6f);
        if (directionOfTravel == 1){
            sr.flipX = false;
        } else if (directionOfTravel == -1){
            sr.flipX = true;
        }
        
        anim.SetBool("Grounded", isGrounded);
        anim.SetFloat("Yvel", yvel);
        anim.SetInteger("Mode", mode);
        anim.SetBool("Bite", biteFrame);
        anim.SetBool("BiteLock", stop);
        anim.SetBool("Hurt", ed.intangible);
        anim.SetBool("Dead", ed.dying);
    }


    void OnTriggerEnter2D(Collider2D other) {

        if (other.CompareTag("SceneBoundA") || other.CompareTag("SceneBoundB") || other.CompareTag("RoomBound") || other.CompareTag("EnemyBlocker")) {
            directionOfTravel = directionOfTravel * -1;
            mode = 0;
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            brrmiteSprite.transform.localScale = new Vector3 (0.7f,1.5f,1);
            if (other.GetComponent<AttackCW>().dir == 2 || other.GetComponent<AttackCW>().crouching) {
                Yvel(3,true);
            } else if (other.GetComponent<AttackCW>().dir == -1) {
                Xvel(5,true);
            } else if (other.GetComponent<AttackCW>().dir == 1) {
                Xvel(-5,true);
            } else if (other.GetComponent<AttackCW>().dir == -2) {
                Yvel(-3,true);
            }
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
