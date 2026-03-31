using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SkeletonVisual : MonoBehaviour
{
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField] private EnemyEntity _enemyEntity;
    [SerializeField] private GameObject _shadowSkeleton;
    private const string HIT = "Hit";
    private const string ISDIE = "IsDie";
    private const string ATTACK = "Attack";
    private const string ISRUNNING = "IsRunning";
    private const string CHASINGSPEEDMILTIPLER = "ChasingSpeedMultipler";
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _enemyAI.OnEnemyAttack += _enemyAI_OnEnemyAttack;
        _enemyEntity.OnTakeHit += _enemyEntity_OnTakeHit;
        _enemyEntity.OnDeath += _enemyEntity_OnDeath;
    }

    public void EnableCollider()
    {
        _enemyEntity.PolygonEnableCollider();
    }

    public void DisableCollider()
    {
        _enemyEntity.PolygonDisableCollider();
    }


    private void Update()
    {
        _animator.SetBool(ISRUNNING, _enemyAI.IsRunning);
        _animator.SetFloat(CHASINGSPEEDMILTIPLER, _enemyAI.GetRoamingAnimationSpeed());
    }

    private void _enemyEntity_OnDeath(object sender, System.EventArgs e)
    {
        _animator.SetBool(ISDIE, true);
        _spriteRenderer.sortingOrder = -1;
        _shadowSkeleton.SetActive(false);
    }
    private void _enemyEntity_OnTakeHit(object sender, EventArgs e)
    {
        _animator.SetTrigger(HIT);
    }

    private void OnDestroy()
    {
        _enemyAI.OnEnemyAttack -= _enemyAI_OnEnemyAttack;
    }

    private void _enemyAI_OnEnemyAttack(object sender, EventArgs e)
    {
        Debug.Log("Enemy attack animation triggered");
        _animator.SetTrigger(ATTACK);
    }

}
