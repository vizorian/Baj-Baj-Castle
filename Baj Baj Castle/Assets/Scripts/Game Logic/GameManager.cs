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
    private GameObject pauseMenu;
    // Resources
    private LevelManager levelManager;
    public GameObject Canvas;
    public GameObject GameCanvas;
    public GameObject GridObject;
    public Sprite CellSprite;
    public bool IsDebug = false;
    private bool isNextLevel;
    public bool IsNewGame = true;
    public int Level = 1;
    public int MaxLevels = 3;

    // References
    public Player player;
    // Logic
    public SaveData SaveData;
    public GameState GameState;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadPlayerData();
            GameState = GameState.MainMenu;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(GridObject);
            // Wrong
            // DontDestroyOnLoad(Canvas);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    bool isSceneLoaded = false;
    bool isSaveLoaded = false;

    void Update()
    {
        if (GameState == GameState.Reload)
        {
            Cleanup();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameState == GameState.Escape)
            {
                PauseGame();
            }
            else if (GameState == GameState.Pause)
            {
                UnpauseGame();
            }
        }

        // TODO clean this up
        // clean the bool mess
        // potentially do game states
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!isSceneLoaded || isNextLevel) // if scene is loaded or next level
            {
                if (isNextLevel)
                {
                    isNextLevel = false;
                    levelManager.Cleanup();
                    levelManager.GenerateLevel(Level);
                }
                else
                {
                    isSceneLoaded = true;
                    levelManager = gameObject.AddComponent<LevelManager>();
                    levelManager.InstantiateComponent(CellSprite, GridObject, IsDebug);
                    levelManager.GenerateLevel(Level);
                }
                // Canvas = GameObject.Find("Canvas");
                Canvas.SetActive(false);
                GameCanvas = GameObject.Find("GameCanvas");
                var loading = GameCanvas.transform.Find("Loading").gameObject;
                if (loading != null)
                {
                    Debug.Log("Enabling loading screen");
                    loading.SetActive(true);
                }
            }
            else // checks if generation is complete
            {
                if (!levelManager.StartingLevelPopulation && levelManager.IsGenerated) // if complete
                {
                    levelManager.PopulateLevel(Level);
                }
                else if (levelManager.IsPopulated) // if population is complete
                {
                    player = levelManager.Player.GetComponent<Player>();
                    if (isSaveLoaded)
                    {
                        isSaveLoaded = false;
                        player.SetSaveData(SaveData);
                    }

                    levelManager.IsPopulated = false;
                    // TODO improve this
                    var loading = GameCanvas.transform.Find("Loading").gameObject;
                    if (loading != null)
                    {
                        loading.SetActive(false);
                    }
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "GameOver")
        {
            if (Input.anyKeyDown)
            {
                Loader.Load(Loader.Scene.Menu, GameState.MainMenu);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Menu")
        {

        }
    }

    private void Cleanup()
    {
        // Canvas = GameObject.Find("Canvas");
        Level = 1;

        if (levelManager != null)
        {
            levelManager.Cleanup();
            Destroy(levelManager);
            levelManager = null;
        }

        if (pauseMenu != null)
        {
            Destroy(pauseMenu);
            pauseMenu = null;
        }

        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        isNextLevel = false;
        isSceneLoaded = false;
        isSaveLoaded = false;
        GameState = GameState.MainMenu;
    }

    private void PauseGame()
    {
        pauseMenu = Instantiate(PauseMenuPrefab, GameCanvas.transform);
        Time.timeScale = 0;
        GameState = GameState.Pause;
    }

    // Saving
    public void SavePlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        FileStream file = File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();
        Debug.Log("Saving player data");
        if (!IsNewGame)
        {
            Debug.Log(player.name);
            if (levelManager.Player != null)
            {
                SaveData = levelManager.Player.GetComponent<Player>().GetSaveData();
                Debug.Log("Got ingame player data");
            }
            else
            {
                Debug.Log("No player data received");
            }
        }
        Debug.Log("Saved data: " + SaveData.ToString());
        bf.Serialize(file, SaveData);
        file.Close();
        file.Dispose();
    }

    public bool LoadPlayerData()
    {
        Debug.Log("Loading player data");
        SaveData = new SaveData();
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            FileStream file = File.Open(path, FileMode.Open);
            if (file.Length > 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                SaveData = (SaveData)bf.Deserialize(file);
                file.Close();
                file.Dispose();
                IsNewGame = false;
                Debug.Log("Loaded file.");
                return false;
            }
            else
            {
                Debug.Log("Save file empty. New game.");
            }
        }
        else
        {
            Debug.Log("No save file found. New game.");
        }
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
    public void PlayGame()
    {
        Debug.Log("Play game is called by button");
        if (IsNewGame)
        {
            Debug.Log("New game");
            Loader.Load(Loader.Scene.Game, GameState.Escape);
        }
        else
        {
            Debug.Log("Continuing game");
            var mainMenu = Canvas.transform.Find("MainMenu").gameObject;
            var castleMenu = Canvas.transform.Find("CastleMenu").gameObject;
            mainMenu.SetActive(false);
            castleMenu.SetActive(true);
        }
    }

    public void CheckNewGame()
    {
        if (IsNewGame)
        {
            IsNewGame = true;
        }
        else
        {
            IsNewGame = false;
        }
    }

    public void UpdateUpgradeMenu()
    {
        var upgradeMenu = GameObject.Find("UpgradeMenu");
        var treasureCount = upgradeMenu.transform.Find("Header").Find("Subheader").Find("TreasureCount").gameObject.GetComponent<TextMeshProUGUI>();
        treasureCount.text = SaveData.Gold.ToString();
        string[] stats = { "Strength", "Agility", "Intelligence", "Luck", "Health", "Defense" };

        foreach (string stat in stats)
        {
            var statText = upgradeMenu.transform.Find("Upgrade" + stat).Find("Text").Find("Subtext");
            var statCount = statText.Find(stat + "Count").gameObject.GetComponent<TextMeshProUGUI>();
            var statCost = statText.Find("Price").Find(stat + "Price").gameObject.GetComponent<TextMeshProUGUI>();
            var statLevel = SaveData.GetStat(stat);
            statCount.text = statLevel.ToString();
            statCost.text = CalculateCost(statLevel).ToString();
        }
    }

    public void UpgradeStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                var cost = CalculateCost(SaveData.StrengthUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.StrengthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Agility":
                cost = CalculateCost(SaveData.AgilityUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.AgilityUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Intelligence":
                cost = CalculateCost(SaveData.IntelligenceUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.IntelligenceUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Luck":
                cost = CalculateCost(SaveData.LuckUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.LuckUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Health":
                cost = CalculateCost(SaveData.HealthUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.HealthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Defense":
                cost = CalculateCost(SaveData.DefenseUpgradeLevel);
                if (SaveData.Gold >= cost)
                {
                    SaveData.Gold -= cost;
                    SaveData.DefenseUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
        }
        SavePlayerData();
        UpdateUpgradeMenu();
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

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        GameState = GameState.Escape;
        Destroy(pauseMenu);
        pauseMenu = null;
    }

    public void QuitToMenu()
    {
        SavePlayerData();
        Destroy(pauseMenu);
        Time.timeScale = 1;
        Loader.Load(Loader.Scene.Menu, GameState.Reload);
    }

    // This method is called when the player presses the "Quit" button
    public void QuitToDesktop()
    {
        SavePlayerData();
        Application.Quit();
    }

    // TODO death
    // This method is called when the player dies.
    // It brings the player to the GameOver scene.
    public void Defeat()
    {
        SavePlayerData();
        Debug.Log("You died");
        Loader.Load(Loader.Scene.GameOver, GameState.GameOver);
    }

    // TODO victory
    // This method is called when the player wins.
    // It stops the game and displays a victory message.
    public void Victory()
    {
        SavePlayerData();
        Debug.Log("Won the game");
        Loader.Load(Loader.Scene.GameOver, GameState.Victory);
    }

    // This method is called when the player finishes a level.
    // It generates a new level.
    public void NextLevel()
    {
        SavePlayerData();
        Level++;
        isNextLevel = true;
        Debug.Log("Loading new level");
    }
}
