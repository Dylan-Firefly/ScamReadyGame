using ScamReady.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneAppView : MonoBehaviour
    {
        [SerializeField] private PhoneScenarioController phoneController;
        [SerializeField] private GameObject window;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text scenarioTitleText;
        [SerializeField] private GameObject action;
        [SerializeField] private GameObject dialogContent;
        [SerializeField] private Transform dialogContainer;
        [SerializeField] private PhoneDialogBoxView dialogBoxPrefab;
        [SerializeField] private UIWindowAnimator windowAnimator;

        private Button proceedButton;
        private Button stopContactButton;
        private Button ignoreButton;
        private Button rejectButton;
        private PhoneDetailView detailView;
        private PhoneCallState? displayedCallState;
        private Transform searchRoot;

        private void Awake()
        {
            if (phoneController == null)
                phoneController = FindFirstObjectByType<PhoneScenarioController>();

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
            if (dialogContainer == null && dialogContent != null)
            {
                var grid = dialogContent.GetComponentInChildren<GridLayoutGroup>(true);
                dialogContainer = grid != null ? grid.transform : dialogContent.transform;
            }
            if (proceedButton == null) proceedButton = FindButton("Proceed");
            if (stopContactButton == null) stopContactButton = FindButton("StopContact");
            if (ignoreButton == null) ignoreButton = FindButton("Ignore");
            if (rejectButton == null) rejectButton = FindButton("Reject");
            if (detailView == null && window != null)
                detailView = window.GetComponentInChildren<PhoneDetailView>(true);
            if (windowAnimator == null && window != null)
                windowAnimator = window.GetComponent<UIWindowAnimator>();

            if (phoneController == null || window == null || closeButton == null
                || scenarioTitleText == null || action == null || dialogContent == null
                || dialogContainer == null || dialogBoxPrefab == null || detailView == null
                || proceedButton == null || stopContactButton == null
                || ignoreButton == null || rejectButton == null)
            {
                Debug.LogError("PhoneAppView 缺少必需的手机控制器、结局依赖或 UI 引用。", this);
                enabled = false;
                return;
            }

            closeButton.onClick.AddListener(phoneController.ClosePhone);
            proceedButton.onClick.AddListener(Proceed);
            stopContactButton.onClick.AddListener(phoneController.StopContact);
            ignoreButton.onClick.AddListener(Ignore);
            rejectButton.onClick.AddListener(Reject);
            detailView.Connect(phoneController);
        }

        private void OnEnable()
        {
            if (phoneController == null) return;
            phoneController.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (phoneController != null)
                phoneController.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            if (phoneController == null) return;
            if (closeButton != null) closeButton.onClick.RemoveListener(phoneController.ClosePhone);
            if (proceedButton != null) proceedButton.onClick.RemoveListener(Proceed);
            if (stopContactButton != null) stopContactButton.onClick.RemoveListener(phoneController.StopContact);
            if (ignoreButton != null) ignoreButton.onClick.RemoveListener(Ignore);
            if (rejectButton != null) rejectButton.onClick.RemoveListener(Reject);
        }

        private void Refresh()
        {
            scenarioTitleText.text = phoneController.Level != null ? phoneController.Level.Title : string.Empty;
            bool inCall = phoneController.CallState == PhoneCallState.InCall;
            action.SetActive(inCall);
            dialogContent.SetActive(inCall);

            if (displayedCallState != phoneController.CallState)
            {
                displayedCallState = phoneController.CallState;
                RebuildDialog();
            }

            SetVisible(phoneController.IsPhoneOpen);
        }

        private void RebuildDialog()
        {
            for (int i = dialogContainer.childCount - 1; i >= 0; i--)
                Destroy(dialogContainer.GetChild(i).gameObject);

            if (phoneController.CallState != PhoneCallState.InCall || phoneController.Level == null)
                return;

            foreach (var line in phoneController.Level.DialogLines)
            {
                if (line == null) continue;
                var dialog = Instantiate(dialogBoxPrefab, dialogContainer);
                dialog.Bind(line, phoneController.HasCollectedEvidence(line.Id),
                    () => phoneController.CollectEvidence(line.Id));
            }
        }

        private void Proceed() => phoneController.ChooseResponse(PhoneResponse.Trust);
        private void Ignore() => phoneController.ChooseResponse(PhoneResponse.Ignore);
        private void Reject() => phoneController.ChooseResponse(PhoneResponse.Report);

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
