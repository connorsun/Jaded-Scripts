using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;
using Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;

public class PlayerControllerCW : MonoBehaviour
{
    private float saveStamp = -5.0f;
    public Rigidbody2D rb;

    public SpriteRenderer sr;
    public Transform ts;
    public GameObject spriteobj;
    //object with sprite on it
    public static GameObject roomBox;
    //the bounding box of the current room
    public BoxCollider2D bc;
    public Vector2 bcsize;
    public GameObject SceneManagerObj;
    public GameObject SceneManagerPrefab;
    public SceneManagerCW sm;
    public float framesSinceSceneLoad = 0;
    public bool roomTransitioning;
    public int lockRoomTransitioning;
    
    //xmovement

    public float xspeed;
    //the target acceleration the player moves at that we change
    public float xaccel;
    //the target acceleration the player moves at that we change
    public int xframes;
    //the target number of frames that it takes to reach full accel
    public int xframesmod;
    //the number of frames left to acceleration, used to calculate
    public float xinput;
    //input.getaxis
    private float prevxinput;
    //to detect when to accel
    private bool prevxheld;
    //for switchback input
    public float xvelmove;

    public static float timerscale;
    public int directionOfTravel;
    //direction of travel -> integer version of xinput
    public float prevxvel;
    public float prevxvelreal;
    private int slopeBuffer;
    private int ignoreSlopeBufferFrames;
    //for anims
    public bool accelerationControl;
    public int accelerationValue;
    private float accelerationMagnitude;
    private float decelerationMagnitude;

    //ground detection

    public bool onGround;
    //self-explanatory
    private bool onGroundFar;
    private bool prevGrounded;
    public bool isWalledLeft;
    public bool isWalledRight;
    public bool ceiled;
    //on ceiling
    public GameObject groundPoint;
    //point at which raycasts originate for grounding
    public GameObject cameraPoint;
    private CameraControllerCW cameraController;
    public LayerMask groundMask;
    //mask to detect what will make onground true in the raycast
    public LayerMask boundsMask;
    public LayerMask waterBlockMask;
    public GameObject semisolidGround;
    public bool inGround;

    //ymovement

    public float ycap;
    //y velocity cap, target y acceleration
    public float yvel = 0.0f;
    //the number put as rb.velocity.y to apply vertical forces
    public float yaccel;
    //how fast you accelerate downwards
    public float jumpForce;
    //how fast you accelerate downwards
    public bool jumping;
    //to communicate between update and fixedupdate
    public bool canJump;
    //so that pressing jump feels better overall
    public int coyoteFrames;
    //for coyote time jumps (off platform)
    public float jumpTimer;
    //wallJumping
    public bool wallJumping;
    public float wallJumpApplyForce;
    private float wallJumpForceApplicationTimestamp;
    //tracker for assisted wallsliding
    public int assistWallSlide;
    //the leeway that it gives for you to jump better, timer
    private float jumpTimerHeight;
    //to control the height of jumps
    public float jumpBuffer;
    //the leeway that it gives for you to jump better, can set to see result, input buffering
    public bool dashBuffer;
    private int isBufferedDash;
    private float dashInputTimer = -9.0f;
    //leeway for dash
    public float squish;
    //squish the player
    public float yveladd;
    //instance of yvel before applied other y velocities that would be bigger than cap and mess with squish
    private float yvelsquish;
    //adds to yvel
    public float yvelset;
    //sets yvel to new number
    public bool canyveladd;
    //adds to yvel
    public bool canyvelset;
    //sets yvel to new number
    public bool wallSliding;
    //are u wallsliding? ok
    private bool prevWallSliding;
    private bool ignorePrevWallSliding;
    private bool wallSlidingAnims;
    private bool prevWallSlidingAnims;
    private float slideStamp = -9.0f;
    private float cancelSlideStamp = -9.0f;
    private bool cancelSlide;
    private int wallDir;
    public bool lockedWallSliding;
    public float xveladd;
    //adds to yvel
    public float xvelset;
    //sets yvel to new number
    public bool canxveladd;
    //adds to yvel
    public bool canxvelset;
    //sets yvel to new number
    public int facingdir;
    //dirfacing
    private int saveFacingDir;
    private int yinput;
    //input.getaxis

    public float prevyvel;
    
    //death
    public float health;
    private bool knockbackDisable = false;
    //knockbackDisables input
    private float knockbackDisableTimer;
    //to count the time until knockbackDisable ends
    public bool inputDisable = false;
    //disables input
    public float inputDisableTimer;
    public bool invincible;
    //knockbackDisables damage
    private float invincibilityTimer;
    //to count the time until invincibility ends
    private bool touchingDeath;
    //if u r touching death this is true
    private bool touchingConduit;
    private float blinkTimer;
    //to count the time until invincibility ends
    public static int resetting;
    public static int spikeResetting;
    private bool hitDeath;

    private float respawnStamp;
    //if u hit death? used while disabled


    //slowDash
    public bool slowmo;
    //time is changing
    public bool prevslowmo;
    //anims
    public bool canslowDash;
    //can slowDash aim, when let go you will go
    public bool prevcanslowDash;
    //can slowDash aim, when let go you will go
    public bool sdDisable;
    //disables movement
    public float sdDisableTimer;
    //times how long dash should be
    public bool slowDashing;
    //if you are dashing rn
    private int startSlowDash;
    //did u start dash last frame? ok
    public Vector2 slowDir;
    //direction to dash
    public bool onTarget;
    //if you are locked on or not
    public float dashDistance;
    //if you are locked on or not
    public LayerMask dashMask;
    //mask to detect what will make onground true in the raycast
    public LayerMask boundsCheck;
    public float dashTime;
    //time that it takes to do dash
    public bool dashinvincible;
    //invincible during dash
    public int dashFrameCount;
    //dashframecount for normal dashing, keeping same distance
    public Vector2 dashHitPoint;

    public bool airDash;
    //only dash once

    //public float dashMeter;
    //for dash doing damage
    public GameObject dashHitbox; 
    private GameObject dolphinHitbox;
    private bool dashHitExtend;
    private float dashHitTimer;
    private AttackCW atk;

    public Vector2 spikePoint;
    private bool ensureSpikePoint;

    public bool dashCancel;
    //did u cancel ur dash? ok cool
    public bool saveTargetDash;
    //are u in a target dash? ok cool
    public bool dashInputBool;
    public bool gravityCancel;
    private float keepTargetCheckStamp;
    private GameObject dashHitboxObj;

    //attacks
    public bool attackLock = false;
    //locks all attacks and flipping
    private bool crouching;
    //are u crouching? ok cool
    private float attackStamp;
    public GameObject staffSwing;
    private float extendDashInvincibilityStamp=-9.0f;
    private bool extendNormalDashMomentum = false;
    private bool spongeExtending;
    private float stopNormalDashMomentumStamp = -9.0f;
    private int framesSinceEndedNormalDash;
    //private bool extendDashCancel = true;
    private float dashInvincibilityTimer = -9.0f;
    private float cameraZoneExitStamp = - 9.0f;
    private GameObject saveCameraZone;
    private GameObject cameraZoneRoomBox;
    private bool exitedCameraZone;
    public bool changingScene;
    private float changeSceneStamp = -9.0f;
    //scenes
    private int sceneNum;
    private Vector2 keepSlowDir;
    private int turnAroundFrames;

    //graphics (not anims)
    public Light2D pointLight;
    public Light2D emanateLight;
    float plIntensity;
    float plOuter;
    float emIntensity;
    float emOuter;
    float rpoint;
    float gpoint;
    float bpoint;

    private Animator anim;
    private int attackAnimIndex;
    private bool attackAnimLock;
    private int animFlipDir;
    public GameObject maskobj;
    private string[] attackAnims;
    private bool inJump;
    public bool disableMaskChanges;
    private int atkdir;
    public int atkfloat;
    public float lineRendererResetStamp;
    public int downPressed;
    public int disablePlatforms;
    public int disablePlatformsBuffer;
    private float WallJumpXvel;
    private float framesSinceWallJump;
    private int whichBound;
    public bool dashTurn;
    private ParticleSystem dashTrail;
    private ParticleSystem bubbleTrail;
    //healing
    public int healing;
    public GameObject healingOrb;
    public GameObject healEffect;
    private float healStamp = -9.0f;
    private float healStamp2 = -9.0f;
    private bool queueHeal = false;
    //slam
    private int slamInput;
    private float slamInputStamp = -9.0f;
    public bool slamming;
    public int slamState;
    private bool revertSlamVelocity;
    private bool prevSlamming;

    private GameObject targetSelect;
    public GameObject targetSelected;
    private float slamStallStamp;
    //zipline
    public bool ziplining;
    public Vector2 zipDir;
    private Vector2 zipPos;
    private int zipSetPos;
    private bool extendZipMomentum;
    private float timeEndedZip;
    private bool zipJumpable;
    private GameObject otherZipPoint;
    public GameObject targetHit;
    private bool inZiplinePoint;
    private bool prevInZiplinePoint;
    private Collider2D savedZiplinePoint;
    //misc ig
    private float spikeFadeStamp;
    public bool spikeInvincible;
    private GameObject deathFade;
    private SpriteRenderer deathFadeSprite;
    private float deathFadeTimer = -9.0f;
    public bool deathFading;
    private bool deathFadeReset;
    private GameObject deathFadePlayer;
    public GameObject[] blackBars;
    public bool disableAllActions;
    private bool crossSceneDeath = false;
    private bool stopDeathStopTimer = false;
    private bool slamInputBool = false;
    private GameObject hookObject;
    private Vector2 hookPoint;
    private AudioSource dashSoundReference;
    private AudioSource slowdownSoundReference;
    private GameObject slamShockwave;
    private GameObject slashEffect;
    private GameObject slashGraphic;
    private GameObject gliderBack;
    private GameObject gliderDestroyParticle;

    //mushroom
    /*
    public bool mushroomJump;
    private float mushroomJumpTimer;
    private bool mushroomCharging;
    */
    private bool wallAttacking;
    public bool gliderObtained;
    public bool gliderActivated;
    private bool prevGliderActivated;
    public GameObject currentGliderReference;
    private bool ventGliding;
    public bool onCrumble;
    public bool noclip;
    public float noclipSpeed = 10;
    private float noclipyinput;
    private bool noclipprevy;
    private float noclipprevyinput;
    public bool superInvincible;
    private float conduitHitTimestamp;
    public Vector3 cameraZoneTargetPoint;
    private Vector3 cameraZoneSavePos;
    public bool inCamshiftZone;

    private GameObject dashCancelGraphic;
    public float ventCancelTimer = -9.0f;
    private Vector2 returnPos;
    private bool smoothTargetAngle;
    private bool prevDashCancel;
    private int smoothWaitFrames;
    private int framesSinceLastRecordingSpawn;

    //spin attack
    public bool chargingSpin;
    private float spinChargeStamp;
    public bool spinAttacking;
    private bool spinAttackFrame;
    private float spinAttackTimer;
    private int spinAttackStage;
    private int spinAttackType;
    private GameObject spinAttackAir;
    private GameObject spinChargeParticle;
    private GameObject spinChargeParticle2;
    private float spinChargeParticleStamp;
    private float spinAttackSpeed;
    private GameObject previousZiplinePoint;
    private bool zipReenterDisable;
    //water blocks
    public bool swimming;
    public Vector2 swimDir;
    private GameObject waterBlockChecker;
    private GameObject waterBlockChecker2;
    private int swimCancelLock;
    private int framesSinceEndedSwim;
    public float swimSpeed;
    public bool dolphinJumping;
    private int dolphinJumpFrames;
    private GameObject waterBlockCollider;
    public bool extendDolphinMomentum;
    private float dolphinJumpStamp;
    private float dolphinGravFloat;
    public int inWhirlpool;
    private List<Vector3> whirlpoolCenters;
    public bool inCannon;
    public bool superdashing;
    private float cannonLaunchStamp;
    private GameObject tempSpike;
    public bool canCreateTempSpike;
    public bool inPurpleWater;
    private float waterBlockWaitTime;
    public bool touchingSponge;
    private GameObject cannonRef;
    private float superdashEndStamp = -9.0f;
    private bool hideHook;
    private float initialDolphinX;
    public float waterLockTurningTimer = -9.0f;
    private GameObject splash1Ref;
    private GameObject splash2Ref;
    private Edge savedEdge;
    private Vector2 savedEdgePos;
    private bool otherIgnoreInvinc;
    private bool noInvincibleBlink;
    public bool inBubble;
    public int hasExitedBubble;
    public bool bubbleWarping;
    public bool bubbleTeleporting;
    private float bubbleWarpTimestamp;
    public Vector2 bubbleDir;
    public GameObject bubbleRef;
    private Vector2 bubbleSaveDir;
    private Vector2 bubbleSavePos;
    public bool bubbleCancelTeleporting;
    public bool bubbleCancelling = false;
    private float swimStartStamp = -9.0f;
    private Vector3 swimStartPoint;
    private bool exitBubbleFrameBuffer;
    public int bubbleBuffer;
    public GameObject dolphinAttack;
    private float logSlamHeight;
    public bool bubbleExtendLocking;
    private float bubbleExtendLockStamp;
    private bool dolphinTransFrame;
    private int dolphinSmoothWaitFrames;
    private bool smoothDolphinTargetAngle;
    private float dolphinLerpWaitStamp;
    private bool prevRoomBoxCleared;
    private GameObject screenLock;
    private SpriteRenderer screenLockSprite;
    private Animator screenLockAnim;
    private bool cancelDolphinMomentum;
    public Vector4 quadrantCounter;
    private GameObject quadrantParent;
    private bool touchingQuadrant;
    public Vector3 lastPosOutsideWater;
    private int standingOnOneWayFrames;
    private bool eventRoomBuffer;
    private bool enterSceneLock;
    private float enterSceneStamp;
    public PlayableDirector timeline;
    public TaskCompletionSource<bool> pauseTask;
    public float xPosToReach;
    public DialogueManagerCW dm;
    private bool crouchAttackCutscene;
    private bool touchEdgeCutscene;
    private bool waitingForDashCutsceneLand;
    public bool freezePosition;
    private bool cutscene42Dash;
    private Vector2 cutscene42SavePos;
    private bool animSlowingTime;
    private float animSpeed;
    public int slowdownStartFrames = 0;
    public bool inCutscene;
    private bool timelineSpeedOverride;
    private bool timelinePause;
    private double timelinePauseTime;
    private List<IMarker> markers;
    private int markerIndex;
    private bool attackBuffer = false;
    private float attackInputTimer = -9.0f;
    private bool upDooring;
    private float upDoorStamp;
    private Vector2 upDoorPos;
    public bool disableSpikePointSave = false;
    public bool beingPushed;
    public bool dying;
    public bool freeze;
    public float freezeStamp;
    public bool inAbductorCircle;
    public Sprite frozenSprite;
    private int frozenFrames;
    public bool reducedGravity;
    public bool extremeGravity;
    public int reducedGravity2;
    public bool specialTarget;
    public bool cyprusBubble;
    private int unclearRepeatedly;
    private int clearRepeatedly;
    public bool cutsceneCameraParented;

    void Awake()
    {
        framesSinceSceneLoad = 0;
        if (SaveDataCW.savedGames.Count == 0) {
            for (int i=0;i<3;i++) SaveDataCW.savedGames.Add(null);
        }
        SaveDataCW.Load();
        if (Game.loadedGame == null) {
            Game.loadedGame = SaveDataCW.savedGames[0];
        }
        SaveDataCW.Save();
    }
    void Start()
    {
        resetting = 0;
        spikeResetting = 0;
        Setup();
        ObjectSetup();
        Reset();
        //SetPosition();
        //health = StaticDataCW.Health;
        health = 3;
    }
    
