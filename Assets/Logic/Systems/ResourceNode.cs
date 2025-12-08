using UnityEngine;
using Lineage.Managers;

#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Lineage.Systems.ResourceNodes
{
    /// <summary>
    /// Resource node that can be harvested by pops
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] private ResourceType resourceType = ResourceType.Food;
        [SerializeField] private float maxResourceAmount = 100f;
        [SerializeField] private float currentResourceAmount = 100f;
        [SerializeField] private float regenerationRate = 5f; // Per second
        [SerializeField] private float harvestRate = 10f; // Per second when being harvested

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color depletedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private Color originalColor;

        public enum ResourceType
        {
            Food,
            Wood,
            Stone,
            Water
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            currentResourceAmount = maxResourceAmount;
        }

        private void Update()
        {
            // Regenerate resources over time
            if (currentResourceAmount < maxResourceAmount)
            {
                currentResourceAmount = Mathf.Min(currentResourceAmount + regenerationRate * Time.deltaTime, maxResourceAmount);
                UpdateVisuals();
            }
        }

        public bool CanHarvest()
        {
            return currentResourceAmount > 0;
        }

        public float Harvest(float amount)
        {
            float harvestedAmount = Mathf.Min(amount, currentResourceAmount);
            currentResourceAmount -= harvestedAmount;
            UpdateVisuals();
            return harvestedAmount;
        }

        public ResourceType GetResourceType()
        {
            return resourceType;
        }

        public float GetResourcePercentage()
        {
            return currentResourceAmount / maxResourceAmount;
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            float percentage = GetResourcePercentage();
            spriteRenderer.color = Color.Lerp(depletedColor, originalColor, percentage);
            spriteRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, percentage);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1.5f); // Harvest range visualization
        }
    }
}
