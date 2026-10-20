using System;
using System.Collections.Generic;
using ScamReady.Evidence;
using ScamReady.Feedback;
using ScamReady.Phone;
using ScamReady.Responses;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>统一接收场景操作并更新会话状态。</summary>
    public sealed class ScenarioController : MonoBehaviour
    {
        [SerializeField, Tooltip("绑定本关的 EmailScenarioDefinition 内容资产。")]
        private EmailScenarioDefinition definition;
        [Header("Phone scenario")]
        [SerializeField] private PhoneScenarioCollection phoneCollection;
        [SerializeField, Min(0)] private int phoneLevelIndex;

        private readonly HashSet<string> collectedPhoneEvidenceIds = new HashSet<string>();
        private double startedAt;
        private double callStartedAt;
        private double completedCallDurationSeconds;

        public EmailScenarioDefinition Definition => definition;
        public ScenarioSession Session { get; private set; }
        public ScenarioEvaluation Result { get; private set; }
        public PhoneScenarioLevel PhoneLevel { get; private set; }
        public PhoneCallState CallState { get; private set; }
        public PhoneResponse? PhoneDecision { get; private set; }
        public string PhoneOutcomeTitle { get; private set; }
        public string PhoneOutcomeBody { get; private set; }
        public int CollectedPhoneEvidenceCount => collectedPhoneEvidenceIds.Count;
        public int RequiredPhoneEvidenceCount => PhoneLevel == null ? 0 : PhoneLevel.RequiredEvidenceCount;
        public double CallDurationSeconds => CallState == PhoneCallState.InCall
            ? Time.realtimeSinceStartupAsDouble - callStartedAt : completedCallDurationSeconds;
        public event Action Changed;

        public VerificationPageDefinition CurrentVerificationPage
        {
            get
            {
                if (Session == null || Session.CurrentVerificationPageId == null) return null;
                foreach (var page in BrowserPages())
                    if (page != null && page.Id == Session.CurrentVerificationPageId) return page;
                return null;
            }
        }

        private void Start() => Restart();

        public void Restart()
        {
            if (definition == null)
            {
                Debug.LogError("ScenarioController 缺少关卡配置资产。", this);
                return;
            }

            startedAt = Time.realtimeSinceStartupAsDouble;
            Session = new ScenarioSession(definition.Id, definition.Email.Id);
            Result = null;
            PhoneLevel = null;
            ResetPhoneProgress();
            if (phoneCollection != null)
                SetPhoneLevel(phoneLevelIndex);
            Changed?.Invoke();
        }

        public void LoadPhoneLevel(int index)
        {
            if (SetPhoneLevel(index))
                Changed?.Invoke();
        }

        private bool SetPhoneLevel(int index)
        {
            if (phoneCollection == null)
            {
                Debug.LogError("ScenarioController 缺少 PhoneScenarioCollection 资产。", this);
                return false;
            }

            if (index < 0 || index >= phoneCollection.Levels.Count)
            {
                Debug.LogError($"手机关卡索引 {index} 超出配置范围。", this);
                return false;
            }

            var level = phoneCollection.Levels[index];
            if (level == null || string.IsNullOrWhiteSpace(level.Id))
            {
                Debug.LogError($"手机关卡 {index} 缺少关卡数据或 ID。", this);
                return false;
            }

            phoneLevelIndex = index;
            PhoneLevel = level;
            ResetPhoneProgress();
            return true;
        }

        private void ResetPhoneProgress()
        {
            collectedPhoneEvidenceIds.Clear();
            PhoneDecision = null;
            PhoneOutcomeTitle = string.Empty;
            PhoneOutcomeBody = string.Empty;
            CallState = PhoneCallState.Incoming;
            callStartedAt = 0d;
            completedCallDurationSeconds = 0d;
        }

        public bool HasCollectedPhoneEvidence(string lineId) =>
            collectedPhoneEvidenceIds.Contains(lineId);

        public bool CollectPhoneEvidence(string lineId)
        {
            if (CallState != PhoneCallState.InCall || PhoneLevel == null
                || string.IsNullOrWhiteSpace(lineId))
                return false;

            var line = PhoneLevel.FindDialogLine(lineId);
            if (line == null || !line.IsEvidence || collectedPhoneEvidenceIds.Contains(lineId)
                || Session == null || !Session.CollectPhoneEvidence(lineId, ElapsedSeconds))
                return false;

            collectedPhoneEvidenceIds.Add(lineId);
            Changed?.Invoke();
            return true;
        }

        public void AnswerCall()
        {
            if (CallState != PhoneCallState.Incoming || PhoneLevel == null) return;
            CallState = PhoneCallState.InCall;
            callStartedAt = Time.realtimeSinceStartupAsDouble;
            Changed?.Invoke();
        }

        public void HangUp()
        {
            if (CallState == PhoneCallState.Completed) return;

            bool verified = CallState == PhoneCallState.InCall
                && RequiredPhoneEvidenceCount > 0
                && CollectedPhoneEvidenceCount == RequiredPhoneEvidenceCount;
            FinishPhoneCall(verified ? PhoneResponse.Report : PhoneResponse.Ignore);
        }

        public void ChoosePhoneResponse(PhoneResponse response)
        {
            if (CallState != PhoneCallState.InCall) return;
            FinishPhoneCall(response);
        }

        private void FinishPhoneCall(PhoneResponse response)
        {
            if (PhoneLevel == null) return;

            var outcome = PhoneLevel.FindOutcome(response);
            if (outcome == null)
            {
                Debug.LogError($"手机关卡 {PhoneLevel.Id} 未配置 {response} 结局。", this);
                return;
            }

            if (!TryChoosePhoneResponse(ToContactResponse(response)))
            {
                Debug.LogError($"手机回应 {response} 无法记录到当前会话。", this);
                return;
            }

            completedCallDurationSeconds = CallDurationSeconds;
            PhoneDecision = response;
            PhoneOutcomeTitle = outcome.Title;
            PhoneOutcomeBody = outcome.Body;
            CallState = PhoneCallState.Completed;
            Changed?.Invoke();
        }

        private bool TryChoosePhoneResponse(ContactResponse response)
        {
            return Session != null && Session.ChoosePhoneResponse(response, ElapsedSeconds);
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

        public void OpenPhone()
        {
            if (Session != null && Session.OpenPhone(ElapsedSeconds)) Changed?.Invoke();
        }
        public void ClosePhone()
        {
            if (Session != null && Session.ClosePhone(ElapsedSeconds)) Changed?.Invoke();
        }

        public void StopPhoneContact()
        {
            if (Session != null && Session.ClosePhone(true, ElapsedSeconds)) Changed?.Invoke();
        }

        public void OpenEmail()
        {
            if (Session != null && Session.OpenEmail(ElapsedSeconds)) Changed?.Invoke();
        }

        public void CloseEmail()
        {
            if (Session != null && Session.CloseEmail(false, ElapsedSeconds)) Changed?.Invoke();
        }

        public void StopContact()
        {
            if (Session != null && Session.CloseEmail(true, ElapsedSeconds)) Changed?.Invoke();
        }

        public void ChooseResponse(ContactResponse response)
        {
            if (Session == null || !Session.IsEmailOpen || Session.IsComplete) return;
            // 配有链接的邮件先进入页面；打开链接不等于已经提交信息。
            if (response == ContactResponse.Proceed && !string.IsNullOrWhiteSpace(definition.Email.ProceedLink))
            {
                OpenLink(definition.Email.ProceedLink);
                return;
            }
            if (!Session.ChooseResponse(response, ElapsedSeconds)) return;
            // 决定、快照与评估完成后统一通知界面，避免显示中间状态。
            Result = ScenarioEvaluator.Evaluate(definition, Session);
            Changed?.Invoke();
        }

        public void OpenBrowser()
        {
            if (Session != null && Session.OpenBrowser(ElapsedSeconds)) Changed?.Invoke();
        }

        public void CloseBrowser()
        {
            if (Session != null && Session.CloseBrowser(ElapsedSeconds)) Changed?.Invoke();
        }

        public void OpenBrowserHome()
        {
            if (Session != null && Session.OpenBrowserHome()) Changed?.Invoke();
        }

        public void OpenVerificationPage(VerificationPageDefinition page)
        {
            if (Session == null || page == null || string.IsNullOrWhiteSpace(page.Id)) return;
            // 主页只提供可信入口，邮件带来的页面必须通过链接访问。
            foreach (var available in definition.VerificationPages)
            {
                if (available != page || page.IsUnsafe) continue;
                if (Session.OpenVerificationPage(page.Id, ElapsedSeconds)) Changed?.Invoke();
                return;
            }
        }

        public void OpenLink(string address)
        {
            if (Session == null || Session.IsComplete || string.IsNullOrWhiteSpace(address)) return;
            foreach (var page in BrowserPages())
            {
                if (page == null || string.IsNullOrWhiteSpace(page.Id)
                    || !string.Equals(page.Address, address, StringComparison.Ordinal)) continue;
                double elapsed = ElapsedSeconds;
                Session.CloseEvidenceSummary();
                if (Session.IsReminderVisible) Session.DismissReminder(elapsed);
                Session.OpenBrowser(elapsed);
                Session.OpenVerificationPage(page.Id, elapsed);
                if (page.IsUnsafe) Session.RecordUnsafeLinkOpened(page.Id, elapsed);
                Changed?.Invoke();
                return;
            }
            Debug.LogWarning("链接未配置对应的本关页面：" + address, this);
        }

        public void SubmitBrowserInformation()
        {
            var page = CurrentVerificationPage;
            if (Session == null || page == null || !page.IsUnsafe
                || !Session.SubmitInformation(page.Id, ElapsedSeconds)) return;
            Result = ScenarioEvaluator.Evaluate(definition, Session);
            Changed?.Invoke();
        }

        private System.Collections.Generic.IEnumerable<VerificationPageDefinition> BrowserPages()
        {
            foreach (var page in definition.VerificationPages) yield return page;
            foreach (var page in definition.LinkedPages) yield return page;
        }

        private double ElapsedSeconds => Time.realtimeSinceStartupAsDouble - startedAt;

        public EvidencePlacement FindEvidencePlacement(string id)
        {
            foreach (var placement in definition.EvidencePlacements)
                if (placement.Evidence != null && placement.Evidence.Id == id) return placement;
            return null;
        }

        public void CollectEvidence(EvidencePlacement placement)
        {
            if (Session == null || placement == null || placement.Evidence == null
                || string.IsNullOrWhiteSpace(placement.Evidence.Id)
                || placement.Page != CurrentVerificationPage) return;

            foreach (var available in definition.EvidencePlacements)
            {
                if (available != placement) continue;
                if (!Session.CollectEvidence(placement.Evidence.Id, ElapsedSeconds)) return;
                var reminder = definition.Reminder;
                if (reminder != null && !string.IsNullOrWhiteSpace(reminder.Id)
                    && Session.CollectedEvidenceIds.Count >= reminder.TriggerEvidenceCount)
                    Session.TriggerReminder(reminder.Id, ElapsedSeconds);
                Changed?.Invoke();
                return;
            }
        }

        public void OpenEvidenceSummary()
        {
            if (Session != null && Session.OpenEvidenceSummary(ElapsedSeconds)) Changed?.Invoke();
        }

        public void CloseEvidenceSummary()
        {
            if (Session != null && Session.CloseEvidenceSummary()) Changed?.Invoke();
        }

        public void DismissReminder()
        {
            if (Session != null && Session.DismissReminder(ElapsedSeconds)) Changed?.Invoke();
        }
    }
}
