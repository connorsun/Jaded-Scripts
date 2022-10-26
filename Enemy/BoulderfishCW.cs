using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderfishCW : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public float xv;
    public float yv;
    //rad
    public float dir;
    //deg
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
    private float intangStamp;
    private float changeDirStamp;
    private float changeDirTime;
    public int mode;
    private int prevmode;
    private bool playerInSight;
    private bool prevPlayerInSight;
    public float playerDistance;
    private float prevPlayerDistance;
    public float speed;
    private float sightTimer;
    public GameObject boulderfishAttack;
    public GameObject boulderfishSprite;
    public float attackCooldown = -9.0f;
    private float playerSightStamp;
    private bool lockMovement;
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
    private bool initialSpawnDir;
    private float saveDir;
    private bool minimode;
    private float lerpTurn;
    private Vector2 dirVec;
    public float width;
    public float length;

    private LineRenderer lr;
    private int wallInvincibility;
    private int bounceCount;
    private float noTurnStamp = -9.0f;
    private Vector2 playerLogPos;
    private int framesSinceSceneLoad;
    private bool getHit;

    private Mesh mesh;
    public LayerMask meshMask;
    private Material matDefault;
    public GameObject meshObj;
    private Vector3 meshPoint;
    private bool saveInOut;
    private bool cancelOutFade;
    private int outi;
    private float modeCooldownStamp = -9.0f;
    private bool bounceBack;
    public Vector2 boundsCenter;
    public Vector2 boundsSize;
    private bool regenerate;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.layer = 27;
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 3;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        xv = 0f;
        yv = 0f;
        player = GameObject.Find("Player");
        playerrb = player.GetComponent<Rigidbody2D>();
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);
        ed.disableOrbDrop = true;
        playerLogPos = playerrb.position;

        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(1f, 1f);
        solid.isTrigger = false;
        wallInvincibility = 0;

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

        

        ed.deathTime = 0.5f;
        outi= 0;
        cancelOutFade = false;

        /*lr.startWidth = 0f;
        lr.endWidth = 4f;
        dirVec = DegToVec(dir);
        dirVec.Normalize();
        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, dirVec);
        lr.startColor = new Color(1f, 1f, 1f, 0.4f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);*/

    }

    public Vector3 GetVectorFromAngle(float angle) {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI/180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void CreateObjects() {
        boulderfishAttack = Resources.Load<GameObject>("Prefabs/BoulderExplosion");
        boulderfishSprite = Instantiate(Resources.Load<GameObject>("Prefabs/BoulderfishSprite") as GameObject, gameObject.transform);
        boulderfishSprite.transform.localPosition = new Vector3 (0,0,0);
        sr = boulderfishSprite.GetComponent<SpriteRenderer>();
        ed.sr = boulderfishSprite.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Tilemap";
        sr.sortingOrder = 10;
        anim = boulderfishSprite.GetComponent<Animator>();
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";
        ed.attachedObjects = new GameObject[3];
        ed.attachedObjects[0] = boulderfishSprite;
        ed.attachedObjects[1] = groundPoint;
        bounceCount = 0;
        if (ed.roomBox != null) {
            float maxy = -99999f;
            float miny = 99999f;
            float maxx = -99999f;
            float minx = 99999f;
            foreach (Vector2 point in ed.roomBox.GetComponent<PolygonCollider2D>().points) {
                if (point.x > maxx) {
                    maxx = point.x;
                }
                if (point.x < minx) {
                    minx = point.x;
                }
                if (point.y > maxy) {
                    maxy = point.y;
                }
                if (point.y < miny) {
                    miny = point.y;
                }
            }
            boundsSize = new Vector2(maxx-minx, maxy-miny);
            boundsCenter = (Vector2) ed.roomBox.transform.position + new Vector2(minx + boundsSize.x/2f, miny + boundsSize.y/2f);
        }

        matDefault = Resources.Load("Materials/StingerMesh") as Material;
        meshObj = new GameObject("Mesh");
        meshObj.layer = 23;
        meshObj.transform.position = Vector3.zero;
        mesh = new Mesh();
        meshObj.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
        mr.material = matDefault;
        ed.attachedObjects[2] = meshObj;
        EnableReset();
    }

    public void EnableReset() {
        dir = 0f;
        if (ed.notEnemy && framesSinceSceneLoad > 5) {
            for (int i = 0; i < ed.attachedObjects.Length; i++) {
                if (ed.attachedObjects[i] != null) {
                    Destroy(ed.attachedObjects[i]);
                }
            }
            Destroy(gameObject);
        }
        mode = 0;
        speed = 0f;
        xv = 0f;
        yv = 0f;
        boulderfishSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
        minimode = false;
        rb.position = savePos;
        meshPoint = gameObject.transform.position;
        outi= 0;
        cancelOutFade = false;
        modeCooldownStamp = -9.0f;
        bounceBack = false;
        regenerate = false;
        /*if (mesh != null) {
            //Destroy(mesh);
            matDefault = Resources.Load("Materials/boulderfishMesh") as Material;
            meshObj = new GameObject("Mesh");
            meshObj.transform.position = Vector3.zero;
            mesh = new Mesh();
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
            mr.material = matDefault;
        }*/
        /*lr.startWidth = 0f;
        width = 8.1f;
        length = 6f;
        lr.endWidth = width;
        dirVec = DegToVec((-newDir + 90f));
        dirVec.Normalize();
        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, (Vector2)gameObject.transform.position + (dirVec * length));
        lr.startColor = new Color(1f, 1f, 1f, 0.2f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);
        wallInvincibility = 0;
        bounceCount = 0;*/
    }

    

    // Update is called once per frame
    void FixedUpdate()
    {
        framesSinceSceneLoad++;
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
        playerDistance = Mathf.Sqrt(Mathf.Pow(player.GetComponent<Transform>().position.x - transform.position.x,2f)+Mathf.Pow(player.GetComponent<Transform>().position.y - transform.position.y,2f));
        if (getHit && ed != null) {
            ed.intangible = true;
            ed.health -= 1;
            getHit = false;
        }
        if (ed.disabled) {
            xv = 0f;
            yv = 0f;
        }
        if (framesSinceSceneLoad > 3f) {
            Vector2 newpos = rb.position;
            if (rb.position.x > boundsCenter.x + boundsSize.x/2f - 0.5f) {
                newpos.x = boundsCenter.x + boundsSize.x/2f - 0.5f;
            }
            if (rb.position.x < boundsCenter.x - boundsSize.x/2f + 0.5f) {
                newpos.x = boundsCenter.x - boundsSize.x/2f + 0.5f;
            }
            if (rb.position.y > boundsCenter.y + boundsSize.y/2f - 0.5f) {
                newpos.y = boundsCenter.y + boundsSize.y/2f - 0.5f;
            }
            if (rb.position.y < boundsCenter.y - boundsSize.y/2f + 0.5f) {
                newpos.y = boundsCenter.y - boundsSize.y/2f + 0.5f;
            }
            rb.position = newpos;
        }
        /*lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, (Vector2)gameObject.transform.position + (dirVec * length));
        lr.startColor = new Color(1f, 1f, 1f, 0.4f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);
        lr.endWidth = width;*/

        if (mode == 0) {
            speed = 0f;
            if (playerDistance < 8f && StaticDataCW.Time > modeCooldownStamp + 1.1f) {
                mode = 1;
            }
        }
        if (mode == 1 && playerDistance > 12.5f) {
            mode = 0;
        }
        if (StaticDataCW.Time > modeCooldownStamp + 0.7f) {
            bounceBack = false;
            regenerate = true;
        }
        if (StaticDataCW.Time > modeCooldownStamp + 1.1f) {
            regenerate = false;
        }
        if (bounceBack) {
            boulderfishSprite.transform.Rotate(new Vector3(0f, 0f, 40f*Time.timeScale));
        } else {
            boulderfishSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if (mode == 1) {
            mode = 2;
            speed = 2f;
        } else if (mode == 2) {
            if (speed < 7.5f) {
                speed += 0.1f * Time.timeScale;
            }
            float playerDir = Mathf.Atan2((playerrb.position.y - rb.position.y), (playerrb.position.x - rb.position.x));
            if (StaticDataCW.Time > noTurnStamp + 0.2f) {
                if (Mathf.Abs((saveDir - playerDir)) < 0.2f || (Mathf.Abs((saveDir - playerDir)) > 2*Mathf.PI - 0.2f && Mathf.Abs((saveDir - playerDir)) < 2*Mathf.PI + 0.2f)) {
                    saveDir = playerDir;
                } else if (((saveDir - playerDir) < Mathf.PI && (saveDir - playerDir) > 0f) || (saveDir - playerDir) < -Mathf.PI) {
                    saveDir -= 0.075f * Time.timeScale;
                } else {
                    saveDir += 0.075f * Time.timeScale;
                }
            }
            if (saveDir > 2f*Mathf.PI || saveDir < -2f*Mathf.PI) {
                saveDir = 0f;
            }
            /*if (StaticDataCW.Time > noTurnStamp + 0.2f) {
                if ((saveDir % Mathf.PI) - (playerDir % Mathf.PI) > 0f) {
                    if (saveDir > Mathf.PI ^ playerDir > Mathf.PI) {
                        saveDir += 0.0275f * Time.timeScale;
                    } else {
                        saveDir -= 0.0275f * Time.timeScale;
                    }
                } else {
                    if (saveDir > Mathf.PI ^ playerDir > Mathf.PI) {
                        saveDir -= 0.0275f * Time.timeScale;
                    } else {
                        saveDir += 0.0275f * Time.timeScale;
                    }
                }
            }*/
            dir = saveDir;
            if (playerDistance < 0.8f) {
                Explode();
                Xvel(-(playerrb.position.x - rb.position.x)*12f, true);
                Yvel(-(playerrb.position.y - rb.position.y)*12f, true);
                mode = 0;
                modeCooldownStamp = StaticDataCW.Time;
                bounceBack = true;
            }
        }

        prevPlayerDistance = playerDistance;
        dir = dir % (2 * Mathf.PI);
        Vector2 directionVector = new Vector2(Mathf.Cos(dir),Mathf.Sin(dir));
        directionVector.Normalize();
        if (!lockMovement && !bounceBack) {
            xv = directionVector.x * speed;
            //xv = Mathf.Lerp(xv,directionVector.x * speed,0.15f);
            yv = directionVector.y * speed;
        } else if (lockMovement) {
            xv = 0f;
            yv = 0f;
        } else {
            Xvel(xvelset * 0.95f, true);
            Yvel(yvelset * 0.95f, true);
        }

        if (player.transform.position.x >= transform.position.x) {
            
            if (regenerate) {
                sr.flipY = false;
                sr.flipX = true;
            } else {
                if (mode == 0) {
                    sr.flipX = true;
                    sr.flipY = false;
                } else {
                    sr.flipX = false;
                    sr.flipY = true;
                }
            }
        } else {
            sr.flipY = false;
            sr.flipX = false;
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
            if (ed.notEnemy) {
                for (int i = 0; i < ed.attachedObjects.Length; i++) {
                    if (ed.attachedObjects[i] != null) {
                        Destroy(ed.attachedObjects[i]);
                    }
                }
                Destroy(gameObject);
            } else {
                dir = 0f;
                mode = 0;
                speed = 0f;
                xv = 0f;
                yv = 0f;
                boulderfishSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
                minimode = false;
                rb.position = savePos;
                meshPoint = gameObject.transform.position;
                outi= 0;
                cancelOutFade = false;
                modeCooldownStamp = -9.0f;
                bounceBack = false;
                regenerate = false;
            }
        }
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }

        prevmode = mode;
    
        anim.SetInteger("Mode", mode);
        anim.SetBool("FlyingAway", bounceBack);
        anim.SetBool("Regenerate", regenerate);
        /*
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
        */
        if (mode != 0){
            boulderfishSprite.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * dir) + 180f);
        }

        if (ed.deathStamp != -1){
            anim.SetBool("Death", true);
            mode = 3;
            speed = 0f;
            ed.health = 0;
            xv = 0;
            yv = 0;
        } else{
            anim.SetBool("Death", false);
        }
        prevPlayerInSight = playerInSight;
    }
    void OnTriggerEnter2D(Collider2D other) {
        /*if (other.CompareTag("RoomBound") || other.CompareTag("EnemyBlocker") || other.gameObject.layer == 14){
            if (mode == 2) {
                Explode();
            }
        } else */if (other.CompareTag("Attack")/* && other.GetComponent<AttackCW>().type != 2 */&& other.GetComponent<AttackCW>().type != 5 && (ed == null || (!ed.intangible && !ed.dead && !ed.dying))) {
            intangStamp = StaticDataCW.Time;
            if (ed == null) {
                getHit = true;
            } else {
                ed.intangible = true;
                ed.health -= other.GetComponent<AttackCW>().dmgValue;
            }
            mode = 0;
            modeCooldownStamp = StaticDataCW.Time;
            bounceBack = true;
            if (other.GetComponent<AttackCW>().dir == 2 || other.GetComponent<AttackCW>().crouching) {
                Yvel(13,true);
            } else if (other.GetComponent<AttackCW>().dir == -1) {
                Xvel(13,true);
            } else if (other.GetComponent<AttackCW>().dir == 1) {
                Xvel(-13,true);
            } else if (other.GetComponent<AttackCW>().dir == -2) {
                Yvel(-13,true);
            }
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
        }
    }
    void Explode() {
            GameObject boulder = Instantiate(Resources.Load<GameObject>("Prefabs/Boulder")) as GameObject;
            boulder.transform.position = transform.position;
            GameObject attack = Instantiate(boulderfishAttack, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = dir;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 18f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.04f,0f,0f);
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.1f;
            attack.GetComponent<EnemyAttackCW>().colliderDisable = true;
            attack.GetComponent<EnemyAttackCW>().dontDeleteWhenParentDies = true;
    }
    private bool CheckForWall(Vector2 point, float distance, Vector2 dir)
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


    private Vector2 DegToVec(float degree){
        float radians = degree * (Mathf.PI/180f);
        Vector2 newAngle;
        newAngle.x = Mathf.Cos(radians);
        newAngle.y = Mathf.Sin(radians);
        return newAngle;
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

    private Vector2 RotateVector(Vector2 point, float degree)
    {
        float a = degree * Mathf.Deg2Rad;
        float s = Mathf.Sin(a);
        float c = Mathf.Cos(a);
        Vector2 Vec = new Vector2(
            point.x * c - point.y * s,
            point.y * c + point.x * s
        );
        Vec.Normalize();
        return Vec;
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
