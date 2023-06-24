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
            smash,
            inactive
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

        public float counter;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            counter += Time.deltaTime;

            if (counter > 50)
            {
                if (beatMode == BeatMode.normal)
                {
                    beatMode = BeatMode.inactive;
                    beatNormal.SetActive(false);
                    GameObject beatSmashedGO = Instantiate(beatSmashed, transform.position, Quaternion.identity);
                }
            }
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
            if (beatMode == BeatMode.smash) return;

            beatMode = BeatMode.smash;

            beatNormal.SetActive(false);

            GameObject.Find("SmashSound").GetComponent<AudioSource>().Play();

            beatSmashedEffectGO = Instantiate(beatSmashedEffect, transform.position, Quaternion.identity);

            GameObject beatSmashedGO = Instantiate(beatSmashed, transform.position, Quaternion.identity);
            hulls = beatSmashedGO.SliceInstantiate(position, direction, beatSmashed.GetComponent<MeshRenderer>().material);
            Destroy(beatSmashedGO);

            if(hulls != null && hulls.Length > 0)
            {
                hulls[0].AddComponent<MeshCollider>().convex = true;
                hulls[0].AddComponent<Rigidbody>().AddExplosionForce(200f, position, 1);
                if(hulls.Length > 1) hulls[1].AddComponent<MeshCollider>().convex = true;
                if (hulls.Length > 1)  hulls[1].AddComponent<Rigidbody>().AddExplosionForce(200f, position, 1);
                StartCoroutine(DestroyHulls());
            }
        }

        private IEnumerator DestroyHulls()
        {
            yield return new WaitForSeconds(2f);

            List<GameObject> gos = new List<GameObject>();

            for(int i = 0; i < hulls.Length; i++)
            {
                if(hulls[i] != null)
                    gos.Add(hulls[i]);
            }

            foreach (GameObject go in gos)
            {
                Destroy(go);
            }

            Destroy(beatSmashedEffectGO);
        }
    }
}