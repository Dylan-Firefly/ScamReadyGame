using ScamReady.Scenarios;
using ScamReady.Verification;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    /// <summary>挂在常驻 ReminderUI 上，仅显示或关闭一次提醒，不自动打开邮件。</summary>
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
            // 保留场景配置的边距和最小高度，长文案只增加所需空间。
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

        /// <summary>仅显示提醒面板；显示动效在这里接入，不修改提醒触发或已读状态。</summary>
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

            // 保持面板底边不动，向上扩展，避免链接越过面板或盖住底部操作栏。
            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            panelRect.anchoredPosition += Vector2.up * (heightChange * panelRect.pivot.y);
            titleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titleHeight);
            titleRect.anchoredPosition = new Vector2(titleRect.anchoredPosition.x,
                -titleTopPadding - titleHeight * (1f - titleRect.pivot.y));
            bodyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bodyHeight);
            bodyRect.anchoredPosition = new Vector2(bodyRect.anchoredPosition.x,
                -titleTopPadding - titleHeight - titleBodySpacing - bodyHeight * (1f - bodyRect.pivot.y));
        }

        /// <summary>仅隐藏提醒面板。玩家关闭提醒仍调用 Controller.DismissReminder。</summary>
        public void Hide()
        {
            if (!panel.activeSelf) return;
            panel.SetActive(false);
        }
    }
}
