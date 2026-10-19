
using System.Collections.Generic;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.Evidence
{
    /// <summary>
    /// Reject 时显示已收集证据，允许玩家选择并提交。
    /// 不单独保存玩家进度，最终提交由 ScenarioController 处理。
    /// </summary>
    public sealed class EvidenceSubmissionView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private Transform evidenceContainer;
        [SerializeField] private Toggle evidenceTogglePrefab;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text selectedCountText;
        [SerializeField] private GameObject emptyState;

        private readonly List<Toggle> toggles = new List<Toggle>();
        private readonly List<string> evidenceIds = new List<string>();

        private ScenarioSession displayedSession;
        private bool wasOpen;

        private void Awake()
        {
            submitButton.onClick.AddListener(Submit);
            cancelButton.onClick.AddListener(
                controller.CancelEvidenceSubmission);
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            controller.Changed -= Refresh;
        }

        private void Refresh()
        {
            var session = controller.Session;
            bool isOpen = session != null &&
                          session.IsEvidenceSubmissionOpen;

            if (isOpen && (!wasOpen ||
                           displayedSession != session))
            {
                BuildEvidenceList(session);
            }

            displayedSession = session;
            wasOpen = isOpen;

            if (window != null)
                window.SetActive(isOpen);

            if (isOpen)
                UpdateSelectedCount();
        }

        private void BuildEvidenceList(ScenarioSession session)
        {
            ClearEvidenceList();

            foreach (string id in session.CollectedEvidenceIds)
            {
                var placement = controller.FindEvidencePlacement(id);

                if (placement == null ||
                    placement.Evidence == null)
                    continue;

                Toggle toggle = Instantiate(
                    evidenceTogglePrefab, evidenceContainer);

                toggle.gameObject.SetActive(true);
                toggle.isOn = false;

                TMP_Text label =
                    toggle.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = placement.Evidence.Title +
                                 "\n" +
                                 placement.Evidence.Summary;
                }

                toggle.onValueChanged.AddListener(
                    _ => UpdateSelectedCount());

                toggles.Add(toggle);
                evidenceIds.Add(id);
            }

            if (emptyState != null)
                emptyState.SetActive(toggles.Count == 0);
        }

        private void ClearEvidenceList()
        {
            foreach (Toggle toggle in toggles)
            {
                if (toggle != null)
                    Destroy(toggle.gameObject);
            }

            toggles.Clear();
            evidenceIds.Clear();
        }

        private void UpdateSelectedCount()
        {
            int selected = 0;

            foreach (Toggle toggle in toggles)
            {
                if (toggle != null && toggle.isOn)
                    selected++;
            }

            if (selectedCountText != null)
            {
                selectedCountText.text =
                    "Selected Evidence: " + selected;
            }

            // 允许提交零个证据，系统会评估为未验证。
            if (submitButton != null)
                submitButton.interactable = true;
        }

        private void Submit()
        {
            var selectedIds = new List<string>();

            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] != null && toggles[i].isOn)
                    selectedIds.Add(evidenceIds[i]);
            }

            controller.SubmitEvidence(selectedIds);
        }
    }
}
