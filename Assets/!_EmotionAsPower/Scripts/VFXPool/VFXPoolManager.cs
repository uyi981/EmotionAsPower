
using System.Collections.Generic;
using UnityEngine;

public class VFXPoolManager : MonoBehaviour
{
    Dictionary<string, VFXPool> pools = new Dictionary<string, VFXPool>();
    float poolLifetime = 30f;
    public GameObject PopSKillObject(string id)
    {
        if (pools.ContainsKey(id))
        {
            VFXPool pool = pools[id];
            if (pool != null)
            {
                GameObject skillObject = pool.GetSkillObject();
                if (skillObject != null)
                {
                    skillObject.SetActive(true);
                    return skillObject;
                }
                GameObject obj1 = Instantiate(Singleton<VFXObjectSystem>.Instance.GetVFXById(id).gameObject);
              //  obj1.transform.SetParent(transform);
                return obj1;
            }
        }
        VFXPool newPool = new VFXPool();
        pools.Add(id, newPool);
        GameObject obj = Instantiate(Singleton<VFXObjectSystem>.Instance.GetVFXById(id).gameObject);
       // obj.transform.SetParent(transform);
        return obj;
    }
    public void ReturnToPool(string id, GameObject skillObject)
    {
        if (pools.ContainsKey(id))
        {
            VFXPool pool = pools[id];
            pool.ReturnSkillObject(skillObject);
        }
        else
        {
            VFXPool pool = new VFXPool();
            pools.Add(id, pool);
            pool.ReturnSkillObject(skillObject);
        }
    }
    void CheckExpiredPools()
    {
        List<string> toRemove = new List<string>();
        foreach (var kvp in pools)
        {
            if (Time.time - kvp.Value.lastUsedTime > poolLifetime)
            {
                kvp.Value.Clear();
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var id in toRemove)
        {
            pools.Remove(id);
        }
    }
    private void Start()
    {
        InvokeRepeating(nameof(CheckExpiredPools), 10f, 10f); // chạy sau 10s, rồi mỗi 10s
    }


}

