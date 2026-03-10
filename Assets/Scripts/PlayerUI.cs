using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject circleTurnText;
    [SerializeField] private GameObject crossTurnText;

    private void Awake() {
        circleTurnText.SetActive(false);
        crossTurnText.SetActive(false);
    }

    private void Start() {
        GameManager.Instance.OnCurrentPlayerChange += GameManager_OnCurrentPlayerChange;
    }

    // when player turn changes, activate the "your turn" UI
    private void GameManager_OnCurrentPlayerChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetCurrentTurnPlayer() == GameManager.PlayerType.Cross) {
            crossTurnText.SetActive(true);
            circleTurnText.SetActive(false);
        }
        else {
            circleTurnText.SetActive(true);
            crossTurnText.SetActive(false);
        }
    }
}
