using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SerializableDict<K,V> {
    [SerializeField]
    SerializableDictItem<K,V>[] items;

    public Dictionary<K,V> ToDictionary() {
        Dictionary<K,V> newDict = new Dictionary<K, V>();
        foreach (var item in items)
        {
            newDict.Add(item.key,item.value);
        }
        return newDict;
    }
}

[Serializable]
public class SerializableDictItem<K,V> {
    [SerializeField]
    public K key;
    [SerializeField]
    public V value;
}
