using ScamReady.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneAppView : MonoBehaviour
    {
        [SerializeField] private PhoneScenarioController phoneController;
        [SerializeField] private GameObject window;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text scenarioTitleText;
        [SerializeField] private TMP_Text setupText;
        [SerializeField] private TMP_Text senderText;
        [SerializeField] private TMP_Text phoneNumberText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text noticeText;
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private TMP_Text outcomeTitleText;
        [SerializeField] private TMP_Text outcomeBodyText;
        [SerializeField] private Transform interactionContainer;
        [SerializeField] private Button interactionButtonPrefab;
        [SerializeField] private UIWindowAnimator windowAnimator;

        private void Awake()
        {
            if (phoneController == null || window == null || closeButton == null
                || scenarioTitleText == null || setupText == null || senderText == null
                || phoneNumberText == null || messageText == null || noticeText == null
                || outcomePanel == null || outcomeTitleText == null || outcomeBodyText == null
                || interactionContainer == null || interactionButtonPrefab == null)
            {
                Debug.LogError("PhoneAppView 缺少必需的 UI 或控制器引用。", this);
                enabled = false;
                return;
            }

            closeButton.onClick.AddListener(phoneController.ClosePhone);
            if (windowAnimator == null)
                windowAnimator = window.GetComponent<UIWindowAnimator>();
        }

        private void OnEnable()
        {
            phoneController.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (phoneController != null)
                phoneController.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            if (closeButton != null && phoneController != null)
                closeButton.onClick.RemoveListener(phoneController.ClosePhone);
        }

        private void Refresh()
        {
            RefreshContent();
            SetVisible(phoneController.IsPhoneOpen);
        }

        private void RefreshContent()
        {
            var level = phoneController.Level;
            var message = phoneController.CurrentMessage;
            if (level == null || message == null) return;

            scenarioTitleText.text = level.Title;
            setupText.text = level.Setup;
            senderText.text = level.Sender;
            phoneNumberText.text = level.PhoneNumber;
            messageText.text = message.Text;
            noticeText.text = phoneController.LatestNotice;
            noticeText.gameObject.SetActive(!string.IsNullOrEmpty(phoneController.LatestNotice));

            bool decided = phoneController.Decision.HasValue;
            outcomePanel.SetActive(decided);
            outcomeTitleText.text = phoneController.OutcomeTitle;
            outcomeBodyText.text = phoneController.OutcomeBody;
            RebuildInteractions(message, decided);
        }

        private void RebuildInteractions(PhoneMessageData message, bool decided)
        {
            for (int i = interactionContainer.childCount - 1; i >= 0; i--)
                Destroy(interactionContainer.GetChild(i).gameObject);

            if (decided) return;

            if (interactionButtonPrefab == null)
            {
                Debug.LogError("PhoneAppView 缺少交互按钮 Prefab。", this);
                return;
            }

            foreach (var interaction in message.Interactions)
            {
                var button = Instantiate(interactionButtonPrefab, interactionContainer);
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label == null)
                {
                    Debug.LogError("手机交互按钮 Prefab 缺少 TMP_Text 子对象。", button);
                    button.interactable = false;
                    continue;
                }

                label.text = interaction.Label;
                string interactionId = interaction.Id;
                button.onClick.AddListener(() => phoneController.SelectInteraction(interactionId));
            }
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        public void Show()
        {
            if (window.activeSelf) return;
            if (windowAnimator != null) windowAnimator.PlayOpen();
            else window.SetActive(true);
        }

        public void Hide()
        {
            if (!window.activeSelf) return;
            if (windowAnimator != null) windowAnimator.PlayClose();
            else window.SetActive(false);
        }
    }
}
