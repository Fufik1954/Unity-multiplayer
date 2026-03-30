using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;

    private float _lastShotTime;
    private int _currentAmmo;

    private PlayerNetwork _playerNetwork;

    public override void OnNetworkSpawn()
    {
        if (_playerNetwork == null)
        {
            _playerNetwork = GetComponent<PlayerNetwork>();
        }

        if (IsServer)
        {
            _playerNetwork.Ammo.Value = _maxAmmo; 
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        if (!_playerNetwork.IsAlive.Value) return;

        // 1. Жив ли игрок?
        if (_playerNetwork.HP.Value <= 0)
        {
            return;
        }

        // 2. Есть ли патроны?
        if (_playerNetwork.Ammo.Value <= 0)
        {
            return;
        }

        // 3. Прошёл ли кулдаун?
        if (Time.time < _lastShotTime + _cooldown)
        {
            return;
        }

        _lastShotTime = Time.time;
        _playerNetwork.Ammo.Value--;

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f,
                             Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();

        no.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}