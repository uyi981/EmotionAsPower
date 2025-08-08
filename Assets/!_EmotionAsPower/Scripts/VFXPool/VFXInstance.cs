using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXInstance : MonoBehaviour
{
    public string id;
    public Color color;
    public float size;
    public float time;
    public SkillType skillType;
    public VisualEffect visualEffect;
    public void ReturnToPool()
    {
      Singleton<VFXPoolManager>.Instance.ReturnToPool(id,gameObject);    
    }
    private void OnEnable()
    {
        StartCoroutine(PlayVFX());
    }
    public IEnumerator PlayVFX()
    {
        yield return  new WaitForSeconds(time);
        ReturnToPool();
    }    

    //public Particl

}
public enum SkillType
{
    Static,     // Đứng yên
    Projectile  // Bay tới mục tiêu
}
