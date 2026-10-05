using System.Collections.Generic;
using ScamReady.Responses;

namespace ScamReady.Scenarios
{
    public enum ScenarioApp
    {
        Desktop,
        Email,
        Browser,
        Result
    }

    public enum ScenarioEventType
    {
        EmailReceived,
        EmailOpened,
        EmailClosed,
        ContactStopped,
        ResponseChosen,
        BrowserOpened,
        BrowserClosed,
        VerificationPageOpened,
        EvidenceCollected,
        EvidenceSummaryOpened,
        ReminderTriggered,
        ReminderDismissed
    }

    /// <summary>记录操作顺序，后续查验与反馈模块可在此基础上扩展。</summary>
    public sealed class ScenarioEvent
    {
        public int SequenceIndex { get; }
        public double ElapsedSeconds { get; }
        public ScenarioEventType Type { get; }
        public string TargetId { get; }

        internal ScenarioEvent(int sequenceIndex, double elapsedSeconds, ScenarioEventType type, string targetId)
        {
            SequenceIndex = sequenceIndex;
            ElapsedSeconds = elapsedSeconds;
            Type = type;
            TargetId = targetId;
        }
    }

    /// <summary>一次游玩的状态；关卡资产与 UI 都不另存玩家进度。</summary>
    public sealed class ScenarioSession
    {
        private readonly List<ScenarioEvent> history = new List<ScenarioEvent>();
        private readonly List<string> collectedEvidenceIds = new List<string>();
        private readonly string emailId;
        private string reminderId;

        public string ScenarioId { get; }
        public ScenarioApp ActiveApp { get; private set; }
        public bool IsEmailOpen => ActiveApp == ScenarioApp.Email;
        public bool IsBrowserOpen => ActiveApp == ScenarioApp.Browser;
        public string CurrentVerificationPageId { get; private set; }
        public bool HasReadEmail { get; private set; }
        public bool HasUnreadEmail => !HasReadEmail;
        public ContactResponse? Decision { get; private set; }
        public bool IsComplete => Decision.HasValue;
        public IReadOnlyList<ScenarioEvent> History { get; }
        public IReadOnlyList<string> CollectedEvidenceIds { get; }
        public IReadOnlyList<string> DecisionEvidenceIds { get; private set; }
        public int? DecisionSequenceIndex { get; private set; }
        public bool IsEvidenceSummaryOpen { get; private set; }
        public bool HasTriggeredReminder { get; private set; }
        public bool IsReminderVisible { get; private set; }

        internal ScenarioSession(string scenarioId, string emailId)
        {
            ScenarioId = scenarioId;
            this.emailId = emailId;
            History = history.AsReadOnly();
            CollectedEvidenceIds = collectedEvidenceIds.AsReadOnly();
            DecisionEvidenceIds = new List<string>().AsReadOnly();
            Record(ScenarioEventType.EmailReceived, emailId, 0);
        }

        internal bool OpenEmail(double elapsedSeconds)
        {
            if (IsComplete || IsEmailOpen) return false;
            if (IsBrowserOpen) Record(ScenarioEventType.BrowserClosed, ScenarioId, elapsedSeconds);
            ActiveApp = ScenarioApp.Email;
            HasReadEmail = true;
            Record(ScenarioEventType.EmailOpened, emailId, elapsedSeconds);
            return true;
        }

        internal bool CloseEmail(bool stopContact, double elapsedSeconds)
        {
            if (!IsEmailOpen) return false;
            ActiveApp = ScenarioApp.Desktop;
            Record(stopContact ? ScenarioEventType.ContactStopped : ScenarioEventType.EmailClosed,
                emailId, elapsedSeconds);
            return true;
        }

        internal bool ChooseResponse(ContactResponse response, double elapsedSeconds)
        {
            if (!IsEmailOpen || IsComplete) return false;
            if (response != ContactResponse.Proceed && response != ContactResponse.Ignore
                && response != ContactResponse.Reject) return false;
            Decision = response;
            // 保存独立副本，最终选择后的收集不会改变当时的查验依据。
            DecisionEvidenceIds = new List<string>(collectedEvidenceIds).AsReadOnly();
            DecisionSequenceIndex = history.Count + 1;
            ActiveApp = ScenarioApp.Result;
            IsEvidenceSummaryOpen = false;
            IsReminderVisible = false;
            Record(ScenarioEventType.ResponseChosen, response.ToString(), elapsedSeconds);
            return true;
        }

        internal bool OpenBrowser(double elapsedSeconds)
        {
            if (IsComplete || IsBrowserOpen) return false;
            if (IsEmailOpen) CloseEmail(false, elapsedSeconds);
            ActiveApp = ScenarioApp.Browser;
            Record(ScenarioEventType.BrowserOpened, ScenarioId, elapsedSeconds);
            return true;
        }

        internal bool CloseBrowser(double elapsedSeconds)
        {
            if (!IsBrowserOpen) return false;
            ActiveApp = ScenarioApp.Desktop;
            Record(ScenarioEventType.BrowserClosed, ScenarioId, elapsedSeconds);
            return true;
        }

        internal bool OpenBrowserHome()
        {
            if (!IsBrowserOpen || CurrentVerificationPageId == null) return false;
            CurrentVerificationPageId = null;
            return true;
        }

        internal bool OpenVerificationPage(string pageId, double elapsedSeconds)
        {
            if (!IsBrowserOpen || CurrentVerificationPageId == pageId) return false;
            CurrentVerificationPageId = pageId;
            Record(ScenarioEventType.VerificationPageOpened, pageId, elapsedSeconds);
            return true;
        }

        private void Record(ScenarioEventType type, string targetId, double elapsedSeconds)
        {
            history.Add(new ScenarioEvent(history.Count + 1, elapsedSeconds, type, targetId));
        }

        public bool HasCollectedEvidence(string id) => collectedEvidenceIds.Contains(id);

        internal bool CollectEvidence(string id, double elapsedSeconds)
        {
            if (!IsBrowserOpen || HasCollectedEvidence(id)) return false;
            collectedEvidenceIds.Add(id);
            Record(ScenarioEventType.EvidenceCollected, id, elapsedSeconds);
            return true;
        }

        internal bool OpenEvidenceSummary(double elapsedSeconds)
        {
            if (IsComplete || IsEvidenceSummaryOpen) return false;
            IsEvidenceSummaryOpen = true;
            Record(ScenarioEventType.EvidenceSummaryOpened, ScenarioId, elapsedSeconds);
            return true;
        }

        internal bool CloseEvidenceSummary()
        {
            if (!IsEvidenceSummaryOpen) return false;
            IsEvidenceSummaryOpen = false;
            return true;
        }

        internal bool TriggerReminder(string id, double elapsedSeconds)
        {
            if (HasTriggeredReminder || Decision.HasValue) return false;
            reminderId = id;
            HasTriggeredReminder = true;
            IsReminderVisible = true;
            Record(ScenarioEventType.ReminderTriggered, id, elapsedSeconds);
            return true;
        }

        internal bool DismissReminder(double elapsedSeconds)
        {
            if (!IsReminderVisible) return false;
            IsReminderVisible = false;
            Record(ScenarioEventType.ReminderDismissed, reminderId, elapsedSeconds);
            return true;
        }
    }
}
