using Photon.Pun;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PS.Manager
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public float Health = 1f;
        public static GameObject LocalPlayerInstance;

        [SerializeField] private GameObject beams;
        private bool isFiring;
        private bool isDead;
    #if ENABLE_INPUT_SYSTEM
        private StarterAssetsInputs input;
    #endif

        private void Awake()
        {
            input = GetComponent<StarterAssetsInputs>();

            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }

            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // 게임 매니저는 모든 씬에 존재
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // If this was the local player instance, clean up the static reference
            if (photonView.IsMine && LocalPlayerInstance == gameObject)
            {
                LocalPlayerInstance = null;
            }
        }

         private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode)
        {
            // After a scene load, check if the local player is in a valid position.
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0, 5, 0);
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInput();

                // 현재 체력 관리
                if (Health <= 0f && !isDead)
                {
                    isDead = true;
                    GameManager.Instance.LeaveRoom();
                }
            }

            // Beam 활성화
            if (beams != null && isFiring != beams.activeInHierarchy)
            {
                beams.SetActive(isFiring);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Debug.Log("Ouch!!");
            Health -= 0.1f * Time.deltaTime;
        }

        private void ProcessInput()
        {
            if (input.fire)
            {
                if (!isFiring)
                {
                    Debug.Log("히히 발사");
                    isFiring = true;
                }
            }
            else if (!input.fire)
            {
                if (isFiring)
                {
                    isFiring = false;
                }
            }
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 이게 자신이다. 자신의 데이터를 다른 사람에게 전송
                stream.SendNext(isFiring);
                stream.SendNext(Health);
            }
            else
            {
                // 데이터 수신
                this.isFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }

        }
    }
}
