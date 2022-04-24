using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        // TODO remove
        DeletePlayerData();
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(GridObject);
    }

    bool isSceneLoaded = false;

    void Update()
    {
        // TODO clean this up
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
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
    public void PlayGame()
    {
        LoadPlayerData();
        if (saveData.IsNewGame)
        {
            saveData.IsNewGame = false;
            SavePlayerData();

            Debug.Log("Loading game scene");
            // loading escape mode
            Loader.Load(Loader.Scene.Game);
        }
        else
        {
            Debug.Log("Continuing game & opening Castle mode.");
        }
    }

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
