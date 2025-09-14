using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EventCardUI : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Button chooseButton;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;

        private StreamEventSO data;
        private Action<StreamEventSO> onChoose;

        /// <summary>
        /// Bind the card to data and an onChoose callback.
        /// </summary>
        public void Bind(StreamEventSO evt, Action<StreamEventSO> onChooseCallback)
        {
            data = evt;
            onChoose = onChooseCallback;

            if (title) title.text = string.IsNullOrEmpty(evt.title) ? "Untitled Event" : evt.title;
            if (description) description.text = string.IsNullOrEmpty(evt.description) ? "" : evt.description;

            if (chooseButton)
            {
                chooseButton.onClick.RemoveAllListeners();
                chooseButton.onClick.AddListener(OnChooseClicked);
                chooseButton.interactable = true;
            }

            if (canvasGroup) { canvasGroup.alpha = 1f; canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true; }
        }

        private void OnChooseClicked()
        {
            // Guard against double-clicks
            if (chooseButton) chooseButton.interactable = false;
            if (canvasGroup) { canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; }

            var cb = onChoose;
            var evt = data;

            // Null out before invoking to avoid reentrancy issues
            onChoose = null;
            data = null;

            if (cb != null) cb(evt);
        }

        // Optional helpers for polish:
        public void SetHighlight(Color c)
        {
            if (background) background.color = c;
        }

        public void SetEnabled(bool enabled)
        {
            if (chooseButton) chooseButton.interactable = enabled;
            if (canvasGroup)
            {
                canvasGroup.interactable = enabled;
                canvasGroup.blocksRaycasts = enabled;
                canvasGroup.alpha = enabled ? 1f : 0.5f;
            }
        }
    }
}
