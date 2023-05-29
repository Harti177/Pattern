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

        [ContextMenu("Arrange")]
        public GameObject[][] ArrangeGame()
        {
            if (instantiatedPrefabs != null)
            {
                foreach (GameObject go in instantiatedPrefabs)
                {
                    Destroy(go);
                }
            }

            GameObject[][] holderGOs = new GameObject[yCount][];
            int[] holder = new int[xCount * yCount];
            int index;

            System.Random random = new System.Random(seed);
            for (int a = 1; a < prefabsLimit.Length; a++)
            {
                for (int b = 0; b < prefabsLimit[a]; b++)
                {
                    index = random.Next(0, holder.Length - 1);
                    holder[index] = a;
                }
            }

            instantiatedPrefabs = new List<GameObject>();

            index = 0;
            for (int j = 0; j < yCount; j++)
            {
                holderGOs[j] = new GameObject[xCount];

                for (int i = 0; i < xCount; i++)
                {

                    float rad = Mathf.PI / 180 * (i * (360 / xCount));
                    float x = (radius * Mathf.Cos(rad)) + origin.x;
                    float y = (radius * Mathf.Sin(rad)) + origin.z;
                    Vector3 position = new Vector3(x, j * height, y);

                    int prefabsIndex = holder[index] == 0 ? 0 : holder[index];

                    GameObject go = Instantiate(prefabs[prefabsIndex], position, Quaternion.identity, transform);
                    instantiatedPrefabs.Add(go);
                    holderGOs[j][i] = go; 
                    index++;
                }
            }

            return holderGOs;
        }
    }
}
