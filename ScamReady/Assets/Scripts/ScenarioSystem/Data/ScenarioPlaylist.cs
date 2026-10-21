using System;
using System.Collections.Generic;
using ScamReady.Phone;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>引用现有情景数据，不复制内容，也不保存玩家进度。</summary>
    [Serializable]
    public sealed class ScenarioPlaylistEntry
    {
        [SerializeField] private ScenarioType type;
        [SerializeField] private EmailScenarioDefinition email;
        [SerializeField] private PhoneScenarioCollection phoneCollection;
        [SerializeField, Tooltip("电话情景的稳定 ID；调整集合顺序不会改变此引用。")]
        private string phoneLevelId;

        public ScenarioType Type => type;
        public EmailScenarioDefinition Email => email;
        public PhoneScenarioCollection PhoneCollection => phoneCollection;
        public string Id => type == ScenarioType.Email ? email?.Id : phoneLevelId;
        public int PhoneLevelIndex => phoneCollection == null ? -1 : phoneCollection.FindLevelIndex(phoneLevelId);
        public bool IsValid => type == ScenarioType.Email
            ? email != null && email.Email != null && !string.IsNullOrWhiteSpace(email.Id)
                && !string.IsNullOrWhiteSpace(email.Email.Id)
            : type == ScenarioType.Phone && PhoneLevelIndex >= 0;
    }

    /// <summary>当前场景内可发布的联系；顺序或随机均按一轮不重复的方式取用。</summary>
    [CreateAssetMenu(menuName = "Scam Ready/Scenario Playlist", fileName = "ScenarioPlaylist")]
    public sealed class ScenarioPlaylist : ScriptableObject
    {
        [SerializeField] private ScenarioPlaylistEntry[] entries = Array.Empty<ScenarioPlaylistEntry>();
        [SerializeField, Tooltip("每轮打乱有效情景；关闭时按列表顺序发布。")]
        private bool shuffle;
        [SerializeField, Tooltip("全部情景完成后，继续发布下一轮联系。")]
        private bool loop = true;

        public IReadOnlyList<ScenarioPlaylistEntry> Entries => entries;
        public bool Shuffle => shuffle;
        public bool Loop => loop;
    }
}
