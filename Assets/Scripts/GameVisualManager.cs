using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour {
    // use network behavior instead of monobehavior so rpc calls can be made
    private const float GRID_SIZE_X = 4.5f;
    private const float GRID_SIZE_Y = 3.75f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start() {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e) {
        Debug.Log("GameManager_OnClickedOnGridPosition");
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    // client must ask server permission first in order to spawn a move on the grid
    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType) {
        Debug.Log("SpawnObject");

        // depending on if it is a host or client, assign x or o prefab
        Transform prefab;

        switch (playerType) {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;

            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }

        // Note: by adding network transform component to the prefabs, it will sync this transform for both the client and the host
        Transform spawnTransform = Instantiate(
            prefab,
            GetGridWorldPosition(x, y),
            Quaternion.identity
        );

        // makes sure the prefab spawns across the network so that it is seen by all clients
        spawnTransform.GetComponent<NetworkObject>().Spawn(true);

        Debug.Log("RPC received playerType: " + playerType);
    }

    private Vector2 GetGridWorldPosition(int x, int y) {
        // calculate the position to spawn the Xs and Os at
        return new Vector2(
            -GRID_SIZE_X + (x + 1) * GRID_SIZE_X,
            -GRID_SIZE_X + y * GRID_SIZE_Y
        );
    }
}
