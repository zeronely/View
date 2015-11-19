using UnityEngine;
using System.Collections;

namespace Spate
{
    public class BaseBehaviour : MonoBehaviour
    {
        private Transform _transform;
        private GameObject _gameObject;

        public Transform CachedTransform
        {
            get
            {
                if (_transform == null)
                    _transform = transform;
                return _transform;
            }
        }

        public GameObject CachedGameObject
        {
            get
            {
                if (_gameObject == null)
                    _gameObject = gameObject;
                return _gameObject;
            }
        }
    }
}