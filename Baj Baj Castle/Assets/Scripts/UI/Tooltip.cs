using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance { get; private set; }

    private RectTransform canvasTransform;
    private RectTransform rectTransform;
    private TextMeshProUGUI text;
    private RectTransform backgroundTransform;
    private Vector2 position;

    private void Awake()
    {
        Instance = this;

        canvasTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        backgroundTransform = transform.Find("Background").GetComponent<RectTransform>();
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();

        HideTooltip();
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 newPosition = position;

        // Check if tooltip is off screen
        if (newPosition.x + backgroundTransform.sizeDelta.x > canvasTransform.sizeDelta.x)
        {
            newPosition.x = canvasTransform.sizeDelta.x - backgroundTransform.sizeDelta.x;
        }

        if (newPosition.y + backgroundTransform.sizeDelta.y > canvasTransform.sizeDelta.y)
        {
            newPosition.y = canvasTransform.sizeDelta.y - backgroundTransform.sizeDelta.y;
        }

        rectTransform.anchoredPosition = newPosition;
    }

    private void ShowTooltip(string text, Vector2 location, bool useMousePosition)
    {
        if (useMousePosition)
        {
            position = Input.mousePosition / canvasTransform.localScale.x;
        }
        else
        {
            position = Camera.main.WorldToViewportPoint(location) * canvasTransform.sizeDelta;
        }

        SetText(text);
        UpdatePosition();
        gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void SetText(string text)
    {
        this.text.SetText(text);
        this.text.ForceMeshUpdate();
        var textSize = this.text.GetRenderedValues(false);
        var padding = new Vector2(10, 10);
        backgroundTransform.sizeDelta = textSize + padding;
    }

    public static void ShowTooltip_Static(string text, Vector2 position, bool useMousePosition = false)
    {
        Instance.ShowTooltip(text, position, useMousePosition);
    }

    public static void HideTooltip_Static()
    {
        Instance.HideTooltip();
    }
}