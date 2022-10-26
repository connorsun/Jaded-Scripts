using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SceneManagerCW : MonoBehaviour
{
    // Start is called before the first frame update
    public bool dying;
    private Scene prevScene;
    private int sceneNum;
    private AsyncOperation[] scenesLoaded = new AsyncOperation[3];
    private string[] scenesLoadedNames = new string[3];
    public static bool freeze = false;
    public float freezeStamp;
    public float freezeTime;
    private List<GameObject> freezable;
    private List<ScriptsFreezable> scriptsFreezable;
    private List<GameObject> bcfreezable;
    private List<GameObject> animfreezable;
    private float saveTimeScale;
    private float saveTimeAtFreeze;
    private float addTimeDiff;
    public float timeDifference = 0f;
    private bool negateTimedFreeze;
    public bool unpausable;
    public static List<GameObject> soundModules;
    public static GameObject[] musicModules = new GameObject[4];
    private static AudioClip[] allSounds;
    private static string[] allSoundNames;
    private static int[] allSoundCounter;
    private static AudioClip[] allMusic;
    private static SongData[] allMusicNames;
    private static AudioMixerGroup SFX;
    public static float volume;
    public static float sfxVolume;
    public static float musicVolume;
    public bool waitingForLevelText;
    public bool waitingForAreaText;
    public float unPauseStamp = -9.0f;
    public static bool paused;
    private static int enterSceneMuteFrames = 0;
    public static int maxModules = 75;
    private static int activeMusicSession;

    void Awake()
    {
        paused = false;
        if (StaticDataCW.Keys == null || StaticDataCW.Keys.Count == 0) {
            ReloadKeybinds();
        }

        SceneManagerCW.allSounds = Resources.LoadAll<AudioClip>("Sounds");
        SceneManagerCW.allSoundNames = new string[allSounds.Length];
        for (int i = 0; i < allSounds.Length; i++) {
            allSoundNames[i] = allSounds[i].ToString();
            //substring gets rid of pesky "(unityengine.audioclip)"
            allSoundNames[i] = allSoundNames[i].Substring(0, allSoundNames[i].Length - 24);
        }
        DontDestroyOnLoad(this.gameObject);
        SceneManagerCW.SFX = Resources.Load<AudioMixerGroup>("Sounds/SFX");
        SceneManagerCW.soundModules = new List<GameObject>();
        for (int i = 0; i < SceneManagerCW.maxModules; i++) {
            GameObject soundModule = new GameObject();
            soundModule.name = "Sound Module " + i;
            soundModule.tag = "SoundModule";
            soundModule.AddComponent<AudioSource>();
            soundModule.GetComponent<AudioSource>().playOnAwake = false;
            soundModule.GetComponent<AudioSource>().outputAudioMixerGroup = SceneManagerCW.SFX;
            soundModule.AddComponent<ModuleCW>();
            SceneManagerCW.soundModules.Add(soundModule);
            DontDestroyOnLoad(soundModule);
            soundModule.transform.parent = this.transform;
        }

        SceneManagerCW.allMusic = Resources.LoadAll<AudioClip>("Music");
        SceneManagerCW.allMusicNames = new SongData[allMusic.Length];
        
        for (int i = 0; i < allMusic.Length; i++) {
            allMusicNames[i].name = allMusic[i].ToString();
            //print(allMusicNames[i].name);
            //substring gets rid of pesky "(unityengine.audioclip)"
            allMusicNames[i].name = allMusicNames[i].name.Substring(0, allMusicNames[i].name.Length - 24);
            //print(allMusicNames[i].name);

            string num = string.Empty;
            int bpm;

            for (int j = 0; j < allMusicNames[i].name.Length; j++)
            {
                if (char.IsDigit(allMusicNames[i].name[j]))
                    num += allMusicNames[i].name[j];
            }

            if (num.Length > 0) {
                bpm = int.Parse(num);
                if (allMusicNames[i].name.Trim(' ') == "Ice Cave") {
                    allMusicNames[i].tail = 3.0 * (1.0 / (double)bpm) * 60.0;
                } {
                    allMusicNames[i].tail = 4.0 * (1.0 / (double)bpm) * 60.0;
                }
            }

            string newName = string.Empty;

            for (int e = 0; e < allMusicNames[i].name.Length; e++)
            {
                if (!char.IsDigit(allMusicNames[i].name[e]))
                    newName += allMusicNames[i].name[e];
            }

            newName = newName.Trim(' ');

            allMusicNames[i].name = newName;
        }

        for (int i = 0; i < 4; i++) {
            musicModules[i] = new GameObject();
            musicModules[i].name = "Music Module " + i;
            musicModules[i].tag = "SoundModule";
            musicModules[i].AddComponent<AudioSource>();
            musicModules[i].GetComponent<AudioSource>().playOnAwake = false;
            musicModules[i].GetComponent<AudioSource>().outputAudioMixerGroup = SceneManagerCW.SFX;
            musicModules[i].AddComponent<ModuleCW>();
            DontDestroyOnLoad(musicModules[i]);
            musicModules[i].transform.parent = this.transform;
        }
    }
    void Start()
    {
        //moved allscenes initializer to gameinitializercw
        volume = PlayerPrefs.GetFloat("Volume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        StaticDataCW.InWorldMap = false;
        /*if (StaticDataCW.EnemiesLoaded == null) {
            StaticDataCW.EnemiesLoaded = new List<Enemy>();
        }*/
        SaveDataCW.Load();
        Game.loadedGame.inWorldMap = false;
        SaveDataCW.Save();

        unpausable = false;
        //SceneManagerCW.PlayMusic("opening_chords", 1f, false, 0f);
        
    }
    void OnNewScene() {
        /*if (StaticDataCW.CurrentCheckpointScene != null && StaticDataCW.CurrentCheckpointScene != SceneManager.GetActiveScene().name) {
            scenesLoaded[0] = SceneManager.LoadSceneAsync(StaticDataCW.CurrentCheckpointScene, LoadSceneMode.Additive);
            scenesLoadedNames[0] = StaticDataCW.CurrentCheckpointScene;
            if (scenesLoaded[0] != null) {
                scenesLoaded[0].allowSceneActivation = false;
            }
        }*/
        paused = false;
        enterSceneMuteFrames = 10;
        int i;
        int totalSceneNum = 0;
        for (i = 0; i < StaticDataCW.AllScenes.Length; i++) {
            sceneNum = StaticDataCW.AllScenes[i].IndexOf(SceneManager.GetActiveScene().name);
            if (sceneNum != -1) {
                totalSceneNum += sceneNum;
                break;
            } else {
                totalSceneNum += StaticDataCW.AllScenes[i].Count;
            }
        }
        SaveDataCW.Load();
        Game.loadedGame.currentScene = SceneManager.GetActiveScene().name;
        if (totalSceneNum > Game.loadedGame.furthestScene) {
            Game.loadedGame.furthestScene = totalSceneNum;
            print("new scene");
            //waitingForAreaText = true;
        } else {
            //waitingForAreaText = false;
        }
        SaveDataCW.Save();
        /*if (sceneNum == -1) {
            print("error- current scene not in static array");
        } else if (sceneNum == 0) {
            print("sceneNumZero");
            //waitingForLevelText = true;
            if (sceneNum != StaticDataCW.AllScenes[i].Count-1) {
                scenesLoaded[1] = SceneManager.LoadSceneAsync(StaticDataCW.AllScenes[i][sceneNum+1], LoadSceneMode.Additive);
                scenesLoadedNames[1] = StaticDataCW.AllScenes[i][sceneNum+1];
                if (scenesLoaded[1] != null) {
                    scenesLoaded[1].allowSceneActivation = false;
                }
            }
        } else if (sceneNum == StaticDataCW.AllScenes[i].Count-1) {
            //waitingForLevelText = false;
            scenesLoaded[2] = SceneManager.LoadSceneAsync(StaticDataCW.AllScenes[i][sceneNum-1], LoadSceneMode.Additive);
            scenesLoadedNames[2] = StaticDataCW.AllScenes[i][sceneNum-1];
            if (scenesLoaded[2] != null) {
                scenesLoaded[2].allowSceneActivation = false;
            }
        } else {
            //waitingForLevelText = false;
            scenesLoaded[1] = SceneManager.LoadSceneAsync(StaticDataCW.AllScenes[i][sceneNum+1], LoadSceneMode.Additive);
            scenesLoadedNames[1] = StaticDataCW.AllScenes[i][sceneNum+1];
            if (scenesLoaded[1] != null) {
                scenesLoaded[1].allowSceneActivation = false;
            }
            scenesLoaded[2] = SceneManager.LoadSceneAsync(StaticDataCW.AllScenes[i][sceneNum-1], LoadSceneMode.Additive);
            scenesLoadedNames[2] = StaticDataCW.AllScenes[i][sceneNum-1];
            if (scenesLoaded[2] != null) {
                scenesLoaded[2].allowSceneActivation = false;
            }
        }*/
        GameObject.FindWithTag("MainCamera").GetComponent<CameraControllerCW>().SetPosition();
        GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>().SetPosition();
        /*GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in allEnemies) {
            if (!SceneManagerCW.isEnemyLoaded(enemy)) {
                StaticDataCW.EnemiesLoaded.Add(new Enemy(enemy.transform.position, false));
            }
        }*/
        //SceneManagerCW.QueueSong(Random.Range(1,3)==1?"opening_chords":"allala it is 12 o clock - 6_3_21", 0.25f);
    }

    public void ReloadKeybinds() {
        StaticDataCW.Keys = new Dictionary<string, KeyCode>();
            StaticDataCW.Keys.Add("Jump",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump", "Space")));
            StaticDataCW.Keys.Add("Dash",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Dash", "Mouse1")));
            StaticDataCW.Keys.Add("Attack",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Attack", "Mouse0")));
            StaticDataCW.Keys.Add("Up",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Up", "W")));
            StaticDataCW.Keys.Add("Down",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Down", "S")));
            StaticDataCW.Keys.Add("Left",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Left", "A")));
            StaticDataCW.Keys.Add("Right",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Right", "D")));
            //StaticDataCW.Keys.Add("Heal",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Heal", "E")));
            StaticDataCW.Keys.Add("Special",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Special", "LeftShift")));
            StaticDataCW.Keys.Add("Select",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Select", "Space")));
            StaticDataCW.Keys.Add("Back",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Select", "Escape")));
            StaticDataCW.Keys.Add("NoclipBind",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("NoclipBind", "None")));
            StaticDataCW.Keys.Add("ReturnBind",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ReturnBind", "None")));
            StaticDataCW.Keys.Add("ClearBind",(KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ClearBind", "None")));
    }

    public static void ChangeVolume(/*float ratio,*/ int musicsoundboth) {
        if (musicsoundboth == 0 || musicsoundboth == 2) {
            foreach (GameObject module in musicModules) {
                module.GetComponent<AudioSource>().volume = module.GetComponent<ModuleCW>().volume*SceneManagerCW.volume*SceneManagerCW.musicVolume;
            }
        }
        if (musicsoundboth == 1 || musicsoundboth == 2) {
            foreach (GameObject module in soundModules) {
                module.GetComponent<AudioSource>().volume = module.GetComponent<ModuleCW>().volume*SceneManagerCW.volume*SceneManagerCW.sfxVolume;
            }
        }
        /*if (musicsoundboth == 0 || musicsoundboth == 2) {
            foreach (GameObject module in musicModules) {
                if (ratio < 0.0001f && ratio > -0.0001f) {
                    module.GetComponent<ModuleCW>().holdingData = true;
                    module.GetComponent<ModuleCW>().holdingValue = module.GetComponent<AudioSource>().volume;
                }
                if (ratio < -0.05f) {
                    module.GetComponent<ModuleCW>().holdingData = false;
                    module.GetComponent<AudioSource>().volume = module.GetComponent<ModuleCW>().holdingValue;
                } else {
                    module.GetComponent<AudioSource>().volume = module.GetComponent<AudioSource>().volume*ratio;
                }
            }
        }
        if (musicsoundboth == 1 || musicsoundboth == 2) {
            foreach (GameObject module in soundModules) {
                if (ratio < 0.0001f && ratio > -0.0001f) {
                    module.GetComponent<ModuleCW>().holdingData = true;
                    module.GetComponent<ModuleCW>().holdingValue = module.GetComponent<AudioSource>().volume;
                }
                if (ratio < -0.05f) {
                    module.GetComponent<ModuleCW>().holdingData = false;
                    module.GetComponent<AudioSource>().volume = module.GetComponent<ModuleCW>().holdingValue;
                } else {
                    module.GetComponent<AudioSource>().volume = module.GetComponent<AudioSource>().volume*ratio;
                }
            }
        }*/
    }

    public static async Task QueueSong(string music, float volumeCoefficient) {
        string[] musicNames = new string[SceneManagerCW.allMusicNames.Length];
        for (int i = 0; i < SceneManagerCW.allMusicNames.Length; i++) {
            musicNames[i] = SceneManagerCW.allMusicNames[i].name;
            print(musicNames[i] + " " + music);
        }
        
        if (System.Array.IndexOf(musicNames, music) != -1) {
            print("huuh");
            AudioClip musicClip = SceneManagerCW.allMusic[System.Array.IndexOf(musicNames, music)];
            SongData songData = SceneManagerCW.allMusicNames[System.Array.IndexOf(musicNames, music)];
            print(musicClip);
            GameObject newmodule = null;
            GameObject oldmodule = null;
            bool found = false;
            for (int i = 0; i < musicModules.Length; i++) {
                if (musicModules[i].GetComponent<AudioSource>().isPlaying) {
                    oldmodule = musicModules[i];
                    if (i == musicModules.Length - 1) {
                        newmodule = musicModules[0];
                    } else {
                        newmodule = musicModules[i+1];
                    }
                    found = true;
                }
            }
            if (!found) {
                oldmodule = musicModules[0];
                newmodule = musicModules[1];
            }
            //if (musicModules[0].GetComponent<AudioSource>().isPlaying) {newmodule = musicModules[1]; oldmodule = musicModules[0];} else {newmodule = musicModules[0]; oldmodule = musicModules[1];}

            print("Queueing: " + musicClip.name);
            print("Fading out: " + oldmodule);
            print("Playing on: " + newmodule);
            print("Which is currently playing: " + newmodule.GetComponent<AudioSource>().clip);
            if (oldmodule.GetComponent<AudioSource>().clip == null || oldmodule.GetComponent<AudioSource>().clip.name != musicClip.name) {
                activeMusicSession++;
                if (activeMusicSession > 1000) {
                    activeMusicSession = 0;
                }
                newmodule.GetComponent<AudioSource>().clip = musicClip;
                newmodule.GetComponent<AudioSource>().volume = 0f;
                newmodule.GetComponent<ModuleCW>().volume = 0f;
                newmodule.GetComponent<AudioSource>().PlayScheduled(AudioSettings.dspTime);
                double initialStamp = AudioSettings.dspTime;
                while (oldmodule.GetComponent<AudioSource>().volume > 0.005f) {
                    oldmodule.GetComponent<AudioSource>().volume -= 0.005f;
                    newmodule.GetComponent<AudioSource>().volume += SceneManagerCW.volume * SceneManagerCW.musicVolume * volumeCoefficient * 0.005f;
                    await new WaitForFixedUpdate();
                }
                newmodule.GetComponent<AudioSource>().volume = SceneManagerCW.volume * SceneManagerCW.musicVolume * volumeCoefficient;
                newmodule.GetComponent<ModuleCW>().volume = volumeCoefficient;
                oldmodule.GetComponent<AudioSource>().volume = 0f;
                oldmodule.GetComponent<ModuleCW>().volume = 0f;
                oldmodule.GetComponent<AudioSource>().Stop();
                PlayMusic(musicClip, volumeCoefficient, initialStamp, songData.tail);
            }
        }
    }

    public static async Task PlayMusic(AudioClip musicClip, float volumeCoefficient, double newStartTime, double tail) {
        int toggle = 0;
        /*if (musicModules[0].GetComponent<AudioSource>().isPlaying) {
            toggle = 1;
        } else {
            toggle = 0;
        }*/
        for (int i = 0; i < musicModules.Length; i++) {
            if (musicModules[i].GetComponent<AudioSource>().isPlaying) {
                if (i == musicModules.Length - 1) {
                    toggle = 0;
                } else {
                    toggle = i+1;
                }
            }
        }
        double duration = ((double)musicClip.samples / musicClip.frequency) - tail;
        double nextStartTime = newStartTime + duration;
        activeMusicSession++;
        int thisActiveMusicSession = activeMusicSession;
        if (activeMusicSession > 1000) {
            activeMusicSession = 0;
        }

        double waitTime = 0;

        if (Mathf.Max((float)(nextStartTime - AudioSettings.dspTime - 0.5f), 0) != 0) {
            waitTime = nextStartTime - AudioSettings.dspTime;
        }
        
        await new WaitForSecondsRealtime((float)(waitTime));

        while (true) {
            if (thisActiveMusicSession != activeMusicSession) {
                break;
            }
            musicModules[toggle].GetComponent<AudioSource>().clip = musicClip;
            musicModules[toggle].GetComponent<AudioSource>().volume = SceneManagerCW.volume * SceneManagerCW.musicVolume * volumeCoefficient;
            musicModules[toggle].GetComponent<ModuleCW>().volume = volumeCoefficient;
            musicModules[toggle].GetComponent<AudioSource>().PlayScheduled(nextStartTime);
            nextStartTime = nextStartTime + duration;

            if (toggle == musicModules.Length - 1) {
                toggle = 0;
            } else {
                toggle++;
            }
            //toggle = 1 - toggle;
            if (thisActiveMusicSession != activeMusicSession) {
                break;
            }
            await new WaitForSecondsRealtime((float)(duration - 1));
        }
    }

    //returns the audio source playing the sound 
    public static AudioSource PlaySound(string sound, float volumeCoefficient) {
        if (System.Array.IndexOf(SceneManagerCW.allSoundNames, sound) != -1) {
            AudioClip soundClip = SceneManagerCW.allSounds[System.Array.IndexOf(SceneManagerCW.allSoundNames, sound)];
            //SceneManagerCW.allSoundCounter = new int[allSounds.Length];
            int counter = 0;
            AudioSource refe = null;
            foreach (GameObject module in SceneManagerCW.soundModules) {
                if (module == null) {
                    SceneManagerCW.soundModules.Remove(module);
                } else {
                    /*if (module.GetComponent<AudioSource>().clip == soundClip) {
                        counter += 1;
                        //refe = module.GetComponent<AudioSource>();
                    }*/
                    //normal sound call
                    if (module.GetComponent<AudioSource>().clip == null || !module.GetComponent<AudioSource>().isPlaying/* && counter < 15*/) {
                        module.GetComponent<AudioSource>().clip = soundClip;
                        if (enterSceneMuteFrames > 0) {
                            module.GetComponent<AudioSource>().volume = 0;
                            module.GetComponent<ModuleCW>().volume = 0f;
                        } else {
                            module.GetComponent<AudioSource>().volume = SceneManagerCW.volume * SceneManagerCW.sfxVolume * volumeCoefficient;
                            module.GetComponent<ModuleCW>().volume = volumeCoefficient;
                        }
                        module.GetComponent<AudioSource>().Play();
                        SceneManagerCW.ClearSound(soundClip.length, module, false);
                        //print(module);
                        return module.GetComponent<AudioSource>();
                    }
                }
            }
            //overflow
            //if (/*counter < 15) {
                GameObject soundModule = new GameObject();
                soundModule.transform.parent = GameObject.FindWithTag("SceneManager").transform;
                soundModule.AddComponent<AudioSource>();
                soundModule.name = "Sound Module";
                soundModule.tag = "SoundModule";
                soundModule.GetComponent<AudioSource>().playOnAwake = false;
                
                SceneManagerCW.soundModules.Add(soundModule);
                DontDestroyOnLoad(soundModule);
                soundModule.GetComponent<AudioSource>().clip = soundClip;
                if (enterSceneMuteFrames > 0) {
                    soundModule.GetComponent<AudioSource>().volume = 0;
                    soundModule.GetComponent<ModuleCW>().volume = 0f;
                } else {
                    soundModule.GetComponent<AudioSource>().volume = SceneManagerCW.volume * SceneManagerCW.sfxVolume * volumeCoefficient;
                    soundModule.GetComponent<ModuleCW>().volume = volumeCoefficient;
                }
                soundModule.GetComponent<AudioSource>().outputAudioMixerGroup  = SceneManagerCW.SFX;
                soundModule.GetComponent<AudioSource>().Play();
                SceneManagerCW.ClearSound(soundClip.length, soundModule, true);
                return soundModule.GetComponent<AudioSource>();
            //} else {
                //return refe;
            //}
        } else {
            print("Error: Sound not found.");
            return null;
        }
    }

    public static void StopSound(AudioSource source, string sound) {
        if (source != null && source.clip == SceneManagerCW.allSounds[System.Array.IndexOf(SceneManagerCW.allSoundNames, sound)]) {
            source.Stop();
            source.clip = null;
            if (SceneManagerCW.soundModules.IndexOf(source.gameObject) >= SceneManagerCW.maxModules) {
                SceneManagerCW.soundModules.Remove(source.gameObject);
                GameObject.Destroy(source.gameObject);
            }
        }
    }

    private static async Task ClearSound(float time, GameObject module, bool remove) {
        AudioSource aus = module.GetComponent<AudioSource>();
        while (aus.isPlaying)
        {
            await Task.Yield();
        }
        if (module != null) {
            aus.Stop();
            aus.clip = null;
            if (remove) {
                SceneManagerCW.soundModules.Remove(module);
                GameObject.Destroy(module);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene() != prevScene) {
            if (SceneManager.GetActiveScene().name == "TitleScreen") {
                Destroy(gameObject);
            } else {
                OnNewScene();
            }
        }
        if (Input.GetButtonDown("Pause") && SceneManager.GetActiveScene().name != "TitleScreen" && !unpausable) {
            Pause();
        }
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1) {
            StaticDataCW.Dying = false;
            StaticDataCW.PrevScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
        prevScene = SceneManager.GetActiveScene();
    }
    void FixedUpdate()
    {
        if (enterSceneMuteFrames > 0) {
            enterSceneMuteFrames--;
        }
        if(freeze && !negateTimedFreeze && Time.time > freezeStamp + freezeTime) {
            UnFreeze();
        }
        if (freeze) {
            addTimeDiff = Time.time - saveTimeAtFreeze;
        } else {
            StaticDataCW.Time = Time.time - timeDifference;
        }
    }

    public void Pause()
    {
        if (!freeze) {
            paused = true;
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().Pause();
            Freeze(0f,false);
            Time.timeScale = 0f;
        } else {
            paused = false;
            GameObject.FindWithTag("Canvas").GetComponent<UIControllerCW>().Unpause();
            UnFreeze();
        }
    }
    public void ChangeScene(string scene, bool isDying) {
        StaticDataCW.Dying = isDying;
        StaticDataCW.PrevScene = SceneManager.GetActiveScene().name;
        paused = false;
        /*
        int n;
        for (n = 0; n < scenesLoadedNames.Length; n++) {
            if (scenesLoadedNames[n] == scene) {
                break;
            }
        }
        */
        //int n = System.Array.IndexOf(scenesLoadedNames, scene);
        //scenesLoaded[n].allowSceneActivation = true;

        //scenesLoaded[n].allowSceneActivation = true;
        //GameObject.Find(SceneManager.GetActiveScene().name).active = false;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        //GameObject.Find(scene).active = true;
    }

    public void ReturnToMainMenu(bool toMap) {
        if (freeze) {
            UnFreeze();
        }
        StaticDataCW.Dying = false;
        StaticDataCW.Health = 10;
        StaticDataCW.InWorldMap = toMap;
        SaveDataCW.Load();
        PlayerControllerCW player = GameObject.FindWithTag("Player").GetComponent<PlayerControllerCW>();
        if (Game.loadedGame != null && player != null) {
            Game.loadedGame.inWorldMap = toMap;
            Game.loadedGame.currentSpikePointx = player.spikePoint.x;
            Game.loadedGame.currentSpikePointy = player.spikePoint.y;
            Game.loadedGame.health = player.health + player.healing;
            StaticDataCW.Health = player.health + player.healing;
            Game.loadedGame.healingOrbs = StaticDataCW.HealingOrbs;
        }
        SaveDataCW.Save();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
    }
    //Next 2 voids are debug, remove on launch
    public void UnlockLevel2() {
        SaveDataCW.Load();
        Game.loadedGame.levelsUnlocked = 2;
        Game.loadedGame.furthestCP = 15;
        Game.loadedGame.furthestScene = 12;
        SaveDataCW.Save();
    }

    public void UnlockAllLevels() {
        SaveDataCW.Load();
        Game.loadedGame.levelsUnlocked = 4;
        Game.loadedGame.furthestCP = 47;
        Game.loadedGame.furthestScene = 39;
        SaveDataCW.Save();
    }

    public void Freeze(float seconds, bool timed) {
        saveTimeScale = Time.timeScale;
        Time.timeScale = 1.0f;
        saveTimeAtFreeze = Time.time;
        freeze = true;
        if (timed) {
            freezeStamp = Time.time;
            freezeTime = seconds;
            negateTimedFreeze = false;
        } else {
            negateTimedFreeze = true;
        }
        freezable = new List<GameObject>();
        scriptsFreezable = new List<ScriptsFreezable>();
        bcfreezable = new List<GameObject>();
        animfreezable = new List<GameObject>();
        GameObject[] allActiveObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject i in allActiveObjects) {
            if (!(i.CompareTag("MainCamera") || i.CompareTag("Light") || i.CompareTag("BKGLayer") || i.CompareTag("ParallaxingLayer") || i.CompareTag("SceneManager") || i.CompareTag("Canvas") || i.CompareTag("UIElement") || i.CompareTag("Vcam")|| i.CompareTag("SoundModule") || i.name == "GameController")) {
                MonoBehaviour[] scripts = i.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour j in scripts) {
                    if (j.enabled) {
                        j.enabled = false;
                        scriptsFreezable.Add(new ScriptsFreezable(j,i));
                    }
                }
                if (i.GetComponent<Rigidbody2D>() != null) {
                    if (((i.GetComponent<Rigidbody2D>().constraints & RigidbodyConstraints2D.FreezePositionX) == 0) && ((i.GetComponent<Rigidbody2D>().constraints & RigidbodyConstraints2D.FreezePositionY) == 0)) { 
                        freezable.Add(i);
                        i.GetComponent<Rigidbody2D>().constraints |= RigidbodyConstraints2D.FreezePosition;
                    }
                    
                }
                if (i.GetComponent<BoxCollider2D>() != null && i.GetComponent<BoxCollider2D>().enabled) {
                    i.GetComponent<BoxCollider2D>().enabled = false;
                    bcfreezable.Add(i);
                }
                if (i.GetComponent<Animator>() != null && i.GetComponent<Animator>().enabled) {
                    i.GetComponent<Animator>().enabled = false;
                    animfreezable.Add(i);
                }
            }
        }
    }
    public void UnFreeze() {
        freeze = false;
        unPauseStamp = StaticDataCW.Time;
        Time.timeScale = saveTimeScale;
        timeDifference += addTimeDiff;
        GameObject[] allActiveObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject i in allActiveObjects) {
            if (!(i.CompareTag("MainCamera") || i.CompareTag("Light") || i.CompareTag("BKGLayer") || i.CompareTag("ParallaxingLayer") || i.CompareTag("SceneManager") || i.CompareTag("Canvas") || i.CompareTag("UIElement") || i.CompareTag("Vcam")|| i.CompareTag("SoundModule") || i.name == "GameController")) {
                MonoBehaviour[] scripts = i.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour j in scripts) {
                    foreach (ScriptsFreezable s in scriptsFreezable) {
                        if(s.getObj() == i && s.getScript() == j) {
                            j.enabled = true;
                            break;
                        }
                    }
                }
                if (i.GetComponent<Rigidbody2D>() != null && freezable.Contains(i)) {
                    i.GetComponent<Rigidbody2D>().constraints &= ~(RigidbodyConstraints2D.FreezePosition);
                }
                if (i.GetComponent<BoxCollider2D>() != null && bcfreezable.Contains(i)) {
                    i.GetComponent<BoxCollider2D>().enabled = true;
                }
                if (i.GetComponent<Animator>() != null && animfreezable.Contains(i)) {
                    i.GetComponent<Animator>().enabled = true;
                }
            }
        }
    }

    /*public static bool isEnemyLoaded (GameObject enemy) {
        if (StaticDataCW.EnemiesLoaded != null) {
            for (int i = 0; i < StaticDataCW.EnemiesLoaded.Count; i++) {
                if (StaticDataCW.EnemiesLoaded[i].isID(enemy.transform.position)) {
                    return true;
                }
            }
        }
        return false;
    }*/
}

class ScriptsFreezable {
    private MonoBehaviour script;
    private GameObject obj;
    public ScriptsFreezable(MonoBehaviour script, GameObject obj) {
        this.script = script;
        this.obj = obj;
    }
    public GameObject getObj() {
        return obj;
    }
    public MonoBehaviour getScript() {
        return script;
    }
}

public class Enemy {
    private GameObject enemyObj;
    public bool dead;
    private float id;
    public Enemy(Vector3 position, bool dead) {
        this.dead = dead;
        id = position.x+position.y+(SceneManager.GetActiveScene().buildIndex)*3.4532f;
    }
    public float getID() {
        return id;
    }
    public bool isID(Vector3 position) {
        return (position.x+position.y+(SceneManager.GetActiveScene().buildIndex)*3.4532f) < (id + 0.1f) && (position.x+position.y+(SceneManager.GetActiveScene().buildIndex)*3.4532f) > id - 0.1f;
    }
    public bool isDead() {
        return dead;
    }
}

public struct SongData
{
    public string name;
    public double tail;

    public SongData(string name, double tail)
    {
        this.name = name;
        this.tail = tail;
    }
}