using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{

    public static Player Instance {  get; private set; }

    [SerializeField] private PlayerSO _playerSO;
    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private PlayerHealthUI _playerHealthUI;

    private bool _isDead = false;
    private int _currentHealth;
    private Rigidbody2D _rb;
    private float _minSpeed = 0.1f;
    private bool isRunning = false;
    Vector2 inputVector;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    public event EventHandler OnTakeHit;
    public event EventHandler OnDeath;


    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    private void Start()
    {
        _currentHealth = _playerSO.health;
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
    }

    private void Update()
    {
        if (_isDead)
        {
            inputVector = Vector2.zero;
            return;
        }
        inputVector = GameInput.Instance.GetMovementVector();

    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void TakeDamage(int damage = 1)
    {
        if (_isDead) return;

        _currentHealth -= 1;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        StartCoroutine(FlashRoutine());

        OnTakeHit?.Invoke(this, EventArgs.Empty);
        Debug.Log("Урон пришел от: " + StackTraceUtility.ExtractStackTrace());
        _playerHealthUI.UpdateHealthVisual(_currentHealth);

        if(_currentHealth <= 0)
        {
            Die();
        }

    }
    public bool IsDead()
    {
        return _isDead;
    }
    private void Die()
    {
        if (_isDead) return; 
        _isDead = true;
        OnDeath?.Invoke(this, EventArgs.Empty);
        this.enabled = false;
        _rb.linearVelocity = Vector2.zero;
        ActiveWeapon.Instance.GetActiveWeapon().gameObject.SetActive(false);
        if (TryGetComponent(out Collider2D col))
            col.enabled = false;
    }
    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }
    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        if (_isDead) return;
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = _originalColor;
    }


    private void HandleMovement()
    {
        _rb.MovePosition(_rb.position + inputVector*(movingSpeed*Time.fixedDeltaTime));
        if (Mathf.Abs(inputVector.x) > _minSpeed || Mathf.Abs(inputVector.y) > _minSpeed)
        {
            isRunning = true;
        }
        else { isRunning = false; }
    }
}
