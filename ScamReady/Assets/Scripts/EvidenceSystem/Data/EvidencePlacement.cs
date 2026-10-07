using System;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Evidence
{
    /// <summary>本关在哪个官网页面显示哪条可点击线索。</summary>
    [Serializable]
    public sealed class EvidencePlacement
    {
        [SerializeField, Tooltip("引用本关 Verification Pages 中的页面资产。")]
        private VerificationPageDefinition page;
        [SerializeField] private EvidenceDefinition evidence;
        [SerializeField, TextArea(2, 6)] private string content;

        public VerificationPageDefinition Page => page;
        public EvidenceDefinition Evidence => evidence;
        public string Content => content;
    }
}
