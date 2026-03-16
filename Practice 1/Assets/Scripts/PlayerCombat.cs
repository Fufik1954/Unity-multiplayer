using Unity.Netcode;
using UnityEngine;

public class PlayerCombatRaycast : NetworkBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private LayerMask _playerLayer;

    private void Awake()
    {
        _playerLayer = LayerMask.GetMask("Player");
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // Пускаем луч вперед от игрока (текущая позиция + направление)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _attackRange, _playerLayer))
        {
            // Если попали, то получаем компонент PlayerNetwork у объекта
            PlayerNetwork target = hit.collider.GetComponent<PlayerNetwork>();
            DealDamageServerRpc(target.NetworkObjectId, _damage);
        }
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        // Сервер проверяет, существует ли цель среди заспавненных сетевых объектов
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
        {
            PlayerNetwork target = targetObject.GetComponent<PlayerNetwork>();
            if (target != null && target != _playerNetwork)
            {
                int nextHp = Mathf.Max(0, target.HP.Value - damage);
                target.HP.Value = nextHp;
                Debug.Log("УДАР");
            }
        }
    }

    // Визуализация луча
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * _attackRange);
    }
}