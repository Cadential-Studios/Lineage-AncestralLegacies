using UnityEngine;
using UnityEngine.UI;
using Lineage.Debug;

namespace Lineage.UI
{
    /// <summary>
    /// Handles menu button interactions to toggle visibility of UI panels
    /// </summary>
    public class MenuButtonHandler : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button commandsButton;
        [SerializeField] private Button announcementsButton;
        [SerializeField] private Button inspectorButton;

        [Header("Menu Panels")]
        [SerializeField] private GameObject commandsPanel;
        [SerializeField] private GameObject announcementsPanel;
        [SerializeField] private GameObject inspectorPanel;

        private void Start()
        {
            SetupButtons();
            FindPanels();
        }

        private void SetupButtons()
        {
            // Find buttons by name if not assigned
            if (commandsButton == null)
            {
                var stoneButton = GameObject.Find("Stone Button");
                if (stoneButton != null)
                    commandsButton = stoneButton.GetComponentInChildren<Button>();
            }

            if (announcementsButton == null)
            {
                var stoneButton2 = GameObject.Find("Stone Button (2)");
                if (stoneButton2 != null)
                    announcementsButton = stoneButton2.GetComponentInChildren<Button>();
            }

            if (inspectorButton == null)
            {
                var stoneButton1 = GameObject.Find("Stone Button (1)");
                if (stoneButton1 != null)
                    inspectorButton = stoneButton1.GetComponentInChildren<Button>();
            }

            // Assign click listeners
            if (commandsButton != null)
            {
                commandsButton.onClick.RemoveAllListeners();
                commandsButton.onClick.AddListener(OnCommandsClicked);
                UpdateButtonText(commandsButton.gameObject, "Commands");
            }

            if (announcementsButton != null)
            {
                announcementsButton.onClick.RemoveAllListeners();
                announcementsButton.onClick.AddListener(OnAnnouncementsClicked);
                UpdateButtonText(announcementsButton.gameObject, "Announcements");
            }

            if (inspectorButton != null)
            {
                inspectorButton.onClick.RemoveAllListeners();
                inspectorButton.onClick.AddListener(OnInspectorClicked);
                UpdateButtonText(inspectorButton.gameObject, "Inspector");
            }
        }

        private void FindPanels()
        {
            // Find panels by name if not assigned
            if (commandsPanel == null)
                commandsPanel = GameObject.Find("Commands_Menu");

            if (announcementsPanel == null)
                announcementsPanel = GameObject.Find("Announcements_Menu");

            if (inspectorPanel == null)
                inspectorPanel = GameObject.Find("Inspector_Menu");

            // Log if panels not found
            if (commandsPanel == null)
                Log.Warning("Commands_Menu panel not found!", Log.LogCategory.UI);
            if (announcementsPanel == null)
                Log.Warning("Announcements_Menu panel not found!", Log.LogCategory.UI);
            if (inspectorPanel == null)
                Log.Warning("Inspector_Menu panel not found!", Log.LogCategory.UI);
        }

        private void UpdateButtonText(GameObject buttonObject, string text)
        {
            var textComponent = buttonObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        private void OnCommandsClicked()
        {
            if (commandsPanel != null)
            {
                bool newState = !commandsPanel.activeSelf;
                commandsPanel.SetActive(newState);
                Log.Info($"Commands panel {(newState ? "shown" : "hidden")}", Log.LogCategory.UI);
            }
        }

        private void OnAnnouncementsClicked()
        {
            if (announcementsPanel != null)
            {
                bool newState = !announcementsPanel.activeSelf;
                announcementsPanel.SetActive(newState);
                Log.Info($"Announcements panel {(newState ? "shown" : "hidden")}", Log.LogCategory.UI);
            }
        }

        private void OnInspectorClicked()
        {
            if (inspectorPanel != null)
            {
                bool newState = !inspectorPanel.activeSelf;
                inspectorPanel.SetActive(newState);
                Log.Info($"Inspector panel {(newState ? "shown" : "hidden")}", Log.LogCategory.UI);
            }
        }
    }
}
