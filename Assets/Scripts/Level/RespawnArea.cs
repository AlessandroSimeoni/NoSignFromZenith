using System;
using Character;
using GauntletActions;
using PickableObjects;
using UnityEngine;

namespace Level
{
    public class RespawnArea : MonoBehaviour
    {
        [System.Serializable]
        public struct CubePlacement
        {
            public GameObject cube;
            public Vector3 destinationPosition;
        }

        [SerializeField] private LayerMask cubeLayers;
        [SerializeField] private CubePlacement[] cubePlacements;
        [SerializeField] private Vector3 playerPosition;

        private CharacterMovement characterMovement = null;
        private GauntletManager gauntletManager = null;

        private void OnTriggerEnter(Collider other)
        {
            if (cubeLayers == (cubeLayers | (1 << other.gameObject.layer)))
            {
                // if the object is grabbed, invoke the release
                if (other.gameObject.layer == LayerMask.NameToLayer("GrabbedObject"))
                {
                    PickableObject pickableObject = other.gameObject.GetComponent<PickableObject>();
                    pickableObject.invokeRelease.Invoke();
                }

                // reset rigidbody velocities
                other.attachedRigidbody.velocity = Vector3.zero;
                other.attachedRigidbody.angularVelocity = Vector3.zero;

                // Find the index of the cube in the array
                int cubeIndex = Array.FindIndex(cubePlacements, x => x.cube == other.gameObject);
                // Position the cube
                other.attachedRigidbody.MovePosition(cubePlacements[cubeIndex].destinationPosition);
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // get and cache components
                characterMovement = (characterMovement == null) ? other.gameObject.GetComponent<CharacterMovement>() : characterMovement;
                gauntletManager = (gauntletManager == null) ? other.gameObject.GetComponentInChildren<GauntletManager>() : gauntletManager;

                // release the grabbed object if any
                gauntletManager.ForceObjectRelease();
                //  teleport the player
                characterMovement.Teleport(playerPosition);
            }
        }
    }
}