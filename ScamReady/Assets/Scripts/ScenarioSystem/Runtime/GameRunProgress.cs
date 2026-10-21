using UnityEngine;

namespace ScamReady.Scenarios
{
    public static class GameRunProgress
    {
        public static bool HasCompletedTutorial { get; private set; }

        public static void MarkTutorialCompleted() => HasCompletedTutorial = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewRun()
        {
            // Reset each Play session, including when domain reload is disabled.
            HasCompletedTutorial = false;
        }
    }
}
