using Knight.Utils;
using System;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private State _stardingState;
    [SerializeField] private float _delayChase = 0.5f;
    [SerializeField] private float _roamingDistanceMax = 7f;
    [SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimeMax = 2f;
    [SerializeField] private float _ChasingDistance = 8f;
    [SerializeField] private float _attackingDistance = 1.5f;
    
    
    private bool _isReadyToChase = false;
    private float _ChasingSpeedMultiplay = 2f;
    private float _delayAttack = 1f;
    private float _lastAttackTime = 0f; 
    private float _attackRate = 1.5f;
    private bool _waitingBeforeChase = false;
    private bool _waitingAfterChase = false;
    private bool _waitingRoam = false;
    private float _radius = 15f;
    public float moveRadius = 5f;
    public Vector3 startPosition;
    private bool _playerInsideZone = false;


    private NavMeshAgent _navMeshAgent;
    private Transform _playerTransform;
    private Transform _centerPosition;
    private State _currentState;
    private float _roamingTimer;
    private Vector3 _roamPosition;
    private float _roamingSpeed;
    private float _chasingSpeed;
    private Player _player;
    



    public event EventHandler OnEnemyAttack;

    public bool IsRunning
    {
        get
        {
            if (_navMeshAgent.velocity.sqrMagnitude < 0.3f)
                return false;
            return true;
        }
    }


    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        // 1. »щем игрока по всей сцене, а не на обьекте скелета
        _player = FindFirstObjectByType<Player>();
        _navMeshAgent.updateRotation = false; // „тобы не вращалс€ когда двигалсм€
        _navMeshAgent.updateUpAxis = false;  // „тобы оринетаци€ navMash не вли€ла на оринетацию обьекта
        _currentState = State.Roaming;
        _roamingSpeed = _navMeshAgent.speed;
        _chasingSpeed = _roamingSpeed + _ChasingSpeedMultiplay;

    }

    private void Start()
    {
        if(_player != null) 
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        _centerPosition = transform;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (_currentState == State.Death) return;
        StateHandler();
        if (_currentState == State.Attacking || _currentState ==  State.Chasing)
            ChangeFacingDirectionByVelocity();

        float distance = Vector3.Distance(transform.position, startPosition);
        if (distance > moveRadius)
        {
            if (_navMeshAgent.hasPath)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
            }

            Vector3 clamped = startPosition + Vector3.ClampMagnitude(transform.position - startPosition, moveRadius);
            _navMeshAgent.Warp(clamped);
        }
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, moveRadius);
    }
 

    public void SetDeathState()
    {
        _currentState = State.Death;
        _navMeshAgent.ResetPath();
    }
    
    public float GetRoamingAnimationSpeed()
    {
        return _navMeshAgent.speed / _roamingSpeed;
    }

    private IEnumerator WaitBeforeChasing()
    {
        _waitingBeforeChase = true;
        _isReadyToChase = false; 

        _isReadyToChase = true; 
        _waitingBeforeChase = false;
        yield break;
    }

    private IEnumerator WaitBeforeAttacing()
    {
        _waitingAfterChase = true;
        yield return new WaitForSeconds(_delayChase);

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer < _ChasingDistance)
            _currentState = State.Attacking;

        _waitingAfterChase = false;
    }

    private IEnumerator WaitRoaming()
    {
        _waitingRoam = true;
        _navMeshAgent.ResetPath();
        yield return new WaitForSeconds(2.0f);
        Roaming();
        _roamingTimer = _roamingTimeMax;
        _waitingRoam = false;
    }

    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void StateHandler()
    {
        switch (_currentState)
        {
            case State.Roaming:
                Vector3 offset = transform.position - _centerPosition.position;
                Vector3 clampefOffset = Vector3.ClampMagnitude(offset, _radius);
                transform.position = _centerPosition.position + clampefOffset;
                if (!_waitingRoam)
                {
                    _roamingTimer -= Time.deltaTime;    
                    if (_roamingTimer < 0)
                    {
                        StartCoroutine(WaitRoaming());

                    }
                }
                CheckCurrentState();
                ChangeFacingDirectionByVelocity();
                break;
            case State.Chasing:
                ChasingTarget();
                if(!_waitingAfterChase)
                    StartCoroutine(WaitBeforeAttacing());
                CheckCurrentState();
                break;
            case State.Attacking:
                CheckCurrentState();
                float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
                if (distanceToPlayer < _attackingDistance)
                {
                    TryAttack();
                }
                else
                { 
                    if(!_waitingBeforeChase)
                    {
                        StartCoroutine(WaitBeforeChasing());
                    }             
                }
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                CheckCurrentState();

                _roamingTimer -= Time.deltaTime;
                if (_roamingTimer <= 0)
                {
                    _currentState = State.Roaming; 
                }
                break;
        }

    }

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime > _delayAttack)
        {
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);
            _lastAttackTime = Time.time + _attackRate;
        }
    }

    private void ChasingTarget() {         
        _navMeshAgent.SetDestination(_playerTransform.position);
    }


    private void CheckCurrentState()
    {
        if (_player == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        float distanceToStartPosition = Vector3.Distance(startPosition, _playerTransform.position);
        _playerInsideZone = (distanceToStartPosition <= moveRadius);
        State _newState = State.Roaming;
        if (!_player.IsDead() && _playerInsideZone)
        {
            if (distanceToPlayer < _ChasingDistance)
            {
                if (distanceToPlayer < _attackingDistance)
                {
                    _navMeshAgent.velocity = Vector3.zero;
                    _newState = State.Attacking;
                }
                else
                {

                    if (!_waitingBeforeChase && !_isReadyToChase)
                    {
                        StartCoroutine(WaitBeforeChasing());
                    }

                    if (_isReadyToChase)
                    {
                        _newState = State.Chasing;
                    }
                }
            }
        }

        if (_newState != _currentState)
        {
            if (_newState == State.Idle)
            {
                _navMeshAgent.ResetPath();
            }
            if (_newState == State.Chasing)
            {
                _navMeshAgent.ResetPath(); // „тобы сбросить путь который был до этого и не было рывков при смене состо€ни€
                _navMeshAgent.speed = _chasingSpeed;
                if (distanceToPlayer <= _attackingDistance)
                {
                    _navMeshAgent.isStopped = true;
                    _navMeshAgent.velocity = Vector3.zero;

                    LookAtPlayer();
                }
                else
                {
                    _navMeshAgent.isStopped = false;
                }

            }
            else if (_newState == State.Roaming)
            {
                _roamingTimer = 0f; // 0f потому что у нас в состо€нии Roaming в начале Update отнимаетс€ врем€ и если оно меньше 0 то вызываетс€ метод Roaming() и устанавливаетс€ таймер на максимум, а так как мы его обнул€ем то в следущем кадре он будет меньше 0 и вызоветс€ метод Roaming() и установитс€ таймер на максимум, тем самым мы не будем ждать пока таймер закончитс€ а сразу же начнем бродить
                _navMeshAgent.speed = _roamingSpeed;
            }
            else if (_newState == State.Attacking)
            {
                {
                    _navMeshAgent.ResetPath();
                }
            }
            _currentState = _newState;
        }
    }
    private void Roaming()
    {
        _roamPosition = GetRoamingPosition();
        _navMeshAgent.SetDestination(_roamPosition);
    }

    private Vector3 GetRoamingPosition()
    {
        return startPosition + Utils.GetRandomDir() * UnityEngine.Random.Range(_roamingDistanceMin, _roamingDistanceMax);
    }
    
    private void LookAtPlayer()
    {
        float distanceToPlayer = _playerTransform.position.x - transform.position.x;

        if (distanceToPlayer <= 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void ChangeFacingDirectionByVelocity()
    {
        if (Mathf.Abs(_navMeshAgent.velocity.x) > 0.001f)
        {
            float target = (_navMeshAgent.velocity.x < 0) ? 180f: 0f;
            transform.rotation = Quaternion.Euler(0f, target, 0f);
        }
    }
}
