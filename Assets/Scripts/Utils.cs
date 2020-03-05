using UnityEngine;


/// <summary>
/// Utility class.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Returns a descendant of a parent transform.
    /// </summary>
    /// <param name="aName">Name of the descendant.</param>
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        // Try to find the child.
        Transform result = aParent.Find(aName);
        if (result != null)
        {
            return result;
        }
        // Recurse through descendants.
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}