using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCursor : MonoBehaviour
{
    [SerializeField] public int menuItemsCount = 3;
    [SerializeField] public Button[] menuButtons;

    [Header("Offset")]
    public float offsetY = 50f;
    public float offsetX = 50f;
    public bool useHorizontal = false;

    private int currentIndex = 0;
    private RectTransform rectTransform;
    private Vector2 initialPosition;

    [Header("Levitazione")]
    [SerializeField] private float levitationAmplitude = 5f;
    [SerializeField] private float levitationSpeed = 2f;

    private RectTransform childRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        childRect = rectTransform.GetChild(0).GetComponent<RectTransform>();
    }

    void Update()
    {
        HandleInput();
        Levitate();
    }

    private void HandleInput()
    {
        if (!useHorizontal)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                MoveUp();
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                MoveDown();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                MoveUp();
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                MoveDown();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            PressCurrentButton();

        setTextColor(currentIndex, Color.white); 
    }

    private void Levitate()
    {
        float levitationOffset = Mathf.Sin(Time.time * levitationSpeed) * levitationAmplitude;

        if (!useHorizontal)
        {
            rectTransform.anchoredPosition = new Vector2(
                initialPosition.x,
                initialPosition.y - (offsetY * currentIndex) + levitationOffset
            );

            childRect.anchoredPosition = new Vector2(
                childRect.anchoredPosition.x,
                -levitationOffset
            );
        }
        else
        {
            rectTransform.anchoredPosition = new Vector2(
                initialPosition.x + (offsetX * currentIndex) + levitationOffset,
                initialPosition.y
            );

            childRect.anchoredPosition = new Vector2(
                -levitationOffset,
                childRect.anchoredPosition.y
            );
        }
    }

    public void MoveUp()
    {
        if (currentIndex > 0)
            currentIndex--;
    }

    public void MoveDown()
    {
        if (currentIndex < menuItemsCount - 1)
            currentIndex++;
    }
    
    // ← METODO PUBBLICO PER BATTLEMANAGER
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    
    // ← RESET INDICE (per quando cambiano i nemici disponibili)
    public void ResetIndex()
    {
        currentIndex = 0;
    }

    private void PressCurrentButton()
    {
        if (menuButtons != null && 
            currentIndex < menuButtons.Length && 
            menuButtons[currentIndex] != null)
        {
            Debug.Log($"Pressed button: {menuButtons[currentIndex].name}");
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    private void setTextColor(int index, Color color)
    {
        if (menuButtons != null && index < menuButtons.Length && menuButtons[index] != null)
        {
            TextMeshProUGUI tmpComponent = menuButtons[index].GetComponentInChildren<TextMeshProUGUI>();
            if (tmpComponent != null)
                tmpComponent.color = color;
            else
            {
                Text textComponent = menuButtons[index].GetComponentInChildren<Text>();
                if (textComponent != null)
                    textComponent.color = color;
            }
        }

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i != index && menuButtons[i] != null)
            {
                TextMeshProUGUI tmpComponent = menuButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpComponent != null)
                    tmpComponent.color = Color.gray;
                else
                {
                    Text textComponent = menuButtons[i].GetComponentInChildren<Text>();
                    if (textComponent != null)
                        textComponent.color = Color.gray;
                }
            }
        }
    }
}