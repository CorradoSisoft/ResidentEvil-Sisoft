using UnityEngine;
using TMPro;

public class DocumentCounter : MonoBehaviour
{
    public static DocumentCounter Instance;

    public int totalDocuments = 4;
    private int found = 0;

    [Header("UI")]
    public TextMeshProUGUI counterText; // ← trascina il TMP nell'Inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DocumentFound()
    {
        found++;
        GameManager.Instance.SetFlag($"doc_{found}", true);
        UpdateUI();

        if (found >= totalDocuments)
            TriggerEnding();
    }

    public int GetFound() => found;

    public void RestoreCount(int count)
    {
        found = count;
        for (int i = 1; i <= found; i++)
            GameManager.Instance.SetFlag($"doc_{i}", true);
        UpdateUI(); // ← aggiorna UI anche al load
    }

    void UpdateUI()
    {
        if (counterText != null)
            counterText.text = $"Trova tutti i Documenti Sisoft e scopri la verità {found}/{totalDocuments}";
    }

    void TriggerEnding()
    {
        Debug.Log("HAI TROVATO TUTTI I DOCUMENTI!");
    }
}