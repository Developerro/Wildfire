using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Player player; 
    public Image healthBar;
    public Image manaBar;

    void Update()
    {
        if (player == null) return;

        float healthPercent = player.health / player.maxHealth;
        healthBar.fillAmount = healthPercent;

        float manaPercent = player.currentMana / player.maxMana;
        manaBar.fillAmount = manaPercent;
    }
}
