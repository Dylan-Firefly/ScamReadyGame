using System.Collections.Generic;
using ScamReady.Evidence;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using ScamReady.UI;

namespace ScamReady.Verification
{
    /// <summary>浏览器首页与通用信息页。挂在常驻 BrowserUI 上，所有 UI 引用在 Inspector 绑定。</summary>
    public sealed class BrowserAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button homeButton;
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject pagePanel;
        [SerializeField, Tooltip("绑定两个官网入口组件，顺序对应关卡的 Verification Pages。")]
        private VerificationLinkView[] links;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private TMP_Text siteText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.ScrollRect bodyScroll;
        [SerializeField, Tooltip("绑定 EvidenceSpot Prefab 的组件，按本关页面配置生成卡片。")]
        private EvidenceSpotView evidenceSpotPrefab;
        [SerializeField, Tooltip("绑定 BodyScrollView/Viewport/Content，卡片显示在正文之后。")]
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
            // 销毁在帧末执行，先隐藏旧卡片，避免布局暂时计算出重复高度。
            foreach (var spot in evidenceSpots)
            {
                spot.gameObject.SetActive(false);
                Destroy(spot.gameObject);
            }
            evidenceSpots.Clear();
            if (page == null) return;
            foreach (var placement in controller.Definition.EvidencePlacements)
            {
                if (placement.Page != page || placement.Evidence == null) continue;
                var spot = Instantiate(evidenceSpotPrefab, evidenceContainer);
                spot.Bind(controller, placement);
                evidenceSpots.Add(spot);
            }
        }

        private void Refresh()
        {
            RefreshLinks();
            bool pageChanged = RefreshContent();
            SetVisible(controller.Session != null && controller.Session.IsBrowserOpen);
            if (pageChanged && displayedPage != null) ResetScroll();
        }

        private void RefreshLinks()
        {
            var pages = controller.Definition.VerificationPages;
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

        /// <summary>仅显示浏览器窗口；开窗动效和音效在这里接入。导航操作调用 Controller。</summary>
        public void Show()
        {
            if (window.activeSelf) return;

            windowAnimator.PlayOpen();
            audioPlayer?.PlayOpen();
        }

        /// <summary>仅隐藏浏览器窗口，保留当前页面和阅读位置。</summary>
        public void Hide()
        {
            if (!window.activeSelf) return;

            audioPlayer?.PlayClose();
            windowAnimator.PlayClose();
        }


        private void ResetScroll()
        {
            // 换页或重开关卡时从顶部阅读；关闭再打开同一页保留阅读位置。
            Canvas.ForceUpdateCanvases();
            bodyScroll.StopMovement();
            bodyScroll.verticalNormalizedPosition = 1f;
        }
    }
}
