using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private const string IS_RUNNING = "IsRunning";
    private const string TAKE_HIT = "TakeHit";
    private const string DEATH = "Death";
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool _isDied = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Player.Instance.OnTakeHit += Player_OnTakeHit;
        Player.Instance.OnDeath += Player_OnDeath;
    }

    private void Update()
    {
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        AdjustPlayerFacingDirection();
    }

    private void Player_OnTakeHit(object sender, System.EventArgs e)
    {
        animator.SetTrigger(TAKE_HIT);
    }

    private void Player_OnDeath(object sender, System.EventArgs e)
    {
        animator.SetTrigger(DEATH);
        _isDied = true;
    }


    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();

        if (mousePos.x < playerPosition.x){
            spriteRenderer.flipX = true;
        }
        else { spriteRenderer.flipX = false; }
        if (_isDied)
            this.enabled = false;
    }
}
