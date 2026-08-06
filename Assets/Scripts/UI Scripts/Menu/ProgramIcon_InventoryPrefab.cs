using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

// Add these three interfaces to your class definition
public class ProgramIcon_PrefabInventoryUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image containerImage;
    public Image iconArtImage;
    public Image borderImage;
    public Image backgroundImage;
    public TextMeshProUGUI nameDisplay;
    [SerializeField] private Animator animator;

    public ProgramData programData { get; private set; }
    private InventoryManager inventoryManager;
    private Canvas mainCanvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    public TextMeshProUGUI stackCountText;
    public GameObject stackCountPopup;

    public bool ActiveBarIcon = false;
    bool isOptimized = false;

    public void Initialize(ProgramData data, InventoryManager manager, Canvas canvas, bool isActive, bool isOptimized)
    {
        programData = data;
        inventoryManager = manager;
        mainCanvas = canvas;
        canvasGroup = GetComponent<CanvasGroup>();
        nameDisplay.text = data.ProgramName;
        iconArtImage.sprite = data.IconSprite;
        borderImage.color = data.Rarity.DisplayColor;
        backgroundImage.color = data.ApplicationSuite.SuiteColor;
        this.isOptimized = isOptimized;
        ActiveBarIcon = isActive;
    }

    void Start()
    {
        if (animator != null)
        {
            animator.SetBool("isOptimized", isOptimized);
        }
    }

    // ADD THIS NEW METHOD
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // LEFT-CLICK: Move one stack
            if (ActiveBarIcon)
                inventoryManager.DeactivateProgram(programData, 1);
            else
                inventoryManager.ActivateProgram(programData, 1);
        }

        // Show info regardless, works if right clicked.
        inventoryManager.OnInventoryIconClicked(programData);
    }

    // ----- DRAGGING -----

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Show info
        inventoryManager.OnInventoryIconClicked(programData);

        // Store starting point
        originalParent = transform.parent;

        transform.SetParent(mainCanvas.transform, true);
        transform.SetAsLastSibling();

        // Make the icon see-through and disable raycasts
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        (transform as RectTransform).anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            transform.SetParent(originalParent);
            (transform as RectTransform).anchoredPosition = Vector2.zero;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryManager.ShowCostPreview(programData, !ActiveBarIcon);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryManager.ClearCostPreview();
    }
}