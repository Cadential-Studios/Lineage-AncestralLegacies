using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Lineage.Managers;
using Lineage.Debug;

namespace Lineage.UI
{
    /// <summary>
    /// Smart button that automatically determines its functionality based on its text label.
    /// Attach this to any button and it will automatically handle the appropriate logic
    /// based on what the button's text says.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SmartButton : MonoBehaviour
    {
        [Header("Smart Button Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool useCustomText = false;
        [SerializeField] private string customButtonText = "";

        private Button button;
        private TMP_Text buttonText;
        private string currentButtonText;

        // Button action mappings - you can easily add more here!
        private readonly System.Collections.Generic.Dictionary<string, System.Action> buttonActions =
            new System.Collections.Generic.Dictionary<string, System.Action>(System.StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            // Get components
            button = GetComponent<Button>();

            // Try to find TMP_Text component on this object or children
            buttonText = GetComponentInChildren<TMP_Text>();

            if (buttonText == null)
            {
                // Fallback to regular Text component if TMP_Text not found
                var regularText = GetComponentInChildren<Text>();
                if (regularText != null)
                {
                    Log.Warning($"SmartButton on {gameObject.name}: Using regular Text instead of TMP_Text. Consider upgrading to TextMeshPro for better performance.", Log.LogCategory.UI);
                }
                else
                {
                    Log.Error($"SmartButton on {gameObject.name}: No Text or TMP_Text component found! Button will not function properly.", Log.LogCategory.UI);
                    return;
                }
            }

            // Initialize button actions
            InitializeButtonActions();

            // Set up the button click listener
            SetupButtonListener();
        }

        private void InitializeButtonActions()
        {
            // Clear existing actions
            buttonActions.Clear();

            // Game Flow Actions
            buttonActions["Quit"] = QuitGame;
            buttonActions["Exit"] = QuitGame;
            buttonActions["Quit Game"] = QuitGame;
            buttonActions["Exit Game"] = QuitGame;

            buttonActions["Play"] = PlayGame;
            buttonActions["Start"] = PlayGame;
            buttonActions["Start Game"] = PlayGame;
            buttonActions["New Game"] = PlayGame;

            buttonActions["Continue"] = ContinueGame;
            buttonActions["Resume"] = ResumeGame;
            buttonActions["Resume Game"] = ResumeGame;

            buttonActions["Restart"] = RestartGame;
            buttonActions["Restart Game"] = RestartGame;

            // Menu Navigation
            buttonActions["Main Menu"] = GoToMainMenu;
            buttonActions["Menu"] = GoToMainMenu;
            buttonActions["Back"] = GoBack;
            buttonActions["Return"] = GoBack;

            buttonActions["Settings"] = OpenSettings;
            buttonActions["Options"] = OpenSettings;
            buttonActions["Preferences"] = OpenSettings;

            buttonActions["Credits"] = OpenCredits;

            // Save/Load Actions
            buttonActions["Save"] = SaveGame;
            buttonActions["Save Game"] = SaveGame;
            buttonActions["Quick Save"] = QuickSave;

            buttonActions["Load"] = LoadGame;
            buttonActions["Load Game"] = LoadGame;
            buttonActions["Quick Load"] = QuickLoad;

            // Game Actions
            buttonActions["Pause"] = PauseGame;
            buttonActions["Unpause"] = UnpauseGame;

            // Population Actions
            buttonActions["Spawn Pop"] = SpawnPop;
            buttonActions["Add Pop"] = SpawnPop;
            buttonActions["Create Pop"] = SpawnPop;

            // Debug Actions (only in debug builds)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            buttonActions["Debug Console"] = OpenDebugConsole;
            buttonActions["Console"] = OpenDebugConsole;
            buttonActions["Toggle Debug"] = ToggleDebug;
            buttonActions["God Mode"] = ToggleGodMode;
#endif

            // UI Actions
            buttonActions["Close"] = CloseUI;
            buttonActions["Cancel"] = CloseUI;
            buttonActions["OK"] = CloseUI;
            buttonActions["Confirm"] = ConfirmAction;
            buttonActions["Accept"] = ConfirmAction;
            buttonActions["Yes"] = ConfirmAction;
            buttonActions["No"] = CancelAction;
            buttonActions["Decline"] = CancelAction;

            if (debugMode)
            {
                Log.Info($"SmartButton: Initialized {buttonActions.Count} button actions", Log.LogCategory.UI);
            }
        }

        private void SetupButtonListener()
        {
            if (button == null) return;

            // Remove existing listeners to avoid duplicates
            button.onClick.RemoveAllListeners();

            // Add our smart button listener
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            // Get the current button text
            string buttonText = GetButtonText();

            if (string.IsNullOrEmpty(buttonText))
            {
                Log.Warning($"SmartButton on {gameObject.name}: Button text is empty or null!", Log.LogCategory.UI);
                return;
            }

            if (debugMode)
            {
                Log.Info($"SmartButton clicked: '{buttonText}'", Log.LogCategory.UI);
            }

            // Try to find and execute the appropriate action
            if (buttonActions.TryGetValue(buttonText, out System.Action action))
            {
                try
                {
                    action.Invoke();
                    if (debugMode)
                    {
                        Log.Info($"SmartButton: Successfully executed action for '{buttonText}'", Log.LogCategory.UI);
                    }
                }
                catch (System.Exception e)
                {
                    Log.Error($"SmartButton: Error executing action for '{buttonText}': {e.Message}", Log.LogCategory.UI);
                }
            }
            else
            {
                Log.Warning($"SmartButton: No action found for button text '{buttonText}'. Available actions: {string.Join(", ", buttonActions.Keys)}", Log.LogCategory.UI);
            }
        }

        private string GetButtonText()
        {
            // Use custom text if specified
            if (useCustomText && !string.IsNullOrEmpty(customButtonText))
            {
                return customButtonText.Trim();
            }

            // Try TMP_Text first
            if (buttonText != null)
            {
                return buttonText.text.Trim();
            }

            // Fallback to regular Text
            var regularText = GetComponentInChildren<Text>();
            if (regularText != null)
            {
                return regularText.text.Trim();
            }

            return "";
        }

        #region Button Action Implementations

        // Game Flow Actions
        private void QuitGame()
        {
            Log.Info("SmartButton: Quitting game", Log.LogCategory.UI);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PlayGame()
        {
            Log.Info("SmartButton: Starting new game", Log.LogCategory.UI);
            // Load the main game scene (adjust scene name as needed)
            SceneManager.LoadScene("GameScene"); // Change to your actual game scene name
        }

        private void ContinueGame()
        {
            Log.Info("SmartButton: Continuing game", Log.LogCategory.UI);
            if (SaveManager.Instance != null)
            {
                if (SaveManager.Instance.SaveExists(0)) // Auto-save slot
                {
                    SaveManager.Instance.LoadGame(0);
                }
                else
                {
                    Log.Warning("No save file found to continue from", Log.LogCategory.UI);
                    PlayGame(); // Start new game if no save exists
                }
            }
        }

        private void ResumeGame()
        {
            Log.Info("SmartButton: Resuming game", Log.LogCategory.UI);
            Time.timeScale = 1f;
            // Hide pause menu if it exists
            var pauseMenu = FindFirstObjectByType<Canvas>()?.transform.Find("PauseMenu");
            if (pauseMenu != null)
            {
                pauseMenu.gameObject.SetActive(false);
            }
        }

        private void RestartGame()
        {
            Log.Info("SmartButton: Restarting game", Log.LogCategory.UI);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Menu Navigation
        private void GoToMainMenu()
        {
            Log.Info("SmartButton: Going to main menu", Log.LogCategory.UI);
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu"); // Change to your actual main menu scene name
        }

        private void GoBack()
        {
            Log.Info("SmartButton: Going back", Log.LogCategory.UI);
            // Try to find a menu manager or just close current UI
            CloseUI();
        }

        private void OpenSettings()
        {
            Log.Info("SmartButton: Opening settings", Log.LogCategory.UI);
            // Try to find and open settings panel
            var settingsPanel = FindFirstObjectByType<Canvas>()?.transform.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(true);
            }
            else
            {
                Log.Warning("Settings panel not found", Log.LogCategory.UI);
            }
        }

        private void OpenCredits()
        {
            Log.Info("SmartButton: Opening credits", Log.LogCategory.UI);
            var creditsPanel = FindFirstObjectByType<Canvas>()?.transform.Find("CreditsPanel");
            if (creditsPanel != null)
            {
                creditsPanel.gameObject.SetActive(true);
            }
            else
            {
                Log.Warning("Credits panel not found", Log.LogCategory.UI);
            }
        }

        // Save/Load Actions
        private void SaveGame()
        {
            Log.Info("SmartButton: Saving game", Log.LogCategory.UI);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }

        private void QuickSave()
        {
            Log.Info("SmartButton: Quick saving", Log.LogCategory.UI);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickSave();
            }
        }

        private void LoadGame()
        {
            Log.Info("SmartButton: Loading game", Log.LogCategory.UI);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame();
            }
        }

        private void QuickLoad()
        {
            Log.Info("SmartButton: Quick loading", Log.LogCategory.UI);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QuickLoad();
            }
        }

        // Game Actions
        private void PauseGame()
        {
            Log.Info("SmartButton: Pausing game", Log.LogCategory.UI);
            Time.timeScale = 0f;
        }

        private void UnpauseGame()
        {
            Log.Info("SmartButton: Unpausing game", Log.LogCategory.UI);
            Time.timeScale = 1f;
        }

        // Population Actions
        private void SpawnPop()
        {
            Log.Info("SmartButton: Spawning pop", Log.LogCategory.UI);
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.SpawnPop();
            }
        }

