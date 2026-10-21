using System;
using System.Collections.Generic;
using ScamReady.Phone;
using UnityEngine;

namespace ScamReady.Scenarios
{
    [Serializable]
    public sealed class ScenarioPlaylistEntry
    {
        [SerializeField] private ScenarioType type;
        [SerializeField] private EmailScenarioDefinition email;
        [SerializeField] private PhoneScenarioCollection phoneCollection;
        [SerializeField, Tooltip("Stable phone scenario ID, independent of collection order.")]
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

    [CreateAssetMenu(menuName = "Scam Ready/Scenario Playlist", fileName = "ScenarioPlaylist")]
    public sealed class ScenarioPlaylist : ScriptableObject
    {
        [SerializeField] private ScenarioPlaylistEntry[] entries = Array.Empty<ScenarioPlaylistEntry>();
        [SerializeField, Tooltip("Shuffle scenarios each round; otherwise use list order.")]
        private bool shuffle;
        [SerializeField, Tooltip("Start another round after all scenarios finish.")]
        private bool loop = true;

        public IReadOnlyList<ScenarioPlaylistEntry> Entries => entries;
        public bool Shuffle => shuffle;
        public bool Loop => loop;
    }
}
