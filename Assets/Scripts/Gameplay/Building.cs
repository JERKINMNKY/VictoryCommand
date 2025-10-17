using System.Collections;
using UnityEngine;
using IFC.Data;

namespace IFC.Gameplay
{
    /// <summary>
    /// Handles upgrade logic for a building using BuildingData.
    /// </summary>
    public class Building : MonoBehaviour
    {
        [SerializeField] private BuildingData _data;
        private bool _isUpgrading;
        private float _upgradeTimer;

        public delegate void UpgradeProgress(float percent);
        public event UpgradeProgress OnUpgradeProgress;

        public void StartUpgrade()
        {
            if (_isUpgrading)
                return;

            StartCoroutine(UpgradeCoroutine());
        }

        private IEnumerator UpgradeCoroutine()
        {
            _isUpgrading = true;
            _upgradeTimer = 0f;
            float duration = _data.UpgradeTimeSeconds;

            while (_upgradeTimer < duration)
            {
                _upgradeTimer += Time.deltaTime;
                OnUpgradeProgress?.Invoke(_upgradeTimer / duration);
                yield return null;
            }

            FinishUpgrade();
        }

        private void FinishUpgrade()
        {
            _isUpgrading = false;
            // Handle logic to level up, apply effects, etc.
            Debug.Log($"{_data.BuildingName} upgrade complete.");
        }

        public void CancelUpgrade()
        {
            if (!_isUpgrading)
                return;

            StopAllCoroutines();
            _isUpgrading = false;
            Debug.Log($"{_data.BuildingName} upgrade canceled.");
        }
    }
}
