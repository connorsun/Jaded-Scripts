using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;

public class EnemyDisablerCW : MonoBehaviour
{
    // Start is called before the first frame update
    public bool notEnemy;
    public GameObject roomBox;
    private MonoBehaviour[] em1;
    public bool disabled;
    public bool intangible;
    public SpriteRenderer sr;
    private Rigidbody2D rb;
    public Vector3 savePos;
    private Transform ts;
    public int health;
    public int maxhealth;
    public float deathStamp = -1;
    private bool isFirstFrame = true;
    public GameObject[] attachedObjects;
    public float deathTime;
    public bool dying;
    public bool dead;
    public GameObject collectibleHealingOrb;
    public float orbcount;
    private bool instaDeath;
    private LineRenderer lr;
    private int prevhealth;
    public bool waitDeath;
    public bool disableOrbDrop;
    private bool countAsEnemy;
    private Light2D pointLight;
    public bool inLockedRoom;
    public GameObject lockObj;
    private SpriteRenderer locksr;
    public int lockFading;
    public bool hideLock;
    private float lockHideTimer;

    public Material matDefault;
    public Material matWhite;

    float plIntensity;
    float plOuter;
    float plInner;
    public bool dontRevive;
    public bool isVoiceOrb;
    
    
    void Start()
    {
        em1 = GetComponents<MonoBehaviour>();
        disabled = false;
        rb = GetComponent<Rigidbody2D>();
        ts = GetComponent<Transform>();
        if (GetComponent<SpriteRenderer>() != null) {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        if (GetComponent<LineRenderer>() != null){
            lr = GetComponent<LineRenderer>();
        }
        savePos = rb.position;
        health = maxhealth;
        collectibleHealingOrb = Resources.Load<GameObject>("Prefabs/CollectibleHealingOrb");
        lockFading = 0;
        /*if (SceneManagerCW.isEnemyLoaded(gameObject)) {
            for (int i = 0; i < StaticDataCW.EnemiesLoaded.Count; i++) {
                if (StaticDataCW.EnemiesLoaded[i].isID((Vector3) rb.position)) {
                    if (StaticDataCW.EnemiesLoaded[i].isDead()) {
                        sr.enabled = false;
                        if (GetComponent<LineRenderer>() != null){
                            lr.enabled = false;
                        }
                        instaDeath = true;
                        InstaDie(false);
                    } else {
                        instaDeath = false;
                        dead = false;
                    }
                }
            }
        } else {
            instaDeath = false;
            dead = false;
        }*/

        if (matDefault == null) {
            matDefault = Resources.Load("Materials/Sprite-Lit-Default") as Material;
        }
        matWhite = Resources.Load("Materials/Brightness") as Material;

        Invoke("SetLight", .1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inLockedRoom && lockObj == null) {
            lockObj = Instantiate(Resources.Load<GameObject>("Prefabs/LockedEnemySprite") as GameObject);
            lockObj.transform.parent = transform;
            lockObj.transform.localPosition = Vector3.zero;
            locksr = lockObj.GetComponent<SpriteRenderer>();
            locksr.color = new Color (226f, 255f, 120f, 1f);
            locksr.sortingOrder = 10;
            lockFading = 1;
            lockObj.GetComponent<Animator>().Play("Lock", -1, 0f);
        }
        if (PlayerControllerCW.resetting > 0 || PlayerControllerCW.spikeResetting > 0) {
            health = maxhealth;
            rb.velocity = new Vector2(0f,0f);
            if (!isVoiceOrb) {
                rb.position = savePos;
            } else {
                //rb.position = (Vector2) GameObject.FindWithTag("Player").transform.position;
            }
            if (!dontRevive) {
                if ((dead || dying)) {
                    Revive();
                } else {
                    for (int i = 0; i < attachedObjects.Length; i++) {
                        if (attachedObjects[i] != null) {
                            Destroy(attachedObjects[i]);
                        }
                    }
                    for (int i = 0; i < em1.Length; i++) {
                        if (em1 != null) {
                            System.Type T = em1[i].GetType();
                            foreach(System.Reflection.MethodInfo _m in T.GetMethods())
                            {
                                if(_m.Name == "CreateObjects")
                                {
                                    _m.Invoke(em1[i],null);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        if (lockFading == 1 && locksr != null) {
            locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 0f);
            /*
            if (locksr.color.a > 0.31f || locksr.color.a < 0.29f) {
                locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, Mathf.Lerp(locksr.color.a, 0.3f, 0.1f));
            } else {
                locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 0.3f);
            }
            */
        }
        if (lockFading == 2 && locksr != null) {
            if (locksr.color.a < 1f) {
                locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, Mathf.Lerp(locksr.color.a, 1f, 0.1f));
            } else {
                locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 1f);
            }
        }
        if (locksr != null && lockObj != null) {
            if (hideLock || lockObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hide") || (roomBox != null && (roomBox.GetComponent<RoomConfigureCW>().isCleared || !roomBox.GetComponent<RoomConfigureCW>().isClearable))) {
                locksr.enabled = false;
            } else {
                locksr.enabled = true;
            }
        }

        if (sr != null && sr.material == null) {
            sr.material = matDefault;
        }

        /*if (PlayerControllerCW.spikeResetting > 0 && !dead && !dying) {
            health = maxhealth;
            rb.velocity = new Vector2(0f,0f);
            rb.position = savePos;
        }*/

        if (!dead) {
            if (roomBox != PlayerControllerCW.roomBox && !disabled && !isVoiceOrb) {
                disabled = true;
                rb.velocity = new Vector2(0f,0f);
                sr.enabled = false;
                if (GetComponent<LineRenderer>() != null){
                    lr.enabled = false;
                }
                for (int i = 0; i < em1.Length; i++) {
                    if (em1[i] != GetComponent<EnemyDisablerCW>()) {
                        em1[i].enabled = false;
                    }
                }
            } else if (roomBox == PlayerControllerCW.roomBox && disabled && !isVoiceOrb) {
                disabled = false;
                health = maxhealth;
                ts.position = savePos;
                rb.velocity = new Vector2(0f,0f);
                if (gameObject.GetComponent<BoxCollider2D>() != null){
                    gameObject.GetComponent<BoxCollider2D>().enabled = true;
                }
                if (GetComponent<LineRenderer>() != null){
                    lr.enabled = true;
                }
                sr.enabled = true;
                for (int i = 0; i < em1.Length; i++) {
                    em1[i].enabled = true;
                }
                for (int i = 0; i < attachedObjects.Length; i++) {
                    if (attachedObjects[i] != null) {
                        Destroy(attachedObjects[i]);
                    }
                }
                if (lockObj != null && locksr != null) {
                    locksr.color = new Color (226f, 255f, 120f, 1f);
                    lockFading = 1;
                    lockObj.GetComponent<Animator>().Play("Lock", -1, 0f);
                }
                for (int i = 0; i < em1.Length; i++) {
                    if (em1 != null) {
                        System.Type T = em1[i].GetType();
                        foreach(System.Reflection.MethodInfo _m in T.GetMethods())
                        {
                            if(_m.Name == "CreateObjects")
                            {
                                _m.Invoke(em1[i],null);
                            }
                        }
                    }
                }
            }
        }

        if (health < 1 && !dying && !waitDeath) {
            Die();
        }

        if (StaticDataCW.Time > deathStamp + deathTime && deathStamp != -1){
            if (!instaDeath && !notEnemy) {
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive --;
            }
            for (int i = 0; i < attachedObjects.Length; i++) {
                if (attachedObjects[i] != null) {
                    Destroy(attachedObjects[i]);
                }
            }
            dead = true;
            deathStamp = -1;
            disabled = true;
            rb.velocity = new Vector2(0f,0f);
            if (gameObject.GetComponent<BoxCollider2D>() != null){
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
            sr.enabled = false;
            if (GetComponent<LineRenderer>() != null){
                lr.enabled = false;
            }
            for (int i = 0; i < em1.Length; i++) {
                if (em1[i] != GetComponent<EnemyDisablerCW>()) {
                    em1[i].enabled = false;
                }
            }
        }

        if (health < prevhealth && !dead){
            WhiteFlash();
        }
        prevhealth = health;
    }

    public void WhiteFlash() {
        sr.material = matWhite;
        Invoke("ResetMaterial", .1f);
        if (locksr != null) {
            locksr.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 1f);
        }
    }

    void ResetMaterial()
    {
        if (sr != null) {
            sr.material = matDefault;
        }
    }

    void SetLight()
    {
        if (pointLight == null) {
            bool found = false;
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                if (gameObject.transform.GetChild(i).childCount > 0 && gameObject.transform.GetChild(i).GetChild(0).tag == "Light") {
                    pointLight = gameObject.transform.GetChild(i).GetChild(0).GetComponent<Light2D>();
                    found = true;
                    break;
                }
            }
            if (sr != null) {
                if (!found) {
                    pointLight = sr.gameObject.GetComponentsInChildren<Light2D>()[0];
                }
                plIntensity = (float)pointLight.intensity;
                plOuter = (float)pointLight.pointLightOuterRadius;
                plInner = (float)pointLight.pointLightInnerRadius;
                //print(pointLight);
                //print(gameObject.name + "Loaded!");
            }
        }
    }

    void OnTriggerStay2D (Collider2D other) 
    {
        if (other.CompareTag("RoomBox")) {
            roomBox = other.gameObject;
            if (isFirstFrame && (!instaDeath || countAsEnemy) && !notEnemy) {
                inLockedRoom = true;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive ++;
                Array.Resize(ref roomBox.GetComponent<RoomConfigureCW>().enemies, roomBox.GetComponent<RoomConfigureCW>().enemiesActive);
                roomBox.GetComponent<RoomConfigureCW>().enemies[roomBox.GetComponent<RoomConfigureCW>().enemiesActive - 1] = gameObject;
                isFirstFrame = false;
            }
        }
    }
    
    public void Die() {
        instaDeath = false;
        intangible = true;
        deathStamp = StaticDataCW.Time;
        dying = true;
        health = 42069;
        StartCoroutine("FadeLight");
        if (lockObj != null) {
            lockObj.GetComponent<Animator>().Play("Unlock");
        }
        lockFading = 2;
        /*if (!disableOrbDrop) {
            for (int i = 0; i < orbcount; i++){
                Instantiate(collectibleHealingOrb, transform.position, Quaternion.identity);
            }
        }*/
        //GetComponent<BoxCollider2D>().enabled = false;
        for (int i = 0; i < attachedObjects.Length; i++) {
            if (attachedObjects[i] != null) {
                if (attachedObjects[i].GetComponent<BoxCollider2D>() != null)
                attachedObjects[i].GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        /*for (int i = 0; i < StaticDataCW.EnemiesLoaded.Count; i++) {
            if (StaticDataCW.EnemiesLoaded[i].isID((Vector3) savePos)) {
                StaticDataCW.EnemiesLoaded[i].dead = true;
            }
        }*/
        sr.material = matWhite;
        Invoke("ResetMaterial", .1f);
    }

    IEnumerator FadeLight() {
        float repetitions = deathTime / 0.02f;
        print(pointLight);
        for (int i = 0; i < Mathf.FloorToInt(repetitions); i++) {
            pointLight.intensity = Mathf.SmoothDamp((float)pointLight.intensity,0.0f,ref plIntensity,deathTime / 1.5f);
            pointLight.pointLightOuterRadius = Mathf.SmoothDamp((float)pointLight.pointLightOuterRadius,0.0f,ref plOuter,deathTime);
            pointLight.pointLightInnerRadius = Mathf.SmoothDamp((float)pointLight.pointLightInnerRadius,0.0f,ref plInner,deathTime);
            yield return new WaitForFixedUpdate();
        }
    }

    public void InstaDie(bool countsAsEnemy) {
        instaDeath = true; 
        countAsEnemy = countsAsEnemy;
        intangible = true;
        deathStamp = StaticDataCW.Time - deathTime;
        dying = true;
        health = 42069;
        
        //GetComponent<BoxCollider2D>().enabled = false;
        if (gameObject.GetComponent<BoxCollider2D>() != null){
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        for (int i = 0; i < attachedObjects.Length; i++) {
            if (attachedObjects[i] != null) {
                if (attachedObjects[i].GetComponent<BoxCollider2D>() != null)
                attachedObjects[i].GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        /*for (int i = 0; i < StaticDataCW.EnemiesLoaded.Count; i++) {
            if (StaticDataCW.EnemiesLoaded[i].isID((Vector3) savePos)) {
                StaticDataCW.EnemiesLoaded[i].dead = true;
            }
        }*/
        if (countsAsEnemy) {
            if (!notEnemy && roomBox != null) {
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive --;
            }
            print(attachedObjects.Length);
            for (int i = 0; i < attachedObjects.Length; i++) {
                if (attachedObjects[i] != null) {
                    Destroy(attachedObjects[i]);
                }
            }
            /*if (!disableOrbDrop) {
                for (int i = 0; i < orbcount; i++){
                    Instantiate(collectibleHealingOrb, transform.position, Quaternion.identity);
                }
            }*/
            dead = true;
            deathStamp = -1;
            disabled = true;
            //if (rb != null) {
                rb.velocity = new Vector2(0f,0f);
            //}
            if (sr != null) {
                sr.enabled = false;
            }
            if (GetComponent<LineRenderer>() != null){
                lr.enabled = false;
            }
            //if (em1 != null) {
                for (int i = 0; i < em1.Length; i++) {
                    if (em1[i] != GetComponent<EnemyDisablerCW>()) {
                        em1[i].enabled = false;
                    }
                }
            //}
        }
        /*
        for (int i = 0; i < em1.Length; i++) {
            if (em1[i] != GetComponent<EnemyDisablerCW>()) {
                em1[i].enabled = false;
            }
        }
        for (int i = 0; i < attachedObjects.Length; i++) {
            if (attachedObjects[i] != null) {
                if (attachedObjects[i].GetComponent<BoxCollider2D>() != null)
                attachedObjects[i].GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        roomBox.GetComponent<RoomConfigureCW>().enemiesActive --;
        for (int i = 0; i < attachedObjects.Length; i++) {
            if (attachedObjects[i] != null) {
                Destroy(attachedObjects[i]);
            }
        }
        intangible = true;
        dying = true;
        health = 42069;
        dead = true;
        deathStamp = -1;
        disabled = true;
        rb.velocity = new Vector2(0f,0f);
        sr.enabled = false;
        */
    }

    public void Revive() {
        dontRevive = false;
        instaDeath = false;
        intangible = false;
        countAsEnemy = false;
        dying = false;
        deathStamp = -1;
        dead = false;
        health = maxhealth;
        for (int i = 0; i < attachedObjects.Length; i++) {
            if (attachedObjects[i] != null) {
                Destroy(attachedObjects[i]);
            }
        }
        //GetComponent<BoxCollider2D>().enabled = false;
        for (int i = 0; i < em1.Length; i++) {
            if (em1 != null) {
                System.Type T = em1[i].GetType();
                foreach(System.Reflection.MethodInfo _m in T.GetMethods())
                    {
                        if(_m.Name == "CreateObjects")
                        {
                            _m.Invoke(em1[i],null);
                        }
                        /*if(_m.Name == "EnableReset")
                        {
                            _m.Invoke(em1[i], null);
                        }*/
                    }
            }
        }
        disabled = false;
        if (!isVoiceOrb) {
            ts.position = savePos;
        }
        rb.velocity = new Vector2(0f,0f);
        if (gameObject.GetComponent<BoxCollider2D>() != null){
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        sr.enabled = true;
        if (GetComponent<LineRenderer>() != null){
            lr.enabled = true;
        }
        for (int i = 0; i < em1.Length; i++) {
            em1[i].enabled = true;
        }
        if (!notEnemy && roomBox != null) {
            roomBox.GetComponent<RoomConfigureCW>().enemiesActive ++;
        }
        Invoke("SetLight", .1f);
    }
}