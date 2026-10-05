using System;
using ScamReady.Responses;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>统一接收操作并更新会话；不依赖具体邮件文案或窗口布局。</summary>
    public sealed class ScenarioController : MonoBehaviour
    {
        [SerializeField, Tooltip("绑定本关的 EmailScenarioDefinition 内容资产。")]
        private EmailScenarioDefinition definition;
        private double startedAt;

        public EmailScenarioDefinition Definition => definition;
        public ScenarioSession Session { get; private set; }
        public event Action Changed;

        public VerificationPageDefinition CurrentVerificationPage
        {
            get
            {
                if (Session == null || Session.CurrentVerificationPageId == null) return null;
                foreach (var page in definition.VerificationPages)
                    if (page != null && page.Id == Session.CurrentVerificationPageId) return page;
                return null;
            }
        }

        private void Start() => Restart();

        public void Restart()
        {
            if (definition == null)
            {
                Debug.LogError("ScenarioController 缺少关卡配置资产。", this);
                return;
            }

            startedAt = Time.realtimeSinceStartupAsDouble;
            Session = new ScenarioSession(definition.Id, definition.Email.Id);
            Changed?.Invoke();
        }

        public void OpenEmail()
        {
            if (Session != null && Session.OpenEmail(ElapsedSeconds)) Changed?.Invoke();
        }

        public void CloseEmail()
        {
            if (Session != null && Session.CloseEmail(false, ElapsedSeconds)) Changed?.Invoke();
        }

        public void StopContact()
        {
            if (Session != null && Session.CloseEmail(true, ElapsedSeconds)) Changed?.Invoke();
        }

        public void ChooseResponse(ContactResponse response)
        {
            if (Session != null && Session.ChooseResponse(response, ElapsedSeconds)) Changed?.Invoke();
        }

        public void OpenBrowser()
        {
            if (Session != null && Session.OpenBrowser(ElapsedSeconds)) Changed?.Invoke();
        }

        public void CloseBrowser()
        {
            if (Session != null && Session.CloseBrowser(ElapsedSeconds)) Changed?.Invoke();
        }

        public void OpenBrowserHome()
        {
            if (Session != null && Session.OpenBrowserHome()) Changed?.Invoke();
        }

        public void OpenVerificationPage(VerificationPageDefinition page)
        {
            if (Session == null || page == null || string.IsNullOrWhiteSpace(page.Id)) return;
            // 只允许打开当前关卡配置的页面，避免其他关卡的入口混入会话。
            foreach (var available in definition.VerificationPages)
            {
                if (available != page) continue;
                if (Session.OpenVerificationPage(page.Id, ElapsedSeconds)) Changed?.Invoke();
                return;
            }
        }

        private double ElapsedSeconds => Time.realtimeSinceStartupAsDouble - startedAt;
    }
}
