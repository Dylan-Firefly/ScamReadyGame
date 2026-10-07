using System.Collections;
using UnityEngine;

namespace ScamReady.UI
{
    public sealed class UIWindowAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float startScale = 0.92f;
        [SerializeField] private float openDuration = 0.18f;
        [SerializeField] private float closeDuration = 0.12f;

        private Coroutine animationRoutine;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void PlayOpen()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
            }

            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            animationRoutine = StartCoroutine(OpenRoutine());
        }

        public void PlayClose()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
            }

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            animationRoutine = StartCoroutine(CloseRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            float elapsed = 0f;

            target.localScale = Vector3.one * startScale;

            while (elapsed < openDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / openDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);

                target.localScale = Vector3.Lerp(
                    Vector3.one * startScale,
                    Vector3.one,
                    t
                );

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                }

                yield return null;
            }

            target.localScale = Vector3.one;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            animationRoutine = null;
        }

        private IEnumerator CloseRoutine()
        {
            float elapsed = 0f;

            while (elapsed < closeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / closeDuration);

                target.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.one * startScale,
                    t
                );

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                yield return null;
            }

            target.localScale = Vector3.one;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            gameObject.SetActive(false);
            animationRoutine = null;
        }
    }
}