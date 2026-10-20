using System;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneDetailView : MonoBehaviour
    {
        [SerializeField] private TMP_Text callerNameText;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Button answerButton;
        [SerializeField] private Button hangUpButton;

        private ScenarioController scenarioController;

        private void Awake() => ResolveReferences();

        private void OnEnable()
        {
            if (scenarioController != null)
                scenarioController.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (scenarioController != null)
                scenarioController.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            if (answerButton != null)
                answerButton.onClick.RemoveListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.RemoveListener(HangUp);
        }

        public void Connect(ScenarioController controller)
        {
            ResolveReferences();
            if (scenarioController != null)
                scenarioController.Changed -= Refresh;
            if (answerButton != null)
                answerButton.onClick.RemoveListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.RemoveListener(HangUp);

            scenarioController = controller;
            if (scenarioController != null)
                scenarioController.Changed += Refresh;

            if (answerButton != null)
                answerButton.onClick.AddListener(AnswerCall);
            if (hangUpButton != null)
                hangUpButton.onClick.AddListener(HangUp);
            Refresh();
        }

        private void ResolveReferences()
        {
            if (callerNameText == null) callerNameText = FindText("Caller");
            if (numberText == null) numberText = FindText("Number");
            if (stateText == null) stateText = FindText("State");
            if (timeText == null) timeText = FindText("Time");
            if (answerButton == null) answerButton = FindButton("Answer");
            if (hangUpButton == null) hangUpButton = FindButton("HangUp");

            if (callerNameText == null || numberText == null || stateText == null || timeText == null
                || answerButton == null || hangUpButton == null)
                Debug.LogError("PhoneDetailView 缺少 Caller、Number、State、Time、Answer 或 HangUp 引用。", this);
        }

        private void Update()
        {
            if (scenarioController != null && scenarioController.CallState == PhoneCallState.InCall)
                RefreshTime();
        }

        private void Refresh()
        {
            if (scenarioController == null || callerNameText == null || numberText == null || stateText == null
                || timeText == null || answerButton == null || hangUpButton == null)
                return;

            callerNameText.text = scenarioController.PhoneLevel != null ? scenarioController.PhoneLevel.CallerName : string.Empty;
            numberText.text = scenarioController.PhoneLevel != null ? scenarioController.PhoneLevel.PhoneNumber : string.Empty;
            bool incoming = scenarioController.CallState == PhoneCallState.Incoming;
            stateText.gameObject.SetActive(incoming);
            bool inCall = scenarioController.CallState == PhoneCallState.InCall;
            timeText.gameObject.SetActive(inCall);
            answerButton.gameObject.SetActive(incoming);
            hangUpButton.gameObject.SetActive(scenarioController.CallState != PhoneCallState.Completed);
            stateText.text = "Waiting for answer";
            RefreshTime();
        }

        private void RefreshTime()
        {
            if (scenarioController == null || timeText == null) return;
            var duration = TimeSpan.FromSeconds(scenarioController.CallDurationSeconds);
            timeText.text = duration.TotalHours >= 1d
                ? duration.ToString(@"hh\:mm\:ss")
                : duration.ToString(@"mm\:ss");
        }

        private void AnswerCall() => scenarioController.AnswerCall();
        private void HangUp() => scenarioController.HangUp();

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
