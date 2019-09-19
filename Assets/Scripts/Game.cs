using UnityEngine;

public class Game : MonoBehaviour
{
    public bool EnableServer;
    public bool EnableClient;

    public GameObject PlayerPrefab;
    
    private Server.Game _serverGame;
    private Client.Game _clientGame;
    
    // Start is called before the first frame update
    void Start()
    {
        if (EnableServer)
        {
            _serverGame = new Server.Game(PlayerPrefab);
            _serverGame.Start();
        }

        if (EnableClient)
        {
            _clientGame = new Client.Game(PlayerPrefab);
            _clientGame.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableServer)
        {
            _serverGame.Update();
        }

        if (EnableClient)
        {
            _clientGame.Update();
        }
    }
}
