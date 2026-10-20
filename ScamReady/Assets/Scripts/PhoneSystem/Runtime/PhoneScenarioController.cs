using System;
using System.Collections.Generic;
using ScamReady.Responses;
using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Phone
{
    public sealed class PhoneScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private PhoneScenarioCollection collection;
        [SerializeField, Min(0)] private int levelIndex;

        private readonly HashSet<string> collectedEvidenceIds = new HashSet<string>();
        private ScenarioSession lastScenarioSession;
        private double callStartedAt;
        private double completedCallDurationSeconds;

        public PhoneScenarioLevel Level { get; private set; }
        public PhoneCallState CallState { get; private set; }
        public PhoneResponse? Decision { get; private set; }
        public string OutcomeTitle { get; private set; }
        public string OutcomeBody { get; private set; }
        public int CollectedEvidenceCount => collectedEvidenceIds.Count;
        public int RequiredEvidenceCount => Level == null ? 0 : Level.RequiredEvidenceCount;
        public double CallDurationSeconds => CallState == PhoneCallState.InCall
            ? Time.realtimeSinceStartupAsDouble - callStartedAt : completedCallDurationSeconds;
        public bool IsPhoneOpen => scenarioController != null
            && scenarioController.Session != null
            && scenarioController.Session.IsPhoneOpen;
        public event Action Changed;

        private void Awake()
        {
            if (scenarioController == null)
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
            LoadLevel(levelIndex);
        }

        private void OnEnable()
        {
            if (scenarioController == null) return;
            scenarioController.Changed += HandleScenarioChanged;
            lastScenarioSession = scenarioController.Session;
        }

        private void OnDisable()
        {
            if (scenarioController != null)
                scenarioController.Changed -= HandleScenarioChanged;
        }

        public void LoadLevel(int index)
        {
            if (collection == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 PhoneScenarioCollection 资产。", this);
                return;
            }

            if (index < 0 || index >= collection.Levels.Count)
            {
                Debug.LogError($"手机关卡索引 {index} 超出配置范围。", this);
                return;
            }

            var level = collection.Levels[index];
            if (level == null || string.IsNullOrWhiteSpace(level.Id))
            {
                Debug.LogError($"手机关卡 {index} 缺少关卡数据或 ID。", this);
                return;
            }

            levelIndex = index;
            Level = level;
            ResetProgress();
            Changed?.Invoke();
        }

        public void RestartLevel() => LoadLevel(levelIndex);

        public bool HasCollectedEvidence(string lineId) => collectedEvidenceIds.Contains(lineId);

        public void CollectEvidence(string lineId)
        {
            if (CallState != PhoneCallState.InCall || Level == null) return;

            var line = Level.FindDialogLine(lineId);
            if (line == null || !line.IsEvidence || collectedEvidenceIds.Contains(lineId)) return;
            if (scenarioController == null || !scenarioController.CollectPhoneEvidence(lineId)) return;
            collectedEvidenceIds.Add(lineId);
            Changed?.Invoke();
        }

        public void AnswerCall()
        {
            if (CallState != PhoneCallState.Incoming) return;
            CallState = PhoneCallState.InCall;
            callStartedAt = Time.realtimeSinceStartupAsDouble;
            Changed?.Invoke();
        }

        public void HangUp()
        {
            if (CallState == PhoneCallState.Completed) return;

            bool verified = CallState == PhoneCallState.InCall
                && RequiredEvidenceCount > 0
                && CollectedEvidenceCount == RequiredEvidenceCount;
            Finish(verified ? PhoneResponse.Report : PhoneResponse.Ignore);
        }

        public void ChooseResponse(PhoneResponse response)
        {
            if (CallState != PhoneCallState.InCall) return;
            Finish(response);
        }

        public void StopContact()
        {
            // if (CallState != PhoneCallState.InCall) return;
            // if (scenarioController == null)
            // {
            //     Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
            //     return;
            // }

            // completedCallDurationSeconds = CallDurationSeconds;
            // CallState = PhoneCallState.Completed;
            // scenarioController.StopPhoneContact();
            if (scenarioController == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
                return;
            }

            scenarioController.ClosePhone();
        }

        public void OpenPhone()
        {
            if (scenarioController == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
                return;
            }

            scenarioController.OpenPhone();
        }

        public void ClosePhone()
        {
            if (scenarioController == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
                return;
            }

            scenarioController.ClosePhone();
        }

        private void Finish(PhoneResponse response)
        {
            if (Level == null) return;

            var outcome = Level.FindOutcome(response);
            if (outcome == null)
            {
                Debug.LogError($"手机关卡 {Level.Id} 未配置 {response} 结局。", this);
                return;
            }

            if (scenarioController == null
                || !scenarioController.ChoosePhoneResponse(ToContactResponse(response)))
            {
                Debug.LogError($"手机回应 {response} 无法记录到当前会话。", this);
                return;
            }

            Decision = response;
            OutcomeTitle = outcome.Title;
            OutcomeBody = outcome.Body;
            completedCallDurationSeconds = CallDurationSeconds;
            CallState = PhoneCallState.Completed;
            Changed?.Invoke();
        }

        private static ContactResponse ToContactResponse(PhoneResponse response)
        {
            switch (response)
            {
                case PhoneResponse.Trust: return ContactResponse.Proceed;
                case PhoneResponse.Ignore: return ContactResponse.Ignore;
                case PhoneResponse.Report: return ContactResponse.Reject;
                default: throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }

        private void HandleScenarioChanged()
        {
            var session = scenarioController.Session;
            if (!ReferenceEquals(session, lastScenarioSession))
            {
                lastScenarioSession = session;
                if (session != null) ResetProgress();
            }

            Changed?.Invoke();
        }

        private void ResetProgress()
        {
            collectedEvidenceIds.Clear();
            Decision = null;
            OutcomeTitle = string.Empty;
            OutcomeBody = string.Empty;
            CallState = PhoneCallState.Incoming;
            callStartedAt = 0d;
            completedCallDurationSeconds = 0d;
        }
    }
}
