using System.Collections;
using UnityEngine;

public class SwordSlashVisual : MonoBehaviour
{

    [SerializeField] private int _damage = 10;
    [SerializeField] private Sword _sword;
    [SerializeField] private Collider2D _hitbox;

    private const string ATTACK = "Attack";
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>(); 
    }

    private void Start() 
    {
        _sword.OnSwordAttack += Sword_OnSwordAttack;
    }

    private IEnumerator ActivateHitbox()
    {
        _hitbox.enabled = true;
        yield return new WaitForSeconds(0.15f); // Hitbox active for 0.4 seconds
        _hitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyEntity damageable))
        {
            damageable.TakeDamage(_damage);
        }
    }

    private void Sword_OnSwordAttack(object sender, System.EventArgs e)
    {
        _animator.SetTrigger(ATTACK);
        StartCoroutine(ActivateHitbox()); 
    }

}
