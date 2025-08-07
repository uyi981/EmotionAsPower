using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private RawImage background;

    [SerializeField] private Texture2D[] gameArts;

    private void OnEnable()
    {
        int index = Random.Range(0, gameArts.Length);
        background.texture = gameArts[index];
    }
}