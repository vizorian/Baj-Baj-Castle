using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    public List<GameObject> Contents;
    public int GoldContained = 0;

    private bool isOpen = false;
    //private bool isLocked = false;
    private bool isLooted = false;

    public Sprite ClosedSprite;
    public Sprite OpenSprite;
    public Sprite EmptySprite;

    private protected override void OnInteraction()
    {
        if (!isOpen) // On open
        {
            isOpen = true;
            _spriteRenderer.sprite = OpenSprite;
            GenerateContent();
            return;
        }

        if (!isLooted) // On loot
        {
            isLooted = true;
            _spriteRenderer.sprite = EmptySprite;
            gameObject.tag = "Object";

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Player.Gold += GoldContained;
            }
            FloatingText.Create("+" + GoldContained.ToString() + "G", Color.yellow, transform.position, 1f, 0.5f, 0.2f);

            foreach (var item in Contents)
            {
                // Get random 360 degree angle
                var angle = UnityEngine.Random.Range(0, 360);
                var direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
                // get random distance away from chest
                var range = Mathf.Max(boxCollider.size.x, boxCollider.size.y) / 2;
                var distance = UnityEngine.Random.Range(range, 2 * range);
                var position = transform.position + direction * distance;
                // spawn item
                var itemObject = Instantiate(item, position, Quaternion.identity);
                LevelManager.Instance.AddItem(itemObject);
            }
            Contents.Clear();

            return;
        }
    }

    // Puts random items in chest, based on level and luck
    private void GenerateContent()
    {
        int luck = 0;
        int level = 1;
        if (GameManager.Instance != null)
        {
            luck = GameManager.Instance.SaveData.LuckUpgradeLevel;
            level = GameManager.Instance.Level;
        }

        // Generate gold
        GoldContained = UnityEngine.Random.Range(1, level * (luck + 1) * 3);

        // Roll for item count based on luck
        var potentialItemCount = UnityEngine.Random.Range(0, level + (luck + 1));
        int itemCount = 0;
        // roll 1d100 for item count
        for (var i = 0; i < potentialItemCount; i++)
        {
            var roll = UnityEngine.Random.Range(0, 101);
            if (roll <= luck * 1.5f + 1 - itemCount)
            {
                itemCount++;
            }
        }

        if (itemCount == 0)
        {
            // 50 % chance
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                itemCount++;
            }
        }

        // Generate items
        for (var i = 0; i < itemCount; i++)
        {
            var item = GameAssets.Instance.itemPrefabs[UnityEngine.Random.Range(0, GameAssets.Instance.itemPrefabs.Count)];
            Contents.Add(item);
        }
    }
}
