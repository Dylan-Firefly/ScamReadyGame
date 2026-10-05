using UnityEngine;

public class EvidenceSystem : MonoBehaviour
{
    public bool hasEvidence = false;

    public void CollectEvidence()
    {
        hasEvidence = true;
        Debug.Log("Evidence collected.");
    }

    public bool HasEvidence()
    {
        return hasEvidence;
    }

    public void ResetEvidence()
    {
        hasEvidence = false;
    }
}