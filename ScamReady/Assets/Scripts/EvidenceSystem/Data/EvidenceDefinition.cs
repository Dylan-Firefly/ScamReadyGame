using UnityEngine;

namespace ScamReady.Evidence
{
    [CreateAssetMenu(menuName = "Scam Ready/Evidence", fileName = "Evidence")]
    public sealed class EvidenceDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("Unique evidence ID used for collection and evaluation.")]
        private string id;
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string summary;

        public string Id => id;
        public string Title => title;
        public string Summary => summary;
    }
}
