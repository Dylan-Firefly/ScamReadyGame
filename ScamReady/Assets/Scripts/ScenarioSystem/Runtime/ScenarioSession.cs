using System.Collections.Generic;
using ScamReady.Responses;

namespace ScamReady.Scenarios
{
    public enum ScenarioEventType
    {
        EmailReceived,
        EmailOpened,
        EmailClosed,
        ContactStopped,
        ResponseChosen
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
        private readonly string emailId;

        public string ScenarioId { get; }
        public bool IsEmailOpen { get; private set; }
        public bool HasReadEmail { get; private set; }
        public bool HasUnreadEmail => !HasReadEmail;
        public ContactResponse? Decision { get; private set; }
        public IReadOnlyList<ScenarioEvent> History { get; }

        internal ScenarioSession(string scenarioId, string emailId)
        {
            ScenarioId = scenarioId;
            this.emailId = emailId;
            History = history.AsReadOnly();
            Record(ScenarioEventType.EmailReceived, emailId, 0);
        }

        internal bool OpenEmail(double elapsedSeconds)
        {
            if (IsEmailOpen) return false;
            IsEmailOpen = true;
            HasReadEmail = true;
            Record(ScenarioEventType.EmailOpened, emailId, elapsedSeconds);
            return true;
        }

        internal bool CloseEmail(bool stopContact, double elapsedSeconds)
        {
            if (!IsEmailOpen) return false;
            IsEmailOpen = false;
            Record(stopContact ? ScenarioEventType.ContactStopped : ScenarioEventType.EmailClosed,
                emailId, elapsedSeconds);
            return true;
        }

        internal bool ChooseResponse(ContactResponse response, double elapsedSeconds)
        {
            if (!IsEmailOpen || Decision.HasValue) return false;
            Decision = response;
            Record(ScenarioEventType.ResponseChosen, response.ToString(), elapsedSeconds);
            return true;
        }

        private void Record(ScenarioEventType type, string targetId, double elapsedSeconds)
        {
            history.Add(new ScenarioEvent(history.Count + 1, elapsedSeconds, type, targetId));
        }
    }
}
