using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScamReady.Tutorial
{
    /// <summary>教程表现与输入遮罩；原按钮保留原有层级和监听，滚动单独转发。</summary>
    public sealed class TutorialOverlayView : MonoBehaviour, ICanvasRaycastFilter,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private UnityEngine.UI.Image inputBlocker;
        [SerializeField, Tooltip("依次为左、右、上、下四块遮罩。")]
        private RectTransform[] dimmers;
        [SerializeField] private RectTransform focusFrame;
        [SerializeField] private RectTransform guidePanel;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private TMP_Text nextText;
        [SerializeField] private UnityEngine.UI.Button skipButton;
        [SerializeField] private TMP_Text skipText;

        private readonly Vector3[] corners = new Vector3[4];
        private RectTransform focusTarget;
        private RectTransform clickTarget;
        private UnityEngine.UI.ScrollRect scroll;
        private TutorialStep currentStep;
        private EventSystem lockedEventSystem;
        private bool previousNavigation;
        private PointerEventData forwardedDrag;
        private bool confirmingSkip;
        private int stepNumber;
        private int stepCount;

        public event Action NextRequested;
        public event Action SkipRequested;

        private void Awake()
        {
            nextButton.onClick.AddListener(RequestNext);
            skipButton.onClick.AddListener(RequestSkip);
            titleText.richText = false;
            bodyText.richText = false;
        }

        private void RequestNext()
        {
            // 确认框的右侧按钮提交跳过；普通提示框的右侧按钮仍是 Next。
            if (confirmingSkip) SkipRequested?.Invoke();
            else NextRequested?.Invoke();
        }

        private void RequestSkip()
        {
            if (confirmingSkip) CancelSkipConfirmation();
            else ShowSkipConfirmation();
        }

        public void ShowSkipConfirmation()
        {
            if (currentStep == null || confirmingSkip) return;
            StopForwardedDrag();
            confirmingSkip = true;
            progressText.text = "TUTORIAL";
            titleText.text = "Skip the tutorial?";
            bodyText.text = "Are you sure you want to skip? You can continue exploring on your own. "
                + "Resetting this scenario will not show the tutorial again.";
            nextButton.gameObject.SetActive(true);
            nextText.text = "Skip tutorial";
            skipText.text = "Keep tutorial";
            inputBlocker.gameObject.SetActive(true);
            SetInputLock(true);
            RefreshLayout();
        }

        public void CancelSkipConfirmation()
        {
            if (!confirmingSkip || currentStep == null) return;
            confirmingSkip = false;
            RefreshStepContent();
            inputBlocker.gameObject.SetActive(currentStep.BlockInput);
            SetInputLock(currentStep.BlockInput);
            RefreshLayout();
        }

        private void OnEnable() => Canvas.willRenderCanvases += RefreshLayout;

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= RefreshLayout;
            Hide();
        }

        private void OnDestroy()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(RequestNext);
            if (skipButton != null) skipButton.onClick.RemoveListener(RequestSkip);
        }

        public void ShowStep(TutorialStep step, int number, int count, RectTransform target,
            UnityEngine.UI.ScrollRect scrollRect)
        {
            StopForwardedDrag();
            confirmingSkip = false;
            currentStep = step;
            stepNumber = number;
            stepCount = count;
            focusTarget = target;
            clickTarget = step.InteractionTarget != null ? step.InteractionTarget : target;
            scroll = scrollRect;
            RefreshStepContent();
            inputBlocker.gameObject.SetActive(step.BlockInput);
            SetInputLock(step.BlockInput);
            transform.SetAsLastSibling();
            overlayRoot.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            RefreshLayout();
        }

        private void RefreshStepContent()
        {
            progressText.text = "TUTORIAL  " + stepNumber + " / " + stepCount;
            titleText.text = currentStep.Title;
            bodyText.text = currentStep.Body;
            nextText.text = string.IsNullOrWhiteSpace(currentStep.NextLabel) ? "Next" : currentStep.NextLabel;
            skipText.text = "Skip tutorial";
            nextButton.gameObject.SetActive(currentStep.Completion == TutorialCompletion.Next);
        }

        public void Hide()
        {
            StopForwardedDrag();
            SetInputLock(false);
            confirmingSkip = false;
            currentStep = null;
            focusTarget = null;
            clickTarget = null;
            scroll = null;
            if (overlayRoot != null) overlayRoot.gameObject.SetActive(false);
        }

        private void SetInputLock(bool locked)
        {
            if (locked && lockedEventSystem == null && EventSystem.current != null)
            {
                lockedEventSystem = EventSystem.current;
                previousNavigation = lockedEventSystem.sendNavigationEvents;
                lockedEventSystem.sendNavigationEvents = false;
                lockedEventSystem.SetSelectedGameObject(null);
            }
            else if (!locked && lockedEventSystem != null)
            {
                // 键盘 Submit 也不能绕过聚焦限制；退出时恢复原有导航设置。
                lockedEventSystem.SetSelectedGameObject(null);
                lockedEventSystem.sendNavigationEvents = previousNavigation;
                lockedEventSystem = null;
            }
        }

        private void RefreshLayout()
        {
            if (currentStep == null || !overlayRoot.gameObject.activeInHierarchy) return;
            var area = overlayRoot.rect;
            bool hasFocus = !confirmingSkip && focusTarget != null && focusTarget.gameObject.activeInHierarchy;
            Rect hole = hasFocus ? TargetRect(focusTarget) : new Rect(area.center, Vector2.zero);
            if (hasFocus && currentStep.FocusEvidence && scroll != null && scroll.viewport != null)
                hole = Intersection(hole, TargetRect(scroll.viewport));
            hole = Intersection(hole, area);
            hasFocus &= hole.width > 0 && hole.height > 0;
            focusFrame.gameObject.SetActive(hasFocus);
            if (hasFocus) Place(focusFrame, new Rect(hole.position - Vector2.one * 4, hole.size + Vector2.one * 8));

            bool blockInput = currentStep.BlockInput || confirmingSkip;
            foreach (var dimmer in dimmers) dimmer.gameObject.SetActive(blockInput);
            if (blockInput)
            {
                if (!hasFocus) hole = new Rect(area.center, Vector2.zero);
                Place(dimmers[0], Rect.MinMaxRect(area.xMin, area.yMin, hole.xMin, area.yMax));
                Place(dimmers[1], Rect.MinMaxRect(hole.xMax, area.yMin, area.xMax, area.yMax));
                Place(dimmers[2], Rect.MinMaxRect(hole.xMin, hole.yMax, hole.xMax, area.yMax));
                Place(dimmers[3], Rect.MinMaxRect(hole.xMin, area.yMin, hole.xMax, hole.yMin));
            }
            LayoutGuide(area, hasFocus ? (Rect?)hole : null);
        }

        private Rect TargetRect(RectTransform target)
        {
            target.GetWorldCorners(corners);
            Vector2 min = overlayRoot.InverseTransformPoint(corners[0]);
            Vector2 max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 point = overlayRoot.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Intersection(Rect a, Rect b) => Rect.MinMaxRect(
            Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin),
            Mathf.Max(Mathf.Max(a.xMin, b.xMin), Mathf.Min(a.xMax, b.xMax)),
            Mathf.Max(Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.yMax, b.yMax)));

        private void LayoutGuide(Rect area, Rect? hole)
        {
            float width = Mathf.Min(360, area.width - 24);
            float titleHeight = titleText.GetPreferredValues(titleText.text, width - 40, Mathf.Infinity).y;
            float bodyHeight = bodyText.GetPreferredValues(bodyText.text, width - 40, Mathf.Infinity).y;
            float height = Mathf.Min(area.height - 24, 110 + titleHeight + bodyHeight);
            guidePanel.sizeDelta = new Vector2(width, height);
            SetTextRect(titleText.rectTransform, 20, 38, width - 40, titleHeight);
            SetTextRect(bodyText.rectTransform, 20, 48 + titleHeight, width - 40, height - titleHeight - 110);
            Vector2 center = area.center;
            if (hole.HasValue)
            {
                var h = hole.Value;
                var candidates = new[]
                {
                    new Vector2(h.xMin - width / 2 - 16, h.center.y),
                    new Vector2(h.xMax + width / 2 + 16, h.center.y),
                    new Vector2(h.center.x, h.yMax + height / 2 + 16),
                    new Vector2(h.center.x, h.yMin - height / 2 - 16)
                };
                float bestScore = float.MaxValue;
                foreach (var candidate in candidates)
                {
                    var clamped = ClampCenter(candidate, area, width, height);
                    var overlap = Intersection(new Rect(clamped - new Vector2(width, height) / 2,
                        new Vector2(width, height)), h);
                    float score = overlap.width * overlap.height * 1000 + (clamped - h.center).sqrMagnitude;
                    if (score >= bestScore) continue;
                    bestScore = score;
                    center = clamped;
                }
            }
            else if (!confirmingSkip && !currentStep.BlockInput)
                center = new Vector2(area.center.x, area.yMin + height / 2 + 16);
            guidePanel.anchoredPosition = ClampCenter(center, area, width, height);
        }

        private static Vector2 ClampCenter(Vector2 center, Rect area, float width, float height) => new Vector2(
            Mathf.Clamp(center.x, area.xMin + width / 2 + 12, area.xMax - width / 2 - 12),
            Mathf.Clamp(center.y, area.yMin + height / 2 + 12, area.yMax - height / 2 - 12));

        private static void SetTextRect(RectTransform target, float left, float top, float width, float height)
        {
            target.anchorMin = target.anchorMax = new Vector2(0, 1);
            target.pivot = new Vector2(0, 1);
            target.anchoredPosition = new Vector2(left, -top);
            target.sizeDelta = new Vector2(width, height);
        }

        private static void Place(RectTransform target, Rect rect)
        {
            target.anchorMin = target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = rect.center;
            target.sizeDelta = rect.size;
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (currentStep == null || confirmingSkip || !currentStep.BlockInput) return true;
            if (RectTransformUtility.RectangleContainsScreenPoint(guidePanel, screenPoint, eventCamera)) return true;
            if (!currentStep.AllowTargetClick || clickTarget == null || !clickTarget.gameObject.activeInHierarchy)
                return true;
            bool inside = RectTransformUtility.RectangleContainsScreenPoint(clickTarget, screenPoint, eventCamera);
            if (inside && currentStep.FocusEvidence && scroll != null && scroll.viewport != null)
                inside = RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, screenPoint, eventCamera);
            // false 让透明输入层放行；四块视觉遮罩与聚焦框都不参与 Raycast。
            return !inside;
        }

        private bool CanScroll(PointerEventData data) => !confirmingSkip && currentStep != null && currentStep.BlockInput
            && scroll != null && scroll.isActiveAndEnabled && scroll.viewport != null
            && RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, data.position, data.pressEventCamera);

        public void OnInitializePotentialDrag(PointerEventData data)
        {
            if (CanScroll(data)) scroll.OnInitializePotentialDrag(data);
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (!CanScroll(data)) return;
            forwardedDrag = data;
            scroll.OnBeginDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (forwardedDrag != null && scroll != null) scroll.OnDrag(data);
        }

        public void OnEndDrag(PointerEventData data) => StopForwardedDrag();

        public void OnScroll(PointerEventData data)
        {
            if (CanScroll(data)) scroll.OnScroll(data);
        }

        private void StopForwardedDrag()
        {
            if (forwardedDrag != null && scroll != null) scroll.OnEndDrag(forwardedDrag);
            forwardedDrag = null;
        }
    }
}
