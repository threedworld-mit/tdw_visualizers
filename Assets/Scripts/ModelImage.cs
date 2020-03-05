using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// UI image of a model.
/// </summary>
public class ModelImage : MonoBehaviour
{
    /// <summary>
    /// Action for when this is selected.
    /// </summary>
    public static Action<ModelImage> OnSelect;
    /// <summary>
    /// Action for this is deselected.
    /// </summary>
    public static Action OnDeselect;
    /// <summary>
    /// The image of the model.
    /// </summary>
    public Image image;
    /// <summary>
    /// The image shown when we make a selection.
    /// </summary>
    public Image highlight;
    /// <summary>
    /// The text directly underneath image, displaying the name.
    /// </summary>
    public Text text;


    private void Awake()
    {
        OnDeselect += Deselect;

        // These need to be manually re-assigned.
        // This is probably a Unity bug.
        image = transform.Find("Image").GetComponent<Image>();
        text = transform.FindDeepChild("Name").GetComponent<Text>();
        highlight = transform.FindDeepChild("highlight").GetComponent<Image>();
    }


    private void Start()
    {
        // The highlight must initially be visible for the MaterialVisualizer to find it.
        highlight.gameObject.SetActive(false);
    }


    /// <summary>
    /// Handled by event system.
    /// 
    /// </summary>
    public void Click()
    {
        if (highlight.gameObject.activeSelf)
        {
            // Deselect.
            OnDeselect?.Invoke();
        }
        else
        {
            // Deselect.
            OnDeselect?.Invoke();
            // Select.
            highlight.gameObject.SetActive(true);
            OnSelect?.Invoke(this);
        }
    }


    private void Deselect()
    {
        highlight.gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        // Deselect.
        OnDeselect?.Invoke();
    }
}