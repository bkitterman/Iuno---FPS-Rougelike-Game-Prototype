using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveProgramDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Player player;
    [SerializeField] private bool ActiveBar;

    public void OnDrop(PointerEventData eventData)
    {
        bool result;

        ProgramIcon_PrefabInventoryUI draggedIcon = eventData.pointerDrag.GetComponent<ProgramIcon_PrefabInventoryUI>();
        if (draggedIcon != null)
        {
            ProgramData dataToActivate = draggedIcon.programData;
            if (ActiveBar)
            {
                if (draggedIcon.ActiveBarIcon == true) result = false;
                else result = inventoryManager.ActivateProgram(dataToActivate, 0);
            }
            else
            {
                if (draggedIcon.ActiveBarIcon == false) result = false;
                else result = inventoryManager.DeactivateProgram(dataToActivate, 0);
            }

            if (!result)
            {
                inventoryManager.RefreshUI();
                return;
            }
            eventData.pointerDrag = null;    
            Destroy(draggedIcon.gameObject);
        }
    }
}