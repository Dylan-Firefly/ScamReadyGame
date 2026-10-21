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

    /// <summary>决策时生成一次的只读结果，不引用可继续变化的玩家进度。</summary>
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
        // 证据完整与判断正确是两件事，不能把充分查验后误报真实通知当作成功。
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
