using ScamReady.Responses;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private ScrollRect bodyScroll;
        [SerializeField] private Button restartButton;
        private ScenarioEvaluation displayedResult;

        private void Awake()
        {
            restartButton.onClick.AddListener(controller.Restart);
            titleText.richText = false;
            bodyText.richText = false;
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
            window.SetActive(result != null);
            if (result == null)
            {
                displayedResult = null;
                return;
            }
            if (result == displayedResult) return;
            displayedResult = result;

            var message = controller.Definition.Feedback.GetMessage(result.Outcome);
            scenarioText.text = "Scenario complete  |  " + controller.Definition.Title;
            titleText.text = message.Title;
            bodyText.text = (message.Body ?? string.Empty)
                .Replace("{evidenceCount}", result.EvidenceCount.ToString())
                .Replace("{requiredCount}", result.RequiredEvidenceCount.ToString())
                .Replace("{collectedRequiredCount}", result.CollectedRequiredEvidenceCount.ToString());

            string response = result.Response == ContactResponse.Reject ? "Reject / Report" : result.Response.ToString();
            string verification = result.HasCompleteVerification ? "Complete verification"
                : result.EvidenceCount == 0 ? "No evidence collected" : "Incomplete verification";
            processText.text = "Your decision: " + response + "\n"
                + "Evidence at decision: " + result.EvidenceCount + "  |  Required: "
                + result.CollectedRequiredEvidenceCount + "/" + result.RequiredEvidenceCount
                + "\n" + verification;

            // 激活窗口并更新文案后计算高度，长反馈可以滚动阅读。
            window.transform.SetAsLastSibling();
            transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            bodyScroll.StopMovement();
            bodyScroll.verticalNormalizedPosition = 1f;
        }
    }
}
