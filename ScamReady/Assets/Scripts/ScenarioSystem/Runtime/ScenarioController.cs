using System;
using ScamReady.Responses;
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

        private double ElapsedSeconds => Time.realtimeSinceStartupAsDouble - startedAt;
    }
}
