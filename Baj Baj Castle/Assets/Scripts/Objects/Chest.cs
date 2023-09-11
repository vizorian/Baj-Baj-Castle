namespace Objects;

[UsedImplicitly]
public class Chest : Interactable
{
    public Sprite OpenSprite;
    public Sprite EmptySprite;
    public List<GameObject> Contents;
    public int GoldContained;

    private bool isOpen;
    private bool isLooted;

    // Handle interaction
    private protected override void OnInteraction()
    {
        if (!isOpen) // On open
        {
            isOpen = true;
            SpriteRenderer.sprite = OpenSprite;
            GenerateContent();
            return;
        }

        if (!isLooted) // On loot
        {
            isLooted = true;
            SpriteRenderer.sprite = EmptySprite;
            gameObject.tag = "Object";

            if (GameManager.Instance != null) GameManager.Instance.Player.Gold += GoldContained;
            FloatingText.Create("+" + GoldContained + "G", Color.yellow, transform.position, 1f, 0.5f, 0.2f);

            foreach (var item in Contents)
            {
                // Get random 360 degree angle
                var angle = Random.Range(0, 360);
                var direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
                // Get random distance away from chest
                var range = Mathf.Max(BoxCollider.size.x, BoxCollider.size.y) / 2;
                var distance = Random.Range(range, 2 * range);
                var position = transform.position + direction * distance;
                // Spawn item
                var itemObject = Instantiate(item, position, Quaternion.identity);
                LevelManager.Instance.AddItem(itemObject);
            }

            Contents.Clear();
        }
    }

    // Generates random items in chest, based on level and luck
    private void GenerateContent()
    {
        var luck = 0;
        var level = 1;
        if (GameManager.Instance != null)
        {
            luck = GameManager.Instance.SaveData.LuckUpgradeLevel;
            level = GameManager.Instance.Level;
        }

        // Generate gold
        GoldContained = Random.Range(1, 5 + level * (luck + 1) * 3);

        // Roll for item count based on luck
        var potentialItemCount = Random.Range(0, 2 * level + luck + 2);
        var itemCount = 0;
        // roll 1d100 for item count
        for (var i = 0; i < potentialItemCount; i++)
        {
            var roll = Random.Range(0, 101);
            if (roll <= luck * 1.5f + 5 - itemCount) itemCount++;
        }

        if (itemCount == 0)
            // 50 % chance
            if (Random.Range(0, 2) == 0)
                itemCount++;

        // Generate items
        var potions = GameAssets.Instance.ItemPrefabs.Where(i => i.name.Contains("Potion")).ToList();
        var woodenWeapons = GameAssets.Instance.ItemPrefabs.Where(i => i.name.Contains("Wooden")).ToList();
        var ironWeapons = GameAssets.Instance.ItemPrefabs.Where(i => i.name.Contains("Iron")).ToList();
        var steelWeapons = GameAssets.Instance.ItemPrefabs.Where(i => i.name.Contains("Steel")).ToList();
        var goldenWeapons = GameAssets.Instance.ItemPrefabs.Where(i => i.name.Contains("Golden")).ToList();
        for (var i = 0; i < itemCount; i++)
        {
            GameObject item;
            if (Random.Range(0, 3) == 0)
                item = potions[Random.Range(0, potions.Count)];
            else if (Random.Range(0, 101) <= 5 + luck * 0.5f)
                item = goldenWeapons[Random.Range(0, goldenWeapons.Count)];
            else if (Random.Range(0, 101) <= 10 + luck)
                item = steelWeapons[Random.Range(0, steelWeapons.Count)];
            else if (Random.Range(0, 101) <= 20 + luck * 2)
                item = ironWeapons[Random.Range(0, ironWeapons.Count)];
            else
                item = woodenWeapons[Random.Range(0, woodenWeapons.Count)];

            // Add to chest contents
            Contents.Add(item);
        }
    }
}