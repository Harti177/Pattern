using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Harti.Pattern
{
    public class Saber : MonoBehaviour
    {
        public enum HandType
        {
            left,
            right
        }

        public HandType handType;

        public Game game;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        }

        private void OnTriggerEnter(Collider other)
        {
            game.CheckSaberCollisionEnter(other.gameObject, handType);
        }

        private void OnTriggerExit(Collider other)
        {
            game.CheckSaberCollisionExit(other.gameObject, handType);
        }
    }

}