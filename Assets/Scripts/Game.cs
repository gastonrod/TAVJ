using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    public bool EnableServer;
    public bool EnableClient;

    public String ServerAddress;
    public short ServerPort;
    public short ClientPort;

    public GameObject ServerPlayerPrefab;
    public GameObject ClientPlayerPrefab;

    private Server.Game _serverGame;
    private Client.Game _clientGame;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        
        if (EnableServer)
        {
            _serverGame = new Server.Game(ServerPlayerPrefab, ServerPort);
            _serverGame.Start();
        }

        if (EnableClient)
        {
            _clientGame = new Client.Game(ClientPlayerPrefab, ServerAddress, ServerPort, ClientPort);
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
