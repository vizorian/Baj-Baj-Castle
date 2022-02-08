using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    private bool isOpen = false;
    //private bool isLocked = false;
    private bool isLooted = false;

    public Sprite ClosedSprite;
    public Sprite OpenSprite;
    public Sprite EmptySprite;

    private protected override void OnInteraction()
    {
        if (!isOpen)
        {
            isOpen = true;
            _spriteRenderer.sprite = OpenSprite;
            return;
        }

        if (!isLooted)
        {
            isLooted = true;
            _spriteRenderer.sprite = EmptySprite;
            gameObject.tag = "Object";
            return;
        }
    }
}
