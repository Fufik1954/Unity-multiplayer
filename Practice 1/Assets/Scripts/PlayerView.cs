using TMPro;           
using Unity.Collections; 
using Unity.Netcode;    
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;      
    [SerializeField] private TMP_Text _hpText;    


    // OnNetworkSpawn вызывается, когда объект появился в сети
    public override void OnNetworkSpawn()
    {
        // Когда ник меняется вызываем OnNicknameChanged
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;

        // Когда здоровье меняется вызываем OnHpChanged
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        // Сразу добавляем текущее состояние, чтобы UI не ждал первого сетевого события
        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);

    }

    // OnNetworkDespawn вызывается, когда объект исчезает из сети 
    public override void OnNetworkDespawn()
    {
        _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged -= OnHpChanged;
    }

    // Обработчик изменения ника
    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        // ToString() преобразует FixedString32Bytes в обычную строку для TextMeshPro
        _nicknameText.text = newValue.ToString();
    }

    // Обработчик изменения здоровья
    private void OnHpChanged(int oldValue, int newValue)
    {
        _hpText.text = $"HP: {newValue}";
    }
}