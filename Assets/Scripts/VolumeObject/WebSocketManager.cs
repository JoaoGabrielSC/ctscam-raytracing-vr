using UnityEngine;
using WebSocketSharp;

public class WebSocketManager : MonoBehaviour
{
    WebSocket ws;
    public Transform cube;

    void Start()
    {
        string websocketUrl = EnvLoader.Get("WEBSOCKET_URL");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Recebido: " + e.Data);
            APIData data = JsonUtility.FromJson<APIData>(e.Data);
            UpdateObject(data);
        };
        ws.Connect();
    }

    void UpdateObject(APIData data)
    {
        cube.position = new Vector3(data.position_x, data.position_y, data.position_z);
        cube.localScale = new Vector3(data.width, data.height, data.length);
        cube.rotation = Quaternion.Euler(data.pitch, data.yaw, data.row);
    }

    public void SendUpdate()
    {
        APIData data = new APIData
        {
            position_x = cube.position.x,
            position_y = cube.position.y,
            position_z = cube.position.z,
            width = cube.localScale.x,
            height = cube.localScale.y,
            length = cube.localScale.z,
            pitch = cube.rotation.eulerAngles.x,
            yaw = cube.rotation.eulerAngles.y,
            row = cube.rotation.eulerAngles.z,
            id = 1
        };

        string jsonData = JsonUtility.ToJson(data);
        ws.Send(jsonData);
    }

    void OnDestroy()
    {
        ws.Close();
    }
}

[System.Serializable]
public class APIData
{
    public float position_x;
    public float position_y;
    public float position_z;
    public float width;
    public float height;
    public float length;
    public float pitch;
    public float row;
    public float yaw;
    public int id;
}
