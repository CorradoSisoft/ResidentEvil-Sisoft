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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip pressSound;

    private RectTransform childRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        childRect = rectTransform.GetChild(0).GetComponent<RectTransform>();
    }


    void Update()
    {
        HandleInput();
        HandleMouseHover();
        Levitate();
    }

    /* void HandleMouseHover()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition))
            {
                if (currentIndex != i)
                {
                    currentIndex = i;
                    PlayMoveSound();
                }

                // Click sinistro conferma
                if (Input.GetMouseButtonDown(0))
                    PressCurrentButton();
                return;
            }
        }
    } */
    void HandleMouseHover()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
            
            // ← passa la camera del Canvas
            Canvas canvas = menuButtons[i].GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas != null ? canvas.worldCamera : null;
            
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, canvasCamera))
            {
                if (currentIndex != i)
                {
                    currentIndex = i;
                    PlayMoveSound();
                }

                if (Input.GetMouseButtonDown(0))
                    PressCurrentButton();
                return;
            }
        }
    }

    public void RebuildLayout()
    {
        // Crea lista dei soli pulsanti visibili
        var visibleButtons = new System.Collections.Generic.List<Button>();
        
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;
            if (!menuButtons[i].gameObject.activeSelf) continue;
            visibleButtons.Add(menuButtons[i]);
        }

        // Riposiziona
        for (int i = 0; i < visibleButtons.Count; i++)
        {
            RectTransform rt = visibleButtons[i].GetComponent<RectTransform>();
            if (!useHorizontal)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, initialPosition.y - (offsetY * i));
            else
                rt.anchoredPosition = new Vector2(initialPosition.x + (offsetX * i), rt.anchoredPosition.y);
        }

        // Aggiorna l'array con solo i pulsanti visibili
        menuButtons = visibleButtons.ToArray();
        menuItemsCount = menuButtons.Length;
        currentIndex = 0;
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
        {
            currentIndex--;
            PlayMoveSound();
        }
    }

    public void MoveDown()
    {
        if (currentIndex < menuItemsCount - 1)
        {
            currentIndex++;
            PlayMoveSound();
        }
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
            PlayPressSound();
            Debug.Log($"Pressed button: {menuButtons[currentIndex].name}");
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    private void PlayMoveSound()
    {
        if (audioSource != null && moveSound != null)
            audioSource.PlayOneShot(moveSound);
    }

    private void PlayPressSound()
    {
        if (audioSource != null && pressSound != null)
            audioSource.PlayOneShot(pressSound);
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