    void Setup() 
    {
        rb = GetComponent<Rigidbody2D>();
        ts = GetComponent<Transform>();
        bc = GetComponent<BoxCollider2D>();
        groundMask |= (1 << 8);
        groundMask |= (1 << 19);
        groundMask |= (1 << 20);
        dashMask |= (1 << 8);
        dashMask |= (1 << 10);
        dashMask |= (1 << 12);
        dashMask |= (1 << 13);
        dashMask |= (1 << 20);
        dashMask |= (1 << 21);
        dashMask |= (1 << 24);
        dashMask |= (1 << 27);
        boundsCheck |= (1 << 8);
        boundsCheck |= (1 << 10);
        boundsCheck |= (1 << 13);
        boundsCheck |= (1 << 20);
        boundsCheck |= (1 << 21);
        boundsCheck |= (1 << 27);
        boundsMask |= (1 << 12);
        boundsMask |= (1 << 14);
        waterBlockMask |= (1 << 24);
        returnPos = new Vector2(Game.loadedGame.currentSpikePointx, Game.loadedGame.currentSpikePointy);
    }
    void Reset() 
    {
        freezePosition = false;
        pauseTask = new TaskCompletionSource<bool>();
        //rb.position = new Vector3(29.5f,3f,0f);
        rb.velocity = new Vector3(0f,0f,0f);
        sr.enabled = true;
        prevxheld = false;
        knockbackDisable = false;
        knockbackDisableTimer = 0f;
        touchingDeath = false;
        touchingConduit = false;
        invincible = true;
        dashinvincible = false;
        invincibilityTimer = StaticDataCW.Time - 0.6f;
        airDash = true;
        slowmo = false;
        prevslowmo = false;
        inJump = false;
        xspeed = 6.2f;
        xaccel = 0;
        xframes = 8;
        xframesmod = 0;
        xvelmove = 0;
        ycap = -12.5f;
        yaccel = 0.75f;
        jumpForce = 16;
        jumpBuffer = 0.1f;
        dashBuffer = false;
        isBufferedDash = 0;
        dashInputTimer = -9.0f;
        jumping = false;
        coyoteFrames = 0;
        sdDisable = false;
        knockbackDisable = false;
        yvel = 0f;
        facingdir = 1;
        canJump = false;
        animSlowingTime = false;
        squish = 1f;
        canyveladd = false;
        animSpeed = 1f;
        canyvelset = false;
        canxveladd = false;
        canxvelset = false;
        knockbackDisableTimer = -9.0f;
        canslowDash = false;
        slowDashing = false;
        if (dashTrail != null) {
            dashTrail.Stop();
        }
        if (bubbleTrail != null) {
            bubbleTrail.Stop();
        }
        if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
        dashHitTimer = -9.0f;
        dashHitExtend = false;
        if (dashSoundReference != null) {
            SceneManagerCW.StopSound(dashSoundReference, "Dashing");
        }
        onTarget = false;
        dashDistance = 0f;
        dashTime = 0f;
        unclearRepeatedly = 0;
        clearRepeatedly = 0;
        dashinvincible = false;
        dashFrameCount = 0;
        slowmo = false;
        dashCancel = false;
        attackLock = false;
        turnAroundFrames = 0;
        ceiled = false;
        prevyvel = 0f;
        attackAnimIndex = 0;
        startSlowDash = 0;
        saveTargetDash = false;
        attackAnims = new string[6] {"Attack1betr", "Attack2", "Attack3", "DownAttack", "UpAttack 1", "DownAttackGoingUp"};
        //dashMeter = 1;
        slopeBuffer = 0;
        ignoreSlopeBufferFrames = 0;
        prevGrounded = true;
        prevSlamming = false;
        disableMaskChanges = false;
        atkfloat = 0;
        gravityCancel = false;
        keepTargetCheckStamp = -9.0f;
        slowDir = new Vector2(0,0);
        wallSliding = false;
        wallSlidingAnims = false;
        slideStamp = -9.0f;
        cancelSlideStamp = -9.0f;
        cancelSlide = false;
        wallDir = 0;
        wallAttacking = false;
        disablePlatforms = 0;
        disablePlatformsBuffer = 0;
        plIntensity = 0.5f;
        plOuter = 0.5f;
        emIntensity = 0.3f;
        emOuter = 1.5f;
        cameraZoneExitStamp = -9.0f;
        extendNormalDashMomentum = false;
        spongeExtending = false;
        extendDolphinMomentum = false;
        stopNormalDashMomentumStamp = -9.0f;
        healing = 0;
        healStamp = -9.0f;
        healStamp2 = -9.0f;
        queueHeal = false;
        ensureSpikePoint = false;
        if (SpecialSpikeCast(Vector2.down, false).x > -9998.9f || SpecialSpikeCast(Vector2.down, false).y > -9998.9f || SpecialSpikeCast(Vector2.down, false).x < -9999.1f || SpecialSpikeCast(Vector2.down, false).y < -9999.1f) {
            spikePoint = SpecialSpikeCast(Vector2.down, false);
        } else {
            if (CheckGroundedPlatformOneWay((Vector2) groundPoint.transform.position, 2f, Vector2.up)) {
                spikePoint = SpecialSpikeCast(Vector2.up, false);
            } else {
                spikePoint = SpecialSpikeCast(Vector2.down, true);
            }
        }
        slamInputStamp = -9.0f;
        slamInput = 0;
        slamState = 0;
        slamming = false;
        accelerationControl = false;
        assistWallSlide = 0;
        lockedWallSliding = false;
        crossSceneDeath = false;
        inGround = false;
        slamInputBool = false;
        /*
        mushroomJump = false;
        mushroomCharging = false;
        */
        gliderObtained = false;
        gliderActivated = false;
        inZiplinePoint = false;
        prevInZiplinePoint = false;
        wallJumpApplyForce = 0f;
        wallJumpForceApplicationTimestamp = -9.0f;
        ventGliding = false;
        onCrumble = false;
        currentGliderReference = null;
        conduitHitTimestamp = -9.0f;
        if (hookObject != null) {
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
        if (roomBox != null) {
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f;
        }
        ventCancelTimer = -9.0f;
        spinAttacking = false;
        if (slashGraphic != null) {
            Destroy(slashGraphic);
        }
        spinAttackFrame = false;
        spinAttackStage = 0;
        chargingSpin = false;
        spinChargeStamp = -9.0f;
        spinChargeParticleStamp = -9.0f;
        spinAttackTimer = -9.0f;
        zipReenterDisable = false;
        swimming = false;
        dolphinJumping = false;
        //inWhirlpool = 0;
        whirlpoolCenters = new List<Vector3>();
        inCannon = false;
        superdashing = false;
        cannonLaunchStamp = -9.0f;
        inPurpleWater = false;
        canCreateTempSpike = true;
        waterBlockWaitTime = -9.0f;
        touchingSponge = false;
        superdashEndStamp = -9.0f;
        hideHook = false;
        otherIgnoreInvinc = false;
        noInvincibleBlink = false;
        inBubble = false;
        bubbleWarping = false;
        spikeInvincible = false;
        bubbleCancelling = false;
        bubbleWarpTimestamp = -9.0f;
        swimStartStamp = -9.0f;
        bubbleBuffer = 0;
        waterLockTurningTimer = -9.0f;
        logSlamHeight = 0f;
        bubbleExtendLockStamp = -9.0f;
        bubbleExtendLocking = false;
        dolphinTransFrame = false;
        smoothDolphinTargetAngle = false;
        dolphinLerpWaitStamp = -9.0f;
        if (screenLock != null) {
            Destroy(screenLock);
        }
        lockRoomTransitioning = 0;
        cancelDolphinMomentum = false;
        bubbleCancelTeleporting = false;
        quadrantCounter = Vector4.zero;
        touchingQuadrant = false;
        waitingForDashCutsceneLand = false;
        cutscene42Dash = false;
        slowdownStartFrames = 0;
        inCutscene = false;
        timelineSpeedOverride = false;
        timelinePause = false;
        timelinePauseTime = 0.0;
        markers = null;
        markerIndex = 0;
        respawnStamp = StaticDataCW.Time;
        attackBuffer = false;
        attackInputTimer = -9.0f;
        upDooring = false;
        beingPushed = false;
        dying = false;
        freeze = false;
        freezeStamp = -9.0f;
        inAbductorCircle = false;
        if (anim != null) {
            anim.enabled = true;
        }
        reducedGravity = false;
        reducedGravity2 = 0;
        extremeGravity = false;
        specialTarget = false;
        cyprusBubble = false;
        cutsceneCameraParented = false;
        if (cameraPoint != null)
        {
            cameraPoint.transform.parent = transform;
        }
    }

    void ObjectSetup()
    {
        if (groundPoint == null)
        {
            groundPoint = new GameObject(this.name + " GroundPoint");
            groundPoint.transform.parent = this.gameObject.transform;
            bcsize = bc.size;
            groundPoint.transform.localPosition = new Vector3(0f,(-0.5f*bcsize.y) + bc.offset.y,0f);
            groundPoint.tag = "Groundpoint";
        }
        if (cameraPoint == null)
        {
            cameraPoint = new GameObject(this.name + " CameraPoint");
            cameraPoint.transform.parent = this.gameObject.transform;
            cameraPoint.transform.localPosition = new Vector3(0f, 0f, 0f);
            cameraPoint.tag = "CameraPoint";
        }
        if (waterBlockChecker == null)
        {
            waterBlockChecker = new GameObject(this.name + " WaterBlockChecker");
            waterBlockChecker.transform.parent = this.gameObject.transform;
            waterBlockChecker.transform.localPosition = new Vector3(0f, 0f, 0f);
            waterBlockChecker.layer = 25;
            waterBlockChecker.AddComponent<BoxCollider2D>();
            waterBlockChecker.GetComponent<BoxCollider2D>().isTrigger = false;
            waterBlockChecker.GetComponent<BoxCollider2D>().size = new Vector2(0.05f, 0.05f);
            waterBlockChecker.AddComponent<FrostGuardDetectorCW>();
            waterBlockChecker.AddComponent<Rigidbody2D>();
            waterBlockChecker.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            waterBlockChecker.GetComponent<Rigidbody2D>().constraints |= RigidbodyConstraints2D.FreezeRotation;
            waterBlockChecker.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        if (waterBlockChecker2 == null)
        {
            waterBlockChecker2 = new GameObject(this.name + " WaterBlockChecker2");
            waterBlockChecker2.transform.parent = this.gameObject.transform;
            waterBlockChecker2.transform.localPosition = new Vector3(0f, 0f, 0f);
            waterBlockChecker2.layer = 25;
            waterBlockChecker2.AddComponent<BoxCollider2D>();
            waterBlockChecker2.GetComponent<BoxCollider2D>().isTrigger = false;
            waterBlockChecker2.GetComponent<BoxCollider2D>().size = new Vector2(0.51f, 0.9f);
            waterBlockChecker2.AddComponent<FrostGuardDetectorCW>();
            waterBlockChecker2.AddComponent<Rigidbody2D>();
            waterBlockChecker2.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            waterBlockChecker2.GetComponent<Rigidbody2D>().constraints |= RigidbodyConstraints2D.FreezeRotation;
            waterBlockChecker2.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        if (waterBlockCollider == null)
        {
            waterBlockCollider = Instantiate(Resources.Load<GameObject>("Prefabs/WaterBlockCollider") as GameObject);
            waterBlockCollider.transform.parent = this.gameObject.transform;
            waterBlockCollider.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        if (semisolidGround == null) {
            semisolidGround = GameObject.FindWithTag("Semisolid");
        }
        if (spriteobj == null)
        {
            spriteobj = GameObject.FindWithTag("Spriteobj");
            sr = spriteobj.GetComponent<SpriteRenderer>();
            anim = spriteobj.GetComponent<Animator>();
        }
        frozenSprite = Resources.Load<Sprite>("Textures/PlayerFrozen");
        staffSwing = Resources.Load<GameObject>("Prefabs/Swing");
        spinAttackAir = Resources.Load<GameObject>("Prefabs/SpinAttackAir");
        spinChargeParticle = Resources.Load<GameObject>("Prefabs/SpinChargeParticles");
        spinChargeParticle2 = Resources.Load<GameObject>("Prefabs/SpinChargeParticles2");
        healingOrb = Resources.Load<GameObject>("Prefabs/HealingOrb");
        healEffect = Resources.Load<GameObject>("Prefabs/HealEffect");
        dashHitbox = Resources.Load<GameObject>("Prefabs/DashHitbox");
        dolphinHitbox = Resources.Load<GameObject>("Prefabs/DolphinHitbox");
        splash1Ref = Resources.Load<GameObject>("Prefabs/Splash");
        splash2Ref = Resources.Load<GameObject>("Prefabs/Splash2");
        SceneManagerPrefab = Resources.Load<GameObject>("Prefabs/SceneManager");
        if (GameObject.FindWithTag("SceneManager") == null) {
            SceneManagerObj = Instantiate(SceneManagerPrefab);
        } else {
            SceneManagerObj = GameObject.FindWithTag("SceneManager");
        }
        cameraController = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>();
        sm = SceneManagerObj.GetComponent<SceneManagerCW>();
        //Instantiate(Resources.Load<GameObject>("Prefabs/LineRenderer"), transform.position, Quaternion.identity);
        if (pointLight == null)
        {
            GameObject lightobj = new GameObject(this.name + "Point Light");
            lightobj.transform.parent = this.gameObject.transform;
            lightobj.transform.localPosition = new Vector3(0, 0.485f, 0);
            lightobj.tag = "Light";
            pointLight = lightobj.AddComponent<Light2D>();
            pointLight.lightType = (Light2D.LightType)Light2D.LightType.Point;
            pointLight.pointLightInnerRadius = 0.2f;
            pointLight.pointLightOuterRadius = 0.25f;
            pointLight.color = Color.red;
            pointLight.intensity = 1f;
            pointLight.applyToSortingLayers = new int[1];
            if (GameObject.Find("GameController") != null){
                pointLight.applyToSortingLayers[0] = 0; //default
            }
        }
        if (dashTrail == null)
        {
            GameObject dashTrailObj = Instantiate(Resources.Load("Prefabs/DashTrail")) as GameObject;
            dashTrailObj.transform.parent = this.gameObject.transform;
            dashTrailObj.transform.localPosition = new Vector3(0, 0, 0);
            dashTrail = dashTrailObj.GetComponent<ParticleSystem>();
            dashTrail.Stop();
        }
        if (bubbleTrail == null)
        {
            GameObject bubbleTrailObj = Instantiate(Resources.Load("Prefabs/BubbleTrail")) as GameObject;
            bubbleTrailObj.transform.parent = this.gameObject.transform;
            bubbleTrailObj.transform.localPosition = new Vector3(0, 0, 0);
            bubbleTrail = bubbleTrailObj.GetComponent<ParticleSystem>();
            bubbleTrail.Stop();
        }
        if (emanateLight == null)
        {
            GameObject emlightobj = new GameObject(this.name + "Emanate Light");
            emlightobj.transform.parent = this.gameObject.transform;
            emlightobj.transform.localPosition = new Vector3(0, 0.485f, 0);
            emlightobj.tag = "Light";
            emanateLight = emlightobj.AddComponent<Light2D>();
            emanateLight.lightType = (Light2D.LightType)Light2D.LightType.Point;
            emanateLight.pointLightInnerRadius = 0.5f;
            emanateLight.pointLightOuterRadius = 1.5f;
            emanateLight.color = Color.red;
            emanateLight.intensity = 0.5f;
            emanateLight.applyToSortingLayers = new int[2];
            if (GameObject.Find("GameController") != null){
                emanateLight.applyToSortingLayers[0] = GameInitializerCW.SortIDs["Decals"]; //decals
            }
            if (GameObject.Find("GameController") != null){
                emanateLight.applyToSortingLayers[1] = GameInitializerCW.SortIDs["Wall"]; //wall
            }
        }
        if (maskobj == null)
        {
            maskobj = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerMask"), transform.position, Quaternion.identity);
            maskobj.transform.parent = this.gameObject.transform;
            maskobj.transform.localPosition = Vector3.zero;
        }
        /*if (dashCancelGraphic == null) {
            dashCancelGraphic = Instantiate(Resources.Load<GameObject>("Prefabs/DashCancel"), transform.position, Quaternion.identity);
            dashCancelGraphic.transform.parent = this.gameObject.transform;
            dashCancelGraphic.transform.localPosition = Vector3.zero;
            dashCancelGraphic.SetActive(false);
        }*/
        if (targetSelect == null)
        {
            targetSelect = Resources.Load<GameObject>("Prefabs/DashSelector");
        }
        if (slamShockwave == null)
        {
            slamShockwave = Resources.Load<GameObject>("Prefabs/SlamCharge");
        }
        if (hookObject == null)
        {
            hookObject = Instantiate(Resources.Load<GameObject>("Prefabs/HookObject"), transform.position, Quaternion.identity);
        }
        if (dm == null)
        {
            if (GameObject.FindWithTag("Canvas") != null) {
                dm = GameObject.FindWithTag("Canvas").GetComponent<DialogueManagerCW>();
            }
        }
        if (slashEffect == null)
        {
            slashEffect = Resources.Load<GameObject>("Prefabs/Slash");
        }
        if (gliderBack == null)
        {
            gliderBack = Instantiate(Resources.Load<GameObject>("Prefabs/GliderBack"), transform.position, Quaternion.identity);
            gliderBack.transform.parent = this.gameObject.transform;
            gliderBack.transform.localPosition = Vector3.zero;
            gliderBack.GetComponent<SpriteRenderer>().enabled = false;
        }
        if (gliderDestroyParticle == null)
        {
            gliderDestroyParticle = Resources.Load<GameObject>("Prefabs/GliderDestroyParticle");
        }
        if (tempSpike == null) {
            tempSpike = Resources.Load<GameObject>("Prefabs/TempSpike");
        }
    }
    public void SetPosition()
    {
        int i;
        for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
            sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
            if (sceneNum != -1) {
                break;
            } 
        }
        Game.loadedGame.abilitiesUnlocked = 0;
        if (StaticDataCW.SpawnAtCP == 1) {
            if (i == 0) {
                Game.loadedGame.abilitiesUnlocked = -2;
            }
            if (GameObject.FindWithTag("CheckpointFirst") != null) {
                transform.position = GameObject.FindWithTag("CheckpointFirst").transform.position;
                StaticDataCW.CurrentCheckpointPos = (new Vector2(GameObject.FindWithTag("CheckpointFirst").GetComponent<Transform>().position.x,GameObject.FindWithTag("CheckpointFirst").GetComponent<Transform>().position.y));
                StaticDataCW.CurrentCheckpointScene = (SceneManager.GetActiveScene().name);
                SaveDataCW.Load();
                int cpNum = GameObject.FindWithTag("CheckpointFirst").GetComponent<CheckpointCW>().checkpointNum;
                if (Game.loadedGame != null && cpNum > Game.loadedGame.furthestCP) {
                    Game.loadedGame.furthestCP = cpNum;
                }
                SaveDataCW.Save();
            } else {
                transform.position = GameObject.FindWithTag("Checkpoint").transform.position;
            }
            StaticDataCW.Dying = false; 
            health = 3;
            StaticDataCW.SpawnAtCP = 0;
            /*if (roomBox != null) {
                foreach (GameObject cutsceneTrigger in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
                    cutsceneTrigger.GetComponent<CutsceneTriggerCW>().DisableTriggers(int.Parse(roomBox.name.Substring(4)), false);
                }
            } else {*/
                eventRoomBuffer = true;
            //}
        } else if (StaticDataCW.SpawnAtCP == 2) {
            transform.position = (Vector3) StaticDataCW.MenuSpawnPos;
            StaticDataCW.Dying = false; 
            //health = StaticDataCW.Health;
            health = 3;
            StaticDataCW.SpawnAtCP = 0;
            /*if (roomBox != null) {
                foreach (GameObject cutsceneTrigger in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
                    cutsceneTrigger.GetComponent<CutsceneTriggerCW>().DisableTriggers(int.Parse(roomBox.name.Substring(4)), false);
                }
            } else {*/
                eventRoomBuffer = true;
            //}
        } else if (StaticDataCW.Dying) {
            transform.position = StaticDataCW.CurrentCheckpointPos;
            StaticDataCW.Dying = false; 
            health = 3;
            invincible = true;
            spikeInvincible = true;
            sr.enabled = false;
            slowmo = false;
            sdDisable = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            dashTrail.Clear();
            bubbleTrail.Clear();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            Time.timeScale = 1f;
            deathFade = Instantiate(Resources.Load("Prefabs/ScreenFadeWorld", typeof(GameObject)) as GameObject);
            deathFade.transform.parent = GameObject.FindWithTag("MainCamera").transform;
            deathFade.transform.localScale = new Vector3(10f, 10f, 10f);
            deathFade.transform.localPosition = new Vector3(0f,0f,10f);
            deathFadeSprite = deathFade.GetComponent<SpriteRenderer>();
            deathFadeTimer = StaticDataCW.Time;
            deathFadeSprite.color = new Color(1f,0.5f,1f,1f);
            deathFadePlayer = Instantiate(Resources.Load("Prefabs/DyingPlayer", typeof(GameObject)) as GameObject);
            deathFadePlayer.transform.parent = this.transform;
            deathFadePlayer.transform.localPosition = new Vector3(0f,0f,0f);
            deathFadePlayer.GetComponent<SpriteRenderer>().enabled = false;
            SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = true;
            deathFading = true;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            deathFadeReset = true;
            disableAllActions = true;
            inputDisable = true;
            xinput = 0;
            inputDisableTimer = StaticDataCW.Time + 2.5f;
            jumping = false;
            crossSceneDeath = false;
            stopDeathStopTimer = true;
            slamInputBool = false;
        } else if (StaticDataCW.AllScenes[i].Count == 1) {
            transform.position = new Vector3(GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.x,GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.y,0);
            health = 3;
        } else if ((sceneNum == 0 && StaticDataCW.PrevScene != StaticDataCW.AllScenes[i][sceneNum+1])) {
            transform.position = new Vector3(GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.x,GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.y,0);
            health = 3;
        } else if (sceneNum != (StaticDataCW.AllScenes[i].Count - 1) && (StaticDataCW.PrevScene == StaticDataCW.AllScenes[i][sceneNum+1])) {
            transform.position = new Vector3(GameObject.FindWithTag("SceneBoundB").GetComponent<Transform>().position.x,GameObject.FindWithTag("SceneBoundB").GetComponent<Transform>().position.y,0);
            //health = StaticDataCW.Health;
            health = 3;
            /*
            foreach (GameObject cutsceneTrigger in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
                cutsceneTrigger.GetComponent<CutsceneTriggerCW>().DisableTriggers(0, true);
            }*/
        } else {
            transform.position = new Vector3(GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.x,GameObject.FindWithTag("SceneBoundA").GetComponent<Transform>().position.y,0);
            //health = StaticDataCW.Health;
            health = 3;
        }
        if (StaticDataCW.CurrentCheckpointScene == null) {
            StaticDataCW.CurrentCheckpointPos = transform.position;
            StaticDataCW.CurrentCheckpointScene = (SceneManager.GetActiveScene().name);
            health = 3;
        }
        enterSceneLock = true;
        enterSceneStamp = StaticDataCW.Time;
        inputDisable = true;
        inputDisableTimer = StaticDataCW.Time + 999999f;
        if (SpikeCast().x > -9998.9f || SpikeCast().y > -9998.9f || SpikeCast().x < -9999.1f || SpikeCast().y < -9999.1f) {
            transform.position = SpikeCast();
        }
        spikePoint = (Vector2)transform.position;
        if (i == 0) {
            crouchAttackCutscene = true;
            touchEdgeCutscene = true;
        }

        SaveDataCW.Load();
        PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
        if (Game.loadedGame != null && player != null) {
            Game.loadedGame.currentSpikePointx = player.spikePoint.x;
            Game.loadedGame.currentSpikePointy = player.spikePoint.y;
        }
        SaveDataCW.Save();
    }

    void Update()
    {
        if (Game.loadedGame.abilitiesUnlocked >= 0 && Input.GetKeyDown(StaticDataCW.Keys["Dash"]) && (!disableAllActions || swimming || spinAttacking || cutscene42Dash) && !changingScene && framesSinceSceneLoad > 5 && !inCannon && !superdashing){
            hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().material = Resources.Load("Materials/WhiteGlow") as Material;
            dashInputBool = true;
            hookObject.transform.position = (Vector2)gameObject.transform.position + (slowDir);
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
            hookObject.transform.GetChild(0).gameObject.transform.position = gameObject.transform.position;
            if (!slowmo) {
                if(!knockbackDisable && !slowDashing && airDash && (extendDashInvincibilityStamp < 43) && !ziplining && !spikeInvincible && !slamming && !swimming && !spinAttacking) {
                    slowmo = true;
                    slowdownStartFrames = 0;
                    dashCancel = false;
                    startSlowDash = 1;
                    gravityCancel = false;
                    //saveFacingDir = facingdir;
                    slowdownSoundReference = SceneManagerCW.PlaySound("Slowdown", 0.8f);
                    smoothTargetAngle = false;
                } else {
                    dashBuffer = true;
                    dashInputTimer = StaticDataCW.Time;
                }
            }
            RoomConfigureCW roominstance = roomBox.GetComponent<RoomConfigureCW>();
            if (roominstance != null) {
                for (int i = 0; i < roominstance.enemies.Length; i++){
                    if (roominstance.enemies[i] != null){
                        if (roominstance.enemies[i].GetComponent<EnemyDisablerCW>() == null || roominstance.enemies[i].GetComponent<EnemyDisablerCW>().dead == false){
                            Vector2 enemdistvec = (Vector2)roominstance.enemies[i].transform.position - (Vector2)gameObject.transform.position;
                            float distanceenemy = enemdistvec.magnitude;
                            Vector2 dashDir = Vector2.zero;
                            Vector2 hitPoint = Vector2.zero;
                            float dashDist = 0f;
                            bool found = false;
                            float dirToObj = Mathf.Atan2((rb.position.y - roominstance.enemies[i].transform.position.y), (rb.position.x - roominstance.enemies[i].transform.position.x));
                            GameObject hitobj = null;
                            int foundCounter = 0;
                            for (int j = 0; j < 6; j++){
                                found = TargetRay(new Vector2(-Mathf.Cos(dirToObj),-Mathf.Sin(dirToObj)), j, 1.8f, ref dashDir, ref hitPoint, ref dashDist, ref hitobj, false);
                                if (found){foundCounter++;}
                                found = TargetRay(new Vector2(-Mathf.Cos(dirToObj),-Mathf.Sin(dirToObj)), -j, 1.8f, ref dashDir, ref hitPoint, ref dashDist, ref hitobj, false);
                                if (found){foundCounter++;}
                            }
                            if (distanceenemy < 10 && foundCounter > 1){
                                GameObject instanceT = Instantiate(targetSelect);
                                instanceT.transform.position = roominstance.enemies[i].transform.position;
                                instanceT.transform.parent = roominstance.enemies[i].transform;
                            }
                        }
                    }
                }
            }
        }
        if (Game.loadedGame.abilitiesUnlocked >= 0 && (Input.GetKeyUp(StaticDataCW.Keys["Dash"]) || (isBufferedDash == 1 && !Input.GetKey(StaticDataCW.Keys["Dash"]) || (slowmo && !Input.GetKey(StaticDataCW.Keys["Dash"]) && !inCannon && isBufferedDash == 0)) && !disableAllActions && !changingScene && framesSinceSceneLoad > 5)){
            dashInputBool = false;
            if (slowmo){
                spriteobj.transform.localScale = new Vector3(0.6f, 1f, 1f);
            }
            if (slowmo){
                slowmo = false;
                if (airDash && !dashCancel/* && isBufferedDash == 0*/) {
                    airDash = false;
                } else if (dashCancel){
                    //dashCancelGraphic.SetActive(false);
                    slowDir = new Vector2(0,0);
                    //SceneManagerCW.PlaySound("Speed Up", 0.6f);
                    canslowDash = false;
                    inCannon = false;
                    sr.enabled = true;
                    if (cannonRef != null) {
                        cannonRef.GetComponent<CannonCW>().resetDir = true;
                        cannonRef.GetComponent<CannonCW>().resetStamp = StaticDataCW.Time;
                    }
                }
                isBufferedDash = 0;
                dashBuffer = false;
                SceneManagerCW.StopSound(slowdownSoundReference, "Slowdown");
            }
        }
        if (slowmo){
            canslowDash = true;
            if (!dashCancel){
                sdDisable = true;
            }
        } else {
            canslowDash = false;
        }   
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        timerscale = 1/Time.timeScale;

        if (!knockbackDisable && !sdDisable && !disableAllActions && !changingScene) {
            if (Input.GetKeyDown(StaticDataCW.Keys["Jump"])) {
                //if (!mushroomJump) {
                    //sets bool jumping to true so we can communicate to fixedupdate to apply the vel
                    jumping = true;
                    //jumptimer is for if you press jump before you hit the ground, will save input up to 0.17 secs
                    jumpTimer = StaticDataCW.Time;
                /*} else {
                    mushroomJumpTimer = StaticDataCW.Time + 1f;
                    mushroomCharging = true;
                }*/
            }
            if (StaticDataCW.Time >= jumpTimer + jumpBuffer){
                //if you press jump more than 0.1f before you hit the ground, make it so you dont jump when hit ground
                jumping = false;
            }
            if (StaticDataCW.Time >= dashInputTimer + 0.3f){
                dashBuffer = false;
            }
            if (StaticDataCW.Time >= attackInputTimer + 0.1f && attackBuffer) {
                attackBuffer = false;
            }
            if (dashBuffer && !knockbackDisable && !slowDashing && airDash && (extendDashInvincibilityStamp < 43) && !ziplining && !spikeInvincible && !slamming && !slowmo && !swimming) {
                slowmo = true;
                slowdownStartFrames = 0;
                dashCancel = false;
                startSlowDash = 1;
                gravityCancel = false;
                //saveFacingDir = facingdir;
                canslowDash = true;
                slowdownSoundReference = SceneManagerCW.PlaySound("Slowdown", 0.8f);
                isBufferedDash = 4;
                hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().material = Resources.Load("Materials/WhiteGlow") as Material;
                hookObject.transform.position = (Vector2)gameObject.transform.position + (slowDir);
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                hookObject.transform.GetChild(0).gameObject.transform.position = gameObject.transform.position;
            }
            /*if ((Input.GetKeyDown(StaticDataCW.Keys["Heal"]) || queueHeal) && StaticDataCW.HealingOrbs > 0 && health + healing < 10 && health != 0) {
                if (healing == 0) {
                    healing = 1;
                    StaticDataCW.HealingOrbs -= 1;
                    healStamp = StaticDataCW.Time;
                    Instantiate(healingOrb, transform.position, Quaternion.identity);
                    Instantiate(healEffect, gameObject.transform);
                    queueHeal = false;
                    SceneManagerCW.PlaySound("Heal", 1);
                } else if (healing == 1) {
                    if (StaticDataCW.Time > healStamp + 0.26f) {
                        healing ++;
                        StaticDataCW.HealingOrbs -= 1;
                        healStamp2 = StaticDataCW.Time;
                        Instantiate(healingOrb, transform.position, Quaternion.identity);
                        Instantiate(healEffect, gameObject.transform);
                        queueHeal = false;
                        SceneManagerCW.PlaySound("Heal", 1);
                    } else {
                        queueHeal = true;
                    }
                } else if (healing == 2) {
                    queueHeal = true;
                }
            }
            if (queueHeal && StaticDataCW.HealingOrbs == 0) {
                queueHeal = false;
            }*/
            if (Input.GetKey(StaticDataCW.Keys["Attack"]) && !chargingSpin && Game.loadedGame.abilitiesUnlocked >= -1) {
                chargingSpin = true;
                spinChargeStamp = StaticDataCW.Time;
                spinChargeParticleStamp = StaticDataCW.Time + 0.2f;
            }
            if ((Input.GetKeyDown(StaticDataCW.Keys["Attack"]) || attackBuffer) && Game.loadedGame.abilitiesUnlocked >= -1 && framesSinceSceneLoad > 3 && !wallSlidingAnims && !slowmo && !ziplining && !slamming && !swimming && (!dolphinJumping || dolphinJumpFrames <= 14)/* && !gliderActivated*/) {
                if (attackLock) {
                    if (!attackBuffer) {
                        attackBuffer = true;
                        attackInputTimer = StaticDataCW.Time;
                    }
                } else {
                    attackBuffer = false;
                    gliderActivated = false;
                    GameObject swingInstance = Instantiate(staffSwing, transform.position, Quaternion.identity) as GameObject;
                    swingInstance.transform.parent = transform;
                    if (atk != null) {
                        atk.DestroySelf();
                    }
                    atk = swingInstance.GetComponent<AttackCW>();
                    atk.crouching = false;
                    if (Input.GetKey(StaticDataCW.Keys["Down"]) && !crouching && !onGround) {
                        atk.dir = -2;
                    } else if (crouching) {
                        atk.crouching = true;
                        if (sr.flipX) {
                            atk.dir = 1;
                        } else {
                            atk.dir = -1;
                        }
                    } else if (Input.GetKey(StaticDataCW.Keys["Up"])) {
                        atk.dir = 2;
                    } else if (sr.flipX) {
                        atk.dir = 1;
                    } else {
                        atk.dir = -1;
                    }
                    if (StaticDataCW.Time > attackStamp + 0.7f){
                        attackAnimIndex = 0;
                    } else {
                        attackAnimIndex++;
                    }
                    int attackSound = attackAnimIndex;
                    if (attackAnimIndex > 2){
                        attackSound = 2;
                    }
                    slashGraphic = Instantiate(slashEffect, transform.position, Quaternion.identity) as GameObject;
                    slashGraphic.transform.parent = this.transform;
                    slashGraphic.GetComponent<SpriteRenderer>().flipX = sr.flipX;
                    Animator slashAnim = slashGraphic.GetComponent<Animator>();
                    int dirGraph;
                    if (sr.flipX){
                        dirGraph = -1;
                    } else {
                        dirGraph = 1;
                    }
                    if (atk.dir == 1 || atk.dir == -1){ 
                        switch (attackAnimIndex){
                            case 0:
                                slashGraphic.transform.position += new Vector3(0.41f * dirGraph, 0f, 0);
                                slashAnim.SetInteger("Type", 0);
                                break;
                            case 1:
                                slashGraphic.transform.position += new Vector3(0.51f * dirGraph, -0.33f, 0);
                                slashAnim.SetInteger("Type", 1);
                                break;
                            case 2:
                                slashGraphic.transform.position += new Vector3(0.5f * dirGraph, 0.33f, 0);
                                slashAnim.SetInteger("Type", 2);
                                break;
                            case 3:
                                slashGraphic.transform.position += new Vector3(0.41f * dirGraph, 0f, 0);
                                slashAnim.SetInteger("Type", 0);
                                break;
                        }
                    } else if (atk.dir == 2) {
                        slashGraphic.transform.position += new Vector3(0.16f * dirGraph, 0.64f, 0);
                        slashAnim.SetInteger("Type", -1);
                        slashAnim.SetInteger("Dir", 2);
                    } else if (atk.dir == -2) {
                        slashGraphic.transform.position += new Vector3(0.07f * dirGraph, -1f, 0);
                        slashAnim.SetInteger("Type", -1);
                        slashAnim.SetInteger("Dir", -2);
                    }
                    
                    SceneManagerCW.PlaySound("Slash" + (attackSound + 1), 1);
                    atkdir = atk.dir;
                    if ((atkdir == 1 || atkdir == -1) && yvel < 0f && atkfloat == 0) {
                        atkfloat = 1;
                    }
                    atk.type = 1;
                    atk.disabletime = 0.05f;
                    atk.deathtime = 0.1f;
                    atk.dmgValue = 1;
                    attackLock = true;
                    attackStamp = StaticDataCW.Time;
                    animFlipDir = facingdir;
                    /*if (!Input.GetKey(StaticDataCW.Keys["Up"]) && !Input.GetKey(StaticDataCW.Keys["Down"])) {
                        Xvel(4*facingdir,false);
                    }*/
                }
            }

            if (attackLock && StaticDataCW.Time > attackStamp + 0.21f) {
                attackLock = false;
                wallAttacking = false;
            }
        }
        if (!Input.GetKey(StaticDataCW.Keys["Attack"]) && chargingSpin) {
            if (StaticDataCW.Time > spinChargeStamp + 0.7f && !slowmo && !slowDashing && !sdDisable && !slamming && !ziplining && !wallSliding && !disableAllActions) {
                chargingSpin = false;
                spinAttacking = true;
                gravityCancel = false;
                spinAttackFrame = true;
                spinAttackStage = 1;
                spinAttackTimer = StaticDataCW.Time;
                slashGraphic = Instantiate(slashEffect, transform.position, Quaternion.identity) as GameObject;
                slashGraphic.transform.parent = this.transform;
                slashGraphic.GetComponent<SpriteRenderer>().flipX = sr.flipX;
                Animator slashAnim = slashGraphic.GetComponent<Animator>();
                if (onGround) {
                    spinAttackType = 1;
                    spinAttackSpeed = 10f;
                    slashAnim.SetInteger("Type", 3);
                    slashAnim.SetInteger("Dir", 1);
                } else {
                    spinAttackType = 2;
                    //Yvel(0f, true);
                    Xvel(rb.velocity.x, true);
                    Yvel(Mathf.Lerp(yvel, 0f, 0.2f), true);
                    slashAnim.SetInteger("Type", 3);
                    slashAnim.SetInteger("Dir", 0);
                }
                inputDisableTimer = StaticDataCW.Time + 0.8f;
                disableAllActions = true;
                inputDisable = true;
                xinput = 0f;
                gliderActivated = false;
                dolphinJumping = false;
            } else {
                chargingSpin = false;
            }
        }
        if (Input.GetKey(StaticDataCW.Keys["Down"]) && (xinput == 0) && onGround && !knockbackDisable && !sdDisable && !disableAllActions) {
            spriteobj.transform.localScale = new Vector3(1f, 0.79f, 1);
            crouching = true;
        } else {
            crouching = false;
        }
        if (Input.GetKeyUp(StaticDataCW.Keys["Down"]) && (xinput == 0) && onGround && !knockbackDisable && !sdDisable && !disableAllActions) {
            spriteobj.transform.localScale = new Vector3(spriteobj.transform.localScale.x, 0.9f, 1);
        }
        if (!disableAllActions) {
            if ((!Input.GetKey(StaticDataCW.Keys["Down"]) && !Input.GetKey(StaticDataCW.Keys["Up"]) && !onGround) || (!Input.GetKey(StaticDataCW.Keys["Up"]) && onGround)){
                yinput = 0;
            } else if (Input.GetKey(StaticDataCW.Keys["Down"]) && !onGround){
                yinput = -1;
            } else if (Input.GetKey(StaticDataCW.Keys["Up"]) && !Input.GetKey(StaticDataCW.Keys["Down"])){
                yinput = 1;
            }
        }
        if (Input.GetKeyDown(StaticDataCW.Keys["Down"]) && !disableAllActions) {
            downPressed = 1;
        }
        if (Input.GetKeyDown(StaticDataCW.Keys["Special"]) && !slamming && !wallSliding && !onGround && !slowmo && !slowDashing && (!inputDisable || ziplining) && reducedGravity2 == 0) {
            slamInputBool = true;
        }
    }

    void FixedUpdate()
    {
        if (noclip) {
            bc.isTrigger = true;
            if (Input.GetKey(StaticDataCW.Keys["Left"])  && !Input.GetKey(StaticDataCW.Keys["Right"])) {
                xinput = -1f;
                prevxheld = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Right"])  && !Input.GetKey(StaticDataCW.Keys["Left"])) {
                xinput = 1f;
                prevxheld = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Left"])  && Input.GetKey(StaticDataCW.Keys["Right"])) {
                if (prevxheld == false) {
                    xinput = prevxinput*-1f;
                } else {
                    xinput = prevxinput;
                }
                prevxheld = true;
            } else {
                xinput = 0f;
                prevxheld = false;
            }
            if (Input.GetKey(StaticDataCW.Keys["Down"])  && !Input.GetKey(StaticDataCW.Keys["Up"])) {
                noclipyinput = -1f;
                noclipprevy = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Up"])  && !Input.GetKey(StaticDataCW.Keys["Down"])) {
                noclipyinput = 1f;
                noclipprevy = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Down"])  && Input.GetKey(StaticDataCW.Keys["Up"])) {
                if (noclipprevy == false) {
                    noclipyinput = noclipprevyinput*-1f;
                } else {
                    noclipyinput = noclipprevyinput;
                }
                noclipprevy = true;
            } else {
                noclipyinput = 0f;
                noclipprevy = false;
            }
            if (Input.GetKey(StaticDataCW.Keys["Jump"])) {
                rb.velocity = noclipSpeed * 3f * new Vector2(xinput, noclipyinput);
            } else {
                rb.velocity = noclipSpeed * new Vector2(xinput, noclipyinput);
            }
            if (changingScene && Time.time > changeSceneStamp + 0.3f) {
                int i;
                for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
                    sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
                    if (sceneNum != -1) {
                        break;
                    } 
                }
                if (whichBound == 0) {
                    SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(StaticDataCW.AllScenes[i][sceneNum-1], false);
                } else if (whichBound == 1) {
                    if (!StaticDataCW.InWorldMap) {
                        SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(StaticDataCW.AllScenes[i][sceneNum+1], false);
                    } else {
                        changingScene = false;
                        SceneManagerObj.GetComponent<SceneManagerCW>().ReturnToMainMenu(true);
                    }
                } else {
                    string scene = StaticDataCW.CurrentCheckpointScene;
                    //StaticDataCW.Health = 3;
                    SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(scene, true);
                }
            }
        } else {
        if (framesSinceSceneLoad < 300) {
            framesSinceSceneLoad ++;
        }
        if (SceneManager.GetActiveScene().name != "PrologueScene1") {
            if (framesSinceSceneLoad >= 20 && enterSceneLock) {
                enterSceneLock = false;
                inputDisable = false;
            }
        } else {
            if (StaticDataCW.Time > enterSceneStamp + 0.5f && enterSceneLock) {
                enterSceneLock = false;
                inputDisable = false;
            }
        }
        /*
        if (eventRoomBuffer && roomBox != null) {
            foreach (GameObject cutsceneTrigger in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
                cutsceneTrigger.GetComponent<CutsceneTriggerCW>().DisableTriggers(int.Parse(roomBox.name.Substring(4)), false);
            }
            eventRoomBuffer = false;
        }*/
        if (StaticDataCW.Time > deathFadeTimer + 1.2f && deathFading && deathFadeReset) {
            StaticDataCW.HealingOrbs = 0;
            health = 3;
            if (crossSceneDeath) {
                string scene = StaticDataCW.CurrentCheckpointScene;
                //StaticDataCW.Health = 3;
                SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(scene, true);
            } else {
                Reset();
                resetting = 2;
                rb.position = StaticDataCW.CurrentCheckpointPos;
                deathFadeReset = false;
            }
        }
        if (deathFadeReset && !deathFading && StaticDataCW.Time > inputDisableTimer - 0.1f) {
            Destroy(deathFadePlayer);
            foreach (GameObject obj in blackBars) {
                Destroy(obj);
            }
            sr.enabled = true;
            sdDisable = false;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            invincible = false;
            spikeInvincible = false;
            if (SceneManagerObj != null) {
                SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = false;
            }
            disableAllActions = false;
            deathFadeReset = false;
        }
        if (deathFadeReset && !deathFading) {
            GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.HardOut, 0.7f);
        }
        if (stopDeathStopTimer && StaticDataCW.Time < deathFadeTimer + 1.8f && deathFading) {
            deathFade.GetComponent<DeathFadeCW>().stopTimer = StaticDataCW.Time;
        }
        if (StaticDataCW.Time > deathFadeTimer + 1.8f && deathFading) {
            GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
            Reset();
            rb.position = SpikeCast();
            inputDisable = true;
            xinput = 0;
            inputDisableTimer = StaticDataCW.Time + 0.8f;
            sdDisable = true;
            jumping = false;
            deathFading = false;
            if (deathFadePlayer != null) {
                deathFadePlayer.GetComponent<Animator>().Play("PlayerRespawn");
                deathFadePlayer.transform.localPosition = new Vector3(0f,-0.5f,0f);
                deathFadePlayer.GetComponent<SpriteRenderer>().enabled = true;
                deathFadeReset = true;
            }
            if (stopDeathStopTimer) {
                deathFade.GetComponent<DeathFadeCW>().stopTimer = StaticDataCW.Time - 1.5f;
                stopDeathStopTimer = false;
            }
        }
        if (changingScene && Time.time > changeSceneStamp + 0.3f) {
            int i;
            for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
                sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
                if (sceneNum != -1) {
                    break;
                } 
            }
            if (whichBound == 0) {
                SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(StaticDataCW.AllScenes[i][sceneNum-1], false);
            } else if (whichBound == 1) {
                if (!StaticDataCW.InWorldMap) {
                    SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(StaticDataCW.AllScenes[i][sceneNum+1], false);
                } else {
                    changingScene = false;
                    SceneManagerObj.GetComponent<SceneManagerCW>().ReturnToMainMenu(true);
                }
            } else {
                string scene = StaticDataCW.CurrentCheckpointScene;
                //StaticDataCW.Health = 3;
                SceneManagerObj.GetComponent<SceneManagerCW>().ChangeScene(scene, true);
            }
        }
        if (extendDashInvincibilityStamp > 0) {
            extendDashInvincibilityStamp --;
        }
        smoothWaitFrames++;
        if (startSlowDash == 1) {
            startSlowDash = 2;
        }
        if (saveTargetDash) {
            saveTargetDash = false;
        }
        if (!SceneManagerCW.paused) {
            if (slowmo && !changingScene) {
                if (Time.timeScale > 0.5f) {
                    Time.timeScale = (0.82f - 0.1f*(Mathf.Min(20f, Mathf.Abs(yvel))/20f)) * Time.timeScale;
                } else {
                    Time.timeScale = 0.82f * Time.timeScale;
                }
                Mathf.Lerp(xaccel, 0f, 0.4f);
                if (yvel < -10f) {
                    yvel = -10f;
                }
                Mathf.Lerp(yvel, 0f, 0.4f);

            } else {
                Time.timeScale = 1.65f * Time.timeScale;
            }
            if (Time.timeScale <= 0.005f){
                Time.timeScale = 0.005f;
            }
            if (Time.timeScale > 0.9f) {
                Time.timeScale = 1f;
            }
        } else {
            Time.timeScale = 0f;
        }
        if (resetting > 0) {
            resetting -= 1;
        }
        if (bubbleBuffer > 0) {
            bubbleBuffer--;
        }
        if (spikeResetting > 0) {
            spikeResetting -= 1;
        }
        if (isBufferedDash > 0) {
            isBufferedDash -= 1;
        }
        if (downPressed > 0) {
            downPressed += 1;
        }
        if (slopeBuffer > 0) {
            slopeBuffer --;
        }
        if (ignoreSlopeBufferFrames > 0) {
            ignoreSlopeBufferFrames --;
        }
        if (ignorePrevWallSliding) {
            ignorePrevWallSliding = false;
        }
        if (disablePlatforms > 0) {
            if (disablePlatforms == 5) {
                if (disablePlatformsBuffer == 1) {
                    disablePlatforms --;
                    disablePlatformsBuffer = 0;
                } else if (wallSliding && disablePlatformsBuffer == 0) {
                    disablePlatformsBuffer = 24;
                } else if (disablePlatformsBuffer > 1) {
                    disablePlatformsBuffer--;
                } else {
                    disablePlatforms --;
                }
            } else {
                disablePlatforms --;
            }
        } else {
            disablePlatforms = 0;
            disablePlatformsBuffer = 0;
            if (semisolidGround != null) {
                semisolidGround.GetComponent<PlatformEffector2D>().colliderMask = -1;
            }
        }
        if (ensureSpikePoint && (SpikeCast().x > -9998.9f || SpikeCast().y > -9998.9f || SpikeCast().x < -9999.1f || SpikeCast().y < -9999.1f) && !ziplining) {
            spikePoint = SpikeCast();
            ensureSpikePoint = false;
        }
        
        /*if (Time.time > saveStamp + 5f) {
            SaveDataCW.Load();
            if (Game.loadedGame != null) {
                Game.loadedGame.currentSpikePointx = spikePoint.x;
                Game.loadedGame.currentSpikePointy = spikePoint.y;
                Game.loadedGame.health = health;
                Game.loadedGame.healingOrbs = StaticDataCW.HealingOrbs;
            }
            saveStamp = Time.time;
            SaveDataCW.Save();
        }*/

        //comment this out/uncomment when recording
        /*
        framesSinceLastRecordingSpawn++;
        if (Input.GetKey(KeyCode.C) && framesSinceLastRecordingSpawn > 2) {
            GameObject spawn = new GameObject();
            spawn.transform.position = transform.position;
            spawn.name = "HologramPoint";
            spawn.AddComponent<CustomPropertiesCW>().string1 = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (slowmo || sdDisable) {
                spawn.GetComponent<CustomPropertiesCW>().vector2_1 = slowDir;
            }
            spawn.GetComponent<CustomPropertiesCW>().bool1 = sr.flipX;
            framesSinceLastRecordingSpawn = 0;
        }
        */
        if (cutscene42Dash) {
            airDash = true;
        }
        if (freeze) {
            /*Xvel(0f, true);
            Yvel(0f, true);
            anim.enabled = false;
            sr.sprite = frozenSprite;*/
            if (StaticDataCW.Time > freezeStamp + 5.5f) {
                /*disableAllActions = false;
                inputDisable = false;
                anim.enabled = true;*/
                freeze = false;
            }
        }
        
        
        if (healing == 2 && StaticDataCW.Time > healStamp + 0.5f) {
            if (health < 10) {
                health ++;
            }
            healing --;
            healStamp = healStamp2;
        }
        if (healing == 1 && StaticDataCW.Time > healStamp + 0.5f) {
            if (health < 10) {
                health ++;
            }
            healing --;
        }
        if (canslowDash) {
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = Mathf.Lerp(roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping, 0f, 0.2f);
        } else if (prevcanslowDash) {
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.2f;
        } else if (/*roomBox != cameraZoneRoomBox || */exitedCameraZone) {
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = Mathf.Lerp(roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping, 0f, 0.1f);
            if (StaticDataCW.Time > cameraZoneExitStamp + 0.5f) {
                exitedCameraZone = false;
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.2f;
            }
        }
        framesSinceWallJump++;
        framesSinceEndedSwim++;
        framesSinceEndedNormalDash++;
        //grounddetect
        if (!ziplining) {
            onGround = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up) ||
                    CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.02f, -Vector2.up) ||
                    CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.02f, -Vector2.up);
            onGroundFar = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.4f, -Vector2.up) ||
                    CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.4f, -Vector2.up) ||
                    CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.4f, -Vector2.up);
                    //jumpdetect
            canJump = CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.03f, -Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.03f, -Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.03f, -Vector2.up);
            CheckForCrumble();
        }
        
        //ceildetect
        ceiled = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.0f, bcsize.y), 0.02f, Vector2.up) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, bcsize.y), 0.02f, Vector2.up) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, bcsize.y), 0.02f, Vector2.up);

        if (onGround) {
            gravityCancel = false;
            cancelSlide = false;
            if (waitingForDashCutsceneLand) {
                if (transform.position.y <= -1f) {
                    TextOnlyCutscene(41);
                }
                waitingForDashCutsceneLand = false;
            }
        }
        if (!onGround) {
            standingOnOneWayFrames = 0;
        }

        if (onGround && yvel < 3f && !prevGrounded && gliderObtained && !slowDashing && !ziplining) {
            gliderActivated = false;
            if (!onCrumble && !swimming) {
                gliderObtained = false;
                ventCancelTimer = -9.0f;
                currentGliderReference = null;
                GameObject gliderParticle = Instantiate(gliderDestroyParticle as GameObject);
                gliderParticle.transform.position = transform.position;
                gliderParticle.GetComponent<BurstCW>().deathTime = 0.5f;
            }
            accelerationControl = false;
        }

        if (coyoteFrames > 0) {
            coyoteFrames --;
        }

        if (!onGround && prevGrounded && !inJump && yvel <= 0.05f) {
            coyoteFrames = 3;
        }

        if (!inPurpleWater) {
            canCreateTempSpike = true;
        }

        if ((inZiplinePoint || prevInZiplinePoint) && !ziplining && (!slowmo && onTarget && dashinvincible) && savedZiplinePoint != null && savedZiplinePoint.gameObject == targetHit) {
            rb.position = (Vector2) savedZiplinePoint.transform.position;
            zipPos = (Vector2) savedZiplinePoint.transform.position;
            zipSetPos = 2;
            ziplining = true;
            extendZipMomentum = false;
            inputDisable = true;
            inputDisableTimer = Time.time + 9999999f;
            xinput = 0;
            zipDir = savedZiplinePoint.gameObject.GetComponent<FrostGuardDetectorCW>().ziplineDir;
            airDash = true;
            sdDisable = false;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            SceneManagerCW.PlaySound("Pop", 1);
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            slowDir = new Vector2(0,0);
            onTarget = false;
            dashTurn = false;
            otherZipPoint = savedZiplinePoint.gameObject.GetComponent<FrostGuardDetectorCW>().otherZipPoint;
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        isWalledLeft = CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x,0f), 0.08f, Vector2.left) &&
                CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0.5f*bcsize.y), 0.08f, Vector2.left) &&
                CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, bcsize.y), 0.08f, Vector2.left);

        isWalledRight = CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0f), 0.08f, Vector2.right) &&
                CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0.5f*bcsize.y), 0.08f, Vector2.right) &&
                CheckGroundedWallJump((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, bcsize.y), 0.08f, Vector2.right);

        //sponge
        touchingSponge = false;
        CheckSponge((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0.5f*bcsize.y), 0.08f, Vector2.left, 0.5f*bcsize.y);
        CheckSponge((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0.5f*bcsize.y), 0.08f, Vector2.right, 0.5f*bcsize.y);
        CheckSponge((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.08f, -Vector2.up, 0.5f*bcsize.x);
        CheckSponge((Vector2)groundPoint.transform.position + new Vector2(0.0f, bcsize.y), 0.08f, Vector2.up, 0.5f*bcsize.x);
        
        if (ceiled && !touchingSponge) {
            yvel = 0f;
            inJump = false;
            gravityCancel = true;
        }

        //bubbles
        if (bubbleWarping && bubbleTeleporting && StaticDataCW.Time > bubbleWarpTimestamp + 0.3f) {
            rb.position = bubbleRef.transform.position;
            swimDir = bubbleDir;
            swimSpeed = 0f;
            hasExitedBubble = 0;
        } else if (bubbleWarping && bubbleTeleporting) {
            rb.position = new Vector3(Mathf.Lerp(rb.position.x, bubbleRef.transform.position.x, 0.2f), Mathf.Lerp(rb.position.y, bubbleRef.transform.position.y, 0.2f), 0f);
        }

        if (bubbleWarping && bubbleTeleporting && StaticDataCW.Time > bubbleWarpTimestamp + 0.7f) {
            bubbleTeleporting = false;
            sr.enabled = true;
            spikeInvincible = false;
            swimSpeed = 5f;
            if (WaterCast(5f, swimDir, true) < 2f && WaterCast(5f, swimDir, true) >= 0f) {
                bubbleExtendLockStamp = StaticDataCW.Time;
                bubbleExtendLocking = true;
            }
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0f;
        roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f;
        }

        if (bubbleWarping && StaticDataCW.Time > bubbleWarpTimestamp + 0.95f && !bubbleCancelling) {
            if (hasExitedBubble < 1) {
                swimDir = -swimDir;
                swimSpeed = 5f;
                bubbleCancelTeleporting = true;
                bubbleCancelling = true;
                spikeInvincible = true;
                bubbleExtendLocking = false;
            } else {
                invincible = false;
                noInvincibleBlink = false;
                bubbleWarping = false;
                spikeInvincible = false;
            }
        }
        if (exitBubbleFrameBuffer) {
            exitBubbleFrameBuffer = false;
            hasExitedBubble = 0;
        }

        if (bubbleWarping && StaticDataCW.Time > bubbleWarpTimestamp + 1.15f) {
            if (bubbleCancelTeleporting) {
                rb.position = bubbleSavePos;
                swimDir = bubbleSaveDir;
                hasExitedBubble = 0;
                exitBubbleFrameBuffer = true;
                bubbleCancelTeleporting = false;
                sr.enabled = false;
                swimSpeed = 0f;
                if (WaterCast(5f, swimDir, true) < 2f && WaterCast(5f, swimDir, true) >= 0f) {
                    bubbleExtendLockStamp = StaticDataCW.Time;
                    bubbleExtendLocking = true;
                }
            } else if (StaticDataCW.Time > bubbleWarpTimestamp + 1.5f) {
                if (swimSpeed == 0f) {
                    sr.enabled = true;
                    spikeInvincible = false;
                    swimSpeed = 5f;
                }
                if (hasExitedBubble > 0) {
                    invincible = false;
                    noInvincibleBlink = false;
                    bubbleWarping = false;
                    spikeInvincible = false;
                    bubbleCancelling = false;
                }
            }
        } else if (bubbleWarping && bubbleCancelTeleporting && StaticDataCW.Time > bubbleWarpTimestamp + 0.9f) {
            //rb.position = new Vector3(Mathf.Lerp(rb.position.x, bubbleRef.transform.position.x, 0.2f), Mathf.Lerp(rb.position.y, bubbleRef.transform.position.y, 0.2f), 0f);
        }
        if (bubbleExtendLocking && StaticDataCW.Time > bubbleExtendLockStamp + 0.6f) {
            bubbleExtendLocking = false;
        }

        //updoor
        if (upDooring && StaticDataCW.Time > upDoorStamp + 0.4f) {
            transform.position = new Vector3(upDoorPos.x, upDoorPos.y, 0f);
        }
        if (upDooring && StaticDataCW.Time > upDoorStamp + 0.6f) {
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading = false;
        }
        if (upDooring && StaticDataCW.Time > upDoorStamp + 1.0f) {
            upDooring = false;
            invincible = false;
            inputDisable = false;
            disableAllActions = false;
            noInvincibleBlink = false;
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0f;
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f;
        }

        //swimming water blocks
        if (swimming) {
            if (!waterBlockChecker.GetComponent<FrostGuardDetectorCW>().waterBlockInside && swimCancelLock == 0 && !(bubbleWarping && bubbleTeleporting) && StaticDataCW.Time > swimStartStamp + 0.08f  && StaticDataCW.Time > SceneManagerObj.GetComponent<SceneManagerCW>().unPauseStamp + 0.05f) {
                swimming = false;
                bubbleTrail.Stop();
                cancelDolphinMomentum = false;
                framesSinceEndedSwim = 0;
                dolphinJumping = true;
                SpawnDolphinAttack();
                dolphinJumpFrames = 27;
                bubbleExtendLocking = false;
                if (freeze) {
                    swimSpeed = 14f*0.45f;
                } else {
                    swimSpeed = 14f;
                }
                swimDir = rb.velocity.normalized;
                initialDolphinX = swimDir.x * swimSpeed;
                disableAllActions = false;
                dolphinJumpStamp = StaticDataCW.Time;
                GameObject splash = Instantiate(splash2Ref as GameObject);
                splash.transform.position = transform.position;
                /*
                if (savedEdge != null) {
                    GameObject splash = Instantiate(splash2Ref as GameObject);
                    splash.transform.position = savedEdgePos;
                    if (savedEdge.direction == 0) {
                        splash.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    } else if (savedEdge.direction == 2) {
                        splash.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
                    } else if (savedEdge.direction == 1) {
                        splash.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    } else {
                        splash.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    }
                }*/
                dolphinTransFrame = true;
                /*if (swimDir.y > 0f && swimDir.y < 0.5f) {
                    dolphinGravFloat = swimDir.y/4f;
                }*/
            } else {
                inputDisable = true;
                if (swimCancelLock > 0) {
                    swimCancelLock --;
                }/*
                bool hitWall = false;
                if (isWalledLeft) {
                    swimDir = Vector2.Reflect(swimDir, Vector2.left);
                    hitWall = true;
                }
                if (isWalledRight) {
                    swimDir = Vector2.Reflect(swimDir, Vector2.right);
                    hitWall = true;
                }
                if (onGround) {
                    swimDir = Vector2.Reflect(swimDir, Vector2.down);
                    hitWall = true;
                }
                if (ceiled) {
                    swimDir = Vector2.Reflect(swimDir, Vector2.up);
                    hitWall = true;
                }*/
                /*if ((transform.position-swimStartPoint).magnitude > 0.4f) {
                    swimStartStamp = -9.0f;
                }*/
                float effectiveSwimSpeed = -1f;
                if ((!bubbleWarping && !bubbleExtendLocking/* || (bubbleExtendLocking && !bubbleCancelling)*/) && StaticDataCW.Time > waterLockTurningTimer) {
                    float current = VecToDeg(swimDir);
                    Vector2 mouseVector = Camera.main.ScreenToWorldPoint(CameraControllerCW.InputRemap(Input.mousePosition)/*new Vector3(InputEx.mousePosition.x, (InputEx.mousePosition.y-StaticDataCW.ScreenOffset)*StaticDataCW.ScreenOffsetScale, 0f)*/) - (Vector3) rb.position;
                    float target = VecToDeg(mouseVector);
                    float diff = target - current;
                    if (Mathf.Abs(diff) > 5f && StaticDataCW.Time > swimStartStamp + 0.12f) {
                        if (diff < 0) {
                            diff += 360;
                        }
                        //swimDir = mouseVector.normalized;
                        if (diff > 180) {
                            swimDir = RotateVector(swimDir, ((360f - diff) * (0.2f/*0.1f*/ + Mathf.Abs(180f - diff)/1800f))*(Mathf.Sqrt(mouseVector.magnitude)/0.9f/*1.25f*/)*(bubbleExtendLocking? Mathf.Pow(Mathf.Abs(bubbleExtendLockStamp-StaticDataCW.Time), 2.4f-Mathf.Abs(bubbleExtendLockStamp-StaticDataCW.Time)*1f):1f));
                        } else {
                            swimDir = RotateVector(swimDir, (-diff *  (0.2f/*0.1f*/ + Mathf.Abs(180f - diff)/1800f))*(Mathf.Sqrt(mouseVector.magnitude)/0.9f/*1.25f*/)*(bubbleExtendLocking? Mathf.Pow(Mathf.Abs(bubbleExtendLockStamp-StaticDataCW.Time), 2.4f-Mathf.Abs(bubbleExtendLockStamp-StaticDataCW.Time)*1f):1f));
                        }
                    }
                    //if (!Input.GetKey(StaticDataCW.Keys["Jump"])) {
                        swimSpeed = Mathf.Lerp(swimSpeed, 4f, 0.25f);
                    //} else {
                        //swimSpeed = Mathf.Lerp(swimSpeed, 7f, 0.25f);
                    //}
                    float distToEdge = WaterCast(5f, rb.velocity.normalized, false);
                    if (distToEdge < 2f && distToEdge > 0f) {
                        effectiveSwimSpeed = swimSpeed - 2f + distToEdge;
                    } else {
                        effectiveSwimSpeed = swimSpeed;
                    }
                }
                if (effectiveSwimSpeed == -1) {
                    effectiveSwimSpeed = swimSpeed;
                }
                if (freeze) { 
                    effectiveSwimSpeed *= 0.45f;
                }
                
                /*if (hitWall && swimSpeed < 5f) {
                    swimSpeed = 5f;
                }*/
                Xvel(swimDir.x * effectiveSwimSpeed, true);
                Yvel(swimDir.y * effectiveSwimSpeed, true);
                if (inWhirlpool > 0) {
                    foreach (Vector3 whirlpoolCenter in whirlpoolCenters) {
                        Vector2 whirlDir = ((Vector2) whirlpoolCenter - rb.position).normalized;
                        Xvel(whirlDir.x * 2.75f, false);
                        Yvel(whirlDir.y * 2.75f, false);
                    }
                }
            }
        }
        if (dolphinJumping) {  
            if (dolphinJumpFrames > 19) {
                Xvel(swimDir.x * swimSpeed, true);
                Yvel(swimDir.y * swimSpeed, true);
                if (yvel < 0f) {
                    yvel = 0f;
                }
            } else if (dolphinJumpFrames > 14) {
                Xvel(swimDir.x * swimSpeed, true);
                Yvel(swimDir.y * swimSpeed, true);
                Yvel((dolphinJumpFrames-20)*(1f/* + dolphinGravFloat*/), false);
            }
            
            if (!slowmo) {
                dolphinJumpFrames--;
            }
            if (dolphinJumpFrames <= 19) {
                inputDisable = false;
            }
            if (dolphinJumpFrames <= 14) {
                if (!cancelDolphinMomentum) {
                    extendDolphinMomentum = true;
                } else {
                    extendDolphinMomentum = false;
                }
            }
            /*
            if (dolphinJumpFrames <= 20) {
                if (Mathf.Abs(rb.velocity.x) < Mathf.Abs(initialDolphinX * 0.4f)) {
                    Xvel(initialDolphinX * 0.4f, true);
                }
            }*/

            if (/*dolphinJumpFrames <= 0 || */(onGround && !touchingSponge && dolphinJumpFrames < 22) || slowDashing || wallSliding || slamming || spinAttacking || attackLock) {
                dolphinJumping = false;
                inputDisable = false;
            }
        }
        if (!dolphinJumping) {
            extendDolphinMomentum = false;
        }

        /*if (swimming || slowDashing || dolphinJumping) {
            Edge waterEdge = null;
            Edge purpleWaterEdge = null;
            float waterDist = 0;
            float purpleWaterDist = 0; 
            if (GameObject.FindWithTag("WaterBlock") != null) {
                (waterEdge, waterDist) = GameObject.FindWithTag("WaterBlock").GetComponent<WaterBlockCW>().raycast(rb.velocity.normalized, rb.position);
            } 
            if (GameObject.FindWithTag("PurpleWaterBlock") != null) {
                (purpleWaterEdge, purpleWaterDist) = GameObject.FindWithTag("PurpleWaterBlock").GetComponent<WaterBlockCW>().raycast(rb.velocity.normalized, rb.position);
            } 
            if (waterEdge != null) {
                if (purpleWaterEdge != null) {
                    if (waterDist < purpleWaterDist) {
                        savedEdge = waterEdge;
                    } else {
                        savedEdge = purpleWaterEdge;
                    }
                } else {
                    savedEdge = waterEdge;
                }
            } else if (purpleWaterEdge != null) {
                savedEdge = purpleWaterEdge;
            } else if (!swimming) {
                savedEdge = null;
            }
            if (SwimCast(1.5f, rb.velocity.normalized).x >= -99998f && SwimCast(1.5f, rb.velocity.normalized).y >= -99998f) {
                savedEdgePos = SwimCast(1.5f, rb.velocity.normalized);
            }
        }*/

        if (prevGrounded && !onGround && ignoreSlopeBufferFrames == 0 && (CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.3f, -Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.3f, -Vector2.up) ||
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.3f, -Vector2.up))) {
            slopeBuffer = 17;
        }

        if (!prevGrounded && onGround && slopeBuffer == 0 && prevyvel < 0 && !ziplining && !swimming) {
            SceneManagerCW.PlaySound("Land", 1);
        }

        if (crouchAttackCutscene && crouching && attackLock && roomBox != null && roomBox.name == "Room3" && GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode == 4) {
            crouchAttackCutscene = false;
            TextOnlyCutscene(35);
        }

        //xmovement
        if (!knockbackDisable && !inputDisable && !beingPushed && !slowmo && extendDashInvincibilityStamp < 40/* && !lockedWallSliding*/) {
            if (Input.GetKey(StaticDataCW.Keys["Left"]) && !Input.GetKey(StaticDataCW.Keys["Right"])) {
                xinput = -1f;
                prevxheld = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Right"])  && !Input.GetKey(StaticDataCW.Keys["Left"])) {
                xinput = 1f;
                prevxheld = false;
            } else if (Input.GetKey(StaticDataCW.Keys["Left"])  && Input.GetKey(StaticDataCW.Keys["Right"])) {
                if (prevxheld == false) {
                    xinput = prevxinput*-1f;
                } else {
                    xinput = prevxinput;
                }
                prevxheld = true;
            } else {
                xinput = 0f;
                prevxheld = false;
            }
            if (extendDolphinMomentum) {
                int jumpdir;
                if (xvelmove > 0) {
                    jumpdir = 1;
                } else if (xvelmove < 0) {
                    jumpdir = -1;
                } else {
                    jumpdir = 0;
                }
                
                if (jumpdir == (int)(xinput * -1f)) {
                    cancelDolphinMomentum = true;
                }
            }
        }    
        if ((onGround || swimming) && atkfloat == 5) {
            atkfloat = 0;
        }

        if (!swimming) {
            lastPosOutsideWater = transform.position;
        }

        if (inCutscene) {
            beingPushed = false;
        }
        
        /*
        if (mushroomCharging) {
            if (StaticDataCW.Time < mushroomJumpTimer) {
                if (!Input.GetKey(StaticDataCW.Keys["Jump"])) {
                    mushroomCharging = false;
                } else {
                    xinput = 0;
                }
            } else {
                Yvel(30f, true);
                mushroomCharging = false;
                mushroomJump = false;
            }
        }
        */

        if (!inZiplinePoint || onGround) {
            zipReenterDisable = false;
        }

        /*if (onGround && dashMeter <= 0.98f) {
            dashMeter += 0.04f;
        }   */

        //acceleration, replenishes the frames left to accelerate
        if (xinput != 0 && prevxinput == 0){
            xframesmod = xframes;
        }

        if(xinput != 0 && xinput != prevxinput){
            xframesmod = xframes;
        }
        
        //makes the current rb.velocity.x become a fraction of the target speed based on the # of frames left
        if (accelerationControl) {
            xframes = accelerationValue;
        } else {
            xframes = 8;
        }
        if (accelerationControl) {
                //xaccel = xspeed * Mathf.Pow(accelerationMagnitude, xframesmod) * xinput;
                if ((xinput == 1 && xaccel > 0) || (xinput == -1 && xaccel < 0)) {
                    xaccel = Mathf.Lerp(xaccel, xinput*xspeed, accelerationMagnitude);
                } else {
                    xaccel = Mathf.Lerp(xaccel, xinput*xspeed, decelerationMagnitude);
                }
        } else if((xinput != 0 && xinput == prevxinput)){
            if (onGround) {
                xaccel = xspeed * Mathf.Pow(0.87f, xframesmod) * xinput;
            } else {
                xaccel = xspeed * Mathf.Pow(0.5f, xframesmod) * xinput;
            }
        } else if (!knockbackDisable) {
            xaccel = xaccel - (xaccel * 0.65f * Time.timeScale);
        }
