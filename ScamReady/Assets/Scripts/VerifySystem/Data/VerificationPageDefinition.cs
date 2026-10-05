using UnityEngine;

namespace ScamReady.Verification
{
    /// <summary>浏览器中的可编辑官网页面，可被不同关卡引用。</summary>
    [CreateAssetMenu(menuName = "Scam Ready/Verification Page", fileName = "VerificationPage")]
    public sealed class VerificationPageDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("稳定且唯一的页面 ID，用于查验记录。修改文案时不要改变 ID。")]
        private string id;
        [SerializeField] private string entryLabel;
        [SerializeField] private string siteName;
        [SerializeField, Tooltip("游戏中展示的模拟地址，不会打开真实网站。")]
        private string address;
        [SerializeField] private string title;
        [SerializeField, TextArea(8, 24)] private string body;

        public string Id => id;
        public string EntryLabel => entryLabel;
        public string SiteName => siteName;
        public string Address => address;
        public string Title => title;
        public string Body => body;
    }
}
