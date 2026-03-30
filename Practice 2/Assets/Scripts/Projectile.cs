using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Только сервер обрабатывает попадания
        if (!IsServer) return;

        // Проверяем, что снаряд уже заспавнен
        if (!IsSpawned) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        // Не наносим урон самому себе
        if (target.OwnerClientId == OwnerClientId) return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

        // Уничтожаем снаряд
        NetworkObject.Despawn(destroy: true);
    }
}