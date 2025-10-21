using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private float defaultDuration = 2f;

        private CanvasGroup _canvasGroup;
        private float _timer;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            HideImmediate();
        }

        private void Update()
        {
            if (_timer <= 0f)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                HideImmediate();
            }
        }

        public void Show(string message, float duration = -1f)
        {
            if (label == null)
            {
                return;
            }

            label.text = message;
            _timer = duration > 0f ? duration : defaultDuration;
            _canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
            _timer = 0f;
        }

        public void Initialize(Text text)
        {
            label = text;
            HideImmediate();
        }
    }
}
