using System;
using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Phone
{
    public sealed class PhoneScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private PhoneScenarioCollection collection;
        [SerializeField, Min(0)] private int levelIndex;

        private ScenarioSession lastScenarioSession;

        public PhoneScenarioLevel Level { get; private set; }
        public PhoneMessageData CurrentMessage { get; private set; }
        public PhoneResponse? Decision { get; private set; }
        public string OutcomeTitle { get; private set; }
        public string OutcomeBody { get; private set; }
        public string LatestNotice { get; private set; }
        public bool IsPhoneOpen => scenarioController != null
            && scenarioController.Session != null
            && scenarioController.Session.IsPhoneOpen;
        public event Action Changed;

        private void Awake()
        {
            if (scenarioController == null)
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
            LoadLevel(levelIndex);
        }

        private void OnEnable()
        {
            if (scenarioController != null)
            {
                scenarioController.Changed += HandleScenarioChanged;
                lastScenarioSession = scenarioController.Session;
            }
        }

        private void OnDisable()
        {
            if (scenarioController != null)
                scenarioController.Changed -= HandleScenarioChanged;
        }

        public void LoadLevel(int index)
        {
            if (collection == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 PhoneScenarioCollection 资产。", this);
                return;
            }

            if (index < 0 || index >= collection.Levels.Count)
            {
                Debug.LogError($"手机关卡索引 {index} 超出配置范围。", this);
                return;
            }

            var level = collection.Levels[index];
            if (level == null || string.IsNullOrWhiteSpace(level.InitialMessageId))
            {
                Debug.LogError($"手机关卡 {index} 缺少关卡数据或起始消息 ID。", this);
                return;
            }

            var initialMessage = level.FindMessage(level.InitialMessageId);
            if (initialMessage == null)
            {
                Debug.LogError($"手机关卡 {level.Id} 找不到起始消息 {level.InitialMessageId}。", this);
                return;
            }

            levelIndex = index;
            Level = level;
            CurrentMessage = initialMessage;
            Decision = null;
            OutcomeTitle = string.Empty;
            OutcomeBody = string.Empty;
            LatestNotice = string.Empty;
            Changed?.Invoke();
        }

        public void RestartLevel() => LoadLevel(levelIndex);

        public void SelectInteraction(string interactionId)
        {
            if (Level == null || CurrentMessage == null || Decision.HasValue) return;

            var interaction = CurrentMessage.FindInteraction(interactionId);
            if (interaction == null)
            {
                Debug.LogWarning($"当前手机消息中找不到交互项：{interactionId}", this);
                return;
            }

            switch (interaction.Type)
            {
                case PhoneInteractionType.ShowMessage:
                    if (!ShowMessage(interaction.TargetMessageId)) return;
                    LatestNotice = string.Empty;
                    break;
                case PhoneInteractionType.OpenLink:
                    if (string.IsNullOrWhiteSpace(interaction.LinkAddress))
                    {
                        Debug.LogWarning($"手机链接交互 {interaction.Id} 未配置地址。", this);
                        return;
                    }

                    LatestNotice = interaction.LinkAddress + "\n" + interaction.LinkPreview;

                    if (!string.IsNullOrWhiteSpace(interaction.TargetMessageId)
                        && !ShowMessage(interaction.TargetMessageId)) return;
                    break;
                case PhoneInteractionType.ChooseResponse:
                    Decision = interaction.Response;
                    OutcomeTitle = interaction.OutcomeTitle;
                    OutcomeBody = interaction.OutcomeBody;
                    break;
                default:
                    Debug.LogError($"手机交互 {interaction.Id} 使用了不支持的类型。", this);
                    return;
            }

            Changed?.Invoke();
        }

        private bool ShowMessage(string messageId)
        {
            var target = Level.FindMessage(messageId);
            if (target == null)
            {
                Debug.LogWarning($"手机关卡 {Level.Id} 的交互目标消息不存在：{messageId}", this);
                return false;
            }

            CurrentMessage = target;
            return true;
        }

        private void HandleScenarioChanged()
        {
            var session = scenarioController.Session;
            if (!ReferenceEquals(session, lastScenarioSession))
            {
                lastScenarioSession = session;
                if (session != null && Level != null)
                    ResetProgress();
            }

            Changed?.Invoke();
        }

        private void ResetProgress()
        {
            CurrentMessage = Level.FindMessage(Level.InitialMessageId);
            Decision = null;
            OutcomeTitle = string.Empty;
            OutcomeBody = string.Empty;
            LatestNotice = string.Empty;
        }

        public void OpenPhone()
        {
            if (scenarioController == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
                return;
            }

            scenarioController.OpenPhone();
        }

        public void ClosePhone()
        {
            if (scenarioController == null)
            {
                Debug.LogError("PhoneScenarioController 缺少 ScenarioController 引用。", this);
                return;
            }

            scenarioController.ClosePhone();
        }
    }
}