/*
        if (framesSinceWallJump < 20) {
            WallJumpXvel = Mathf.Lerp(WallJumpXvel, 0, 0.5f);
            Xvel(WallJumpXvel, true);
        }
        */

        //decreases # of frames until target x velocity
        xframesmod -= 1;

        //makes sure xframesmod doesnt become negative
        if (xframesmod <= 0){
            xframesmod = 0;
        }
        
        if (!knockbackDisable) {
            if (extendDashInvincibilityStamp < 50 && !extendNormalDashMomentum && !extendZipMomentum && !extendDolphinMomentum && (StaticDataCW.Time > stopNormalDashMomentumStamp + 0.2f || !onGround)) {
                xvelmove = xvelmove - (xvelmove * 0.35f * Time.timeScale);
            } else if (extendNormalDashMomentum) {
                xvelmove = xvelmove - (xvelmove * 0.17f * Time.timeScale);
                if ((onGround || xinput != 0 || isWalledLeft || isWalledRight) && !touchingSponge && !spongeExtending) {
                    extendNormalDashMomentum = false;
                    spongeExtending = false;
                }
            } else if (extendZipMomentum) {
                xvelmove = xvelmove - (xvelmove * 0.1f * Time.timeScale);
                if (onGround || isWalledLeft || isWalledRight) {
                    extendZipMomentum = false;
                }
            } else if (extendDolphinMomentum) {
                xvelmove = xvelmove - (xvelmove * 0.045f * Time.timeScale);
                if (onGround || isWalledLeft || isWalledRight) {
                    extendDolphinMomentum = false;
                }
            } else if (StaticDataCW.Time < stopNormalDashMomentumStamp + 0.2f && onGround) {
                xvelmove = xvelmove - (xvelmove * 0.7f * Time.timeScale);
            } else if (wallJumping) {
                xvelmove = xvelmove - (xvelmove * 0.05f * Time.timeScale);
            }
        } else {
            xvelmove = xvelmove - (xvelmove * 0.13f * Time.timeScale);
        }
        

        if (slowDashing){
            xaccel = 0;
            xvelmove = 0;
        }

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
        
        if (xaccel < 0.005f && xaccel > -0.005f){
            xaccel = 0;
        }

        //applies xvel
        if (freeze) {
            rb.velocity = new Vector2(xaccel*0.45f + xvelmove, rb.velocity.y);
        } else {
            rb.velocity = new Vector2(xaccel + xvelmove, rb.velocity.y);
        }


        //flipping side to side
        if (!inputDisable) {
            if (xinput > 0 && !attackLock && !slowmo){
                sr.flipX = false;
            } 
            if (xinput < 0 && !attackLock && !slowmo){
                sr.flipX = true;   
            }
        } else if (swimming || dolphinJumping) {
            if (swimDir.x >= 0) {
                sr.flipX = true;
            } else {
                sr.flipX = false;
            }
        }

        if (sr.flipX) {facingdir = -1;} else {facingdir = 1;}
        //sets direction facing

        if (xinput > 0) {
            directionOfTravel = 1;
        } else if (xinput < 0) {
            directionOfTravel = -1;
        } else {
            directionOfTravel = 0;
        }
        bool isWalledBottom = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfTravel,0), 0.02f, new Vector2(directionOfTravel,0)) &&
        !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfTravel,0.5f*bcsize.y), 0.02f, new Vector2(directionOfTravel,0)) ||
        CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfTravel,bcsize.y), 0.02f, new Vector2(directionOfTravel,0)));
        if (isWalledBottom && directionOfTravel != 0) {
            float height;
            bool steppable = false;
            for (height = 0; height < 0.26f; height += 0.02f) {
                if (!CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfTravel,height), 0.02f, new Vector2(directionOfTravel,0))) {
                    steppable = true;
                    height += 0.02f;
                    break;
                }
            }
            if (steppable && !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x,bcsize.y), height, Vector2.up) ||
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0f,bcsize.y), height, Vector2.up) ||
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x,bcsize.y), height, Vector2.up))) {
                rb.position = new Vector2(rb.position.x, rb.position.y + height);
                slopeBuffer = 2;
                if (xframesmod > 2) {
                    xframesmod = 2;
                }
            }
        }
        
        prevxinput = xinput;
        

        //falling

        //target yaccel downards changes based on short/tall jumping
        if (freezePosition) {
            yvel = 0;
        } else if (extremeGravity) {
            yvel = yvel - (yaccel * 2.2f * Time.timeScale);
        } else if (sdDisable || ziplining || spikeInvincible || spinAttacking || swimming || (dolphinJumping && dolphinJumpFrames > 14) || reducedGravity) {
            //wooo
        }  else if (reducedGravity2 == 1) {
            yvel = yvel - (yaccel * 0.5f * Time.timeScale);
        } else if (reducedGravity2 == 2) {
            yvel = yvel - (yaccel * 0.25f * Time.timeScale);
        } else if (gravityCancel) {
            yvel = yvel - (yaccel * 1.25f * Time.timeScale);
        }/* else if (extendDashInvincibilityStamp > 54){
            yvel = yvel - (yaccel * 10.0f);
        }*/ else if (slamming && slamState == 1) {
            yvel = yvel - (yaccel * 1f * Time.timeScale);
        } else if (slamming && slamState == 2) {
            
        } else if (slamming && slamState == 3) {
            yvel = yvel - (yaccel * 2.4f * Time.timeScale);
        } else if (rb.velocity.y > 20 && (!Input.GetKey(StaticDataCW.Keys["Jump"]) && inJump)){
            yvel = yvel - (yaccel * 5.0f * Time.timeScale);
        } else if (!inputDisable && !knockbackDisable && (rb.velocity.y > 0.1f && (!Input.GetKey(StaticDataCW.Keys["Jump"]) && inJump) && StaticDataCW.Time > jumpTimerHeight + 0.07f && !spinAttacking)){
            //yvel = yvel - (yaccel * 5.0f * Time.timeScale);
            yvel = yvel*0.05f;
            //accels faster down for shorter jump, default gravity
        } else if (rb.velocity.y > 0 && inJump){
            yvel = yvel - (yaccel * 1.13f * Time.timeScale);
            //higher gravity up for higher jump
        } else if (inJump) {
            yvel = yvel - (yaccel * Time.timeScale);
            //less gravity for higher jump
        }else {
            yvel = yvel - (yaccel * 1.25f * Time.timeScale);
        }

        bool onSemisolid = CheckForPlatform((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.03f, -Vector2.up) ||
                CheckForPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.03f, -Vector2.up) ||
                CheckForPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.03f, -Vector2.up);
        if (onSemisolid && downPressed > 0 && semisolidGround != null) {
            semisolidGround.GetComponent<PlatformEffector2D>().colliderMask &= ~(1 << 11);
            downPressed = 0;
            /*if (((isWalledLeft && xinput == -1) || (isWalledRight && xinput == 1))) {
                disablePlatforms = 6;
            } else {*/
                disablePlatforms = 6;
                ignoreSlopeBufferFrames = 6;
            //}
            
        }
        if (downPressed == 9) {
            downPressed = 0;
        }

        if (onGround && rb.velocity.y <= 1E-03 && disablePlatforms == 0){
            //makes it so you dont accel downwards when grounded
            yvel = 0;
        }

        //wall sliding wallsliding wallslide wallslider slide
        if ((/*(assistWallSlide >= 0 && (wallJumping && ((rb.velocity.x > 0 || isWalledRight) || (rb.velocity.x < 0 || isWalledRight)))) || assistWallSlide > 0 || */((isWalledLeft && xinput == -1) || (isWalledRight && xinput == 1))|| disablePlatformsBuffer > 0) && !knockbackDisable && !slowmo && !slowDashing && (!onGround || disablePlatformsBuffer > 0) && !slamming /*&& !dolphinJumping*/ && !swimming) {
            /*if (framesSinceWallJump > 1 && ((isWalledLeft && !Input.GetKey(StaticDataCW.Keys["Left"])) || (isWalledRight && !Input.GetKey(StaticDataCW.Keys["Right"])))) {
                if (assistWallSlide == 0) {
                    assistWallSlide = 1;
                    if (isWalledLeft) {
                        xinput = -1;
                    } else {
                        xinput = 1;
                    }
                } else if (assistWallSlide == 1 && ((isWalledLeft && !Input.GetKey(StaticDataCW.Keys["Right"])) || (isWalledRight && !Input.GetKey(StaticDataCW.Keys["Left"])))) {
                    assistWallSlide = 2;
                } else if (assistWallSlide == 2 && ((isWalledLeft && Input.GetKey(StaticDataCW.Keys["Right"])) || (isWalledRight && Input.GetKey(StaticDataCW.Keys["Left"])))) {
                    assistWallSlide = -1;
                    lockedWallSliding = false;
                }
            }
            if (assistWallSlide == 0) {  
                lockedWallSliding = true;
            }*/
            if (wallSliding == false || (prevyvel > 0 && yvel <= 0)) {
                slideStamp = StaticDataCW.Time;
            }
            /*
            if (yvel > 0 && StaticDataCW.Time > wallJumpForceApplicationTimestamp + 0.15f && StaticDataCW.Time < wallJumpForceApplicationTimestamp + 0.3f && ((isWalledLeft && xinput == -1) || (isWalledRight && xinput == 1))) {
                yvel = 0;
                slideStamp = StaticDataCW.Time;
            }*/
            if (isWalledLeft) {
                wallDir = -1;
            } else {
                wallDir = 1;
            }
            if (yvel <= 0f) {
                inJump = false;
            }
            wallSliding = true;
            wallSlidingAnims = true;
            if (StaticDataCW.Time < slideStamp + 0.65f) {
                ycap = -0.8f;
            } else {
                ycap = -1.75f;
            }

        } else {
            if (!cancelSlide && prevWallSlidingAnims && ((isWalledLeft && Input.GetKey(StaticDataCW.Keys["Right"])) || (isWalledRight && Input.GetKey(StaticDataCW.Keys["Left"]))) && (!knockbackDisable && !slowmo && !slowDashing && !onGround && disablePlatforms == 0/* && !dolphinJumping*/ && !swimming)) {
                cancelSlideStamp = StaticDataCW.Time;
                cancelSlide = true;
            }
            /*
            if (lockedWallSliding) {
                if (((isWalledLeft && Input.GetKey(StaticDataCW.Keys["Right"])) || (isWalledRight && Input.GetKey(StaticDataCW.Keys["Left"])))) {
                    lockedWallSliding = false;
                    wallSlidingAnims = false;
                    wallSliding = false;
                } else {
                    if (wallSliding == false || (prevyvel > 0 && yvel <= 0)) {
                        slideStamp = StaticDataCW.Time;
                    }
                    if (isWalledLeft) {
                        wallDir = -1;
                    } else {
                        wallDir = 1;
                    }
                    if (yvel <= 0f) {
                        inJump = false;
                    }
                    wallSliding = true;
                    wallSlidingAnims = true;
                    if (StaticDataCW.Time < slideStamp + 0.65f) {
                        ycap = -0.8f;
                    } else {
                        ycap = -1.75f;
                    }
                }

            } else {*/
                wallSlidingAnims = false;
                wallSliding = false;
                if (!cancelSlide || knockbackDisable || slowmo || slowDashing || onGround) {
                    if (!slamming) {
                        ycap = -12.5f;
                    }
                    wallSliding = false;
                    wallDir = 0;
                } else {
                    if (wallSliding == false || (prevyvel > 0 && yvel <= 0)) {
                        slideStamp = StaticDataCW.Time;
                    }
                    wallSliding = true;
                    ycap = -12.5f;
                    if (isWalledLeft) {
                        wallDir = -1;
                    } else if (isWalledRight) {
                        wallDir = 1;
                    }
                }
            //}
        }
        if (!wallSlidingAnims || onGround || (!isWalledRight && !isWalledLeft)) {
            wallSlidingAnims = false;
            wallSliding = false;
            //lockedWallSliding = false;
            //assistWallSlide = 0;
        }
        if (gliderObtained) {
            if (Input.GetKey(StaticDataCW.Keys["Jump"]) && !wallSliding && !ziplining && !onGround && !slamming && !attackLock && !spinAttacking && (!dolphinJumping || anim.GetCurrentAnimatorStateInfo(0).IsName("Curl") || anim.GetCurrentAnimatorStateInfo(0).IsName("GlideFall"))) {
                gliderActivated = true;
                if (dolphinJumping) {
                    dolphinJumping = false;
                    inputDisable = false;
                }
                ycap = -1.5f;
                accelerationControl = true;
                accelerationMagnitude = 0.4f;
                decelerationMagnitude = 0.2f;
            } else {
                gliderActivated = false;
                accelerationControl = false;
                ventGliding = false;
                if (!wallSliding && !slamming) {
                    ycap = -12.5f;
                }
            }
        }
        if (!gliderActivated) {
            ventCancelTimer = -9.0f;
        }
        /*if (yvel < 0) {
            ventGliding = false;
        }*/

        if (!bubbleWarping) {
            if (canslowDash) {
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = Mathf.Lerp(roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping, 0f, 0.2f);
            } else if (prevcanslowDash) {
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f;
            } else if (yvel < -6f) {
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f - 1.2f*(Mathf.Min(20f, Mathf.Abs(yvel)))/20f;
            } else if (prevyvel < -6f) {
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.4f;
            }
        }
        if (!touchingQuadrant) {
            quadrantCounter = Vector4.zero;
            quadrantParent = null;
        }

        if (slamming && slamState == 3) {
            SlammableCast((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 1.2f, Vector2.down);
            SlammableCast((Vector2)groundPoint.transform.position, 1.2f, Vector2.down);
            SlammableCast((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 1.2f, Vector2.down);
        }
        
        if (slamming && (onGround || slowmo || slowDashing) || (slamState == 1 && onGroundFar)) {
            slamming = false;
            invincible = false;
            accelerationControl = false;
            revertSlamVelocity = true;
            inputDisable = false;
            if (onGround && slamState == 3) {
                GameObject swingInstance = Instantiate(staffSwing, transform.position, Quaternion.identity) as GameObject;
                swingInstance.transform.parent = transform;
                AttackCW slamatk = swingInstance.GetComponent<AttackCW>();
                slamatk.crouching = false;
                slamatk.dir = 2;
                slamatk.type = 3;
                slamatk.disabletime = 0.05f;
                slamatk.deathtime = 0.05f;
                slamatk.dmgValue = 1;
                GameObject.Instantiate(slamShockwave, transform.position + new Vector3(-0.03f, -0.4f, 0), Quaternion.identity);
            }
            slamState = 0;
        }
        
        if (slamming && slamState == 1 && yvel <= 0f) {
            slamState = 2;
            slamStallStamp = StaticDataCW.Time;
            yvel = 0f;
        } if (slamming && slamState == 2 && StaticDataCW.Time > slamStallStamp + 0.09f) {
            accelerationControl = true;
            slamState = 3;
            //accelerationValue = 14;
            accelerationMagnitude = 0.03f;
            decelerationMagnitude = 0.3f;
            inputDisable = false;
            Yvel(-15f, true);
            /*
            GameObject swingInstance = Instantiate(staffSwing, transform.position, Quaternion.identity) as GameObject;
            swingInstance.transform.parent = transform;
            AttackCW slamWindBox = swingInstance.GetComponent<AttackCW>();
            slamWindBox.crouching = false;
            slamWindBox.dir = -2;
            slamWindBox.type = 3;
            slamWindBox.disabletime = 0f;
            slamWindBox.deathtime = 0f;
            slamWindBox.dmgValue = 0;
            */
        }
        /*if (ziplining) {
            bool checkInGround = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0, -0.2f*bcsize.x), 1.2f, Vector2.up) ||
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, -0.2f*bcsize.x), 1.2f, Vector2.up) ||
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, -0.2f*bcsize.x), 1.2f, Vector2.up) ||
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.65f*bcsize.x, 0), 0.8f, Vector2.right) || 
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.65f*bcsize.x, bcsize.y), 0.8f, Vector2.right) || 
            CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.65f*bcsize.x, 0.5f*bcsize.y), 0.8f, Vector2.right);
            if (inGround == 0 && checkInGround) {
                inGround = 1;
            } else if (inGround == 1 && !checkInGround) {
                inGround = 2;
            } else if (inGround == 2 && checkInGround) {
                inGround = 0;
            }
        }*/
        if (slamInputBool && !slamming && !wallSliding && !onGround && !slowmo && !slowDashing && (!inputDisable || ziplining) && !waterBlockChecker2.GetComponent<FrostGuardDetectorCW>().waterBlockInside) {
            slamInputBool = false;
            if (!ziplining || !inGround) {
                slamming = true;
                wallSlidingAnims = false;
                cancelSlide = false;
                GetComponent<BoxCollider2D>().isTrigger = false;
                ziplining = false;
                ycap = -30f;
                Yvel(9f, true);
                Xvel(0f, true);
                xinput = 0;
                slamState = 1;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 1f;
                revertSlamVelocity = false;
                dolphinJumping = false;
                invincible = true;
                invincibilityTimer = StaticDataCW.Time - 0.5f;
                noInvincibleBlink = true;
                logSlamHeight = rb.position.y;
            }
        }
        if (ycap > -12.7f && revertSlamVelocity) {
            revertSlamVelocity = false;
            if (ycap < -12.5f) {
                ycap = -12.5f;
            }
        }
        if (revertSlamVelocity) {
            ycap = Mathf.Lerp(ycap, -12.5f, 0.75f);
        }
        if (reducedGravity2 == 1) {
            ycap = -5f;
        } else if (reducedGravity2 == 2) {
            ycap = -2.6f;
        }



        if (cancelSlide && (StaticDataCW.Time > cancelSlideStamp + 0.18f || framesSinceWallJump == 1)) {
            cancelSlide = false;
            ignorePrevWallSliding = true;
        }

        if (!onGround && yvel <= ycap){
            //makes sure it caps off at ycap
            yvel = ycap;
        }
        if (onGround) {
            inJump = false;
        }

        if (chargingSpin && StaticDataCW.Time > spinChargeParticleStamp + 0.133f) {
            if (StaticDataCW.Time < spinChargeStamp + 0.7f) {
                GameObject spinParticle = Instantiate(spinChargeParticle2);
                spinParticle.transform.SetParent(transform);
                spinParticle.transform.localPosition = Vector3.zero;
                spinChargeParticleStamp = StaticDataCW.Time - 0.1f;
            } else {
                Instantiate(spinChargeParticle).transform.position = transform.position;
                spinChargeParticleStamp = StaticDataCW.Time;
            }
        }
        if (spinAttacking && spinAttackType == 1) {
            if (CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.0f, 0), 0.02f, -Vector2.up) &&
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x, 0), 0.02f, -Vector2.up) &&
                CheckGrounded((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x, 0), 0.02f, -Vector2.up)) {     
                Xvel(spinAttackSpeed*(sr.flipX?-1:1), true);
                spinAttackSpeed = Mathf.Lerp(spinAttackSpeed, 0f, 0.08f);
            } else {
                Xvel(0f, true);
                Yvel(0f, true);
            }
        }
        if (spinAttacking && spinAttackType == 2) {
            Yvel(Mathf.Lerp(yvel, 0f, 0.4f), true);
        }

        if (spinAttacking && StaticDataCW.Time > spinAttackTimer) {
            if (spinAttackStage == 5) {
                spinAttackStage = 0;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                disableAllActions = false;
            } else {
                if (spinAttackType == 1) {
                    GameObject swingInstance = Instantiate(staffSwing, transform.position, Quaternion.identity) as GameObject;
                    swingInstance.transform.parent = transform;
                    AttackCW spinatk = swingInstance.GetComponent<AttackCW>();
                    spinatk.crouching = false;
                    spinatk.type = 4;
                    spinatk.disabletime = 0.05f;
                    spinatk.deathtime = 0.1f;
                    spinatk.dmgValue = 1;
                    spinatk.spinAttackType = spinAttackType;
                    spinatk.spinAttackStage = spinAttackStage;
                    if (spinAttackStage % 2 == 0) {
                        spinatk.dir = sr.flipX? 1:-1;
                    } else {
                        spinatk.dir = sr.flipX? -1:1;
                    }
                } else {
                    GameObject swingInstance = Instantiate(spinAttackAir, transform.position, Quaternion.identity) as GameObject;
                    swingInstance.transform.parent = transform;
                    AttackCW spinatk = swingInstance.GetComponent<AttackCW>();
                    spinatk.crouching = false;
                    spinatk.type = 4;
                    spinatk.disabletime = 0.05f;
                    spinatk.deathtime = 0.1f;
                    spinatk.dmgValue = 1;
                    spinatk.spinAttackType = spinAttackType;
                    spinatk.spinAttackStage = spinAttackStage;
                    spinatk.dir = sr.flipX? 1:-1;
                }
                spinAttackStage++;
                spinAttackTimer = StaticDataCW.Time + 0.2f;
                SceneManagerCW.PlaySound("Slash1", 1);
            }
        }

        
            
        if (jumping && (canJump || coyoteFrames > 0 || isWalledLeft || isWalledRight || wallSliding) && !knockbackDisable && !ziplining && !slamming && !swimming){
            //if you can jump, and we get an input from update, then jump
            jumping = false;
            inJump = true;
            SceneManagerCW.PlaySound("Jump", 1);
            jumpTimerHeight = StaticDataCW.Time;
            if (!canJump && coyoteFrames == 0) {
                //walljump wall jump
                if (wallSliding) {
                    wallSliding = false;
                    if (wallDir == -1) {
                        Xvel(10f, true);
                        wallJumpApplyForce = 1;
                        sr.flipX = false;
                        xinput = 1;
                    } else {
                        Xvel(-10f, true);
                        sr.flipX = true;
                        xinput = -1;
                        wallJumpApplyForce = -1;
                    }
                } else {
                    if (isWalledLeft) {
                        Xvel(10f, true);
                        wallJumpApplyForce = 1;
                    } else if (isWalledRight) {
                        Xvel(-10f, true);
                        wallJumpApplyForce = -1;
                    }
                }
                yvel = jumpForce * 0.75f;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 0.12f;
                wallJumpForceApplicationTimestamp = StaticDataCW.Time;
                framesSinceWallJump = 0;
                WallJumpXvel = xvelset;
                wallJumping = true;
            } else {
                yvel = jumpForce;
                coyoteFrames = 0;
                airDash = true;
            }
        } else if (jumping && StaticDataCW.Time < timeEndedZip + 0.1f && zipJumpable && !knockbackDisable && !ziplining && !gliderObtained) {
            yvel += 15f;
            if (xvelmove > 1f) {
                Xvel(10f, false);
            } else if (xvelmove < -1f) {
                Xvel(-10f, false);
            }
            zipJumpable = false;
        }

        if (StaticDataCW.Time < wallJumpForceApplicationTimestamp + 0.1f) {
            if (wallJumpApplyForce > 0f) {
                wallJumpApplyForce = Mathf.Lerp(wallJumpApplyForce, 0.8f, 0.3f);
            } else {
                wallJumpApplyForce = Mathf.Lerp(wallJumpApplyForce, -0.8f, 0.3f);
            }
            Xvel(10f*wallJumpApplyForce, true);
        } else if (StaticDataCW.Time < wallJumpForceApplicationTimestamp + 0.33f) {
            wallJumpApplyForce = Mathf.Lerp(wallJumpApplyForce, 0f, 0.1f);
            Xvel(2.5f*wallJumpApplyForce, false);
        }

        yvelsquish = yvel;

        if (slowDashing){
            yvel = 0;
        }

        if (atkfloat > 0 && atkfloat < 5) {
            atkfloat++;
            if (!wallSliding) {
                float atkfloatvel = Mathf.Lerp(yvel, 2, 0.9f);
                Yvel(atkfloatvel, true);
            }
        }

        if (canyvelset == true){
            yvel = yvelset;
            canyvelset = false;
        }

        if (canyveladd == true){
            yvel += yveladd;
            canyveladd = false;
        }

        //applies yvel
        rb.velocity = new Vector2(rb.velocity.x, yvel);

        spriteobj.transform.localScale = Vector3.Lerp(spriteobj.transform.localScale, new Vector3(1, 1, 1), 0.2f);

        //bouncy on jump, using yvel makes you stretch out when going up, looked weird going down
        squish = (ycap - (-1.0f * yvelsquish))/ycap;

        /*if (!sdDisable && !(Input.GetKey(StaticDataCW.Keys["Down"]) && (xinput == 0) && onGround) && !knockbackDisable){
            if (squish < 0.85f){
                spriteobj.transform.localScale = new Vector3(0.85f,spriteobj.transform.localScale.y,1);
            } else if (squish <= 0.9f){
                spriteobj.transform.localScale = new Vector3(squish,spriteobj.transform.localScale.y,1);
            } else if (squish >= 1.9f) {
                spriteobj.transform.localScale = new Vector3(((ycap - (yvelsquish))/ycap)+0.9f,spriteobj.transform.localScale.y,1);
            }
        }*/

        //end knockbackDisable timer
        if (knockbackDisable && hitDeath && StaticDataCW.Time > knockbackDisableTimer + 0.05f) {
            knockbackDisable = false;
        } else if (knockbackDisable && !hitDeath && StaticDataCW.Time > knockbackDisableTimer + 0.4f) {
            knockbackDisable = false;
        } else if (knockbackDisable && StaticDataCW.Time < knockbackDisableTimer + 0.4f){

        }
        if (inputDisable && StaticDataCW.Time > inputDisableTimer) {
            inputDisable = false;
            if (wallJumping){
                wallJumping = false;
            }
        }
        if (invincible && StaticDataCW.Time > invincibilityTimer + 0.7f) {
            invincible = false;
        }
        if (!invincible) {
            noInvincibleBlink = false;
        }
        //Get hit if in spike
        if (touchingDeath && !spikeInvincible && !ziplining) {
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            hitDeath = true;
            //health -= 1f;
            Hit(0, null);
            Xvel(0,true);
            Yvel(0,true);
        }
        if (touchingConduit && (!invincible && !dashinvincible && extendDashInvincibilityStamp < 50 && !spikeInvincible)) {
            if (StaticDataCW.Time > conduitHitTimestamp + 0.9f) {
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                health -= 1f;
                Hit(3, null);
                conduitHitTimestamp = StaticDataCW.Time;
            }
        }
        if (spikeInvincible) {
            Xvel(0,true);
            Yvel(0,true);
        }
        if (GameObject.FindWithTag("Canvas") != null && GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading && StaticDataCW.Time > spikeFadeStamp + 0.35f && !upDooring) {
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading = false;
            transform.position = spikePoint;
            spikeInvincible = false;
            SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = false;
            spikeResetting = 2;
            resetting = 2;
            sr.enabled = true;
            Reset();
            if (deathFadePlayer != null) {
                Destroy(deathFadePlayer);
            }
            if (blackBars != null && blackBars.Length > 0 && blackBars[0] != null) {
                foreach (GameObject obj in blackBars) {
                    Destroy(obj);
                }
            }
        } else if (GameObject.FindWithTag("Canvas") != null && GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading && !upDooring) {
            deathFadePlayer.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position);
        }
        //turn transparent on knockback
        if (!spikeInvincible && !noInvincibleBlink && (invincible && (StaticDataCW.Time - invincibilityTimer) % 0.25f > 0.125f)) {
            sr.color = new Color(0f,0f,0f,0.3f);
        } /*else if (dashMeter < 0.98f) {
            sr.color = new Color(1f,0.6f+(dashMeter*0.4f),1f,1f);
        }*/ else {
            sr.color = Color.white;
        }
        if (inCannon) {
            sr.enabled = false;
            if (cannonRef != null) {
                cannonRef.transform.rotation = Quaternion.Euler(0f, 0f, -VecToDeg(slowDir));
                rb.position = cannonRef.transform.position;
            }
            if (!slowDashing && !airDash && !prevslowmo) {
                airDash = true;
                slowmo = true;
                slowdownStartFrames = 0;
            }
        }
        //on the frame you start dashing, cancelling out of the slowdown, this happens
        if (prevslowmo && !slowmo && slowDir != new Vector2(0,0) && !knockbackDisable && !dashCancel && !deathFading){
            //hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Hook") as Material;
            dashFrameCount = 0;
            //SceneManagerCW.PlaySound("Speed Up", 1);
            dashHitExtend = false;
            sdDisableTimer = StaticDataCW.Time;
            slowDashing = true;
            dashTrail.Play();
            dashinvincible = true;
            extendZipMomentum = false;
            //starts timer
            if (!onTarget) {
                dashInvincibilityTimer = StaticDataCW.Time + 0.05f;
                if (Time.timeScale < 0.05) {
                    dashSoundReference = SceneManagerCW.PlaySound("Dashing", 0.8f);
                } else {
                    dashSoundReference = null;
                    SceneManagerCW.PlaySound("Pop", 1.0f);
                }
            } else {
                dashSoundReference = SceneManagerCW.PlaySound("Dashing", 0.8f);
            }
            inJump = false;
            if (inCannon) {
                superdashing = true;
                sr.enabled = true;
                if (cannonRef != null && cannonRef.GetComponentsInChildren<Animator>()[0] != null) {
                    cannonRef.GetComponentsInChildren<Animator>()[0].SetBool("Charging", false);
                    cannonRef.GetComponent<CannonCW>().resetDir = true;
                    cannonRef.GetComponent<CannonCW>().resetStamp = StaticDataCW.Time;
                }
                cannonLaunchStamp = StaticDataCW.Time;
                inCannon = false;
            }
            //if you are targeting when you let go, then you are invincible
        } else if (prevslowmo && !slowmo) {
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            slowDir = new Vector2(0,0);
            canslowDash = false;
            sdDisable = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            SceneManagerCW.PlaySound("Pop", 1);
        }


        if (slowDashing) {
            gliderActivated = false;
        }

        //canslowdash means its happening when ur in slowmode, aiming mode
        if (canslowDash){
            dashFrameCount = 0;
            sdDisableTimer = StaticDataCW.Time;
            slowDir = DashDir(slowDir);
            //dashdir finds the direction you are aiming your dash and sets the direction to it with inputs

            Vector2 dashDir = Vector2.zero;
            Vector2 hitPoint = Vector2.zero;
            float dashDist = 0f;
            bool found = false;
            GameObject hitobj = null;
            
            int foundCounter = 0;
            for (int i = 0; i < 6; i++){
                found = TargetRay(slowDir, i, 1.8f, ref dashDir, ref hitPoint, ref dashDist, ref hitobj, true);
                if (found){foundCounter++;}
                found = TargetRay(slowDir, -i, 1.8f, ref dashDir, ref hitPoint, ref dashDist, ref hitobj, true);
                if (found){foundCounter++;}
            }
                
            GameObject prevobj = targetSelected;
            if (!inCannon) {
                onTarget = foundCounter > 1;
            } else {
                Xvel(0f, true);
                Yvel(0f, true);
            }
            if (onTarget){
                slowDir = dashDir;
                keepSlowDir = slowDir;
                dashHitPoint = hitPoint;
                dashDistance = dashDist * 1.1f;
                targetSelected = hitobj;
                hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", true);
            } else{
                targetSelected = null;
                hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            }
            if (slowDir.x > 0){
                sr.flipX = false;
            } else{
                sr.flipX = true;
            }

            if (targetSelected != prevobj) {
                hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            }

            float angle = 0;
            if (onTarget){
                Vector2 aimdir = dashHitPoint - (Vector2)transform.position;
                aimdir.Normalize();
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
            } else{
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
            }
            angle = angle * (180 / Mathf.PI) - 90;
            Quaternion targetdir = Quaternion.Euler(0, 0, angle);
            hookObject.transform.rotation = targetdir;
            float particleAngle = 90 - angle;
            if (particleAngle > 180) {
                particleAngle -= 360;
            }
            Quaternion particletargetdir = Quaternion.Euler(particleAngle, 90, 0);
            dashTrail.gameObject.transform.rotation = particletargetdir;
            var main = dashTrail.main;
            main.startRotation = particleAngle * Mathf.Deg2Rad;

            Vector2 hookTransform;

            hookObject.transform.position = (Vector2)gameObject.transform.position;

            if (onTarget){
                hookTransform = dashHitPoint;//Vector2.Lerp(hookObject.transform.GetChild(0).gameObject.transform.position, dashHitPoint, 0.5f);
            } else {
                hookTransform = (Vector2)gameObject.transform.position + (slowDir * 4);//Vector2.Lerp(hookObject.transform.GetChild(0).gameObject.transform.position, (Vector2)gameObject.transform.position + (slowDir * 4), 0.5f);
                hookPoint = (Vector2)gameObject.transform.position + (slowDir * 4);
            }
            hookObject.transform.GetChild(0).gameObject.transform.position = hookTransform;
        //if ur not aiming anymore, and ur dashing rn, then go
        } else if (slowDashing && !onTarget){
            if (freeze) {
                Xvel(slowDir.x * 12f,true);
                Yvel(slowDir.y * 12f,true);
            } else {
                Xvel(slowDir.x * 25,true);
                Yvel(slowDir.y * 25,true);
            }
            
            dashFrameCount++;
            hookObject.transform.localPosition = (Vector2)gameObject.transform.position + (slowDir);
            hookObject.transform.GetChild(0).gameObject.transform.position = hookPoint;
            dashInvincibilityTimer = StaticDataCW.Time + 0.06f;
            if (dashHitboxObj == null) {
                dashHitboxObj = Instantiate(dashHitbox, transform.position, Quaternion.identity) as GameObject;
                dashHitboxObj.transform.parent = transform;
                AttackCW dashHit = dashHitboxObj.GetComponent<AttackCW>();
                dashHit.type = 2;
                dashHit.dmgValue = 1;
                dashHit.deathtime = 100f;
                float tempDir = Mathf.Atan2(slowDir.x, slowDir.y) % (2*Mathf.PI);
                if (tempDir < Mathf.PI/4 || tempDir > 7*Mathf.PI/4) {
                    dashHit.dir = 1;
                } else if (tempDir > Mathf.PI/4 || tempDir < 3*Mathf.PI/4) {
                    dashHit.dir = 2;
                } else if (tempDir > 3*Mathf.PI/4 || tempDir < 5*Mathf.PI/4) {
                    dashHit.dir = -1;
                } else if (tempDir > 5*Mathf.PI/4 || tempDir < 7*Mathf.PI/4) {
                    dashHit.dir = -2;
                }
            }
        //if ur not aiming anymore, and ur dashing in target mode, go faster and calculate distance
        } else if (slowDashing && onTarget){
            hookObject.transform.localPosition = (Vector2)gameObject.transform.position + (slowDir);
            if (specialTarget) {
                if (GameObject.Find("Cyprus3") != null) {
                    GameObject.Find("Cyprus3").GetComponent<Cyprus3CW>().playerTargeted = true;
                }
                slowDir = (Vector2) (targetSelected.transform.position - transform.position).normalized;
                float angle = 0;
                dashDistance = (targetSelected.transform.position - transform.position).magnitude;
                dashTime += 1f;
                Vector2 aimdir = (Vector2) targetSelected.transform.position - (Vector2)transform.position;
                aimdir.Normalize();
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
                angle = angle * (180 / Mathf.PI) - 90;
                Quaternion targetdir = Quaternion.Euler(0, 0, angle);
                hookObject.transform.rotation = targetdir;
                float particleAngle = 90 - angle;
                if (particleAngle > 180) {
                    particleAngle -= 360;
                }
                Quaternion particletargetdir = Quaternion.Euler(particleAngle, 90, 0);
                dashTrail.gameObject.transform.rotation = particletargetdir;
                var main = dashTrail.main;
                main.startRotation = particleAngle * Mathf.Deg2Rad;

                Vector2 hookTransform;

                hookObject.transform.position = (Vector2)gameObject.transform.position;

                hookTransform = targetSelected.transform.position;
                hookObject.transform.GetChild(0).gameObject.transform.position = hookTransform;
            } else {
                dashTime = dashDistance / 23;
                hookObject.transform.GetChild(0).gameObject.transform.position = dashHitPoint;
            }
            dashInvincibilityTimer = StaticDataCW.Time + dashDistance/17;
            if (freeze) {
                Xvel(slowDir.x * 11f,true);
                Yvel(slowDir.y * 11f,true);
            } else {
                Xvel(slowDir.x * 23,true);
                Yvel(slowDir.y * 23,true);
            }
            if (dashHitboxObj == null) {
                dashHitboxObj = Instantiate(dashHitbox, transform.position, Quaternion.identity) as GameObject;
                dashHitboxObj.transform.parent = transform;
                AttackCW dashHit = dashHitboxObj.GetComponent<AttackCW>();
                dashHit.type = 2;
                dashHit.dmgValue = 1;
                dashHit.deathtime = 100f;
                float tempDir = Mathf.Atan2(slowDir.x, slowDir.y) % (2*Mathf.PI);
                if (tempDir < Mathf.PI/4 || tempDir > 7*Mathf.PI/4) {
                    dashHit.dir = 1;
                } else if (tempDir > Mathf.PI/4 || tempDir < 3*Mathf.PI/4) {
                    dashHit.dir = 2;
                } else if (tempDir > 3*Mathf.PI/4 || tempDir < 5*Mathf.PI/4) {
                    dashHit.dir = -1;
                } else if (tempDir > 5*Mathf.PI/4 || tempDir < 7*Mathf.PI/4) {
                    dashHit.dir = -2;
                }
            }
            
        }


        //timer for normal dash
        if (sdDisable && ((!superdashing && StaticDataCW.Time > sdDisableTimer + 0.1f) || (superdashing && StaticDataCW.Time > sdDisableTimer + 10f)) && !onTarget && dashFrameCount >= 1) {
            sdDisable = false;
            superdashing = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            ParticleStopTimer(dashTrail, 0.1f);
            framesSinceEndedNormalDash = 0;
            dashHitTimer = StaticDataCW.Time + 0.15f;
            dashHitExtend = true;
            if (dashSoundReference != null){
                SceneManagerCW.PlaySound("Pop", 1);
            }
            slowDir = new Vector2(0,0);
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            Yvel(yvel * 0.5f,true);
            dashFrameCount = 0;
            stopNormalDashMomentumStamp = StaticDataCW.Time;
            dashTurn = false;
            if (!onGround) {
                extendNormalDashMomentum = true;
                spongeExtending = false;
            }
            //sr.flipX = (saveFacingDir == -1)? true:false;
        
        //timer for targeted dash, turning off target mode and invincibility
        } else if (sdDisable && StaticDataCW.Time > sdDisableTimer + dashTime && onTarget){
            sdDisable = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            dashTrail.Stop();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            slowDir = new Vector2(0,0);
            SceneManagerCW.PlaySound("Pop", 1);
            onTarget = false;
            hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            saveTargetDash = true;
            keepTargetCheckStamp = StaticDataCW.Time;
            dashTurn = false;
            if (xvelmove > 17.5f) {
                float modifier = 17.5f/xvelmove;
                Xvel(17.5f,true);
                Yvel(yvel * modifier, true);
            }
            if (xvelmove < -17.5f) {
                float modifier = -17.5f/xvelmove;
                Xvel(-17.5f,true);
                Yvel(yvel * modifier, true);
            }
            if (yvel > 17.5f) {
                float modifier = 17.5f/yvel;
                Yvel(17.5f,true);
                Xvel(xvelmove * modifier, true);
            }
            if (yvel < -17.5f) {
                float modifier = -17.5f/yvel;
                Yvel(-17.5f,true);
                Xvel(xvelmove * modifier, true);
            }
            stopNormalDashMomentumStamp = StaticDataCW.Time;
            if (!onGround) {
                extendNormalDashMomentum = true;
                spongeExtending = false;
            }
            //sr.flipX = (saveFacingDir == -1)? true:false;
        }

        if (slowmo && slowdownStartFrames > 130 && dm.currentTooltipText == "") {
            dm.QueueToolTip(ReplaceWithKeybind("To cancel your dash after you've held %Dash%, drag to the center of the player."), true);
        }
        if (!slowmo && dm.currentTooltipText.Length >= 38 && dm.currentTooltipText.Substring(0, 38) == "To cancel your dash after you've held ") {
            dm.QueueToolTip("", false);
        }
        slowdownStartFrames++;

        //check for superdash
        if (superdashing && sdDisable && dashFrameCount >= 1 && ((onGround && rb.velocity.y <= 0f) || (isWalledRight && rb.velocity.x >= 0f) || (isWalledLeft && rb.velocity.x <= 0f) || (ceiled && rb.velocity.y >= 0f)) && !touchingSponge) {
            sdDisable = false;
            superdashing = false;
            superdashEndStamp = StaticDataCW.Time;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            ParticleStopTimer(dashTrail, 0.05f);
            framesSinceEndedNormalDash = 0;
            dashHitTimer = StaticDataCW.Time + 0.15f;
            dashHitExtend = true;
            if (dashSoundReference != null){
                SceneManagerCW.PlaySound("Pop", 1);
            }
            slowDir = new Vector2(0,0);
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            Yvel(yvel * 0.5f,true);
            dashFrameCount = 0;
            stopNormalDashMomentumStamp = StaticDataCW.Time;
            dashTurn = false;
            if (!onGround) {
                extendNormalDashMomentum = true;
                spongeExtending = false;
            }
            //sr.flipX = (saveFacingDir == -1)? true:false;
        }

        if (StaticDataCW.Time > dashInvincibilityTimer) {
            dashinvincible = false;
            
        }

        if (unclearRepeatedly > 0) {
            unclearRepeatedly--;
            roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
            roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
            
            roomBox.GetComponent<RoomConfigureCW>().Unclear();
        }

        if (clearRepeatedly > 0) {
            clearRepeatedly--;
            roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 0;
            roomBox.GetComponent<RoomConfigureCW>().isCleared = true;
            roomBox.GetComponent<RoomConfigureCW>().Clear();
        }

        if (dashHitExtend && StaticDataCW.Time > dashHitTimer) {
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            dashHitExtend = false;
        }
        
        if (!dolphinJumping && dolphinAttack != null) {
            Destroy(dolphinAttack);
        }

        if (onGround && !sdDisable) {
            airDash = true;
        }
        
        if (inCamshiftZone) {
            cameraZoneTargetPoint = transform.position + cameraZoneSavePos;
        } else {
            cameraZoneTargetPoint = transform.position;
        }
        if (inCannon) {
            cameraZoneTargetPoint += (Vector3) slowDir * 2f;
        }

        if (ziplining) {
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            Xvel(zipDir.x * 14, true);
            Yvel(zipDir.y * 14, true);
            if (zipSetPos > 0) {
                rb.position = zipPos;
                zipSetPos --;
            }
        }  
        /*
        if (StaticDataCW.Time > extendDashInvincibilityStamp + 0.1f && !extendDashCancel) {
            Yvel(2f, true);
            extendDashCancel = true;
        }
        */
        //checks if you were in slow mode 1 frame earlier
        
        prevcanslowDash = canslowDash;

        if (!canslowDash && !slowmo && !slowDashing && sdDisable) {
            sdDisable = false;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (gliderObtained && (anim.GetCurrentAnimatorStateInfo(0).IsName("TransSlide") || anim.GetCurrentAnimatorStateInfo(0).IsName("Slide"))) {
            gliderBack.GetComponent<SpriteRenderer>().enabled = true;
            gliderBack.GetComponent<SpriteRenderer>().flipX = !sr.flipX;
        } else if (gliderObtained && (anim.GetCurrentAnimatorStateInfo(0).IsName("JumpUp") || anim.GetCurrentAnimatorStateInfo(0).IsName("JumpForwards") || anim.GetCurrentAnimatorStateInfo(0).IsName("Slam Charge") || anim.GetCurrentAnimatorStateInfo(0).IsName("Curl") || (onGround && onCrumble))) {
            gliderBack.GetComponent<SpriteRenderer>().enabled = true;
            gliderBack.GetComponent<SpriteRenderer>().flipX = sr.flipX;
        } else {
            gliderBack.GetComponent<SpriteRenderer>().enabled = false;
        }
        
        if ((cameraZoneTargetPoint - transform.position).magnitude < 0.1f && (cameraPoint.transform.position - transform.position).magnitude < 0.1f) {
            cameraPoint.transform.localPosition = new Vector2(0f, 0f);
        } else {
            float x;
            float y;
            if (inCannon) {
                x = Mathf.Lerp(cameraPoint.transform.position.x, cameraZoneTargetPoint.x, 0.08f);
                y = Mathf.Lerp(cameraPoint.transform.position.y, cameraZoneTargetPoint.y, 0.08f);
            } else {
                x = Mathf.Lerp(cameraPoint.transform.position.x, cameraZoneTargetPoint.x, 0.12f);
                y = Mathf.Lerp(cameraPoint.transform.position.y, cameraZoneTargetPoint.y, 0.12f);
            }
            cameraPoint.transform.position = new Vector3(x, y, 0f);
        }

        if (roomBox != null && screenLock == null && !prevRoomBoxCleared && roomBox.GetComponent<RoomConfigureCW>().isCleared && lockRoomTransitioning == 0) {
            screenLock = Instantiate(Resources.Load("Prefabs/ScreenLock", typeof(GameObject)) as GameObject);
            screenLock.transform.parent = GameObject.FindWithTag("MainCamera").transform;
            screenLock.transform.localScale = new Vector3(1f, 1f, 1f);
            screenLock.transform.localPosition = new Vector3(0f,0f,10f);
            screenLockSprite = screenLock.GetComponent<SpriteRenderer>();
            screenLockAnim = screenLock.GetComponent<Animator>();
            screenLockSprite.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 1f);
            screenLockAnim.Play("UnlockScreen");
        }
        if (lockRoomTransitioning > 0) {
            lockRoomTransitioning--;
        }

        if (screenLock != null && (screenLockAnim.GetCurrentAnimatorStateInfo(0).IsName("LockHold"))) {
            if (screenLockSprite.color.a > 0.05f) {
                screenLockSprite.color = new Color(0.8828125f, 0.99609375f, 0.46875f, Mathf.Lerp(screenLockSprite.color.a, 0f, 0.2f));
            } else {
                Destroy(screenLock);
            }
        }
        
        if (screenLock != null && screenLockAnim.GetCurrentAnimatorStateInfo(0).IsName("Hide")) {
            Destroy(screenLock);
        }

        //Anims

        anim.SetInteger("XInput", (int)xinput);
        anim.SetBool("RunningIntoWalls", (isWalledLeft && xinput == -1) || (isWalledRight && xinput == 1));
        anim.SetInteger("YInput", yinput);
        anim.SetBool("OnGround", onGround);
        anim.SetInteger("SlopeBuffer", slopeBuffer); 
        anim.SetFloat("Yvel", (float)yvel);
        if (yvel < 0 && (prevyvel >= 0 || (prevSlamming && !slamming)) && !dolphinJumping){
            anim.SetBool("CurlTrigger", true);
        } else  if (anim.GetCurrentAnimatorStateInfo(0).IsName("Curl")) {
            anim.SetBool("CurlTrigger", false);
        }
        if (wallSlidingAnims && !prevWallSlidingAnims){
            anim.SetBool("SlideTrigger", true);
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("TransSlide")) {
            anim.SetBool("SlideTrigger", false);
        }
        anim.SetBool("SlideAttacking", wallAttacking);
        anim.SetBool("WallSliding", wallSlidingAnims);
        anim.SetBool("Crouching", crouching);
        if (!spinAttacking && !swimming && !(dolphinJumping && yvel > 0f) && (StaticDataCW.Time > attackStamp + 0.15f || facingdir != animFlipDir)){
            anim.SetBool("AttackLock", false);
        } else{
            anim.SetBool("AttackLock", true);
        }
        anim.SetInteger("AttackDir", (atkdir == -1)? 1:atkdir);
        anim.SetInteger("AttackIndex", attackAnimIndex);
        anim.SetBool("AttackChangeLock", attackAnimLock);
        if (attackAnimIndex > 2) {attackAnimIndex = 0;}
        anim.SetBool("Dashing", sdDisable);
        anim.SetBool("Slamming", slamming);
        anim.SetInteger("SlamState", slamState);
        anim.SetBool("Zipping", ziplining);
        anim.SetBool("HasGlider", gliderObtained);
        anim.SetBool("Gliding", gliderActivated);
        anim.SetBool("VentGliding", ventGliding);
        anim.SetBool("DashCancel", dashCancel);
        anim.SetBool("SpinAttacking", spinAttacking);
        anim.SetBool("SpinAttackFrame", spinAttackFrame);
        if (spinAttackFrame) {
            spinAttackFrame = false;
        }
        anim.SetBool("SpinAttackGround", spinAttackType == 1);
        anim.SetBool("Bubble", cyprusBubble);
        /*if (anim.GetCurrentAnimatorStateInfo(0).IsName("Gliding")) {
            anim.SetBool("VentGliding", true);
        } else {
            anim.SetBool("VentGliding", false);
        }*/

        
        if (!gliderObtained) {
            prevGliderActivated = false;
        }

        if (!gliderActivated && prevGliderActivated) {
            anim.SetBool("GliderTrans", true);
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GlideTrans") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
            anim.SetBool("GliderTrans", false);
        }

        
        prevGliderActivated = gliderActivated;

        if (slowmo || (sdDisable && !slowmo)){
            spriteobj.transform.localPosition = new Vector3 (0, 0, 0);
            float angle = 0;
            if (onTarget){
                Vector2 aimdir = dashHitPoint - (Vector2)transform.position;
                aimdir.Normalize();
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
            } else{
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
            }
            angle = angle * (180 / Mathf.PI) - 90;
            if (dashCancel) {
                smoothTargetAngle = true;
                angle = 0;
                hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            } else {
                if (prevDashCancel/* || !prevslowmo*/) {
                    smoothTargetAngle = true;
                    smoothWaitFrames = 0;
                }
                if (!hideHook) {
                    hookObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                }
            }
            //angle = Mathf.Lerp(spriteobj.transform.rotation.z, angle, 0.3f);
            Quaternion saverot = spriteobj.transform.rotation;
            Quaternion targetdir = Quaternion.Euler(0, 0, angle);
            spriteobj.transform.rotation = targetdir;
            float prevrot = spriteobj.transform.rotation.z;
            if (smoothTargetAngle) {
                spriteobj.transform.rotation = Quaternion.Slerp(saverot, targetdir,  0.3f);
            } else {
                spriteobj.transform.rotation = targetdir;
            }
            if (smoothTargetAngle && !dashCancel && smoothWaitFrames > 30) {
                smoothTargetAngle = false;
            }
            prevDashCancel = dashCancel;
            //spriteobj.transform.rotation = Quaternion.Slerp(spriteobj.transform.rotation, targetdir, 0.1f);
        } else if (swimming ) {
            spriteobj.transform.localPosition = new Vector3 (rb.velocity.normalized.x*-0.5f, rb.velocity.normalized.y*-0.5f, 0);
            spriteobj.transform.rotation = Quaternion.Euler(0, 0, -VecToDeg(rb.velocity.normalized));
        } else if (dolphinJumping) {
            
            //spriteobj.transform.rotation = Quaternion.Euler(0, 0, -VecToDeg(rb.velocity.normalized));
            /*float angle = Mathf.Atan2(rb.velocity.normalized.y, rb.velocity.normalized.x);
            angle = angle * (180 / Mathf.PI) - 90;
            if (yvel < 0f && prevyvel >= 0f) {
                dolphinLerpWaitStamp = StaticDataCW.Time;
            }*/
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Ball")/* && StaticDataCW.Time > dolphinLerpWaitStamp + 0.1f*/) {
                smoothDolphinTargetAngle = true;
                //angle = 0;
                spriteobj.transform.rotation = Quaternion.Euler(0, 0, 0f);
                spriteobj.transform.localPosition = new Vector3 (0, -0.5f, 0f);
            } else {
                spriteobj.transform.localPosition = new Vector3 (rb.velocity.normalized.x*-0.5f, rb.velocity.normalized.y*-0.5f, 0);
                spriteobj.transform.rotation = Quaternion.Euler(0, 0, -VecToDeg(rb.velocity.normalized));
                if (prevyvel < 15f) {
                    smoothDolphinTargetAngle = true;
                    dolphinSmoothWaitFrames = 0;
                }
            }/*
            //angle = Mathf.Lerp(spriteobj.transform.rotation.z, angle, 0.3f);
            Quaternion saverot = spriteobj.transform.rotation;
            Quaternion targetdir = Quaternion.Euler(0, 0, angle);
            spriteobj.transform.rotation = targetdir;
            float prevrot = spriteobj.transform.rotation.z;
            if (smoothDolphinTargetAngle) {
                spriteobj.transform.rotation = Quaternion.Slerp(saverot, targetdir,  0.17f);
            } else {
                spriteobj.transform.rotation = targetdir;
            }
            if (smoothDolphinTargetAngle && !dashCancel && smoothWaitFrames > 30) {
                smoothDolphinTargetAngle = false;
            }*/
            //spriteobj.transform.rotation = Quaternion.Slerp(spriteobj.transform.rotation, targetdir, 0.1f);
        }/* else if (freeze) {
            frozenFrames++;
            if (frozenFrames >= 5) {
                if (spriteobj.transform.localPosition.x >= 0f) {
                    spriteobj.transform.localPosition = new Vector3 (-0.1f, 0f, 0);
                } else {
                    spriteobj.transform.localPosition = new Vector3 (0.1f, 0f, 0);
                }
                frozenFrames = 0;
            }
            spriteobj.transform.rotation = Quaternion.Euler(0, 0, 0);
        }*/ else {
            spriteobj.transform.localPosition = new Vector3 (0, -0.5f, 0);
            spriteobj.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (!slowDashing) {
            hideHook = false;
        }
        if (startSlowDash > 0 && slowmo){
            anim.SetBool("DashTrigger", true);
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DashTrans")) {
            anim.SetBool("DashTrigger", false);
        }
        if ((prevslowmo != slowmo && !slowmo) || (Time.timeScale < 0.1)){
            anim.SetBool("DashTrigger2", true);
        } else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash")) {
            anim.SetBool("DashTrigger2", false);
        }
        anim.SetBool("InIdle", anim.GetCurrentAnimatorStateInfo(0).IsName("Idle2"));
        anim.SetBool("DashDisable", slowmo);
        anim.SetBool("SlowDashing", slowDashing);
        anim.SetBool("Swimming", swimming);
        anim.SetBool("DolphinJumping", dolphinJumping);
        anim.SetBool("DolphinTransFrame", dolphinTransFrame);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("DolphinTrans") || !dolphinJumping) {
            dolphinTransFrame = false;
        }
        attackAnimLock = false;

        for (int i = 0; i < attackAnims.Length; i++) {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(attackAnims[i])) {
                attackAnimLock = true;
            }
        }
        if (slashGraphic == null) {
            maskobj.transform.position = transform.position;
            maskobj.transform.localScale = new Vector3(5f,5f,1f);
            disableMaskChanges = true;
        } else {
            disableMaskChanges = false;
        }
        prevyvel = yvel;
        if (rb.velocity.x > 0) {
            prevxvel = 1;
        } else if (rb.velocity.x < 0) {
            prevxvel = -1;
        } else {
            prevxvel = 0;
        }
        prevxvelreal = rb.velocity.x;
        prevslowmo = slowmo;
        prevWallSliding = wallSliding;
        prevWallSlidingAnims = wallSlidingAnims;
        prevGrounded = onGround;
        prevSlamming = slamming;
        prevInZiplinePoint = inZiplinePoint;
        if (roomBox != null) {
            prevRoomBoxCleared = roomBox.GetComponent<RoomConfigureCW>().isCleared;
        }
        if (startSlowDash == 2) {
            startSlowDash = 0;
        }
        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Ball") && !attackLock && !knockbackDisable && !sdDisable) || wallSlidingAnims){
            spriteobj.transform.localScale = new Vector3(1, 1, 1);
        } else if (!onGround && !attackLock && !knockbackDisable && !sdDisable){
            float fallsize = (-ycap - Mathf.Abs(yvel)) / -ycap;
            fallsize = fallsize + 0.6f;
            if (fallsize > 1f){
                fallsize = 1f;
            }
            if (fallsize < 0.8f){
                fallsize = 0.8f;
            }
            //spriteobj.transform.localScale = new Vector3(fallsize, spriteobj.transform.localScale.y, 1);
        }
        }
        
        if (superInvincible) {
            dashinvincible = true;
            invincible = true;
        }

        if (animSlowingTime) {
            if (animSpeed > 0.001f) {
                animSpeed = animSpeed * 0.81f;
            }
        } else {
            animSpeed = 1f;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash") || anim.GetCurrentAnimatorStateInfo(0).IsName("DashCancelHold") || anim.GetCurrentAnimatorStateInfo(0).IsName("DashCancelOut") || anim.GetCurrentAnimatorStateInfo(0).IsName("DashCancelIn") || anim.GetCurrentAnimatorStateInfo(0).IsName("DashTrans")) {
            anim.speed = 1f;
        } else {
            anim.speed = animSpeed;
        }
    }

    void LateUpdate()
    {
        if (airDash){
            pointLight.intensity = Mathf.SmoothDamp((float)pointLight.intensity,0.5f,ref plIntensity,0.05f);
            pointLight.color = new Color(0f, 1f, 0.43137254902f, 1f);
            pointLight.pointLightOuterRadius = Mathf.SmoothDamp((float)pointLight.pointLightOuterRadius,0.5f,ref plOuter,0.05f);
            pointLight.pointLightInnerRadius = 0.2f;

            emanateLight.intensity = Mathf.SmoothDamp((float)emanateLight.intensity,0.25f,ref emIntensity,0.15f);
            float pointred = Mathf.SmoothDamp((float)emanateLight.color.r, 1f, ref rpoint, 0.2f);
            float pointgreen = Mathf.SmoothDamp((float)emanateLight.color.g, 1f, ref gpoint, 0.2f);
            float pointblue = Mathf.SmoothDamp((float)emanateLight.color.b, 1f, ref bpoint, 0.2f);
            emanateLight.color = new Color(pointred, pointgreen, pointblue, 1f);
            emanateLight.pointLightOuterRadius = Mathf.SmoothDamp((float)emanateLight.pointLightOuterRadius,1.85f,ref emOuter,0.15f);
            emanateLight.pointLightInnerRadius = 0.5f;
        } else{
            pointLight.intensity = Mathf.SmoothDamp((float)pointLight.intensity,1.5f,ref plIntensity,0.1f);
            pointLight.color = new Color(0.43137254902f, 0f, 1f, 1f);
            pointLight.pointLightOuterRadius = Mathf.SmoothDamp((float)pointLight.pointLightOuterRadius,0.5f,ref plOuter,0.1f);
            pointLight.pointLightInnerRadius = 0.2f;

            emanateLight.intensity = Mathf.SmoothDamp((float)emanateLight.intensity,0.4f,ref emIntensity,0.15f);
            float pointred = Mathf.SmoothDamp((float)emanateLight.color.r, 0.43137254902f, ref rpoint, 0.2f);
            float pointgreen = Mathf.SmoothDamp((float)emanateLight.color.g, 0f, ref gpoint, 0.2f);
            float pointblue = Mathf.SmoothDamp((float)emanateLight.color.b, 1f, ref bpoint, 0.2f);
            emanateLight.color = new Color(pointred, pointgreen, pointblue, 1f);
            emanateLight.pointLightOuterRadius = Mathf.SmoothDamp((float)emanateLight.pointLightOuterRadius,1.5f,ref emOuter,0.15f);
            emanateLight.pointLightInnerRadius = 0.5f;
        }
    }

    private bool TargetRay(Vector2 centerDir, int index, float stepAngle, ref Vector2 dashDir, ref Vector2 hitPoint, ref float dashDist, ref GameObject hitobj, bool setTargetHit){
        Vector2 dir = RotateVector(centerDir, stepAngle * index);
        RaycastHit2D boundsCast = Physics2D.Raycast(transform.position - new Vector3(0, 0.3f, 0), dir, 10f, boundsMask);
        bool boundsOn = true;
        if (boundsCast.collider != null) {
            Vector2 dist = (Vector2)boundsCast.collider.transform.position - ((Vector2)transform.position - new Vector2(0, 0.3f));
            float distance = dist.magnitude;
            if (boundsCast.transform.tag == "RoomBoundaries" && distance < 0.01f) {
                boundsOn = false;
            }
        }
        RaycastHit2D raycast;
        if (boundsOn) {
            raycast = Physics2D.Raycast(transform.position - new Vector3(0, 0.3f, 0), dir, 10f, dashMask);
        } else {
            raycast = Physics2D.Raycast(transform.position - new Vector3(0, 0.3f, 0), dir, 10f, boundsCheck);
        }
            if (raycast.collider != null) {
                Vector2 dist = (Vector2)raycast.collider.transform.position - (Vector2)transform.position;
                float distance = dist.magnitude;
                if (distance < 10 && (distance > 1 || raycast.transform.tag == "ZiplinePoint") && ((raycast.transform.tag == "Enemy" && !raycast.transform.gameObject.GetComponent<EnemyDisablerCW>().dying) || raycast.transform.tag == "Dashable" || (raycast.transform.tag == "ZiplinePoint" && (!zipReenterDisable || raycast.transform.gameObject != previousZiplinePoint)))) {
                    hitobj = raycast.transform.gameObject;
                    if (setTargetHit) {
                        targetHit = raycast.transform.gameObject;
                    }
                    dist.Normalize();
                    dashDir = dist;
                    hitPoint = (Vector2)raycast.collider.transform.position;
                    dashDist = distance;
                    return true;
                } else{
                    return false;
                }
            } else{
                return false;
            }
    }
    private Vector2 SwimCast(float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(rb.position, dir, 2, waterBlockMask | groundMask | boundsMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - rb.position;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance)
            {
                return groundPoint.point;
            }
        }
        return new Vector2(-99999f, -99999f);
    }

    private float WaterCast(float distance, Vector2 dir, bool includesGround)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(rb.position + dir*distance, -dir, 10, waterBlockMask | groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - rb.position;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance && (!includesGround || groundPoint.collider.gameObject.layer != 8))
            {
                return groundDistance;
            }
        }
        return -1;
    }

    //makes vector2 from inputs to determine direction
    private Vector2 DashDir(Vector2 prevDir)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(CameraControllerCW.InputRemap(Input.mousePosition)/*new Vector3(InputEx.mousePosition.x, (InputEx.mousePosition.y-StaticDataCW.ScreenOffset)*StaticDataCW.ScreenOffsetScale, 0f)*/);

        Vector2 direction = (Vector2)((mousePosition - (transform.position /*+ new Vector3(0f, 0f, 0f)*/)));
        float mag = direction.magnitude;
        direction.Normalize();
        if (mag < 1 && canslowDash) {
            dashCancel = true;
            //dashCancelGraphic.SetActive(true);
            sdDisable = false;
            return prevDir;
        } else{
            sdDisable = true;
            dashCancel = false;
            //dashCancelGraphic.SetActive(false);
            return direction;
        }

        /*
        float vert = Input.GetAxisRaw("Vertical");
        float horiz = Input.GetAxisRaw("Horizontal");
        Vector2 dir = new Vector2(horiz, vert);
        dir.Normalize();
        if (horiz == 0 && vert == 0){
            dashCancel = true;
            return prevDir;
        } else{
            return dir;
        }*/
    }
    /*
    private Vector2 AttackDir() {
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(InputEx.mousePosition);

        Vector2 direction = (Vector2)((mousePosition - transform.position));
        float mag = direction.magnitude;
        Vector2 fourdir;
        direction.Normalize();
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
            if (direction.x > 0){
                return -1;
            } else{
                return 1;
            }
        } else{
            if (direction.y > 0){
                return 2;
            } else{
                return -2;
            }
        }
        
        /*
        float vert = Input.GetAxisRaw("Vertical");
        float horiz = Input.GetAxisRaw("Horizontal");
        Vector2 dir = new Vector2(horiz, vert);
        dir.Normalize();
        if (horiz == 0 && vert == 0){
            dashCancel = true;
            return prevDir;
        } else{
            return dir;
        }
        
    }
    */

    //rotates a vector by degrees
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

    //point = the point that the raycast will originate from, distance is the distance for it to return true, dir is the directional vector
    private bool CheckGrounded(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance && !groundPoint.collider.gameObject.CompareTag("OneWayDown") && !groundPoint.collider.gameObject.CompareTag("OneWayRight") && !groundPoint.collider.gameObject.CompareTag("OneWayLeft"))
            {
                if (groundPoint.collider.gameObject.CompareTag("OneWayUp")) {
                    standingOnOneWayFrames++;
                }
                return true;
            }
        }
        return false;
    }

    private void SlammableCast(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundDistance < distance && groundPoint.collider.gameObject.CompareTag("SlamBreakable"))
            {
                groundPoint.collider.gameObject.GetComponent<TilemapRenderer>().enabled = false;
                groundPoint.collider.gameObject.GetComponent<SlamBreakableCW>().broken = true;
                groundPoint.collider.gameObject.GetComponent<SlamBreakableCW>().center = (Vector2)groundPoint.collider.gameObject.GetComponent<CompositeCollider2D>().bounds.center;
                groundPoint.collider.gameObject.GetComponent<SlamBreakableCW>().hitPoint = (Vector2)groundPoint.point;
                if (groundPoint.collider.gameObject.GetComponent<TilemapCollider2D>() != null) {
                    groundPoint.collider.gameObject.GetComponent<TilemapCollider2D>().enabled = false;
                } else {
                    groundPoint.collider.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }
    
    

    private (Vector2, bool, float) CheckTrigger(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            groundVec.Normalize();
            if (groundDistance < distance && groundPoint.collider.tag == "Enemy")
            {
                return (groundVec, true, groundDistance);
            }
        }
        return (new Vector2 (0,0), false, 0.0f);
    }

    private Vector2 SpikeCast(){
        RaycastHit2D spikePoint = Physics2D.Raycast(transform.position, new Vector2(0, -1), 30, groundMask | boundsMask);
        if (spikePoint.collider != null)
        {
            if (spikePoint.collider.gameObject.layer == 12 || spikePoint.collider.gameObject.layer == 14) {
                return new Vector2(-9999f, -9999f);
            } else {
                return spikePoint.point + new Vector2(0, (bc.size.y * 0.5f));
            }
        } else{
            return transform.position;
        }
    }
    private Vector2 SpecialSpikeCast(Vector2 dir, bool includesBounds){
        RaycastHit2D spikePoint;
        if (includesBounds) {
            spikePoint = Physics2D.Raycast(transform.position, dir, 30, groundMask | boundsMask);
        } else {
            spikePoint = Physics2D.Raycast(transform.position, dir, 30, groundMask);
        }
        if (spikePoint.collider != null)
        {
            return spikePoint.point + new Vector2(0, (bc.size.y * 0.5f));    
        } else{
            return transform.position;
        }
    }

    private bool CheckForPlatform(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundPoint.collider.gameObject.CompareTag("Semisolid")) {
                if (groundDistance < distance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckGroundedWithoutPlatform(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (!groundPoint.collider.gameObject.CompareTag("Semisolid") && !groundPoint.collider.gameObject.CompareTag("OneWayUp")) {
                if (groundDistance < distance)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool CheckGroundedPlatformOneWay(Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundPoint.collider.gameObject.CompareTag("Semisolid") || groundPoint.collider.gameObject.CompareTag("OneWayUp")) {
                if (groundDistance < distance)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool CheckGroundedWallJump (Vector2 point, float distance, Vector2 dir)
    {
        RaycastHit2D groundPoint = Physics2D.Raycast(point, dir, 2, groundMask);
        if (groundPoint.collider != null)
        {
            Vector2 groundVec = groundPoint.point - point;
            float groundDistance = groundVec.magnitude;
            if (groundPoint.collider.gameObject.name != "UnWallJumpable Ground" && groundPoint.collider.gameObject.tag != "Icicle" && !(dir.Equals(Vector2.right) && groundPoint.collider.gameObject.tag == "OneWayRight") && !(dir.Equals(Vector2.left) && groundPoint.collider.gameObject.tag == "OneWayLeft")) {
                if (groundDistance < distance)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void CheckForCrumble()
    {
        bool found = false;
        for (int i = 0; i < 3; i++) {
            Vector2 point = (Vector2)groundPoint.transform.position;
            if (i == 1) {
                point += new Vector2(0.5f*bcsize.x, 0f);
            } else if (i == 2) {
                point += new Vector2(-0.5f*bcsize.x, 0f);
            }
            RaycastHit2D groundedPoint = Physics2D.Raycast(point, Vector2.down, 2, groundMask);
            if (groundedPoint.collider != null)
            {
                Vector2 groundVec = groundedPoint.point - point;
                float groundDistance = groundVec.magnitude;
                if (groundDistance < 0.2f)
                {
                    if (groundedPoint.collider.gameObject.CompareTag("CrumbleBlock")) {
                        if (!groundedPoint.collider.gameObject.GetComponent<CrumbleBlockCW>().crumbling && yvel < 1f) {
                            groundedPoint.collider.gameObject.GetComponent<CrumbleBlockCW>().crumbling = true;
                        }
                        found = true;
                    }
                }
            }
        }
        if (found) {
            onCrumble = true;
        } else {
            onCrumble = false;
        }
    }

    private void CheckSponge(Vector2 point, float distance, Vector2 dir, float diff) {
        bool found = false;
        for (int i = -1; i < 2; i++) {
            RaycastHit2D hitPoint = Physics2D.Raycast(point + (diff * i * RotateVector(dir, 90f)), dir, 2, groundMask);
            if (hitPoint.collider != null)
            {
                Vector2 groundVec = hitPoint.point - (point + (diff * i * RotateVector(dir, 90f)));
                float groundDistance = groundVec.magnitude;
                if (hitPoint.collider.gameObject.CompareTag("Sponge")) {
                    if (groundDistance < distance)
                    {
                        found = true;
                    }
                }
            }
        }
        if (found) {
            float angle = 0f;
            if (swimming) {
                swimDir = Vector2.Reflect(swimDir, dir);
                Xvel(swimDir.x * swimSpeed, true);
                Yvel(swimDir.y * swimSpeed, true);
                touchingSponge = true;
                angle = Mathf.Atan2(swimDir.y, swimDir.x);
            } else if (superdashing) {
                slowDir = Vector2.Reflect(slowDir, dir);
                Xvel(slowDir.x * 25,true);
                Yvel(slowDir.y * 25,true);
                touchingSponge = true;
                hideHook = true;
                if (hookObject != null) {
                    hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
                waterLockTurningTimer = StaticDataCW.Time + 0.17f;
                angle = Mathf.Atan2(slowDir.y, swimDir.x);
            } else if (slowDashing && !onTarget && StaticDataCW.Time > sdDisableTimer + 0.01f && dashFrameCount < 8) {
                slowDir = Vector2.Reflect(slowDir, dir);
                Xvel(slowDir.x * 15,true);
                Yvel(slowDir.y * 15,true);
                sdDisableTimer += 0.08f;
                touchingSponge = true;
                hideHook = true;
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                dolphinJumping = true;
                dolphinJumpFrames = 0;
                waterLockTurningTimer = StaticDataCW.Time + 0.17f;
                xinput = 0;
                inputDisable = true;
                inputDisableTimer = 0.2f;
                angle = Mathf.Atan2(slowDir.y, slowDir.x);
            } else if (slamming && slamState == 3 && dir.y < 0) {
                slamming = false;
                invincible = false;
                accelerationControl = false;
                //revertSlamVelocity = true;
                inputDisable = false;
                slamState = 0;
                Yvel(Mathf.Max(-yvel * 0.8f, 20f + (logSlamHeight - rb.position.y)/0.75f), true);
                airDash = true;
                inJump = false;
                touchingSponge = true;
                /*dolphinJumping = true;
                dolphinJumpFrames = 0;*/
                waterLockTurningTimer = StaticDataCW.Time + 0.17f;
            } else if (dolphinJumping && dolphinJumpFrames > 14) {
                swimDir = Vector2.Reflect(swimDir, dir);
                //dolphinJumpFrames += 5;
                touchingSponge = true;
                waterLockTurningTimer = StaticDataCW.Time + 0.17f;
                angle = Mathf.Atan2(swimDir.y, swimDir.x);
            } else if (framesSinceEndedNormalDash < 4 && !superdashing && StaticDataCW.Time > superdashEndStamp + 0.1f) {
                Vector2 bounceDir = Vector2.Reflect(new Vector2(prevxvelreal, prevyvel).normalized, dir);
                Xvel(bounceDir.x * 25,true);
                if (dir.x > 0.01f || dir.x < -0.01f || dir.y < 0.99f) {
                    Yvel(bounceDir.y * 25,true); 
                } else {
                    Yvel(bounceDir.y * 10,true); 
                }
                stopNormalDashMomentumStamp = StaticDataCW.Time;
                if (!onGround) {
                    extendNormalDashMomentum = true;
                    spongeExtending = true;
                }
                xinput = 0;
                touchingSponge = true;
                if (hookObject != null) {
                    hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
                waterLockTurningTimer = StaticDataCW.Time + 0.17f;
                xinput = 0;
                inputDisable = true;
                inputDisableTimer = 0.2f;
                angle = Mathf.Atan2(bounceDir.y, bounceDir.x);
            }
            angle = angle * (180 / Mathf.PI) - 90;
            float particleAngle = 90 - angle;
            if (particleAngle > 180) {
                particleAngle -= 360;
            }
            Quaternion particletargetdir = Quaternion.Euler(particleAngle, 90, 0);
            dashTrail.gameObject.transform.rotation = particletargetdir;
            var main = dashTrail.main;
            main.startRotation = particleAngle * Mathf.Deg2Rad;
        }
    }

    void OnTriggerEnter2D (Collider2D other) 
    {
        if (other.CompareTag("RoomBox")) {
            GameObject prevRoomBox = roomBox;
            roomBox = other.gameObject;
            if (prevRoomBox != roomBox && framesSinceSceneLoad > 3 && resetting <= 0 && Time.time > SceneManagerObj.GetComponent<SceneManagerCW>().freezeStamp + 0.35f) {
                if (gliderObtained) {
                    GameObject gliderParticle = Instantiate(gliderDestroyParticle as GameObject);
                    gliderParticle.transform.position = transform.position;
                    gliderParticle.GetComponent<BurstCW>().deathTime = 0.5f;
                }
                if (GameObject.FindWithTag("Canvas") != null) {
                    GameObject.FindWithTag("Canvas").GetComponent<DialogueManagerCW>().QueueToolTip("", false);
                }
                health = 3;
                resetting = 2;
                gliderObtained = false;
                currentGliderReference = null;
                gliderActivated = false;
                accelerationControl = false;
                SaveDataCW.Load();
                Game.loadedGame.healthPackInRoomCollected = 0;
                roomTransitioning = true;
                lockRoomTransitioning = 2;
                SceneManagerObj.GetComponent<SceneManagerCW>().Freeze(0.35f, true);
                if (screenLock == null && other.GetComponent<RoomConfigureCW>().isClearable && other.GetComponent<RoomConfigureCW>().enemiesActive > 0 && !roomBox.GetComponent<RoomConfigureCW>().isCleared) {
                    screenLock = Instantiate(Resources.Load("Prefabs/ScreenLock", typeof(GameObject)) as GameObject);
                    screenLock.transform.parent = GameObject.FindWithTag("MainCamera").transform;
                    screenLock.transform.localScale = new Vector3(1f, 1f, 1f);
                    screenLock.transform.localPosition = new Vector3(0f,0f,10f);
                    screenLockSprite = screenLock.GetComponent<SpriteRenderer>();
                    screenLockAnim = screenLock.GetComponent<Animator>();
                    screenLockSprite.color = new Color(0.8828125f, 0.99609375f, 0.46875f, 1f);
                    screenLockAnim.Play("LockScreen");
                }
            }
        }
        if (other.CompareTag("RoomBound")) {
            if (SpikeCast().x > -9998.9f || SpikeCast().y > -9998.9f || SpikeCast().x < -9999.1f || SpikeCast().y < -9999.1f) {
                spikePoint = SpikeCast();
            } else if (other.GetComponent<BoxCollider2D>().size.y != 1) {
                spikePoint = SpecialSpikeCast(Vector2.down, false);
            } else {
                if (CheckGroundedPlatformOneWay((Vector2) groundPoint.transform.position, 2f, Vector2.up)) {
                    spikePoint = SpecialSpikeCast(Vector2.up, false);
                } else {
                    spikePoint = SpecialSpikeCast(Vector2.down, true);
                }
            }
            if (roomTransitioning) {
                roomTransitioning = false;
                if (yvel >= 0 && other.gameObject.GetComponent<BoxCollider2D>() != null && other.gameObject.GetComponent<BoxCollider2D>().size.y <= 1f) {
                    jumpTimerHeight = StaticDataCW.Time + 0.4f;
                    if (yvel < 12) {
                        Yvel(12f, true);
                    }
                }
            }
            SaveDataCW.Load();
            PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
            if (Game.loadedGame != null && player != null) {
                Game.loadedGame.currentSpikePointx = player.spikePoint.x;
                Game.loadedGame.currentSpikePointy = player.spikePoint.y;
            }
            SaveDataCW.Save();
        }
        if (other.CompareTag("Conduit")) {
            touchingConduit = true;
        }
        if (other.CompareTag("Death")) {
            touchingDeath = true;
        } else if (other.CompareTag("RoomBoundTrigger")) {
            Vector2 dirSwitch = new Vector2(rb.velocity.x, Mathf.Abs(rb.velocity.x));
            dirSwitch.Normalize();
            ts.position = new Vector2(ts.position.x+1.7f*dirSwitch.x,ts.position.y);
        } else if (other.CompareTag("Checkpoint") || other.CompareTag("CheckpointFirst")) {
            /*if ((StaticDataCW.CurrentCheckpointScene != (SceneManager.GetActiveScene().name)) || (StaticDataCW.CurrentCheckpointPos != new Vector2(other.gameObject.GetComponent<Transform>().position.x,other.gameObject.GetComponent<Transform>().position.y))) {
                health = 3;
            }*/
            //health = 3;
            int cpNum = other.GetComponent<CheckpointCW>().checkpointNum;
            SaveDataCW.Load();
            if (Game.loadedGame != null && cpNum > Game.loadedGame.furthestCP) {
                Game.loadedGame.furthestCP = cpNum;
            }
            /*if (Game.loadedGame != null) {
                Game.loadedGame.cutsceneIndex = other.gameObject.GetComponent<CheckpointCW>().cutsceneNum;
            }*/
            SaveDataCW.Save();
            StaticDataCW.CurrentCheckpointPos = (new Vector2(other.gameObject.GetComponent<Transform>().position.x,other.gameObject.GetComponent<Transform>().position.y));
            StaticDataCW.CurrentCheckpointScene = (SceneManager.GetActiveScene().name);
            if (other.gameObject.GetComponent<CheckpointCW>() != null){
                StaticDataCW.CurrentCheckpointID = other.gameObject.GetComponent<CheckpointCW>().checkpointNum;
            }
            spikePoint = StaticDataCW.CurrentCheckpointPos;
        } else if (other.CompareTag("SceneBoundA")) {
            //health += healing;
            //StaticDataCW.Health = health;
            SaveDataCW.Load();
            PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
            if (Game.loadedGame != null && player != null) {
                Game.loadedGame.currentSpikePointx = player.spikePoint.x;
                Game.loadedGame.currentSpikePointy = player.spikePoint.y;
            }
            SaveDataCW.Save();
            StaticDataCW.Zoom = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>().zoom;
            changingScene = true;
            changeSceneStamp = Time.time;
            inputDisable = true;
            whichBound = 0;
            inputDisableTimer = StaticDataCW.Time + 0.5f;
            anim.enabled = false;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
            Time.timeScale = 1f;
            slowmo = false;
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            superdashing = false;
            dashBuffer = false;
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().exiting = true;
        } else if (other.CompareTag("SceneBoundB")) {
            //health += healing;
            //StaticDataCW.Health = health;
            SaveDataCW.Load();
            PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
            if (Game.loadedGame != null && player != null) {
                Game.loadedGame.currentSpikePointx = player.spikePoint.x;
                Game.loadedGame.currentSpikePointy = player.spikePoint.y;
            }
            SaveDataCW.Save();
            if (StaticDataCW.Beta && SceneManager.GetActiveScene().name == "Level3Section2Scene4") {
                TextOnlyCutscene(44);
            } else {
                StaticDataCW.Zoom = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>().zoom;
                changingScene = true;
                int i;
                int level = 0;
                for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
                    sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
                    if (sceneNum != -1) {
                        break;
                    } 
                    level ++;
                }
                StaticDataCW.InWorldMap = (sceneNum == StaticDataCW.AllScenes[i].Count - 1);
                SaveDataCW.Load(); 
                if (Game.loadedGame != null) {
                    Game.loadedGame.inWorldMap = StaticDataCW.InWorldMap;
                    if (level + 1 > Game.loadedGame.levelsUnlocked && sceneNum == StaticDataCW.AllScenes[i].Count - 1) {
                        Game.loadedGame.levelsUnlocked = level + 1;
                    }
                }
                SaveDataCW.Save();
                changeSceneStamp = Time.time;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 0.5f;
                whichBound = 1;
                anim.enabled = false;
                Time.timeScale = 1f;
                slowmo = false;
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                superdashing = false;
                dashBuffer = false;
                rb.constraints |= RigidbodyConstraints2D.FreezePosition;
                GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().exiting = true;
            }
        } else if (other.CompareTag("TriggerGround")) {
            inGround = true;
        } else if (other.CompareTag("EnemyAttack") && ((!invincible || other.GetComponent<EnemyAttackCW>().ignoreInvincibility)/* && !dashinvincible && extendDashInvincibilityStamp < 50*/ && !spikeInvincible)) {
            health -= other.GetComponent<EnemyAttackCW>().damage;
            if (other.gameObject.GetComponent<EnemyAttackCW>().type == 2 && !other.gameObject.GetComponent<EnemyAttackCW>().dontDestroyOnPlayer) {
                other.GetComponent<EnemyAttackCW>().ProjDie();
            }
            if (other.gameObject.name == "CabbageHitbox") {
                other.gameObject.transform.parent.gameObject.GetComponent<CabbageCW>().Die();
            }
            otherIgnoreInvinc = other.GetComponent<EnemyAttackCW>().ignoreInvincibility;
            Hit(2, other);
        } else if (other.CompareTag("AltCameraZone")) {
            if (roomBox != null) {
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].Follow = other.GetComponentsInChildren<Transform>()[1];
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 1;
                other.GetComponentsInChildren<Transform>()[1].position = this.transform.position;
                other.GetComponentsInChildren<CameraZoneCW>()[0].isActive = true;
                exitedCameraZone = false;
            }
        } else if (other.CompareTag("CollectibleHealingOrb")) {
            other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            other.gameObject.GetComponent<CollectibleOrbCW>().collecting = true;
            if (StaticDataCW.HealingOrbs < 5) {
                StaticDataCW.HealingOrbs ++;
            }
        } else if (other.CompareTag("ZiplinePoint")) {
            if (!ziplining && (!slowmo && onTarget && dashinvincible) && other.gameObject == targetHit) {
                rb.position = (Vector2) other.transform.position;
                zipPos = (Vector2) other.transform.position;
                zipSetPos = 2;
                ziplining = true;
                extendZipMomentum = false;
                inputDisable = true;
                inputDisableTimer = Time.time + 9999999f;
                xinput = 0;
                zipDir = other.gameObject.GetComponent<FrostGuardDetectorCW>().ziplineDir;
                airDash = true;
                sdDisable = false;
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                SceneManagerCW.PlaySound("Pop", 1);
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                superdashing = false;
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashTurn = false;
                otherZipPoint = other.gameObject.GetComponent<FrostGuardDetectorCW>().otherZipPoint;
                GetComponent<BoxCollider2D>().isTrigger = true;
            } else if (ziplining && other.gameObject == otherZipPoint) {
                ziplining = false;
                SceneManagerCW.PlaySound("Pop", 1);
                GetComponent<BoxCollider2D>().isTrigger = false;
                inputDisable = false;
                Yvel(yvel * 0.75f,true);
                extendZipMomentum = true;
                timeEndedZip = StaticDataCW.Time;
                previousZiplinePoint = other.gameObject;
                zipReenterDisable = true;
                if (!gliderObtained) {
                    zipJumpable = true;
                    if (jumping) {
                        Yvel(15, false);
                        if (xvelmove > 1f) {
                            Xvel(10f, false);
                        } else if (xvelmove < -1f) {
                            Xvel(-10f, false);
                        }
                    }
                }
            }
            savedZiplinePoint = other;
            inZiplinePoint = true;
        } else if (other.CompareTag("Glider")) {
            other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            other.gameObject.GetComponent<Animator>().SetBool("On", false);
            other.gameObject.GetComponent<GliderControllerCW>().squished = true;
            gliderObtained = true;
            currentGliderReference = other.gameObject;
            gliderActivated = false;
        } else if (other.CompareTag("CamshiftZone")) {
            inCamshiftZone = true;
            cameraZoneSavePos = other.GetComponentsInChildren<Transform>()[1].localPosition;
        } else if (other.gameObject.layer == 24 && !spikeInvincible && !ziplining && !noclip && StaticDataCW.Time > SceneManagerObj.GetComponent<SceneManagerCW>().unPauseStamp + 0.05f) {
            if (/*(slowDashing || framesSinceEndedNormalDash < 5) && */framesSinceEndedSwim > 1 && !(bubbleWarping && bubbleTeleporting)) {
                if ((Mathf.Abs(slowDir.x) > 0.05f || Mathf.Abs(slowDir.y) > 0.05f) && slowDashing) {
                    swimDir = slowDir;
                } else {
                    swimDir = rb.velocity.normalized;
                }
                canslowDash = false;
                slowDashing = false;
                dashTrail.Stop();
                slowmo = false;
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashCancel = false;
                sdDisable = false;
                swimming = true;
                bubbleTrail.Play();
                swimStartStamp = StaticDataCW.Time;
                swimStartPoint = transform.position;
                jumping = false;
                inJump = false;
                spinAttackStage = 0;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                airDash = true;
                slamming = false;
                invincible = false;
                wallSliding = false;
                dolphinJumping = false;
                superdashing = false;
                framesSinceEndedNormalDash = 4;
                extendZipMomentum = false;
                extendNormalDashMomentum = false;
                spongeExtending = false;
                swimSpeed = 15f;
                xinput = 0;
                swimCancelLock = 3;
                inputDisable = true;
                inputDisableTimer = Time.time + 9999999f;
                disableAllActions = true;
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                Vector2 checkDir = SwimCast(1.5f, swimDir);
                int index = 1;
                while (checkDir.x < -99998f && checkDir.y < -99998f && index < 360) {
                    checkDir = SwimCast(1.5f, RotateVector(swimDir, 0.5f*index));
                    if (index > 0) {
                        index = -index;
                    } else {
                        index = -index + 1;
                    }
                }
                GameObject splash = Instantiate(splash1Ref as GameObject);
                if (!waterBlockChecker.GetComponent<FrostGuardDetectorCW>().waterBlockInside) {
                    splash.transform.position = transform.position;
                } else {
                    splash.transform.position = lastPosOutsideWater;
                }
                /*
                if (savedEdge != null) {
                    GameObject splash = Instantiate(splash1Ref as GameObject);
                    splash.transform.position = checkDir;
                    if (savedEdge.direction == 0) {
                        splash.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
                    } else if (savedEdge.direction == 2) {
                        splash.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    } else if (savedEdge.direction == 1) {
                        splash.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    } else {
                        splash.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    }
                }*/
                if (!waterBlockChecker.GetComponent<FrostGuardDetectorCW>().waterBlockInside) {
                    if (index >= 360) {
                        Hit(0, null);
                    }
                    if ((rb.position - checkDir).magnitude < 0.25f) {
                        rb.position = checkDir;
                    }
                }
            }/* else if (!swimming && !spikeInvincible && !ziplining && !dolphinJumping && !superInvincible && framesSinceEndedSwim > 1) {
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                hitDeath = true;
                Hit(0, null);
            }*/
        } else if (other.CompareTag("Whirlpool")) {
            whirlpoolCenters.Add(other.gameObject.transform.position);
            inWhirlpool++;
        } else if (other.CompareTag("Cannon") && StaticDataCW.Time > cannonLaunchStamp + 0.15f && !noclip && !spikeInvincible) {
            canslowDash = false;
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            slowmo = true;
                slowdownStartFrames = 0;
            startSlowDash = 1;
            gravityCancel = false;
            //saveFacingDir = facingdir;
            slowdownSoundReference = SceneManagerCW.PlaySound("Slowdown", 0.8f);
            cannonRef = other.gameObject;
            cannonRef.GetComponentsInChildren<Animator>()[0].SetBool("Charging", true);
            smoothTargetAngle = false;
            slowDir = new Vector2(0,0);
            onTarget = false;
            dashCancel = false;
            sdDisable = false;
            slamming = false;
            invincible = false;
            wallSliding = false;
            dolphinJumping = false;
            superdashing = false;
            inCannon = true;
            inputDisable = false;
            Xvel(0f, true);
            Yvel(0f, true);
            xinput = 0;
            rb.position = (Vector2) other.gameObject.transform.position;
        } else if (other.CompareTag("Bubble") && !swimming && !bubbleWarping && !bubbleCancelTeleporting && !noclip) {
            bubbleBuffer = 4;
            bubbleSaveDir = -rb.velocity;
            Vector2 diffDir = (other.gameObject.transform.position - transform.position).normalized;
            if (other.gameObject.GetComponent<BubbleCW>().verticalHorizontal == 1) {
                bubbleDir = diffDir.y >= 0? Vector2.up:Vector2.down;
            } else if (other.gameObject.GetComponent<BubbleCW>().verticalHorizontal == 2) {
                bubbleDir = diffDir.x >= 0? Vector2.right:Vector2.left;
            } else {
                if (diffDir.y >= 0.707106781) {
                    bubbleDir = Vector2.up;
                } else if (diffDir.y <= -0.707106781) {
                    bubbleDir = Vector2.down;
                } else {
                    bubbleDir = diffDir.x >= 0? Vector2.right:Vector2.left;
                }
            }
        } else if (other.CompareTag("BubbleQuadrantDetector")) {
            touchingQuadrant = true;
        } else if (other.CompareTag("AbductorCircle")) {
            if (other.gameObject.GetComponent<AbductorFadeCW>().type == 1) {
                inAbductorCircle = true;
            }
            if (other.gameObject.GetComponent<AbductorFadeCW>().type == 2 && !inAbductorCircle && (!invincible || slamming) && !spikeInvincible && !bubbleWarping && !inCannon) {
                freeze = true;
                frozenFrames = 3;
                freezeStamp = StaticDataCW.Time;
                /*superdashing = false;
                disableAllActions = true;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 100f;
                xinput = 0;
                canslowDash = false;
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                slowmo = false;
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashCancel = false;
                sdDisable = false;
                jumping = false;
                inJump = false;
                spinAttackStage = 0;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                slamming = false;
                invincible = false;
                extendZipMomentum = false;
                extendNormalDashMomentum = false;
                gliderActivated = false;
                accelerationControl = false;
                spongeExtending = false;
                wallSliding = false;
                framesSinceEndedNormalDash = 4;
                gliderActivated = false;
                accelerationControl = false;
                swimSpeed = 0f;
                xinput = 0;
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                */
            }
        }
    }

    void OnCollisionEnter2D (Collision2D other)
    {
        if (other.gameObject.layer == 8 && StaticDataCW.Time > respawnStamp + 0.05f) {
            int directionOfDash = (xvelmove >= 0)? 1:-1;
            bool isWalled = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x*directionOfDash,0f), 0.02f, new Vector2(directionOfDash,0)) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x*directionOfDash, 0.5f*bcsize.y), 0.02f, new Vector2(directionOfDash,0)) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x*directionOfDash, bcsize.y), 0.02f, new Vector2(directionOfDash,0));
            if ((slowDashing || extendDashInvincibilityStamp > 53) && isWalled) {
                bool isWalledBottom = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,0f), 0.05f, new Vector2(directionOfDash,0)) &&
                !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,0.75f*bcsize.y), 0.05f, new Vector2(directionOfDash,0)) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,bcsize.y), 0.05f, new Vector2(directionOfDash,0)));
                if (isWalledBottom && directionOfDash != 0) {
                    float height;
                    bool steppable = false;
                    for (height = 0; height < 0.45f; height += 0.02f) {
                        if (!CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,height), 0.02f, new Vector2(directionOfDash,0))) {
                            steppable = true;
                            height += 0.02f;
                            break;
                        }
                    }
                    if (steppable && !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x,bcsize.y), height, Vector2.up) ||
                    CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0f,bcsize.y), height, Vector2.up) ||
                    CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x,bcsize.y), height, Vector2.up))) {
                        rb.position = new Vector2(rb.position.x, rb.position.y + height);
                        //slopeBuffer = 2;
                    } else {
                        //WallBounce();
                    }   
                } else {
                    //WallBounce();
                }
                bool isWalledTop = CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,bcsize.y), 0.05f, new Vector2(directionOfDash,0)) &&
                !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,0.25f*bcsize.y), 0.05f, new Vector2(directionOfDash,0)) ||
                CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,0f), 0.05f, new Vector2(directionOfDash,0)));
                if (isWalledTop && directionOfDash != 0) {
                    float height;
                    bool steppable = false;
                    for (height = 0; height < 0.45f; height += 0.02f) {
                        if (!CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x * directionOfDash,bcsize.y-height), 0.02f, new Vector2(directionOfDash,0))) {
                            steppable = true;
                            break;
                        }
                    }
                    if (steppable && !(CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(-0.5f*bcsize.x,0), height, Vector2.down) ||
                    CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0f,0), height, Vector2.down) ||
                    CheckGroundedWithoutPlatform((Vector2)groundPoint.transform.position + new Vector2(0.5f*bcsize.x,0), height, Vector2.down))) {
                        rb.position = new Vector2(rb.position.x, rb.position.y - height);
                    } else {
                       //WallBounce();
                    }   
                } else {
                    //WallBounce();
                }
            }
        } else if (touchEdgeCutscene && roomBox.name == "Room3" && (other.gameObject.name == "RoomBound3 Hitbox" || other.gameObject.name == "RoomBound4 Hitbox" || other.gameObject.name == "RoomBound5 Hitbox") && !other.gameObject.GetComponent<BoxCollider2D>().isTrigger) {
            touchEdgeCutscene = false;
            TextOnlyCutscene(36);
        }
    }

    void OnTriggerExit2D (Collider2D other) 
    {
        if (other.CompareTag("Conduit")) {
            touchingConduit = false;
        }
        if (other.CompareTag("Death")) {
            touchingDeath = false;
        } else if (other.CompareTag("AltCameraZone")) {
            other.GetComponentsInChildren<CameraZoneCW>()[0].isActive = false;
            roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].Follow = cameraPoint.transform;
            cameraZoneExitStamp = StaticDataCW.Time;
            saveCameraZone = other.gameObject;
            cameraZoneRoomBox = roomBox;
            exitedCameraZone = true;
        } else if (other.CompareTag("TriggerGround")) {
            inGround = false;
        } else if (other.CompareTag("ZiplinePoint")) {
            inZiplinePoint = false;
        } else if (other.CompareTag("CamshiftZone")) {
            inCamshiftZone = false;
        } else if (other.CompareTag("Vent") && gliderActivated && other.GetComponent<BoxCollider2D>() != null && transform.position.y > other.transform.position.y + other.GetComponent<BoxCollider2D>().size.y/2 + 0.3f) {
            ventCancelTimer = StaticDataCW.Time;
        } else if (other.CompareTag("Whirlpool")) {
            inWhirlpool--;
            whirlpoolCenters.Remove(other.gameObject.transform.position);
        } else if (other.CompareTag("PurpleWaterBlock")) {
            inPurpleWater = false;
        } else if (other.CompareTag("Bubble") && StaticDataCW.Time > SceneManagerObj.GetComponent<SceneManagerCW>().unPauseStamp + 0.05f) {
            inBubble = false;
            hasExitedBubble++;
        } else if (other.CompareTag("BubbleQuadrantDetector")) {
            touchingQuadrant = false;
        } else if (other.CompareTag("RoomBound")) {
            SaveDataCW.Load();
            PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
            if (Game.loadedGame != null && player != null) {
                Game.loadedGame.currentSpikePointx = player.spikePoint.x;
                Game.loadedGame.currentSpikePointy = player.spikePoint.y;
            }
            SaveDataCW.Save();
        } else if (other.CompareTag("AbductorCircle") && other.gameObject.GetComponent<AbductorFadeCW>().type == 1) {
            inAbductorCircle = false;
        }
    }

    void OnTriggerStay2D (Collider2D other) 
    {
        if (other.CompareTag("RoomBound")) {
            if (SpikeCast().x > -9998.9f || SpikeCast().y > -9998.9f || SpikeCast().x < -9999.1f || SpikeCast().y < -9999.1f) {
                spikePoint = SpikeCast();
            } else if (other.GetComponent<BoxCollider2D>().size.y != 1) {
                spikePoint = SpecialSpikeCast(Vector2.down, false);
            } else {
                if (CheckGroundedPlatformOneWay((Vector2) groundPoint.transform.position, 2f, Vector2.up)) {
                    spikePoint = SpecialSpikeCast(Vector2.up, false);
                } else {
                    spikePoint = SpecialSpikeCast(Vector2.down, true);
                }
            }
            if (roomTransitioning) {
                roomTransitioning = false;
                if (yvel >= 0 && other.gameObject.GetComponent<BoxCollider2D>() != null && other.gameObject.GetComponent<BoxCollider2D>().size.y <= 1f) {
                    jumpTimerHeight = StaticDataCW.Time + 0.4f;
                    if (yvel < 12) {
                        Yvel(12f, true);
                    }
                }
            }
        } else if (((!invincible && !dashinvincible && extendDashInvincibilityStamp < 50) && (other.CompareTag("Enemy")) || (other.CompareTag("KnockbackDamage") && !(onTarget && slowDashing)))) {
            hitDeath = false;
            health -= 1;
            Hit(1, other);
        } else if ((((!slowmo && onTarget && dashinvincible) || StaticDataCW.Time < keepTargetCheckStamp + 0.15f) && other.CompareTag("Enemy") && extendDashInvincibilityStamp < 50 && other.gameObject == targetHit)/* || (other.CompareTag("Dashable") && !slowmo && dashinvincible && onTarget && other.gameObject == targetHit && extendDashInvincibilityStamp < 50)*/) {
            extendDashInvincibilityStamp = 60;
            gravityCancel = false;
            //extendDashCancel = false;
            xinput = 0f;
            sdDisable = false;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            SceneManagerCW.PlaySound("Pop", 1);
            dashinvincible = false;
            airDash = true;
            superdashing = false;
            Xvel(rb.velocity.x *0.2f, true);
            keepSlowDir.Normalize();
            Yvel(yvel * 0.75f,true); //doesn't do anything?
            /*
            zipJumpable = true;
            if (jumping) {
                Yvel(15, false);
                if (xvelmove > 1f) {
                    Xvel(10f, false);
                } else if (xvelmove < -1f) {
                    Xvel(-10f, false);
                }
            }
            */
            //if ((Mathf.Atan(keepSlowDir.y/keepSlowDir.x) % (2f*Mathf.PI)) * (keepSlowDir.x >= 0f? -1:1) > 0f) {
                //Yvel(50, true);
            //} else {
            //    Yvel(40, true);
            //}
            /*if (dashMeter > 0.99f && dashMeter < 1.01f) {
                dashMeter = 0f;
                GameObject dashHitboxObj = Instantiate(dashHitbox, transform.position, Quaternion.identity) as GameObject;
                AttackCW dashHit = dashHitboxObj.GetComponent<AttackCW>();
                dashHit.type = 2;
                dashHit.deathtime = 0.2f;
                if (keepSlowDir.x > 0) {
                    if (Mathf.Abs(keepSlowDir.x) > Mathf.Abs(keepSlowDir.y)) {
                        dashHit.dir = -1;
                    } else if (keepSlowDir.y > 0) {
                        dashHit.dir = 2;
                    } else {
                        dashHit.dir = -2;
                    }
                } else {
                    if (Mathf.Abs(keepSlowDir.x) > Mathf.Abs(keepSlowDir.y)) {
                        dashHit.dir = 1;
                    } else if (keepSlowDir.y > 0) {
                        dashHit.dir = 2;
                    } else {
                        dashHit.dir = -2;
                    }
                }
                dashHit.realDir = keepSlowDir;
            }*/
        } else if (other.CompareTag("ResetCrystal")) {
            if (!airDash) {
                airDash = true;
                other.gameObject.GetComponent<ResetCrystalCW>().Break();
            }
            if (slamming && slamState == 3) {
                slamming = false;
                invincible = false;
                accelerationControl = false;
                revertSlamVelocity = true;
                inputDisable = false;
                slamState = 0;
                other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                other.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                other.gameObject.GetComponent<ResetCrystalCW>().squished = true;
                Yvel(-6f, true);
            }
        } else if (other.CompareTag("Vent") && gliderActivated) {
            /*if (yvel < -1.5f) {
                Yvel(2f, false);
            } else if (yvel < 6f) {
                Yvel(1f, false);
            } else if (yvel < 12f) {
                Yvel(0.5f, false);
            }*/
            if (StaticDataCW.Time < ventCancelTimer + 0.2f) {
                Yvel(0f, true);
                ventCancelTimer = StaticDataCW.Time;
            } else {
                Yvel(2.6f, true);
            }
            ventGliding = true;
        } else if (other.gameObject.layer == 24 && !spikeInvincible && !ziplining && !noclip && StaticDataCW.Time > SceneManagerObj.GetComponent<SceneManagerCW>().unPauseStamp + 0.05f) {
            if (!slowDashing && !swimming && dolphinJumping && framesSinceEndedSwim > 1 && !(bubbleWarping && bubbleTeleporting)) {
                swimDir = rb.velocity.normalized;
                canslowDash = false;
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                slowmo = false;
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashCancel = false;
                sdDisable = false;
                swimming = true;
                bubbleTrail.Play();
                swimStartStamp = StaticDataCW.Time;
                swimStartPoint = transform.position;
                jumping = false;
                inJump = false;
                spinAttackStage = 0;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                airDash = true;
                slamming = false;
                invincible = false;
                extendZipMomentum = false;
                extendNormalDashMomentum = false;
                gliderActivated = false;
                accelerationControl = false;
                spongeExtending = false;
                wallSliding = false;
                framesSinceEndedNormalDash = 4;
                gliderActivated = false;
                accelerationControl = false;
                swimSpeed = 15f;
                xinput = 0;
                swimCancelLock = 3;
                inputDisable = true;
                inputDisableTimer = Time.time + 9999999f;
                disableAllActions = true;
                dolphinJumping = false;
                superdashing = false;
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;

                Vector2 checkDir = SwimCast(1.5f, swimDir);
                int index = 1;
                while (checkDir.x < -99998f && checkDir.y < -99998f && index < 360) {
                    checkDir = SwimCast(1.5f, RotateVector(swimDir, 0.5f*index));
                    if (index > 0) {
                        index = -index;
                    } else {
                        index = -index + 1;
                    }
                }
                GameObject splash = Instantiate(splash1Ref as GameObject);
                if (!waterBlockChecker.GetComponent<FrostGuardDetectorCW>().waterBlockInside) {
                    splash.transform.position = transform.position;
                } else {
                    splash.transform.position = lastPosOutsideWater;
                }
                /*if (savedEdge != null) {
                    GameObject splash = Instantiate(splash1Ref as GameObject);
                    splash.transform.position = checkDir;
                    if (savedEdge.direction == 0) {
                        splash.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
                    } else if (savedEdge.direction == 2) {
                        splash.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    } else if (savedEdge.direction == 1) {
                        splash.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    } else {
                        splash.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    }
                }*/
                if (!waterBlockChecker.GetComponent<FrostGuardDetectorCW>().waterBlockInside) {
                    if (index >= 360) {
                        Hit(0, null);
                    }
                    if ((rb.position - checkDir).magnitude < 0.25f) {
                        rb.position = checkDir;
                    }
                }
            }/* else if (!slowDashing && framesSinceEndedNormalDash >= 3 && !swimming && !spikeInvincible && !superInvincible && !ziplining && !dolphinJumping) {
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                hitDeath = true;
                Hit(0, null);
            }*/
        } else if (other.CompareTag("Bubble") && swimming && !bubbleWarping && !bubbleCancelTeleporting && !noclip) {
            inBubble = true;
            BubbleWarp(other);
        } else if (other.CompareTag("BubbleQuadrantDetector")) {
            Vector2 diffDir = (other.gameObject.transform.position - transform.position).normalized;
            if (diffDir.y >= 0.707106781) {
                quadrantCounter += new Vector4(-1f,-1f,-1f,1f);
            } else if (diffDir.y <= -0.707106781) {
                quadrantCounter += new Vector4(-1f,1f,-1f,-1f);
            } else {
                quadrantCounter += diffDir.x >= 0? new Vector4(1f,-1f,-1f,-1f):new Vector4(-1f,-1f,1f,-1f);
            }
            for (int i = 0; i < 4; i++) {
                if (quadrantCounter[i] < 0f) {
                    quadrantCounter[i] = 0f;
                }
                if (quadrantCounter[i] > 6f) {
                    quadrantCounter[i] = 6f;
                }
            }
            quadrantParent = other.GetComponent<QuadrantDetectorCW>().parent;
        } else if (other.CompareTag("UpDoor")) {
            if (!upDooring && Input.GetKeyDown(StaticDataCW.Keys["Up"]) && onGround && !wallSlidingAnims && !slowmo && !slowDashing && !ziplining && !slamming && !swimming && (!dolphinJumping || dolphinJumpFrames <= 14) && !disableAllActions) {
                upDooring = true;
                disableAllActions = true;
                upDoorStamp = StaticDataCW.Time;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 9999f;
                GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading = true;
                spikeFadeStamp = StaticDataCW.Time + 9999f;
                upDoorPos = other.gameObject.GetComponent<UpDoorCW>().endPosition;
                invincible = true;
                invincibilityTimer = StaticDataCW.Time + 1f;
                noInvincibleBlink = true;
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0f;
                roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 0f;
                xinput = 0;
            }
        }
        if (other.CompareTag("PurpleWaterBlock")) {
            if (swimming) {
                inPurpleWater = true;
                if (canCreateTempSpike) {
                    Instantiate(tempSpike as GameObject).transform.position = transform.position;
                    canCreateTempSpike = false;
                }
            }
        }
    }

    //dev commands console debug
    public void EnableNoclip() {
        noclip = !noclip;
        if (!noclip) {
            bc.isTrigger = false;
        }
        superInvincible = !superInvincible;
        if (!superInvincible) {
            invincible = false;
            dashinvincible = false;
        }
    }
    public void SetNoclipSpeed(float speed) {
        noclipSpeed = speed;
    }
    public void ToggleInvisibility() {
        sr.enabled = !sr.enabled;
    }

    public void ToggleInvincibility() {
        superInvincible = !superInvincible;
        if (!superInvincible) {
            invincible = false;
            dashinvincible = false;
        }
    }
    public void ReloadScene() {
        //health += healing;
        //StaticDataCW.Health = health;
        StaticDataCW.InWorldMap = false;
        StaticDataCW.Dying = true;
        StaticDataCW.PrevScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void CPWarp(int cpNum) {
        GameObject[] allCPs = GameObject.FindGameObjectsWithTag("Checkpoint");
        if (allCPs.Length > 0) {
            List<int> cpNums = new List<int>(0);
            foreach (GameObject cp in allCPs) {
                cpNums.Add(cp.GetComponent<CheckpointCW>().checkpointNum);
            }
            cpNums.Sort();
            foreach (GameObject cp in allCPs) {
                if (cp.GetComponent<CheckpointCW>().checkpointNum == cpNums[cpNum]) {
                    rb.velocity = new Vector2(0f, 0f);
                    rb.position = (Vector2) cp.transform.position;
                }
            }
        } else {
            print("No checkpoints found in scene.");
        }
    }
    public void ClearRoom() {
        if (roomBox != null && roomBox.GetComponent<RoomConfigureCW>() != null) {
            roomBox.GetComponent<RoomConfigureCW>().Clear();
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
            obj.GetComponent<CutsceneTriggerCW>().activated = true;
        }
    }
    public void Return() {
        SaveDataCW.Load();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("CutsceneTrigger")) {
            obj.GetComponent<CutsceneTriggerCW>().activated = true;
        }
        rb.position = returnPos;
        Game.loadedGame.currentSpikePointx = returnPos.x;
        Game.loadedGame.currentSpikePointy = returnPos.y;
    }
    public void NoclipBind(string keyToBind) {
        TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
        if (keyToBind != "null_string") {
            PlayerPrefs.SetString("NoclipBind", textInfo.ToTitleCase(keyToBind));
            SceneManagerObj.GetComponent<SceneManagerCW>().ReloadKeybinds();
        }
    }
    public void ReturnBind(string keyToBind) {
        TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
        if (keyToBind != "null_string") {
            PlayerPrefs.SetString("ReturnBind", textInfo.ToTitleCase(keyToBind));
            SceneManagerObj.GetComponent<SceneManagerCW>().ReloadKeybinds();
        }
    }
    public void ClearBind(string keyToBind) {
        TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
        if (keyToBind != "null_string") {
            PlayerPrefs.SetString("ClearBind", textInfo.ToTitleCase(keyToBind));
            SceneManagerObj.GetComponent<SceneManagerCW>().ReloadKeybinds();
        }
    }

    void WallBounce() {
        if (onTarget) {
            Yvel(0,true);
            Xvel(0,true);
            sdDisable = false;
            onTarget = false;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            hookObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Hook", false);
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            SceneManagerCW.PlaySound("Pop", 1);
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            slowDir = new Vector2(0,0);
        }
    }

    void SpawnDolphinAttack() {
        dolphinAttack = Instantiate(dolphinHitbox, transform.position, Quaternion.identity) as GameObject;
        dolphinAttack.transform.parent = transform;
        AttackCW dolphinAtk = dolphinAttack.GetComponent<AttackCW>();
        dolphinAtk.type = 5;
        dolphinAtk.dmgValue = 0;
        dolphinAtk.deathtime = 100f;
        dolphinAtk.dir = 0;
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

    public void BounceOnEnemy(int dir){
        //spriteobj.transform.localScale = new Vector3(1, 1.05f, 1f);
        if (dir == -2) {
            Yvel(6.9f/*8*/,true);
        } else {
            Yvel(-2,true);
        }
        airDash = true;
        inJump = false;
    }
    
    void BubbleWarp(Collider2D other) {
        bubbleSavePos = (Vector2) other.gameObject.transform.position;
        rb.position = bubbleSavePos;
        bubbleRef = other.gameObject.GetComponent<BubbleCW>().otherBubble;
        if (bubbleBuffer == 0) {
            bubbleSaveDir = -swimDir;
            if (quadrantParent != null) {
                Vector2 diffDir = (other.gameObject.transform.position - transform.position).normalized;
                if (other.gameObject.GetComponent<BubbleCW>().verticalHorizontal == 1) {
                    bubbleDir = diffDir.y >= 0? Vector2.up:Vector2.down;
                } else if (other.gameObject.GetComponent<BubbleCW>().verticalHorizontal == 2) {
                    bubbleDir = diffDir.x >= 0? Vector2.right:Vector2.left;
                } else {
                    if (diffDir.y >= 0.707106781) {
                        bubbleDir = Vector2.up;
                    } else if (diffDir.y <= -0.707106781) {
                        bubbleDir = Vector2.down;
                    } else {
                        bubbleDir = diffDir.x >= 0? Vector2.right:Vector2.left;
                    }
                }
            } else {
                Vector2 highest = new Vector2(-1f, 0f);
                for (int i = 0; i < 4; i++) {
                    if (quadrantCounter[i] > highest.x) {
                        highest.x = quadrantCounter[i];
                        highest.y = i;
                    }
                }
                if (highest.y == 0) {
                    bubbleDir = Vector2.right;
                } else if (highest.y == 1) {
                    bubbleDir = Vector2.down;
                } else if (highest.y == 2) {
                    bubbleDir = Vector2.left;
                } else {
                    bubbleDir = Vector2.up;
                }
            }
        }
        other.gameObject.GetComponent<BubbleCW>().direction = bubbleDir;
        other.gameObject.GetComponent<BubbleCW>().playAnim = true;
        bubbleWarping = true;
        sr.enabled = false;
        invincible = true;
        noInvincibleBlink = true;
        invincibilityTimer = StaticDataCW.Time + 999999f;
        Xvel(0f, true);
        Yvel(0f, true);
        bubbleCancelling = false;
        bubbleTeleporting = true;
        bubbleWarpTimestamp = StaticDataCW.Time;
        swimDir = Vector2.right;
        swimSpeed = 0f;
        hasExitedBubble = 0;
        spikeInvincible = true;
        roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.5f;
        roomBox.GetComponentsInChildren<CinemachineVirtualCamera>()[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 0.5f;
    }

    void Hit(int byEnemy, Collider2D other) {
        if (!changingScene) {
            /*if (health < 1f) {
                //die if dead
                Death();
            } else {*/
                //set disables
                hitDeath = byEnemy == 0;
                if (other == null || other.GetComponent<EnemyAttackCW>() == null || !other.GetComponent<EnemyAttackCW>().dontGrantInvincibility) {
                    invincible = true;
                }
                if (!inGround) {
                    ziplining = false;
                }
                GetComponent<BoxCollider2D>().isTrigger = false;
                inputDisable = false;
                invincibilityTimer = StaticDataCW.Time;
                
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                /*
                canslowDash = false;
                slowDashing = false;
                slowmo = false;
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashDistance = 0f;
                dashTime = 0f;
                dashFrameCount = 0;
                dashCancel = false;
                sdDisable = false;
                */
                dashinvincible = false;
                attackLock = false;
                wallAttacking = false;
                lockedWallSliding = false;
                
                if (gliderObtained) {
                    GameObject gliderParticle = Instantiate(gliderDestroyParticle as GameObject);
                    gliderParticle.transform.position = transform.position;
                    gliderParticle.GetComponent<BurstCW>().deathTime = 0.5f;
                }
                gliderObtained = false;
                currentGliderReference = null;
                gliderActivated = false;
                ventCancelTimer = -9.0f;
                accelerationControl = false;
                slamming = false;
                revertSlamVelocity = true;
                assistWallSlide = 0;
                slamState = 0;
                chargingSpin = false;
                disableAllActions = false;
                spinChargeStamp = -9.0f;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                spinChargeParticleStamp = -9.0f;
                spinAttackTimer = -9.0f;
                
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                if (!otherIgnoreInvinc) {
                    SceneManagerCW.PlaySound("Hurt", 1);
                    SceneManagerCW.PlaySound("Pop", 1);
                    noInvincibleBlink = false;
                } else {
                    //SceneManagerCW.PlaySound("Heal", 0.7f);
                    noInvincibleBlink = true;
                }
                if (byEnemy == 0 || health < 1f) {
                    health = 3;
                    if (dashHitboxObj != null) {
                        Destroy(dashHitboxObj);
                    }
                    sdDisable = false;
                    slowDashing = false;
                    dashTrail.Stop();
                    bubbleTrail.Stop();
                    if (dashHitboxObj != null) {
                        Destroy(dashHitboxObj);
                    }
                    superdashing = false;
                    //inWhirlpool = 0;
                    whirlpoolCenters = new List<Vector3>();
                    swimming = false;
                    bubbleTrail.Stop();
                    dolphinJumping = false;
                    hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    slowDir = new Vector2(0,0);
                    inputDisable = true;
                    inputDisableTimer = StaticDataCW.Time + 0.7f;
                    GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().spikeFading = true;
                    spikeFadeStamp = StaticDataCW.Time;
                    Xvel(0f,true);
                    Yvel(0f,true);
                    dashFrameCount = 0;
                    slowmo = false;
                    dashCancel = false;
                    spikeInvincible = true;
                    bubbleWarping = false;
                    jumping = false;
                    dying = true;
                    atkfloat = 0;
                    Time.timeScale = 1f;
                    deathFadePlayer = Instantiate(Resources.Load("Prefabs/DyingPlayerUI", typeof(GameObject)) as GameObject);
                    deathFadePlayer.transform.parent = GameObject.FindWithTag("Canvas").transform;
                    deathFadePlayer.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position);
                    
                    if (sr.flipX) {
                        deathFadePlayer.GetComponent<RectTransform>().localScale = new Vector3(-1f, 1f, 1f);
                    } else {
                        deathFadePlayer.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    }
                    blackBars = new GameObject[1];
                    for (int i = 0; i < 1; i++) {
                        blackBars[i] = Instantiate(Resources.Load("Prefabs/BlackBars", typeof(GameObject)) as GameObject);
                        blackBars[i].GetComponent<RectTransform>().SetParent(GameObject.FindWithTag("Canvas").transform);
                        blackBars[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 5f);
                        blackBars[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    }
                    sr.enabled = false;
                    SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = true;
                }
                //spriteobj.transform.localScale = new Vector3(0.75f, 1.25f, 1f);
                if (byEnemy == 1) {
                    knockbackDisable = true;
                	knockbackDisableTimer = StaticDataCW.Time;
                	//if enemy then knockback
		            if (other.gameObject.transform.position.x > gameObject.transform.position.x) {
		                Xvel(-24,true);
		                Yvel(14,true);
		            } else {
		                Xvel(24,true);
		                Yvel(14,true);
		            }
		        } else if (byEnemy == 2) {
                    if (otherIgnoreInvinc) {
                        canslowDash = false;
                        slowDashing = false;
                        dashTrail.Stop();
                        bubbleTrail.Stop();
                        if (dashHitboxObj != null) {
                            Destroy(dashHitboxObj);
                        }
                        slowmo = false;
                        slowDir = new Vector2(0,0);
                        onTarget = false;
                        dashDistance = 0f;
                        dashTime = 0f;
                        dashFrameCount = 0;
                        dashCancel = false;
                        sdDisable = false;
                        superdashing = false;
                        dolphinJumping = false;
                        extendNormalDashMomentum = false;
                        spongeExtending = false;
                        hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (!slowmo && !sdDisable && !canslowDash) {
                	    knockbackDisable = true;
                        if (otherIgnoreInvinc) {
                	        knockbackDisableTimer = StaticDataCW.Time - 0.2f;
                        } else {
                            knockbackDisableTimer = StaticDataCW.Time;
                        }
                    }
                    EnemyAttackCW es = other.gameObject.GetComponent<EnemyAttackCW>();
                    if (es.relativeDirection) {
                        knockbackDisableTimer = StaticDataCW.Time - 0.1f;
                        Xvel((other.gameObject.transform.position-transform.position).normalized.x*other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,true);
                        Yvel((other.gameObject.transform.position-transform.position).normalized.y*other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,true);
                    } else {
                        if (es.dir == -1) {
                            Xvel(-other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,!es.additivevelocity);
                            Yvel(7,true);
                        } else if (es.dir == 1) {
                            Xvel(other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,!es.additivevelocity);
                            Yvel(7,true);
                        } else if (es.dir == 2) {
                            Xvel(0f,true);
                            Yvel(other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,!es.additivevelocity);
                        } else if (es.dir == -2) {
                            Xvel(0f,true);
                            Yvel(other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,!es.additivevelocity);
                        } else {
                            knockbackDisableTimer = StaticDataCW.Time - 0.1f;
                            Xvel(Mathf.Cos(es.dir)*other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,true);
                            Yvel(Mathf.Sin(es.dir)*other.gameObject.GetComponent<EnemyAttackCW>().knockbackValue,true);
                        }
                    }
                } else if (byEnemy == 3) {
                    //conduit idk
                }
                //reset movement
                xinput = 0f;
                squish = 1f;
                xaccel = 0f;
                otherIgnoreInvinc = false;
            //}
        }
    }

    void Death() 
    {
        /*for (int i = 0; i < StaticDataCW.EnemiesLoaded.Count; i++) {
            StaticDataCW.EnemiesLoaded[i].dead = false;
        }*/
        if (StaticDataCW.CurrentCheckpointScene == SceneManager.GetActiveScene().name) {
            health = 3;
            invincible = true;
            spikeInvincible = true;
            sr.enabled = false;
            slowmo = false;
            sdDisable = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            Time.timeScale = 1f;
            deathFade = Instantiate(Resources.Load("Prefabs/ScreenFadeWorld", typeof(GameObject)) as GameObject);
            deathFade.transform.parent = GameObject.FindWithTag("MainCamera").transform;
            deathFade.transform.localScale = new Vector3(10f, 10f, 10f);
            deathFade.transform.localPosition = new Vector3(0f,0f,10f);
            deathFadeSprite = deathFade.GetComponent<SpriteRenderer>();
            deathFadeTimer = StaticDataCW.Time;
            deathFadePlayer = Instantiate(Resources.Load("Prefabs/DyingPlayer", typeof(GameObject)) as GameObject);
            deathFadePlayer.transform.parent = this.transform;
            deathFadePlayer.transform.localPosition = new Vector3(0f,0f,0f);
            SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = true;
            hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            deathFading = true;
            deathFadeReset = true;
            disableAllActions = true;
            inputDisable = true;
            xinput = 0;
            inputDisableTimer = StaticDataCW.Time + 2f;
            jumping = false;
            crossSceneDeath = false;
            /*
            StaticDataCW.HealingOrbs = 0;
            Reset();
            resetting = 2;
            rb.position = StaticDataCW.CurrentCheckpointPos;
            unpausable = false;
            */
        } else {
            StaticDataCW.HealingOrbs = 0;
            //StaticDataCW.Health = 3;
            StaticDataCW.Zoom = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>().zoom;
            health = 3;
            invincible = true;
            spikeInvincible = true;
            sr.enabled = false;
            slowmo = false;
            sdDisable = false;
            if (dashSoundReference != null) {
                SceneManagerCW.StopSound(dashSoundReference, "Dashing");
            }
            slowDashing = false;
            dashTrail.Stop();
            bubbleTrail.Stop();
            superdashing = false;
            if (dashHitboxObj != null) {
                Destroy(dashHitboxObj);
            }
            Time.timeScale = 1f;
            deathFade = Instantiate(Resources.Load("Prefabs/ScreenFadeWorld", typeof(GameObject)) as GameObject);
            deathFade.transform.parent = GameObject.FindWithTag("MainCamera").transform;
            deathFade.transform.localScale = new Vector3(10f, 10f, 10f);
            deathFade.transform.localPosition = new Vector3(0f,0f,10f);
            deathFadeSprite = deathFade.GetComponent<SpriteRenderer>();
            deathFadeTimer = StaticDataCW.Time;
            deathFadePlayer = Instantiate(Resources.Load("Prefabs/DyingPlayer", typeof(GameObject)) as GameObject);
            deathFadePlayer.transform.parent = this.transform;
            deathFadePlayer.transform.localPosition = new Vector3(0f,0f,0f);
            SceneManagerObj.GetComponent<SceneManagerCW>().unpausable = true;
            deathFading = true;
            deathFadeReset = true;
            disableAllActions = true;
            inputDisable = true;
            xinput = 0;
            inputDisableTimer = StaticDataCW.Time + 2f;
            jumping = false;
            crossSceneDeath = true;
            /*
            changingScene = true;
            changeSceneStamp = Time.time;
            inputDisable = true;
            inputDisableTimer = StaticDataCW.Time + 0.5f;
            whichBound = 2;
            anim.enabled = false;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().exiting = true;
            */
        }
    }

    public void Yvel(float velocity, bool setadd){
        if (setadd){
            canyvelset = true;
            yvelset = velocity;
        } else{
            canyveladd = true;
            yveladd = velocity;
        }
    }

    public void Xvel(float velocity, bool setadd){
        if (setadd){
            canxvelset = true;
            xvelset = velocity;
        } else{
            canxveladd = true;
            xveladd = velocity;
        }
    }

    public async void ParticleStopTimer(ParticleSystem ps, float time) {
        await new WaitForSeconds(time);
        if (slowDashing == false){
            ps.Stop();
        }
    }

    //task
    //while loop until prim reaches thing
    //exit task

    public async Task WalkToX(float xPos, bool srEnabled) {
        //await land on the ground
        xinput = 0;
        while (!onGround) {
            await Task.Yield();
        }
        
        while (!((rb.position.x >= xPos && xinput == 1) || (rb.position.x <= xPos && xinput == -1))) {
            inputDisable = true;
            inputDisableTimer = StaticDataCW.Time + 9999999f;
            if (rb.position.x > xPos) {
                xinput = -1;
                sr.flipX = true;
            } else {
                xinput = 1;
                sr.flipX = false;
            }
            await Task.Yield();
        }
        rb.position = new Vector2(xPos, rb.position.y);
        xinput = 0f;
        sr.enabled = srEnabled;
    }

    public void ResetToIdle() {
        canslowDash = false;
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                slowmo = false;
                slowDir = new Vector2(0,0);
                onTarget = false;
                dashCancel = false;
                sdDisable = false;
                swimming = false;
                jumping = false;
                inJump = false;
                spinAttackStage = 0;
                spinAttacking = false;
                if (slashGraphic != null) {
                    Destroy(slashGraphic);
                }
                if (dashHitboxObj != null) {
                    Destroy(dashHitboxObj);
                }
                airDash = true;
                slamming = false;
                invincible = false;
                extendZipMomentum = false;
                extendNormalDashMomentum = false;
                gliderActivated = false;
                accelerationControl = false;
                spongeExtending = false;
                wallSliding = false;
                framesSinceEndedNormalDash = 4;
                gliderActivated = false;
                accelerationControl = false;
                swimSpeed = 15f;
                xinput = 0;
                swimCancelLock = 3;
                dolphinJumping = false;
                superdashing = false;
                if (dashSoundReference != null) {
                    SceneManagerCW.StopSound(dashSoundReference, "Dashing");
                }
                hookObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;

    }

    //cutscene stuff
    public void CutsceneFreeze(int id) {
        if (upDooring) {
            BufferCutscene(id, 0);
        } else {
            ResetToIdle();
            inputDisable = true;
            invincible = true;
            inputDisableTimer = Time.time + 9999999f;
            invincibilityTimer = Time.time + 9999999f;
            noInvincibleBlink = true;
            disableAllActions = true;
            inCutscene = true;
            if (slashGraphic != null) {
                Destroy(slashGraphic);
            }
            xinput = 0f;
            //sr.enabled = false;
            
            //game crashes when u enter cutscene 3
            StartCutscene(id);
        }
    }
    public async Task BufferCutscene(int id, int type) {
        while (upDooring) {
            await Task.Yield();
        }
        if (type == 0) {
            CutsceneFreeze(id);
        } else {
            TextOnlyCutscene(id);
        }
    }

    public async void TextOnlyCutscene(int id) {
        if (upDooring) {
            await BufferCutscene(id, 1);
        } else {
            disableAllActions = true;
            inCutscene = true;
            ResetToIdle();
            inputDisable = true;
            invincible = true;
            inputDisableTimer = Time.time + 9999999f;
            invincibilityTimer = Time.time + 9999999f;
            noInvincibleBlink = true;
            if (slashGraphic != null) {
                Destroy(slashGraphic);
            }
            xinput = 0f;
            dm.StartCutscene(id);
            await PreCutsceneScript(id);
            await CutsceneScript(id);
            inputDisable = false;
            disableAllActions = false;
            xinput = 0f;
        }
    }
    
    async void StartCutscene(int id) {
        await PreTimelineScript(id);
        GameObject timelineObj = gameObject;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("CutsceneTimeline")) {
            if (int.Parse(obj.name) == id) {
                timelineObj = obj;
            }
        }
        //plays and immediately pauses timeline, awaits real playerobj walk
        timeline = timelineObj.GetComponent<PlayableDirector>();
        SignalReceiver sigr = timelineObj.GetComponent<SignalReceiver>();
        UnityEvent pauseEvent = new UnityEvent();
        print(id);
        pauseEvent.AddListener(Pause);
        if (sigr.Count() > 0) {
            sigr.RemoveAtIndex(0);
        }
        sigr.AddReaction(Resources.Load("Signals/Pause") as SignalAsset, pauseEvent);
        pauseTask = new TaskCompletionSource<bool>();

        dm.StartCutscene(id);

        markerIndex = 0;
        markers = null;
        TimelineAsset ta = (TimelineAsset)timeline.playableAsset;
        IEnumerable<IMarker> enumerable = ta.markerTrack.GetMarkers();
        markers = enumerable.ToList();
        markers.Sort(delegate(IMarker x, IMarker y) 
        {
            if (x.time == y.time) return 0;
            else if (x.time < y.time) return -1;
            else if (x.time > y.time) return 1;
            else return 0;
        });

        timeline.Play();
        GameObject playerCutsceneObj = GameObject.FindWithTag("PlayerCutscene");
        playerCutsceneObj.GetComponent<CutsceneObjectCW>().pointLight.enabled = false;
        playerCutsceneObj.GetComponent<CutsceneObjectCW>().emanateLight.enabled = false;
        playerCutsceneObj.GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
        print(StaticDataCW.Time);

        //plays skips first two signals
        timeline.playableGraph.GetRootPlayable(0).SetSpeed(0);
        await new WaitForSeconds(0.5f);
        //if (timeline.playableGraph.IsValid()) {
            timeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
            await WaitForSignal(timeline);
            timeline.playableGraph.GetRootPlayable(0).SetSpeed(0);
            
            timelineSpeedOverride = true;
            print(StaticDataCW.Time);
            await PreCutsceneScript(id);
            await WalkToX(playerCutsceneObj.transform.position.x, false);
            timelineSpeedOverride = false;
            print(StaticDataCW.Time);

            foreach (GameObject pt in GameObject.FindGameObjectsWithTag("CutsceneCameraPoint")) {
                if (pt.GetComponent<CutsceneCameraPointCW>().id == id) {
                    cameraPoint.transform.SetParent(pt.transform);
                    cameraPoint.transform.localPosition = Vector3.zero;
                    cutsceneCameraParented = true;
                    break;
                }
            } 

            //starts playing
            timeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
            playerCutsceneObj.GetComponent<SpriteRenderer>().enabled = true;
            pointLight.GetComponent<Light2D>().enabled = false;
            emanateLight.GetComponent<Light2D>().enabled = false;
            playerCutsceneObj.GetComponent<CutsceneObjectCW>().pointLight.enabled = true;
            playerCutsceneObj.GetComponent<CutsceneObjectCW>().emanateLight.enabled = true;

            await CutsceneScript(id);

            //end
            float endXPos = playerCutsceneObj.transform.position.x;
            rb.position = new Vector2(endXPos, rb.position.y);
            sr.flipX = playerCutsceneObj.GetComponent<SpriteRenderer>().flipX;
            xinput = 0f;
            await new WaitForFixedUpdate();
        //}
        CutsceneUnfreeze(id);
    }
    public async Task WaitForSignal(PlayableDirector timeline)
    {
        if (timeline.playableGraph.IsValid()) {
            if (!timelineSpeedOverride && timeline != null && timeline.playableGraph.IsValid()) {
                timeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }
            while (!timelinePause) {
                await Task.Yield();
            }
            timelinePause = false;
            print(timelinePauseTime);
            /*pauseTask = new TaskCompletionSource<bool>();
            await pauseTask.Task;
            pauseTask = new TaskCompletionSource<bool>();*/
            print("wait for signal!");
            if (!timelineSpeedOverride && timeline != null && timeline.playableGraph.IsValid()) {
                timeline.playableGraph.GetRootPlayable(0).SetSpeed(0);
                timeline.time = timelinePauseTime;
                markerIndex++;
            }
        }
    }

    public void Pause()
    {
        //timeline.playableGraph.GetRootPlayable(0).SetSpeed(0);
        //if task is paused already do not call
        //pauseTask.SetResult(true);
        print("PAUSE!");

        timelinePauseTime = markers[markerIndex].time + 0.005;
        print(markerIndex);
        print(markers[markerIndex].time);
        timelinePause = true;
        timeline.time = timelinePauseTime;
    }

    public async Task RecursiveCutscene42Fun() {
        while (!slowmo) {
            await Task.Yield();
        }
        if (!GameObject.Find("Block1").GetComponent<TilemapRenderer>().enabled) {
            GameObject breakPart = Instantiate(Resources.Load<GameObject>("Prefabs/TutorialBlockParticle") as GameObject);
            for (int i = 0; i < 10; i++) {
                Instantiate(Resources.Load<GameObject>("Prefabs/TutorialBlockParticle2") as GameObject).transform.position = new Vector3(88.75f + Random.Range(-0.5f, 0.5f), 1.25f + Random.Range(-0.5f, 0.5f), 0f);
                Instantiate(Resources.Load<GameObject>("Prefabs/TutorialBlockParticle2") as GameObject).transform.position = new Vector3(85f + Random.Range(-3f,3f), -5.5f, 0f);
            }
            breakPart.transform.position = new Vector3(88.75f, -4f, 0f);
            GameObject.Find("Block1").GetComponent<TilemapRenderer>().enabled = true;
            GameObject.Find("Block1").GetComponent<TilemapCollider2D>().enabled = true; //
            GameObject.Find("Block1").GetComponent<CompositeCollider2D>().enabled = true;
            GameObject.Find("Block2").GetComponent<TilemapRenderer>().enabled = false;
            GameObject.Find("Block2").GetComponent<TilemapCollider2D>().enabled = false;
            GameObject.Find("Block2").GetComponent<CompositeCollider2D>().enabled = false;
            GameObject.Find("Block3").GetComponent<TilemapRenderer>().enabled = true;
            GameObject.Find("Block3").GetComponent<TilemapCollider2D>().enabled = true; //
            GameObject.Find("Block3").GetComponent<CompositeCollider2D>().enabled = true;
        }
        while ((slowmo || canslowDash) && !slowDashing) {
            await Task.Yield();
        }
        await Task.Yield();
        if (!dashCancel) {
            ResetToIdle();
            Yvel(0f, true);
            Xvel(0f, true);
            freezePosition = true;
            cutscene42Dash = true;
            rb.position = cutscene42SavePos;
            anim.Play("Ball");
            while (!slowmo) {
                await Task.Yield();
            } 
            await RecursiveCutscene42Fun();
        } 
    }

    public async Task RecursiveCutscene40Fun() {
        while (!slowmo) {
            await Task.Yield();
        }
        while ((slowmo || canslowDash) && !slowDashing) {
            await Task.Yield();
        }
        await Task.Yield();
        if (dashCancel) {
            ResetToIdle();
            Yvel(0f, true);
            Xvel(0f, true);
            freezePosition = true;
            cutscene42Dash = true;
            anim.Play("Ball");
            while (!slowmo) {
                await Task.Yield();
            }  
            await RecursiveCutscene40Fun();
        } 
    }

    public async Task WaitUntilDashingOrCanceling() {
        while (slowmo && !slowDashing) {
            await Task.Yield();
        }
    }

    public string ReplaceWithKeybind(string textToReplace) {
        string tooltipText = textToReplace;
        string finalText = "";
                        for (int i = 0; i < tooltipText.Length; i++) {
                            //print(tooltipText[i]);
                            if (tooltipText[i].Equals(char.Parse("%"))) {
                                for (int j = i+1; j < tooltipText.Length; j++) {
                                    //print(tooltipText[j]);
                                    if (tooltipText[j].Equals(char.Parse("%"))) {
                                        string firstString = "";
                                        string lastString = "";
                                        if (i != 0) {
                                            firstString = tooltipText.Substring(0, i);
                                        }
                                        if (j != tooltipText.Length - 1) {
                                            lastString = tooltipText.Substring(j+1);
                                        }
                                        //print(lastString);
                                        //print(tooltipText.Substring(i+1, j-i-1));
                                        string replacementText;
                                        print(tooltipText.Substring(i+1, j-i-1));
                                        if ((StaticDataCW.FriendlyKeys.TryGetValue(StaticDataCW.Keys[tooltipText.Substring(i+1, j-i-1)], out string value)? value:"null") != "null") {
                                            replacementText = StaticDataCW.FriendlyKeys[StaticDataCW.Keys[tooltipText.Substring(i+1, j-i-1)]];
                                        } else {
                                            replacementText = StaticDataCW.Keys[tooltipText.Substring(i+1, j-i-1)].ToString();
                                        }
                                        finalText += replacementText;
                                        i = j;
                                        break;
                                        //tooltipText = firstString + replacementText + lastString;
                                    }
                                }
                            } else {
                                finalText += tooltipText.Substring(i, 1);
                            }
                        }
                    return finalText;
    }

    public void CutsceneUnfreeze(int id) {
        timeline.Stop();
        invincible = false;
        noInvincibleBlink = false;
        inCutscene = false;
        GameObject.FindWithTag("PlayerCutscene").GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
        GameObject.FindWithTag("PlayerCutscene").GetComponent<CutsceneObjectCW>().pointLight.enabled = false;
        GameObject.FindWithTag("PlayerCutscene").GetComponent<CutsceneObjectCW>().emanateLight.enabled = false;
        pointLight.GetComponent<Light2D>().enabled = true;
        emanateLight.GetComponent<Light2D>().enabled = true;
        inputDisable = false;
        disableAllActions = false;
        cutsceneCameraParented = false;
        if (cameraPoint != null)
        {
            cameraPoint.transform.parent = transform;
        }
        xinput = 0f;
        sr.enabled = true;
        timelinePause = false;
        Game.loadedGame.cutsceneIndex = id;
    }

    public void BossSpikeCast() {
        spikePoint = SpikeCast();
        SaveDataCW.Load();
        PlayerControllerCW players = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
        if (Game.loadedGame != null && players != null) {
            //Game.loadedGame.currentSpikePointx = players.spikePoint.x-7f;
            Game.loadedGame.currentSpikePointx = players.spikePoint.x;
            Game.loadedGame.currentSpikePointy = players.spikePoint.y;
            disableSpikePointSave = true;
        }
        SaveDataCW.Save();
    }
    public async Task TimelinePlay()
    {
        timeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
    }
    
    public async Task PreTimelineScript(int id) {
        switch (id) {
            case 9:
                GameObject.Find("NorthernOverseer").GetComponent<EnemyDisablerCW>().enabled = false;
                GameObject.Find("NorthernOverseer").GetComponent<NorthernOverseerCW>().enabled = false;
                break;
            case 44:
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().GoToPos(new Vector2(56f, -2.5f));
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 0;
                await new WaitForSeconds(0.8f);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().goingToPos = false;
                break;
            default:
                break;
        }
    }

    public async Task PreCutsceneScript(int id) {
        switch (id) {
            case 46:
                freezePosition = true;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().slowingTime = true;
                animSlowingTime = true;
                Yvel(0f, true);
                Xvel(0f, true);
                break;
            /*case 50:
                freezePosition = true;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().slowingTime = true;
                animSlowingTime = true;
                Yvel(0f, true);
                Xvel(0f, true);
                break;*/
            default:
                break;
        }
    }

    public async Task CutsceneScript(int id) {
        switch (id) {
                case 3:
                    await WaitForSignal(timeline); //Prim runs over James, trips, gets up and turns around
                    await dm.NextDialogue(0); //Oho! Greetings
                    await WaitForSignal(timeline); //James shivering
                    await dm.NextDialogue(0); // I must've passed out! I'd almost frozen!
                    await dm.NextDialogue(0); //That is good
                    await dm.NextDialogue(0); //James ???
                    await WaitForSignal(timeline); //James back to idle
                    await dm.NextDialogue(0); //Well, thank you for waking me up...
                    await dm.NextDialogue(0); //I am Prim
                    await WaitForSignal(timeline); //James excitement!
                    await dm.NextDialogue(0); //Ohoho! Prim! what an amazing name
                    await WaitForSignal(timeline); //James back to idle
                    await dm.NextDialogue(0); //My name is James
                    await WaitForSignal(timeline); //James poignant like Cyprus holding hand to heart
                    await dm.NextDialogue(0); //I too, am a curious explorer, just as new!
                    await WaitForSignal(timeline); //James looks back at Prim, Prim points
                    await dm.NextDialogue(0); //What is that thing?
                    await dm.TextBox(false, 0); //butterfly flies in, Prim looks up
                    await WaitForSignal(timeline);
                    await dm.NextDialogue(0); //Oh, that? That's just a butterfly
                    await dm.NextDialogue(0); //...Butterfly.
                    await WaitForSignal(timeline); //James excitement!
                    await dm.NextDialogue(0); //Ohoho! There's much more to this world to explore!
                    await WaitForSignal(timeline); //James idle
                    await dm.NextDialogue(0); //Prim ...
                    await dm.NextDialogue(0); //You're curious aren't you?
                    await dm.NextDialogue(0); //Like you, I am a newcomer
                    await WaitForSignal(timeline); //Prim looks back down
                    await dm.NextDialogue(0); //So what brings you up here?
                    await dm.NextDialogue(0); //I was created mere hours ago.
                    await WaitForSignal(timeline); //James excitement!
                    await dm.NextDialogue(0); //A quest? Wonderful!
                    await WaitForSignal(timeline); //James Idle Prim turns away
                    await dm.NextDialogue(0); //I must continue
                    await dm.NextDialogue(0); //Stay safe, and curious!
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline);
                    break;
                case 4:
                    await WaitForSignal(timeline); //Prim crouches behind rock, sees James walk up to Cellia (camera pans over)
                    await dm.NextDialogue(0); //Halt! Who goes there?
                    await WaitForSignal(timeline); //James hands on hips + Cellia Idle
                    await dm.NextDialogue(0); //Greetings, stranger! I am but a humble traveller
                    await dm.NextDialogue(0); //Ah, new are you? A pleasure!
                    await dm.NextDialogue(0); //So, what brings you out here Cellia
                    await WaitForSignal(timeline); //Cellia laughs
                    await dm.NextDialogue(0); //Hahaha! Don't tell anybody, but I've been dodging work
                    await WaitForSignal(timeline); //Both laugh
                    await dm.NextDialogue(0); //Won't tell a soul!
                    await WaitForSignal(timeline); //Both normal
                    await dm.NextDialogue(0); //Oh, thank enic, supposed to be up
                    await WaitForSignal(timeline); //James hands on hips
                    await dm.NextDialogue(0); //Ohoho! The Verdant Talet?
                    await WaitForSignal(timeline); //MUSIC CUTS OUT - Cellia is skeptical, James freezes in fear
                    await dm.NextDialogue(0); //???
                    await dm.NextDialogue(0); //...
                    await dm.NextDialogue(0); //You being serious?
                    await dm.NextDialogue(0); //I- uhh..
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //Cellia walks over to James
                    await dm.NextDialogue(0); //You trying to pull my leg?
                    await dm.NextDialogue(0); //Uhh.. sorry, ma'am
                    await dm.NextDialogue(0); //Trying to scare me, huh? 200 years... 
                    await dm.NextDialogue(0); //Ha! what a load of nonsense 
                    await dm.NextDialogue(0); //sorry, u gotta come w me
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //locks up James
                    await dm.NextDialogue(0); //What are you-hey!
                    await dm.TextBox(false, 0); //James locked away
                    await WaitForSignal(timeline); //end
                    break;
                case 5:
                    await WaitForSignal(timeline); //soldiers and elluirians standing in pride
                    await dm.NextDialogue(0); //we stand
                    await WaitForSignal(timeline); //prim puzzled and pondering, hand on chin
                    await dm.NextDialogue(0); //enic ordail?
                    await WaitForSignal(timeline); //prim looks back up
                    await dm.NextDialogue(0); //the first brave
                    await dm.NextDialogue(0); //we thank you
                    await dm.NextDialogue(0); //we looked to
                    await WaitForSignal(timeline); //soldier turns solemn, shaking head
                    await dm.NextDialogue(0); //jade was cursed
                    await dm.NextDialogue(0); //we are sorry
                    await WaitForSignal(timeline); //determined face soldier
                    await dm.NextDialogue(0); //it took all
                    await dm.NextDialogue(0); //never again
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //soldiers return to idle, villagers disperse, one bumps into prim, embarrassed, slides backwards looking down (still frame)
                    await dm.NextDialogue(0); //oops sorry
                    await WaitForSignal(timeline); //eyes widen
                    await dm.NextDialogue(0); //...
                    await WaitForSignal(timeline); //prim surprised
                    await dm.NextDialogue(0); //...
                    await dm.NextDialogue(0); //THE JADE GOLIATH!
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //elluirian runs away
                    await dm.NextDialogue(0); //?
                    await WaitForSignal(timeline); //soldier turns to see prim
                    await dm.NextDialogue(0); //ORDAIL HAVE MERCY
                    await WaitForSignal(timeline); //prim angry
                    await dm.NextDialogue(0); //enic sent me
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline);
                    break;
            case 6:
                    await WaitForSignal(timeline);
                    await dm.NextDialogue(0); //positions!
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //walkin and checking
                    await dm.NextDialogue(0); //can someone explain??!?!
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline);
                    await dm.NextDialogue(0); //i dont know sir
                    await WaitForSignal(timeline);
                    await dm.NextDialogue(0); //sigh
                    await WaitForSignal(timeline); //outburst
                    await dm.NextDialogue(0); //we need to be ready! 
                    await dm.NextDialogue(0); //with all due respect
                    await WaitForSignal(timeline); //throws hand down by side
                    await dm.NextDialogue(0); //have you forgotten what happened?
                    await WaitForSignal(timeline); //looks up solemnly
                    await dm.NextDialogue(0); //if the might enic couldn't do it
                    await WaitForSignal(timeline); //angry idle
                    await dm.NextDialogue(0); //jade is not to be trifled with
                    await WaitForSignal(timeline); //rogers stands up straighter
                    await dm.NextDialogue(0); //sorry sir youre right
                    await WaitForSignal(timeline); //cyp slumps back down, exhale of exhalation
                    await dm.NextDialogue(0); //dont apologize, youve got a good head on shoulders...
                    await WaitForSignal(timeline); //rogers slouches again, happier
                    await dm.NextDialogue(0); //youre the best general
                    await dm.NextDialogue(0); //a general is nothing w/o
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline); //phyxnic soldier runs from side of screen
                    await dm.NextDialogue(0); //received word!
                    await dm.NextDialogue(0); //...
                    await WaitForSignal(timeline); //cyprus realizes, fear
                    await dm.NextDialogue(0); //!!!
                    await dm.NextDialogue(0); //ENIC ALMIGHTY! THERES A JADE
                    await WaitForSignal(timeline); //turns back to maggots
                    await dm.NextDialogue(0); //QUIT SLACKING!
                    await dm.TextBox(false, 0);
                    await WaitForSignal(timeline);
                    break;
            case 9:
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                BossSpikeCast();
                GameObject.Find("NorthernOverseer").GetComponent<EnemyDisablerCW>().enabled = true;
                GameObject.Find("NorthernOverseer").GetComponent<NorthernOverseerCW>().enabled = true;
                break;
            case 10:
                disableSpikePointSave = false;
                break;
            case 14:
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                spikePoint = SpikeCast();
                //BossSpikeCast
                GameObject.FindWithTag("Cyprus1").GetComponent<Cyprus1CW>().Activate();
                break;
            case 15:
                await WaitForSignal(timeline); //Cyprus helicopter back up at top
                await dm.NextDialogue(0); //WHY WON'T YOU DIE
                await dm.NextDialogue(0); //I'LL KILL YOU
                await WaitForSignal(timeline);
                await dm.NextDialogue(0); //we need to get out of here
                await WaitForSignal(timeline);
                await dm.NextDialogue(0); //my mission is for your sake
                await WaitForSignal(timeline);
                await dm.NextDialogue(0); //I'LL TAKE YOURS
                await WaitForSignal(timeline);
                await dm.NextDialogue(0); //i'm turning around
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                await dm.NextDialogue(0); //...
                await dm.NextDialogue(0); //i took her life
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                if (roomBox.name == "Room6") {
                    roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 0;
                    roomBox.GetComponent<RoomConfigureCW>().isCleared = true;
                    roomBox.GetComponent<RoomConfigureCW>().Clear();
                    clearRepeatedly = 14;
                }
                break;
            case 17:
                await WaitForSignal(timeline); //kids are sitting on the ground, soldiers are standing around, prim walks past the stand, security guard rushes over to stop prim
                await dm.NextDialogue(0); //hey buddy
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline); //guard walks prim back over to stand
                await dm.NextDialogue(0); //they suspect the second jade goliath
                await WaitForSignal(timeline); //security guard back to neutral
                await dm.NextDialogue(0); //the vault is right at the end
                await dm.NextDialogue(0); //just remove the mask
                await WaitForSignal(timeline); //prim uncertain
                await dm.NextDialogue(0); //...
                await WaitForSignal(timeline); //security guard stern, turns around to face phyxnic soldier
                await dm.NextDialogue(0); //remove the mask >:(
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline); //soldier walks over to prim, security guard turns back around
                await dm.NextDialogue(0); //i cannot
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline); //soldier forcibly takes off the mask, guard and soldier alarmed
                await dm.NextDialogue(0); //!!!
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline); //children notice and run away
                await dm.NextDialogue(0); //i am not the jade goliath
                await dm.NextDialogue(0); //ATTACK!
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                float[] posxs = {-207.4f, -211.82f, -206.5f};
                for (int j = 1; j < 4; j++) {
                    GameObject soldier = GameObject.Find((string) "PhyxnicSoldierTag" + j);
                    print("PhyxnicSoldierTag" + j);
                    soldier.GetComponent<PhyxnicSoldierCW>().enabled = true;
                    soldier.GetComponent<EnemyDisablerCW>().enabled = true;
                    soldier.GetComponent<BoxCollider2D>().enabled = true;
                    soldier.GetComponent<EnemyDisablerCW>().savePos = new Vector2(posxs[j-1], 2.25f);
                    soldier.transform.position = new Vector3(posxs[j-1], 2.25f, 0f);
                }
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                await WaitForSignal(timeline);
                break;
            case 21:
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                BossSpikeCast();
                GameObject.Find("TrothController").GetComponent<TrothControllerCW>().TrothFight();
                break;
            case 22:
                disableSpikePointSave = false;
                break;
            case 27:
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                BossSpikeCast();
                GameObject.Find("Cyprus2").GetComponent<EnemyDisablerCW>().enabled = true;
                GameObject.Find("Cyprus2").GetComponent<Cyprus2CW>().enabled = true;
                break;
            case 32:
                await WaitForSignal(timeline);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                await WaitForSignal(timeline);
                spikePoint = SpikeCast();
                SaveDataCW.Load();
                PlayerControllerCW playere = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
                if (Game.loadedGame != null && playere != null) {
                    Game.loadedGame.currentSpikePointx = playere.spikePoint.x-7f;
                    Game.loadedGame.currentSpikePointy = playere.spikePoint.y;
                    disableSpikePointSave = true;
                }
                SaveDataCW.Save();
                GameObject.FindWithTag("Abductor").GetComponent<AbductorCW>().Activate();
                break;
            case 37:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 1;
                dm.Delay(2f, 0, ReplaceWithKeybind("Press %Select% to skip text."));
                //await dm.NextDialogue(2);
                await dm.NextDialogue(4);
                await dm.TextBox(false, 2);
                dm.cancelDelay = true;
                await WaitForSignal(timeline);
                dm.QueueToolTip(ReplaceWithKeybind("Face right and press %Attack% to attack."), true);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Left");
                if (Game.loadedGame.abilitiesUnlocked < -1) {
                    Game.loadedGame.abilitiesUnlocked = -1;
                }
                break;
            case 38:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                //await dm.NextDialogue(2);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                await WaitForSignal(timeline);
                dm.QueueToolTip(ReplaceWithKeybind("Jump and attack to the left in midair."), true);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Right");
                break;
            case 39:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                //await dm.NextDialogue(2);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                await WaitForSignal(timeline);
                dm.QueueToolTip(ReplaceWithKeybind("Hold %Up% while attacking to slash upwards."), true);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Down");
                break;
            case 40:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                await WaitForSignal(timeline);
                dm.QueueToolTip(ReplaceWithKeybind("Hold %Down% and attack in midair to slash downwards."), true);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Up");
                break;
            case 41:
                dm.QueueToolTip("", false);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                break;
            case 42:
                dm.QueueToolTip("", false);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                break;
            case 43:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                await WaitForSignal(timeline);
                roomBox.GetComponent<RoomConfigureCW>().Clear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 0;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Idle");
                break;
            case 44:
                dm.QueueToolTip("", false);
                await WaitForSignal(timeline);
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<EnemyDisablerCW>().health = 8;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 6;
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                GameObject.FindWithTag("Canvas").GetComponent<DialogueManagerCW>().ToolTip("Yellow edges mean you are locked in. Not all rooms with enemies lock.", true);
                await WaitForSignal(timeline);
                break;
            case 45:
                //GameObject.Find("VoiceOrb").GetComponent<SpriteRenderer>().enabled = false;
                //Vector3 origPos = transform.position;
                //transform.position = new Vector3(58.5f, 4f, 0f);
                //sr.enabled = false;
                dm.Delay(1.2f, 1, "");
                dm.QueueToolTip("Your health replenishes at the start of every room.", true);
                await dm.NextDialogue(3);
                //transform.position = origPos;
                //sr.enabled = true;
                await dm.TextBox(false, 2);
                
                dm.QueueToolTip("", false);
                //GameObject.Find("VoiceOrb").GetComponent<SpriteRenderer>().enabled = true;
                break;
            case 46:
                dm.QueueToolTip("", false);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                dm.QueueToolTip(ReplaceWithKeybind("Hold down %Dash% to slow time and release to dash."), true);
                /*slowmo = true;
                dashCancel = false;
                startSlowDash = 1;
                gravityCancel = false;
                slowdownSoundReference = SceneManagerCW.PlaySound("Slowdown", 0.8f);
                smoothTargetAngle = false;*/
                if (Game.loadedGame.abilitiesUnlocked < 0) {
                    Game.loadedGame.abilitiesUnlocked = 0;
                }
                cutscene42Dash = true;
                await RecursiveCutscene40Fun();
                freezePosition = false;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().slowingTime = false;
                animSlowingTime = false;
                waitingForDashCutsceneLand = true;
                cutscene42Dash = false;
                break;
            case 47:
                dm.QueueToolTip("", false);
                await dm.NextDialogue(2);
                await dm.TextBox(false, 2);
                break;
            /*case bad removed:
                dm.QueueToolTip(ReplaceWithKeybind("To cancel your dash after you've held %Dash%, drag to the center of the player."), true);
                cutscene42SavePos = rb.position;
                cutscene42Dash = true;
                await RecursiveCutscene42Fun();
                cutscene42Dash = false;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().slowingTime = false;
                animSlowingTime = false;
                freezePosition = false;
                break;*/
            case 48:
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                break;
            case 49:
                await dm.NextDialogue(0);
                await dm.TextBox(false, 0);
                StaticDataCW.Zoom = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>().zoom;
                changingScene = true;
                int i;
                int level = 0;
                for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
                    sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
                    if (sceneNum != -1) {
                        break;
                    } 
                    level ++;
                }
                StaticDataCW.InWorldMap = true;
                SaveDataCW.Load(); 
                if (Game.loadedGame != null) {
                    Game.loadedGame.inWorldMap = StaticDataCW.InWorldMap;
                }
                SaveDataCW.Save();
                changeSceneStamp = Time.time;
                inputDisable = true;
                inputDisableTimer = StaticDataCW.Time + 0.5f;
                whichBound = 1;
                anim.enabled = false;
                Time.timeScale = 1f;
                slowmo = false;
                slowDashing = false;
                dashTrail.Stop();
                bubbleTrail.Stop();
                superdashing = false;
                dashBuffer = false;
                rb.constraints |= RigidbodyConstraints2D.FreezePosition;
                GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().exiting = true;
                break;
            default:
                break;
        }
    }
    public async Task CutsceneEnd(int id) {
        print(id);
        switch (id) {
            case 9:
                BossSpikeCast();
                GameObject.Find("NorthernOverseer").GetComponent<EnemyDisablerCW>().enabled = true;
                GameObject.Find("NorthernOverseer").GetComponent<NorthernOverseerCW>().enabled = true;
                break;
            case 10:
                disableSpikePointSave = false;
                GameObject.Find("NorthernOverseer").GetComponent<EnemyDisablerCW>().enabled = false;
                GameObject.Find("NorthernOverseer").GetComponent<NorthernOverseerCW>().enabled = false;
                break;
            case 14:
                spikePoint = SpikeCast();
                //BossSpikeCast
                GameObject.FindWithTag("Cyprus1").GetComponent<Cyprus1CW>().Activate();
                break;
            case 15:
                if (roomBox.name == "Room6") {
                    roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 0;
                    roomBox.GetComponent<RoomConfigureCW>().isCleared = true;
                    roomBox.GetComponent<RoomConfigureCW>().Clear();
                    clearRepeatedly = 14;
                }
            case 17:
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                float[] posxs = {-207.4f, -211.82f, -206.5f};
                for (int j = 1; j < 4; j++) {
                    GameObject soldier = GameObject.Find((string) "PhyxnicSoldierTag" + j);
                    print("PhyxnicSoldierTag" + j);
                    soldier.GetComponent<PhyxnicSoldierCW>().enabled = true;
                    soldier.GetComponent<EnemyDisablerCW>().enabled = true;
                    soldier.GetComponent<BoxCollider2D>().enabled = true;
                    soldier.GetComponent<EnemyDisablerCW>().savePos = new Vector2(posxs[j-1], 2.25f);
                    soldier.transform.position = new Vector3(posxs[j-1], 2.25f, 0f);
                }
                break;
            case 21:
                BossSpikeCast();
                GameObject.Find("TrothController").GetComponent<TrothControllerCW>().TrothFight();
                break;
            case 22:
                disableSpikePointSave = false;
                break;
            case 32:
                BossSpikeCast();
                GameObject.FindWithTag("Abductor").GetComponent<AbductorCW>().Activate();
                break;
            case 37:
                dm.QueueToolTip("", false);
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 1;
                dm.QueueToolTip(ReplaceWithKeybind("Face right and press %Attack% to attack."), true);
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Left");
                if (Game.loadedGame.abilitiesUnlocked < -1) {
                    Game.loadedGame.abilitiesUnlocked = -1;
                }
                break;
            case 38:
                dm.QueueToolTip("", false);
                dm.QueueToolTip(ReplaceWithKeybind("Jump and attack to the left in midair."), true);
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 2;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Right");
                break;
            case 39:
                dm.QueueToolTip("", false);
                dm.QueueToolTip(ReplaceWithKeybind("Hold %Up% while attacking to slash upwards."), true);
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 3;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Down");
                break;
            case 40:
                dm.QueueToolTip("", false);
                dm.QueueToolTip(ReplaceWithKeybind("Hold %Down% and attack in midair to slash downwards."), true);
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 4;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Up");
                break;
            case 43:
                dm.QueueToolTip("", false);
                if (roomBox.name == "Room3") {
                    roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 0;
                    roomBox.GetComponent<RoomConfigureCW>().isCleared = true;
                    roomBox.GetComponent<RoomConfigureCW>().Clear();
                    clearRepeatedly = 14;
                }
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 0;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().PlayAnim("Idle");
                break;
            case 44:
                dm.QueueToolTip("", false);
                roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 1;
                roomBox.GetComponent<RoomConfigureCW>().isCleared = false;
                unclearRepeatedly = 14;
                roomBox.GetComponent<RoomConfigureCW>().Unclear();
                GameObject.Find("VoiceOrb").GetComponent<EnemyDisablerCW>().health = 8;
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 6;
                GameObject.FindWithTag("Canvas").GetComponent<DialogueManagerCW>().ToolTip("Yellow edges mean you are locked in. Not all rooms with enemies lock.", true);
                break;
            case 45:
                dm.QueueToolTip("", false);
                if (roomBox.name == "Room4") {
                    roomBox.GetComponent<RoomConfigureCW>().enemiesActive = 0;
                    roomBox.GetComponent<RoomConfigureCW>().isCleared = true;
                    roomBox.GetComponent<RoomConfigureCW>().Clear();
                    clearRepeatedly = 14;
                }
                GameObject.Find("VoiceOrb").GetComponent<VoiceOrbCW>().mode = 0;
                break;
            default:
                break;
        }
    }
}
 