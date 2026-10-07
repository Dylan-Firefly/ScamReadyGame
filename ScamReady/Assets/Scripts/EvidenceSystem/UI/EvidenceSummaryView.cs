using System.Text;
using ScamReady.Scenarios;
using ScamReady.Verification;
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
            RefreshEntry(session, count);
            RefreshContent(session, count);
            SetVisible(session != null && session.IsEvidenceSummaryOpen);
            ResetScrollIfNeeded();
        }

        private void RefreshEntry(ScenarioSession session, int count)
        {
            openButton.gameObject.SetActive(session == null || !session.IsComplete);
            buttonText.text = "Notes (" + count + ")";
        }

        private void RefreshContent(ScenarioSession session, int count)
        {
            resetScroll |= session != displayedSession || count != displayedCount;
            displayedSession = session;
            displayedCount = count;
            emptyState.SetActive(count == 0);
            BrowserLinkText.SetText(summaryText, BuildSummaryText(session), controller.OpenLink);
        }

        private string BuildSummaryText(ScenarioSession session)
        {
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
            return text.ToString();
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        /// <summary>仅显示摘要窗口；开窗动效和音效在这里接入。玩家打开笔记仍通过 Controller。</summary>
        public void Show()
        {
            if (window.activeSelf) return;
            window.SetActive(true);
        }

        /// <summary>仅隐藏摘要窗口，保留内容和阅读位置。</summary>
        public void Hide()
        {
            if (!window.activeSelf) return;
            window.SetActive(false);
        }

        private void ResetScrollIfNeeded()
        {
            // 实际打开并完成布局后再重置位置，关闭期间保留重置请求。
            if (!window.activeInHierarchy || !resetScroll) return;
            Canvas.ForceUpdateCanvases();
            summaryScroll.StopMovement();
            summaryScroll.verticalNormalizedPosition = 1f;
            resetScroll = false;
        }
    }
}
