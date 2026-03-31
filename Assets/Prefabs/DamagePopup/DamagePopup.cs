using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    public TextMeshPro damageText;
    private float _lifeTime = 0.5f;
    private Vector3 _moveVector = new Vector3(0.5f, 1f);

    public void Setup(int damageAmount)
    {
        damageText.text = damageAmount.ToString();
    }

    private void Update()
    {
        transform.position += _moveVector * Time.deltaTime;
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
