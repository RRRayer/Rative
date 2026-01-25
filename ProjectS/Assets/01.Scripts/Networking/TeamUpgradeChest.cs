using Photon.Pun;
using UnityEngine;

namespace ProjectS.Networking
{
    [RequireComponent(typeof(Collider))]
    public class TeamUpgradeChest : MonoBehaviourPun
    {
        private bool collected;

        private void Awake()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;

            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected)
            {
                return;
            }

            PhotonView otherView = other.GetComponentInParent<PhotonView>();
            if (PhotonNetwork.InRoom)
            {
                if (otherView == null || !otherView.IsMine)
                {
                    return;
                }
            }

            collected = true;
            int viewId = photonView != null ? photonView.ViewID : 0;
            TeamUpgradeManager.Instance?.RequestChestPickup(viewId);
            if (!PhotonNetwork.InRoom)
            {
                Destroy(gameObject);
            }
        }
    }
}
