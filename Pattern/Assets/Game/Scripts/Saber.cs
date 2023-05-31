using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

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

        [HideInInspector] public UnityEvent<GameObject, HandType> onCollisionEnter;
        [HideInInspector] public UnityEvent<GameObject, HandType> onCollisionExit;

        public Transform tip; 

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
            onCollisionEnter.Invoke(other.gameObject, handType); 
        }

        private void OnTriggerExit(Collider other)
        {
            onCollisionExit.Invoke(other.gameObject, handType);
        }

        public void Activate()
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * 2f);
        }

        public void DeActivate()
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * 1f);
        }

        public void Lock()
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.80f, 0.40f, 0f) * 2f);
        }
    }
}