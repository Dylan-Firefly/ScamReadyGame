using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScamReady.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class SceneNavigationButton : MonoBehaviour
    {
        [SerializeField, Tooltip("Scene name from the build list, such as Scenario.")]
        private string sceneName;
        private UnityEngine.UI.Button button;

        private void Awake()
        {
            button = GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(OpenScene);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OpenScene);
        }

        public void OpenScene()
        {
            if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("跳转目标未配置或未启用在构建列表中：" + sceneName, this);
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
