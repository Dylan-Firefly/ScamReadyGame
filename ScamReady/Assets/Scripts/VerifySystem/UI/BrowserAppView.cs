using ScamReady.Scenarios;
using TMPro;
using UnityEngine;

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
        private ScenarioSession displayedSession;
        private VerificationPageDefinition displayedPage;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.CloseBrowser);
            homeButton.onClick.AddListener(controller.OpenBrowserHome);
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
            var pages = controller.Definition.VerificationPages;
            for (int i = 0; i < links.Length; i++)
                links[i].Bind(i < pages.Count ? pages[i] : null, controller.OpenVerificationPage);

            var session = controller.Session;
            var page = controller.CurrentVerificationPage;
            bool pageChanged = displayedSession != session || displayedPage != page;
            displayedSession = session;
            displayedPage = page;

            window.SetActive(session != null && session.IsBrowserOpen);
            homePanel.SetActive(page == null);
            pagePanel.SetActive(page != null);
            homeButton.interactable = page != null;
            addressText.text = page == null ? "Official sources" : page.Address;
            if (page == null) return;

            siteText.text = page.SiteName;
            titleText.text = page.Title;
            bodyText.text = page.Body;
            // 换页或重开关卡时从顶部阅读；关闭再打开同一页保留阅读位置。
            if (pageChanged)
            {
                Canvas.ForceUpdateCanvases();
                bodyScroll.StopMovement();
                bodyScroll.verticalNormalizedPosition = 1f;
            }
        }
    }
}
