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

        private Vector3 randomPosition; 
        private Vector3 toBePosition; 

        private void OnEnable()
        {
            randomPosition = GetRandomPoint();
            toBePosition = transform.position; 
            transform.position = randomPosition;
        }

        private void OnDisable()
        {
            
        }

        private Vector3 GetRandomPoint()
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            float normal = 1 / Mathf.Sqrt(x * x + y * y + z * z);
            return new Vector3(x* normal * 20f, y * normal * 20f, z * normal * 20f);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(Vector3.Distance(transform.position, toBePosition)> (2f * Time.deltaTime))
            {
                transform.position = Vector3.Lerp(transform.position, toBePosition, 2f * Time.deltaTime);
            }else if (Vector3.Distance(transform.position, toBePosition) > 0.001f)
            {
                transform.position = toBePosition;
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

        public void DeActivate()
        {
            beatMode = BeatMode.inactive;
            beatNormal.SetActive(false);
            beatCollider.enabled = false;
            GameObject beatSmashedGO = Instantiate(beatSmashed, transform.position, Quaternion.identity);
            beatSmashedGO.transform.parent = transform;
        }

        public void GameOver()
        {
            beatSmashedEffectGO = Instantiate(beatSmashedEffect, transform.position, Quaternion.identity);

            GameObject.Find("SmashSound").GetComponent<AudioSource>().Play();

            StartCoroutine(DestroyGameOverEffect());
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

        private IEnumerator DestroyGameOverEffect()
        {
            yield return new WaitForSeconds(0.5f);

            beatNormal.SetActive(false);

            Destroy(beatSmashedEffectGO);
        }
    }
}