using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lineage.Core.Crafting;
using Lineage.Core.Entities;
using Lineage.Core.Items;

namespace Lineage.Core
{
    /// <summary>
    /// Interface for accessing game data definitions.
    /// </summary>
    public interface IGameDataProvider
    {
        T GetDefinition<T>(string id) where T : GameDataSO;
        EntityDefinitionSO GetEntityDefinition(string id);
        ItemDefinitionSO GetItemDefinition(string id);
        RecipeDefinitionSO GetRecipeDefinition(string id);
        Tag_SO GetTagDefinition(string tagName);
        List<T> GetAllDefinitionsOfType<T>() where T : GameDataSO;
        List<T> GetDefinitionsWithTag<T>(Tag_SO tag) where T : GameDataSO;
        List<T> GetDefinitionsWithTags<T>(List<Tag_SO> tags, bool matchAll = true) where T : GameDataSO;
    }

    /// <summary>
    /// Singleton responsible for loading and providing access to game data definitions.
    /// </summary>
    public class GameDataManager : MonoBehaviour, IGameDataProvider
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameDataManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameDataManager");
                        _instance = go.AddComponent<GameDataManager>();
                        DontDestroyOnLoad(go);
                        _instance.LoadAllData();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, GameDataSO> _dataByID = new Dictionary<string, GameDataSO>();
        private Dictionary<string, Tag_SO> _tagsByName = new Dictionary<string, Tag_SO>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads all GameData and Tag ScriptableObjects from Resources. Logs errors for duplicates or missing data.
        /// </summary>
        private void LoadAllData()
        {
            _dataByID.Clear();
            _tagsByName.Clear();

            var allData = Resources.LoadAll<GameDataSO>("GameData");
            foreach (var data in allData)
            {
                if (data == null || string.IsNullOrEmpty(data.uniqueID))
                {
                    Lineage.Debug.Log.Error($"[GameDataManager] Null or missing uniqueID in GameDataSO asset.", Lineage.Debug.Log.LogCategory.Systems, this);
                    continue;
                }
                if (!_dataByID.ContainsKey(data.uniqueID))
                    _dataByID.Add(data.uniqueID, data);
                else
                    Lineage.Debug.Log.Error($"[GameDataManager] Duplicate GameData uniqueID '{data.uniqueID}' detected.", Lineage.Debug.Log.LogCategory.Systems, this);
            }

            var allTags = Resources.LoadAll<Tag_SO>("GameData");
            foreach (var tag in allTags)
            {
                if (tag == null || string.IsNullOrEmpty(tag.tagName))
                {
                    Lineage.Debug.Log.Error($"[GameDataManager] Null or missing tagName in Tag_SO asset.", Lineage.Debug.Log.LogCategory.Systems, this);
                    continue;
                }
                if (!_tagsByName.ContainsKey(tag.tagName))
                    _tagsByName.Add(tag.tagName, tag);
            }
        }

        /// <inheritdoc/>
        public T GetDefinition<T>(string id) where T : GameDataSO
        {
            if (string.IsNullOrEmpty(id))
            {
                Lineage.Debug.Log.Error($"[GameDataManager] GetDefinition<{typeof(T).Name}> called with null or empty id.", Lineage.Debug.Log.LogCategory.Systems, this);
                return null;
            }
            if (_dataByID.TryGetValue(id, out var data))
            {
                if (data is T typed)
                    return typed;
                Lineage.Debug.Log.Error($"[GameDataManager] Data with id '{id}' is not of type {typeof(T).Name}.", Lineage.Debug.Log.LogCategory.Systems, this);
                return null;
            }
            Lineage.Debug.Log.Warning($"[GameDataManager] No GameData found for id '{id}'.", Lineage.Debug.Log.LogCategory.Systems, this);
            return null;
        }

        /// <inheritdoc/>
        public EntityDefinitionSO GetEntityDefinition(string id) => GetDefinition<EntityDefinitionSO>(id);
        /// <inheritdoc/>
        public ItemDefinitionSO GetItemDefinition(string id) => GetDefinition<ItemDefinitionSO>(id);
        /// <inheritdoc/>
        public RecipeDefinitionSO GetRecipeDefinition(string id) => GetDefinition<RecipeDefinitionSO>(id);

        /// <inheritdoc/>
        public Tag_SO GetTagDefinition(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                Lineage.Debug.Log.Error($"[GameDataManager] GetTagDefinition called with null or empty tagName.", Lineage.Debug.Log.LogCategory.Systems, this);
                return null;
            }
            if (_tagsByName.TryGetValue(tagName, out var tag))
            {
                return tag;
            }
            Lineage.Debug.Log.Warning($"[GameDataManager] No Tag_SO found for tagName '{tagName}'.", Lineage.Debug.Log.LogCategory.Systems, this);
            return null;
        }

        /// <inheritdoc/>
        public List<T> GetAllDefinitionsOfType<T>() where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().ToList();
        }

        /// <inheritdoc/>
        public List<T> GetDefinitionsWithTag<T>(Tag_SO tag) where T : GameDataSO
        {
            if (tag == null)
            {
                Lineage.Debug.Log.Error($"[GameDataManager] GetDefinitionsWithTag<{typeof(T).Name}> called with null tag.", Lineage.Debug.Log.LogCategory.Systems, this);
                return new List<T>();
            }
            return _dataByID.Values.OfType<T>().Where(d => d.tags.Contains(tag)).ToList();
        }

        /// <inheritdoc/>
        public List<T> GetDefinitionsWithTags<T>(List<Tag_SO> tags, bool matchAll = true) where T : GameDataSO
        {
            if (tags == null || tags.Count == 0)
            {
                Lineage.Debug.Log.Error($"[GameDataManager] GetDefinitionsWithTags<{typeof(T).Name}> called with null or empty tags list.", Lineage.Debug.Log.LogCategory.Systems, this);
                return new List<T>();
            }
            return _dataByID.Values.OfType<T>().Where(d =>
                matchAll ? tags.All(t => d.tags.Contains(t)) : tags.Any(t => d.tags.Contains(t))
            ).ToList();
        }
    }
}
