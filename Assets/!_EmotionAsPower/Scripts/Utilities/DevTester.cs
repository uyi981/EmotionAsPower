using System;
using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;
public class DevTester : Singleton<DevTester>
{
    public float spawnRange = 10f;
    public List<GameObject> spawnedItems;
    public int spawnAmount = 100;
    ItemSO[] itemSOs;
    private void Start()
    {
        var items = ContentManager.Instance.ItemSOs;
        
        itemSOs = items.Values.ToArray();
        spawnedItems = new List<GameObject>();
    }

    private void FixedUpdate()
    {
        if(itemSOs.Length == 0)
        {
            var items = ContentManager.Instance.ItemSOs;

            itemSOs = items.Values.ToArray();
            return;
        }
        if(spawnedItems.Count < spawnAmount)
        {
            int itemIndex = Random.Range(0, itemSOs.Length - 1);
            ItemSO itemSO = itemSOs[itemIndex];
            int amount = Random.Range(1, 100);
            Vector3 pos = new Vector3(Random.Range(-spawnRange, spawnRange),
                Random.Range(1, spawnRange),
                Random.Range(-spawnRange, spawnRange));
            var spawnedItem = ItemManager.Instance.SpawnItem(itemSO, amount, pos);
            spawnedItems.Add(spawnedItem);
        }

        //Update Emotion
        EmotionType addEmotion = (EmotionType)Random.Range(1, 6);
        EmotionEnergyManager.Instance.AddEnergy(addEmotion, Random.Range(1, 10));
        EmotionType emotion = (EmotionType)Random.Range(1, 6);
        EmotionEnergyManager.Instance.TryTakeEnergy(emotion, Random.Range(0, 3));
    }

    public static Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }
}
