using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : Interactable
{
    private bool isFlipped = false;

    public Sprite MainSprite;
    public Sprite SideSprite;
    public Sprite UpSprite;
    public Sprite DownSprite;

    protected override void OnInteraction()
    {
        if (!isFlipped)
        {
            isActive = false;
            gameObject.tag = "Object";
            _spriteRenderer.flipX = false;

            if (up)
            {
                _spriteRenderer.sprite = UpSprite;
            }else if (right)
            {
                _spriteRenderer.sprite = SideSprite;
                _spriteRenderer.flipX = true;
            }
            else if (down)
            {
                _spriteRenderer.sprite = DownSprite;
            }
            else
            {
                _spriteRenderer.sprite = SideSprite;
            }
            return;
        }
    }
}
