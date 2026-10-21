using ScamReady.Responses;

namespace ScamReady.Feedback
{
    public enum ScenarioOutcome
    {
        Proceed,
        Ignore,
        ReportUnverified,
        ReportPartial,
        ReportVerified
    }

    public sealed class ScenarioEvaluation
    {
        public ContactResponse Response { get; }
        public ScenarioOutcome Outcome { get; }
        public int EvidenceCount { get; }
        public int CollectedRequiredEvidenceCount { get; }
        public int RequiredEvidenceCount { get; }
        public int UnsafeLinkCount { get; }
        public bool IsLegitimateContact { get; }
        public bool HasCompleteVerification => RequiredEvidenceCount > 0
            && CollectedRequiredEvidenceCount == RequiredEvidenceCount;
        // Complete evidence does not guarantee a correct decision.
        public bool HasSupportedDecision => HasCompleteVerification
            && Response == (IsLegitimateContact ? ContactResponse.Proceed : ContactResponse.Reject);

        internal ScenarioEvaluation(ContactResponse response, ScenarioOutcome outcome,
            int evidenceCount, int collectedRequiredEvidenceCount, int requiredEvidenceCount,
            int unsafeLinkCount, bool isLegitimateContact)
        {
            Response = response;
            Outcome = outcome;
            EvidenceCount = evidenceCount;
            CollectedRequiredEvidenceCount = collectedRequiredEvidenceCount;
            RequiredEvidenceCount = requiredEvidenceCount;
            UnsafeLinkCount = unsafeLinkCount;
            IsLegitimateContact = isLegitimateContact;
        }
    }
}
