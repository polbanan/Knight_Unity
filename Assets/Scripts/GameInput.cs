using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private PlayerInputAction _playerInputAction;

    public event EventHandler OnPlayerAttack;

    private void Awake()
    {
        Instance = this;
        _playerInputAction = new PlayerInputAction();

    }
    private void Start()
    {
        _playerInputAction.Enable();
        _playerInputAction.Combat.Attack.performed += PlayerAttack_started;
        
    }
    private void PlayerAttack_started(InputAction.CallbackContext obj)
    {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = _playerInputAction.Player.Move.ReadValue<Vector2>();

        return inputVector;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }
}
