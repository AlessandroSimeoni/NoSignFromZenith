using UnityEngine;

namespace Hub
{
    public class Sun : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 0.5f;

        void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime);
        }
    }
}