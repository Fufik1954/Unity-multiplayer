using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private int _requiredPlayers = 2;
    [SerializeField] private ConnectionUI _connectionUI;
    [SerializeField] private float _matchDuration = 60f;

    [AllowMutableSyncType]
    public readonly SyncVar<GameState> CurrentState = new SyncVar<GameState>(GameState.WaitingForPlayers);

    [AllowMutableSyncType]
    public readonly SyncVar<int> ConnectedPlayers = new SyncVar<int>(0);

    [AllowMutableSyncType]
    public readonly SyncVar<float> MatchTimer = new SyncVar<float>(60f);

    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        ShowingResults
    }

    public int GetRequiredPlayers() => _requiredPlayers;

    private void Awake()
    {
        CurrentState.OnChange += OnGameStateChanged;
        MatchTimer.OnChange += OnMatchTimerChanged;
        ConnectedPlayers.OnChange += _connectionUI.OnConnectedPlayersChanged;
    }
    private void Update()
    {
        if (!base.IsServerInitialized) return;
        if (CurrentState.Value != GameState.InProgress) return;

        MatchTimer.Value -= Time.deltaTime;
        if (MatchTimer.Value <= 0f)
        {
            EndMatch();
        }
    }

    // Обработчик таймера
    private void OnMatchTimerChanged(float oldValue, float newValue, bool asServer)
    {
        _connectionUI.UpdateMatchTimer(newValue);
    }

    // Обработчик изменения состояния 
    private void OnGameStateChanged(GameState oldValue, GameState newValue, bool asServer)
    {
        switch (CurrentState.Value)
        {
            case GameState.WaitingForPlayers:
                _connectionUI.ShowLobbyPanel();
                break;
            case GameState.InProgress:
                _connectionUI.ShowGamePanel();
                break;
            case GameState.ShowingResults:
                _connectionUI.ShowResultsPanel(GetResultsText());
                break;
        }
    }

    private string GetResultsText()
    {
        string results = "";
        foreach (var player in FindObjectsOfType<PlayerNetwork>())
        {
            if (player != null)
                results += $"{player.Nickname.Value}: {player.Score.Value}\n";
        }
        return results;
    }

    // Инцииализация сервера
    public override void OnStartServer()
    {
        base.ServerManager.OnRemoteConnectionState += OnPlayerConnectionChanged;
        UpdateConnectedPlayers();
    }

    // Собыие подключения / отключения игроков
    private void OnPlayerConnectionChanged(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (!base.IsServerInitialized) return;
        UpdateConnectedPlayers();
    }

    // Обработчик количества игроков
    private void UpdateConnectedPlayers()
    {
        ConnectedPlayers.Value = base.ServerManager.Clients.Count;

        if (CurrentState.Value == GameState.WaitingForPlayers && ConnectedPlayers.Value >= _requiredPlayers)
        {
            Invoke(nameof(StartMatch), 1f);
        }
    }

    // Старт матча
    private void StartMatch()
    {
        CurrentState.Value = GameState.InProgress;
        MatchTimer.Value = _matchDuration;
    }

    // Конец игры
    private void EndMatch()
    {
        if (CurrentState.Value != GameState.InProgress) return;
        CurrentState.Value = GameState.ShowingResults;
        Invoke(nameof(ResetToLobby), 5f);
    }

    // Возврат в лобби
    private void ResetToLobby()
    {

        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            if (player != null)
            {
                player.HP.Value = 100;
                player.IsAlive.Value = true;
                player.Ammo.Value = 20;
                player.Score.Value = 0;
                player.RespawnTime.Value = 0f;
            }
        }

        MatchTimer.Value = _matchDuration;
        CurrentState.Value = GameState.WaitingForPlayers;
        UpdateConnectedPlayers();
    }
}