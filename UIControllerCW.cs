using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;

public class UIControllerCW : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerControllerCW playerController;
    private SceneManagerCW sceneManagerScript;
    private CameraControllerCW cameraController;
    public GameObject[] hearts;
    public GameObject[] orbs;
    private float prevhealth;
    private float prevorbs;
    private Sprite fullHeart;
    private Sprite halfHeart;
    private Sprite emptyHeart;
    private Sprite UIOrbSprite;
    private Sprite NoneSprite;
    private Sprite UIOrbNone;
    private Sprite screenFadeSprite;
    private Sprite lockedRoomCover;
    private GameObject continueButton;
    private GameObject returnToMapButton;
    private GameObject returnToMapConfirm;
    private GameObject returnToMapText;
    private GameObject returnToMapBack;
    private GameObject returnToTitleButton;
    private GameObject keyboardOptions;
    private GameObject optionsButton;
    private GameObject volume;
    private GameObject soundFX;
    private GameObject musicButton;
    private GameObject fullscreenScale;
    private GameObject backButton;
    private GameObject fullscreenButton;
    private GameObject pauseUI;
    private GameObject c;
    private GameObject rm;
    private GameObject rmc;
    private GameObject rmt;
    private GameObject rmb;
    private GameObject rt;
    private GameObject ko;
    private GameObject pui;
    private GameObject o;
    private GameObject v;
    private GameObject sfx;
    private GameObject mu;
    private GameObject fss;
    private GameObject b;
    private GameObject fs;
    private GameObject debug;
    private GameObject debugInst;
    private float saveAlpha;
    public GameObject screenFade;
    public GameObject spikeFade;
    private GameObject eventSystem;
    public bool exiting;
    public bool paused;
    private Image sfi;
    private Image sfi2;
    private float enteringStamp;
    private GameObject uiHider;
    public bool uiHiding;
    private float uiHideValue;
    public bool spikeFading;
    public bool showDebug;
    public List<object> commandList;
    private string debugString;
    private PixelPerfectCamera px;
    private GameObject areaLevelText;
    private GameObject lt;
    private GameObject at;
    private float levelTextStamp;
    private float areaTextStamp;
    public static DebugCommand noclip;
    public static DebugCommand<float> noclipspeed;
    public static DebugCommand invis;
    public static DebugCommand invincible;
    public static DebugCommand<int> cpwarp;
    public static DebugCommand reload;
    public static DebugCommand unlockAllLevels;
    public static DebugCommand unlockLevel2;
    public static DebugCommand clearRoom;
    public static DebugCommand returnToLast;
    public static DebugCommand<string> noclipBind;
    public static DebugCommand<string> returnBind;
    public static DebugCommand<string> clearBind;
    private int enteringBuffer;
    public float hideHeartsStamp = -9.0f;
    public bool heartsHidden = false;
    
    
    void Start()
    {
        areaTextStamp = -9.0f;
        levelTextStamp = -9.0f;
        enteringStamp = Time.time;
        heartsHidden = false;
        emptyHeart = Resources.Load<Sprite>("Textures/heartsCW_0");
        halfHeart = Resources.Load<Sprite>("Textures/heartsCW_1");
        fullHeart = Resources.Load<Sprite>("Textures/heartsCW_2");
        UIOrbSprite = Resources.Load<Sprite>("Textures/healthorbs_1");
        NoneSprite = Resources.Load<Sprite>("Textures/NoneSprite");
        UIOrbNone = Resources.Load<Sprite>("Textures/healthorbs_0");
        screenFadeSprite = Resources.Load<Sprite>("Textures/SceneTransitionFade");
        lockedRoomCover = Resources.Load<Sprite>("Textures/LockedRoomCover");
        continueButton = Resources.Load<GameObject>("Prefabs/ContinueButton");
        returnToMapButton = Resources.Load<GameObject>("Prefabs/ReturnToMap");
        returnToMapConfirm = Resources.Load<GameObject>("Prefabs/ReturnToMapConfirm");
        returnToMapText = Resources.Load<GameObject>("Prefabs/ReturnToMapText");
        returnToMapBack = Resources.Load<GameObject>("Prefabs/ReturnToMapBack");
        returnToTitleButton = Resources.Load<GameObject>("Prefabs/ReturnToTitle");
        keyboardOptions = Resources.Load<GameObject>("Prefabs/KeyboardOptions");
        optionsButton = Resources.Load<GameObject>("Prefabs/OptionsButton");
        volume = Resources.Load<GameObject>("Prefabs/VolumeSettings");
        soundFX = Resources.Load<GameObject>("Prefabs/SFXSettings");
        musicButton = Resources.Load<GameObject>("Prefabs/MusicSettings");
        fullscreenScale = Resources.Load<GameObject>("Prefabs/FullscreenScale");
        backButton = Resources.Load<GameObject>("Prefabs/OptionsBackButton");
        fullscreenButton = Resources.Load<GameObject>("Prefabs/FullscreenButton");
        debug = Resources.Load<GameObject>("Prefabs/DebugInput");
        pauseUI = Resources.Load<GameObject>("Prefabs/PauseUI");
        areaLevelText = Resources.Load<GameObject>("Prefabs/AreaLevelText");
        uiHider = Instantiate(Resources.Load("Prefabs/UIHider", typeof(GameObject)) as GameObject);
        uiHideValue = 1f;
        playerController = GameObject.Find("Player").GetComponent<PlayerControllerCW>();
        sceneManagerScript = GameObject.FindWithTag("SceneManager").GetComponent<SceneManagerCW>();
        cameraController = GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>();
        px = GameObject.FindWithTag("MainCamera").GetComponent<PixelPerfectCamera>();
        eventSystem = Instantiate(Resources.Load("Prefabs/EventSystem", typeof(GameObject)) as GameObject);
        hearts = new GameObject[5];
        for (int i = 0; i < 3; i++) {
            hearts[i] = Instantiate(Resources.Load("Prefabs/UIHealingOrb", typeof(GameObject)) as GameObject);
            hearts[i].GetComponent<RectTransform>().SetParent(this.transform);
            if (GameObject.Find("GameController") != null && GameObject.Find("GameController").GetComponent<GameInitializerCW>().level == 5) {
                hearts[i].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            }
            hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(-224.5f + 20f * i, 120f, 0f);
            hearts[i].GetComponent<RectTransform>().sizeDelta = new Vector2(17f, 20f);
            hearts[i].GetComponent<RectTransform>().localScale = Vector3.one;
        }
        /*orbs = new GameObject[5];
        for (int i = 0; i < 5; i++) {
            orbs[i] = Instantiate(Resources.Load("Prefabs/UIHealingOrb", typeof(GameObject)) as GameObject);
            orbs[i].GetComponent<RectTransform>().SetParent(this.transform);
            orbs[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(-224.5f + 20f * i, 77f, 0f);
            orbs[i].GetComponent<RectTransform>().localScale = new Vector3(0.35f, 0.35f, 0.35f);
        }*/
        
        screenFade = Instantiate(Resources.Load("Prefabs/ScreenFade", typeof(GameObject)) as GameObject);
        screenFade.GetComponent<RectTransform>().SetParent(this.transform);
        screenFade.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
        screenFade.GetComponent<RectTransform>().localScale = new Vector3(10f, 10f, 10f);
        sfi = screenFade.GetComponent<Image>();
        sfi.sprite = screenFadeSprite;
        sfi.color = new Color(1f,1f,1f,1f);
        spikeFade = Instantiate(Resources.Load("Prefabs/ScreenFade", typeof(GameObject)) as GameObject);
        spikeFade.GetComponent<RectTransform>().SetParent(this.transform);
        spikeFade.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
        spikeFade.GetComponent<RectTransform>().localScale = new Vector3(10f, 10f, 10f);
        sfi2 = spikeFade.GetComponent<Image>();
        sfi2.sprite = screenFadeSprite;
        sfi2.color = new Color(1f,1f,1f,0f);
        //RefreshOrbs();
        spikeFading = false;
        showDebug = false;
        noclip = new DebugCommand("noclip", "Allows you to fly and pass through walls. Toggles on/off.", "noclip", () => {
            playerController.EnableNoclip();
        });
        noclipspeed = new DebugCommand<float>("ncspeed", "Changes the speed of noclip.", "ncspeed <speed>", (x) => {
            playerController.SetNoclipSpeed(x);
        });
        invis = new DebugCommand("invis", "Makes you invisible.", "invis", () => {
            playerController.ToggleInvisibility();
        });
        invincible = new DebugCommand("invincible", "Makes you invincible.", "invincible", () => {
            playerController.ToggleInvincibility();
        });
        cpwarp = new DebugCommand<int>("cpwarp", "Warps you to a checkpoint in the current scene, based on a number starting at 0.", "cpwarp [checkpoint number]", (x) => {
            playerController.CPWarp(x);
        });
        reload = new DebugCommand("reload", "Reloads the current scene.", "reload", () => {
            playerController.ReloadScene();
        });
        unlockAllLevels = new DebugCommand("unlockall", "Unlocks all levels and checkpoints.", "unlockAll", () => {
            sceneManagerScript.UnlockAllLevels();
        });
        unlockLevel2 = new DebugCommand("unlocklevel2", "Unlocks level 2 automatically.", "unlockLevel2", () => {
            sceneManagerScript.UnlockLevel2();
        });
        clearRoom = new DebugCommand("clear", "Clears the current locked room.", "clear", () => {
            playerController.ClearRoom();
        });
        returnToLast = new DebugCommand("return", "Returns to last saved position.", "return", () => {
            playerController.Return();
        });
        noclipBind = new DebugCommand<string>("noclipbind", "Binds the noclip function to the chosen key. Type \"none\" to clear the keybind.", "noclipbind [key]", (x) => {
            playerController.NoclipBind(x);
        });
        returnBind = new DebugCommand<string>("returnbind", "Binds the return function to the chosen key. Type \"none\" to clear the keybind.", "returnbind [key]", (x) => {
            playerController.ReturnBind(x);
        });
        clearBind = new DebugCommand<string>("clearbind", "Binds the clear function to the chosen key. Type \"none\" to clear the keybind.", "clearbind [key]", (x) => {
            playerController.ClearBind(x);
        });
        commandList = new List<object>
        {
            noclip,
            noclipspeed,
            invis,
            invincible,
            cpwarp,
            reload,
            unlockAllLevels,
            unlockLevel2,
            clearRoom,
            returnToLast,
            noclipBind,
            returnBind,
            clearBind
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!StaticDataCW.Beta) {
            if (Input.GetKeyDown(KeyCode.BackQuote)) {
                if (!showDebug) {
                    showDebug = true;
                    debugInst = Instantiate(debug) as GameObject;
                    debugInst.transform.SetParent(transform);
                    debugInst.transform.localPosition = new Vector3(0f, -100f, 0f);
                    debugInst.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    debugString = debugInst.GetComponent<InputField>().text;
                    debugInst.GetComponent<InputField>().Select();
                } else {
                    showDebug = false;
                    Destroy(debugInst);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Return) && showDebug) {
            HandleDebugInput();
        }
        if (StaticDataCW.Keys["NoclipBind"] != null && Input.GetKeyDown(StaticDataCW.Keys["NoclipBind"])) {
            playerController.EnableNoclip();
        }
        if (StaticDataCW.Keys["ReturnBind"] != null && Input.GetKeyDown(StaticDataCW.Keys["ReturnBind"])) {
            playerController.Return();
        }
        if (StaticDataCW.Keys["ClearBind"] != null && Input.GetKeyDown(StaticDataCW.Keys["ClearBind"])) {
            playerController.ClearRoom();
        }
    }
    void FixedUpdate()
    {
        if (uiHider.transform.parent == null && GameObject.FindWithTag("MainCamera") != null) {
            uiHider.transform.SetParent(GameObject.FindWithTag("MainCamera").transform);
            uiHider.transform.localPosition = new Vector3(-4.5f, 3f, 0f);
        }
        if (playerController.health != prevhealth) {
            Refresh();
        }
        /*if (StaticDataCW.HealingOrbs != prevorbs) {
            RefreshOrbs();
        }*/
        if (GameObject.Find("GameController") != null && GameObject.Find("GameController").GetComponent<GameInitializerCW>().level != 5) {
            for (int i = 0; i < 3; i++) {
                hearts[i].GetComponent<Image>().color = new Color(hearts[i].GetComponent<Image>().color.r, hearts[i].GetComponent<Image>().color.g, hearts[i].GetComponent<Image>().color.b, uiHideValue);
                //orbs[i].GetComponent<Image>().color = new Color(orbs[i].GetComponent<Image>().color.r, orbs[i].GetComponent<Image>().color.g, orbs[i].GetComponent<Image>().color.b, uiHideValue);
            }
        }
        prevhealth = playerController.health;
        //prevorbs = StaticDataCW.HealingOrbs;
        if (!paused) {
            if (exiting) {
                sfi.sprite = screenFadeSprite;
                sfi.color = new Color(sfi.color.r, sfi.color.g, sfi.color.b, sfi.color.a + 0.066f);
                
            } /*else if (Time.time > enteringStamp + 0.3f && PlayerControllerCW.roomBox != null && PlayerControllerCW.roomBox.GetComponent<RoomConfigureCW>().isClearable && !PlayerControllerCW.roomBox.GetComponent<RoomConfigureCW>().isCleared) {
                sfi.color = new Color(sfi.color.r, sfi.color.g, sfi.color.b, 0.4f);
                sfi.sprite = lockedRoomCover;
            }*/ else {
                if (sfi.color.a > 0) {
                    if (sfi.color.a > 0.999f && enteringBuffer < 20) {
                        enteringBuffer++;
                    } else {
                        sfi.color = new Color(sfi.color.r, sfi.color.g, sfi.color.b, sfi.color.a - 0.066f);
                    }
                }
            }
            if (exiting) {
                sfi.sprite = screenFadeSprite;
            }
        }
        if (heartsHidden) {
            if (StaticDataCW.Time < hideHeartsStamp + 0.5f) {
                for (int i = 0; i < 3; i++) {
                    hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(hearts[i].GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(hearts[i].GetComponent<RectTransform>().anchoredPosition.y, 160f, 0.3f), 0f);
                }
            } else {
                for (int i = 0; i < 3; i++) {
                    hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(hearts[i].GetComponent<RectTransform>().anchoredPosition.x, 160f, 0f);
                }
            }
        } else {
            if (StaticDataCW.Time < hideHeartsStamp + 0.5f) {
                for (int i = 0; i < 3; i++) {
                    hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(hearts[i].GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(hearts[i].GetComponent<RectTransform>().anchoredPosition.y, 120f, 0.3f), 0f);
                }
            } else {
                for (int i = 0; i < 3; i++) {
                   hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(hearts[i].GetComponent<RectTransform>().anchoredPosition.x, 120f, 0f);
                }
            }
        }
        uiHiding = uiHider.GetComponent<UIHiderCW>().uiHiding;
        if (uiHiding && uiHideValue > 0.5f) {
            uiHideValue -= 0.1f;
        } else if (!uiHiding && uiHideValue < 1f) {
            uiHideValue += 0.1f;
        }
        if (spikeFading) {
            sfi2.sprite = screenFadeSprite;
            sfi2.color = new Color(sfi2.color.r, sfi2.color.g, sfi2.color.b, sfi2.color.a + 0.066f);
        } else {
            if (sfi2.color.a > 0) {
                sfi2.color = new Color(sfi2.color.r, sfi2.color.g, sfi2.color.b, sfi2.color.a - 0.066f);
            }
        }
        if (sceneManagerScript.waitingForLevelText) {
            sceneManagerScript.waitingForLevelText = false;
            int sceneNum;
            int level;
            for (level = 0; level < StaticDataCW.AllScenes.Length; level++) {
                sceneNum = StaticDataCW.AllScenes[level].IndexOf(SceneManager.GetActiveScene().name);
                if (sceneNum != -1) {
                    break;
                } 
            }
            LevelText(level);
        }
        if (sceneManagerScript.waitingForAreaText) {
            int sceneNum;
            int totalSceneNum = 0;
            for (int i = 0; i < StaticDataCW.AllScenes.Length; i++) {
                sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
                if (sceneNum != -1) {
                    totalSceneNum += sceneNum;
                    break;
                } else {
                    totalSceneNum += StaticDataCW.AllScenes[i].Count;
                }
            }
            sceneManagerScript.waitingForAreaText = false;
            AreaText(totalSceneNum);
        }
        /*if (lt != null && StaticDataCW.Time < levelTextStamp + 6f) {
            if (StaticDataCW.Time < levelTextStamp + 2f) {
                lt.GetComponent<Text>().color = new Color(lt.GetComponent<Text>().color.r, lt.GetComponent<Text>().color.g, lt.GetComponent<Text>().color.b, lt.GetComponent<Text>().color.a + 0.5f * Time.fixedDeltaTime);
            } else if (StaticDataCW.Time > levelTextStamp + 4f) {
                lt.GetComponent<Text>().color = new Color(lt.GetComponent<Text>().color.r, lt.GetComponent<Text>().color.g, lt.GetComponent<Text>().color.b, lt.GetComponent<Text>().color.a - 0.5f * Time.fixedDeltaTime);
            }
        } else if (lt != null) {
            Destroy(lt);
        }
        if (at != null && StaticDataCW.Time < areaTextStamp + 6f) {
            if (StaticDataCW.Time < areaTextStamp + 2f) {
                at.GetComponent<Text>().color = new Color(lt.GetComponent<Text>().color.r, lt.GetComponent<Text>().color.g, lt.GetComponent<Text>().color.b, at.GetComponent<Text>().color.a + 0.5f * Time.fixedDeltaTime);
            } else if (StaticDataCW.Time > areaTextStamp + 4f) {
                at.GetComponent<Text>().color = new Color(lt.GetComponent<Text>().color.r, lt.GetComponent<Text>().color.g, lt.GetComponent<Text>().color.b, at.GetComponent<Text>().color.a - 0.5f * Time.fixedDeltaTime);
            }
        } else if (at != null) {
            Destroy(at);
        }*/
    }
    private void HandleDebugInput () {
        string[] properties = debugInst.GetComponent<InputField>().text.Split(' ');
        for (int i = 0; i < commandList.Count; i++) {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;
            if (properties[0] == (commandBase.commandId)) {
                if (commandList[i] as DebugCommand != null) {
                    (commandList[i] as DebugCommand).Invoke();
                } else if (commandList[i] as DebugCommand<int> != null) {
                    if (properties.Length > 1) {
                        (commandList[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                    } else {
                        (commandList[i] as DebugCommand<int>).Invoke(0);
                    }
                } else if (commandList[i] as DebugCommand<float> != null) {
                    if (properties.Length > 1) {
                        (commandList[i] as DebugCommand<float>).Invoke(float.Parse(properties[1]));
                    } else {
                        (commandList[i] as DebugCommand<float>).Invoke(0);
                    }
                } else if (commandList[i] as DebugCommand<string> != null) {
                    if (properties.Length > 1) {
                        (commandList[i] as DebugCommand<string>).Invoke(properties[1]);
                    } else {
                        (commandList[i] as DebugCommand<string>).Invoke("null_string");
                    }
                }
            }
        }
        debugInst.GetComponent<InputField>().text = "";
    }
    void Refresh()
    {
        for (int i = 0; i < 3; i++) {
            if (playerController.health >= i+1) {
                hearts[i].GetComponent<Image>().sprite = UIOrbSprite;
            } else {
                hearts[i].GetComponent<Image>().sprite = UIOrbNone;
            }
        }
    }
    public void LevelText(int level) {
        print("spawningLevelText");
        List<string> levelNames = new List<string>() {"Prologue", "Elluir Peak", "Alvoet Nature Reserve"};
        if (levelNames[level] != null) {
            print("yup");
            lt = Instantiate(areaLevelText) as GameObject;
            print(lt.transform.localPosition);
            lt.transform.SetParent(transform);
            lt.transform.localPosition = new Vector3(0f,80f,0f);
            lt.GetComponent<Text>().color = new Color(138f, 43f, 226f, 0f);
            lt.GetComponent<Text>().fontSize = 100;
            lt.GetComponent<Text>().text = levelNames[level];
            levelTextStamp = StaticDataCW.Time;
        }
    }

    public void AreaText(int sceneNum) {
        Dictionary<int, string> areaNames = new Dictionary<int, string>() {{1, "Foothills"}, {5, "Village"}, {6, "Frozen Tunnel"}, {9, "Ascent"}, {11, "Canopy"}, {14, "Forest Floor"}, {17, "Canyon"}, {18, "Laboratory"}, {21, "Chasm"}};
        if (areaNames[sceneNum] != null) {
            at = Instantiate(areaLevelText) as GameObject;
            at.transform.SetParent(transform);
            at.transform.localPosition = new Vector3(0f,35f,0f);
            at.GetComponent<Text>().color = new Color(138f, 43f, 226f, 0f);
            at.GetComponent<Text>().fontSize = 50;
            at.GetComponent<Text>().text = areaNames[sceneNum];
            areaTextStamp = StaticDataCW.Time;
        }
    }
   /*void RefreshOrbs()
    {
        for (int i = 0; i < 5; i++) {
            if (StaticDataCW.HealingOrbs >= (i+1)) {
                orbs[i].GetComponent<Image>().sprite = UIOrbSprite;
            } else {
                orbs[i].GetComponent<Image>().sprite = UIOrbNone;
            }
        }
    }*/
    public void Pause()
    {
        paused = true;
        saveAlpha = sfi.color.a;
        sfi.sprite = screenFadeSprite;
        sfi.color = new Color(sfi.color.r, sfi.color.g, sfi.color.b, 0.2f);
        c = Instantiate(continueButton) as GameObject;
        SaveDataCW.Load();
        rm = Instantiate(returnToMapButton) as GameObject;
        rmc = Instantiate(returnToMapConfirm) as GameObject;
        rmt = Instantiate(returnToMapText) as GameObject;
        rmb = Instantiate(returnToMapBack) as GameObject;
        rm.transform.SetParent(transform);
        rmc.transform.SetParent(transform);
        rmt.transform.SetParent(transform);
        rmb.transform.SetParent(transform);
        rm.transform.localPosition = new Vector3(0f,-46.66f,0f);
        rmc.transform.localPosition = new Vector3(-60f,-100f,0f);
        rmt.transform.localPosition = new Vector3(0f,0f,0f);
        rmb.transform.localPosition = new Vector3(60f,-100f,0f);
        rm.GetComponent<Button>().onClick.AddListener(ReturnToMapDoubleCheck);
        rmc.GetComponent<Button>().onClick.AddListener(delegate{sceneManagerScript.ReturnToMainMenu(true);});
        rmb.GetComponent<Button>().onClick.AddListener(BackToPauseMenu);
        if (rm.transform.GetSiblingIndex() != 0) {
            screenFade.transform.SetSiblingIndex(rm.transform.GetSiblingIndex() - 1);
        } else {
            screenFade.transform.SetAsFirstSibling();
        }
        if (Game.loadedGame.levelsUnlocked > 0) {
            rm.SetActive(true);
        } else {
            rm.SetActive(false);
        }
        rmc.SetActive(false);
        rmt.SetActive(false);
        rmb.SetActive(false);
        rmt.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        rt = Instantiate(returnToTitleButton) as GameObject;
        pui = Instantiate(pauseUI) as GameObject;
        ko = Instantiate(keyboardOptions) as GameObject;
        o = Instantiate(optionsButton) as GameObject;
        v = Instantiate(volume) as GameObject;
        sfx = Instantiate(soundFX) as GameObject;
        mu = Instantiate(musicButton) as GameObject;
        fss = Instantiate(fullscreenScale) as GameObject;
        b = Instantiate(backButton) as GameObject;
        fs = Instantiate(fullscreenButton) as GameObject;
        c.transform.SetParent(transform);
        rt.transform.SetParent(transform);
        pui.transform.SetParent(transform);
        ko.transform.SetParent(transform);
        o.transform.SetParent(transform);
        v.transform.SetParent(transform);
        sfx.transform.SetParent(transform);
        mu.transform.SetParent(transform);
        fss.transform.SetParent(transform);
        b.transform.SetParent(transform);
        fs.transform.SetParent(transform);
        c.transform.localPosition = new Vector3(0f,60f,0f);
        if (Game.loadedGame.levelsUnlocked > 0) {
            rt.transform.localPosition = new Vector3(0f,-100f,0f);
            o.transform.localPosition = new Vector3(0f,6.66f,0f);
            rm.SetActive(true);
        } else {
            rt.transform.localPosition = new Vector3(0f,-60f,0f);
            o.transform.localPosition = new Vector3(0f,0f,0f);
            rm.SetActive(false);
        }
        ko.transform.localPosition = new Vector3(160f,0f,0f);
        pui.transform.localPosition = new Vector3(0f,0f,0f);
        v.transform.localPosition = new Vector3(0f,10f,0f);
        sfx.transform.localPosition = new Vector3(0f,-20f,0f);
        mu.transform.localPosition = new Vector3(0f,-45f,0f);
        fss.transform.localPosition = new Vector3(0f,-80f,0f);
        b.transform.localPosition = new Vector3(0f,-120f,0f);
        fs.transform.localPosition = new Vector3(0f,50f,0f);
        c.GetComponent<Button>().onClick.AddListener(sceneManagerScript.Pause);
        rt.GetComponent<Button>().onClick.AddListener(delegate{sceneManagerScript.ReturnToMainMenu(false);});
        int secrets = 0;
        SaveDataCW.Load();
        foreach (bool check in Game.loadedGame.secretsCollected) {
            if (check) {
                secrets ++;
            }
        }

        ko.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        pui.GetComponentsInChildren<Text>()[1].text = "Amethysts: " + secrets + "/35";
        pui.transform.localScale = new Vector3(1f, 1f, 1f);
        o.GetComponent<Button>().onClick.AddListener(Options);
        v.GetComponent<Slider>().onValueChanged.AddListener(delegate {ChangeVolume();});
        v.GetComponent<Slider>().value = (int) (SceneManagerCW.volume * 100);
        v.GetComponentsInChildren<Text>()[0].text = "Master Volume: " + ((int)  (SceneManagerCW.volume * 100)).ToString();
        v.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        sfx.GetComponent<Slider>().onValueChanged.AddListener(delegate {ChangeSFXVolume();});
        sfx.GetComponent<Slider>().value = (int) (SceneManagerCW.sfxVolume * 100);
        sfx.GetComponentsInChildren<Text>()[0].text = "SFX Volume: " + ((int)  (SceneManagerCW.sfxVolume * 100)).ToString();
        sfx.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        mu.GetComponent<Slider>().onValueChanged.AddListener(delegate {ChangeMusicVolume();});
        mu.GetComponent<Slider>().value = (int) (SceneManagerCW.musicVolume * 100);
        mu.GetComponentsInChildren<Text>()[0].text = "Music Volume: " + ((int)  (SceneManagerCW.musicVolume * 100)).ToString();
        mu.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        fss.GetComponent<Button>().onClick.AddListener(ToggleWindowScale);
        if (Screen.fullScreenMode == FullScreenMode.Windowed) {
            fss.GetComponentsInChildren<Text>()[0].text = "Window Scale: " + cameraController.zoom.ToString();
        } else {
            fss.GetComponentsInChildren<Text>()[0].text = "In Fullscreen";
        }
        b.GetComponent<Button>().onClick.AddListener(BackToPauseMenu);
        fs.GetComponent<Button>().onClick.AddListener(ToggleFullscreen);
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) {
            fs.GetComponentsInChildren<Text>()[0].text = "Exit Fullscreen";
        } else {
            fs.GetComponentsInChildren<Text>()[0].text = "Enter Fullscreen";
        }
        v.SetActive(false);
        ko.SetActive(false);
        sfx.SetActive(false);
        mu.SetActive(false);
        fss.SetActive(false);
        b.SetActive(false);
        fs.SetActive(false);
        if (at != null) {
            at.SetActive(false);
        }
        if (lt != null) {
            lt.SetActive(false);
        }
    }
    public void Unpause()
    {
        paused = false;
        sfi.color = new Color(sfi.color.r, sfi.color.g, sfi.color.b, saveAlpha);
        Destroy(c);
        if (rm != null) {
            Destroy(rm);
        }
        Destroy(rt);
        Destroy(pui);
        Destroy(o);
        Destroy(v);
        Destroy(rmc);
        Destroy(rmt);
        Destroy(rmb);
        Destroy(ko);
        Destroy(sfx);
        Destroy(mu);
        Destroy(fss);
        Destroy(b);
        Destroy(fs);
        if (at != null) {
            at.SetActive(true);
        }
        if (lt != null) {
            lt.SetActive(true);
        }
    }
    public void Options()
    {
        c.SetActive(false);
        rt.SetActive(false);
        o.SetActive(false);
        rm.SetActive(false);
        rmc.SetActive(false);
        rmt.SetActive(false);
        rmb.SetActive(false);
        v.SetActive(true);
        ko.SetActive(true);
        fss.SetActive(true);
        b.SetActive(true);
        fs.SetActive(true);
        sfx.SetActive(true);
        mu.SetActive(true);
    }

    public void ReturnToMapDoubleCheck()
    {
        c.SetActive(false);
        rt.SetActive(false);
        o.SetActive(false);
        rm.SetActive(false);
        rmc.SetActive(true);
        rmt.SetActive(true);
        rmb.SetActive(true);
        v.SetActive(false);
        ko.SetActive(false);
        fss.SetActive(false);
        b.SetActive(false);
        fs.SetActive(false);
        sfx.SetActive(false);
        mu.SetActive(false);
    }
    public void BackToPauseMenu(){
        c.SetActive(true);
        rt.SetActive(true);
        o.SetActive(true);
        rm.SetActive(true);
        rmc.SetActive(false);
        rmt.SetActive(false);
        rmb.SetActive(false);
        v.SetActive(false);
        ko.SetActive(false);
        fss.SetActive(false);
        b.SetActive(false);
        fs.SetActive(false);
        sfx.SetActive(false);
        mu.SetActive(false);
        if (Game.loadedGame.levelsUnlocked > 0) {
            rt.transform.localPosition = new Vector3(0f,-100f,0f);
            o.transform.localPosition = new Vector3(0f,6.66f,0f);
            rm.SetActive(true);
        } else {
            rt.transform.localPosition = new Vector3(0f,-60f,0f);
            o.transform.localPosition = new Vector3(0f,0f,0f);
            rm.SetActive(false);
        }
    }
    public void ToggleFullscreen() {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fs.GetComponentsInChildren<Text>()[0].text = "Enter Fullscreen";
            if (cameraController.zoom == 1 || cameraController.zoom == 2 || cameraController.zoom == 3) {
                fss.GetComponentsInChildren<Text>()[0].text = "Window Scale: " + cameraController.zoom.ToString();
            } else {
                fss.GetComponentsInChildren<Text>()[0].text = "Window Scale: MAX";
            }
        } else {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            fs.GetComponentsInChildren<Text>()[0].text = "Exit Fullscreen";
            fss.GetComponentsInChildren<Text>()[0].text = "In Fullscreen";
        }
    }
    public void ChangeVolume() {
        //old ratio: (SceneManagerCW.volume < 0.0001f && v.GetComponent<Slider>().value != 0)? -1f: (v.GetComponent<Slider>().value / 100f)/SceneManagerCW.volume
        SceneManagerCW.volume = v.GetComponent<Slider>().value / 100f;
        PlayerPrefs.SetFloat("Volume", SceneManagerCW.volume);
        v.GetComponentsInChildren<Text>()[0].text = "Master Volume: " + v.GetComponent<Slider>().value.ToString();
        SceneManagerCW.ChangeVolume(2);
    }
    public void ChangeSFXVolume() {
        SceneManagerCW.sfxVolume = sfx.GetComponent<Slider>().value / 100f;
        PlayerPrefs.SetFloat("SFXVolume", SceneManagerCW.sfxVolume);
        sfx.GetComponentsInChildren<Text>()[0].text = "SFX Volume: " + sfx.GetComponent<Slider>().value.ToString();
        SceneManagerCW.ChangeVolume(1);
    }
    public void ChangeMusicVolume() {
        SceneManagerCW.musicVolume = mu.GetComponent<Slider>().value / 100f;
        PlayerPrefs.SetFloat("MusicVolume", SceneManagerCW.musicVolume);
        mu.GetComponentsInChildren<Text>()[0].text = "Music Volume: " + mu.GetComponent<Slider>().value.ToString();
        SceneManagerCW.ChangeVolume(0);
    }
    public void ToggleWindowScale() {
        if (Screen.fullScreenMode == FullScreenMode.Windowed) {
            if (cameraController.zoom < 8) {
                cameraController.SetZoom(cameraController.zoom + 1);
            } else {
                cameraController.SetZoom(1);
            }
            fss.GetComponentsInChildren<Text>()[0].text = "Window Scale: " + cameraController.zoom.ToString();
        }
    }
    public async void HideHearts() {
        hideHeartsStamp = StaticDataCW.Time;
        heartsHidden = true;
    }
    public async void ShowHearts() {
        hideHeartsStamp = StaticDataCW.Time;
        heartsHidden = false;
    }
}
