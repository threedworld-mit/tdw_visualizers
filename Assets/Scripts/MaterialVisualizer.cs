using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;


/// <summary>
/// Visualize all of the materials in the Materials Library.
/// You must run MaterialScreenshotter before using this.
/// </summary>
public class MaterialVisualizer : MonoBehaviour
{

    #region STRUCTS

    /// <summary>
    /// Metadata for a material.
    /// </summary>
    public struct Record
    {
        /// <summary>
        /// The name of the material.
        /// </summary>
        public string name;
        /// <summary>
        /// The semantic material type.
        /// </summary>
        public SemanticMaterialType type;
    }


    /// <summary>
    /// Metadata for all materials.
    /// </summary>
    public struct Records
    {
        /// <summary>
        /// All of the material records.
        /// </summary>
        public Record[] records;
    }

    #endregion

    /// <summary>
    /// Scrolling speed when using the scroll wheel.
    /// </summary>
    private const float SCROLL_SPEED = 0.1f;

    /// <summary>
    /// The type of search.
    /// </summary>
    private enum SearchType { Name, Type }
    /// <summary>
    /// Scrollview content for images.
    /// </summary>
    [SerializeField]
    private RectTransform content;
    /// <summary>
    /// Prefab for images.
    /// </summary>
    [SerializeField]
    private ModelImage imagePrefab;
    /// <summary>
    /// The quit button.
    /// </summary>
    [SerializeField]
    private Button buttonQuit;
    /// <summary>
    /// Dropdown for search type.
    /// </summary>
    [SerializeField]
    private Dropdown dropdownSearch;
    /// <summary>
    /// Search field.
    /// </summary>
    [SerializeField]
    private InputField inputSearch;
    /// <summary>
    /// Text about the material.
    /// </summary>
    [SerializeField]
    private Text textMaterialInfo;
    /// <summary>
    /// Loading text.
    /// </summary>
    [SerializeField]
    private Text loading;
    [SerializeField]
    private ScrollRect scrollRect;
    /// <summary>
    /// Dictionary of all images.
    /// </summary>
    private Dictionary<ModelImage, Record> images =
        new Dictionary<ModelImage, Record>();

    /// <summary>
    /// All Semantic Material Types, in order as they appear on the dropdown menu.
    /// </summary>
    private List<SemanticMaterialType> types =
        new List<SemanticMaterialType>();
    /// <summary>
    /// Metadata records.
    /// </summary>
    private Records records;

    /// <summary>
    /// Directory to output images.
    /// </summary>
    private static string ImageDirectory
    {
        get
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "TDWImages/MaterialImages");
        }
    }


    private void Start()
    {
        records = JsonConvert.DeserializeObject<Records>(
            File.ReadAllText(Path.Combine(ImageDirectory, "records.json")));

        foreach (Record record in records.records)
        {
            string filepath = Path.Combine(ImageDirectory,
                record.name + ".png");
            if (!File.Exists(filepath))
            {
                continue;
            }
            // Instantiate the image.
            ModelImage img = Instantiate(imagePrefab.gameObject).GetComponent<ModelImage>();

            // Parent this image to the scroll view.
            img.GetComponent<RectTransform>().SetParent(content);
            // Set the image.
            Texture2D tex = GetMaterialImage(filepath);
            img.image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            img.text.text = record.name;
            // Store the image and model name.
            images.Add(img, record);
        }
        loading.gameObject.SetActive(false);

        buttonQuit.onClick.AddListener(() => Application.Quit());
        PopulateDropdownSearch();
        dropdownSearch.onValueChanged.AddListener(SelectSearchType);
        inputSearch.onValueChanged.AddListener(FilterByName);
        // Set a defaul value.
        dropdownSearch.value = 0;

        // Listen to events.
        ModelImage.OnSelect += Select;
        ModelImage.OnDeselect += Deselect;
    }


    private void Update()
    {
        // Handle the scroll wheel.
        if (Input.mouseScrollDelta.magnitude != 0)
        {
            scrollRect.verticalScrollbar.value +=
                Input.mouseScrollDelta.y * SCROLL_SPEED;
            scrollRect.verticalScrollbar.value =
                Mathf.Clamp(scrollRect.verticalScrollbar.value, 0, 1);
        }
    }


    /// <summary>
    /// Given the file path to an image, create the image.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    public static Texture2D GetMaterialImage(string filePath)
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
    /// Select an image.
    /// </summary>
    /// <param name="image">The image.</param>
    private void Select(ModelImage image)
    {
        textMaterialInfo.text = images[image].name + "\tType: " + images[image].type;
    }


    /// <summary>
    /// Deselect an image.
    /// </summary>
    private void Deselect()
    {
        textMaterialInfo.text = "";
    }


    /// <summary>
    /// Populate the dropdown with SemanticMaterialTypes.
    /// </summary>
    private void PopulateDropdownSearch()
    {
        // Get all semantic types.
        types = Enum.GetValues(
            typeof(SemanticMaterialType)).Cast<SemanticMaterialType>
            ().ToList();
        // Sort the the types alphabetically.
        types.Sort();

        // Add each category.
        dropdownSearch.AddOptions(types.Select(
            t => new Dropdown.OptionData(t.ToString())).
            ToList());
    }


    /// <summary>
    /// Select a new search type. 0=name, 1 etc.=types
    /// </summary>
    /// <param name="type">The dropdown index.</param>
    private void SelectSearchType(int type)
    {
        if (type == 0)
        {
            FilterByName(inputSearch.text);
        }
        else
        {
            FilterByCategory(types[type - 1]);
        }
    }


    /// <summary>
    /// Filter the visualizer by a material type.
    /// </summary>
    /// <param name="type">The type of material.</param>
    private void FilterByCategory(SemanticMaterialType type)
    {
        ResetScroll();
        foreach (ModelImage img in images.Keys)
        {
            img.gameObject.SetActive(images[img].type == type &&
                SearchIsMatch(inputSearch.text, images[img]));
        }
    }


    /// <summary>
    /// Filter the visualizer by a material name.
    /// </summary>
    /// <param name="materialName">The name of the material.</param>
    private void FilterByName(string materialName)
    {
        ResetScroll();
        foreach (ModelImage img in images.Keys)
        {
            img.gameObject.SetActive(SearchIsMatch(materialName, images[img]));
        }
    }


    /// <summary>
    /// Returns true if the record's name contains the material name.
    /// </summary>
    /// <param name="materialName">The material name.</param>
    /// <param name="record">The record.</param>
    private bool SearchIsMatch(string materialName, Record record)
    {
        return materialName == "" ? true : record.name.Contains(materialName);
    }


    /// <summary>
    /// Resets the model scroll view.
    /// </summary>
    private void ResetScroll()
    {
        scrollRect.velocity = Vector2.zero;
        scrollRect.verticalNormalizedPosition = 0;
    }
}