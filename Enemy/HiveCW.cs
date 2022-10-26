using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveCW : MonoBehaviour
{
    // Start is called before the first frame update
    public int newDir;
    public int coneLayer;
    public GameObject hiveSprite;
    public GameObject groundPoint;
    private Rigidbody2D rb;
    private EnemyDisablerCW ed;
    private SpriteRenderer sr;
    private GameObject player;
    private Vector2 savePos;
    private LayerMask playerMask;
    private LayerMask groundMask;
    private LayerMask boundsMask;
    private GameObject stingerSpawn;
    private Animator anim;
    private Vector2 solidsize;
    private BoxCollider2D solid;
    public float cooldownTimer;
    public GameObject stingerReference;
    private bool available;
    private bool dying;
    private float deathTimer;
    private float intangStamp;
    private GameObject newStingerSpawn;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ed = GetComponent<EnemyDisablerCW>();
        ed.maxhealth = 2;
        ed.orbcount = 1;
        ed.health = ed.maxhealth;
        savePos = rb.position;
        playerMask |= (1 << 11);
        groundMask |= (1 << 8);
        boundsMask |= (1 << 14);
        boundsMask |= (1 << 12);
        solid = gameObject.GetComponent<BoxCollider2D>();
        solid.size = new Vector2(0.75f, 1f);
        solid.offset = new Vector2(0f, 0f);
        solid.isTrigger = false;
        CreateObjects();
        cooldownTimer = StaticDataCW.Time;
    }

    public void EnableReset() {
        if (stingerReference != null) {
            EnemyDisablerCW stingered = stingerReference.GetComponent<EnemyDisablerCW>();
            if (stingered != null) {
                for (int i = 0; i < stingered.attachedObjects.Length; i++) {
                    if (stingered.attachedObjects[i] != null) {
                        Destroy(stingered.attachedObjects[i]);
                    }
                }
            }
            Destroy(stingerReference);
            stingerReference = null;
        }
        if (newStingerSpawn != null) {
            Destroy(newStingerSpawn);
            newStingerSpawn = null;
        }
        cooldownTimer = StaticDataCW.Time;
        available = true;
        anim.Play("Idle");
    }

    public void CreateObjects() {
        hiveSprite = Instantiate(Resources.Load<GameObject>("Prefabs/HiveSprite") as GameObject, gameObject.transform);
        hiveSprite.transform.localPosition = new Vector3 (0,0,0);
        stingerSpawn = Resources.Load<GameObject>("Prefabs/StingerSpawn");
        stingerReference = null;
        sr = hiveSprite.GetComponent<SpriteRenderer>();
        anim = hiveSprite.GetComponent<Animator>();
        ed.sr = hiveSprite.GetComponent<SpriteRenderer>();
        groundPoint = new GameObject(this.name + " GroundPoint");
        groundPoint.transform.parent = this.gameObject.transform;
        solidsize = solid.size;
        groundPoint.transform.localPosition = new Vector3(0f,(0.5f*-solidsize.y) + solid.offset.y,0f);
        groundPoint.tag = "Groundpoint";
        ed.deathTime = 0.1f;
        ed.attachedObjects = new GameObject[2];
        ed.attachedObjects[0] = hiveSprite;
        ed.attachedObjects[1] = groundPoint;
        available = true;
        EnableReset();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        if (PlayerControllerCW.resetting > 0 || PlayerControllerCW.spikeResetting > 0) {
            cooldownTimer = StaticDataCW.Time;
            available = true;
            if (stingerReference != null) {
                EnemyDisablerCW stingered = stingerReference.GetComponent<EnemyDisablerCW>();
                if (stingered != null) {
                    for (int i = 0; i < stingered.attachedObjects.Length; i++) {
                        if (stingered.attachedObjects[i] != null) {
                            Destroy(stingered.attachedObjects[i]);
                        }
                    }
                }
                Destroy(stingerReference);
                stingerReference = null;
            }
            if (newStingerSpawn != null) {
                Destroy(newStingerSpawn);
                newStingerSpawn = null;
            }
            anim.Play("Idle");
        }
        if (ed.dying) {
            if (newStingerSpawn != null) {
                Destroy(newStingerSpawn);
                newStingerSpawn = null;
            }
        }
        if (ed.intangible && StaticDataCW.Time > intangStamp + 0.05f && ed.deathStamp == -1) {
            ed.intangible = false;
        }
        if (stingerReference != null && !stingerReference.GetComponent<EnemyDisablerCW>().notEnemy) {
            stingerReference.GetComponent<EnemyDisablerCW>().notEnemy = true;
        }
        if (stingerReference != null && stingerReference.GetComponent<EnemyDisablerCW>().dead) {
            EnemyDisablerCW stingered = stingerReference.GetComponent<EnemyDisablerCW>();
            if (stingered != null) {
                for (int i = 0; i < stingered.attachedObjects.Length; i++) {
                    if (stingered.attachedObjects[i] != null) {
                        Destroy(stingered.attachedObjects[i]);
                    }
                }
            }
            Destroy(stingerReference);
            stingerReference = null;
            cooldownTimer = StaticDataCW.Time;
            available = true;
        }
        if (StaticDataCW.Time > cooldownTimer + 0.5f && available) {
            newStingerSpawn = Instantiate(stingerSpawn as GameObject);
            newStingerSpawn.GetComponent<StingerSpawnCW>().hive = gameObject;
            newStingerSpawn.GetComponent<StingerSpawnCW>().newDir = newDir;
            newStingerSpawn.GetComponent<StingerSpawnCW>().coneLayer = coneLayer;
            newStingerSpawn.GetComponent<SpriteRenderer>().sortingOrder = 11;
            if (GameObject.Find("Enemies") != null) {
                newStingerSpawn.transform.parent = GameObject.Find("Enemies").transform;
                newStingerSpawn.transform.localPosition = transform.localPosition;
            }
            available = false;
            anim.Play("Spit");
        }
        if (ed.dying) {
            anim.Play("Death");
        }
    }
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Attack") && !ed.intangible && !ed.dead && !ed.dying && other.GetComponent<AttackCW>().type != 5) {
            ed.intangible = true;
            intangStamp = StaticDataCW.Time;
            ed.health -= other.GetComponent<AttackCW>().dmgValue;
        } else if (other.CompareTag("Death")) {
            ed.health = 0;
        }
    }
}
