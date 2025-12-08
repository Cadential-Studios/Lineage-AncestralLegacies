using System.Collections.Generic;

namespace Lineage.Entities
{
    /// <summary>
    /// Represents all stat types available for entities and their current/max values.
    /// </summary>
    public static class Stat
    {
        /// <summary>
        /// Enum for all available stat types.
        /// </summary>
        public enum ID
        {
            // Primary Stats
            Health = 0,
            Mana = 1,
            Stamina = 2,
            Energy = 3,
            
            // Needs
            Hunger = 10,
            Thirst = 11,
            
            // Attributes
            Strength = 20,
            Agility = 21,
            Intelligence = 22,
            Defense = 23,
            Speed = 24,
            
            // Combat
            Attack = 30,
            MagicPower = 31,
            MagicDefense = 32,
            CriticalHitChance = 33,
            CriticalHitDamage = 34,
            
            // Social
            Charisma = 40,
            Luck = 41
        }

        /// <summary>
        /// Stat data structure containing current and max values.
        /// </summary>
        public struct StatValue
        {
            public float currentValue;
            public float maxValue;
            public float baseValue;

            public StatValue(float baseValue = 100f, float currentValue = -1f)
            {
                this.baseValue = baseValue;
                this.maxValue = baseValue;
                this.currentValue = currentValue < 0 ? baseValue : currentValue;
            }

            public float GetPercentage() => maxValue > 0 ? currentValue / maxValue : 0f;
            public bool IsActive() => currentValue > 0;
        }
    }

    /// <summary>
    /// Represents all available entity states/behaviors.
    /// </summary>
    public static class State
    {
        /// <summary>
        /// Enum for all available state types.
        /// </summary>
        public enum ID
        {
            // Basic States
            Idle = 0,
            Moving = 1,
            Interacting = 2,
            
            // Activity States
            Gathering = 10,
            Crafting = 11,
            Eating = 12,
            Drinking = 13,
            Resting = 14,
            Sleeping = 15,
            
            // Social States
            Socializing = 20,
            Mating = 21,
            Fleeing = 22,
            
            // Combat States
            Defending = 30,
            Attacking = 31,
            
            // Status States
            Dead = 100,
            Incapacitated = 101
        }
    }
}
