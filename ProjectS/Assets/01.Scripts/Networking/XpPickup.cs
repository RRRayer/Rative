using Photon.Pun;
using ProjectS.Networking;
using UnityEngine;

namespace ProjectS.Networking
{
    [RequireComponent(typeof(Collider))]
    public class XpPickup : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        [SerializeField] private float xpAmount = 5f;
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotateSpeed = 90f;

        private bool collected;

        private void Awake()
        {
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
