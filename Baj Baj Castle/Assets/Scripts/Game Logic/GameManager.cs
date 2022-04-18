using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// This class is responsible for managing everything related to the game.
// It is responsible for generating levels, spawning enemies, and managing the player's health.
// It is also responsible for managing the game's state.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Resources
    private LevelManager levelManager;
    public GameObject GridObject;
    public Sprite CellSprite;
    public bool Debug = false;
    private GameState gameState;

    // References
    private Player player;

    // Logic
    public SaveData saveData;

    void Start()
    {
        levelManager = gameObject.AddComponent<LevelManager>();
        levelManager.CellSprite = CellSprite;
        levelManager.GridObject = GridObject;
        levelManager.IsDebug = Debug;
    }

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

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            print("Generating level");
            levelManager.GenerateLevel(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (player == null)
            {
                FindPlayer();
            }
            // Save player data
            SaveState();
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            if (player == null)
            {
                FindPlayer();
            }
            // Load player data
            LoadState();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (player == null)
            {
                FindPlayer();
            }
            LoadState();
            // Set player data
            player.SetSaveData(saveData);
        }
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Saving
    /*
    INT Gold
    INT StrengthUpgradeLevel
    INT AgilityUpgradeLevel
    INT IntelligenceUpgradeLevel
    */
    public void SaveState()
    {
        string path = Application.persistentDataPath + "/save.dat";
        FileStream file = File.Create(path);
        BinaryFormatter bf = new BinaryFormatter();

        // Save player data
        if (player != null)
        {
            saveData = player.GetSaveData();
        }
        else
        {
            saveData = new SaveData();
        }

        bf.Serialize(file, saveData);
        file.Close();
        file.Dispose();

        print("Game saved to " + path);
    }

    public void LoadState()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            print("file found at " + path);
            FileStream file = File.Open(path, FileMode.Open);
            print("file length: " + file.Length);
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
                print("No save data found");
            }
        }
    }

    // This method is called when the player presses the "Start" button.
    // It changes the game state to "Castle".
    // It brings the player to Castle mode.
    public void StartGame()
    {
        gameState = GameState.Castle;
    }

    // This method is called when the player dies.
    // It brings the player back to the Castle mode.
    public void PlayerDied()
    {

    }

    // This method is called when the player wins.
    // It stops the game and displays a victory message.
    public void PlayerWon()
    {

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

    }
}
