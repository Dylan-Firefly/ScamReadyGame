using System;
using ScamReady.Scenarios;
using UnityEngine;

namespace ScamReady.Tutorial
{
    public enum TutorialCompletion
    {
        Next,
        ScenarioEvent,
        ReminderClosed,
        SummaryClosed
    }

    [Serializable]
    public sealed class TutorialStep
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 12)] private string body;
        [SerializeField] private string nextLabel = "Next";
        [SerializeField, Tooltip("Highlighted area. Leave empty for no spotlight.")]
        private RectTransform focusTarget;
        [SerializeField, Tooltip("Clickable target. Defaults to the highlighted area.")]
        private RectTransform interactionTarget;
        [SerializeField, Tooltip("Reading viewport. Allows scrolling while blocking links and buttons.")]
        private RectTransform scrollArea;
        [SerializeField] private bool blockInput = true;
        [SerializeField] private bool allowTargetClick;
        [SerializeField, Tooltip("Find the evidence card by its ID on the current page.")]
        private bool focusEvidence;
        [SerializeField] private TutorialCompletion completion;
        [SerializeField] private ScenarioEventType eventType;
        [SerializeField, Tooltip("Event or evidence ID. Leave empty to accept any event target.")]
        private string targetId;

        public string Title => title;
        public string Body => body;
        public string NextLabel => nextLabel;
        public RectTransform FocusTarget => focusTarget;
        public RectTransform InteractionTarget => interactionTarget;
        public RectTransform ScrollArea => scrollArea;
        public bool BlockInput => blockInput;
        public bool AllowTargetClick => allowTargetClick;
        public bool FocusEvidence => focusEvidence;
        public TutorialCompletion Completion => completion;
        public ScenarioEventType EventType => eventType;
        public string TargetId => targetId;
    }
}
