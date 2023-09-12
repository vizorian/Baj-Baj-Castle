using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        private RectTransform backgroundTransform;
        private RectTransform canvasTransform;
        private RectTransform rectTransform;
        private TextMeshProUGUI text;

        public static Tooltip Instance { get; private set; }

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;

            canvasTransform = GameObject.Find("GameCanvas").GetComponent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();
            backgroundTransform = transform.Find("Background").GetComponent<RectTransform>();
            text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            backgroundTransform.sizeDelta = Vector2.zero;
            text.text = "";
        }

        [UsedImplicitly]
        private void Update()
        {
            UpdatePosition();
        }

        // Update the position of the tooltip
        private void UpdatePosition()
        {
            var newPosition = Input.mousePosition / canvasTransform.localScale.x;

            // Check if tooltip is off screen and adjust position accordingly
            if (newPosition.x + backgroundTransform.sizeDelta.x > canvasTransform.sizeDelta.x)
                newPosition.x = canvasTransform.sizeDelta.x - backgroundTransform.sizeDelta.x;

            if (newPosition.y + backgroundTransform.sizeDelta.y > canvasTransform.sizeDelta.y)
                newPosition.y = canvasTransform.sizeDelta.y - backgroundTransform.sizeDelta.y;

            rectTransform.anchoredPosition = newPosition;
        }

        // Set the tooltip text and activate it
        private void ShowTooltip(string newText)
        {
            SetText(newText);
            UpdatePosition();
            gameObject.SetActive(true);
        }

        // Disable the tooltip
        private void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        // Set the tooltip text and update the size of the background
        private void SetText(string newText)
        {
            text.SetText(newText);
            text.ForceMeshUpdate();
            var textSize = text.GetRenderedValues(false);
            var padding = new Vector2(10, 10);
            backgroundTransform.sizeDelta = textSize + padding;
        }

        public static void ShowTooltip_Static(string text)
        {
            Instance.ShowTooltip(text);
        }

        public static void HideTooltip_Static()
        {
            Instance.HideTooltip();
        }
    }
}