using UnityEngine;

public class ResponseSystem : MonoBehaviour
{
    public EvidenceSystem evidenceSystem;
    public FeedbackSystem feedbackSystem;

    public void Proceed()
    {
        if (evidenceSystem.HasEvidence())
        {
            feedbackSystem.ShowNeutralFeedback(
                "You proceeded after checking independent evidence."
            );
        }
        else
        {
            feedbackSystem.ShowUnsafeFeedback(
                "You proceeded without independently verifying the claim."
            );
        }
    }

    public void StopContact()
    {
        feedbackSystem.ShowNeutralFeedback(
            "You stopped the contact. You can now investigate the claim through independent sources."
        );
    }

    public void IgnoreContact()
    {
        feedbackSystem.ShowNeutralFeedback(
            "You ignored the contact, but the issue remains unresolved."
        );
    }

    public void RejectContact()
    {
        if (evidenceSystem.HasEvidence())
        {
            feedbackSystem.ShowSafeFeedback(
                "You rejected the suspicious contact after verifying the claim through an independent source."
            );
        }
        else
        {
            feedbackSystem.ShowUnsafeFeedback(
                "You rejected the contact without first verifying the claim."
            );
        }
    }
}