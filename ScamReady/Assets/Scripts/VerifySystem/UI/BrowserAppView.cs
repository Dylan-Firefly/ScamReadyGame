using System.Collections.Generic;
using ScamReady.Evidence;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using ScamReady.UI;

namespace ScamReady.Verification
{
    public sealed class BrowserAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button homeButton;
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject pagePanel;
        [SerializeField, Tooltip("Two source entries matching the scenario's page order.")]
        private VerificationLinkView[] links;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private TMP_Text siteText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.ScrollRect bodyScroll;
        [SerializeField, Tooltip("EvidenceSpot prefab used to create page evidence cards.")]
        private EvidenceSpotView evidenceSpotPrefab;
        [SerializeField, Tooltip("BodyScrollView/Viewport/Content; cards appear after the body text.")]
        private Transform evidenceContainer;
        [SerializeField] private GameObject processPanel;
        [SerializeField] private UnityEngine.UI.Button processButton;
        [SerializeField] private TMP_Text processText;
        private readonly List<EvidenceSpotView> evidenceSpots = new List<EvidenceSpotView>();
        private ScenarioSession displayedSession;

        [Header("Audio")]
        [SerializeField] private UIAudioPlayer audioPlayer;

        [Header("Animation")]
        [SerializeField] private UIWindowAnimator windowAnimator;

        private VerificationPageDefinition displayedPage;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.CloseBrowser);
            homeButton.onClick.AddListener(controller.OpenBrowserHome);
            if (processButton != null) 
                processButton.onClick.AddListener(controller.SubmitBrowserInformation);

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }

            if (windowAnimator == null)
            {
                windowAnimator = window.GetComponent<UIWindowAnimator>();
            }
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void RebuildEvidenceSpots(VerificationPageDefinition page)
        {
            // Hide old cards before deferred destruction updates the layout.
            foreach (var spot in evidenceSpots)
            {
                spot.gameObject.SetActive(false);
                Destroy(spot.gameObject);
            }
            evidenceSpots.Clear();
            if (page == null) return;
            foreach (var placement in controller.EvidencePlacements)
            {
                if (placement == null || placement.Page != page || placement.Evidence == null) continue;
                var spot = Instantiate(evidenceSpotPrefab, evidenceContainer);
                spot.Bind(controller, placement);
                evidenceSpots.Add(spot);
            }
        }

        private void Refresh()
        {
            if (!controller.IsEmailScenario && !controller.IsPhoneScenario)
            {
                SetVisible(false);
                return;
            }

            RefreshLinks();
            bool pageChanged = RefreshContent();
            SetVisible(controller.Session != null && controller.Session.IsBrowserOpen);
            if (pageChanged && displayedPage != null) ResetScroll();
        }

        private void RefreshLinks()
        {
            var pages = controller.VerificationPages;
            for (int i = 0; i < links.Length; i++)
                links[i].Bind(i < pages.Count && pages[i] != null && !pages[i].IsUnsafe ? pages[i] : null,
                    controller.OpenVerificationPage, controller.OpenLink);
        }

        private bool RefreshContent()
        {
            var session = controller.Session;
            var page = controller.CurrentVerificationPage;
            bool pageChanged = displayedSession != session || displayedPage != page;
            displayedSession = session;
            displayedPage = page;
            if (pageChanged) RebuildEvidenceSpots(page);
            foreach (var spot in evidenceSpots) spot.Refresh();

            RefreshNavigation(page);
            RefreshPageContent(page);
            RefreshProcessAction(page);
            if (page == null) ShowHomePage();
            else ShowInformationPage();
            return pageChanged;
        }

        private void RefreshNavigation(VerificationPageDefinition page)
        {
            homeButton.interactable = page != null;
            BrowserLinkText.SetText(addressText, page == null ? "Official sources" : page.Address, controller.OpenLink);
        }

        private void RefreshPageContent(VerificationPageDefinition page)
        {
            if (page == null) return;
            siteText.text = page.SiteName;
            titleText.text = page.Title;
            BrowserLinkText.SetText(bodyText, page.Body, controller.OpenLink);
        }

        private void RefreshProcessAction(VerificationPageDefinition page)
        {
            bool visible = page != null && page.IsUnsafe;
            if (!visible)
            {
                HideProcessAction();
                return;
            }
            if (processText != null) processText.text = page.ProcessLabel;
            if (processButton != null) processButton.interactable = controller.Session != null && !controller.Session.IsComplete;
            ShowProcessAction();
        }

        private void ShowProcessAction()
        {
            if (processPanel != null && !processPanel.activeSelf) processPanel.SetActive(true);
        }

        private void HideProcessAction()
        {
            if (processPanel != null && processPanel.activeSelf) processPanel.SetActive(false);
        }

        private void ShowHomePage()
        {
            if (homePanel.activeSelf && !pagePanel.activeSelf) return;
            homePanel.SetActive(true);
            pagePanel.SetActive(false);
        }

        private void ShowInformationPage()
        {
            if (pagePanel.activeSelf && !homePanel.activeSelf) return;
            homePanel.SetActive(false);
            pagePanel.SetActive(true);
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        public void Show()
        {
            if (window.activeSelf) return;

            windowAnimator.PlayOpen();
            audioPlayer?.PlayOpen();
        }

        public void Hide()
        {
            if (!window.activeSelf) return;

            audioPlayer?.PlayClose();
            windowAnimator.PlayClose();
        }


        private void ResetScroll()
        {
            Canvas.ForceUpdateCanvases();
            bodyScroll.StopMovement();
            bodyScroll.verticalNormalizedPosition = 1f;
        }
    }
}
