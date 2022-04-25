using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This class is responsible for managing everything related to the game.
// It is responsible for generating levels, spawning enemies, and managing the player's health.
// It is also responsible for managing the game's state.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject PauseMenuPrefab;
    public GameObject TutorialScreenPrefab;
    private GameObject tutorialScreen;
    private GameObject pauseMenu;
    // Resources
    private LevelManager levelManager;
    public GameObject Canvas;
    public GameObject GridObject;
    public Sprite CellSprite;
    public bool IsDebug = false;
    private bool isNextLevel;
    private bool tutorialDisplayed = false;
    public bool IsNewGame;
    public int Level = 1;
    public int MaxLevels = 3;
    bool isSceneLoaded = false;
    bool isSaveLoaded = false;
    bool isGameOverSet = false;
    // References
    public Player player;
    // Logic
    public SaveData SaveData;
    public GameState GameState;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(Instance.GridObject);
        }
    }

    void Update()
    {
        if (Instance.GameState == GameState.Reload)
        {
            Instance.Cleanup();
        }

        if (Instance.GameState == GameState.Victory || Instance.GameState == GameState.Defeat)
        {
            if (Instance.isGameOverSet == false)
            {
                var sceneCanvas = GameObject.Find("Canvas");
                var sceneText = sceneCanvas.transform.Find("GameOverScreen").Find("Background").Find("Message").GetComponent<TextMeshProUGUI>();
                sceneText.text = Instance.GameState == GameState.Victory ? "You Escaped!" : "You died.";
                Instance.isGameOverSet = true;
            }
            else
            {
                if (Input.anyKeyDown)
                {
                    Instance.QuitToMenu();
                }
            }
        }

        if (Instance.GameState != GameState.Loading)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Instance.GameState == GameState.Escape && Instance.GameState != GameState.Tutorial)
                {
                    Instance.PauseGame();
                }
                else if (Instance.GameState == GameState.Pause && Instance.GameState != GameState.Tutorial)
                {
                    Instance.UnpauseGame();
                }
                else if (Instance.GameState == GameState.Tutorial)
                {
                    Instance.CloseTutorial();
                }
            }
        }

        // TODO clean this up
        // clean the bool mess
        // potentially do game states
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!Instance.isSceneLoaded || Instance.isNextLevel) // if scene is loaded or next level
            {
                if (Instance.isNextLevel)
                {
                    Instance.isNextLevel = false;
                    Instance.levelManager.Cleanup();
                    Instance.levelManager.GenerateLevel(Instance.Level);
                }
                else
                {
                    Instance.isSceneLoaded = true;
                    Instance.levelManager = gameObject.AddComponent<LevelManager>();
                    Instance.levelManager.InstantiateComponent(Instance.CellSprite, GridObject, IsDebug);
                    Instance.levelManager.GenerateLevel(Instance.Level);
                }
                // Canvas = GameObject.Find("Canvas");
                // Canvas.SetActive(false);
                Instance.Canvas = GameObject.Find("GameCanvas");
                var loading = Instance.Canvas.transform.Find("Loading").gameObject;
                if (loading != null)
                {
                    loading.SetActive(true);
                }
            }
            else // checks if generation is complete
            {
                if (!Instance.levelManager.StartingLevelPopulation && Instance.levelManager.IsGenerated) // if complete
                {
                    Instance.levelManager.PopulateLevel(Instance.Level);
                }
                else if (Instance.levelManager.IsPopulated) // if population is complete
                {
                    Instance.player = Instance.levelManager.Player.GetComponent<Player>();

                    if (!Instance.isSaveLoaded)
                    {
                        Instance.isSaveLoaded = true;
                        Instance.player.SetSaveData(Instance.SaveData);
                    }

                    Instance.levelManager.IsPopulated = false;
                    var loading = Instance.Canvas.transform.Find("Loading").gameObject;
                    if (loading != null)
                    {
                        loading.SetActive(false);
                    }
                    Instance.GameState = GameState.Escape;

                    // open tutorial menu
                    if (!Instance.tutorialDisplayed)
                    {
                        Instance.OpenTutorial();
                    }
                }
            }
        }
    }

    private void Cleanup()
    {
        Instance.Canvas = GameObject.Find("Canvas");

        Instance.Level = 1;

        if (Instance.levelManager != null)
        {
            Instance.levelManager.Cleanup();
            Destroy(Instance.levelManager);
            Instance.levelManager = null;
        }

        if (Instance.pauseMenu != null)
        {
            Destroy(Instance.pauseMenu);
            Instance.pauseMenu = null;
        }

        if (Instance.player != null)
        {
            Destroy(Instance.player.gameObject);
            Instance.player = null;
        }

        Instance.isNextLevel = false;
        Instance.isSceneLoaded = false;
        Instance.isSaveLoaded = false;
        Instance.GameState = GameState.MainMenu;
    }

    // Saving
    public void SavePlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        FileStream file = File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();
        if (!Instance.IsNewGame)
        {
            if (Instance.player != null)
            {
                Instance.SaveData = Instance.player.GetSaveData();
            }
        }
        bf.Serialize(file, Instance.SaveData);
        file.Close();
        file.Dispose();
    }

    public bool LoadPlayerData()
    {
        Instance.SaveData = new SaveData();
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            FileStream file = File.Open(path, FileMode.Open);
            if (file.Length > 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                Instance.SaveData = (SaveData)bf.Deserialize(file);
                file.Close();
                file.Dispose();
                Instance.IsNewGame = false;
                return false;
            }
        }
        Instance.IsNewGame = true;
        return true;
    }

    public void DeletePlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    // This method is called when the player presses the "Play" button
    // It loads Castle mode if the player has not yet played the game before
    // Else, escape mode
    public void PlayGame(bool isForced)
    {
        var newGame = Instance.LoadPlayerData();
        if (newGame || isForced)
        {
            Loader.Load(Loader.Scene.Game, GameState.Loading);
        }
        else
        {
            var mainMenu = Instance.Canvas.transform.Find("MainMenu").gameObject;
            var castleMenu = Instance.Canvas.transform.Find("CastleMenu").gameObject;
            mainMenu.SetActive(false);
            castleMenu.SetActive(true);
        }
    }

    // Updates the shop UI with player's data
    public void UpdateUpgradeMenu()
    {
        var upgradeMenu = Instance.Canvas.transform.Find("UpgradeMenu");
        var treasureCount = upgradeMenu.Find("Header").Find("Subheader").Find("TreasureCount").gameObject.GetComponent<TextMeshProUGUI>();
        treasureCount.text = Instance.SaveData.Gold.ToString();
        string[] stats = { "Strength", "Agility", "Intelligence", "Luck", "Health", "Defense" };

        foreach (string stat in stats)
        {
            var statText = upgradeMenu.Find("Upgrade" + stat).Find("Text").Find("Subtext");
            var statCount = statText.Find(stat + "Count").gameObject.GetComponent<TextMeshProUGUI>();
            var statCost = statText.Find("Price").Find(stat + "Price").gameObject.GetComponent<TextMeshProUGUI>();
            var statLevel = Instance.SaveData.GetStat(stat);
            statCount.text = statLevel.ToString();
            if (stat == "Health")
            {
                statCost.text = CalculateCost(statLevel, true).ToString();
            }
            else
            {
                statCost.text = CalculateCost(statLevel).ToString();
            }
        }
    }

    // Upgrades specific stat if the player has enough gold
    // Save data is updated
    // Calls UpdateUpgradeMenu() to update the shop UI
    public void UpgradeStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                var cost = CalculateCost(Instance.SaveData.StrengthUpgradeLevel);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.StrengthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Agility":
                cost = CalculateCost(Instance.SaveData.AgilityUpgradeLevel);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.AgilityUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Intelligence":
                cost = CalculateCost(Instance.SaveData.IntelligenceUpgradeLevel);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.IntelligenceUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Luck":
                cost = CalculateCost(Instance.SaveData.LuckUpgradeLevel);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.LuckUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Health":
                cost = CalculateCost(Instance.SaveData.HealthUpgradeLevel, true);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.HealthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Defense":
                cost = CalculateCost(Instance.SaveData.DefenseUpgradeLevel);
                if (Instance.SaveData.Gold >= cost)
                {
                    Instance.SaveData.Gold -= cost;
                    Instance.SaveData.DefenseUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
        }
        Instance.SavePlayerData();
        Instance.UpdateUpgradeMenu();
    }

    private int CalculateCost(int level, bool isHealth = false)
    {
        if (level == 0)
        {
            return 1;
        }
        if (isHealth)
        {
            return Mathf.RoundToInt((level + (level * 0.5f)));
        }
        else
        {
            return Mathf.RoundToInt((level * (level + 1)));
        }
    }

    private void PauseGame()
    {
        if (Instance.pauseMenu == null)
        {
            Instance.pauseMenu = Instantiate(PauseMenuPrefab, Instance.Canvas.transform);
        }
        else
        {
            Instance.pauseMenu.SetActive(true);
        }
        Time.timeScale = 0;
        Instance.GameState = GameState.Pause;
    }

    public void UnpauseGame()
    {
        Instance.pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Instance.GameState = GameState.Escape;
    }

    public void OpenTutorial()
    {
        Instance.tutorialDisplayed = true;
        Instance.tutorialScreen = Instantiate(TutorialScreenPrefab, Instance.Canvas.transform);
        Time.timeScale = 0;
        Instance.GameState = GameState.Tutorial;
    }

    public void CloseTutorial()
    {
        Destroy(Instance.tutorialScreen);
        Time.timeScale = 1;
        Instance.GameState = GameState.Escape;
    }

    public void QuitToMenu()
    {
        Instance.SavePlayerData();
        if (Instance.GameState == GameState.Pause)
        {
            Destroy(Instance.pauseMenu);
            Time.timeScale = 1;
        }
        Loader.Load(Loader.Scene.Menu, GameState.Reload);
    }

    // This method is called when the player presses the "Quit" button
    public void QuitToDesktop()
    {
        Instance.SavePlayerData();
        Application.Quit();
    }

    // This method is called when the player dies.
    // It brings the player to the GameOver scene.
    public void Defeat()
    {
        Instance.SavePlayerData();
        Loader.Load(Loader.Scene.GameOver, GameState.Defeat);
    }

    // This method is called when the player wins.
    // It stops the game and displays a victory message.
    public void Victory()
    {
        Instance.SavePlayerData();
        Loader.Load(Loader.Scene.GameOver, GameState.Victory);
    }

    // This method is called when the player finishes a level.
    // It generates a new level.
    public void NextLevel()
    {
        Instance.SavePlayerData();
        Instance.Level++;
        Instance.isNextLevel = true;
        Instance.GameState = GameState.Loading;
    }
}
