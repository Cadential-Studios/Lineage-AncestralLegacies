using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0414

namespace Lineage.UI
{
    /// <summary>
    /// Manages toast notifications for player feedback.
    /// Provides non-intrusive messaging for game events.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Notification Settings")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float defaultDisplayDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private int maxVisibleNotifications = 5;
        [SerializeField] private float verticalSpacing = 10f;

        private Queue<NotificationData> pendingNotifications = new Queue<NotificationData>();
        private List<Notification> activeNotifications = new List<Notification>();
        private bool isProcessing = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Only DontDestroyOnLoad if this is a root object
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (notificationContainer == null)
            {
                UnityEngine.Debug.LogError("[NotificationManager] Notification container not assigned!");
            }
        }

        /// <summary>
        /// Shows a notification with specified message and type
        /// </summary>
        public void Show(string message, NotificationType type = NotificationType.Info, float duration = -1f)
        {
            if (string.IsNullOrEmpty(message)) return;

            float displayDuration = duration > 0 ? duration : defaultDisplayDuration;

            NotificationData data = new NotificationData();
            data.message = message;
            data.type = type;
            data.duration = displayDuration;

            pendingNotifications.Enqueue(data);

            if (!isProcessing)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }

        /// <summary>
        /// Shows an info notification
        /// </summary>
        public void ShowInfo(string message, float duration = -1f)
        {
            Show(message, NotificationType.Info, duration);
        }

        /// <summary>
        /// Shows a success notification
        /// </summary>
        public void ShowSuccess(string message, float duration = -1f)
        {
            Show(message, NotificationType.Success, duration);
        }

        /// <summary>
        /// Shows a warning notification
        /// </summary>
        public void ShowWarning(string message, float duration = -1f)
        {
            Show(message, NotificationType.Warning, duration);
        }

        /// <summary>
        /// Shows an error notification
        /// </summary>
        public void ShowError(string message, float duration = -1f)
        {
            Show(message, NotificationType.Error, duration);
        }

        /// <summary>
        /// Clears all active notifications
        /// </summary>
        public void ClearAll()
        {
            foreach (var notification in activeNotifications)
            {
                if (notification != null)
                {
                    Destroy(notification.gameObject);
                }
            }
            activeNotifications.Clear();
            pendingNotifications.Clear();
        }

        private IEnumerator ProcessNotificationQueue()
        {
            isProcessing = true;

            while (pendingNotifications.Count > 0)
            {
                // Wait if too many notifications are visible
                while (activeNotifications.Count >= maxVisibleNotifications)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                NotificationData data = pendingNotifications.Dequeue();
                CreateNotification(data);

                yield return new WaitForSeconds(0.1f); // Small delay between notifications
            }

            isProcessing = false;
        }

        private void CreateNotification(NotificationData data)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                UnityEngine.Debug.LogWarning($"[NotificationManager] Cannot create notification: {data.message}");
                return;
            }

            GameObject notificationObj = Instantiate(notificationPrefab, notificationContainer);
            Notification notification = notificationObj.GetComponent<Notification>();

            if (notification != null)
            {
                notification.Initialize(data, this);
                activeNotifications.Add(notification);

                StartCoroutine(RemoveNotificationAfterDelay(notification, data.duration));

                // Reposition all notifications
                RepositionNotifications();
            }
            else
            {
                UnityEngine.Debug.LogError("[NotificationManager] Notification prefab missing Notification component!");
                Destroy(notificationObj);
            }
        }

        private IEnumerator RemoveNotificationAfterDelay(Notification notification, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (notification != null)
            {
                StartCoroutine(FadeOutAndDestroy(notification));
            }
        }

        private IEnumerator FadeOutAndDestroy(Notification notification)
        {
            CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                    yield return null;
                }
            }

            activeNotifications.Remove(notification);
            Destroy(notification.gameObject);

            RepositionNotifications();
        }

        private void RepositionNotifications()
        {
            float yOffset = 0f;

            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                if (activeNotifications[i] != null)
                {
                    RectTransform rect = activeNotifications[i].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -yOffset);
                        yOffset += rect.sizeDelta.y + verticalSpacing;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct NotificationData
    {
        public string message;
        public NotificationType type;
        public float duration;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Individual notification component
    /// </summary>
    public class Notification : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;

        [Header("Type Colors")]
        [SerializeField] private Color infoColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color warningColor = new Color(1f, 0.7f, 0.2f);
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f);

        public void Initialize(NotificationData data, NotificationManager manager)
        {
            if (messageText != null)
            {
                messageText.text = data.message;
            }

            // Set color based on type
            Color typeColor = GetColorForType(data.type);

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(typeColor.r, typeColor.g, typeColor.b, 0.9f);
            }

            if (iconImage != null)
            {
                iconImage.color = typeColor;
                // TODO: Set icon sprite based on type
            }

            // Fade in
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn(canvasGroup, 0.3f));
        }

        private Color GetColorForType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Info: return infoColor;
                case NotificationType.Success: return successColor;
                case NotificationType.Warning: return warningColor;
                case NotificationType.Error: return errorColor;
                default: return infoColor;
            }
        }

        private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}
