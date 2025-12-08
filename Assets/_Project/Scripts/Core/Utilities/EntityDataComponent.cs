using UnityEngine;
using System.Collections.Generic;
using Lineage.Debug;

#pragma warning disable 0414 // Suppress unused field warnings for inspector fields

using DBStat = Lineage.Database.Stat;
using DBEntity = Lineage.Database.Entity;
using DBState = Lineage.Database.State;
using DBBuff = Lineage.Database.Buff;
using DBHealth = Lineage.Database.Health;

namespace Lineage.Components
{
    /// <summary>
    /// Component that stores complete GameData Entity information for a Pop.
    /// This bridges the gap between the old Pop system and the new GameData system.
    /// </summary>
    public class EntityDataComponent : MonoBehaviour
    {
        [Header("Entity Data")]
        [SerializeField] private DBEntity _entityData;

        [Header("Identity")]
        [SerializeField] private string _entityName = "Unknown Entity";
        [SerializeField] private int _entityID = 0;
        [SerializeField] private int _entityAge = 0; // Placeholder for age tracking

        [Header("Base Stats")]
        [SerializeField] private float _entityHunger = 100f; // Default starting hunger (full)
        [SerializeField] private float _entityThirst = 100f; // Default starting thirst (full)
        [SerializeField] private float _entityEnergy = 100f; // Default starting energy (full)
        [SerializeField] private float _entitySpeed = 5f; // Default speed
        [SerializeField] private float _entityHealth = 100f; // Default health
        [SerializeField] private float _entityMana = 50f; // Default mana

        [Header("Max Stat Values")]
        [SerializeField] private float _entityMaxHunger = 100f; // Default max hunger
        [SerializeField] private float _entityMaxThirst = 100f; // Default max thirst
        [SerializeField] private float _entityMaxEnergy = 100f; // Default max energy
        [SerializeField] private float _entityMaxSpeed = 10f; // Default max speed
        [SerializeField] private float _entityMaxHealth = 100f; // Default max health
        [SerializeField] private float _entityMaxMana = 100f; // Default max mana

        [Header("Ability Stats")]
        [SerializeField] private float _entityStrength = 10f; // Default strength
        [SerializeField] private float _entityAgility = 10f; // Default agility
        [SerializeField] private float _entityIntelligence = 10f; // Default intelligence
        [SerializeField] private float _entityDefense = 10f; // Default defense
        [SerializeField] private float _entityLuck = 5f; // Default luck
        [SerializeField] private float _entityCharisma = 5f; // Default charisma

        [Header("Combat Stats")]
        [Tooltip("These stats are used for combat calculations and can be modified by buffs or equipment. Found in EntityData.")]

        [SerializeField] private float _entityAttack = 10f; // Default attack
        [SerializeField] private float _entityMagicPower = 10f; // Default magic power
        [SerializeField] private float _entityMagicDefense = 10f; // Default magic defense
        [SerializeField] private float _entityCriticalHitChance = 5f; // Default critical hit chance
        [SerializeField] private float _entityCriticalHitDamage = 150f; // Default critical hit damage

        [Header("Traits")]
        [SerializeField] private List<string> _entityTraits = new List<string>(); // Traits that modify stats or abilities

        [Header("Crafting")]
        [SerializeField] private bool _entitycanCraft = false; // Can this entity craft items?
        [SerializeField] private List<string> _craftingRecipes = new List<string>(); // List of available crafting recipes
        [Header("Runtime State")]
        public bool isInitialized = false;

        // Properties for easy access
        public DBEntity EntityData
        {
            get => _entityData;
            set
            {
                _entityData = value;
                isInitialized = true;
                OnEntityDataChanged?.Invoke(_entityData);
            }
        }

        // Events for when entity data changes
        public System.Action<DBEntity> OnEntityDataChanged;
        public System.Action<DBStat.ID, float> OnStatChanged;
        public System.Action<DBState.ID> OnStateChanged;
        private void Start()
        {
            if (!isInitialized)
            {
                Log.Warning($"{gameObject.name} EntityDataComponent is not initialized!", Log.LogCategory.General, this);
            }
        }



        #region Stat Management

