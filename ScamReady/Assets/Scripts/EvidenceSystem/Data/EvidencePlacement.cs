using System;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Evidence
{
    [Serializable]
    public sealed class EvidencePlacement
    {
        [SerializeField, Tooltip("A page from this scenario's verification sources.")]
        private VerificationPageDefinition page;
        [SerializeField] private EvidenceDefinition evidence;
        [SerializeField, TextArea(2, 6)] private string content;

        public VerificationPageDefinition Page => page;
        public EvidenceDefinition Evidence => evidence;
        public string Content => content;
    }
}
