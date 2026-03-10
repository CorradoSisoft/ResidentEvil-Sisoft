using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI")]
    public GameObject inventoryPanel;
    public Image[] slots;
    public TextMeshProUGUI descriptionText;
    public RectTransform slotCursor;

    [Header("Layout slot")]
    public int columns = 2; // 2 colonne, 3 righe
    public int rows = 3;

    private ItemData[] items;
    private int slotCount;
    private bool isOpen = false;
    private int selectedIndex = 0;

    public bool IsOpen => isOpen;

    void Awake()
    {
        Instance = this;
        slotCount = slots.Length;
        items = new ItemData[slotCount];
    }

    void Update()
    {
        // Non aprire inventario se pausa è aperta
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isOpen && PauseMenu.IsPaused) return;
            ToggleInventory();
        }

        if (!isOpen) return;

        HandleNavigation();
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (isOpen)
            MoveCursor(selectedIndex);
        else
            descriptionText.text = "";
    }

    void HandleNavigation()
    {
        int col = selectedIndex % columns;
        int row = selectedIndex / columns;

        // Destra: colonna + 1, stessa riga
        if (Input.GetKeyDown(KeyCode.RightArrow) && col < columns - 1)
            MoveCursor(row * columns + (col + 1));

        // Sinistra: colonna - 1, stessa riga
        if (Input.GetKeyDown(KeyCode.LeftArrow) && col > 0)
            MoveCursor(row * columns + (col - 1));

        // Giù: riga + 1, stessa colonna
        if (Input.GetKeyDown(KeyCode.DownArrow) && row < rows - 1)
            MoveCursor((row + 1) * columns + col);

        // Su: riga - 1, stessa colonna
        if (Input.GetKeyDown(KeyCode.UpArrow) && row > 0)
            MoveCursor((row - 1) * columns + col);
    }

    void MoveCursor(int index)
    {
        if (index < 0 || index >= slotCount) return;
        selectedIndex = index;

        if (slotCursor != null)
            slotCursor.position = slots[index].rectTransform.position;

        if (items[index] != null)
            descriptionText.text = $"<b>{items[index].itemName}</b>\n{items[index].description}";
        else
            descriptionText.text = "";
    }

    public bool AddItem(ItemData item)
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                slots[i].sprite = item.icon;
                slots[i].color = Color.white;
                return true;
            }
        }
        Debug.Log("Inventario pieno!");
        return false;
    }

    public bool HasItem(ItemData item)
    {
        foreach (var i in items)
            if (i == item) return true;
        return false;
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                slots[i].sprite = null;
                slots[i].color = new Color(1, 1, 1, 0);
                if (selectedIndex == i) descriptionText.text = "";
                return;
            }
        }
    }

    public ItemData GetKeyById(string keyId)
    {
        foreach (var item in items)
            if (item != null && item.itemType == ItemType.Key && item.keyId == keyId)
                return item;
        return null;
    }
}