        /// <summary>
        /// Gets a stat by ID from the entity data.
        /// </summary>
        public DBStat GetStat(DBStat.ID statID)
        {
            switch (statID)
            {
                // Get all the stats from EntityData and return them.

                case DBStat.ID.Health: return new DBStat(DBStat.ID.Health, "Health", _entityData.health.current);
                case DBStat.ID.Mana: return _entityData.mana;
                case DBStat.ID.Stamina: return _entityData.stamina;
                case DBStat.ID.Strength: return _entityData.strength;
                case DBStat.ID.Agility: return _entityData.agility;
                case DBStat.ID.Intelligence: return _entityData.intelligence;
                case DBStat.ID.Defense: return _entityData.defense;
                case DBStat.ID.Speed: return _entityData.speed;
                case DBStat.ID.Attack: return _entityData.attack;
                case DBStat.ID.MagicPower: return _entityData.magicPower;
                case DBStat.ID.MagicDefense: return _entityData.magicDefense;
                case DBStat.ID.CriticalHitChance: return _entityData.criticalHitChance;
                case DBStat.ID.CriticalHitDamage: return _entityData.criticalHitDamage;
                case DBStat.ID.Luck: return _entityData.luck;
                case DBStat.ID.Charisma: return _entityData.charisma;
                // Needs system stats
                case DBStat.ID.Hunger: return _entityData.hunger;
                case DBStat.ID.Thirst: return _entityData.thirst;
                case DBStat.ID.Energy: return _entityData.energy;
                case DBStat.ID.Rest: return _entityData.rest;
                // Experience and Level stats
                case DBStat.ID.Experience: return _entityData.experience;
                case DBStat.ID.Level: return _entityData.levelStat;


                default:
                    Log.Warning($"Stat {statID} not found in EntityData", Log.LogCategory.General, this);
                    return new DBStat(statID, statID.ToString(), 0f);
            }
        }

        /// <summary>
        /// Modifies a stat and updates the entity data.
        /// </summary>
        public void ModifyStat(DBStat.ID statID, float amount)
        {
            var entityData = _entityData;

            switch (statID)
            {
                case DBStat.ID.Health:
                    if (amount > 0) entityData.health = entityData.health.Heal(amount);
                    else entityData.health = entityData.health.TakeDamage(-amount);
                    break;
                case DBStat.ID.Mana: entityData.mana.ModifyStat(amount); break;
                case DBStat.ID.Stamina: entityData.stamina.ModifyStat(amount); break;
                case DBStat.ID.Strength: entityData.strength.ModifyStat(amount); break;
                case DBStat.ID.Agility: entityData.agility.ModifyStat(amount); break;
                case DBStat.ID.Intelligence: entityData.intelligence.ModifyStat(amount); break;
                case DBStat.ID.Defense: entityData.defense.ModifyStat(amount); break;
                case DBStat.ID.Speed: entityData.speed.ModifyStat(amount); break;
                case DBStat.ID.Attack: entityData.attack.ModifyStat(amount); break;
                case DBStat.ID.MagicPower: entityData.magicPower.ModifyStat(amount); break;
                case DBStat.ID.MagicDefense: entityData.magicDefense.ModifyStat(amount); break;
                case DBStat.ID.CriticalHitChance: entityData.criticalHitChance.ModifyStat(amount); break;
                case DBStat.ID.CriticalHitDamage: entityData.criticalHitDamage.ModifyStat(amount); break;
                case DBStat.ID.Luck: entityData.luck.ModifyStat(amount); break;
                case DBStat.ID.Charisma: entityData.charisma.ModifyStat(amount); break;
                // Needs system stats
                case DBStat.ID.Hunger: entityData.hunger.ModifyStat(amount); break;
                case DBStat.ID.Thirst: entityData.thirst.ModifyStat(amount); break;
                case DBStat.ID.Energy: entityData.energy.ModifyStat(amount); break;
                case DBStat.ID.Rest: entityData.rest.ModifyStat(amount); break;
                // Experience and Level stats
                case DBStat.ID.Experience: entityData.experience.ModifyStat(amount); break;
                case DBStat.ID.Level:
                    entityData.levelStat.ModifyStat(amount);
                    // Also update the int level field to keep them in sync
                    entityData.level = Mathf.RoundToInt(entityData.levelStat.currentValue);
                    break;
                default:
                    Log.Warning($"Cannot modify unknown stat: {statID}", Log.LogCategory.General, this);
                    break;
            }

            _entityData = entityData;
            OnStatChanged?.Invoke(statID, amount);
        }

