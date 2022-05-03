using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class Table : Interactable
{
    public Sprite DownSprite;
    private readonly bool isFlipped = false;

    public Sprite SideSprite;
    public Sprite UpSprite;

    private protected override void OnInteraction()
    {
        if (!isFlipped)
        {
            gameObject.tag = "Object";
            SpriteRenderer.flipX = false;

            if (Up)
            {
                SpriteRenderer.sprite = UpSprite;
            }
            else if (Right)
            {
                SpriteRenderer.sprite = SideSprite;
                SpriteRenderer.flipX = true;
            }
            else if (Down)
            {
                SpriteRenderer.sprite = DownSprite;
            }
            else
            {
                SpriteRenderer.sprite = SideSprite;
            }
        }
    }
}