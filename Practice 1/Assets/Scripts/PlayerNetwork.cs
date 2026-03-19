using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Получаем ник из статической переменной и отправляем на сервер
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
}