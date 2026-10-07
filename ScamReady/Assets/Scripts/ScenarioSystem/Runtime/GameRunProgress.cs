using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>本次运行的解锁标记；跨场景保留，退出后不保存。</summary>
    public static class GameRunProgress
    {
        public static bool HasCompletedTutorial { get; private set; }

        public static void MarkTutorialCompleted() => HasCompletedTutorial = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewRun()
        {
            // 即使 Editor 关闭 Domain Reload，每次开始 Play 也恢复首次运行状态。
            HasCompletedTutorial = false;
        }
    }
}
