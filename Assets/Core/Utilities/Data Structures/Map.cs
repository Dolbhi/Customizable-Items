using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Custom serializable dictionary
/// Refresh must be called on keys and value change
/// </summary>
[Serializable]
public class Map<TKey, TValue>
{
    // keys
    [SerializeField]
    List<TKey> keysList = new List<TKey>();
    public List<TKey> KeysList
    {
        get { return keysList; }
        set { keysList = value; }
    }
    // values
    [SerializeField]
    List<TValue> valuesList = new List<TValue>();
    public List<TValue> ValuesList
    {
        get { return valuesList; }
        set { valuesList = value; }
    }

    Dictionary<TKey, TValue> dictionaryData = new Dictionary<TKey, TValue>();
    public Dictionary<TKey, TValue> DictionaryData
    {
        get { return dictionaryData; }
        set { dictionaryData = value; }
    }

    public void Refresh()
    {
        try
        {
            dictionaryData.Clear();

            for (int i = 0; i < keysList.Count; i++)
            {
                dictionaryData.Add(keysList[i], valuesList[i]);
            }

        }
        catch (Exception)
        {
            Debug.LogError("KeysList.Count is not equal to ValuesList.Count. It shouldn't happen!");
        }

    }

    public void Add(TKey key, TValue data)
    {
        dictionaryData.Add(key, data);
        keysList.Add(key);
        valuesList.Add(data);
    }

    public void Remove(TKey key)
    {
        valuesList.Remove(dictionaryData[key]);
        keysList.Remove(key);
        dictionaryData.Remove(key);

    }

    public bool ContainsKey(TKey key)
    {
        return DictionaryData.ContainsKey(key);
    }

    public bool ContainsValue(TValue data)
    {
        return DictionaryData.ContainsValue(data);
    }

    public void Clear()
    {
        DictionaryData.Clear();
        keysList.Clear();
        valuesList.Clear();
    }
}