using UnityEngine;

public class Reset : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeleteAllClones();
        }
    }

    private void DeleteAllClones()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("(Clone)"))
            {
                Destroy(obj);
            }
        }

        Debug.Log("Deleted all clones.");
    }
}