using System.Text;
using ScamReady.Scenarios;
using ScamReady.Verification;
using TMPro;
using UnityEngine;

namespace ScamReady.Evidence
{
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
                    if (placement == null)
                    {
                        var line = controller.IsPhoneScenario ? controller.PhoneLevel?.FindDialogLine(id) : null;
                        if (line == null) continue;
                        if (text.Length > 0) text.Append("\n\n");
                        text.Append("Call transcript\nSource: ").Append(controller.CurrentContactName).Append('\n');
                        text.Append(line.Text);
                        continue;
                    }
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

        public void Show()
        {
            if (window.activeSelf) return;
            window.SetActive(true);
        }

        public void Hide()
        {
            if (!window.activeSelf) return;
            window.SetActive(false);
        }

        private void ResetScrollIfNeeded()
        {
            // Reset scrolling only after the window is visible.
            if (!window.activeInHierarchy || !resetScroll) return;
            Canvas.ForceUpdateCanvases();
            summaryScroll.StopMovement();
            summaryScroll.verticalNormalizedPosition = 1f;
            resetScroll = false;
        }
    }
}
