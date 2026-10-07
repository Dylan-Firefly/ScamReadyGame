using ScamReady.Responses;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using ScamReady.UI;

namespace ScamReady.Feedback
{
    /// <summary>常驻 FeedbackUI 只展示已冻结的结果；重新开始交给流程控制器。</summary>
    public sealed class ScenarioResultView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private TMP_Text scenarioText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text processText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.ScrollRect bodyScroll;
        [SerializeField] private UnityEngine.UI.Button restartButton;

        [Header("Audio")]
        [SerializeField] private UIAudioPlayer audioPlayer;

        private ScenarioEvaluation displayedResult;

        private void Awake()
        {
            restartButton.onClick.AddListener(controller.Restart);
            titleText.richText = false;
            bodyText.richText = false;

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            var result = controller.Result;
            if (result == null)
            {
                displayedResult = null;
                Hide();
                return;
            }

            bool resultChanged = result != displayedResult;
            if (resultChanged)
            {
                RefreshContent(result);
                displayedResult = result;
                PlayResultSound(result);
            }
            Show();
            if (resultChanged) ResetScroll();
        }

        private void RefreshContent(ScenarioEvaluation result)
        {
            var message = controller.Definition.Feedback.GetMessage(result.Outcome);
            scenarioText.text = "Scenario complete  |  " + controller.Definition.Title;
            titleText.text = message.Title;
            string body = message.Body;
            string warning = controller.Definition.Feedback.UnsafeLinkWarning;
            if (result.UnsafeLinkCount > 0 && result.Outcome != ScenarioOutcome.Proceed
                && !string.IsNullOrWhiteSpace(warning)) body += "\n\n" + warning;
            bodyText.text = FormatFeedbackBody(body, result);
            RefreshDecisionSummary(result);
        }

        /// <summary>新结算结果首次显示时，根据结果类型播放对应反馈音。</summary>
        private void PlayResultSound(ScenarioEvaluation result)
        {
            if (audioPlayer == null)
                return;

            if (result.Outcome == ScenarioOutcome.ReportVerified)
            {
                audioPlayer.PlaySuccess();
            }
            else
            {
                audioPlayer.PlayWarning();
            }
        }

        private static string FormatFeedbackBody(string body, ScenarioEvaluation result)
        {
            return (body ?? string.Empty)
                .Replace("{evidenceCount}", result.EvidenceCount.ToString())
                .Replace("{requiredCount}", result.RequiredEvidenceCount.ToString())
                .Replace("{collectedRequiredCount}", result.CollectedRequiredEvidenceCount.ToString())
                .Replace("{unsafeLinkCount}", result.UnsafeLinkCount.ToString());
        }

        private void RefreshDecisionSummary(ScenarioEvaluation result)
        {
            string response = result.Response == ContactResponse.Reject ? "Reject / Report" : result.Response.ToString();
            string verification = result.HasCompleteVerification ? "Complete verification"
                : result.EvidenceCount == 0 ? "No evidence collected" : "Incomplete verification";
            processText.text = "Your decision: " + response + "\n"
                + "Evidence at decision: " + result.EvidenceCount + "  |  Required: "
                + result.CollectedRequiredEvidenceCount + "/" + result.RequiredEvidenceCount
                + "\n" + verification;
        }

        /// <summary>仅显示结果窗口；开窗动效和音效在这里接入，不重新评估结果。</summary>
        public void Show()
        {
            if (window.activeSelf) return;
            window.SetActive(true);
            BringToFront();
        }

        /// <summary>仅隐藏结果窗口。重新开始必须调用 Controller.Restart。</summary>
        public void Hide()
        {
            if (!window.activeSelf) return;
            window.SetActive(false);
        }

        private void BringToFront()
        {
            window.transform.SetAsLastSibling();
            transform.SetAsLastSibling();
        }

        private void ResetScroll()
        {
            // 激活窗口并更新文案后计算高度，长反馈可以滚动阅读。
            Canvas.ForceUpdateCanvases();
            bodyScroll.StopMovement();
            bodyScroll.verticalNormalizedPosition = 1f;
        }
    }
}
