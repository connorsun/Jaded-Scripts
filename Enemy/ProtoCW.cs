using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoCW : MonoBehaviour
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
    public int dir;
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
    public GameObject protoAttack;
    public GameObject protoSprite;
    public Sprite cabbage1;
    public Sprite cabbage2;
    public Sprite cabbage3;
    private SpriteRenderer sr;
    private bool jumpCompleted;
    private int saveDir;
    private Animator anim;
    private bool atacking;
    private float attackStamp;

    private float breakSightStamp;
    private int framesSinceJump = 2;
    private int framesOnEdge = 0;
    public bool stop;
    public bool attacking;
    private BoxCollider2D solid;
    private bool permaStop = false;
    private float slamStamp = -9.0f;
    public int spawnDir;
    public float angle;
    private GameObject protoExplosion;
    private bool attacked;
    private bool slamCheck;
    private bool slamSpawnSprite;
    private GameObject protoExplosionWarn;
    private bool initialAttackWait;
    public float initialAttackWaitTime;
    public bool spawnInAir;
    private bool explode;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (spawnDir != 0) {
            dir = spawnDir;
        } else {
            dir = 1;
        }
        if (angle < 0.1f) {
            angle = Mathf.PI/4f;
        } else {
            angle = angle * Mathf.PI/180f;
        }
        permaStop = false;
        saveDir = dir;
        speed = 0f;
        stop = true;
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 3;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        ed.disableOrbDrop = false;
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);
        protoAttack = Resources.Load<GameObject>("Prefabs/Cabbage");
        cabbage1 = Resources.Load<Sprite>("Textures/cabbage-Sheet_0");
        cabbage2 = Resources.Load<Sprite>("Textures/cabbage-Sheet_1");
        cabbage3 = Resources.Load<Sprite>("Textures/cabbage-Sheet_2");
        attackStamp = StaticDataCW.Time;
        attacking = false;
        initialAttackWait = true;
        if (initialAttackWaitTime < 0.1f && initialAttackWaitTime > -0.1f) {
            initialAttackWaitTime = 2f;
        }
        
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.95f, 0.575f);
        solid.offset = new Vector2(0f, -0.23f);
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

    public void CreateObjects() {
        protoSprite = Instantiate(Resources.Load<GameObject>("Prefabs/ProtoSprite") as GameObject, gameObject.transform);
        protoSprite.transform.localPosition = new Vector3 (0,0,0);
        protoExplosion = Resources.Load<GameObject>("Prefabs/ProtoExplosion");
        protoExplosionWarn = Resources.Load<GameObject>("Prefabs/ProtoExplosionWarn");
        sr = protoSprite.GetComponent<SpriteRenderer>();
        anim = protoSprite.GetComponent<Animator>();
        ed.matDefault = Resources.Load("Materials/ProtoGlow") as Material;
        ed.sr = protoSprite.GetComponent<SpriteRenderer>();
        ed.sr.material = ed.matDefault; //WHEEEEE
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(0.5f*-solidsize.y) + solid.offset.y,0f);
        groundPoint.tag = "Groundpoint";
        ed.deathTime = 0.583f;
        ed.attachedObjects = new GameObject[2];
        ed.attachedObjects[0] = protoSprite;
        ed.attachedObjects[1] = groundPoint;
        mode = 0;
        EnableReset();
    }
    
    public void EnableReset() {
        rb.position = savePos;
        mode = 0;
        dir = saveDir;
        stop = true;
        attacking = false;
        attackStamp = StaticDataCW.Time;
        permaStop = false;
        ed.disableOrbDrop = false;
        initialAttackWait = true;
        xvel = 0;
        yvel = 0;
        explode = false;
        if (!spawnInAir) {
            rb.position = SpawnCast();
        }
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
        isWalled = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * -dir,0.5f*solidsize.y), 0.03f, Vector2.right * -dir) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * -dir,0), 0.03f, Vector2.right * -dir) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * -dir,solidsize.y), 0.03f, Vector2.right * -dir);
        bool isPlayerAhead = CheckForPlayer((Vector2)transform.position, 2.2f, dir * Vector2.right) ||
            CheckForPlayer((Vector2)transform.position, 2.2f, new Vector2(dir,0.2f).normalized) ||
            CheckForPlayer((Vector2)transform.position, 2.2f, new Vector2(dir,-0.2f).normalized);
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        bool slamDetection = CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 1f, Vector2.up) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 1f, Vector2.up) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 1f, Vector2.up);
        bool isPlayerAbove = CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x, 0), 5f, Vector2.up) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 5f, Vector2.up) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 4f, new Vector2(0.2f, 1f).normalized) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 4f, new Vector2(-0.2f, 1f).normalized) ||
                CheckForPlayer((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x, 0), 5f, Vector2.up);
        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0)) {
            rb.position = savePos;
            mode = 0;
            dir = saveDir;
            stop = true;
            attacking = false;
            attackStamp = StaticDataCW.Time;
            permaStop = false;
            ed.disableOrbDrop = false;
            initialAttackWait = true;
            xvel = 0;
            yvel = 0;
            explode = false;
        }

        /*if (slamDetection && player.GetComponent<PlayerControllerCW>().slamming && player.GetComponent<PlayerControllerCW>().slamState == 2 && mode != 1) {
            mode = 1;
            ed.health = 1000;
            slamStamp = StaticDataCW.Time;
            slamCheck = true;
            slamSpawnSprite = true;
        }*/
        if (mode == 1 && StaticDataCW.Time > slamStamp + 0.1665f && slamSpawnSprite) {
            Instantiate(protoExplosionWarn, transform);
            slamSpawnSprite = false;
        }
        if (mode == 1 && slamCheck && StaticDataCW.Time > slamStamp + 1f) {
            GameObject attack = Instantiate(protoExplosion, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().damage = 2f;
            attack.GetComponent<EnemyAttackCW>().dir = 2;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 15f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,0f,0f);
            attack.GetComponent<EnemyAttackCW>().setRotationManually = true;
            attack.GetComponent<EnemyAttackCW>().disabletime = 0.05f;
            attack.GetComponent<EnemyAttackCW>().deathtime = 1f;
            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
            attack.GetComponent<AnimationCW>().parent = gameObject;
            ed.disableOrbDrop = true;
            ed.health = 0;
            slamCheck = false;
            explode = true;
        }

        //attacks

        if (!ed.dying && mode == 0 && !attacking && playerDistance > 1.5f && !isPlayerAbove && ((initialAttackWait && StaticDataCW.Time > attackStamp + initialAttackWaitTime) ||  StaticDataCW.Time > attackStamp + 3f) ) {
            attacking = true;
            initialAttackWait = false;
            attackStamp = StaticDataCW.Time;
            attacked = false;
        }

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
            bool isWalledBottom = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * dir,0), 0.02f, new Vector2(dir,0)) &&
            !CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * dir,solidsize.y), 0.02f, new Vector2(dir,0));
            if (isWalledBottom) {
                float height;
                bool steppable = false;
                for (height = 0; height < 0.26f; height += 0.02f) {
                    if (!CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x * dir,height), 0.02f, new Vector2(dir,0))) {
                        steppable = true;
                        break;
                    }
                }
                if (steppable && !(CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*solidsize.x,solidsize.y), height, Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0f,solidsize.y), height, Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*solidsize.x,solidsize.y), height, Vector2.up))) {
                    rb.position = new Vector2(rb.position.x, rb.position.y + height);
                }
            } else {
                permaStop = true;
            }
        }
        if (fullyOnGround) {
            offGroundLock = false;
        }

        if (isGrounded && isOffGround && !prevGrounded && !offGroundLock)
        {
            offGroundLock = true;
        }

        if (offGroundLock && (fullyOnGround || !isGrounded)) {
            offGroundLock = false;
        }

        if (StaticDataCW.Time > attackStamp + 0.7f && attacking) {
            attacking = false;
        }

        if (StaticDataCW.Time > attackStamp + 0.325f && attacking && !attacked) {
            attacked = true;
            GameObject cabbage = Instantiate(protoAttack as GameObject, transform);
            cabbage.transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, 0f);
            switch (Random.Range(1,4)) {
                case 1:
                    cabbage.GetComponentsInChildren<SpriteRenderer>()[1].sprite = cabbage1;
                    break;
                case 2:
                    cabbage.GetComponentsInChildren<SpriteRenderer>()[1].sprite = cabbage2;
                    break;
                case 3:
                    cabbage.GetComponentsInChildren<SpriteRenderer>()[1].sprite = cabbage3;
                    break;
            }
            cabbage.GetComponent<CabbageCW>().dir = new Vector2(Mathf.Cos(angle) * dir, Mathf.Sin(angle)).normalized;
        }

        if (!attacking && mode == 0 && isPlayerAhead && !permaStop && !ed.dying) {
            stop = false;
            speed = 2;
        } else {
            stop = true;
            speed = 0;
        }

        if (isGrounded) {
            if (xvel < speed && dir == -1) {
                xvel += 0.75f;
            } else if (xvel > -speed && dir == 1) {
                xvel -= 0.75f;
            }
            if (xvel < -speed && dir == 1 || xvel > speed && dir == -1) {
                xvel = -dir * speed;
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

        protoSprite.transform.localScale = Vector3.Lerp(protoSprite.transform.localScale, new Vector3(1, 1, 1), 0.6f);
        if (dir == 1){
            sr.flipX = false;
        } else if (dir == -1){
            sr.flipX = true;
        }
        anim.SetBool("Slamming", mode == 1);
        anim.SetBool("Attacking", attacking);
        anim.SetBool("Moving", !stop);
        anim.SetBool("Dying", ed.dying);
        anim.SetBool("Explode", explode);
    }


    void OnTriggerEnter2D(Collider2D other) {

        if (other.CompareTag("SceneBoundA") || other.CompareTag("SceneBoundB") || other.CompareTag("RoomBound") || other.CompareTag("EnemyBlocker")) {
            permaStop = true;
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            if (ed.health > 0 && other.GetComponent<AttackCW>().type == 3 && mode != 1) {
                mode = 1;
                ed.health = 1000;
                slamStamp = StaticDataCW.Time;
                slamCheck = true;
                slamSpawnSprite = true;
            }
            protoSprite.transform.localScale = new Vector3 (0.7f,1.5f,1);
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
