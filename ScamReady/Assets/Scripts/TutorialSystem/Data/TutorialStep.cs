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

    /// <summary>简单的教程步骤配置；场景引用与提示文字可在 Inspector 中编辑。</summary>
    [Serializable]
    public sealed class TutorialStep
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 12)] private string body;
        [SerializeField] private string nextLabel = "Next";
        [SerializeField, Tooltip("显示聚焦框的区域；留空时不挖孔。")]
        private RectTransform focusTarget;
        [SerializeField, Tooltip("允许点击的目标；留空时使用聚焦目标。")]
        private RectTransform interactionTarget;
        [SerializeField, Tooltip("可滚动区域的 Viewport；阅读步骤只转发滚动，不放行链接或按钮。")]
        private RectTransform scrollArea;
        [SerializeField] private bool blockInput = true;
        [SerializeField] private bool allowTargetClick;
        [SerializeField, Tooltip("根据目标证据 ID，在当前浏览器页面中绑定动态生成的证据卡片。")]
        private bool focusEvidence;
        [SerializeField] private TutorialCompletion completion;
        [SerializeField] private ScenarioEventType eventType;
        [SerializeField, Tooltip("事件或证据的稳定 ID；事件步骤留空表示不限定目标。")]
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
