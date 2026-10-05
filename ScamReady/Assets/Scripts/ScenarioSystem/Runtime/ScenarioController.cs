using System;
using ScamReady.Evidence;
using ScamReady.Feedback;
using ScamReady.Responses;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>统一接收操作并更新会话；不依赖具体邮件文案或窗口布局。</summary>
    public sealed class ScenarioController : MonoBehaviour
    {
        [SerializeField, Tooltip("绑定本关的 EmailScenarioDefinition 内容资产。")]
        private EmailScenarioDefinition definition;
        private double startedAt;

        public EmailScenarioDefinition Definition => definition;
        public ScenarioSession Session { get; private set; }
        public ScenarioEvaluation Result { get; private set; }
        public event Action Changed;

        public VerificationPageDefinition CurrentVerificationPage
        {
            get
            {
                if (Session == null || Session.CurrentVerificationPageId == null) return null;
                foreach (var page in definition.VerificationPages)
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
            Changed?.Invoke();
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
            if (Session == null || !Session.ChooseResponse(response, ElapsedSeconds)) return;
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
            // 只允许打开当前关卡配置的页面，避免其他关卡的入口混入会话。
            foreach (var available in definition.VerificationPages)
            {
                if (available != page) continue;
                if (Session.OpenVerificationPage(page.Id, ElapsedSeconds)) Changed?.Invoke();
                return;
            }
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
