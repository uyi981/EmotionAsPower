using UnityEngine;

public class NewHealthBar : MonoBehaviour
{
    [SerializeField]
    private string property = "_Process";
    private float process;
    private Material material;

    private void OnEnable()
    {
        material = GetComponent<Renderer>().material;
    }

    public void SetProcess(float value)
    {
        this.process = value;
        material.SetFloat(property, value);
    }
}