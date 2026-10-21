using System;
using System.Collections.Generic;
using ScamReady.Responses;
using ScamReady.Scenarios;

namespace ScamReady.Feedback
{
    /// <summary>冻结邮件回应、查验依据与通知性质；后续行为仅由情景反馈作叙事表达。</summary>
    public static class ScenarioEvaluator
    {
        public static ScenarioEvaluation Evaluate(EmailScenarioDefinition definition, ScenarioSession session)
        {
            if (!session.Decision.HasValue)
                throw new InvalidOperationException("必须先记录最终选择，再评估本轮结果。");

            // 以指定证据 ID 判断充分查验，无关证据和重复配置不能凑数。
            var requiredIds = new HashSet<string>();
            foreach (var evidence in definition.RequiredEvidence)
                if (evidence != null && !string.IsNullOrWhiteSpace(evidence.Id)) requiredIds.Add(evidence.Id);


            var response = session.Decision.Value;

            // Reject 使用玩家实际提交的证据。
            // Proceed 和 Ignore 保持原有的证据快照。
            var evidenceIds = response == ContactResponse.Reject
                ? session.SubmittedEvidenceIds
                : session.DecisionEvidenceIds;

            var snapshot = new HashSet<string>(evidenceIds);

            int collectedRequired = 0;
            foreach (string id in requiredIds)
                if (snapshot.Contains(id)) collectedRequired++;

            
            ScenarioOutcome outcome;
            if (response == ContactResponse.Proceed) outcome = ScenarioOutcome.Proceed;
            else if (response == ContactResponse.Ignore) outcome = ScenarioOutcome.Ignore;
            else if (requiredIds.Count > 0 && collectedRequired == requiredIds.Count)
                outcome = ScenarioOutcome.ReportVerified;
            else outcome = snapshot.Count == 0
                ? ScenarioOutcome.ReportUnverified : ScenarioOutcome.ReportPartial;

            return new ScenarioEvaluation(response, outcome, snapshot.Count, collectedRequired,
                requiredIds.Count, session.DecisionUnsafeLinkCount, definition.IsLegitimate);
        }
    }
}
