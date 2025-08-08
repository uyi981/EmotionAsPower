using System.Collections.Generic;
using UnityEngine;

public class VFXObjectSystem : Singleton<VFXObjectSystem>
{
    public List<VFXInstance> listVFX = new List<VFXInstance>();
    private Dictionary<string, VFXInstance> vfxDic = new Dictionary<string, VFXInstance>();

    public VFXInstance GetVFXById(string id)
    {
        return vfxDic.ContainsKey(id) ? vfxDic[id] : null;
    }
    private void Start()
    {
        SetUp();
    }
    public void SetUp()
    {
        foreach (var vfx in listVFX)
        {
            if (vfxDic.ContainsKey(vfx.id))
            {
                continue;
            }
            vfxDic.Add(vfx.id, vfx);
        }
    }
}
