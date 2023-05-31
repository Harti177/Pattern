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

        public LineRenderer lineRenderer; 

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
            if (beatMode != BeatMode.smash) beatMode = BeatMode.hit;

            animator.SetBool("Hover", true);

            GameObject.Find("HoverSound").GetComponent<AudioSource>().Play();
        }

        public void UnHover()
        {
            if(beatMode != BeatMode.smash) beatMode = BeatMode.normal;

            animator.SetBool("Hover", false);

            GameObject.Find("UnHoverSound").GetComponent<AudioSource>().Play();
        }

        public void Smash()
        {
            beatMode = BeatMode.smash;

            beatNormal.SetActive(false);
            //beatCollider.enabled = false; 

            GameObject.Find("SmashSound").GetComponent<AudioSource>().Play();
        }
    }
}