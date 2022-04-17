using UnityEngine;

namespace AntonsCameraController
{
    public abstract class ComponentBase : MonoBehaviour
    {
        public Transform T { get; private set; }

        public GameObject GO { get; private set; }

        private void Awake()
        {
            T = transform;
            GO = gameObject;
            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }
        
        protected virtual void OnDestroy() { }

        protected virtual void OnAwake() { }

        protected virtual void OnStart() { }

    }
}
