using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScamReady.Phone
{
    public sealed class PhoneDialogBoxView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text messageText;

        private PhoneDialogLineData line;
        private Action collectEvidence;
        private bool isCollected;

        public void Bind(PhoneDialogLineData dialogLine, bool collected, Action collect)
        {
            if (messageText == null)
            {
                foreach (var text in GetComponentsInChildren<TMP_Text>(true))
                {
                    messageText = text;
                    break;
                }
            }

            if (messageText == null)
            {
                Debug.LogError("PhoneDialogBoxView 缺少 TMP_Text 子对象。", this);
                return;
            }

            line = dialogLine;
            isCollected = collected;
            collectEvidence = collect;
            ConfigureLayout();
            Refresh();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (line == null || !line.IsEvidence || isCollected) return;
            collectEvidence?.Invoke();
            isCollected = true;
            Refresh();
        }

        private void Refresh()
        {
            if (messageText == null || line == null) return;
            messageText.text = line.Text
                + (line.IsEvidence ? isCollected ? "\n✓ Evidence collected" : "\nClick to collect evidence" : string.Empty);
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }

        private void ConfigureLayout()
        {
            var layout = GetComponent<VerticalLayoutGroup>();
            if (layout == null)
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 12, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var contentSizeFitter = GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
                contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
