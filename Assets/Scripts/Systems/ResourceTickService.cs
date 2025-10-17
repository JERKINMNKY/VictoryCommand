using System.Collections.Generic;
using UnityEngine;
using IFC.Data;

namespace IFC.Systems
{
    /// <summary>
    /// Adds resources to the city every in-game minute.
    /// </summary>
    public class ResourceTickService : MonoBehaviour
    {
        [System.Serializable]
        public class CityResource
        {
            public ResourceType type;
            public int amountPerTick;
            public int storageLimit;
            [HideInInspector] public int currentAmount;
        }

        [SerializeField] private List<CityResource> _resources;
        private float _tickTimer;

        private void Update()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= 60f)
            {
                ApplyTick();
                _tickTimer = 0f;
            }
        }

        private void ApplyTick()
        {
            foreach (var res in _resources)
            {
                res.currentAmount = Mathf.Min(res.currentAmount + res.amountPerTick, res.storageLimit);
                Debug.Log($"Ticked {res.amountPerTick} {res.type}, now {res.currentAmount}/{res.storageLimit}");
            }
        }
    }
}
