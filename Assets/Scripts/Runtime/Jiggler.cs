using UnityEngine;

public class Jiggler : MonoBehaviour
{
    public void Start()
    {
        _transform = gameObject.transform;
        _transform.Rotate(new Vector3(
            RandomValue() * 180.0f,
            RandomValue() * 180.0f,
            RandomValue() * 180.0f
            ));
        _material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void Update()
    {
        _transform.Rotate(new Vector3(0, RandomValue() * 5.0f, 0));
        _material.SetVector(Shader.PropertyToID("_BaseColor"), new Vector4(RandomValue(), RandomValue(), RandomValue(), 1.0f));
    }

    private float RandomValue()
        => (float)_random.NextDouble();

    private Transform _transform;
    private Material _material;
    private static System.Random _random = new System.Random();
}
