using System;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyEntity : MonoBehaviour
{
    [SerializeField] private float _attackCooldown = 1.0f; 
    [SerializeField] private EnemySO _enemySO;
    private float _lastAttackTime;
    private int _currentHealth;
    private int _damageAmount;
    private PolygonCollider2D _polygonCollider2D;
    private BoxCollider2D _boxCollider2D;
    private EnemyAI _enemyAI;


    public event EventHandler OnTakeHit;
    public event EventHandler OnDeath;


    private void Awake()
    {
        _polygonCollider2D = GetComponent<PolygonCollider2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        _currentHealth = _enemySO.enemyHealth;
        _damageAmount = _enemySO.enemyDamageAmount;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Проверяем, прошло ли достаточно времени с прошлого удара
            if (Time.time - _lastAttackTime >= _attackCooldown)
            {
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(1);
                    _lastAttackTime = Time.time; // Запоминаем время удара
                }
            }
        }
    }

    public void PolygonDisableCollider()
    {
        _polygonCollider2D.enabled = false;
    }

    public void PolygonEnableCollider()
    {
        _polygonCollider2D.enabled = true;
    }
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        OnTakeHit?.Invoke(this, EventArgs.Empty);
        var damagePopup = Instantiate(_enemySO.damagePopupPrefab, transform.position + Vector3.up * 2.75f, Quaternion.identity);
        damagePopup.Setup(damage);
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
            _boxCollider2D.enabled = false;
            _polygonCollider2D.enabled = false;
            _enemyAI.SetDeathState();
        }
    }

}
