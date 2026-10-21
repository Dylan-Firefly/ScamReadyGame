using UnityEngine;

namespace ScamReady.Verification
{
    [CreateAssetMenu(menuName = "Scam Ready/Verification Page", fileName = "VerificationPage")]
    public sealed class VerificationPageDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("Stable page ID. Keep it unchanged when editing content.")]
        private string id;
        [SerializeField] private string entryLabel;
        [SerializeField] private string siteName;
        [SerializeField, Tooltip("Simulated address; never opens an external website.")]
        private string address;
        [SerializeField] private string title;
        [SerializeField, TextArea(8, 24)] private string body;
        [SerializeField, Tooltip("Marks a risky page and enables submission. Hidden from players.")]
        private bool isUnsafe;
        [SerializeField, Tooltip("Submit button label for risky pages. Submission finishes as Proceed.")]
        private string processLabel = "Process";

        public string Id => id;
        public string EntryLabel => entryLabel;
        public string SiteName => siteName;
        public string Address => address;
        public string Title => title;
        public string Body => body;
        public bool IsUnsafe => isUnsafe;
        public string ProcessLabel => processLabel;
    }
}
