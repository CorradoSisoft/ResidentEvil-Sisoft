using UnityEngine;
using UnityEditor;

public class PasteAsChildForAll
{
    [MenuItem("GameObject/Duplicate As Child To All Selected %#d", false, 0)]
    static void DuplicateAsChild()
    {
        if (Selection.transforms.Length < 2)
        {
            Debug.LogWarning("Seleziona PRIMA l'oggetto da duplicare, POI i parent.");
            return;
        }

        Transform source = Selection.transforms[0];

        for (int i = 1; i < Selection.transforms.Length; i++)
        {
            Transform parent = Selection.transforms[i];

            GameObject clone = Object.Instantiate(source.gameObject);

            Undo.RegisterCreatedObjectUndo(clone, "Duplicate As Child");

            clone.transform.SetParent(parent, false);
            clone.name = source.name;
        }
    }
}