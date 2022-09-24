using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetupTestScene : MonoBehaviour
{
    [MenuItem("TEST/Setup")]
    public static void Execute()
    {
        var source = Selection.activeGameObject;
        source.SetActive(true);
        source.transform.position = new Vector3(0, 0, 0);
        source.transform.rotation = Quaternion.Euler(0, 90, 0);

        var children = new List<GameObject>();
        for (var i = 0; i < 1000; ++i)
        {
            children.Add(Instantiate(
                source,
                new Vector3(
                    Random.value * 3.0f + 0.5f,
                    Random.value * 2.5f,
                    Random.value * 3.0f - 2.0f
                    ),
                source.transform.rotation
            ));
        }
        foreach (var child in children)
        {
            child.transform.parent = source.transform;
            child.AddComponent<Jiggler>();
        }
    }
}