        // Debug Actions
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OpenDebugConsole()
        {
            Log.Info("SmartButton: Opening debug console", Log.LogCategory.UI);
            // TODO: DebugManager is in Editor folder - need to create runtime-accessible debug system
            // if (DebugManager.Instance != null && DebugManager.Instance.consoleManager != null)
            // {
            //     var console = DebugManager.Instance.consoleManager;
            // }
        }

        private void ToggleDebug()
        {
            Log.Info("SmartButton: Toggling debug mode", Log.LogCategory.UI);
            // TODO: DebugManager is in Editor folder - need to create runtime-accessible debug system
            // if (DebugManager.Instance != null)
            // {
            //     DebugManager.Instance.EnableDebugSystems(!DebugManager.Instance.enabled);
            // }
        }

        private void ToggleGodMode()
        {
            Log.Info("SmartButton: Toggling god mode", Log.LogCategory.UI);
            // Implement god mode logic here
        }
#endif

        // UI Actions
        private void CloseUI()
        {
            Log.Info("SmartButton: Closing UI", Log.LogCategory.UI);
            // Try to close parent panel or destroy this object
            Transform parent = transform.parent;
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ConfirmAction()
        {
            Log.Info("SmartButton: Confirm action", Log.LogCategory.UI);
            // Default confirm behavior - close UI
            CloseUI();
        }

        private void CancelAction()
        {
            Log.Info("SmartButton: Cancel action", Log.LogCategory.UI);
            // Default cancel behavior - close UI
            CloseUI();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Add a custom button action at runtime
        /// </summary>
        public void AddCustomAction(string buttonText, System.Action action)
        {
            buttonActions[buttonText] = action;
            if (debugMode)
            {
                Log.Info($"SmartButton: Added custom action for '{buttonText}'", Log.LogCategory.UI);
            }
        }

        /// <summary>
        /// Remove a button action
        /// </summary>
        public void RemoveAction(string buttonText)
        {
            if (buttonActions.ContainsKey(buttonText))
            {
                buttonActions.Remove(buttonText);
                if (debugMode)
                {
                    Log.Info($"SmartButton: Removed action for '{buttonText}'", Log.LogCategory.UI);
                }
            }
        }

        /// <summary>
        /// Check if an action exists for the given text
        /// </summary>
        public bool HasAction(string buttonText)
        {
            return buttonActions.ContainsKey(buttonText);
        }

        /// <summary>
        /// Get all available button actions
        /// </summary>
        public string[] GetAvailableActions()
        {
            var keys = new string[buttonActions.Count];
            buttonActions.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>
        /// Force refresh the button text and setup
        /// </summary>
        public void RefreshButton()
        {
            SetupButtonListener();
            currentButtonText = GetButtonText();
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("List Available Actions")]
        private void ListAvailableActions()
        {
            if (buttonActions.Count == 0)
            {
                InitializeButtonActions();
            }

            UnityEngine.Debug.Log($"SmartButton Available Actions ({buttonActions.Count}):\n" +
                      string.Join("\n", buttonActions.Keys));
        }

        [ContextMenu("Test Button Action")]
        private void TestButtonAction()
        {
            OnButtonClicked();
        }
#endif

        #endregion
    }
}

