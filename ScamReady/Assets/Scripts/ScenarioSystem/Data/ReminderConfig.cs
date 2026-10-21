using System;
using UnityEngine;

namespace ScamReady.Scenarios
{
    [Serializable]
    public sealed class ReminderConfig
    {
        [SerializeField] private string id;
        [SerializeField, Min(1), Tooltip("Unique evidence count needed to show the reminder before a final decision.")]
        private int triggerEvidenceCount = 1;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 6)] private string body;

        public string Id => id;
        public int TriggerEvidenceCount => triggerEvidenceCount;
        public string Title => title;
        public string Body => body;
    }
}
