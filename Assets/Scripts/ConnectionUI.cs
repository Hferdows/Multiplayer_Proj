using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionUI : MonoBehaviour {

    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject joinPanel;

    private const ushort PORT = 7777;

    private void Start() {
        mainPanel.SetActive(true);
        joinPanel.SetActive(false);
    }

    public void ShowJoinPanel() {
        mainPanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void ShowMainPanel() {
        mainPanel.SetActive(true);
        joinPanel.SetActive(false);
    }

//host is always from 0.0.0.0
    public void HostGame() {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData("0.0.0.0", PORT);

        bool success = NetworkManager.Singleton.StartHost();

        if (!success) {
            Debug.LogError("Failed to start host.");
        }
        else {
            Debug.Log("Host started successfully.");
            gameObject.SetActive(false);
            mainPanel.SetActive(false);
        }
    }
//client eneters host IP address in order to join 
    public void JoinGame() {
        string ipAddress = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ipAddress)) {
            Debug.LogWarning("Still must enter the host IP address");
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, PORT);
        bool success = NetworkManager.Singleton.StartClient();

        if (!success) {
            Debug.LogError("Failed to start client");
        }
        else {
            Debug.Log("Client is trying to connect to host at: " + ipAddress);
            gameObject.SetActive(false);
            joinPanel.SetActive(false);
        }
    }
}
