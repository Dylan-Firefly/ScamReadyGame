using System;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>关卡内的一次提醒。ID 留空时不启用。</summary>
    [Serializable]
    public sealed class ReminderConfig
    {
        [SerializeField] private string id;
        [SerializeField, Min(1), Tooltip("收集多少条不同证据后显示，最终决定后不触发。")]
        private int triggerEvidenceCount = 1;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 6)] private string body;

        public string Id => id;
        public int TriggerEvidenceCount => triggerEvidenceCount;
        public string Title => title;
        public string Body => body;
    }
}
