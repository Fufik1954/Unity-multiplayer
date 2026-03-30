using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    // Ник должен быть виден всем клиентам, но менять его может только сервер
    [Header("Network Variables")]
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        "Player",  // значение по умолчанию
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // HP тоже читает каждый клиен, но изменяется только на сервере
    public NetworkVariable<int> HP = new(
        100,  // стартовое здоровье
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> RespawnTime = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Ammo = new(
       10,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
    );


    public override void OnNetworkSpawn()
    {
        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;
       
        if (IsOwner)
        {
            // Получаем ник из статической переменной и отправляем на сервер
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }
    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }
    private void OnHpChanged(int prev, int next)
    {
        // Только сервер запускает цикл смерти
        if (!IsServer) return;
        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (next == false)
        {
            // Игрок умер - скрываем
            HidePlayer();
        }
        else
        {
            // Игрок ожил - показываем
            ShowPlayer();
        }
    }

    private void HidePlayer()
    {
        // Отключаем визуальную модель
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Отключаем коллайдер (чтобы не попадали)
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // Отключаем CharacterController (чтобы не двигался)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
    }

    private void ShowPlayer()
    {
        // Включаем визуальную модель
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        // Включаем коллайдер
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        // Включаем CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
    private IEnumerator RespawnRoutine()
    {
        float timer = 3f;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            RespawnTime.Value = timer;  //Отправляем текущее время всем клиентам
            yield return null;
        }

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Перемещаем в точку респавна
        int idx = Random.Range(0, spawnPoints.Length);
        Vector3 newPosition = spawnPoints[idx].transform.position;

        TPPlayerClientRpc(newPosition);

        //transform.position = newPosition;

        // Восстанавливаем здоровье
        HP.Value = 100;

        // Оживляем игрока
        //IsAlive.Value = true;
    }

    [ClientRpc]
    private void TPPlayerClientRpc(Vector3 spawnPosition)
    {
        // Только владелец этого игрока телепортируется
        if (IsOwner)
        {
            transform.position = spawnPosition;
            Debug.Log($"Player {OwnerClientId} teleported to {spawnPosition}");
            IsAlive.Value = true;
        }
    }
}