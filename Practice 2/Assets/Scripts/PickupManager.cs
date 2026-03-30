using Unity.Netcode;
using UnityEngine;
using System.Collections;

// PickupManager Ч обычный MonoBehaviour, работает только на сервере
public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    private void Start()
    {
        StartCoroutine(WaitForServer());
    }

    private IEnumerator WaitForServer()
    {
        // ∆дем, пока NetworkManager станет сервером
        while (!NetworkManager.Singleton.IsServer)
        {
            yield return null;  // ∆дем 1 кадр
        }

        // “еперь IsServer = true!
        SpawnAll();
    }


    private void SpawnAll()
    {
        foreach (var point in _spawnPoints)
            SpawnPickup(point.position);
        Debug.Log("SpawnALL");
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        go.GetComponent<HealthPickup>().Init(this);
        go.GetComponent<NetworkObject>().Spawn();
    }
}