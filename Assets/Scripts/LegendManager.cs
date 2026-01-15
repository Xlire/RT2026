using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

/// <summary>
/// Class for managing the legend and its interactions in the game.
/// </summary>
public class LegendManager : MonoBehaviour
{
    private const float _panelColorAlpha = 0.7f; // Alpha cannot be changed in the inspector.
    public Color PanelColor = new Color(0.2f, 0.2f, 0.2f, _panelColorAlpha);

    private float _collapsedWidthPercentage = 0.2f;
    private float _expandedWidthPercentage = 0.3f;
    private float _collapsedHeight = 50f;
    private float _maxExpandedHeight = 300f; // Scrolling could be added to the legend panel if needed.
    private float _itemHeight = 40f;

    public GameObject legendPanelPrefab;
    private GameObject _legendPanel;
    private GameObject _inputPanel;
    private GameObject _expandField;
    private Keybinds _keybinds;

    private bool _isExpanded = false;
    private bool _isHidden = false;
    private bool _itemsCreated = false;

    /// <summary>
    /// Set up the legend panel and its components.
    /// </summary>
    private void Start()
    {
        // Find the UI canvas.
        GameObject uiCanvas = GameObject.Find("UI Canvas");
        if (uiCanvas == null)
        {
            Debug.LogError("UI Canvas not found.");
            return;
        }
        
        // Instantiate the legend panel prefab.
        legendPanelPrefab = Resources.Load<GameObject>("Prefabs/UI/LegendPanel");
        if (legendPanelPrefab == null)
        {
            Debug.LogError("LegendPanel prefab not found.");
            return;
        }
        _legendPanel = Instantiate(legendPanelPrefab, uiCanvas.transform);
        _legendPanel.name = "LegendPanel";

        _inputPanel = _legendPanel.transform.Find("InputPanel").gameObject;
        _expandField = _legendPanel.transform.Find("ExpandLegend").gameObject;

        if (_inputPanel == null || _expandField == null)
        {
            Debug.LogError("Issue with finding the children of LegendPanel.");
            return;
        }

        _keybinds = KeybindManager.Instance.keybinds;

        MakeLegendPanel();
        ApplyLegendWidth();
    }

