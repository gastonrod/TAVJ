using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyController : MonoBehaviour
    {
        
        private GameObject _player;
        private CharacterController _controller;

        private int _maxMove = 3;
        // Start is called before the first frame update
        void Start()
        {
            _player = GameObject.FindWithTag("Player");
            gameObject.AddComponent<CharacterController>();
            _controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 move =  _player.transform.position - gameObject.transform.position;
            move.Normalize();
            _controller.Move(Time.deltaTime * 5 * move);
        }

    }
}