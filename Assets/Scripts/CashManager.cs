using TMPro;
using UnityEngine;

public class CashManager : MonoBehaviour
{
    public static CashManager Instance { get; private set; }

    public TMP_Text cashText;
    public int cashAmount = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public void Start()
    {
        UpdateCashText();
    }

    public void AddCash(int amount)
    {
        cashAmount += amount;
        UpdateCashText();
    }

    private void UpdateCashText()
    {
        cashText.text = cashAmount.ToString();
    }
}
