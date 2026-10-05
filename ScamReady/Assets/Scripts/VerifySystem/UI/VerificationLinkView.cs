using System;
using TMPro;
using UnityEngine;

namespace ScamReady.Verification
{
    /// <summary>一个官网入口。Inspector 绑定自身按钮、入口文字和地址文字。</summary>
    public sealed class VerificationLinkView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text addressText;
        private VerificationPageDefinition page;
        private Action<VerificationPageDefinition> selected;

        private void Awake() => button.onClick.AddListener(OpenPage);

        public void Bind(VerificationPageDefinition definition, Action<VerificationPageDefinition> onSelected)
        {
            page = definition;
            selected = onSelected;
            gameObject.SetActive(page != null);
            if (page == null) return;
            label.text = page.EntryLabel;
            addressText.text = page.Address;
        }

        private void OpenPage() => selected?.Invoke(page);
    }
}
