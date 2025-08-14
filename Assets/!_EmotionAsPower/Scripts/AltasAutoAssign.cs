using UnityEngine;
using UnityEngine.U2D;

public class AltasAutoAssign : MonoBehaviour
{
    public string spriteName;
    public SpriteAtlas spriteAtlas;
    private void Start()
    {
      transform.GetComponentInChildren<SpriteRenderer>().sprite = spriteAtlas.GetSprite(spriteName);
    }
}
