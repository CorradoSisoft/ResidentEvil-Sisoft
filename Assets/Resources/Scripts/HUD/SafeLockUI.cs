using UnityEngine;
using UnityEngine.UI;

public class SafeLockUI : MonoBehaviour
{
    public Button[] numberButtons; // 0-9
    
    void Start()
    {
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int captured = i;
            numberButtons[i].onClick.AddListener(() => 
                SafeLock.Current?.PressKey(captured.ToString()));
        }
    }
}