using _Project.Scripts.Utility;
using UnityEngine;

public class Match3Camera : MonoBehaviour
{
    private Camera _gameCamera;

    private void Start()
    {
        _gameCamera = this.GetComponent<Camera>();
        SetupCameraView();
    }

    private void SetupCameraView()
    {
        _gameCamera.transform.position = new Vector3((float)(Constants. BOARD_WIDTH  - 1)/ 2f,
            (float)(Constants.BOARD_HEIGHT -1)/ 2f,-10f);
        float ar = (float)Screen.width / (float)Screen.height;
        float vertSize = (float)Constants.BOARD_HEIGHT / 2f + (float)Constants.BORDER_SIZE;
        float horizSize = ((float)Constants. BOARD_WIDTH  / 2f + (float)Constants.BORDER_SIZE) / ar;

        _gameCamera.orthographicSize = (vertSize > horizSize) ? vertSize : horizSize;
    }
}
