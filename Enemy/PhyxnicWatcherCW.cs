using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PhyxnicWatcherCW : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public float xv;
    public float yv;
    public Vector2 dir;
    public GameObject player;
    public Rigidbody2D playerrb;
    private EnemyDisablerCW ed;
    private Vector3 savePos;
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
    private float solidsize;
    private CircleCollider2D trigger;
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
    public GameObject phyxnicWatcherAttack;
    public GameObject peaklingSprite;
    public float attackCooldown = -9.0f;
    private float playerSightStamp;
    public bool lockMovement;
    private SpriteRenderer sr;

    private Animator anim;
    private bool flyAnim;
    public bool attackFrame;
    private bool attackAnimLock;
    private bool attackLock;
    public bool attackAnimFramelock;
    private float alertStamp;
    private int targetDir;
    private CircleCollider2D solid;
    public Transform followPoint;
    public float nextWaypointDistance;
    private Path path;
    public int currentWaypoint = 0;
    private bool atEndOfPath = false;
    private Seeker seeker;
    private float pathRefreshStamp;
    public int turretCount = 0;
    private float spawnTurretStamp = -9.0f;
    private float attackStamp = -9.0f;
    private Vector2 chargeDir;
    private GameObject turret;
    private bool attackPause;
    private float attackPauseStamp;
    private GameObject attack;
    private float attackEndStamp = -9.0f;
    private bool attackEnding;
    private float wallTestStamp = -9.0f;
    private bool wallTesting;
    private float enterModeStamp = -9.0f;
    private bool enterMode;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 14;
        ed.orbcount = 2;
        ed.health = ed.maxhealth;
        xv = 0f;
        yv = 0f;
        player = GameObject.Find("Player");
        playerrb = player.GetComponent<Rigidbody2D>();
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        groundMask |= (1 << 20);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);
        nextWaypointDistance = 0.1f;
        turretCount = 0;

        solid = gameObject.GetComponent<CircleCollider2D>();
        solid.radius = 1f;
        solid.isTrigger = false;

        ed.matDefault = Resources.Load("Materials/PhyxnicGlow") as Material;

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

        

        ed.deathTime = 0.8f;
    }

    public void CreateObjects() {
        phyxnicWatcherAttack = Resources.Load<GameObject>("Prefabs/PhyxnicWatcherAttack");
        peaklingSprite = Instantiate(Resources.Load<GameObject>("Prefabs/PhyxnicWatcherSprite") as GameObject, gameObject.transform);
        turret = Resources.Load<GameObject>("Prefabs/PhyxnicTurret");
        peaklingSprite.transform.localPosition = new Vector3 (0,0,0);
        sr = peaklingSprite.GetComponent<SpriteRenderer>();
        ed.sr = peaklingSprite.GetComponent<SpriteRenderer>();
        ed.sr.material = ed.matDefault; //WHEEEEE
        anim = peaklingSprite.GetComponent<Animator>();
        seeker = gameObject.AddComponent<Seeker>();
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.radius;
        groundPoint.transform.localPosition = new Vector3(0f,-solidsize,0f);
        groundPoint.tag = "Groundpoint";
        GameObject follower = new GameObject(this.name + " Follower");
        followPoint = follower.transform;
        ed.attachedObjects = new GameObject[3];
        ed.attachedObjects[0] = peaklingSprite;
        ed.attachedObjects[1] = groundPoint;
        ed.attachedObjects[2] = follower;
        EnableReset();
    }

    public void EnableReset() {
        mode = 0;
        xv = 0f;
        lockMovement = true;
        yv = 0f;
        rb.position = savePos;
        spawnTurretStamp = -9.0f;
        attackStamp = StaticDataCW.Time;
        attackPause = false;
        attackEndStamp = -9.0f;
        attackEnding = false;
        anim.Play("Idle");
        wallTesting = false;
        attackLock = false;
        enterMode = false;
     }

    void OnPathComplete(Path p) {
        path = p;
        currentWaypoint = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //failsafe
        if (turretCount < 0) {
            turretCount = 0;
        }
        playerInSight = CheckForPlayer(transform.position, 6f);
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        if (ed.disabled) {
            xv = 0f;
            yv = 0f;
        }

        if (mode == 0) {
            lockMovement = true;
            if (playerDistance <= 5f) {
                mode = 1;
                attackPause = false;
                if (playerDistance < 1.5f) {
                    followPoint.position = playerrb.position + (rb.position - playerrb.position).normalized * 1.5f;
                } else {
                    followPoint.position = playerrb.position;
                }
                seeker.StartPath(rb.position, followPoint.position, OnPathComplete);
                pathRefreshStamp = StaticDataCW.Time;
                spawnTurretStamp = StaticDataCW.Time - 2f;
                attackStamp = StaticDataCW.Time;
                lockMovement = false;
                enterModeStamp = StaticDataCW.Time;
                enterMode = true;
            }
        }
        
        if (StaticDataCW.Time > enterModeStamp + 0.17f && enterMode) {
            enterMode = false;
        }
        
        
        if (playerDistance < 11f){
            if (mode == 1 && !attackLock) {
                if (player.transform.position.x > transform.position.x){
                    sr.flipX = false;
                } else{
                    sr.flipX = true;
                }
            }
        }
        if (mode == 1) {
            speed = 2.5f;
            if (!attackLock) {
                if (path != null && currentWaypoint >= path.vectorPath.Count) {
                    atEndOfPath = true;
                } else {
                    if (!attackPause && !attackEnding) {
                        lockMovement = false;
                    }
                    atEndOfPath = false;
                    if (path != null) {
                        dir = ((Vector2) path.vectorPath[currentWaypoint] - rb.position).normalized;
                    }
                    if (path != null &&( Vector2.Distance(rb.position, path.vectorPath[currentWaypoint])) < nextWaypointDistance) {
                        currentWaypoint++;
                    }
                }
                if (StaticDataCW.Time > pathRefreshStamp + 0.5f) {
                    if (seeker.IsDone()) {
                        if (playerDistance < 1.5f) {
                            followPoint.position = playerrb.position + (rb.position - playerrb.position).normalized * 1.5f;
                        } else {
                            followPoint.position = playerrb.position;
                        }
                        seeker.StartPath(rb.position, followPoint.position, OnPathComplete);
                    }
                    pathRefreshStamp = StaticDataCW.Time;
                }
                if (playerDistance >= 11f) {
                    mode = 0;
                    lockMovement = true;
                }
                if (StaticDataCW.Time < spawnTurretStamp + 4.5f) {
                    anim.SetInteger("Mode", 0);
                }
                if (StaticDataCW.Time > attackStamp + 1f + 0.5f * turretCount && !attackPause && !stunned && !ed.dying && playerInSight) {
                    attackPauseStamp = StaticDataCW.Time;
                    attackPause = true;
                    lockMovement = true;
                }
                if (StaticDataCW.Time > attackPauseStamp + 1f && attackPause) {
                    attackPause = false;
                    attackLock = true;
                    lockMovement = false;
                    chargeDir = (playerrb.position - rb.position).normalized;
                    attack = Instantiate(phyxnicWatcherAttack) as GameObject;
                    attack.GetComponent<EnemyAttackCW>().type = 1;
                    attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
                    attack.GetComponent<EnemyAttackCW>().damage = 1f;
                    attack.GetComponent<EnemyAttackCW>().dir = Mathf.Atan2(chargeDir.y, chargeDir.x);
                    attack.GetComponent<EnemyAttackCW>().knockbackValue = 17f;
                    attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0f, 0f ,0f);
                    attack.GetComponent<EnemyAttackCW>().additivevelocity = false;
                    attack.GetComponent<EnemyAttackCW>().deathtime = 5f;
                    attack.transform.parent = this.transform;
                    attackStamp = StaticDataCW.Time;
                }
                if (StaticDataCW.Time > spawnTurretStamp + 4.5f) {
                    GameObject newTurret = Instantiate(turret) as GameObject;
                    newTurret.GetComponent<PhyxnicTurretCW>().parent = gameObject;
                    newTurret.transform.position = transform.position;
                    spawnTurretStamp = StaticDataCW.Time;
                    anim.SetInteger("Mode", 2);
                }
            } else {
                dir = chargeDir;
                speed = 6.5f;
                if (StaticDataCW.Time > attackStamp + 5f) {
                    attackStamp = StaticDataCW.Time;
                    attackLock = false;
                    //anim.SetInteger("Mode", 2);
                    attackEndStamp = StaticDataCW.Time;
                    lockMovement = true;
                    attackEnding = true;
                }
            }
            
        }

        if (StaticDataCW.Time > attackEndStamp + 0.33333f && attackEnding) {
            attackEnding = false;
            lockMovement = false;
        }

        prevPlayerDistance = playerDistance;
        /*
        if (!lockMovement) {
            xv = Mathf.Lerp(xv,dir.x * speed,0.15f);
            yv = Mathf.Lerp(yv,dir.y * speed,0.15f);
        } else {
            xv = Mathf.Lerp(xv,0f,0.1f);
            yv = Mathf.Lerp(yv,0f,0.1f);
        }
        */
        if (!lockMovement) {
            xv = dir.x * speed;
            yv = dir.y * speed;
        } else {
            xv = 0f;
            yv = 0f;
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
            EnableReset();
        }
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }
        if (stunned && StaticDataCW.Time > intangStamp + 0.5f){
            stunned = false;
        }

        if (StaticDataCW.Time > attackCooldown + 1.7f){
            attackAnimLock = true;
        } else if (StaticDataCW.Time > attackCooldown + 0.6f) {
            attackAnimLock = false;
        }

        prevmode = mode;

        if (attackLock == true) {
            anim.SetInteger("Mode", 1);
        }
        anim.SetBool("AttackLock", attackLock);
        anim.SetBool("AttackPause", attackPause);
        anim.SetBool("AttackEnding", attackEnding);
        anim.SetBool("PoweringOn", enterMode);
        anim.SetBool("Off", mode == 0);
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
            yv = yv*-1f;
            rb.position = new Vector2(rb.position.x + xv, rb.position.y + yv);*/
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
    void OnCollisionEnter2D (Collision2D other)
    {
        if ((other.gameObject.layer == 8 || other.gameObject.layer == 12 || other.gameObject.layer == 14) && attackLock) {
            if (StaticDataCW.Time > attackStamp + 0.4f) {
                attackLock = false;
                attackStamp = StaticDataCW.Time;
                anim.SetInteger("Mode", 2);
                if (attack != null) {
                    Destroy(attack);
                }
                attackEndStamp = StaticDataCW.Time;
                lockMovement = true;
                attackEnding = true;
            } else {
                wallTesting = true;
                wallTestStamp = attackStamp;
            }
        }
    }
    
    void OnCollisionStay2D (Collision2D other)
    {
        if ((other.gameObject.layer == 8 || other.gameObject.layer == 12 || other.gameObject.layer == 14) && attackLock) {
            if (wallTesting && StaticDataCW.Time > wallTestStamp + 0.4f) {
                attackLock = false;
                attackStamp = StaticDataCW.Time;
                anim.SetInteger("Mode", 2);
                if (attack != null) {
                    Destroy(attack);
                }
                attackEndStamp = StaticDataCW.Time;
                lockMovement = true;
                attackEnding = true;
                wallTesting = false;
            } else if (!wallTesting) {
                wallTesting = true;
                wallTestStamp = StaticDataCW.Time;
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
