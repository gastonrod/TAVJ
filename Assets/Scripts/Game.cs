using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    public bool EnableServer;
    public bool EnableClient;

    public int Tickrate;
    public int Framerate;

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
        Application.targetFrameRate = Framerate;
        
        if (EnableServer)
        {
            _serverGame = new Server.Game(ServerPlayerPrefab, ServerPort, Tickrate);
            _serverGame.Start();
        }

        if (EnableClient)
        {
            _clientGame = new Client.Game(ClientPlayerPrefab, ServerAddress, ServerPort, ClientPort, Tickrate);
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
    
    void FixedUpdate()
    {
        if (EnableServer)
        {
            _serverGame.FixedUpdate();
        }

        if (EnableClient)
        {
            _clientGame.FixedUpdate();
        }
    }
}
