using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO rework tooltips as gameobjects that get created when in use and destroyed when not in use
public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance { get; private set; }
    private RectTransform canvasTransform;
    private RectTransform rectTransform;
    private TextMeshProUGUI text;
    private RectTransform backgroundTransform;
    private Vector2 position;
    private Transform followTarget;
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
        Vector2 newPosition;
        if (followTarget != null)
        {
            newPosition = Camera.main.WorldToViewportPoint(followTarget.position) * canvasTransform.sizeDelta;
        }
        else
        {
            newPosition = Input.mousePosition;
        }

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

    private void ShowTooltip(string text, Transform target)
    {
        followTarget = target;

        if (followTarget != null)
        {
            position = Camera.main.WorldToViewportPoint(followTarget.position) * canvasTransform.sizeDelta;
        }
        else
        {
            position = Input.mousePosition / canvasTransform.localScale.x;
        }

        UpdatePosition();
        SetText(text);
        gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
        followTarget = null;
    }

    private void SetText(string text)
    {
        this.text.SetText(text);
        this.text.ForceMeshUpdate();
        var textSize = this.text.GetRenderedValues(false);
        var padding = new Vector2(10, 10);
        backgroundTransform.sizeDelta = textSize + padding;
    }

    public static void ShowTooltip_Static(string text, Transform target = null)
    {
        Instance.ShowTooltip(text, target);
    }

    public static void HideTooltip_Static()
    {
        Instance.HideTooltip();
    }
}
