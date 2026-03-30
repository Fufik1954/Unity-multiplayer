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
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private TMP_Text _respawnTimerText;
    [SerializeField] private Transform _uiCanvas;

    private PlayerShooting _playerShooting;

    private void Awake()
    {
        _playerShooting = GetComponent<PlayerShooting>();
    }

    // OnNetworkSpawn вызывается, когда объект появился в сети
    public override void OnNetworkSpawn()
    {
        // Когда ник меняется вызываем OnNicknameChanged
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;

        // Когда здоровье меняется вызываем OnHpChanged
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        _playerNetwork.Ammo.OnValueChanged += OnAmmoChanged;

        _playerNetwork.RespawnTime.OnValueChanged += OnRespawnTimeChanged;

        // Сразу добавляем текущее состояние, чтобы UI не ждал первого сетевого события
        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);

        AdjustUIToOwner();

        if (!IsOwner)
        {
            _ammoText.gameObject.SetActive(false);
        }

    }

    // OnNetworkDespawn вызывается, когда объект исчезает из сети 
    public override void OnNetworkDespawn()
    {
        // Отписка обязательна, чтобы не оставлять "висячие" обработчики, объект может остаться в памяти или вызывать ошибки
        _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged -= OnHpChanged;
        _playerNetwork.RespawnTime.OnValueChanged -= OnRespawnTimeChanged;
        _playerNetwork.Ammo.OnValueChanged -= OnAmmoChanged;
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

    private void AdjustUIToOwner()
    {
        if (IsOwner)
        {
            _uiCanvas.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            _uiCanvas.localRotation = Quaternion.identity;
        }
    }
    private void OnAmmoChanged(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            _ammoText.text = $"Ammo: {newValue}";
        }
    }
    private void OnRespawnTimeChanged(float oldValue, float newValue)
    {
        if (!IsOwner)
        {
            _respawnTimerText.gameObject.SetActive(false);
            return;
        }

        if (newValue > 0)
        {
            // Показываем таймер и обновляем текст
            _respawnTimerText.gameObject.SetActive(true);
            int seconds = Mathf.CeilToInt(newValue);
            _respawnTimerText.text = $"Respawn: {seconds}s";
        }
        else
        {
            // Скрываем таймер
            _respawnTimerText.gameObject.SetActive(false);
        }
    }
}