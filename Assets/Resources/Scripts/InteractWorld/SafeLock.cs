using UnityEngine;

public class SafeLock : MonoBehaviour, IInteragibile
{
    public static bool IsAnyOpen = false;

    [Header("Configurazione")]
    public string correctCode = "17254";
    public ItemData rewardItem;

    [Header("UI")]
    public GameObject safePanel;

    private string currentInput = "";
    private bool solved = false;
    private bool isOpen = false;
    public static bool _justClosed = false;


    public static SafeLock Current { get; private set; }

    void Update()
    {
        if (!isOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return; // ← consuma l'input
        }
    }

    public void PressKey(string digit)
    {
        Debug.Log($"PressKey chiamato con: {digit} | currentInput: {currentInput} | correctCode: {correctCode}");
        if (currentInput.Length >= correctCode.Length) return;

        currentInput += digit;

        // Auto-verifica quando si raggiunge l'ultima cifra
        if (currentInput.Length == correctCode.Length)
        {
            if (currentInput == correctCode)
            {
                solved = true;
                Debug.Log($"Codice corretto! rewardItem = {rewardItem}"); 
                Close();
                if (rewardItem != null)
                {
                    InventoryManager.Instance.AddItem(rewardItem);
                    
                    // ← aggiunto
                    if (rewardItem.itemType == ItemType.SpecialDocument)
                        DocumentCounter.Instance.DocumentFound();
                    
                    InventoryManager.Instance.OpenInventory();
                }
                GameManager.Instance.SetFlag($"safe_{gameObject.name}", true);
            }
            else
            {
                // Codice sbagliato: reset silenzioso
                currentInput = "";
                Close();
            }
        }
    }

    public void Interagisci()
    {
        Debug.Log($"[SafeLock] {gameObject.name} Interagisci chiamato, solved={solved}");
        if (solved) return;
        IsAnyOpen = true;
        isOpen = true;
        currentInput = ""; // reset ad ogni apertura
        safePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Current = this;
    }

    public void Close()
    {
        IsAnyOpen = false;
         _justClosed = true;
        isOpen = false;
        safePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Current = null;
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (!solved && hintInteract != null) hintInteract.SetActive(true);
    }

    // Proprietà pubblica per SaveManager
    public bool IsSolved => solved;

    // Metodo per restore al load
    public void SetSolved()
    {
        solved = true;
        GameManager.Instance.SetFlag($"safe_{gameObject.name}", true);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}