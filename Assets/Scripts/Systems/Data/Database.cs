using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lineage.Database
{
#region Global Enums

    /// <summary>
    /// Defines the rarity levels for various game elements.
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Specifies the type of element a rarity level can apply to.
    /// </summary>
    public enum RarityType
    {
        Entity, // For entities like monsters, NPCs, etc.
        Item, // For items like weapons, armor, etc.
        Skill, // For skills that can be learned or used
        Buff, // For buffs that can be applied to entities
        Quest // For quests that can be undertaken
    }

    #endregion

#region Utility Structs
    
    #region Description
    /// <summary>
    /// Contains utility structs for various gameplay elements.
    /// </summary>
    #endregion

    /// <summary>
    /// Represents health data with utility methods for managing current and maximum health.
    /// </summary>
    public struct Health  // todo: find a way to implement this into the Stat struct to become a Stat<Health>?
    {
        /// <summary>
        /// The current health value.
        /// </summary>
        public float current;
        /// <summary>
        /// The maximum possible health value.
        /// </summary>
        public float max;

        /// <summary>
        /// Initializes a new instance of the <see cref="Health"/> struct.
        /// </summary>
        /// <param name="max">The maximum health.</param>
        /// <param name="current">The current health. If less than 0, defaults to max health.</param>
        public Health(float max, float current = -1f)
        {
            this.max = max;
            this.current = current < 0 ? max : current;
        }

        /// <summary>
        /// Gets the health percentage (current / max).
        /// </summary>
        public readonly float Percentage => max > 0 ? current / max : 0f;
        /// <summary>
        /// Gets a value indicating whether the entity is alive (current health > 0).
        /// </summary>
        public readonly bool IsAlive => current > 0f;
        /// <summary>
        /// Gets a value indicating whether the health is above 50%.
        /// </summary>
        public readonly bool IsHealthy => Percentage > 0.5f;
        /// <summary>
        /// Gets a value indicating whether the health is below 25% (critical).
        /// </summary>
        public readonly bool IsCritical => Percentage < 0.25f;

        /// <summary>
        /// Reduces health by the specified damage amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health TakeDamage(float damage)
        {
            return new Health(max, Mathf.Max(0f, current - damage));
        }

        /// <summary>
        /// Increases health by the specified amount, capped at maximum health.
        /// </summary>
        /// <param name="amount">The amount of health to restore.</param>
        /// <returns>A new Health struct with the updated health.</returns>
        public readonly Health Heal(float amount)
        {
            return new Health(max, Mathf.Min(max, current + amount));
        }
    }


    public struct Age
    {
        /// <summary>
        /// The current age in years.
        /// </summary>
        public readonly int currentAge;
        /// <summary>
        /// The maximum age this entity can reach.
        /// </summary>
        public readonly int maxAge;
        public StatModifiers modifiers; // Optional modifiers for age-related effects

        /// <summary>
        /// Initializes a new instance of the <see cref="Age"/> struct.
        /// </summary>
        /// <param name="max">The maximum age.</param>
        /// <param name="current">The current age. If less than 0, defaults to 0.</param>
        /// <param name="ageModifiers">Optional modifiers for age-related effects.</param>
        public Age(int max, int current = -1, StatModifiers ageModifiers = default)
        {
            maxAge = max;
            currentAge = current < 0 ? 0 : current;
            modifiers = ageModifiers;
        }
    }
    /// <summary>
    /// Represents entity size with predefined values for different size categories.
    /// </summary>


    public struct EntitySize
    {
        /// <summary>
        /// Defines standard entity size categories.
        /// </summary>
        public enum Size
        {
            Small,
            Medium,
            Large,
            Huge,
            Gargantuan
        }

        /// <summary>
        /// The size category.
        /// </summary>
        public readonly Size size;
        /// <summary>
        /// The radius of the entity.
        /// </summary>
        public readonly float radius;
        /// <summary>
        /// The height of the entity.
        /// </summary>
        public readonly float height;
        /// <summary>
        /// The weight of the entity.
        /// </summary>
        public readonly float weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySize"/> struct.
        /// </summary>
        /// <param name="size">The size category.</param>
        /// <param name="radius">The entity's radius.</param>
        /// <param name="height">The entity's height.</param>
        /// <param name="weight">The entity's weight.</param>
        public EntitySize(Size size, float radius, float height, float weight)
        {
            this.size = size;
            this.radius = radius;
            this.height = height;
            this.weight = weight;
        }

        /// <summary>
        /// Gets a predefined Small entity size.
        /// </summary>
        public static EntitySize Small => new EntitySize(Size.Small, 0.5f, 1f, 50f);
        /// <summary>
        /// Gets a predefined Medium entity size.
        /// </summary>
        public static EntitySize Medium => new EntitySize(Size.Medium, 1f, 2f, 150f);
        /// <summary>
        /// Gets a predefined Large entity size.
        /// </summary>
        public static EntitySize Large => new EntitySize(Size.Large, 1.5f, 3f, 400f);
        /// <summary>
        /// Gets a predefined Huge entity size.
        /// </summary>
        public static EntitySize Huge => new EntitySize(Size.Huge, 2f, 4f, 800f);
        /// <summary>
        /// Gets a predefined Gargantuan entity size.
        /// </summary>
        public static EntitySize Gargantuan => new EntitySize(Size.Gargantuan, 3f, 6f, 1600f);
    }

    /// <summary>
    /// Represents stat modifiers for temporary or permanent effects, including flat and percentage bonuses.
    /// </summary>
    public struct StatModifiers
    {
        /// <summary>
        /// The flat value to be added to a base stat.
        /// </summary>
        public readonly float flatModifier;
        /// <summary>
        /// The percentage value to modify a base stat (e.g., 0.1 for a 10% increase).
        /// </summary>
        public readonly float percentageModifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatModifiers"/> struct.
        /// </summary>
        /// <param name="flat">The flat modifier value.</param>
        /// <param name="percentage">The percentage modifier value (e.g., 0.1 for 10%).</param>
        public StatModifiers(float flat = 0f, float percentage = 0f)
        {
            flatModifier = flat;
            percentageModifier = percentage;
        }

        /// <summary>
        /// Applies the modifiers to a base stat value.
        /// </summary>
        /// <param name="baseValue">The base stat value to modify.</param>
        /// <returns>The modified stat value.</returns>
        public float ApplyTo(float baseValue)
        {
            return (baseValue + flatModifier) * (1f + percentageModifier);
        }

        /// <summary>
        /// Combines two StatModifiers instances.
        /// </summary>
        /// <param name="a">The first StatModifiers.</param>
        /// <param name="b">The second StatModifiers.</param>
        /// <returns>A new StatModifiers instance with combined flat and percentage modifiers.</returns>
        public static StatModifiers operator +(StatModifiers a, StatModifiers b)
        {
            return new StatModifiers(
                a.flatModifier + b.flatModifier,
                a.percentageModifier + b.percentageModifier
            );
        }
    }

    //todo: add an idea list for other utility structs like:
    // - Position (x, y, z coordinates)
    // - Rotation (pitch, yaw, roll)
    // - Scale (uniform or non-uniform scaling)
    // - Color (RGBA values)
    // - Design (visual and thematic elements)
    // - Label (Visual textual representation in game, toggleable.)
    // - Tooltip (Information display on hover or click.)
    // - StatBar (UI representation for stats like health, mana, etc.) using a sprite with a fill setting
    // - ProgressBar (UI representation for tasks or loading screens) using a sprite with a fill setting
    #endregion

#region Game Data Structures

    /// <summary>
    /// Represents data for a living entity in the game.
    /// </summary>
    public struct Entity
    {
        /// <summary>
        /// Unique identifiers for predefined entity types.
        /// </summary>        
        public enum ID
        {
            Pop = 0,
            Kaari = 1,
            Wolf = 2,
            Bear = 3,
            Goblin = 4,
            Orc = 5,
            Dragon = 6,
            Boar = 7,
            Troll = 8,
            Zombie = 9,
            Skeleton = 10,
            Vampire = 11,
            Werewolf = 12,
            Sabertooth = 13,
            Mammoth = 14,
            Phoenix = 15,
            Hydra = 16,
            Kraken = 17,
            Minotaur = 18,
            Golem = 19,
            Chimera = 20,
            Basilisk = 21,
            Centaur = 22,
            Harpy = 23,
            Siren = 24,
            Yeti = 25,
        }

        /// <summary>
        /// Defines the behavioral categories for entities.
        /// </summary>
        public enum EntityType
        {
            Boss,
            Minion,
            NPC, // Non-Playable Character
            PlayerControlled,
            Animal,
            Monster,
        }

        /// <summary>
        /// Defines the aggression levels for entities.
        /// </summary>
        public enum AggressionType
        {
            Passive,
            Neutral,
            Aggressive
        }

        // Basic Identity
        /// <summary>The display name of the entity.</summary>
        public string entityName;
        /// <summary>The unique identifier for this entity, often corresponds to <see cref="ID"/>.</summary>
        public int entityID;
        /// <summary>The faction this entity belongs to.</summary>
        public string entityFaction;
        /// <summary>A short description of the entity.</summary>
        public string entityDescription;
        /// <summary>The icon representing this entity in UI elements.</summary>
        public Sprite entityIcon;

        // Entity Properties
        /// <summary>A list of types this entity belongs to (e.g., Boss, Animal).</summary>
        public List<EntityType> entityType;
        /// <summary>The rarity level of this entity.</summary>
        public Rarity rarity;
        /// <summary>The chance of this entity spawning, as a percentage (0-100).</summary>
        public float spawnChance;
        /// <summary>The current level of this entity.</summary>
        public int level;
        /// <summary>The physical size category of this entity.</summary>
        public EntitySize entitySize;
        /// <summary>The aggression behavior of this entity.</summary>
        public AggressionType aggressionType;

        // Health and Status
        /// <summary>The current health status of this entity.</summary>
        public Health health;
        /// <summary>Indicates whether this entity uses mana or a similar resource.</summary>
        public bool usesMana;
        /// <summary>Indicates whether this entity is currently alive.</summary>
        public bool isAlive;

        // Stats
        /// <summary>The mana (or equivalent resource) stat for this entity.</summary>
        public Stat mana;
        /// <summary>The attack power stat for this entity.</summary>
        public Stat attack;
        /// <summary>The defense stat for this entity.</summary>
        public Stat defense;
        /// <summary>The movement speed stat for this entity.</summary>
        public Stat speed;

        //Todo: Additional stat management methods can be added here, such as:
        // - GetStatByID(Stat.ID id) for direct stat access
        // - ApplyStatModifier(Stat.ID id, float modifier, float duration)
        // - GetCombatPower() for calculating overall combat effectiveness
        // - RefreshAllStats() for recalculating all stat values

        /// <summary>Additional stats for comprehensive entity management.</summary>
        public Stat stamina;
        public Stat strength;
        public Stat agility;
        public Stat intelligence;
        public Stat magicPower;
        public Stat magicDefense;
        public Stat criticalHitChance;
        public Stat criticalHitDamage;
        public Stat luck;
        public Stat charisma;        /// <summary>
                                     /// Needs system stats - manage survival requirements.
                                     /// </summary>
        public Stat hunger;
        public Stat thirst;
        public Stat energy;
        public Stat rest;

        /// <summary>
        /// Experience and leveling stats.
        /// </summary>
        public Stat experience;
        public Stat levelStat;

        /// <summary>List of currently active buffs on this entity.</summary>
        public List<Buff> activeBuffs;        /// <summary>Current state of the entity for state machine behavior.</summary>
        public State currentState;
        public bool isInCombat => currentState.stateID == (int)State.ID.Attacking || currentState.stateID == (int)State.ID.Defending;
        public bool canCraft => tags.Contains("CanCraft");

        /// <summary>List of available states this entity can transition to.</summary>
        public List<State> availableStates;

        /// <summary>
        /// Applies a buff to this entity.
        /// </summary>
        /// <param name="buff">The buff to apply.</param>
        public void ApplyBuff(Buff buff)
        {
            if (activeBuffs == null) activeBuffs = new List<Buff>();
            activeBuffs.Add(buff);
            // Apply buff effects based on buff type
            switch (buff.buffID)
            {
                case (int)Buff.ID.HealthRegen:
                    health = health.Heal(buff.strength);
                    break;
                case (int)Buff.ID.ManaRegen:
                    mana.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.SpeedBoost:
                    speed.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.StrengthBoost:
                    attack.ModifyStat(buff.strength);
                    break;
                case (int)Buff.ID.DefenseBoost:
                    defense.ModifyStat(buff.strength);
                    break;
            }
        }

        /// <summary>
        /// Removes a buff from this entity.
        /// </summary>
        /// <param name="buffID">The ID of the buff to remove.</param>
        public void RemoveBuff(int buffID)
        {
            if (activeBuffs == null) return;

            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].buffID == buffID)
                {
                    // Remove buff effects
                    Buff buff = activeBuffs[i];
                    switch (buff.buffID)
                    {
                        case (int)Buff.ID.SpeedBoost:
                            speed.ModifyStat(-buff.strength);
                            break;
                        case (int)Buff.ID.StrengthBoost:
                            attack.ModifyStat(-buff.strength);
                            break;
                        case (int)Buff.ID.DefenseBoost:
                            defense.ModifyStat(-buff.strength);
                            break;
                    }
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Updates entity stats with modifiers from buffs and other sources.
        /// </summary>
        public void UpdateStats()
        {
            // Reset current values to base values
            mana.currentValue = mana.baseValue;
            attack.currentValue = attack.baseValue;
            defense.currentValue = defense.baseValue;
            speed.currentValue = speed.baseValue;

            // Apply active buff modifiers
            if (activeBuffs != null)
            {
                foreach (var buff in activeBuffs)
                {
                    switch (buff.buffID)
                    {
                        case (int)Buff.ID.SpeedBoost:
                            speed.currentValue += buff.strength;
                            break;
                        case (int)Buff.ID.StrengthBoost:
                            attack.currentValue += buff.strength;
                            break;
                        case (int)Buff.ID.DefenseBoost:
                            defense.currentValue += buff.strength;
                            break;
                    }
                }
            }
        }        /// <summary>
                 /// Heals the entity by a specified amount.
                 /// </summary>
                 /// <param name="amount">Amount of health to restore.</param>
        public void Heal(float amount)
        {
            health = health.Heal(amount);
        }        /// <summary>
                 /// Damages the entity by a specified amount.
                 /// </summary>
                 /// <param name="damage">Amount of damage to apply.</param>
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0, damage - defense.currentValue);
            health = health.TakeDamage(actualDamage);

            if (health.current <= 0)
            {
                isAlive = false;
                ChangeState(State.ID.Dead);
            }
        }        /// <summary>
                 /// Changes the entity's current state.
                 /// </summary>
                 /// <param name="newStateID">The ID of the new state.</param>
        public void ChangeState(State.ID newStateID)
        {
            if (availableStates != null)
            {
                var newState = availableStates.Find(s => s.stateID == (int)newStateID);
                if (newState.stateID == (int)newStateID) // Valid state found
                {
                    currentState = newState;
                }
            }
        }        /// <summary>
                 /// Initializes entity states based on tags and entity type.
                 /// </summary>
        public void InitializeStates()
        {
            if (availableStates == null) availableStates = new List<State>();

            // Add basic states all entities can have
            availableStates.Add(new State(State.ID.Idle, "Idle", "Entity is idle and not performing any specific action"));

            // Add states based on entity type
            if (entityType.Contains(EntityType.PlayerControlled))
            {
                availableStates.Add(new State(State.ID.Exploring, "Exploring", "Entity is exploring the environment"));
                availableStates.Add(new State(State.ID.Crafting, "Crafting", "Entity is crafting items or equipment"));
                availableStates.Add(new State(State.ID.Resting, "Resting", "Entity is resting or recovering"));
            }

            if (entityType.Contains(EntityType.Animal) || entityType.Contains(EntityType.Monster))
            {
                availableStates.Add(new State(State.ID.Patrolling, "Patrolling", "Entity is patrolling an area"));
                availableStates.Add(new State(State.ID.Hunting, "Hunting", "Entity is actively hunting for resources or prey"));
                availableStates.Add(new State(State.ID.Fleeing, "Fleeing", "Entity is fleeing from danger"));
            }

            if (aggressionType == AggressionType.Aggressive)
            {
                availableStates.Add(new State(State.ID.Attacking, "Attacking", "Entity is attacking a target"));
            }

            // Set initial state
            currentState = availableStates[0]; // Start with first available state (usually Idle)
        }

        // Game Balance
        /// <summary>A modifier affecting the value (e.g., loot drop value) of this entity.</summary>
        public int valueModifier;

        /// <summary>A modifier affecting the experience points granted by this entity.</summary>
        public int experienceModifier;

        // Utility
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
                                         /// Initializes a new instance of the <see cref="Entity"/> class with a specific name and ID.
                                         /// </summary>
                                         /// <param name="name">The name of the entity.</param>
                                         /// <param name="id">The <see cref="ID"/> of the entity.</param>
        public Entity(string name, ID id, string faction = "", string description = "", Sprite icon = null, Rarity rarity = Rarity.Common, float spawnChance = 0f, int level = 1, EntitySize? size = null, AggressionType aggressionType = AggressionType.Neutral, Health? healthValue = null, bool usesMana = false, bool isAlive = true, int valueModifier = 0, int experienceModifier = 0)
        {
            entityName = name;
            entityID = (int)id;
            entityFaction = faction;
            entityDescription = description;
            entityIcon = icon;
            this.rarity = rarity;
            this.spawnChance = spawnChance;
            this.level = level;
            entitySize = size ?? EntitySize.Medium;
            this.aggressionType = aggressionType;
            health = healthValue ?? new Health(100f);
            this.usesMana = usesMana;
            this.isAlive = isAlive;
            this.valueModifier = valueModifier;
            this.experienceModifier = experienceModifier;

            // Initialize entity type
            entityType = new List<EntityType>();

            // Initialize collections
            activeBuffs = new List<Buff>();
            availableStates = new List<State>();
            tags = new List<string>();

            // Initialize default state
            currentState = new State(State.ID.Idle, "Idle", "Entity is idle and not performing any specific action");

            mana = new Stat(Stat.ID.Mana, "Mana", 0f);
            attack = new Stat(Stat.ID.Attack, "Attack", 0f);
            defense = new Stat(Stat.ID.Defense, "Defense", 0f);
            speed = new Stat(Stat.ID.Speed, "Speed", 0f);
            stamina = new Stat(Stat.ID.Stamina, "Stamina", 100f);
            strength = new Stat(Stat.ID.Strength, "Strength", 10f);
            agility = new Stat(Stat.ID.Agility, "Agility", 10f);
            intelligence = new Stat(Stat.ID.Intelligence, "Intelligence", 10f);
            magicPower = new Stat(Stat.ID.MagicPower, "Magic Power", 0f);
            magicDefense = new Stat(Stat.ID.MagicDefense, "Magic Defense", 0f);
            criticalHitChance = new Stat(Stat.ID.CriticalHitChance, "Critical Hit Chance", 0.05f);
            criticalHitDamage = new Stat(Stat.ID.CriticalHitDamage, "Critical Hit Damage", 1.5f);
            luck = new Stat(Stat.ID.Luck, "Luck", 10f);
            charisma = new Stat(Stat.ID.Charisma, "Charisma", 10f);            // Initialize needs stats with appropriate defaults
            hunger = new Stat(Stat.ID.Hunger, "Hunger", 100f, 0f, 100f);
            thirst = new Stat(Stat.ID.Thirst, "Thirst", 100f, 0f, 100f);
            energy = new Stat(Stat.ID.Energy, "Energy", 100f, 0f, 100f);
            rest = new Stat(Stat.ID.Rest, "Rest", 100f, 0f, 100f);

            // Initialize experience and level stats
            experience = new Stat(Stat.ID.Experience, "Experience", 0f, 0f, float.MaxValue);
            levelStat = new Stat(Stat.ID.Level, "Level", level, 1f, float.MaxValue);
        }

        //Todo: Implement advanced entity behavior systems such as:
        // - State machine transitions with conditions and timers
        // - AI decision trees based on entity stats and environmental factors  
        // - Social interaction systems for entities to communicate
        // - Memory system for entities to remember past events and entities
        // - Goal-oriented action planning (GOAP) for complex behaviors

        //TODO: Make skills influence base values like speed, attack, defense, etc.

    }    /// <summary>
         /// Represents an item in the game, including its type, rarity, quality, and other properties.
         /// </summary>
    public struct Item
    {
        /// <summary>
        /// Unique identifiers for predefined item types.
        /// </summary>
        public enum ID
        {
            IronSword = 0,
            SteelAxe = 1,
            EnchantedStaff = 2,
            LeatherArmor = 10,
            ChainMail = 11,
            DragonScaleArmor = 12,
            HealthPotion = 20,
            ManaPotion = 21,
            Bread = 22,
            AncientKey = 30,
            GoldCoin = 40
        }

        /// <summary>
        /// Defines the categories of items.
        /// </summary>
        public enum ItemType
        {
            Weapon,
            Armor,
            Consumable,
            QuestItem,
            Miscellaneous
        }
        /// <summary>
        /// Defines the rarity levels specifically for items.
        /// </summary>
        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
        /// <summary>
        /// Defines the quality levels for items.
        /// </summary>
        public enum ItemQuality
        {
            Poor,
            Fair,
            Good,
            Excellent,
            Masterwork
        }
        /// <summary>
        /// Defines the equipment slots items can occupy.
        /// </summary>
        public enum ItemSlot
        {
            Head,
            Chest,
            Legs,
            Feet,
            Hands,
            Neck,
            Ring,
            Weapon,
            Offhand,
            Trinket
        }

        /// <summary>The display name of the item.</summary>
        public string itemName;
        /// <summary>The unique identifier for this item.</summary>
        public int itemID;
        /// <summary>The type or category of this item.</summary>
        public ItemType itemType;
        /// <summary>The weight of the item.</summary>
        public float weight;
        /// <summary>The quantity of this item (if stackable).</summary>
        public int quantity;
        /// <summary>The monetary value of the item.</summary>
        public int value;
        /// <summary>The rarity level of this item.</summary>
        public ItemRarity itemRarity;
        /// <summary>The quality level of this item.</summary>
        public ItemQuality itemQuality;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
                                         /// Initializes a new instance of the <see cref="Item"/> struct.
                                         /// </summary>
                                         /// <param name="name">The name of the item.</param>
                                         /// <param name="id">The unique ID of the item.</param>
                                         /// <param name="type">The type of the item.</param>
                                         /// <param name="weight">The weight of the item.</param>
                                         /// <param name="quantity">The quantity of the item.</param>
                                         /// <param name="value">The value of the item.</param>
                                         /// <param name="rarity">The rarity of the item.</param>
                                         /// <param name="quality">The quality of the item.</param>
        public Item(string name, ID id, ItemType type, float weight = 1f, int quantity = 1, int value = 10, ItemRarity rarity = ItemRarity.Common, ItemQuality quality = ItemQuality.Fair)
        {
            itemName = name;
            itemID = (int)id;
            itemType = type;
            this.weight = weight;
            this.quantity = quantity;
            this.value = value;
            itemRarity = rarity;
            itemQuality = quality;
            tags = new List<string>();
        }
    }

    public struct State
    {
        /// <summary>
        /// Unique identifiers for predefined state types.
        /// </summary>
        public enum ID
        {
            Idle = 0,
            Attacking = 1,
            Defending = 2,
            Fleeing = 3,
            Searching = 4, // Represents a state where the entity is searching for something
            Resting = 5, // Represents a state where the entity is resting or recovering (includes sleeping, meditation, or idle recovery)
            Patrolling = 6, // Represents a state where the entity is patrolling an area
            Interacting = 7, // Represents a state where the entity is interacting with an object or another entity
            Hauling = 8, // Represents a state where the entity is hauling or transporting items
            Gathering = 9, // Represents a state where the entity is gathering resources or items
            Hiding = 10, // Represents a state where the entity is hiding or camouflaging itself
            Socializing = 11,   // Represents a state where the entity is interacting with other entities socially
            Crafting = 12, // Represents a state where the entity is crafting items or equipment
            Healing = 13, // Represents a state where the entity is healing itself or others
            Exploring = 14, // Represents a state where the entity is exploring the environment
            Hunting = 15, // Represents a state where the entity is actively hunting for resources or prey
            Playing = 16, // Represents a state where the entity is engaged in play activities
            Fishing = 17, // Represents a state where the entity is engaged in fishing activities
            Farming = 18, // Represents a state where the entity is engaged in farming activities
            Sleeping = 21, // Represents a state where the entity is specifically sleeping
            // Special states
            Dead = 19, // Represents a state where the entity is dead
            Unconscious = 20, // Represents a state where the entity is incapacitated but not dead
        }

        /// <summary>
        /// The unique identifier for this state, often corresponds to <see cref="ID"/>.
        /// </summary>
        public int stateID;
        /// <summary>The display name of the state.</summary>
        public string stateName;        /// <summary>A short description of the state.</summary>
        public string stateDescription;

        /// <summary>Duration this state has been active (in seconds).</summary>
        public float stateDuration;

        /// <summary>Priority level for state transitions (higher values = higher priority).</summary>
        public int priority;

        /// <summary>Whether this state can be interrupted by other states.</summary>
        public bool canBeInterrupted;

        /// <summary>Energy cost per second while in this state.</summary>
        public float energyCostPerSecond;

        /// <summary>
        /// Determines the logic of the state and what occurs when the state is active.
        /// This method should be called regularly to update state behavior.
        /// </summary>
        public void UpdateState(Entity entity)
        {
            stateDuration += Time.deltaTime;
            
            // Apply energy cost
            if (energyCostPerSecond > 0 && entity.stamina.currentValue > 0)
            {
                entity.stamina.ModifyStat(-energyCostPerSecond * Time.deltaTime);
            }

            // State-specific logic can be added here or handled by external state machine
        }

        /// <summary>
        /// Called when entering this state.
        /// </summary>
        public void OnEnterState(Entity entity)
        {
            stateDuration = 0f;
            // State-specific entry logic can be added here
        }

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        public void OnExitState(Entity entity)
        {
            // State-specific exit logic can be added here
        }        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the state.</param>
        /// <param name="name">The name of the state.</param>
        /// <param name="description">The description of the state.</param>
        /// <param name="priority">Priority level for state transitions (default: 1).</param>
        /// <param name="canBeInterrupted">Whether this state can be interrupted (default: true).</param>
        /// <param name="energyCostPerSecond">Energy cost per second while in this state (default: 0).</param>
        public State(ID id, string name, string description, int priority = 1, bool canBeInterrupted = true, float energyCostPerSecond = 0f)
        {
            stateID = (int)id;
            stateName = name;
            stateDescription = description;
            stateDuration = 0f;
            this.priority = priority;
            this.canBeInterrupted = canBeInterrupted;
            this.energyCostPerSecond = energyCostPerSecond;
        }

        /// <summary>
        /// Checks if this state can transition to another state based on priority and interruption rules.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        /// <returns>True if transition is allowed, false otherwise.</returns>
        public bool CanTransitionTo(State newState)
        {
            // Cannot transition to same state
            if (stateID == newState.stateID) return false;
            
            // Can always transition if current state can be interrupted
            if (canBeInterrupted) return true;
            
            // Can transition if new state has higher priority
            return newState.priority > priority;
        }

        //Todo: Transition the state logic from other scripts to pull from this data.
        // This state system provides a foundation for AI behavior management.
        // External scripts should use Entity.ChangeState() method to change states
        // and can query Entity.currentState for current behavior information.
    }
    /// <summary>
    /// Represents a buff or debuff effect that can be applied to entities.
    /// </summary>
    public struct Buff
    {
        /// <summary>
        /// Unique identifiers for predefined buff types.
        /// </summary>
        public enum ID
        {
            HealthRegen = 0,
            ManaRegen = 1,
            SpeedBoost = 2,
            StrengthBoost = 3,
            DefenseBoost = 4,
            CriticalHitChanceBoost = 5,
            CriticalHitDamageBoost = 6,
            ExperienceBoost = 7,
            LuckBoost = 8
        }
        /// <summary>
        /// Defines the nature of the buff (e.g., temporary, permanent, debuff).
        /// </summary>
        public enum BuffType
        {
            Temporary,
            Permanent,
            Debuff
        }

        /// <summary>The unique identifier for this buff, often corresponds to <see cref="ID"/>.</summary>
        public int buffID;
        /// <summary>The display name of the buff.</summary>
        public string buffName;
        /// <summary>A short description of the buff's effects.</summary>
        public string buffDescription;
        /// <summary>The type of the buff (e.g., temporary, permanent).</summary>
        public BuffType buffType;
        /// <summary>The duration of the buff in seconds. A value of 0 typically indicates a permanent buff.</summary>
        public float duration; // 0 for permanent buffs
        /// <summary>The magnitude or strength of the buff's effect.</summary>
        public float strength;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buff"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the buff.</param>
        /// <param name="name">The name of the buff.</param>
        /// <param name="description">The description of the buff.</param>
        /// <param name="type">The type of the buff.</param>
        /// <param name="strength">The strength/magnitude of the buff.</param>
        /// <param name="duration">The duration of the buff in seconds (0 for permanent).</param>
        public Buff(ID id, string name, string description, BuffType type, float strength, float duration = 0f)
        {
            buffID = (int)id;
            buffName = name;
            buffDescription = description;
            buffType = type;
            this.strength = strength;
            this.duration = duration;
            tags = new List<string>();
        }
    }

    /// <summary>
    /// Represents a specific statistic for an entity, such as Health, Mana, or Attack
    /// </summary>
    public struct Stat
    {
        /// <summary>
        /// Unique identifiers for predefined stat types.
        /// </summary>
        public enum ID
        {
            Health = 0,
            Mana = 1,
            Stamina = 2,
            Strength = 3,
            Agility = 4,
            Intelligence = 5,
            Defense = 6,
            Speed = 7,
            CriticalHitChance = 8,
            CriticalHitDamage = 9,
            Attack = 10,
            MagicPower = 11,
            MagicDefense = 12,
            Experience = 13,
            Level = 14,
            Luck = 15,
            Charisma = 16,
            // Needs System Stats
            Hunger = 17,
            Thirst = 18,
            Energy = 19,
            Rest = 20
        }

        /// <summary>
        /// Defines the category of a stat (e.g., primary, secondary).
        /// </summary>
        public enum StatType
        {
            Primary, // Core stats like Health, Mana, etc.
            Secondary, // Derived stats like Attack, Defense, etc.
            Tertiary // Miscellaneous stats like Experience, Level, etc.
        }

        /// <summary>The unique identifier for this stat, often corresponds to <see cref="ID"/>.</summary>
        public ID statID;
        /// <summary>The display name of the stat.</summary>
        public string statName;
        /// <summary>A short description of the stat.</summary>
        public string statDescription;
        /// <summary>The type or category of this stat.</summary>
        public StatType statType;
        /// <summary>The base value of the stat before any modifiers.</summary>
        public float baseValue;
        /// <summary>The current value of the stat after modifiers.</summary>
        public float currentValue;
        /// <summary>The minimum possible value for this stat.</summary>
        public float minValue;
        /// <summary>The maximum possible value for this stat.</summary>
        public float maxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stat"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ID"/> of the stat.</param>
        /// <param name="name">The name of the stat.</param>
        /// <param name="baseVal">The base value of the stat.</param>
        /// <param name="minVal">The minimum value of the stat.</param>
        /// <param name="maxVal">The maximum value of the stat.</param>
        /// <param name="type">The type of the stat.</param>
        /// <param name="description">The description of the stat.</param>
        public Stat(ID id, string name, float baseVal = 0f, float minVal = 0f, float maxVal = 100f, StatType type = StatType.Primary, string description = "")
        {
            statID = id;
            statName = name;
            statDescription = description;
            statType = type;
            baseValue = baseVal;
            currentValue = baseVal;
            minValue = minVal;
            maxValue = maxVal;
        }

        /// <summary>
        /// Gets the current value of the stat as a percentage of its maximum value.
        /// </summary>
        /// <returns>The current value as a percentage (0.0 to 1.0), or 0 if max value is 0.</returns>
        public float GetPercentage()
        {
            return maxValue > 0 ? currentValue / maxValue : 0f;
        }

        /// <summary>
        /// Modifies the current value of the stat by a given amount, clamped by min/max values.
        /// </summary>
        /// <param name="amount">The amount to add to the current value (can be negative).</param>
        public void ModifyStat(float amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        /// <summary>
        /// Sets the current value of the stat, clamped by min/max values.
        /// </summary>
        /// <param name="value">The new value for the stat.</param>
        public void SetCurrentValue(float value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
        }
        

    }

    /// <summary>
    /// Represents a skill in the game. Some can be influenced by the Entities stats, items, and buffs.
    /// Skills can be used to perform actions, craft items, or gain experience.
    /// </summary>
    public struct Skill
    {
        /// <summary>
        /// Unique identifiers for predefined skill types.
        /// </summary>        
        public enum ID
        {
            Combat = 0,
            Crafting = 1,
            Gathering = 2,
            Social = 3,
            Magic = 4,
            Exploration = 5,
            Survival = 6,
            Stealth = 7,
            Engineering = 8,
            Alchemy = 9
        }

        /// <summary>
        /// Defines the categories of skills.
        /// </summary>
        public enum SkillType
        {
            Combat,
            Crafting,
            Gathering,
            Social,
            Magic,
            Exploration,
            Survival,
            Stealth,
            Engineering,
            Alchemy
        }

        /// <summary>The unique identifier for this skill, often corresponds to <see cref="ID"/>.</summary>
        public ID skillID;
        /// <summary>The name of the skill, often corresponding to its <see cref="SkillType"/>.</summary>
        public SkillType skillName;
        /// <summary>The type or category of this skill.</summary>
        public SkillType skillType;
        /// <summary>The current experience points for this skill.</summary>
        public float experience;
        /// <summary>The current level of this skill.</summary>
        public int level;
        /// <summary>A list of tags for categorization or special mechanics.</summary>
        public List<string> tags;        /// <summary>
                                         /// Initializes a new instance of the <see cref="Skill"/> struct.
                                         /// </summary>
                                         /// <param name="id">The <see cref="ID"/> of the skill.</param>
                                         /// <param name="type">The <see cref="SkillType"/> of the skill.</param>
                                         /// <param name="initialExperience">The initial experience points for the skill.</param>
                                         /// <param name="initialLevel">The initial level of the skill.</param>
        public Skill(ID id, SkillType type, float initialExperience = 0f, int initialLevel = 1)
        {
            skillID = id;
            skillName = type;
            skillType = type;
            experience = initialExperience;
            level = initialLevel;
            tags = new List<string>();
        }

        /// <summary>
        /// Adds experience to this skill and calculates level progression.
        /// </summary>
        /// <param name="expPoints">The amount of experience to add.</param>
        /// <returns>True if the skill leveled up, false otherwise.</returns>
        public bool AddExperience(float expPoints)
        {
            int oldLevel = level;
            experience += expPoints;
            // Simple leveling formula: every 100 exp = 1 level
            int newLevel = Mathf.FloorToInt(experience / 100f) + 1;
            level = Mathf.Max(1, newLevel);
            return level > oldLevel;
        }
    }

    /// <summary>
    /// A leveling system that uses experience to determine the level of a trait.
    /// </summary>

    public struct LevelingSystem
    {
        /// <summary>
        /// The current level of the trait.
        /// </summary>
        public int currentLevel;
        /// <summary>
        /// The experience points required to reach the next level.
        /// </summary>
        public int experienceToNextLevel;
        public int maxLevel; // Optional: Maximum level this system can reach
        public int experiencePerLevel; // Optional: Experience required per level, can be adjusted for balance
        public StatModifiers modifiers; // Optional modifiers for leveling effects
        public List<Trait> traitRewards; // Optional list of traits that can be rewarded at certain levels
        public List<Skill> skillRewards; // Optional list of skills that can be rewarded at certain levels
        public List<Item> itemRewards; // Optional list of items that can be rewarded at certain levels

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelingSystem"/> struct.
        /// </summary>
        /// <param name="level">The initial level.</param>
        /// <param name="experience">The initial experience points required for the next level.</param>
        public LevelingSystem(int level, int experience)
        {            currentLevel = level;
            experienceToNextLevel = experience;
            maxLevel = 100; // Default max level
            experiencePerLevel = 100; // Default experience per level
            modifiers = new StatModifiers(); // Default empty modifiers
            traitRewards = new List<Trait>(); // Empty list
            skillRewards = new List<Skill>(); // Empty list
            itemRewards = new List<Item>(); // Empty list
        }

        /// <summary>
        /// Adds experience and levels up if threshold is reached.
        /// </summary>
        /// <param name="expPoints">Experience points to add.</param>
        /// <returns>True if leveled up, false otherwise.</returns>
        public bool AddExperience(int expPoints)
        {
            int oldLevel = currentLevel;
            experienceToNextLevel -= expPoints;
            
            while (experienceToNextLevel <= 0 && currentLevel < maxLevel)
            {
                currentLevel++;
                experienceToNextLevel += experiencePerLevel * currentLevel; // Scaling difficulty
            }
            
            return currentLevel > oldLevel;
        }

        /// <summary>
        /// Checks if enough experience exists to reach the next level.
        /// </summary>
        /// <returns>True if ready to level up, false otherwise.</returns>
        public bool CanLevelUp()
        {
            return experienceToNextLevel <= 0 && currentLevel < maxLevel;
        }

        /// <summary>
        /// Gets the total experience invested in this leveling system.
        /// </summary>
        /// <returns>Total experience spent.</returns>
        public int GetTotalExperience()
        {
            int totalExp = 0;
            for (int i = 1; i < currentLevel; i++)
            {
                totalExp += experiencePerLevel * i;
            }
            return totalExp + (experiencePerLevel * currentLevel - experienceToNextLevel);
        }

        /// <summary>
        /// Gets the percentage progress toward the next level.
        /// </summary>
        /// <returns>Progress percentage (0.0 to 1.0).</returns>
        public float GetProgressToNextLevel()
        {
            int currentLevelExp = experiencePerLevel * currentLevel;
            int earnedExp = currentLevelExp - experienceToNextLevel;
            return currentLevelExp > 0 ? (float)earnedExp / currentLevelExp : 1f;
        }

        //Todo: Consider renaming to ExperienceSystem or ProgressionSystem for clarity.
    }
    public struct Trait
    {
        /// <summary>
        /// The unique identifier for this trait.
        /// </summary>
        public enum ID
        {
            Brave = 0,
            Cunning = 1,
            Strong = 2,
            Agile = 3,
            Wise = 4,
            Charismatic = 5,
            Resilient = 6,
            Stealthy = 7,
            Lucky = 8,
            Fearless = 9,
            Compassionate = 10,
            Honorable = 11,
            Curious = 12,
            Patient = 13,
            Resourceful = 14,
            Determined = 15,
            Creative = 16,
            Loyal = 17,
            Optimistic = 18,
            Pessimistic = 19,
            Skeptical = 20,
            Adventurous = 21,
            Sneaky = 22,
            QuickWitted = 23,
            Empathetic = 24,
            Clumbsy = 25,
            Perceptive = 26,
            Intuitive = 27,
            Analytical = 28,
            Quick = 29,
            Daring = 30,
            Fearful = 31,
            Impulsive = 32,
            Naive = 33,
            Arrogant = 34,
            QuickCrafter = 35,
            Masterful = 36,
            LuckyCrafter = 37,
            Efficient = 38,
            ResourcefulCrafter = 39,
            Strategic = 40,
            Diplomatic = 41,

        }

        /// <summary>
        /// The name of the trait.
        /// </summary>
        public readonly string traitName;
        /// <summary>
        /// A brief description of the trait.
        /// </summary>
        public readonly string description;
        /// <summary>
        /// The category of the trait (e.g., Combat, Social, etc.).
        /// </summary>
        public readonly string category;

        public ID traitID; // The unique identifier for this trait, often corresponds to <see cref="ID"/>
        public StatModifiers modifiers; // Optional modifiers for trait effects
        public List<string> tags; // Optional tags for categorization or special mechanics
        public List<Trait> requiredTraits; // Optional reference to a required trait for this trait to be applicable
        public Skill.ID requiredSkill; // Optional reference to a required skill for this trait to be applicable
        public List<Item.ID> requiredItems; // Optional reference to a list of required items for this trait to be applicable
        public Stat requiredStat; // Optional reference to a required stat for this trait to be applicable        /// <summary>
        /// Initializes a new instance of the <see cref="Trait"/> struct.
        /// </summary>
        /// <param name="id">The unique identifier for the trait.</param>
        /// <param name="name">The name of the trait.</param>
        /// <param name="description">A brief description of the trait.</param>
        /// <param name="category">The category of the trait.</param>
        public Trait(ID id, string name, string description, string category)
        {
            traitID = id;
            traitName = name;
            this.description = description;
            this.category = category;
            modifiers = new StatModifiers();
            tags = new List<string>();
            requiredTraits = new List<Trait>();
            requiredSkill = Skill.ID.Combat; // Default, can be changed
            requiredItems = new List<Item.ID>();
            requiredStat = new Stat(Stat.ID.Strength, "Default"); // Default, can be changed
        }
    }

    /// <summary>
    /// Represents a quest or mission that can be assigned to entities.
    /// </summary>
    public struct Quest
    {
        /// <summary>
        /// The unique identifiers for predefined or hardcoded quests. ID is usually the name of the quest.
        /// This can be used to reference specific quests in the game.
        /// </summary>
        public enum ID
        {
            GatherQuest_IGotBerries = 0, //IDEA: One of the first quests, where the pops are tasked with gathering berries for the lineage
            DefendQuest_UnderAttack = 1, //IDEA: A quest to defend the village from an incoming attack, possibly from goblins or bandits.
            HuntingQuest_FromHowlToYelp = 2, //IDEA: A quest to hunt a pack of wolves nearby
            KnowledgeQuest_TheMarkOfTheBeast = 3, //Possibly a quest when a pop is inflicted with a werewolf curse.


        }
        public enum Type
        {
            GatherResources = 0, // Represents quests that involve gathering resources or items
            DefendTerritory = 1, // Represents quests that involve defending a location or entity from threats
            ExploreArea = 2, // Represents quests that involve exploring new areas or dungeons
            CraftItems = 3, // Represents quests that involve crafting items or equipment
            SocialInteraction = 4, // Represents quests that involve interacting with NPCs or other entities
            KillTarget = 5, // Represents quests that involve hunting creatures or animals
            Knowledge = 6, // Represents quests that involve learning knowledge or discovering lore.
            EscortNPC = 7, // Represents quests that involve escorting an NPC to a destination
            RescueMission = 8, // Represents quests that involve rescuing an NPC or entity from danger
            Diplomatic = 9, // Represents quests that involve diplomatic negotiations or interactions
            Investigation = 10, // Represents quests that involve investigating a mystery or event
            Genetics = 11, // Represents quests that involve genetic research or experimentation
        }

        public enum Status
        {
            NotStarted = 0,
            InProgress = 1,
            Completed = 2,
            Failed = 3
        }

        public ID questID;
        public string questName;
        public string description;
        public Status status;
        public List<Objective> objectives;
        public List<Item> rewards;
        public int experienceReward;
        public int questCompletionPercentage; // Optional: Percentage of quest completion, useful for tracking progress
        public Type questType; // Optional: Type of the quest, useful for categorization or filtering

        public Quest(ID id, string name, string description)
        {
            questID = id;
            questName = name;
            this.description = description;
            status = Status.NotStarted;
            objectives = new List<Objective>(); ///todo: make a struct called Objectives and create objectives that can be added to quests that holds the name, description, and completion status.
            rewards = new List<Item>(); ///todo: add a tag to this item (if not currency) called "objectiveReward" to indicate that this item is a reward for completing the quest and show what quest it was rewarded for.
            experienceReward = 0;
            questCompletionPercentage = 0;
            questType = Type.GatherResources; // Default quest type
        }
    }

    public struct Objective
    {
        public enum ID
        {
            Collect = 0,
            Defeat = 1,
            Explore = 2,
            Craft = 3,
            Interact = 4,
            TalkToNPC = 5
        }

        public ID objectiveID;
        public string objectiveName;
        public string description;
        public bool isCompleted;
        public List<Item> objectiveReward; // Optional reward item for completing this objective
        public Quest quest; // Optional reference to the quest this objective belongs to
        public List<string> tags; // Optional tags for categorization or special mechanics
        public List<NPC> relatedNPCs; // Optional list of NPCs related to this objective
        public enum Difficulty{
            Easy = 0,
            Medium = 1,
            Hard = 2,
            Expert = 3
        }
        public Stat experienceRewardLineage; // Optional experience reward for completing this objective
            //TODO: Add a class for the Lineage, and the experience will be added to the lineage of the entity that completes this objective.
        public Stat experienceRewardPersonal; // Optional personal experience reward for if an individual pop completes this objective

        public Difficulty difficultyLevel; // Optional difficulty level associated with this objective
        public Objective(ID id, string name, string desc)
        {
            objectiveID = id;
            objectiveName = name;
            description = desc;
            isCompleted = false;
            objectiveReward = new List<Item>();
            quest = default(Quest);
            tags = new List<string>();
            relatedNPCs = new List<NPC>();
            experienceRewardLineage = new Stat(Stat.ID.Experience, "Lineage Experience", 0f);
            experienceRewardPersonal = new Stat(Stat.ID.Experience, "Personal Experience", 0f);
            difficultyLevel = Difficulty.Easy;
        }
    }
    /// <summary>
    /// Represents genetic information for hereditary traits.
    /// </summary>
    public struct Genetics
    {
        public enum GeneType
        {
            Stat_Modifier = 0, // Adds a bonus to a specific stat
            Acquire_Trait = 1, // Represents a specific trait
            Skill_Multiplier = 2, // Adds a boost to increasing a specific skill
            Appearance = 3, // Represents physical appearance traits
            Behavior = 4, // Represents behavioral traits
            Mutation = 5, // Represents a mutation that can affect stats or abilities.
            /// todo: Add more gene types as needed for future expansion.
        }

        public GeneType geneType;
        public float dominantValue;
        public float recessiveValue;
        public bool isDominant;

        public Genetics(GeneType type, float dominant, float recessive, bool isDominantExpressed = true)
        {
            geneType = type;
            dominantValue = dominant;
            recessiveValue = recessive;
            isDominant = isDominantExpressed;
        }

        public float GetExpressedValue()
        {
            return isDominant ? dominantValue : recessiveValue;
        }

        /// TODO: Research and implement basic genetic inheritance patterns.
        /// This could include Mendelian inheritance, polygenic traits, and mutations.

        /// TODO: Design a system for genetic mutations that can occur during gameplay.
        /// This could involve random mutations that affect stats, traits, or abilities.
    }

    /// <summary>
    /// Represents an NPC with behavior patterns and relationships.
    /// </summary>
    public struct NPC
    {
        public enum Archetype
        {
            Trader = 0,
            Warrior = 1,
            Scholar = 2,
            Healer = 3,
            Guide = 4,
            Hermit = 5
        }

        public string npcName;
        public Archetype archetype;
        public Entity entityData;
        public List<string> dialogueKeys;
        public Dictionary<string, int> relationships; // NPC name -> relationship value
        public List<Quest> availableQuests;

        public NPC(string name, Archetype type, Entity data)
        {
            npcName = name;
            archetype = type;
            entityData = data;
            dialogueKeys = new List<string>();
            relationships = new Dictionary<string, int>();
            availableQuests = new List<Quest>();
        }
    }

    /// <summary>
    /// Represents lore entries for world-building and immersion.
    /// </summary>
    public struct LoreEntry
    {
        public enum Category
        {
            History = 0,
            Legend = 1,
            Technology = 2,
            Culture = 3,
            Geography = 4,
            Bestiary = 5
        }

        public string title;
        public Category category;
        public string content;
        public bool isDiscovered;
        public List<string> relatedEntries;

        public LoreEntry(string title, Category category, string content)
        {
            this.title = title;
            this.category = category;
            this.content = content;
            isDiscovered = false;
            relatedEntries = new List<string>();
        }
    }

    /// <summary>
    /// Represents a journal entry for tracking events and discoveries.
    /// </summary>
    public struct JournalEntry
    {
        public enum EntryType
        {
            Discovery = 0,
            Event = 1,
            Quest = 2,
            Encounter = 3,
            Achievement = 4
        }

        public string title;
        public EntryType type;
        public string content;
        public DateTime timestamp;
        public bool isImportant;

        public JournalEntry(string title, EntryType type, string content, bool important = false)
        {
            this.title = title;
            this.type = type;
            this.content = content;
            timestamp = DateTime.Now;
            isImportant = important;
        }
    }

    // Todo: Consider adding Dialogue system, Weather system, Economy system, and Territory management structures.
    #endregion
}