    /// <summary>
    /// Check for inputs every frame.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ToggleLegend();
        if (Input.GetKeyDown(KeyCode.H)) ToggleLegendVisibility();
    }

    /// <summary>
    /// Called when the script is loaded or a value is changed in the inspector.
    /// Makes sure the legend panel is set up correctly.
    /// </summary>
    private void OnValidate()
    {
        if (_legendPanel == null) _legendPanel = GameObject.Find("LegendPanel");
        if (_inputPanel == null) _inputPanel = GameObject.Find("InputPanel");
        if (_expandField == null) _expandField = GameObject.Find("ExpandLegend");

        if (_legendPanel == null || _inputPanel == null || _expandField == null) return;

        PanelColor.a = _panelColorAlpha;
        _legendPanel.GetComponent<Image>().color = PanelColor;
    }

    /// <summary>
    /// Toggle the legend panel between an expanded and collapsed state.
    /// If hidden and toggled, legend panel is unhidden.
    /// </summary>
    private void ToggleLegend()
    {
        if (_isHidden)
        {
            _legendPanel.SetActive(true);
            _isHidden = false;
        }

        if (_isExpanded) CollapseLegend();
        else ExpandLegend();
    }

    /// <summary>
    /// Expand the legend panel to show all keybinds.
    /// </summary>
    private void ExpandLegend()
    {
        if (!_itemsCreated) 
        {
            CreateLegendItems();
        }

        _inputPanel.SetActive(true);
        _isExpanded = true;
        ApplyLegendWidth();

        _expandField.GetComponentInChildren<TextMeshProUGUI>().text = "Collapse Legend <sprite name=\"q\">";

        LayoutRebuilder.ForceRebuildLayoutImmediate(_legendPanel.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Collapse the legend panel to show only the title and the expand button.
    /// </summary>
    private void CollapseLegend()
    {
        _inputPanel.SetActive(false);
        _isExpanded = false;
        ApplyLegendWidth();

        _expandField.GetComponentInChildren<TextMeshProUGUI>().text = "Expand Legend <sprite name=\"q\">";
    
        _legendPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(_legendPanel.GetComponent<RectTransform>().sizeDelta.x, 50f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_legendPanel.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Toggle the visibility of the legend panel.
    /// </summary>
    private void ToggleLegendVisibility()
    {
        if (_isHidden)
        {
            _legendPanel.SetActive(true);
            _isHidden = false;
        }
        else
        {
            _legendPanel.SetActive(false);
            _isHidden = true;
        }
    }

    /// <summary>
    /// Create the legend panel and its components.
    /// </summary>
    private void MakeLegendPanel()
    {
        _legendPanel.GetComponent<Image>().color = PanelColor;

        // Set legend panel anchoring and pivot.
        RectTransform legendPanelRect = _legendPanel.GetComponent<RectTransform>();
        legendPanelRect.anchorMin = new Vector2(1, 1);  // top-left
        legendPanelRect.anchorMax = new Vector2(1, 1);  // top-left
        legendPanelRect.pivot = new Vector2(1, 1);      // top-left
        legendPanelRect.anchoredPosition = new Vector2(-10f, -10f); // small offset from top-left corner

        // Control the whole legend's layout with a vertical layout group.
        VerticalLayoutGroup legendPanelLayoutGroup = _legendPanel.GetComponent<VerticalLayoutGroup>();
        if (legendPanelLayoutGroup == null)
        {
            legendPanelLayoutGroup = _legendPanel.AddComponent<VerticalLayoutGroup>();
        }
        legendPanelLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        legendPanelLayoutGroup.spacing = 0;
        legendPanelLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
        legendPanelLayoutGroup.childForceExpandWidth = true;
        legendPanelLayoutGroup.childForceExpandHeight = false;

        // Make sure the Image component adapts to the RectTransform.
        Image legendPanelImage = _legendPanel.GetComponent<Image>();
        if (legendPanelImage != null)
        {
            legendPanelImage.preserveAspect = false;
            legendPanelImage.type = Image.Type.Sliced; // Allow for resizing the panel image.
        }

        RectTransform inputPanelRect = _inputPanel.GetComponent<RectTransform>();
        inputPanelRect.anchorMin = new Vector2(0, 0.2f);
        inputPanelRect.anchorMax = new Vector2(1, 1);
        inputPanelRect.pivot = new Vector2(0.5f, 1);
        inputPanelRect.anchoredPosition = Vector2.zero;
        inputPanelRect.sizeDelta = Vector2.zero;

        // Add a vertical layout group to hold the legend items.
        VerticalLayoutGroup layoutGroup = _inputPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = _inputPanel.AddComponent<VerticalLayoutGroup>();
        }
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.spacing = 10f;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = _inputPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = _inputPanel.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _inputPanel.SetActive(false); // Start with the input panel hidden.
    }

    /// <summary>
    /// Apply the width percentage to the legend panel.
    /// </summary>
    private void ApplyLegendWidth()
    {
        if (_legendPanel == null) return;

        RectTransform legendPanelRect = _legendPanel.GetComponent<RectTransform>();

        RectTransform canvasRect = _legendPanel.transform.root.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;

        float targetWidth = canvasWidth * (_isExpanded ? _expandedWidthPercentage : _collapsedWidthPercentage);
        float targetHeight = _isExpanded ? CalculateExpandedHeight() : _collapsedHeight;

        legendPanelRect.sizeDelta = new Vector2(targetWidth, targetHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(legendPanelRect);
    }

    /// <summary>
    /// Calculate the height for the expanded legend.
    /// </summary>
    /// <returns>The calculated height for the expanded legend.</returns>
    private float CalculateExpandedHeight()
    {
        // Calculate the height of the legend panel based on the number of items.
        int itemCount = _inputPanel.transform.childCount + 1;
        float totalHeight = itemCount * _itemHeight + (itemCount - 1) * 10f; // Add spacing between items.

        // Limit the height to MaxExpandedHeight.
        return Mathf.Min(totalHeight, _maxExpandedHeight);
    }

    /// <summary>
    /// Create the legend items based on the keybinds.
    /// </summary>
    private void CreateLegendItems()
    {
        foreach (Transform child in _inputPanel.transform)
        {
            Destroy(child.gameObject);
        }

        ApplyLegendWidth();

        // Add the hardcoded keybinds.
        AddKeybindEntry("Move", "<sprite name=\"w\"><sprite name=\"a\"><sprite name=\"s\"><sprite name=\"d\">");
        AddKeybindEntry("Jump", "<sprite name=\"space\">");

        _keybinds = KeybindManager.Instance.keybinds;
        var keybindFields = typeof(Keybinds).GetFields();

        foreach (var field in keybindFields)
        {
            KeyCode keyCode = (KeyCode)field.GetValue(_keybinds);

            string label = field.Name;
            string keyName = keyCode.ToString().ToLower();

            if (keyName == "none") continue;

            string spriteText = $"<sprite name=\"{keyName}\">";

            AddKeybindEntry(label, spriteText);
        }

        AddKeybindEntry("Exit", "<sprite name=\"esc\">");
        
        _itemsCreated = true;
    }

    /// <summary>
    /// Add a keybind entry to the legend panel.
    /// </summary>
    /// <param name="label">The label for the keybind.</param>
    /// <param name="spriteText">The sprite text for the keybind.</param>
    private void AddKeybindEntry(string label, string spriteText)
    {
        RectTransform canvasRect = _legendPanel.transform.root.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;

        GameObject entryPanel = new GameObject(label + " Panel");
        entryPanel.transform.SetParent(_inputPanel.transform, false);

        RectTransform panelRect = entryPanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(canvasWidth * _expandedWidthPercentage, _itemHeight);

        HorizontalLayoutGroup horizontalLayout = entryPanel.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayout.spacing = 20;

        GameObject labelObject = new GameObject(label + " Label");
        labelObject.transform.SetParent(entryPanel.transform, false);

        // Add and modify the label's text component.
        TextMeshProUGUI labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Color.white;

        GameObject spriteObject = new GameObject(label + " Icon");
        spriteObject.transform.SetParent(entryPanel.transform, false);

        // Add and modify the sprite's text component.
        TextMeshProUGUI spriteTextComponent = spriteObject.AddComponent<TextMeshProUGUI>();
        spriteTextComponent.text = spriteText;
        spriteTextComponent.fontSize = 14;
        spriteTextComponent.alignment = TextAlignmentOptions.Right;
        spriteTextComponent.color = Color.white;

        // Add the keybind sprites to the required component.
        TMP_SpriteAsset spriteAsset = Resources.Load<TMP_SpriteAsset>("Sprites/input sprites/input_icons");
        if (spriteAsset != null)
        {
            spriteTextComponent.spriteAsset = spriteAsset;
            Debug.Log("Sprite asset found and assigned.");
        }
        else
        {
            Debug.LogError("Sprite asset not found.");
        }

        // Set the layout elements for the label and sprite objects.
        LayoutElement labelLayout = labelObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 100;

        LayoutElement iconLayout = spriteObject.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 100;
    }
}
