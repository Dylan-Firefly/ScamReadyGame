using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Email
{
    /// <summary>绑定收件箱列表与详情。挂在常驻 EmailUI 上，关闭 EmailWindow 时保留订阅。</summary>
    public sealed class EmailAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button inboxButton;
        [SerializeField] private EmailInboxItemView inboxItem;
        [SerializeField] private EmailDetailView detail;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.CloseEmail);
            inboxButton.onClick.AddListener(controller.OpenEmail);
            detail.Connect(controller);
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            RefreshContent();
            SetVisible(controller.Session != null && controller.Session.IsEmailOpen);
        }

        private void RefreshContent()
        {
            inboxItem.Bind(controller.Definition.Email);
            detail.Bind(controller.Definition.Email, controller.Session);
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        /// <summary>仅显示邮件窗口；开窗动效和音效在这里接入。玩法操作调用 Controller。</summary>
        public void Show()
        {
            if (window.activeSelf) return;
            window.SetActive(true);
        }

        /// <summary>仅隐藏邮件窗口；关闭动效和音效在这里接入。</summary>
        public void Hide()
        {
            if (!window.activeSelf) return;
            window.SetActive(false);
        }
    }
}
