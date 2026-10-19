using System;
using System.Collections.Generic;
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

    [Serializable]
    public sealed class PhoneScenarioLevel
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string sender;
        [SerializeField] private string phoneNumber;
        [SerializeField, TextArea(2, 5)] private string setup;
        [SerializeField] private string initialMessageId;
        [SerializeField] private PhoneMessageData[] messages = new PhoneMessageData[0];

        public string Id => id;
        public string Title => title;
        public string Sender => sender;
        public string PhoneNumber => phoneNumber;
        public string Setup => setup;
        public string InitialMessageId => initialMessageId;
        public IReadOnlyList<PhoneMessageData> Messages => messages;

        public PhoneMessageData FindMessage(string messageId)
        {
            foreach (var message in messages)
                if (message != null && message.Id == messageId)
                    return message;
            return null;
        }
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
