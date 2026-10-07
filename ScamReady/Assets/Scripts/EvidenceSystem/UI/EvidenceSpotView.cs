using ScamReady.Scenarios;
using TMPro;
using UnityEngine;

namespace ScamReady.Evidence
{
    /// <summary>可点击的关键内容卡片。Prefab 内绑定按钮、内容与保存状态文字。</summary>
    public sealed class EvidenceSpotView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text statusText;
        private ScenarioController controller;
        private EvidencePlacement placement;

        private void Awake() => button.onClick.AddListener(Collect);

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

        private void Collect() => controller.CollectEvidence(placement);
    }
}
