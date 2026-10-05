using ScamReady.Scenarios;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Email
{
    /// <summary>绑定收件箱列表与详情。挂在常驻桌面上，避免窗口关闭后丢失订阅。</summary>
    public sealed class EmailAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button inboxButton;
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
            inboxItem.Bind(controller.Definition.Email);
            detail.Bind(controller.Definition.Email, controller.Session);
            window.SetActive(controller.Session != null && controller.Session.IsEmailOpen);
        }
    }
}
