using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingerCW : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    public float xv;
    public float yv;
    //rad
    public float dir;
    //deg
    public float newDir;
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
    private int mode;
    private int prevmode;
    private bool playerInSight;
    private bool prevPlayerInSight;
    public float playerDistance;
    private float prevPlayerDistance;
    public float speed;
    private float sightTimer;
    public GameObject stingerAttack;
    public GameObject stingerSprite;
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
    public float saveDir;
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
    public GameObject visionCone;
    private bool visionFading;
    private bool visionOff;
    public int coneLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 1;
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
        meshMask |= (1 << 8);
        meshMask |= (1 << 12);
        meshMask |= (1 << 14);
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        
        ed.disableOrbDrop = true;
        playerLogPos = playerrb.position;

        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.9f, 0.75f);
        solid.isTrigger = false;
        wallInvincibility = 0;

        CreateObjects();


        stingerSprite.transform.rotation = Quaternion.Euler(0, 0, newDir - 180);
        lerpTurn = newDir - 180;

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

        

        ed.deathTime = 0.4f;
        outi= 0;
        cancelOutFade = false;

        /*
        lr.startWidth = 0f;
        lr.endWidth = 4f;
        dirVec = DegToVec(dir);
        dirVec.Normalize();
        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, dirVec);
        lr.startColor = new Color(1f, 1f, 1f, 0.4f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);*/
        visionCone.transform.localPosition = Vector3.zero;
        visionCone.transform.localScale = new Vector3(1f, 1f, 0f);
        visionCone.transform.Find("Rotator").rotation = Quaternion.Euler(0f, 0f, 35f - newDir);
        if (coneLayer != 0) {
            visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().sortingOrder = coneLayer;
        }
        //StartCoroutine("Fade", false);
    }

    public void Mesh(float viewDistance) {
        float fov = 70f;
        int resolution = 10;
        int rayCount = Mathf.RoundToInt(fov * resolution);
        float angle = 360 - newDir + 125;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = meshPoint;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++) {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(meshPoint, GetVectorFromAngle(angle), viewDistance, meshMask);
            if (raycastHit2D.collider == null){
                vertex = meshPoint + GetVectorFromAngle(angle) * viewDistance;
            } else {
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;
            
            if (i > 0){
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex -1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            
            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public Vector3 GetVectorFromAngle(float angle) {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI/180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void CreateObjects() {
        dir = (-newDir + 90f) * Mathf.Deg2Rad;
        stingerAttack = Resources.Load<GameObject>("Prefabs/StingerExplosion");
        stingerSprite = Instantiate(Resources.Load<GameObject>("Prefabs/StingerSprite") as GameObject, gameObject.transform);
        stingerSprite.transform.localPosition = new Vector3 (0,0,0);
        sr = stingerSprite.GetComponent<SpriteRenderer>();
        ed.sr = stingerSprite.GetComponent<SpriteRenderer>();
        anim = stingerSprite.GetComponent<Animator>();
        //sets up groundpoint
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*solidsize.y),0f);
        groundPoint.tag = "Groundpoint";
        ed.attachedObjects = new GameObject[4];
        ed.attachedObjects[0] = stingerSprite;
        ed.attachedObjects[1] = groundPoint;
        bounceCount = 0;
        visionCone = Instantiate(Resources.Load<GameObject>("Prefabs/StingerCone") as GameObject);
        visionCone.transform.SetParent(transform);

        matDefault = Resources.Load("Materials/StingerMesh") as Material;
        meshObj = new GameObject("Mesh");
        meshObj.layer = 23;
        meshObj.transform.position = Vector3.zero;
        mesh = new Mesh();
        meshObj.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
        mr.material = matDefault;
        ed.attachedObjects[2] = meshObj;
        ed.attachedObjects[3] = visionCone;
        EnableReset();
    }

    public void EnableReset() {
        if (ed.notEnemy && framesSinceSceneLoad > 5) {
            for (int i = 0; i < ed.attachedObjects.Length; i++) {
                if (ed.attachedObjects[i] != null) {
                    Destroy(ed.attachedObjects[i]);
                }
            }
            Destroy(gameObject);
        }
        dir = (-newDir + 90f) * Mathf.Deg2Rad;
        mode = 0;
        speed = 0f;
        xv = 0f;
        yv = 0f;
        stingerSprite.transform.rotation = Quaternion.Euler(0, 0, 180 - newDir);
        minimode = false;
        rb.position = savePos;
        meshPoint = gameObject.transform.position;
        outi= 0;
        cancelOutFade = false;
        visionFading = false;
        visionOff = false;
        
        /*if (mesh != null) {
            //Destroy(mesh);
            matDefault = Resources.Load("Materials/StingerMesh") as Material;
            meshObj = new GameObject("Mesh");
            meshObj.transform.position = Vector3.zero;
            mesh = new Mesh();
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
            mr.material = matDefault;
        }*/
        /*
        lr.startWidth = 0f;
        width = 8.1f;
        length = 6f;
        lr.endWidth = width;
        dirVec = DegToVec((-newDir + 90f));
        dirVec.Normalize();
        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, (Vector2)gameObject.transform.position + (dirVec * length));
        lr.startColor = new Color(1f, 1f, 1f, 0.2f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);*/
        if (visionCone != null) {
            visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().enabled = true;
            visionCone.transform.localPosition = Vector3.zero;
            visionCone.transform.localScale = new Vector3(1f, 1f, 0f);
            visionCone.transform.Find("Rotator").rotation = Quaternion.Euler(0f, 0f, 35f - newDir);
            if (coneLayer != 0) {
                visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().sortingOrder = coneLayer;
            }
            wallInvincibility = 0;
            bounceCount = 0;
        }
        //StartCoroutine("Fade", false);
    }

    /*IEnumerator Fade(bool inout)
    //true == in, false == out
    {
        saveInOut = inout;
        if (inout) {
            if ((outi != 15 && outi != 0) || cancelOutFade) {
                cancelOutFade = true;
                for (int i = 0; i < 15; i++) {
                    Mesh(Mathf.Lerp(6 * outi / 30.0f, 0.0f, i / 15.0f));
                    //mesh function takes in a viewdistance parameter
                    //to make this fade in, i'm calling the function every frame and upping the viewdistance to make it fade in
                    yield return new WaitForFixedUpdate();
                    //wait one fixedupdate, loops 15 times
                }
            } else {
                for (int i = 0; i < 15; i++) {
                    Mesh(Mathf.Lerp(6.0f, 0.0f, i / 15.0f));
                    //mesh function takes in a viewdistance parameter
                    //to make this fade in, i'm calling the function every frame and upping the viewdistance to make it fade in
                    yield return new WaitForFixedUpdate();
                    //wait one fixedupdate, loops 15 times
                }
            }
            outi = 0;
            cancelOutFade = false;
            //GameObject.Destroy(meshObj);
            //if the cone is still there, this code isnt reached
            yield return new WaitForFixedUpdate();
        } else {
            if (!cancelOutFade) {
                for (int i = 0; i < 30; i++) {
                    outi = i;
                    if (!cancelOutFade) {
                        Mesh(Mathf.Lerp(0.0f, 6.0f, i / 30.0f));
                        yield return new WaitForFixedUpdate();
                    } else {
                        yield break;
                    }
                }
            } else {
                yield break;
            }
        }
    }*/

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
        playerInSight = CheckForPlayer(transform.position, 6f);
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
        if ((up || down) && mode == 2 && wallInvincibility == 0) {
            /*if (bounceCount < 13) {
                saveDir = saveDir * -1f;
                bounceCount++;
            } else {
                Explode();
            }*/
            Explode();
        }
        if ((left || right) && mode == 2 && wallInvincibility == 0) {
            /*if (bounceCount < 13) {
                saveDir = Mathf.PI - saveDir;
                bounceCount++;
            } else {
                Explode();
            }*/
            Explode();
        }
        if (wallInvincibility > 0) {
            wallInvincibility --;
        }
        /*
        lr.SetPosition(0, gameObject.transform.position);
        lr.SetPosition(1, (Vector2)gameObject.transform.position + (dirVec * length));
        lr.startColor = new Color(1f, 1f, 1f, 0.4f);
        lr.endColor = new Color(1f, 1f, 1f, 0f);
        lr.endWidth = width;*/

        if (mode == 0) {
            speed = 0f;
            if (playerInSight) {
                mode = 1;
            }
        }

        if (mode == 1) {
            float xVec = Mathf.Lerp(dirVec.x, 0f, 0.3f);
            float yVec = Mathf.Lerp(dirVec.y, 0f, 0.3f);
            dirVec = new Vector2(xVec, yVec);
            width = Mathf.Lerp(width, 0f, 0.3f);
            flyAnim = false;
            if (prevmode != mode){
                //StartCoroutine("Fade", true);
                visionFading = true;
                speed = -1f;
                sightTimer = StaticDataCW.Time + 0.55f;
                minimode = false;
                saveDir = dir;
            }
            dir = saveDir;
            if (StaticDataCW.Time > sightTimer) {
                mode = 2;
                speed = 7f;
                wallInvincibility = 2;
                noTurnStamp = StaticDataCW.Time;
                playerLogPos = playerrb.position;
            }
        } else if (mode == 2) {
            speed = 7f;
            float playerDir = Mathf.Atan2((playerrb.position.y - rb.position.y), (playerrb.position.x - rb.position.x));
            float playerLogDir = Mathf.Atan2((playerLogPos.y - rb.position.y), (playerLogPos.x - rb.position.x));
            /*
            if (playerDir - saveDir < -Mathf.PI) {
                print("diff < -pi: " + (-2*Mathf.PI - (playerDir - saveDir)).ToString());
                float newDir = (playerLogDir + ((-2*Mathf.PI - (playerDir - saveDir))/16f) * Time.timeScale);
                saveDir = newDir;
                /*if (saveDir - newDir < -Mathf.PI) {
                    saveDir = (Mathf.Lerp(saveDir, newDir - Mathf.PI, 0.1f));
                } else if (saveDir - newDir > Mathf.PI) {
                    saveDir = (Mathf.Lerp(saveDir, newDir + Mathf.PI, 0.1f));
                } else {
                    saveDir = (Mathf.Lerp(saveDir, newDir, 0.1f));
                }
            } else if (playerDir - saveDir > Mathf.PI) {
                print("diff > pi: " + (-2*Mathf.PI + (playerDir - saveDir)).ToString());
                float newDir = (playerLogDir + ((-2*Mathf.PI + (playerDir - saveDir))/16f) * Time.timeScale);
                saveDir = newDir;
            } else {
                print("Within pi: " + (playerDir - saveDir).ToString());
                float newDir = (playerLogDir + ((playerDir - saveDir)/16f) * Time.timeScale);
                saveDir = newDir;
            }
            saveDir = saveDir % Mathf.PI;
            */
            if (StaticDataCW.Time > noTurnStamp + 0.2f) {
                if (Mathf.Abs((saveDir - playerLogDir)) < 0.2f || (Mathf.Abs((saveDir - playerLogDir)) > 2*Mathf.PI - 0.2f && Mathf.Abs((saveDir - playerLogDir)) < 2*Mathf.PI + 0.2f)) {
                    saveDir = playerLogDir;
                } else if (((saveDir - playerLogDir) < Mathf.PI && (saveDir - playerLogDir) > 0f) || (saveDir - playerLogDir) < -Mathf.PI) {
                    saveDir -= 0.075f * Time.timeScale;
                } else {
                    saveDir += 0.075f * Time.timeScale;
                }
            }
            if (saveDir > 2f*Mathf.PI || saveDir < -2f*Mathf.PI) {
                saveDir = 0f;
            }
            if ((rb.position - playerLogPos).magnitude < 2f) {
                playerLogPos = playerrb.position;
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
            if (playerDistance < 1f) {
                Explode();
            }
        }

        prevPlayerDistance = playerDistance;
        dir = dir % (2 * Mathf.PI);
        Vector2 directionVector = new Vector2(Mathf.Cos(dir),Mathf.Sin(dir));
        directionVector.Normalize();
        if (!lockMovement) {
            xv = directionVector.x * speed;
            //xv = Mathf.Lerp(xv,directionVector.x * speed,0.15f);
            yv = directionVector.y * speed;
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
            if (ed.notEnemy) {
                for (int i = 0; i < ed.attachedObjects.Length; i++) {
                    if (ed.attachedObjects[i] != null) {
                        Destroy(ed.attachedObjects[i]);
                    }
                }
                Destroy(gameObject);
            } else {
                dir = (-newDir + 90f) * Mathf.Deg2Rad;
                mode = 0;
                speed = 0f;
                xv = 0f;
                yv = 0f;
                stingerSprite.transform.rotation = Quaternion.Euler(0, 0, 180 - newDir);
                minimode = false;
                rb.position = savePos;
                meshPoint = gameObject.transform.position;
                outi= 0;
                cancelOutFade = false;
                /*if (mesh != null) {
                    Destroy(mesh);
                    matDefault = Resources.Load("Materials/StingerMesh") as Material;
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
                lr.endColor = new Color(1f, 1f, 1f, 0f);*/
                wallInvincibility = 0;
                bounceCount = 0;
                visionFading = false;
                visionOff = false;
                if (visionCone != null) {
                    visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().enabled = true;
                    if (coneLayer != 0) {
                        visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().sortingOrder = coneLayer;
                    }
                    visionCone.transform.localPosition = Vector3.zero;
                    visionCone.transform.localScale = new Vector3(1f, 1f, 0f);
                    visionCone.transform.Find("Rotator").rotation = Quaternion.Euler(0f, 0f, 35f - newDir);
                }
                //StartCoroutine("Fade", false);
            }
        }
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }

        if (visionFading && visionCone != null && !visionOff) {
            visionCone.transform.localScale = new Vector3(visionCone.transform.localScale.x - 0.2f, visionCone.transform.localScale.y - 0.2f, 1f);
            if (visionCone.transform.localScale.x < 0.1f) {
                visionOff = true;
                visionCone.transform.Find("Rotator").transform.Find("Renderer").GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        prevmode = mode;
    
        anim.SetInteger("Mode", mode);
        anim.SetBool("Minimode", minimode);
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
            stingerSprite.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * dir) + 90);
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
        } else */if (other.CompareTag("Attack") && other.GetComponent<AttackCW>().type != 5 && (ed == null || (!ed.intangible && !ed.dead && !ed.dying))) {
            intangStamp = StaticDataCW.Time;
            if (ed == null) {
                getHit = true;
            } else {
                ed.intangible = true;
                ed.health -= other.GetComponent<AttackCW>().dmgValue;
            }
            if (other.GetComponent<AttackCW>().dir == 2 || other.GetComponent<AttackCW>().crouching) {
                Yvel(4,true);
            } else if (other.GetComponent<AttackCW>().dir == -1) {
                Xvel(4,true);
            } else if (other.GetComponent<AttackCW>().dir == 1) {
                Xvel(-4,true);
            } else if (other.GetComponent<AttackCW>().dir == -2) {
                Yvel(-4,true);
            }
            if (ed.health <= 0 && !saveInOut) {
                //StartCoroutine("Fade", true);
            }
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
            if (!saveInOut) {
                //StartCoroutine("Fade", true);
            }
        }
    }
    void Explode() {
        if (ed.health > 0f && ed.health < 10f) {
            mode = 3;
            speed = 0f;
            ed.health = 0;
            ed.disableOrbDrop = true;
            xv = 0;
            yv = 0;
            GameObject attack = Instantiate(stingerAttack, transform) as GameObject;
            attack.GetComponent<EnemyAttackCW>().type = 1;
            attack.GetComponent<EnemyAttackCW>().parentEnemy = gameObject;
            attack.GetComponent<EnemyAttackCW>().damage = 1f;
            attack.GetComponent<EnemyAttackCW>().dir = dir;
            attack.GetComponent<EnemyAttackCW>().knockbackValue = 18f;
            attack.GetComponent<EnemyAttackCW>().offset = new Vector3(0.04f,0f,0f);
            attack.GetComponent<EnemyAttackCW>().deathtime = 0.25f;
            attack.GetComponent<EnemyAttackCW>().colliderDisable = true;
            attack.GetComponent<EnemyAttackCW>().dontDeleteWhenParentDies = true;
        }
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

    private bool CheckForPlayer(Vector2 point, float viewDistance)
    {
        Vector2 dirOfCheck = (Vector2)player.transform.position - point;
        float checkDistance = dirOfCheck.magnitude;
        dirOfCheck.Normalize();
        /*
        Vector2 aimDir = DegToVec(newDir);
        Vector2 upperBound = RotateVector(aimDir, 25);
        Vector2 lowerBound = RotateVector(aimDir, -25);
        */
        float anglediff = (newDir - VecToDeg(dirOfCheck) + 180) % 360 - 180;
        if (anglediff <= 35 && anglediff>=-35) {
            RaycastHit2D check = Physics2D.Raycast(point, dirOfCheck, viewDistance, playerMask | groundMask | boundsMask);
            if (check.collider != null && check.transform.gameObject.layer == 11)
            {
                if (checkDistance < viewDistance)
                {
                    return true;
                }
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
