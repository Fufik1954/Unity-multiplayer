using TMPro;
using FishNet.Managing;     
using FishNet.Transporting;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _ipInput;
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private GameObject _lobbyPanel;     
    [SerializeField] private TMP_Text _lobbyStatusText;
    [SerializeField] private GameObject _resultsPanel;
    [SerializeField] private TMP_Text _resultsScoresText;
    [SerializeField] private TMP_Text _resultsTimerText;
    [SerializeField] private GameObject _gameInfoPanel;
    [SerializeField] private TMP_Text _matchTimerText;

    [SerializeField] private GameManager _gameManager;

    private float _resultsCountdown = 5f;
    private bool _isCountingDown = false;

    public static string PlayerNickname { get; private set; } = "Player";
    public static string ServerIP { get; private set; } = "localhost";

    public void StartAsClient()
    {
        SaveNickname();
        SaveServerIP();

        InstanceFinder.ClientManager.StartConnection(ServerIP, 7777);

        if (_connectionPanel != null)
            _connectionPanel.SetActive(false);
        if (_lobbyPanel != null)
            _lobbyPanel.SetActive(true);

    }
    private void Update()
    {
        if (_isCountingDown && _resultsPanel != null && _resultsPanel.activeSelf)
        {
            _resultsCountdown -= Time.deltaTime;

            if (_resultsTimerText != null)
            {
                int seconds = Mathf.Max(0, Mathf.CeilToInt(_resultsCountdown));
                _resultsTimerText.text = $"Возврат в лобби через {seconds}с...";
            }

            if (_resultsCountdown <= 0f)
            {
                _isCountingDown = false;
            }
        }
    }

    public void UpdateMatchTimer(float time)
    {
        if (_matchTimerText != null)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(time));
            _matchTimerText.text = $"Time: {seconds}";
        }
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void SaveServerIP()  
    {
        string rawValue = _ipInput != null ? _ipInput.text : string.Empty;
        ServerIP = string.IsNullOrWhiteSpace(rawValue) ? "localhost" : rawValue.Trim();
    }

    public void OnConnectedPlayersChanged(int oldValue, int newValue, bool asServer)
    {
        if (_lobbyStatusText != null && _gameManager != null)
        {
            _lobbyStatusText.text = $"Ожидание игроков: {newValue}/{_gameManager.GetRequiredPlayers()}";
           
        }
    }

    public void ShowLobbyPanel()
    {
        if (_resultsPanel != null) _resultsPanel.SetActive(false);
        if (_gameInfoPanel != null) _gameInfoPanel.SetActive(false);
        if (_lobbyPanel != null) _lobbyPanel.SetActive(true);
        _isCountingDown = false;
    }

    public void ShowGamePanel()
    {
        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
        if (_resultsPanel != null) _resultsPanel.SetActive(false);
        if (_gameInfoPanel != null) _gameInfoPanel.SetActive(true);
    }

    public void ShowResultsPanel(string resultsText)
    {
        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
        if (_gameInfoPanel != null) _gameInfoPanel.SetActive(false);
        if (_resultsPanel != null) _resultsPanel.SetActive(true);

        if (_resultsScoresText != null)
            _resultsScoresText.text = resultsText;

        _resultsCountdown = 5f;
        _isCountingDown = true;
    }
}