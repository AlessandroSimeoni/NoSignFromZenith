using Audio;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Interactable
{
    public class Database : InteractableObject
    {
        [SerializeField] private UIDatabase uIDatabase;
        [SerializeField] private AudioClip jingleSFX;

        public static List<Collectibles> collectibles = new List<Collectibles>();

        public override void Interact()
        {
            uIDatabase.ShowScreenElements();
        }

        public IEnumerator Jingle()
        {
            yield return new WaitForSeconds(4.0f);
            AudioPlayer.PlaySFX(jingleSFX, transform.position, 0.0f);
        }
    }
}