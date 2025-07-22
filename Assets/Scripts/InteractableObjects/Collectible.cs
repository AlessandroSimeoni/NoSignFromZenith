using Audio;
using UnityEngine;

namespace Interactable
{
    [System.Serializable]   // da togliere?
    public class Collectibles
    {
        public RegistryID ID;
        public int subIndex;
        public bool hasBeenRead = false;
    }


    public class Collectible : InteractableObject
    {
        [SerializeField] private RegistryID ID;
        [SerializeField, Min(0)] private int subIndex;
        [SerializeField] private AudioClip collectSFX;
        [Header("Rotation")]
        [SerializeField] private float degreesPerSecond = 5.0f;


        private void Start()
        {
            // destroy the collectible if the player already got it
            int index = Database.collectibles.FindIndex(c => c.ID == this.ID && c.subIndex == this.subIndex);
            if (index >= 0)
                Destroy(this.gameObject);
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime);
        }

        public override void Interact()
        {
            Collectibles collectible = new Collectibles { ID = this.ID, subIndex = this.subIndex};

            // add to the database list this collectible
            Database.collectibles.Add(collectible);

            //play sfx
            AudioPlayer.PlaySFX(collectSFX, transform.position);

            //destroy this gameobject
            Destroy(gameObject);
        }
    }
}
