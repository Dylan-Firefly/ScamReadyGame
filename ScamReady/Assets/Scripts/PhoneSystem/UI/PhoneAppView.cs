using System.Collections;
using ScamReady.Scenarios;
using ScamReady.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private GameObject window;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text scenarioTitleText;
        [SerializeField] private GameObject action;
        [SerializeField] private GameObject dialogContent;
        [SerializeField] private Transform dialogContainer;
        [SerializeField] private PhoneDialogBoxView dialogBoxPrefab;
        [SerializeField, Min(0f)] private float dialogLineDelaySeconds = 1.25f;
        [SerializeField] private UIWindowAnimator windowAnimator;

        private Button proceedButton;
        private Button stopContactButton;
        private Button ignoreButton;
        private Button rejectButton;
        private ScrollRect dialogScrollRect;
        private Coroutine dialogRoutine;
        private int nextDialogLineIndex;
        private PhoneDetailView detailView;
        private PhoneCallState? displayedCallState;
        private Transform searchRoot;

        private void Awake()
        {
            if (scenarioController == null)
                scenarioController = FindFirstObjectByType<ScenarioController>();

            if (window == null)
                window = gameObject;
            searchRoot = window != null ? window.transform : transform;
            if (closeButton == null)
                closeButton = FindButton("CloseButton");
            if (scenarioTitleText == null)
                scenarioTitleText = FindText("scenarioTitleText");
            if (action == null)
                action = FindObject("Action");
            if (dialogContent == null)
                dialogContent = FindObject("Dialog Content");
            if (dialogScrollRect == null && dialogContent != null)
                dialogScrollRect = dialogContent.GetComponentInChildren<ScrollRect>(true);
            if (dialogContainer == null && dialogContent != null)
            {
                var grid = FindObject("Grid");
                dialogContainer = grid != null ? grid.transform
                    : dialogScrollRect != null && dialogScrollRect.content != null
                        ? dialogScrollRect.content
                        : dialogContent.transform;
            }
            if (proceedButton == null) proceedButton = FindButton("Proceed");
            if (stopContactButton == null) stopContactButton = FindButton("StopContact");
            if (ignoreButton == null) ignoreButton = FindButton("Ignore");
            if (rejectButton == null) rejectButton = FindButton("Reject");
            if (detailView == null && window != null)
                detailView = window.GetComponentInChildren<PhoneDetailView>(true);
            if (windowAnimator == null && window != null)
                windowAnimator = window.GetComponent<UIWindowAnimator>();

            if (scenarioController == null || window == null || closeButton == null
                || scenarioTitleText == null || action == null || dialogContent == null
                || dialogContainer == null || dialogScrollRect == null
                || dialogBoxPrefab == null || detailView == null
                || proceedButton == null || stopContactButton == null
                || ignoreButton == null || rejectButton == null)
            {
                Debug.LogError("PhoneAppView 缺少必需的手机控制器、结局依赖或 UI 引用。", this);
                enabled = false;
                return;
            }

            ConfigureDialogScrollView();
            closeButton.onClick.AddListener(scenarioController.ClosePhone);
            proceedButton.onClick.AddListener(Proceed);
            stopContactButton.onClick.AddListener(scenarioController.StopPhoneContact);
            ignoreButton.onClick.AddListener(Ignore);
            rejectButton.onClick.AddListener(Reject);
            detailView.Connect(scenarioController);
        }

        private void OnEnable()
        {
            if (scenarioController == null) return;
            scenarioController.Changed += Refresh;
            Refresh();
            StartDialogSequenceIfNeeded();
        }

        private void OnDisable()
        {
            if (scenarioController != null)
                scenarioController.Changed -= Refresh;
            StopDialogSequence();
        }

        private void OnDestroy()
        {
            if (scenarioController == null) return;
            if (closeButton != null) closeButton.onClick.RemoveListener(scenarioController.ClosePhone);
            if (proceedButton != null) proceedButton.onClick.RemoveListener(Proceed);
            if (stopContactButton != null) stopContactButton.onClick.RemoveListener(scenarioController.StopPhoneContact);
            if (ignoreButton != null) ignoreButton.onClick.RemoveListener(Ignore);
            if (rejectButton != null) rejectButton.onClick.RemoveListener(Reject);
        }

        private void Refresh()
        {
            scenarioTitleText.text = scenarioController.PhoneLevel != null ? scenarioController.PhoneLevel.Title : string.Empty;
            bool inCall = scenarioController.CallState == PhoneCallState.InCall;
            action.SetActive(inCall);
            dialogContent.SetActive(inCall);

            if (displayedCallState != scenarioController.CallState)
            {
                displayedCallState = scenarioController.CallState;
                RebuildDialog();
            }

            SetVisible(scenarioController.Session != null && scenarioController.Session.IsPhoneOpen);
        }

        private void RebuildDialog()
        {
            StopDialogSequence();
            for (int i = dialogContainer.childCount - 1; i >= 0; i--)
                Destroy(dialogContainer.GetChild(i).gameObject);
            nextDialogLineIndex = 0;

            if (scenarioController.CallState != PhoneCallState.InCall || scenarioController.PhoneLevel == null)
                return;

            StartDialogSequenceIfNeeded();
        }

        private IEnumerator DisplayDialogLines()
        {
            var lines = scenarioController.PhoneLevel.DialogLines;
            while (nextDialogLineIndex < lines.Count
                && scenarioController.CallState == PhoneCallState.InCall)
            {
                var line = lines[nextDialogLineIndex++];
                if (line == null) continue;

                bool wasAtBottom = dialogScrollRect.viewport == null
                    || dialogContainer.GetComponent<RectTransform>().rect.height
                        <= dialogScrollRect.viewport.rect.height + 1f
                    || dialogScrollRect.verticalNormalizedPosition <= 0.05f;
                var dialog = Instantiate(dialogBoxPrefab, dialogContainer);
                dialog.Bind(line, scenarioController.HasCollectedPhoneEvidence(line.Id),
                    () => scenarioController.CollectPhoneEvidence(line.Id));

                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialogContainer);
                Canvas.ForceUpdateCanvases();
                if (wasAtBottom)
                    dialogScrollRect.verticalNormalizedPosition = 0f;

                if (nextDialogLineIndex < lines.Count && dialogLineDelaySeconds > 0f)
                    yield return new WaitForSecondsRealtime(dialogLineDelaySeconds);
            }

            dialogRoutine = null;
        }

        private void StartDialogSequenceIfNeeded()
        {
            if (!enabled || dialogRoutine != null || scenarioController == null
                || scenarioController.CallState != PhoneCallState.InCall
                || scenarioController.PhoneLevel == null
                || nextDialogLineIndex >= scenarioController.PhoneLevel.DialogLines.Count)
                return;

            dialogRoutine = StartCoroutine(DisplayDialogLines());
        }

        private void StopDialogSequence()
        {
            if (dialogRoutine == null) return;
            StopCoroutine(dialogRoutine);
            dialogRoutine = null;
        }

        private void ConfigureDialogScrollView()
        {
            var contentRect = dialogContainer as RectTransform;
            if (contentRect == null)
            {
                Debug.LogError("PhoneAppView 的对话 Grid 必须是 RectTransform。", this);
                enabled = false;
                return;
            }
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;

            var gridLayout = dialogContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
                gridLayout.enabled = false;

            var verticalLayout = dialogContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
                verticalLayout = dialogContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            var contentSizeFitter = dialogContainer.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
                contentSizeFitter = dialogContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            dialogScrollRect.content = contentRect;
            dialogScrollRect.horizontal = false;
            dialogScrollRect.vertical = true;
            dialogScrollRect.horizontalScrollbar = null;
            dialogScrollRect.verticalScrollbar = null;
            foreach (var scrollbar in dialogContent.GetComponentsInChildren<Scrollbar>(true))
                scrollbar.gameObject.SetActive(false);
            dialogScrollRect.enabled = true;
        }

        private void Proceed() => scenarioController.ChoosePhoneResponse(PhoneResponse.Trust);
        private void Ignore() => scenarioController.ChoosePhoneResponse(PhoneResponse.Ignore);
        private void Reject() => scenarioController.ChoosePhoneResponse(PhoneResponse.Report);

        private GameObject FindObject(string objectName)
        {
            foreach (var transform in searchRoot.GetComponentsInChildren<Transform>(true))
                if (transform.name == objectName)
                    return transform.gameObject;
            return null;
        }

        private Button FindButton(string objectName)
        {
            var target = FindObject(objectName);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private TMP_Text FindText(string objectName)
        {
            foreach (var text in searchRoot.GetComponentsInChildren<TMP_Text>(true))
                if (text.gameObject.name == objectName)
                    return text;
            return null;
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        public void Show()
        {
            if (window.activeSelf) return;
            if (windowAnimator != null) windowAnimator.PlayOpen();
            else window.SetActive(true);
        }

        public void Hide()
        {
            if (!window.activeSelf) return;
            if (windowAnimator != null) windowAnimator.PlayClose();
            else window.SetActive(false);
        }
    }
}