        #endregion

        #region State Management

        /// <summary>
        /// Changes the entity's current state.
        /// </summary>
        public bool ChangeState(DBState.ID newStateID)
        {
            var entityData = _entityData;
            entityData.ChangeState(newStateID);
            _entityData = entityData;

            OnStateChanged?.Invoke(newStateID);
            return true;
        }

        /// <summary>
        /// Gets the current state of the entity.
        /// </summary>
        public DBState GetCurrentState()
        {
            return _entityData.currentState;
        }

        /// <summary>
        /// Gets all available states for this entity.
        /// </summary>
        public List<DBState> GetAvailableStates()
        {
            return _entityData.availableStates ?? new List<DBState>();
        }
        #endregion

        #region Buff Management

        /// <summary>
        /// Applies a buff to the entity.
        /// </summary>
        public void ApplyBuff(DBBuff buff)
        {
            var entityData = _entityData;
            entityData.ApplyBuff(buff);
            _entityData = entityData;
        }

        /// <summary>
        /// Removes a buff from the entity.
        /// </summary>
        public void RemoveBuff(int buffID)
        {
            var entityData = _entityData;
            entityData.RemoveBuff(buffID);
            _entityData = entityData;
        }

        /// <summary>
        /// Gets all active buffs on the entity.
        /// </summary>
        public List<DBBuff> GetActiveBuffs()
        {
            return _entityData.activeBuffs ?? new List<DBBuff>();
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculates the overall "combat power" of this entity.
        /// </summary>
        public float GetCombatPower()
        {
            return (_entityData.attack.currentValue + _entityData.defense.currentValue +
                   _entityData.strength.currentValue + _entityData.agility.currentValue) / 4f;
        }

        /// <summary>
        /// Calculates the overall "social power" of this entity.
        /// </summary>
        public float GetSocialPower()
        {
            return (_entityData.charisma.currentValue + _entityData.intelligence.currentValue +
                   _entityData.luck.currentValue) / 3f;
        }

        /// <summary>
        /// Gets the entity's current age (you'll need to implement age tracking).
        /// </summary>
        public int GetAge()
        {
            // This would need to be implemented based on your age system
            // For now, return a placeholder
            return _entityData.level * 5; // Rough approximation
        }

        /// <summary>
        /// Checks if the entity is alive and healthy.
        /// </summary>
        public bool IsHealthy()
        {
            return _entityData.isAlive && _entityData.health.current > (_entityData.health.max * 0.5f);
        }

        #endregion

        #region Needs Management

        [Header("Needs System Configuration")]
        [SerializeField] private bool enableNeedsDecay = false; // DISABLED FOR TESTING - set to true when fixed
        [SerializeField] private float hungerDecayRate = 0.1f;  // Per second: 100/0.1 = 1000s = 16.7 min to starve
        [SerializeField] private float thirstDecayRate = 0.15f; // Per second: 100/0.15 = 667s = 11 min to dehydrate
        [SerializeField] private float energyDecayRate = 0.08f;
        [SerializeField] private float restDecayRate = 0.05f;

        /// <summary>
        /// Updates needs decay over time. Call this from Update() or a manager.
        /// </summary>
        public void UpdateNeeds(float deltaTime)
        {
            // GUARD: Never decay uninitialized entities
            if (!isInitialized) return;
            if (!enableNeedsDecay) return;

            // Apply decay rates to needs
            ModifyStat(DBStat.ID.Hunger, -hungerDecayRate * deltaTime);
            // Apply decay rates to needs
            ModifyStat(DBStat.ID.Hunger, -hungerDecayRate * deltaTime);
            ModifyStat(DBStat.ID.Thirst, -thirstDecayRate * deltaTime);
            ModifyStat(DBStat.ID.Energy, -energyDecayRate * deltaTime);
            ModifyStat(DBStat.ID.Rest, -restDecayRate * deltaTime);
        }

        /// <summary>
        /// Satisfies hunger by the given amount.
        /// </summary>
        public void EatFood(float amount)
        {
            ModifyStat(DBStat.ID.Hunger, amount);
        }

        /// <summary>
        /// Satisfies thirst by the given amount.
        /// </summary>
        public void DrinkWater(float amount)
        {
            ModifyStat(DBStat.ID.Thirst, amount);
        }

        /// <summary>
        /// Restores energy by the given amount.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            ModifyStat(DBStat.ID.Energy, amount);
        }

