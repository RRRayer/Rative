using Photon.Pun;
using PS.Networking;
using UnityEngine;

namespace PS.Networking
{
    [RequireComponent(typeof(Collider))]
    public class XpPickup : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        [SerializeField] private float xpAmount = 5f;
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float pickupDelaySeconds = 0.25f;

        private bool collected;
        private float spawnTime;
        private bool pickupEnabled;

        private void Awake()
        {
            spawnTime = Time.time;
            pickupEnabled = pickupDelaySeconds <= 0f;

            Collider physicsCollider = GetComponent<Collider>();
            if (physicsCollider != null)
            {
                physicsCollider.isTrigger = false;
            }

            if (GetComponent<TriggerMarker>() == null)
            {
                SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.6f;
                gameObject.AddComponent<TriggerMarker>();
            }

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private sealed class TriggerMarker : MonoBehaviour
        {
        }

        private void Update()
        {
            if (!pickupEnabled && (Time.time - spawnTime) >= pickupDelaySeconds)
            {
                pickupEnabled = true;
            }

            if (rotate)
            {
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            }
        }

        public void SetAmount(float amount)
        {
            xpAmount = amount;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;
            if (data != null && data.Length > 0)
            {
                xpAmount = (float)data[0];
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected)
            {
                return;
            }

            if (!pickupEnabled)
            {
                return;
            }

            PhotonView otherView = other.GetComponentInParent<PhotonView>();
            if (otherView == null || !otherView.IsMine)
            {
                return;
            }

            collected = true;
            SharedProgressionManager.Instance?.RequestPickup(photonView.ViewID, xpAmount);
            if (!PhotonNetwork.InRoom)
            {
                Destroy(gameObject);
            }
        }
    }
}
