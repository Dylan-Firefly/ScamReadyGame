using System.Collections.Generic;
using ScamReady.Email;
using ScamReady.Evidence;
using ScamReady.Feedback;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Scenarios
{
    [CreateAssetMenu(menuName = "Scam Ready/Email Scenario", fileName = "EmailScenario")]
    public sealed class EmailScenarioDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 5)] private string setup;
        [SerializeField] private string gameDateText;
        [SerializeField, Tooltip("Enable for genuine notices. Used during evaluation only.")]
        private bool isLegitimate;
        [SerializeField] private EmailData email = new EmailData();
        [SerializeField, Tooltip("Official sources for this scenario. The browser shows two entries.")]
        private VerificationPageDefinition[] verificationPages = new VerificationPageDefinition[0];
        [SerializeField, Tooltip("Pages reached through message links, outside the trusted homepage sources.")]
        private VerificationPageDefinition[] linkedPages = new VerificationPageDefinition[0];
        [SerializeField, Tooltip("Page content and evidence placements. Use unique evidence IDs.")]
        private EvidencePlacement[] evidencePlacements = new EvidencePlacement[0];
        [SerializeField, Tooltip("Required evidence for evaluation. Does not lock the report button.")]
        private EvidenceDefinition[] requiredEvidence = new EvidenceDefinition[0];
        [SerializeField] private ReminderConfig reminder = new ReminderConfig();
        [SerializeField] private ScenarioFeedbackConfig feedback = new ScenarioFeedbackConfig();

        public string Id => id;
        public string Title => title;
        public string Setup => setup;
        public string GameDateText => gameDateText;
        public bool IsLegitimate => isLegitimate;
        public EmailData Email => email;
        public IReadOnlyList<VerificationPageDefinition> VerificationPages => verificationPages;
        public IReadOnlyList<VerificationPageDefinition> LinkedPages => linkedPages;
        public IReadOnlyList<EvidencePlacement> EvidencePlacements => evidencePlacements;
        public IReadOnlyList<EvidenceDefinition> RequiredEvidence => requiredEvidence;
        public ReminderConfig Reminder => reminder;
        public ScenarioFeedbackConfig Feedback => feedback;
    }
}
