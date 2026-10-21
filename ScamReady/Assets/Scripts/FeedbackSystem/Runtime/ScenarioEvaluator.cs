using System;
using System.Collections.Generic;
using ScamReady.Responses;
using ScamReady.Scenarios;

namespace ScamReady.Feedback
{
    public static class ScenarioEvaluator
    {
        public static ScenarioEvaluation Evaluate(EmailScenarioDefinition definition, ScenarioSession session)
        {
            if (!session.Decision.HasValue)
                throw new InvalidOperationException("必须先记录最终选择，再评估本轮结果。");

            var requiredIds = new HashSet<string>();
            foreach (var evidence in definition.RequiredEvidence)
                if (evidence != null && !string.IsNullOrWhiteSpace(evidence.Id)) requiredIds.Add(evidence.Id);


            var response = session.Decision.Value;

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
