using Photon.Pun;
using UnityEngine;

namespace ProjectS.Gameplay.Networking
{
    public class EnemyTransformSync : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private float sendIntervalSeconds = 1f;
        [SerializeField] private float positionLerpSpeed = 8f;
        [SerializeField] private float rotationLerpSpeed = 10f;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float lastSendTime;
        private bool hasRemoteState;

        private void Awake()
        {
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            lastSendTime = -sendIntervalSeconds;
        }

        private void Update()
        {
            if (photonView.IsMine || !hasRemoteState)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                bool shouldSend = Time.time - lastSendTime >= sendIntervalSeconds;
                stream.SendNext(shouldSend);
                if (shouldSend)
                {
                    lastSendTime = Time.time;
                    stream.SendNext(transform.position);
                    stream.SendNext(transform.rotation);
                }
            }
            else
            {
                bool hasUpdate = (bool)stream.ReceiveNext();
                if (!hasUpdate)
                {
                    return;
                }

                targetPosition = (Vector3)stream.ReceiveNext();
                targetRotation = (Quaternion)stream.ReceiveNext();
                hasRemoteState = true;
            }
        }
    }
}
