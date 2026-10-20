using System;
using System.Collections.Generic;
using ScamReady.Evidence;
using ScamReady.Scenarios;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Phone
{
    public enum PhoneInteractionType
    {
        ShowMessage,
        OpenLink,
        ChooseResponse
    }

    public enum PhoneResponse
    {
        Trust,
        Ignore,
        Report
    }

    public enum PhoneCallState
    {
        Incoming,
        InCall,
        Completed
    }

    [Serializable]
    public sealed class PhoneScenarioLevel
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string callerName;
        [SerializeField] private string sender;
        [SerializeField] private string phoneNumber;
        [SerializeField, TextArea(2, 5)] private string setup;
        [SerializeField] private string initialMessageId;
        [SerializeField] private PhoneDialogLineData[] dialogLines = new PhoneDialogLineData[0];
        [SerializeField] private PhoneOutcomeData[] outcomes = new PhoneOutcomeData[0];
        [SerializeField] private PhoneMessageData[] messages = new PhoneMessageData[0];
        [SerializeField, Tooltip("浏览器主页中可直接访问的可信页面。")]
        private VerificationPageDefinition[] verificationPages = new VerificationPageDefinition[0];
        [SerializeField, Tooltip("通过页面链接访问的其他页面。")]
        private VerificationPageDefinition[] linkedPages = new VerificationPageDefinition[0];
        [SerializeField, Tooltip("页面中可收集的证据及其来源内容。")]
        private EvidencePlacement[] evidencePlacements = new EvidencePlacement[0];
        [SerializeField, Tooltip("完成充分查验所需的关键证据。")]
        private EvidenceDefinition[] requiredEvidence = new EvidenceDefinition[0];

        public string Id => id;
        public string Title => title;
        public string CallerName => string.IsNullOrWhiteSpace(callerName) ? sender : callerName;
        public string PhoneNumber => phoneNumber;
        public string Setup => setup;
        public string InitialMessageId => initialMessageId;
        public IReadOnlyList<PhoneDialogLineData> DialogLines => dialogLines;
        public IReadOnlyList<VerificationPageDefinition> VerificationPages => verificationPages;
        public IReadOnlyList<VerificationPageDefinition> LinkedPages => linkedPages;
        public IReadOnlyList<EvidencePlacement> EvidencePlacements => evidencePlacements;
        public IReadOnlyList<EvidenceDefinition> RequiredEvidence => requiredEvidence;
        public int RequiredEvidenceCount
        {
            get
            {
                var ids = new HashSet<string>();
                foreach (var line in dialogLines)
                    if (line != null && line.IsEvidence && !string.IsNullOrWhiteSpace(line.Id))
                        ids.Add(line.Id);
                foreach (var evidence in requiredEvidence)
                    if (evidence != null && !string.IsNullOrWhiteSpace(evidence.Id))
                        ids.Add(evidence.Id);
                return ids.Count;
            }
        }
        public IReadOnlyList<PhoneMessageData> Messages => messages;

        public PhoneOutcomeData FindOutcome(PhoneResponse response)
        {
            foreach (var outcome in outcomes)
                if (outcome != null && outcome.Response == response)
                    return outcome;
            return null;
        }

        public PhoneDialogLineData FindDialogLine(string lineId)
        {
            foreach (var line in dialogLines)
                if (line != null && line.Id == lineId)
                    return line;
            return null;
        }

        public bool IsRequiredEvidenceCollected(ScenarioSession session)
        {
            if (session == null) return false;

            bool hasRequiredEvidence = false;
            foreach (var line in dialogLines)
            {
                if (line == null || !line.IsEvidence || string.IsNullOrWhiteSpace(line.Id)) continue;
                hasRequiredEvidence = true;
                if (!session.HasCollectedEvidence(line.Id)) return false;
            }

            foreach (var evidence in requiredEvidence)
            {
                if (evidence == null || string.IsNullOrWhiteSpace(evidence.Id)) continue;
                hasRequiredEvidence = true;
                if (!session.HasCollectedEvidence(evidence.Id)) return false;
            }

            return hasRequiredEvidence;
        }

        public EvidencePlacement FindEvidencePlacement(string evidenceId)
        {
            foreach (var placement in evidencePlacements)
                if (placement != null && placement.Evidence != null
                    && placement.Evidence.Id == evidenceId) return placement;
            return null;
        }

        public PhoneMessageData FindMessage(string messageId)
        {
            foreach (var message in messages)
                if (message != null && message.Id == messageId)
                    return message;
            return null;
        }
    }

    [Serializable]
    public sealed class PhoneDialogLineData
    {
        [SerializeField] private string id;
        [SerializeField, TextArea(2, 8)] private string text;
        [SerializeField, Tooltip("点击此对话段即可收集为本关所需证据。")]
        private bool isEvidence;

        public string Id => id;
        public string Text => text;
        public bool IsEvidence => isEvidence;
    }

    [Serializable]
    public sealed class PhoneOutcomeData
    {
        [SerializeField] private PhoneResponse response;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 8)] private string body;

        public PhoneResponse Response => response;
        public string Title => title;
        public string Body => body;
    }

    [Serializable]
    public sealed class PhoneMessageData
    {
        [SerializeField] private string id;
        [SerializeField, TextArea(2, 8)] private string text;
        [SerializeField] private PhoneInteractionData[] interactions = new PhoneInteractionData[0];

        public string Id => id;
        public string Text => text;
        public IReadOnlyList<PhoneInteractionData> Interactions => interactions;

        public PhoneInteractionData FindInteraction(string interactionId)
        {
            foreach (var interaction in interactions)
                if (interaction != null && interaction.Id == interactionId)
                    return interaction;
            return null;
        }
    }

    [Serializable]
    public sealed class PhoneInteractionData
    {
        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private PhoneInteractionType type;
        [SerializeField, Tooltip("ShowMessage / OpenLink 后可跳转到的本关消息 ID。")]
        private string targetMessageId;
        [SerializeField, Tooltip("OpenLink 仅在游戏内展示，不会打开真实网站。")]
        private string linkAddress;
        [SerializeField, TextArea(2, 5)] private string linkPreview;
        [SerializeField] private PhoneResponse response;
        [SerializeField] private string outcomeTitle;
        [SerializeField, TextArea(2, 8)] private string outcomeBody;

        public string Id => id;
        public string Label => label;
        public PhoneInteractionType Type => type;
        public string TargetMessageId => targetMessageId;
        public string LinkAddress => linkAddress;
        public string LinkPreview => linkPreview;
        public PhoneResponse Response => response;
        public string OutcomeTitle => outcomeTitle;
        public string OutcomeBody => outcomeBody;
    }

    [CreateAssetMenu(menuName = "Scam Ready/Phone Scenario Collection", fileName = "PhoneScenarioCollection")]
    public sealed class PhoneScenarioCollection : ScriptableObject
    {
        [SerializeField] private PhoneScenarioLevel[] levels = new PhoneScenarioLevel[0];

        public IReadOnlyList<PhoneScenarioLevel> Levels => levels;
    }
}
