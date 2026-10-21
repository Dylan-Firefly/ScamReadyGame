using System;
using TMPro;
using UnityEngine;

namespace ScamReady.Verification
{
    public sealed class VerificationLinkView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text addressText;
        private VerificationPageDefinition page;
        private Action<VerificationPageDefinition> selected;

        private void Awake() => button.onClick.AddListener(OpenPage);

        public void Bind(VerificationPageDefinition definition, Action<VerificationPageDefinition> onSelected,
            Action<string> onOpenLink)
        {
            page = definition;
            selected = onSelected;
            gameObject.SetActive(page != null);
            if (page == null) return;
            label.text = page.EntryLabel;
            BrowserLinkText.SetText(addressText, page.Address, onOpenLink);
        }

        private void OpenPage() => selected?.Invoke(page);
    }
}