        /// <summary>
        /// Restores rest by the given amount.
        /// </summary>
        public void Sleep(float amount)
        {
            ModifyStat(DBStat.ID.Rest, amount);
        }

        /// <summary>
        /// Gets the current hunger level (0-100).
        /// </summary>
        public float GetHunger() => GetStat(DBStat.ID.Hunger).currentValue;

        /// <summary>
        /// Gets the current thirst level (0-100).
        /// </summary>
        public float GetThirst() => GetStat(DBStat.ID.Thirst).currentValue;

        /// <summary>
        /// Gets the current energy level (0-100).
        /// </summary>
        public float GetEnergy() => GetStat(DBStat.ID.Energy).currentValue;

        /// <summary>
        /// Gets the current rest level (0-100).
        /// </summary>
        public float GetRest() => GetStat(DBStat.ID.Rest).currentValue;        /// Returns false if entity data is not initialized.
                                                                               /// </summary>
        public bool HasCriticalNeeds()
        {
            // NEVER kill uninitialized entities - their stats are 0 by default
            if (!isInitialized)
            {
                return false;
            }

            // Only die if hunger OR thirst reach absolute zero
            float hunger = GetHunger();
            float thirst = GetThirst();

            // Additional safety: if both are exactly 0, likely uninitialized struct
            if (hunger == 0f && thirst == 0f)
            {
                UnityEngine.Debug.LogWarning($"[Entity] {gameObject.name} has 0/0 hunger/thirst - likely uninitialized!");
                return false;
            }

            bool critical = hunger <= 0f || thirst <= 0f;

            if (critical)
            {
                UnityEngine.Debug.LogWarning($"[Entity] {gameObject.name} critical needs! H:{hunger:F0} T:{thirst:F0}");
            }

            return critical;
        }

        /// <summary>
        /// Sets the base value of a stat. Use this for initial setup rather than ModifyStat.
        /// </summary>
        /// <param name="statID">The stat to set.</param>
        /// <summary>
        /// Sets the base value of a stat. Use this for initial setup rather than ModifyStat.
        /// </summary>
        /// <param name="statID">The stat to set.</param>
        /// <param name="value">The new base value.</param>
        public void SetBaseStat(DBStat.ID statID, float value)
        {
            var entityData = _entityData;

            switch (statID)
            {
                case DBStat.ID.Health:
                    entityData.health = new DBHealth(value, value);
                    break;
                case DBStat.ID.Mana:
                    entityData.mana = new DBStat(statID, "Mana", value);
                    break;
                case DBStat.ID.Stamina:
                    entityData.stamina = new DBStat(statID, "Stamina", value);
                    break;
                case DBStat.ID.Strength:
                    entityData.strength = new DBStat(statID, "Strength", value);
                    break;
                case DBStat.ID.Agility:
                    entityData.agility = new DBStat(statID, "Agility", value);
                    break;
                case DBStat.ID.Intelligence:
                    entityData.intelligence = new DBStat(statID, "Intelligence", value);
                    break;
                case DBStat.ID.Defense:
                    entityData.defense = new DBStat(statID, "Defense", value);
                    break;
                case DBStat.ID.Speed:
                    entityData.speed = new DBStat(statID, "Speed", value);
                    break;
                case DBStat.ID.Attack:
                    entityData.attack = new DBStat(statID, "Attack", value);
                    break;
                case DBStat.ID.Hunger:
                    entityData.hunger = new DBStat(statID, "Hunger", value);
                    break;
                case DBStat.ID.Thirst:
                    entityData.thirst = new DBStat(statID, "Thirst", value);
                    break;
                case DBStat.ID.Energy:
                    entityData.energy = new DBStat(statID, "Energy", value);
                    break;
                default:
                    Log.Warning($"Cannot set base stat for {statID}", Log.LogCategory.General, this);
                    break;
            }

            _entityData = entityData;
            OnStatChanged?.Invoke(statID, value);
        }

        #endregion
    }
}