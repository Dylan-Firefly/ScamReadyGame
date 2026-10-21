using ScamReady.Scenarios;
using ScamReady.UI;
using TMPro;
using UnityEngine;

namespace ScamReady.Evidence
{
    public sealed class EvidenceSpotView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text statusText;

        [Header("Audio")]
        [SerializeField] private UIAudioPlayer audioPlayer;

        private ScenarioController controller;
        private EvidencePlacement placement;

        private void Awake()
        {
            button.onClick.AddListener(Collect);

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }
        }

        public void Bind(ScenarioController scenario, EvidencePlacement evidencePlacement)
        {
            controller = scenario;
            placement = evidencePlacement;
            contentText.richText = false;
            contentText.text = placement.Content;
            Refresh();
        }

        public void Refresh()
        {
            bool saved = controller.Session != null && controller.Session.HasCollectedEvidence(placement.Evidence.Id);
            button.interactable = !saved;
            statusText.text = saved ? "Saved to notes" : "Click to save this record";
        }

        private void Collect()
        {
            bool wasSaved = controller.Session != null &&
                            controller.Session.HasCollectedEvidence(placement.Evidence.Id);

            controller.CollectEvidence(placement);

            bool isSaved = controller.Session != null &&
                           controller.Session.HasCollectedEvidence(placement.Evidence.Id);

            if (!wasSaved && isSaved)
            {
                audioPlayer?.PlayEvidenceCollected();
            }
        }
    }
}
