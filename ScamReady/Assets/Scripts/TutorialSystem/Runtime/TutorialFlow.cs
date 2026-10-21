using ScamReady.Evidence;
using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Tutorial
{
    public sealed class TutorialFlow : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private TutorialOverlayView overlay;
        [SerializeField, Tooltip("Browser content containing generated evidence cards.")]
        private Transform evidenceContainer;
        [SerializeField] private TutorialStep[] steps = new TutorialStep[0];

        private ScenarioSession observedSession;
        private int stepIndex;
        private int historyStart;
        private bool refreshRequested;
        private bool showRequested;
        private bool finished;
        private bool hasStarted;

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

        private void RequestRefresh()
        {
            // Unlock Continue only after completing the tutorial scenario.
            if (controller.Session != null && controller.Session.IsComplete)
                GameRunProgress.MarkTutorialCompleted();
            refreshRequested = true;
        }

        private void LateUpdate()
        {
            // Wait for views to refresh before finding dynamic targets.
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
            // Keep tutorial completion separate from scenario resets.
            finished = hasStarted;
            showRequested = !finished;
            hasStarted = true;
            overlay.Hide();
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
            var cards = evidenceContainer.GetComponentsInChildren<EvidenceSpotView>(false);
            int index = 0;
            foreach (var placement in controller.EvidencePlacements)
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
