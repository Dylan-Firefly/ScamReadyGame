using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        }
    }
}
