using System.Text;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;

namespace ScamReady.Evidence
{
    /// <summary>挂在常驻 EvidenceUI 上，展示已收集的摘要。所有窗口引用在 Inspector 绑定。</summary>
    public sealed class EvidenceSummaryView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private UnityEngine.UI.Button openButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private GameObject window;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private UnityEngine.UI.ScrollRect summaryScroll;
        private ScenarioSession displayedSession;
        private int displayedCount;
        private bool resetScroll;

        private void Awake()
        {
            openButton.onClick.AddListener(controller.OpenEvidenceSummary);
            closeButton.onClick.AddListener(controller.CloseEvidenceSummary);
            summaryText.richText = false;
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            var session = controller.Session;
            int count = session == null ? 0 : session.CollectedEvidenceIds.Count;
            resetScroll |= session != displayedSession || count != displayedCount;
            displayedSession = session;
            displayedCount = count;
            buttonText.text = "Notes (" + count + ")";
            emptyState.SetActive(count == 0);

            var text = new StringBuilder();
            if (session != null)
            {
                foreach (string id in session.CollectedEvidenceIds)
                {
                    var placement = controller.FindEvidencePlacement(id);
                    if (placement == null) continue;
                    if (text.Length > 0) text.Append("\n\n");
                    text.Append(placement.Evidence.Title).Append('\n');
                    text.Append("Source: ").Append(placement.Page.SiteName).Append('\n');
                    text.Append(placement.Evidence.Summary);
                }
            }
            summaryText.text = text.ToString();
            bool visible = session != null && session.IsEvidenceSummaryOpen;
            window.SetActive(visible);
            // 实际打开并完成布局后再重置位置，关闭期间保留重置请求。
            if (visible && resetScroll)
            {
                Canvas.ForceUpdateCanvases();
                summaryScroll.StopMovement();
                summaryScroll.verticalNormalizedPosition = 1f;
                resetScroll = false;
            }
        }
    }
}
