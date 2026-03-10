using Unity.Netcode;
using System;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    
    public static GameManager Instance { get; private set; }

    private PlayerType localPlayerType;

    // autosynchronized for everyone playing when using network variable
    private NetworkVariable<PlayerType> currentTurnPlayer = new NetworkVariable<PlayerType>();

    // array to keep track of where tiles are placed, and also if they equal 3 in a row
    private PlayerType[,] playerTypeArray;

    public enum PlayerType {
        None,
        Cross,
        Circle,
    }

    public event EventHandler OnCurrentPlayerChange;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnGameWinEventArgs : EventArgs {
        public Vector2Int centerGridPosition;
        public PlayerType winPlayerType;
    }

    // when grid is clicked make sure to trigger event and index the coordinates and which player clicked it
    public class OnClickedOnGridPositionEventArgs : EventArgs {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    private void Awake() {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance!");
        }

        Instance = this;
        playerTypeArray = new PlayerType[3, 3];
    }

    // when a player spawns onto the network, make them either a cross (if network Id = 0) or player circle (if equals 1)
    public override void OnNetworkSpawn() {
        if (NetworkManager.Singleton.LocalClientId == 0) {
            localPlayerType = PlayerType.Cross;
        }
        else {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer) {
            currentTurnPlayer.Value = PlayerType.Cross; // START GAME
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentTurnPlayer.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) => {
            OnCurrentPlayerChange?.Invoke(this, EventArgs.Empty);
        };

        Debug.Log("Assigned PlayerType: " + localPlayerType);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj) {
        // start the game once both players are connected
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2) {
        }
    }

    // only server can approve player moves when they click the grid and if it is their turn to do so
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType) {
        Debug.Log("ClickedOnGridPositionRpc " + x + ", " + y);

        // check to make sure it is the players turn before allowing them to place a piece
        if (currentTurnPlayer.Value == PlayerType.None) {
            return;
        }
        
        if (playerType != currentTurnPlayer.Value) {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None) {
            //already occupied
            return;
        }

        playerTypeArray[x, y] = playerType;

        // index info about the move player has made when click happens
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs {
            x = x,
            y = y,
            playerType = playerType,
        });

        // update the current turn player to the other player after move is taken
        switch (currentTurnPlayer.Value) {
            default:
            case PlayerType.Cross:
                currentTurnPlayer.Value = PlayerType.Circle;
                break;

            case PlayerType.Circle:
                currentTurnPlayer.Value = PlayerType.Cross;
                break;
        }

        DetectWin();
    }

//Detect wins whether they are 3 in a row horizontally, vertically or diagonally 
    private void DetectWin() {
        //bottom row 3 in a row 
        if (playerTypeArray[0,0] != PlayerType.None &&
            playerTypeArray[0,0] == playerTypeArray[1,0] &&
            playerTypeArray[1,0] == playerTypeArray[2,0]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,0], 1, 0);
            return;
        }

        //middle row 3 in a row 
        if (playerTypeArray[0,1] != PlayerType.None &&
            playerTypeArray[0,1] == playerTypeArray[1,1] &&
            playerTypeArray[1,1] == playerTypeArray[2,1]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,1], 1, 1);
            return;
        }

        //top row 3 in a row 
        if (playerTypeArray[0,2] != PlayerType.None &&
            playerTypeArray[0,2] == playerTypeArray[1,2] &&
            playerTypeArray[1,2] == playerTypeArray[2,2]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,2], 1, 2);
            return;
        }

        //left column vertical
        if (playerTypeArray[0,0] != PlayerType.None &&
            playerTypeArray[0,0] == playerTypeArray[0,1] &&
            playerTypeArray[0,1] == playerTypeArray[0,2]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,0], 0, 1);
            return;
        }

    //middle column verical
        if (playerTypeArray[1,0] != PlayerType.None &&
            playerTypeArray[1,0] == playerTypeArray[1,1] &&
            playerTypeArray[1,1] == playerTypeArray[1,2]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[1,0], 1, 1);
            return;
        }

        //right column vertical
        if (playerTypeArray[2,0] != PlayerType.None &&
            playerTypeArray[2,0] == playerTypeArray[2,1] &&
            playerTypeArray[2,1] == playerTypeArray[2,2]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[2,0], 2, 1);
            return;
        }

        //diagonal bottom-left to top-right
        if (playerTypeArray[0,0] != PlayerType.None &&
            playerTypeArray[0,0] == playerTypeArray[1,1] &&
            playerTypeArray[1,1] == playerTypeArray[2,2]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,0], 1, 1);
            return;
        }

    //diagonal top-left to bottom-right
        if (playerTypeArray[0,2] != PlayerType.None &&
            playerTypeArray[0,2] == playerTypeArray[1,1] &&
            playerTypeArray[1,1] == playerTypeArray[2,0]) {

            currentTurnPlayer.Value = PlayerType.None;
            ShowGameOverRpc(playerTypeArray[0,2], 1, 1);
            return;
        }
    }

    //when a win is detected send trigger game over UI for both ends 
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowGameOverRpc(PlayerType winner, int centerX, int centerY) {
        OnGameWin?.Invoke(this, new OnGameWinEventArgs {
            centerGridPosition = new Vector2Int(centerX, centerY),
            winPlayerType = winner
        });
    }    

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentTurnPlayer()
    {
        return currentTurnPlayer.Value;
    }
}