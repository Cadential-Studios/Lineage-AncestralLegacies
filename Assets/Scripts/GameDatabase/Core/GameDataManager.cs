using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Core
{
    /// <summary>
    /// Singleton responsible for loading and providing access to game data definitions.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameDataManager>();
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

        private void LoadAllData()
        {
            _dataByID.Clear();
            _tagsByName.Clear();

            var allData = Resources.LoadAll<GameDataSO>("GameData");
            foreach (var data in allData)
            {
                if (data != null && !string.IsNullOrEmpty(data.uniqueID))
                {
                    if (!_dataByID.ContainsKey(data.uniqueID))
                        _dataByID.Add(data.uniqueID, data);
                    else
                        Debug.LogWarning($"Duplicate GameData uniqueID {data.uniqueID} detected.");
                }
            }

            var allTags = Resources.LoadAll<Tag_SO>("GameData");
            foreach (var tag in allTags)
            {
                if (tag != null && !string.IsNullOrEmpty(tag.tagName))
                {
                    if (!_tagsByName.ContainsKey(tag.tagName))
                        _tagsByName.Add(tag.tagName, tag);
                }
            }
        }

        public T GetDefinition<T>(string id) where T : GameDataSO
        {
            if (_dataByID.TryGetValue(id, out var data))
            {
                return data as T;
            }
            return null;
        }

        public EntityDefinitionSO GetEntityDefinition(string id) => GetDefinition<EntityDefinitionSO>(id);
        public ItemDefinitionSO GetItemDefinition(string id) => GetDefinition<ItemDefinitionSO>(id);
        public RecipeDefinitionSO GetRecipeDefinition(string id) => GetDefinition<RecipeDefinitionSO>(id);

        public Tag_SO GetTagDefinition(string tagName)
        {
            if (_tagsByName.TryGetValue(tagName, out var tag))
            {
                return tag;
            }
            return null;
        }

        public List<T> GetAllDefinitionsOfType<T>() where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().ToList();
        }

        public List<T> GetDefinitionsWithTag<T>(Tag_SO tag) where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().Where(d => d.tags.Contains(tag)).ToList();
        }

        public List<T> GetDefinitionsWithTags<T>(List<Tag_SO> tags, bool matchAll = true) where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().Where(d =>
                matchAll ? tags.All(t => d.tags.Contains(t)) : tags.Any(t => d.tags.Contains(t))
            ).ToList();
        }
    }
}
