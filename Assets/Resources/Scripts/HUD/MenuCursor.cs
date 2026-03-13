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

    // Pulsanti originali (tutti, inclusi quelli nascosti) e le loro posizioni originali
    private Button[] _allButtons;
    private Vector2[] _originalButtonPositions;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        childRect = rectTransform.GetChild(0).GetComponent<RectTransform>();

        // Salva l'array originale completo e le posizioni originali di ogni pulsante
        _allButtons = (Button[])menuButtons.Clone();
        _originalButtonPositions = new Vector2[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
                _originalButtonPositions[i] = menuButtons[i].GetComponent<RectTransform>().anchoredPosition;
        }
    }

    void Update()
    {
        HandleInput();
        HandleMouseHover();
        Levitate();
    }

    void HandleMouseHover()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            RectTransform rt = menuButtons[i].GetComponent<RectTransform>();
            
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
        // Ripristina PRIMA le posizioni originali di tutti i pulsanti
        for (int i = 0; i < _allButtons.Length; i++)
        {
            if (_allButtons[i] != null)
                _allButtons[i].GetComponent<RectTransform>().anchoredPosition = _originalButtonPositions[i];
        }

        // Raccoglie i pulsanti visibili partendo dall'array originale completo
        var visibleButtons = new System.Collections.Generic.List<Button>();
        for (int i = 0; i < _allButtons.Length; i++)
        {
            if (_allButtons[i] == null) continue;
            if (!_allButtons[i].gameObject.activeSelf) continue;
            visibleButtons.Add(_allButtons[i]);
        }

        if (visibleButtons.Count == 0) return;

        // Usa la posizione originale del primo pulsante visibile come ancora
        int firstOriginalIndex = System.Array.IndexOf(_allButtons, visibleButtons[0]);
        Vector2 anchor = _originalButtonPositions[firstOriginalIndex];

        // Riposiziona in sequenza
        for (int i = 0; i < visibleButtons.Count; i++)
        {
            RectTransform rt = visibleButtons[i].GetComponent<RectTransform>();
            if (!useHorizontal)
                rt.anchoredPosition = new Vector2(anchor.x, anchor.y - (offsetY * i));
            else
                rt.anchoredPosition = new Vector2(anchor.x + (offsetX * i), anchor.y);
        }

        // Aggiorna l'array attivo con solo i pulsanti visibili
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

    public int GetCurrentIndex() => currentIndex;
    public void ResetIndex() => currentIndex = 0;

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