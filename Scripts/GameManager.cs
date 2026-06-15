using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int lives = 20;
    public int money = 150;

    public Text livesText;
    public Text moneyText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void LoseLife()
    {
        lives--;
        UpdateUI();
        if (lives <= 0)
            Debug.Log("Game Over!");
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount) return false;
        money -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (livesText) livesText.text = "Vidas: " + lives;
        if (moneyText) moneyText.text = "Dinheiro: $" + money;
    }
}
