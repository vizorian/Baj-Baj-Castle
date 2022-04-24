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
    public static GameManager Instance;

    // Resources
    private LevelManager levelManager;
    public GameObject Canvas;
    public GameObject GridObject;
    public Sprite CellSprite;
    public bool IsDebug = false;
    private bool isNextLevel;

    public int Level = 1;
    public int MaxLevels = 3;

    // References
    private Player player;
    // Logic
    public SaveData saveData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadPlayerData();
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(GridObject);
    }

    bool isSceneLoaded = false;
    bool isSaveLoaded = false;

    void Update()
    {
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
                Canvas = GameObject.Find("Canvas");
                var loading = Canvas.transform.Find("Loading").gameObject;
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
                        player.SetSaveData(saveData);
                    }

                    levelManager.IsPopulated = false;
                    var loading = Canvas.transform.Find("Loading").gameObject;
                    if (loading != null)
                    {
                        Debug.Log("Disabling loading screen");
                        loading.SetActive(false);
                    }
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "GameOver")
        {
            if (Input.anyKeyDown)
            {
                Loader.Load(Loader.Scene.Menu);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Menu")
        {

        }
    }

    // Saving
    public void SavePlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        FileStream file = File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();

        // Save player data
        if (player != null)
        {
            print("Player found, taking new data.");
            saveData = player.GetSaveData();
        }

        bf.Serialize(file, saveData);
        file.Close();
        file.Dispose();

        print("Game saved to " + path);
    }

    public void LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            FileStream file = File.Open(path, FileMode.Open);
            if (file.Length > 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                saveData = (SaveData)bf.Deserialize(file);

                file.Close();
                file.Dispose();
                print("Loaded save data: " + saveData.ToString());
            }
            else
            {
                print("Empty file found. New game.");
                saveData = new SaveData();
            }
        }
        else
        {
            print("No save data found. New game.");
            saveData = new SaveData();
        }
    }

    public void DeletePlayerData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            File.Delete(path);
            print("Deleted save data.");
        }
    }

    // This method is called when the player presses the "Play" button
    // It loads Castle mode if the player has not yet played the game before
    // Else, escape mode
    public void PlayGame(bool force = false)
    {
        if (saveData.IsNewGame || force)
        {
            if (!force)
            {
                saveData.IsNewGame = false;
                SavePlayerData();
            }

            Debug.Log("Loading game scene");
            // loading escape mode
            Loader.Load(Loader.Scene.Game);
        }
        else
        {
            var mainMenu = Canvas.transform.Find("MainMenu").gameObject;
            var castleMenu = Canvas.transform.Find("CastleMenu").gameObject;
            mainMenu.SetActive(false);
            castleMenu.SetActive(true);
            Debug.Log("Continuing game & opening Castle mode.");
        }
    }

    public void UpdateUpgradeMenu()
    {
        var upgradeMenu = GameObject.Find("UpgradeMenu");
        var treasureCount = upgradeMenu.transform.Find("Header").Find("Subheader").Find("TreasureCount").gameObject.GetComponent<TextMeshProUGUI>();
        treasureCount.text = saveData.Gold.ToString();
        string[] stats = { "Strength", "Agility", "Intelligence", "Luck", "Health", "Defense" };

        foreach (string stat in stats)
        {
            var statText = upgradeMenu.transform.Find("Upgrade" + stat).Find("Text").Find("Subtext");
            var statCount = statText.Find(stat + "Count").gameObject.GetComponent<TextMeshProUGUI>();
            var statCost = statText.Find("Price").Find(stat + "Price").gameObject.GetComponent<TextMeshProUGUI>();
            var statLevel = saveData.GetStat(stat);
            statCount.text = statLevel.ToString();
            statCost.text = CalculateCost(statLevel).ToString();
        }
    }

    public void UpgradeStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                var cost = CalculateCost(saveData.StrengthUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.StrengthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Agility":
                cost = CalculateCost(saveData.AgilityUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.AgilityUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Intelligence":
                cost = CalculateCost(saveData.IntelligenceUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.IntelligenceUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Luck":
                cost = CalculateCost(saveData.LuckUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.LuckUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Health":
                cost = CalculateCost(saveData.HealthUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.HealthUpgradeLevel++;
                }
                else
                {
                    return;
                }
                break;
            case "Defense":
                cost = CalculateCost(saveData.DefenseUpgradeLevel);
                if (saveData.Gold >= cost)
                {
                    saveData.Gold -= cost;
                    saveData.DefenseUpgradeLevel++;
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
            return Mathf.RoundToInt((level * (level / 10)));
        }
        else
        {
            return Mathf.RoundToInt((level * (level + 1)));
        }
    }

    // This method is called when the player presses the "Quit" button
    public void QuitToDesktop()
    {
        Application.Quit();
    }

    // TODO death
    // This method is called when the player dies.
    // It brings the player to the GameOver scene.
    public void Defeat()
    {
        Debug.Log("You died");
        throw new NotImplementedException();
        // Loader.Load(Loader.Scene.GameOver);
    }

    // TODO victory
    // This method is called when the player wins.
    // It stops the game and displays a victory message.
    public void Victory()
    {
        Debug.Log("Won the game");
        throw new NotImplementedException();
        // Loader.Load(Loader.Scene.GameOver);
    }

    // This method is called when the player presses the restart button.
    // It resets the game and generates a new level.
    public void Restart()
    {

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
