using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Harti.Pattern
{
    public class Beat : MonoBehaviour
    {
        public enum BeatMode
        {
            normal,
            hit,
            smash
        }

        [HideInInspector] public BeatMode beatMode;

        public int beatType = -1;
        public int xPosition;
        public int yPosition;

        public Collider beatCollider;
        public GameObject beatNormal;
        public GameObject beatHover;

        public Animator animator; 

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Hover()
        {
            beatMode = BeatMode.hit;

            beatNormal.SetActive(false);
            beatHover.SetActive(true);

            animator.SetBool("Hover", true);
        }

        public void UnHover()
        {
            beatMode = BeatMode.normal;

            beatNormal.SetActive(true);
            beatHover.SetActive(false);

            animator.SetBool("Hover", false);
        }

        public void Smash()
        {
            beatMode = BeatMode.smash;

            beatNormal.SetActive(false);
            beatHover.SetActive(false);
            beatCollider.enabled = false; 
        }
    }
}