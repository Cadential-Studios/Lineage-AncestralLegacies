using UnityEngine;
using Lineage.Core.Entities;
using Lineage.Core.Items;
using Lineage.Entities;
using LegacyEntityType = Lineage.Entities.EntityType;

namespace Lineage.Core
{
    /// <summary>
    /// Bridge utility to help transition from the legacy EntityTypeData system
    /// to the new GameDataManager/EntityDefinitionSO system.
    /// Provides methods to convert between old and new data formats.
    /// </summary>
    public static class GameDataBridge
    {
        /// <summary>
        /// Try to get an EntityDefinitionSO from the GameDataManager.
        /// Falls back to null if not found (caller should then use legacy system).
        /// </summary>
        /// <param name="uniqueID">The unique identifier of the entity definition.</param>
        /// <returns>The EntityDefinitionSO if found, otherwise null.</returns>
        public static EntityDefinitionSO TryGetEntityDefinition(string uniqueID)
        {
            if (string.IsNullOrEmpty(uniqueID)) return null;
            
            if (GameDataManager.Instance != null)
            {
                return GameDataManager.Instance.GetEntityDefinition(uniqueID);
            }
            return null;
        }

        /// <summary>
        /// Try to get an ItemDefinitionSO from the GameDataManager.
        /// </summary>
        /// <param name="uniqueID">The unique identifier of the item definition.</param>
        /// <returns>The ItemDefinitionSO if found, otherwise null.</returns>
        public static ItemDefinitionSO TryGetItemDefinition(string uniqueID)
        {
            if (string.IsNullOrEmpty(uniqueID)) return null;
            
            if (GameDataManager.Instance != null)
            {
                return GameDataManager.Instance.GetItemDefinition(uniqueID);
            }
            return null;
        }

        /// <summary>
        /// Apply EntityDefinitionSO data to an Entity component.
        /// Used during entity initialization when using the new data system.
        /// Note: This modifies stats additively from their current values.
        /// For setting base stats, ensure entity has default values first.
        /// </summary>
        /// <param name="entity">The Entity component to configure.</param>
        /// <param name="definition">The EntityDefinitionSO to apply.</param>
        public static void ApplyDefinitionToEntity(Entity entity, EntityDefinitionSO definition)
        {
            if (entity == null || definition == null) return;

            // Apply resource tags
            foreach (var tag in definition.resourceTags)
            {
                entity.AddResourceTag(tag);
            }

            // Apply behavior subtags
            foreach (var subtag in definition.behaviorSubtags)
            {
                entity.AddBehaviorSubtag(subtag);
            }

            // Note: Base stats should be set through the entity's data initialization
            // The Entity class uses ModifyStat for runtime changes, not base setup
            // This bridge is primarily for tag/behavior configuration
        }

        /// <summary>
        /// Check if an EntityDefinitionSO exists in the GameData system.
        /// </summary>
        /// <param name="uniqueID">The unique identifier to check.</param>
        /// <returns>True if the definition exists, otherwise false.</returns>
        public static bool HasEntityDefinition(string uniqueID)
        {
            return TryGetEntityDefinition(uniqueID) != null;
        }

        /// <summary>
        /// Check if an ItemDefinitionSO exists in the GameData system.
        /// </summary>
        /// <param name="uniqueID">The unique identifier to check.</param>
        /// <returns>True if the definition exists, otherwise false.</returns>
        public static bool HasItemDefinition(string uniqueID)
        {
            return TryGetItemDefinition(uniqueID) != null;
        }

        /// <summary>
        /// Convert legacy EntityType enum to new EntityCategory.
        /// </summary>
        /// <param name="legacyType">The legacy EntityType value.</param>
        /// <returns>The corresponding EntityCategory value.</returns>
        public static EntityCategory ConvertEntityType(LegacyEntityType legacyType)
        {
            return legacyType switch
            {
                LegacyEntityType.Pop => EntityCategory.Pop,
                LegacyEntityType.Animal => EntityCategory.Animal,
                LegacyEntityType.NPC => EntityCategory.NPC,
                LegacyEntityType.Structure => EntityCategory.Structure,
                LegacyEntityType.Object => EntityCategory.Resource,
                _ => EntityCategory.Other
            };
        }

        /// <summary>
        /// Convert new EntityCategory to legacy EntityType enum.
        /// </summary>
        /// <param name="category">The EntityCategory value.</param>
        /// <returns>The corresponding legacy EntityType value.</returns>
        public static LegacyEntityType ConvertEntityCategory(EntityCategory category)
        {
            return category switch
            {
                EntityCategory.Pop => LegacyEntityType.Pop,
                EntityCategory.Animal => LegacyEntityType.Animal,
                EntityCategory.NPC => LegacyEntityType.NPC,
                EntityCategory.Structure => LegacyEntityType.Structure,
                EntityCategory.Resource => LegacyEntityType.Object,
                _ => LegacyEntityType.Object
            };
        }
    }
}
// Pseudocode generated by codewrx.ai
