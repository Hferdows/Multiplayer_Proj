using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    private void Start() {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        gameObject.SetActive(false);
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e) {
        if(e.winPlayerType == GameManager.Instance.GetLocalPlayerType()) {
            resultTextMesh.text = "YOU WIN!";
            resultTextMesh.color = winColor;
        }
        else {
            resultTextMesh.text = "YOU LOSE!";
            resultTextMesh.color = loseColor;            
        }
         gameObject.SetActive(true);
    }
}
