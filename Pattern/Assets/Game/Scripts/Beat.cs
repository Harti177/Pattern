using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

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
        public GameObject beatSmashed;
        public GameObject beatSmashedEffect;

        public Animator animator;

        private GameObject[] hulls; 
        private GameObject beatSmashedEffectGO; 

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

        public void Smash(Vector3 position, Vector3 direction)
        {
            beatMode = BeatMode.smash;

            beatNormal.SetActive(false);

            GameObject.Find("SmashSound").GetComponent<AudioSource>().Play();

            beatSmashedEffectGO = Instantiate(beatSmashedEffect, transform.position, Quaternion.identity);

            GameObject beatSmashedGO = Instantiate(beatSmashed, transform.position, Quaternion.identity);
            GameObject[] hulls = beatSmashedGO.SliceInstantiate(position, direction, beatSmashed.GetComponent<MeshRenderer>().material);
            Destroy(beatSmashedGO);

            hulls[0].AddComponent<MeshCollider>().convex = true;
            hulls[0].AddComponent<Rigidbody>().AddExplosionForce(200f, position, 1);
            hulls[1].AddComponent<MeshCollider>().convex = true;
            hulls[1].AddComponent<Rigidbody>().AddExplosionForce(200f, position, 1);
            StartCoroutine(DestroyHulls());
        }

        private IEnumerator DestroyHulls()
        {
            yield return new WaitForSeconds(2f);

            Destroy(hulls[0]);
            Destroy(hulls[1]);
            Destroy(beatSmashedEffectGO);
        }
    }
}