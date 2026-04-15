using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealthUI : MonoBehaviour
{
    public Image playerHealthBar;
    public int maxHealth = 100;
    private int _currentHealth;

    public void Start()
    {
       playerHealthBar.enabled = true;
        _currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        UpdateHealthBar(); 
    }
    private void UpdateHealthBar()
    {
        playerHealthBar.fillAmount = (float)_currentHealth / maxHealth;
    }
}
