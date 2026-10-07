using ScamReady.Scenarios;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScamReady.UI
{
    /// <summary>主菜单的开始、继续与退出；Continue 只控制跳转，不恢复关卡进度。</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button newGameButton;
        [SerializeField] private UnityEngine.UI.Button continueButton;
        [SerializeField] private UnityEngine.UI.Button exitButton;
        [SerializeField] private string tutorialSceneName = "TutorialScenario";
        [SerializeField] private string scenarioSceneName = "Scenario";

        private void Awake()
        {
            newGameButton.onClick.AddListener(NewGame);
            continueButton.onClick.AddListener(Continue);
            exitButton.onClick.AddListener(Exit);
        }

        private void OnEnable() => RefreshContinue();

        private void OnDestroy()
        {
            if (newGameButton != null) newGameButton.onClick.RemoveListener(NewGame);
            if (continueButton != null) continueButton.onClick.RemoveListener(Continue);
            if (exitButton != null) exitButton.onClick.RemoveListener(Exit);
        }

        private void RefreshContinue()
        {
            continueButton.gameObject.SetActive(GameRunProgress.HasCompletedTutorial);
        }

        public void NewGame() => OpenScene(tutorialSceneName);

        public void Continue()
        {
            if (GameRunProgress.HasCompletedTutorial) OpenScene(scenarioSceneName);
        }

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("主菜单目标未配置或未启用在构建列表中：" + sceneName, this);
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
