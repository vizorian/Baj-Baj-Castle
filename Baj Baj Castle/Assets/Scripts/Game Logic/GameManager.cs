// This class is responsible for managing everything related to the game.
namespace Game_Logic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Resources
    public Player Player;
    public SaveData SaveData;
    public GameObject PauseMenuPrefab;
    public GameObject TutorialScreenPrefab;
    public GameObject Canvas;
    public Sprite CellSprite;
    public GlobalGameState CurrentGlobalGameState;
    public GameObject GridObject;

    // References
    private LevelManager levelManager;
    private GameObject pauseMenu;
    private GameObject tutorialScreen;

    // Logic
    public int Level = 1;
    public int MaxLevels = 3;
    private bool tutorialDisplayed;
    public bool IsDebug = false;
    private bool isGameOverSet;
    public bool IsNewGame;
    private bool isNextLevel;
    private bool isSaveLoaded;
    private bool isSceneLoaded;

    [UsedImplicitly]
    private void Awake()
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

    [UsedImplicitly]
    private void Update()
    {
        if (Instance.CurrentGlobalGameState == GlobalGameState.Reload) Instance.Cleanup(); // Reload

        if (Instance.CurrentGlobalGameState == GlobalGameState.Victory || Instance.CurrentGlobalGameState == GlobalGameState.Defeat) // GameOver scene logic
        {
            if (Instance.isGameOverSet == false)
            {
                var sceneCanvas = GameObject.Find("Canvas");
                var sceneText = sceneCanvas.transform.Find("GameOverScreen").Find("Background").Find("Message")
                    .GetComponent<TextMeshProUGUI>();
                do
                {
                    sceneText.text = Instance.CurrentGlobalGameState == GlobalGameState.Victory ? "You Escaped!" : "You died.";
                } while (sceneText.text != "You died." && sceneText.text != "You Escaped!");

                Instance.isGameOverSet = true;
            }
            else
            {
                if (Input.anyKeyDown) Instance.QuitToMenu();
            }
        }

        if (Instance.CurrentGlobalGameState != GlobalGameState.Loading) // Pausing logic
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Instance.CurrentGlobalGameState == GlobalGameState.Escape && Instance.CurrentGlobalGameState != GlobalGameState.Tutorial)
                    Instance.PauseGame();
                else if (Instance.CurrentGlobalGameState == GlobalGameState.Pause && Instance.CurrentGlobalGameState != GlobalGameState.Tutorial)
                    Instance.UnpauseGame();
                else if (Instance.CurrentGlobalGameState == GlobalGameState.Tutorial) Instance.CloseTutorial();
            }

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

                Instance.Canvas = GameObject.Find("GameCanvas");
                var loading = Instance.Canvas.transform.Find("Loading").gameObject;
                if (loading != null) loading.SetActive(true);
            }
            else // checks if generation is complete
            {
                if (!Instance.levelManager.StartingLevelPopulation && Instance.levelManager.IsGenerated) // if complete
                {
                    Instance.levelManager.PopulateLevel(Instance.Level);
                }
                else if (Instance.levelManager.IsPopulated) // if population is complete
                {
                    Instance.Player = Instance.levelManager.Player.GetComponent<Player>();

                    if (!Instance.isSaveLoaded)
                    {
                        Instance.isSaveLoaded = true;
                        Instance.Player.SetSaveData(Instance.SaveData);
                    }

                    Instance.levelManager.IsPopulated = false;
                    var loading = Instance.Canvas.transform.Find("Loading").gameObject;
                    if (loading != null) loading.SetActive(false);
                    Instance.CurrentGlobalGameState = GlobalGameState.Escape;

                    // open tutorial menu
                    if (!Instance.tutorialDisplayed) Instance.OpenTutorial();
                }
            }
        }
    }

    // Cleanup process
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

        if (Instance.Player != null)
        {
            Destroy(Instance.Player.gameObject);
            Instance.Player = null;
        }

        Instance.isNextLevel = false;
        Instance.isSceneLoaded = false;
        Instance.isSaveLoaded = false;
        Instance.CurrentGlobalGameState = GlobalGameState.MainMenu;
    }

    // Save player data
    public void SavePlayerData()
    {
        var path = Application.persistentDataPath + "/save.dat";
        var file = File.Create(path);
        var bf = new BinaryFormatter();
        if (Instance.Player != null) Instance.SaveData = Instance.Player.GetSaveData();
        bf.Serialize(file, Instance.SaveData);
        file.Close();
        file.Dispose();
    }

    // Load player data
    public bool LoadPlayerData()
    {
        Instance.SaveData = new SaveData();
        var path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            var file = File.Open(path, FileMode.Open);
            if (file.Length > 0)
            {
                var bf = new BinaryFormatter();
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

    // Delete player data
    [UsedImplicitly]
    public void DeletePlayerData()
    {
        var path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path)) File.Delete(path);
    }

    // This method is called when the player presses the "Play" button
    // It loads Castle mode if the player has not yet played the game before
    [UsedImplicitly]
    public void PlayGame(bool isForced)
    {
        var newGame = Instance.LoadPlayerData();
        if (newGame || isForced)
        {
            Loader.Load(Loader.Scene.Game, GlobalGameState.Loading);
        }
        else
        {
            var mainMenu = Instance.Canvas.transform.Find("MainMenu").gameObject;
            var castleMenu = Instance.Canvas.transform.Find("CastleMenu").gameObject;
            mainMenu.SetActive(false);
            castleMenu.SetActive(true);
        }
    }

    // Updates the upgrade menu UI with player data
    public void UpdateUpgradeMenu()
    {
        var upgradeMenu = Instance.Canvas.transform.Find("UpgradeMenu");
        var treasureCount = upgradeMenu.Find("Header").Find("Subheader").Find("TreasureCount").gameObject
            .GetComponent<TextMeshProUGUI>();
        treasureCount.text = Instance.SaveData.Gold.ToString();
        string[] stats = { "Strength", "Agility", "Intelligence", "Luck", "Health", "Defense" };

        foreach (var stat in stats)
        {
            var statText = upgradeMenu.Find("Upgrade" + stat).Find("Text").Find("Subtext");
            var statCount = statText.Find(stat + "Count").gameObject.GetComponent<TextMeshProUGUI>();
            var statCost = statText.Find("Price").Find(stat + "Price").gameObject.GetComponent<TextMeshProUGUI>();
            var statLevel = Instance.SaveData.GetStat(stat);
            statCount.text = statLevel.ToString();
            statCost.text = stat == "Health" ? CalculateCost(statLevel, true).ToString() : CalculateCost(statLevel, false).ToString();
        }
    }

    // Upgrades specific stat if the player has enough gold
    [UsedImplicitly]
    public void UpgradeStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                var cost = CalculateCost(Instance.SaveData.StrengthUpgradeLevel, false);
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
                cost = CalculateCost(Instance.SaveData.AgilityUpgradeLevel, false);
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
                cost = CalculateCost(Instance.SaveData.IntelligenceUpgradeLevel, false);
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
                cost = CalculateCost(Instance.SaveData.LuckUpgradeLevel, false);
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
                cost = CalculateCost(Instance.SaveData.DefenseUpgradeLevel, false);
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

        // Save player data
        Instance.SavePlayerData();
        // Update upgrade menu UI
        Instance.UpdateUpgradeMenu();
    }

    // Calculates the cost of an upgrade
    private int CalculateCost(int level, bool isHealth)
    {
        if (level == 0) return 1;
        if (isHealth)
            return Mathf.RoundToInt(level + level * 0.5f);
        return Mathf.RoundToInt(level * (level + 1));
    }

    // Pause the game
    private void PauseGame()
    {
        if (Instance.pauseMenu == null)
            Instance.pauseMenu = Instantiate(PauseMenuPrefab, Instance.Canvas.transform);
        else
            Instance.pauseMenu.SetActive(true);

        // Freeze game
        Time.timeScale = 0;
        Instance.CurrentGlobalGameState = GlobalGameState.Pause;
    }

    // Resume the game
    public void UnpauseGame()
    {
        Instance.pauseMenu.SetActive(false);

        // Unfreeze game
        Time.timeScale = 1;
        Instance.CurrentGlobalGameState = GlobalGameState.Escape;
    }

    // Open tutorial window
    public void OpenTutorial()
    {
        Instance.tutorialDisplayed = true;
        Instance.tutorialScreen = Instantiate(TutorialScreenPrefab, Instance.Canvas.transform);

        // Freeze game
        Time.timeScale = 0;
        Instance.CurrentGlobalGameState = GlobalGameState.Tutorial;
    }

    // Close tutorial window
    public void CloseTutorial()
    {
        Destroy(Instance.tutorialScreen);

        // Unfreeze game
        Time.timeScale = 1;
        Instance.CurrentGlobalGameState = GlobalGameState.Escape;
    }

    // Return to main menu
    public void QuitToMenu()
    {
        Instance.SavePlayerData();
        if (Instance.CurrentGlobalGameState == GlobalGameState.Pause)
        {
            Destroy(Instance.pauseMenu);

            // Unfreeze game
            Time.timeScale = 1;
        }

        Loader.Load(Loader.Scene.Menu, GlobalGameState.Reload);
    }

    // Quit to desktop
    [UsedImplicitly]
    public void QuitToDesktop()
    {
        Instance.SavePlayerData();
        Application.Quit();
    }


    // End game with defeat
    public void Defeat()
    {
        Instance.SavePlayerData();
        Loader.Load(Loader.Scene.GameOver, GlobalGameState.Defeat);
    }

    // End game with victory
    public void Victory()
    {
        Instance.SavePlayerData();
        Loader.Load(Loader.Scene.GameOver, GlobalGameState.Victory);
    }

    // Begin new level creation
    public void NextLevel()
    {
        Instance.SavePlayerData();
        Instance.Level++;
        Instance.isNextLevel = true;
        Instance.CurrentGlobalGameState = GlobalGameState.Loading;
    }
}