using System;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScamReady.Verification
{
    /// <summary>把普通文案中的网址显示为可点击链接，仅交给游戏内导航，不访问真实网站。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class BrowserLinkText : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        private static readonly Regex UrlPattern = new Regex(@"https?://[^\s<>""']+",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private TMP_Text text;
        private string displayedContent;
        private Action<string> openLink;

        /// <summary>首次绑定时添加文字点击组件，已有 TMP 字段和场景引用保持不变。</summary>
        public static void SetText(TMP_Text target, string content, Action<string> onOpenLink)
        {
            var view = target.GetComponent<BrowserLinkText>();
            if (view == null) view = target.gameObject.AddComponent<BrowserLinkText>();
            view.Bind(target, content ?? string.Empty, onOpenLink);
        }

        private void Bind(TMP_Text target, string content, Action<string> onOpenLink)
        {
            text = target;
            openLink = onOpenLink;
            text.richText = true;
            if (displayedContent == content) return;
            displayedContent = content;
            var output = new StringBuilder();
            int position = 0;
            bool hasLinks = false;
            foreach (Match match in UrlPattern.Matches(content))
            {
                string address = match.Value.TrimEnd('.', ',', ';', '!', ')', ']', '}');
                output.Append(Escape(content.Substring(position, match.Index - position)));
                output.Append("<link=\"").Append(address).Append("\"><color=#2469B3><u>")
                    .Append(Escape(address)).Append("</u></color></link>");
                position = match.Index + address.Length;
                hasLinks = true;
            }
            output.Append(Escape(content.Substring(position)));
            text.raycastTarget = hasLinks;
            text.text = output.ToString();
        }

        private static string Escape(string value) => value.Replace("<", "<noparse><</noparse>");

        public void OnPointerDown(PointerEventData eventData)
        {
            // 自己接收按下，避免地址文字位于 Button 内时，按下交给父按钮、点击却交给文字而失效。
            if (text != null) text.ForceMeshUpdate();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || text == null) return;
            int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
            if (index >= 0) openLink?.Invoke(text.textInfo.linkInfo[index].GetLinkID());
        }
    }
}
