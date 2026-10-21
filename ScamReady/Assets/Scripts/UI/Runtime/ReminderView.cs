using ScamReady.Scenarios;
using ScamReady.Verification;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    public sealed class ReminderView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject panel;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        private RectTransform panelRect;
        private float titleTopPadding;
        private float titleBodySpacing;
        private float bottomPadding;
        private float minimumTitleHeight;
        private float minimumBodyHeight;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.DismissReminder);
            panelRect = (RectTransform)panel.transform;
            var titleRect = titleText.rectTransform;
            var bodyRect = bodyText.rectTransform;
            titleTopPadding = -titleRect.offsetMax.y;
            titleBodySpacing = titleRect.offsetMin.y - bodyRect.offsetMax.y;
            bottomPadding = panelRect.rect.height + bodyRect.offsetMin.y;
            minimumTitleHeight = titleRect.rect.height;
            minimumBodyHeight = bodyRect.rect.height;
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            var session = controller.Session;
            bool visible = session != null && session.IsReminderVisible && !session.IsEvidenceSummaryOpen;
            if (visible) RefreshContent();
            SetVisible(visible);
        }

        private void RefreshContent()
        {
            titleText.text = controller.Definition.Reminder.Title;
            BrowserLinkText.SetText(bodyText, controller.Definition.Reminder.Body, controller.OpenLink);
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        public void Show()
        {
            if (!panel.activeSelf) panel.SetActive(true);
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            var titleRect = titleText.rectTransform;
            var bodyRect = bodyText.rectTransform;
            float titleHeight = Mathf.Max(minimumTitleHeight,
                Mathf.Ceil(titleText.GetPreferredValues(titleRect.rect.width, Mathf.Infinity).y));
            float bodyHeight = Mathf.Max(minimumBodyHeight,
                Mathf.Ceil(bodyText.GetPreferredValues(bodyRect.rect.width, Mathf.Infinity).y));
            float height = titleTopPadding + titleHeight + titleBodySpacing + bodyHeight + bottomPadding;
            float heightChange = height - panelRect.rect.height;

            // Grow upward while keeping the bottom edge fixed.
            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            panelRect.anchoredPosition += Vector2.up * (heightChange * panelRect.pivot.y);
            titleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titleHeight);
            titleRect.anchoredPosition = new Vector2(titleRect.anchoredPosition.x,
                -titleTopPadding - titleHeight * (1f - titleRect.pivot.y));
            bodyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bodyHeight);
            bodyRect.anchoredPosition = new Vector2(bodyRect.anchoredPosition.x,
                -titleTopPadding - titleHeight - titleBodySpacing - bodyHeight * (1f - bodyRect.pivot.y));
        }

        public void Hide()
        {
            if (!panel.activeSelf) return;
            panel.SetActive(false);
        }
    }
}
