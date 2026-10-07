using ScamReady.Evidence;
using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Tutorial
{
    /// <summary>只读取现有会话推进教程，不替玩家操作，也不改写玩法或评估状态。</summary>
    public sealed class TutorialFlow : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private TutorialOverlayView overlay;
        [SerializeField, Tooltip("绑定当前浏览器生成证据卡片的 Content。")]
        private Transform evidenceContainer;
        [SerializeField] private TutorialStep[] steps = new TutorialStep[0];

        private ScenarioSession observedSession;
        private int stepIndex;
        private int historyStart;
        private bool refreshRequested;
        private bool showRequested;
        private bool finished;

        private void OnEnable()
        {
            if (controller == null || overlay == null || steps.Length == 0)
            {
                Debug.LogError("TutorialFlow 缺少 Controller、Overlay 或步骤配置。", this);
                return;
            }
            controller.Changed += RequestRefresh;
            overlay.NextRequested += NextStep;
            overlay.SkipRequested += Skip;
            observedSession = null;
            RequestRefresh();
        }

        private void OnDisable()
        {
            if (controller != null) controller.Changed -= RequestRefresh;
            if (overlay == null) return;
            overlay.NextRequested -= NextStep;
            overlay.SkipRequested -= Skip;
            overlay.Hide();
        }

        private void RequestRefresh() => refreshRequested = true;

        private void LateUpdate()
        {
            // 等普通 View 响应完 Changed，再读取动态卡片和窗口，避免依赖订阅顺序。
            if (!refreshRequested || controller == null || overlay == null || steps.Length == 0) return;
            refreshRequested = false;
            var session = controller.Session;
            if (session == null)
            {
                overlay.Hide();
                return;
            }
            if (observedSession != session) ResetForSession(session);
            if (session.IsComplete)
            {
                Finish();
                return;
            }
            if (finished) return;

            if (IsStepComplete(steps[stepIndex], session)) Advance();
            if (!finished && showRequested) ShowCurrentStep();
        }

        private void ResetForSession(ScenarioSession session)
        {
            observedSession = session;
            stepIndex = 0;
            historyStart = session.History.Count;
            finished = false;
            showRequested = true;
        }

        private bool IsStepComplete(TutorialStep step, ScenarioSession session)
        {
            if (step.Completion == TutorialCompletion.ReminderClosed) return !session.IsReminderVisible;
            if (step.Completion == TutorialCompletion.SummaryClosed) return !session.IsEvidenceSummaryOpen;
            if (step.Completion != TutorialCompletion.ScenarioEvent) return false;
            for (int i = historyStart; i < session.History.Count; i++)
            {
                var action = session.History[i];
                if (action.Type == step.EventType
                    && (string.IsNullOrEmpty(step.TargetId) || action.TargetId == step.TargetId)) return true;
            }
            return false;
        }

        public void NextStep()
        {
            if (finished || observedSession == null || observedSession.IsComplete
                || steps[stepIndex].Completion != TutorialCompletion.Next) return;
            Advance();
            RequestRefresh();
        }

        private void Advance()
        {
            stepIndex++;
            if (stepIndex >= steps.Length)
            {
                Finish();
                return;
            }
            historyStart = observedSession.History.Count;
            showRequested = true;
            RequestRefresh();
        }

        private void ShowCurrentStep()
        {
            showRequested = false;
            var step = steps[stepIndex];
            var focus = step.FocusEvidence ? FindEvidenceTarget(step.TargetId) : step.FocusTarget;
            var scroll = step.ScrollArea == null ? null
                : step.ScrollArea.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (step.FocusEvidence && focus != null && scroll != null) RevealEvidence(focus, scroll);
            overlay.ShowStep(step, stepIndex + 1, steps.Length, focus, scroll);
        }

        private RectTransform FindEvidenceTarget(string evidenceId)
        {
            var page = controller.CurrentVerificationPage;
            if (page == null || evidenceContainer == null) return null;
            // 与浏览器的生成顺序对应，只在绑定的容器内寻找，不访问 View 的私有字段。
            var cards = evidenceContainer.GetComponentsInChildren<EvidenceSpotView>(false);
            int index = 0;
            foreach (var placement in controller.Definition.EvidencePlacements)
            {
                if (placement.Page != page || placement.Evidence == null) continue;
                if (placement.Evidence.Id == evidenceId)
                    return index < cards.Length ? cards[index].transform as RectTransform : null;
                index++;
            }
            Debug.LogWarning("教程证据不在当前浏览器页面中：" + evidenceId, this);
            return null;
        }

        private static void RevealEvidence(RectTransform target, UnityEngine.UI.ScrollRect scroll)
        {
            // 阅读说明完成后，仅把目标卡片移入可见区域，不产生任何玩家行为记录。
            Canvas.ForceUpdateCanvases();
            if (scroll.content == null || scroll.viewport == null) return;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, target);
            float offset = 0;
            if (bounds.min.y < scroll.viewport.rect.yMin) offset = scroll.viewport.rect.yMin - bounds.min.y;
            else if (bounds.max.y > scroll.viewport.rect.yMax) offset = scroll.viewport.rect.yMax - bounds.max.y;
            scroll.StopMovement();
            scroll.content.anchoredPosition += new Vector2(0, offset);
        }

        public void Skip() => Finish();

        private void Finish()
        {
            finished = true;
            showRequested = false;
            overlay.Hide();
        }
    }
}
