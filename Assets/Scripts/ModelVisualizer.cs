using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using Newtonsoft.Json;


/// <summary>
/// Handles most of the code for the Model Visualizer app.
/// </summary>
public class ModelVisualizer : MonoBehaviour
{

    #region STRUCTS

    /// <summary>
    /// Metadata for a model.
    /// </summary>
    public struct Record
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        public string name;
        /// <summary>
        /// The model's wnid.
        /// </summary>
        public string wnid;
        /// <summary>
        /// The model's category.
        /// </summary>
        public string wcategory;
    }


    /// <summary>
    /// Metadata for all models.
    /// </summary>
    public struct Records
    {
        /// <summary>
        /// All of the model records.
        /// </summary>
        public Record[] records;
    }

    #endregion

    #region CONSTANTS

    /// <summary>
    /// Text to show models of all synsets.
    /// </summary>
    private const string WNID_ALL = "ALL";
    /// <summary>
    /// Index in the dropdown of the default filter.
    /// </summary>
    private const int DEFAULT_WNID_FILTER = 0;
    /// <summary>
    /// Scrolling speed when using the scroll wheel.
    /// </summary>
    private const float SCROLL_SPEED = 0.1f;

    #endregion

    #region FIELDS

    /// <summary>
    /// Directory to output images.
    /// </summary>
    private static string ImageDirectory
    {
        get
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "TDWImages/ModelImages");
        }
    }

    [Header("Prefabs")]
    [SerializeField]
    private ModelImage modelImagePrefab;
    [Header("UI")]
    [SerializeField]
    private Dropdown dropdownWnids;
    [SerializeField]
    private RectTransform imageScrollView;
    [SerializeField]
    private InputField searchBar;
    [SerializeField]
    private Text textModelInfo;
    [SerializeField]
    private ScrollRect scrollRect;

    /// <summary>
    /// Model metadata.
    /// </summary>
    private Records records;

    /// <summary>
    /// Dictionary of wcategories. Key = category. Value = wnid.
    /// </summary>
    private Dictionary<string, string> wcategories = new Dictionary<string, string>();

    /// <summary>
    /// Dictionary of all models. Key = the gameobject. Value = record.
    /// </summary>
    private Dictionary<ModelImage, string> models = new Dictionary<ModelImage, string>();

    #endregion

    private void Awake()
    {
        // Get my records.
        records = JsonConvert.DeserializeObject<Records>(
            File.ReadAllText(Path.Combine(ImageDirectory, "records.json")));

        // Populate the categories and wnids.
        foreach (Record record in records.records)
        {
            if (!wcategories.ContainsKey(record.wcategory))
            {
                wcategories.Add(record.wcategory, record.wnid);
            }
        }
        // Show the images.
        PopulateCategories();
    }


    private void Update()
    {
        // Handle the scroll wheel input.
        if (Input.mouseScrollDelta.magnitude != 0)
        {
            scrollRect.verticalScrollbar.value +=
                Input.mouseScrollDelta.y * SCROLL_SPEED;
            scrollRect.verticalScrollbar.value =
                Mathf.Clamp(scrollRect.verticalScrollbar.value, 0, 1);
        }
    }

    #region METHODS_DROPDOWN_WNIDS

    /// <summary>
    /// Fetch all of the wnids.
    /// </summary>
    private void PopulateCategories()
    {
        // Remove existing options.
        dropdownWnids.ClearOptions();

        // Add the ALL option.
        dropdownWnids.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData(WNID_ALL) });

        // Add all of the options.
        List<string> categories = wcategories.Keys.ToList();

        // Sort the categories alphabetically.
        categories.Sort();
        dropdownWnids.AddOptions(categories);

        // Subscribe to event: When the dropdown value is changed, filter by category.
        dropdownWnids.onValueChanged.AddListener(FilterByCategory);

        // Get the model images.
        PopulateModelImages();
    }


    /// <summary>
    /// Filter the display by category.
    /// </summary>
    /// <param name="catIndex">The current dropdown value.</param>
    private void FilterByCategory(int catIndex)
    {
        // Deselect and reset the view.
        Deselect();
        ResetScroll();

        // Get the category selected in the dropdown.
        string category = dropdownWnids.options[catIndex].text;
        // Show all?
        if (category == WNID_ALL)
        {
            foreach (ModelImage key in models.Keys)
            {
                key.gameObject.SetActive(true);
            }
        }
        // Apply the selected filter.
        else
        {
            // Show only models with this wnid.
            List<string> names = records.records.Where(
                r => r.wcategory == category).
                Select(r => r.name).ToList();

            // Show only images in the category.
            foreach (ModelImage key in models.Keys)
            {
                key.gameObject.SetActive(names.Contains(models[key]));
            }

            // Reset the content position.
            imageScrollView.anchoredPosition = Vector2.zero;
        }
    }

    #endregion

    #region METHODS_MODELS

    /// <summary>
    /// Given the file path to an image, create the image.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    public static Texture2D GetModelImage(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }


    /// <summary>
    /// Get all model images. Only do this at start!
    /// </summary>
    private void PopulateModelImages()
    {
        DirectoryInfo d = new DirectoryInfo(ImageDirectory);

        foreach (FileInfo f in d.GetFiles())
        {
            string modelName = f.Name.Replace("\r", "").Replace(".jpg", "").Replace(".png", "");
            // Ignore blanks.
            if (modelName == "")
            {
                continue;
            }
            // Set the image.
            Texture2D tex = GetModelImage(f.FullName);
            if (tex == null)
            {
                continue;
            }

            // Instantiate the image.
            ModelImage img = Instantiate(modelImagePrefab.gameObject).GetComponent<ModelImage>();
            // Parent this image to the scroll view.
            img.GetComponent<RectTransform>().SetParent(imageScrollView);

            img.image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            img.text.text = modelName;
            // Store the image and model name.
            models.Add(img, modelName);
        }

        // Apply filter.
        dropdownWnids.value = DEFAULT_WNID_FILTER;

        // Listen to events.
        ModelImage.OnSelect += Select;
        ModelImage.OnDeselect += Deselect;
        searchBar.onValueChanged.AddListener(Search);
    }


    /// <summary>
    /// Event for when a model image is selected.
    /// </summary>
    /// <param name="img">The selected model image.</param>
    private void Select(ModelImage img)
    {
        SetModelInfo(img);
    }


    /// <summary>
    /// Deselect the currently selected image (if any).
    /// </summary>
    private void Deselect()
    {
        textModelInfo.text = "";
    }

    #endregion

    #region METHODS_SEARCH_BAR

    /// <summary>
    /// Search for a model with string filter in its model_name.
    /// </summary>
    /// <param name="filter">The search string.</param>
    private void Search(string filter)
    {
        // Deselect and reset the view.
        Deselect();
        ResetScroll();
        // No filter? Filter by wnid instead.
        if (filter == "")
        {
            FilterByCategory(dropdownWnids.value);
        }
        // Apply search filter.
        else
        {
            foreach (ModelImage key in models.Keys)
            {
                key.gameObject.SetActive(models[key].Contains(filter));
            }
        }
    }

    #endregion

    #region TEXT_MODEL_INFO

    /// <summary>
    /// Associate a ModelImage object with a MongoDBRecord.
    /// </summary>
    /// <param name="img">The ModelImage object.</param>
    private void SetModelInfo(ModelImage img)
    {
        Record record = records.records.First(r => r.name == models[img]);
        // Display info.
        textModelInfo.text = record.name + "\n" +
            record.wnid + "\n" +
            record.wcategory;
    }

    #endregion

    #region METHODS_SCROLL_VIEW

    /// <summary>
    /// Resets the model scroll view.
    /// </summary>
    private void ResetScroll()
    {
        scrollRect.velocity = Vector2.zero;
        scrollRect.verticalNormalizedPosition = 0;
    }

    #endregion

}