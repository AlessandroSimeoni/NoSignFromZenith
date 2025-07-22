using UnityEngine;

namespace UI
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] protected GameObject screenElements;

        public virtual void ShowScreenElements()
        {
            screenElements.SetActive(true);
        }

        public virtual void HideScreenElements()
        {
            screenElements.SetActive(false);
        }
    }
}