using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lineage.Managers;

namespace Lineage.UI
{
    /// <summary>
    /// Manages UI updates for resources, population, and game state
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Resource Displays")]
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI faithText;
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI populationText;

        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI seasonText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Game Speed")]
        [SerializeField] private TextMeshProUGUI gameSpeedText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speed1Button;
        [SerializeField] private Button speed2Button;
        [SerializeField] private Button speed3Button;

        [Header("Selection Info")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private TextMeshProUGUI selectedPopName;
        [SerializeField] private TextMeshProUGUI selectedPopStats;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SubscribeToEvents();
            SetupButtons();
            UpdateAllDisplays();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnFoodChanged += UpdateFoodDisplay;
                ResourceManager.Instance.OnFaithChanged += UpdateFaithDisplay;
                ResourceManager.Instance.OnWoodChanged += UpdateWoodDisplay;
            }

            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopulationChanged += UpdatePopulationDisplay;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayChanged += UpdateDayDisplay;
                TimeManager.Instance.OnSeasonChanged += UpdateSeasonDisplay;
                TimeManager.Instance.OnTimeOfDayChanged += UpdateTimeDisplay;
                TimeManager.Instance.OnGameSpeedChanged += UpdateGameSpeedDisplay;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnFoodChanged -= UpdateFoodDisplay;
                ResourceManager.Instance.OnFaithChanged -= UpdateFaithDisplay;
                ResourceManager.Instance.OnWoodChanged -= UpdateWoodDisplay;
            }

            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopulationChanged -= UpdatePopulationDisplay;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayChanged -= UpdateDayDisplay;
                TimeManager.Instance.OnSeasonChanged -= UpdateSeasonDisplay;
                TimeManager.Instance.OnTimeOfDayChanged -= UpdateTimeDisplay;
                TimeManager.Instance.OnGameSpeedChanged -= UpdateGameSpeedDisplay;
            }
        }

        private void SetupButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => TimeManager.Instance?.TogglePause());
            
            if (speed1Button != null)
                speed1Button.onClick.AddListener(() => TimeManager.Instance?.SetGameSpeed(1f));
            
            if (speed2Button != null)
                speed2Button.onClick.AddListener(() => TimeManager.Instance?.SetGameSpeed(2f));
            
            if (speed3Button != null)
                speed3Button.onClick.AddListener(() => TimeManager.Instance?.SetGameSpeed(3f));
        }

        private void UpdateAllDisplays()
        {
            if (ResourceManager.Instance != null)
            {
                UpdateFoodDisplay(ResourceManager.Instance.currentFood);
                UpdateFaithDisplay(ResourceManager.Instance.currentFaithPoints);
                UpdateWoodDisplay(ResourceManager.Instance.currentWood);
            }

            if (PopulationManager.Instance != null)
            {
                UpdatePopulationDisplay(PopulationManager.Instance.currentPopulation);
            }

            if (TimeManager.Instance != null)
            {
                UpdateDayDisplay(TimeManager.Instance.CurrentDay);
                UpdateSeasonDisplay(TimeManager.Instance.CurrentSeason);
                UpdateTimeDisplay(TimeManager.Instance.CurrentGameTime);
                UpdateGameSpeedDisplay(TimeManager.Instance.GameSpeed);
            }
        }

        private void UpdateFoodDisplay(float amount)
        {
            if (foodText != null)
                foodText.text = $"Food: {Mathf.FloorToInt(amount)}";
        }

        private void UpdateFaithDisplay(float amount)
        {
            if (faithText != null)
                faithText.text = $"Faith: {Mathf.FloorToInt(amount)}";
        }

        private void UpdateWoodDisplay(float amount)
        {
            if (woodText != null)
                woodText.text = $"Wood: {Mathf.FloorToInt(amount)}";
        }

        private void UpdatePopulationDisplay(int population)
        {
            if (populationText != null)
            {
                int cap = PopulationManager.Instance != null ? PopulationManager.Instance.populationCap : 0;
                populationText.text = $"Pop: {population}/{cap}";
            }
        }

        private void UpdateDayDisplay(int day)
        {
            if (dayText != null)
                dayText.text = $"Day {day}";
        }

        private void UpdateSeasonDisplay(int season)
        {
            if (seasonText != null)
            {
                string seasonName = TimeManager.Instance != null ? TimeManager.Instance.SeasonName : "Spring";
                seasonText.text = seasonName;
            }
        }

        private void UpdateTimeDisplay(float gameTime)
        {
            if (timeText != null)
            {
                int hours = Mathf.FloorToInt(gameTime);
                int minutes = Mathf.FloorToInt((gameTime - hours) * 60);
                timeText.text = $"{hours:00}:{minutes:00}";
            }
        }

        private void UpdateGameSpeedDisplay(float speed)
        {
            if (gameSpeedText != null)
            {
                if (TimeManager.Instance != null && TimeManager.Instance.IsPaused)
                    gameSpeedText.text = "|| PAUSED";
                else
                    gameSpeedText.text = $"Speed: {speed:F1}x";
            }
        }

        public void ShowSelectionInfo(Entities.Pop pop)
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                
                if (selectedPopName != null)
                    selectedPopName.text = pop.popName;
                
                if (selectedPopStats != null)
                {
                    selectedPopStats.text = $"Health: {pop.health:F0}/{pop.maxHealth:F0}\n" +
                                           $"Hunger: {pop.hunger:F0}%\n" +
                                           $"Thirst: {pop.thirst:F0}%";
                }
            }
        }

        public void HideSelectionInfo()
        {
            if (selectionPanel != null)
                selectionPanel.SetActive(false);
        }
    }
}
