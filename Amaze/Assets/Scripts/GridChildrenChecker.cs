using UnityEngine;

public class GridChildrenChecker : MonoBehaviour
{
    public GameObject gridGameObject;

    public void CheckChildren()
    {
        if (gridGameObject == null)
        {
            // Debug.LogError("No grid assigned.");
            return;
        }

        Debug.Log($"Checking children of: {gridGameObject.name}");
        foreach (Transform child in gridGameObject.transform)
        {
            // Debug.Log($"Found child: {child.name}");
        }

        PathObject[] pathObjects = gridGameObject.GetComponentsInChildren<PathObject>(true);
        foreach (PathObject po in pathObjects)
        {
            // Debug.Log($"Found PathObject: {po.gameObject.name}");
        }
    }
}
