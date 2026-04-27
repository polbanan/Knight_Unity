using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI: MonoBehaviour
{
    [Header("Спрайты: от полной полоски до пустой")]
    public Sprite[] healthSprites; 

    private Image _uiImage;
    private int _currentHealth;
    private int _maxHealth;

    private void Awake()
    {
        _uiImage = GetComponent<Image>();
        _maxHealth = healthSprites.Length - 1;
        _currentHealth = _maxHealth;
    }

    public void UpdateHealthVisual(int currentHealth)
    {
        
        int spriteIndex = _maxHealth - currentHealth;
        spriteIndex = Mathf.Clamp(spriteIndex, 0, healthSprites.Length - 1);

        _uiImage.sprite = healthSprites[spriteIndex];
    }
}