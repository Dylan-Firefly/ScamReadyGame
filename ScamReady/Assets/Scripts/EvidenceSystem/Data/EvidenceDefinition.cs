using UnityEngine;

namespace ScamReady.Evidence
{
    /// <summary>可编辑的证据内容。来源页面和放置关系由关卡指定。</summary>
    [CreateAssetMenu(menuName = "Scam Ready/Evidence", fileName = "Evidence")]
    public sealed class EvidenceDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("稳定且唯一的证据 ID，用于去重、历史与结果评估。")]
        private string id;
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string summary;

        public string Id => id;
        public string Title => title;
        public string Summary => summary;
    }
}
