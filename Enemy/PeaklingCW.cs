using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeaklingCW : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public float xv;
    public float yv;
    private float dir;
    public GameObject player;
    public Rigidbody2D playerrb;
    private EnemyDisablerCW ed;
    private Vector3 savePos;
    private bool up;
    private bool down;
    private bool left;
    private bool right;
    public GameObject groundPoint;
    //point at which raycasts originate for grounding
    public LayerMask groundMask;
    //mask to detect what will make onground true in the raycast
    private LayerMask playerMask;
    private LayerMask boundsMask;
    public float yveladd;
    //adds to yvel
    public float yvelset;
    //sets yvel to new number
    public bool canyveladd;
    //adds to yvel
    public bool canyvelset;
    //sets yvel to new number
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
    private GameObject damagebox;
    private Vector2 solidsize;
    private BoxCollider2D trigger;
    private bool stunned;
    private float intangStamp;
    private float changeDirStamp;
    private float changeDirTime;
    private int mode;
    private int prevmode;
    private bool playerInSight;
    private bool prevPlayerInSight;
    public float playerDistance;
    private float prevPlayerDistance;
    public float speed;
    private float sightTimer;
    public GameObject peaklingAttack;
    public GameObject peaklingSprite;
    public float attackCooldown = -9.0f;
    private float playerSightStamp;
    public bool lockMovement;
    private SpriteRenderer sr;

    private Animator anim;
    private bool flyAnim;
    public bool attackFrame;
    private bool attackAnimLock;
    private float attackLock;
    public bool attackAnimFramelock;
    private float alertStamp;
    private int targetDir;
    private BoxCollider2D solid;
    private bool attackCancelled;
    private bool noMoreAttacks;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 4;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        dir = Random.Range(0f,2f);
        if (Random.Range(0,2) == 0) {
            speed = 1.4f;
            targetDir = 1;
        } else {
            speed = -1.4f;
            targetDir = -1;
        }
        changeDirTime = 1f;
        changeDirStamp = StaticDataCW.Time;
        xv = 0f;
        yv = 0f;
        player = GameObject.Find("Player");
        playerrb = player.GetComponent<Rigidbody2D>();
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);

        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.9f, 0.75f);
        solid.isTrigger = false;

        CreateObjects();

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

        

        ed.deathTime = 0.5952f;
    }

    public void CreateObjects() {
        peaklingAttack = Resources.Load<GameObject>("Prefabs/PeaklingAttack");
        peaklingSprite = Instantiate(Resources.Load<GameObject>("Prefabs/PeaklingSprite") as GameObject, gameObject.transform);
        peaklingSprite.transform.localPosition = new Vector3 (0,0,0);
        sr = peaklingSprite.GetComponent<SpriteRenderer>();
        ed.sr = peaklingSprite.GetComponent<SpriteRenderer>();
        anim = peaklingSprite.GetComponent<Animator>();
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";
        ed.attachedObjects = new GameObject[2];
        ed.attachedObjects[0] = peaklingSprite;
        ed.attachedObjects[1] = groundPoint;
        EnableReset();
    }

    public void EnableReset() {
        if (Random.Range(0,2) == 0) {
            speed = 1.4f;
            targetDir = 1;
        } else {
            speed = -1.4f;
            targetDir = -1;
        }
        mode = 0;
        xv = 0f;
        yv = 0f;
        rb.position = savePos;
        attackCancelled = false;
        attackAnimFramelock = false;
        attackAnimLock = false;
        attackCooldown = -9.0f;
        anim.SetBool("Alert", false);
        anim.Play("IdleFly");
     }

    // Update is called once per frame
    void FixedUpdate()
    {
        up = CheckForWall((Vector2)groundPoint.transform.position + new Vector2(0.0f, solidsize.y), 0.05f, Vector2.up) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(solidsize.x/2f, solidsize.y), 0.05f, Vector2.up) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(-solidsize.x/2f, solidsize.y), 0.05f, Vector2.up);
        down = CheckForWall((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0f), 0.05f, -Vector2.up) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(solidsize.x/2f, 0f), 0.05f, -Vector2.up) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(-solidsize.x/2f, 0f), 0.05f, -Vector2.up);
        left = CheckForWall((Vector2)groundPoint.transform.position + new Vector2(-solidsize.x/2f, 0f), 0.05f, Vector2.left) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(-solidsize.x/2f, solidsize.y/2f), 0.05f, Vector2.left) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(-solidsize.x/2f, solidsize.y), 0.05f, Vector2.left);
        right = CheckForWall((Vector2)groundPoint.transform.position + new Vector2(solidsize.x/2f, 0), 0.05f, Vector2.right) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(solidsize.x/2f, solidsize.y/2f), 0.05f, Vector2.right) ||
                CheckForWall((Vector2)groundPoint.transform.position + new Vector2(solidsize.x/2f, solidsize.y), 0.05f, Vector2.right);
        if (GameObject.Find("GameController") != null && GameObject.Find("GameController").GetComponent<GameInitializerCW>().level == 5) {
            playerInSight = CheckForPlayer(transform.position, 12f);
        } else {
            playerInSight = CheckForPlayer(transform.position, 6f);
        }
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        if (ed.disabled) {
            xv = 0f;
            yv = 0f;
        }
        dir = (dir % 2*Mathf.PI)/Mathf.PI;
        if (up) {
            dir *= -1f;
            Yvel(-3f, true);
        } else if (down) {
            dir *= -1f;
            Yvel(3f,true);
        } else if (left) {
            if (dir >= 0f) {
                dir = 1f - dir;
            } else {
                dir = -1f + dir;
            }
            Xvel(3f,true);
        } else if (right) {
            if (dir >= 0f) {
                dir = 1f - dir;
            } else {
                dir = -1f + dir;
            }
            Xvel(-3f,true);
        }
        dir = dir * Mathf.PI;

        if (mode == 0) {
            if (Mathf.Abs(speed) >= 1.25f) {
                speed = 1.25f * targetDir;
            } else if (targetDir == 1) {
                speed += 0.1f; 
            } else  {
                speed -= 0.1f;
            }
            dir = 0f;
            if (StaticDataCW.Time > changeDirStamp + changeDirTime) {
                speed = 1.24f * targetDir;
                targetDir = -targetDir;
                changeDirStamp = StaticDataCW.Time;
                changeDirTime = 1f;
            }

            if (playerInSight) {
                mode = 1;
                attackLock = StaticDataCW.Time;
                attackCooldown = StaticDataCW.Time;
                attackAnimFramelock = false;
                attackAnimLock = false;
            }
        }
        if (playerDistance < 11f){
            if (mode == 0) {
                if (speed > 0f) {
                    sr.flipX = false;
                } else {
                    sr.flipX = true;
                }
            } else {
                if (player.transform.position.x > transform.position.x){
                    sr.flipX = false;
                } else{
                    sr.flipX = true;
                }
            }
        }
        if (StaticDataCW.Time > sightTimer + 0.5f && !attackAnimFramelock){
            lockMovement = false;
        }
        if (mode == 1) {
            speed = 1.25f;
            flyAnim = false;
            if (playerInSight) {
                playerSightStamp = StaticDataCW.Time;
            }
            if (playerDistance > 11f || StaticDataCW.Time > playerSightStamp + 0.5f) {
                mode = 0;
            } else if (prevmode != mode){
                lockMovement = true;
                sightTimer = StaticDataCW.Time;
            } else if (GameObject.Find("GameController") != null && GameObject.Find("GameController").GetComponent<GameInitializerCW>().level == 5 && playerDistance < 7f && !attackAnimFramelock && StaticDataCW.Time > attackLock + 0.2f && !noMoreAttacks) {
                attackFrame = true;
                attackAnimFramelock = true;
                attackCooldown = StaticDataCW.Time - 1.7f;
                attackCancelled = false;
                lockMovement = true;
            } else if (playerDistance > 3.5f) {
                /*if (rb.position.x < playerrb.position.x) {
                    dir = Mathf.Atan2((rb.position.y - playerrb.position.y), (rb.position.x - playerrb.position.x))/Mathf.PI;
                } else {
                    dir = -(Mathf.Atan2((rb.position.y - playerrb.position.y), (rb.position.x - playerrb.position.x))/Mathf.PI - Mathf.PI);
                }*/
                dir = Mathf.Atan2((playerrb.position.y - rb.position.y), (playerrb.position.x - rb.position.x));
            } else if (playerDistance > 3f && !noMoreAttacks) {
                if ((prevPlayerDistance <= 3f || prevPlayerDistance > 3.5f) && !attackAnimFramelock && StaticDataCW.Time > attackLock + 2f){
                    attackFrame = true;
                    attackAnimFramelock = true;
                    if (GameObject.Find("GameController") == null || GameObject.Find("GameController").GetComponent<GameInitializerCW>().level != 5) {
                        attackCooldown = StaticDataCW.Time - 1.7f;
                    }
                    attackCancelled = false;
                }
                lockMovement = true;
                if (StaticDataCW.Time > attackCooldown + 1.7f && !attackAnimFramelock){
                    attackFrame = true;
                    attackAnimFramelock = true;
                }
            } else {
                /*if (playerDistance < 2f && attackAnimFramelock && StaticDataCW.Time < attackCooldown + 2.2f) {
                    attackFrame = false;
                    attackAnimFramelock = false;
                    attackCancelled = true;
                }*/
                /*if (rb.position.x < playerrb.position.x) {
                    dir = Mathf.Atan((rb.position.y - playerrb.position.y)/(rb.position.x - playerrb.position.x));
                } else {
                    dir = -(Mathf.Atan((rb.position.y - playerrb.position.y)/(rb.position.x - playerrb.position.x)));
                }*/
                dir = Mathf.Atan2(-(playerrb.position.y - rb.position.y), -(playerrb.position.x - rb.position.x));
                flyAnim = true;
            }
        }

        if (StaticDataCW.Time > attackCooldown + 2.5f && attackAnimFramelock && !stunned && !ed.dying) {
            GameObject attack = Instantiate(peaklingAttack) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 2;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
                    /*if (rb.position.x < playerrb.position.x) {
                        attack.GetComponent<EnemyAttackCW>().dir = Mathf.Atan2((rb.position.y - playerrb.position.y), (rb.position.x - playerrb.position.x));
                    } else {
                        attack.GetComponent<EnemyAttackCW>().dir = -(Mathf.Atan2((rb.position.y - playerrb.position.y), (rb.position.x - playerrb.position.x)));
                    }*/
            attack.GetComponent<EnemyAttackCW>().dir = Mathf.Atan2((playerrb.position.y - rb.position.y) + Random.Range(-0.25f,-0.25f), (playerrb.position.x - rb.position.x) + Random.Range(-0.25f,0.25f));
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 5f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f,0f,0f);
            attack.GetComponent<EnemyAttackCW>().projSpeed = 4f;
            attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
            attack.GetComponent<EnemyAttackCW>().deathtime = 4f;
            attack.GetComponent<EnemyAttackCW>().projectileDeathTime = 0.1f;
            attackCooldown = StaticDataCW.Time;
            attackAnimFramelock = false;
            attackLock = StaticDataCW.Time;
            if (GameObject.Find("GameController") != null && GameObject.Find("GameController").GetComponent<GameInitializerCW>().level == 5) {
                noMoreAttacks = true;
            }
        }

        prevPlayerDistance = playerDistance;
        dir = dir % (2 * Mathf.PI);
        Vector2 directionVector = new Vector2(Mathf.Cos(dir),Mathf.Sin(dir));
        directionVector.Normalize();
        if (!lockMovement) {
            xv = Mathf.Lerp(xv,directionVector.x * speed,0.15f);
            yv = Mathf.Lerp(yv,directionVector.y * speed,0.15f);
        } else {
            xv = Mathf.Lerp(xv,0f,0.1f);
            yv = Mathf.Lerp(yv,0f,0.1f);
        }

        if (canyvelset == true){
            yv = yvelset;
            canyvelset = false;
        }

        if (canyveladd == true){
            yv += yveladd;
            canyveladd = false;
        }

        if (canxvelset == true){
            xv = xvelset;
            canxvelset = false;
        }

        if (canxveladd == true){
            xv += xveladd;
            canxveladd = false;
        }
    
        rb.velocity = new Vector2(xv,yv);

        if (PlayerControllerCW.resetting > 0 || (PlayerControllerCW.spikeResetting > 0)) {
            rb.position = savePos;
            yv = 0f;
            xv = 0f;
            if (Random.Range(0,1) == 0) {
                speed = 1.25f;
                targetDir = 1;
            } else {
                speed = -1.25f;
                targetDir = -1;
            }
            mode = 0;
            EnableReset();
        }
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }
        if (stunned && StaticDataCW.Time > intangStamp + 0.5f){
            stunned = false;
        }

        if (StaticDataCW.Time > attackCooldown + 1.7f && !attackCancelled){
            attackAnimLock = true;
        } else if (StaticDataCW.Time > attackCooldown + 0.6f) {
            attackAnimLock = false;
        }

        prevmode = mode;

        anim.SetBool("Direction", flyAnim);
        anim.SetBool("Hurt", ed.intangible);
        anim.SetBool("Attack", attackFrame);
        anim.SetBool("AttackLock", attackAnimLock);
        if (playerInSight != prevPlayerInSight && playerInSight){
            anim.SetBool("Alert", true);
            alertStamp = StaticDataCW.Time;
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Alert") && StaticDataCW.Time > alertStamp + anim.GetCurrentAnimatorStateInfo(0).length){
            anim.SetBool("Alert", false);
        }
            
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            attackFrame = false;
        }

        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
        } else{
            anim.SetBool("Death", false);
        }
        prevPlayerInSight = playerInSight;
    }
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("RoomBound") || other.CompareTag("EnemyBlocker") || other.gameObject.layer == 14){
            /*xv = xv*-1f;
            yv = yv*-1f;*/
        } else if (other.CompareTag("Attack") && !ed.intangible && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
            if (other.GetComponent<AttackCW>().dir == 2 || other.GetComponent<AttackCW>().crouching) {
                Yvel(4,true);
            } else if (other.GetComponent<AttackCW>().dir == -1) {
                Xvel(4,true);
            } else if (other.GetComponent<AttackCW>().dir == 1) {
                Xvel(-4,true);
            } else if (other.GetComponent<AttackCW>().dir == -2) {
                if (other.GetComponent<AttackCW>().dmgValue == 0) {
                    Yvel(-20,true);
                } else {
                    Yvel(-4,true);
                }
            } else if (other.CompareTag("Death")) {
                ed.health = 0;
            }
        }
    }
    private bool CheckForWall(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
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

    private bool CheckForPlayer(Vector2 point, float viewDistance)
    {
        Vector2 dir = (Vector2)player.transform.position - point;
        float checkDistance = dir.magnitude;
        dir.Normalize();
        RaycastHit2D check = Physics2D.Raycast(point, dir, viewDistance, playerMask | groundMask | boundsMask);
        if (check.collider != null && check.transform.gameObject.layer == 11)
        {
            if (checkDistance < viewDistance)
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
}
