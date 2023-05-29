using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Harti.Pattern
{
    public class Arrange : MonoBehaviour
    {
        [SerializeField] private int xCount = 24;
        [SerializeField] private int yCount = 10;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float height = 0.25f;
        [SerializeField] private Vector3 origin = Vector3.zero;
        [SerializeField] private int seed = 5;

        public GameObject[] prefabs;
        public int[] prefabsLimit;

        List<GameObject> instantiatedPrefabs;

        // Start is called before the first frame update
        void Start()
        {
            ArrangeGame();
        }

        [ContextMenu("Arrange")]
        public void ArrangeGame()
        {
            if (instantiatedPrefabs != null)
            {
                foreach (GameObject go in instantiatedPrefabs)
                {
                    Destroy(go);
                }
            }

            int[] holder = new int[xCount * yCount];
            int index;

            System.Random random = new System.Random(seed);
            for (int i = 1; i < prefabsLimit.Length; i++)
            {
                for (int j = 0; j < prefabsLimit[i]; j++)
                {
                    index = random.Next(0, holder.Length - 1);
                    holder[index] = i;
                }
            }

            instantiatedPrefabs = new List<GameObject>();

            index = 0;
            for (int i = 0; i < yCount; i++)
            {
                for (int j = 0; j < xCount; j++)
                {

                    float rad = Mathf.PI / 180 * (j * (360 / xCount));
                    float x = (radius * Mathf.Cos(rad)) + origin.x;
                    float y = (radius * Mathf.Sin(rad)) + origin.z;
                    Vector3 position = new Vector3(x, i * height, y);

                    int prefabsIndex = holder[index] == 0 ? 0 : holder[index];

                    GameObject go = Instantiate(prefabs[prefabsIndex], position, Quaternion.identity, transform);
                    instantiatedPrefabs.Add(go);
                    index++;
                }
            }
        }
    }
}
