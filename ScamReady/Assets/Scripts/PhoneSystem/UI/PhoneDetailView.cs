using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneDetailView : MonoBehaviour
    {
        [SerializeField] private TMP_Text callerNameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Button answerButton;
        [SerializeField] private Button hangUpButton;

        private PhoneScenarioController phoneController;

        private void Awake() => ResolveReferences();

        private void OnEnable()
        {
            if (phoneController != null)
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
            if (answerButton != null)
                answerButton.onClick.RemoveListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.RemoveListener(HangUp);
        }

        public void Connect(PhoneScenarioController controller)
        {
            ResolveReferences();
            if (phoneController != null)
                phoneController.Changed -= Refresh;
            if (answerButton != null)
                answerButton.onClick.RemoveListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.RemoveListener(HangUp);

            phoneController = controller;
            if (phoneController != null)
                phoneController.Changed += Refresh;

            if (answerButton != null)
                answerButton.onClick.AddListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.AddListener(HangUp);
            Refresh();
        }

        private void ResolveReferences()
        {
            if (callerNameText == null) callerNameText = FindText("Caller");
            if (stateText == null) stateText = FindText("State");
            if (timeText == null) timeText = FindText("Time");
            if (answerButton == null) answerButton = FindButton("Answer");
            if (hangUpButton == null) hangUpButton = FindButton("HangUp");

            if (callerNameText == null || stateText == null || timeText == null
                || answerButton == null || hangUpButton == null)
                Debug.LogError("PhoneDetailView 缺少 Caller、State、Time、Answer 或 HangUp 引用。", this);
        }

        private void Update()
        {
            if (phoneController != null && phoneController.CallState == PhoneCallState.InCall)
                RefreshTime();
        }

        private void Refresh()
        {
            if (phoneController == null || callerNameText == null || stateText == null
                || timeText == null || answerButton == null || hangUpButton == null)
                return;

            callerNameText.text = phoneController.Level != null ? phoneController.Level.CallerName : string.Empty;
            bool incoming = phoneController.CallState == PhoneCallState.Incoming;
            stateText.gameObject.SetActive(incoming);
            bool inCall = phoneController.CallState == PhoneCallState.InCall;
            timeText.gameObject.SetActive(inCall);
            answerButton.gameObject.SetActive(incoming);
            hangUpButton.gameObject.SetActive(phoneController.CallState != PhoneCallState.Completed);
            stateText.text = "Waiting for answer";
            RefreshTime();
        }

        private void RefreshTime()
        {
            if (phoneController == null || timeText == null) return;
            var duration = TimeSpan.FromSeconds(phoneController.CallDurationSeconds);
            timeText.text = duration.TotalHours >= 1d
                ? duration.ToString(@"hh\:mm\:ss")
                : duration.ToString(@"mm\:ss");
        }

        private void AnswerCall() => phoneController.AnswerCall();
        private void HangUp() => phoneController.HangUp();

        private TMP_Text FindText(string objectName)
        {
            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
                if (text.gameObject.name == objectName)
                    return text;
            return null;
        }

        private Button FindButton(string objectName)
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
                if (button.gameObject.name == objectName)
                    return button;
            return null;
        }
    }
}
