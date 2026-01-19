using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using ProjectS.Networking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PS.Events;
using PS.Base;

namespace PS.Manager
{
    /// <summary>
    /// Defines the different states of the game flow.
    /// </summary>
    public enum GameState
    {
        Waiting,    // Waiting for the stage to start
        InProgress, // Stage is in progress, enemies are spawning
        Boss,       // Boss has spawned
        Cleared     // Stage has been cleared
    }

    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Settings")]
        [Tooltip("The prefab to use for representing the player")]
        [SerializeField] private GameObject playerPrefab;
        public bool testStartStage;

        [Header("Stage Settings")]
        [SerializeField] private float stageElapsedTime; // Current elapsed time of the stage
        // stageDuration removed as the stage is cleared by defeating the boss, and the timer's main purpose is to trigger spawns.
        [SerializeField] private List<float> eliteSpawnTimes = new List<float> { 3f, 6f }; // 3, 6 seconds for testing
        [SerializeField] private float bossSpawnTime = 9f; // 9 seconds for testing

        [Header("Game Flow Events")]
        [SerializeField] private VoidEventChannelSO onStageStart;
        [SerializeField] private VoidEventChannelSO onEliteSpawn;
        [SerializeField] private VoidEventChannelSO onBossSpawn;
        [SerializeField] private VoidEventChannelSO onStageClear;

        /// <summary>
        /// Invoked whenever the game state changes.
        /// </summary>
        public event UnityAction<GameState> OnStateChanged;

        private GameState _currentState;
        public GameState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnStateChanged?.Invoke(_currentState);
                }
            }
        }

        private Coroutine _stageTimerCoroutine; // Reference to the running stage timer coroutine

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (SharedProgressionManager.Instance == null)
            {
                gameObject.AddComponent<SharedProgressionManager>();
            }
        }

        private void OnEnable()
        {
            onStageClear.OnEventRaised += StageCleared;
        }

        private void OnDisable()
        {
            onStageClear.OnEventRaised -= StageCleared;
        }

        private void Start()
        {
            CurrentState = GameState.Waiting;

            if (playerPrefab == null)
            {
                Log.E("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    StartCoroutine(InstantiatePlayerWhenInRoom());
                }
                else
                {
                    Log.D($"Ignoring scene load for {SceneManager.GetActiveScene().name}");
                }
            }
        }

        private void Update()
        {
            // For debugging: automatically start the stage after a delay.
            // In the final version, this should be triggered by a player action (e.g., activating a device).
            if (testStartStage)
            {
                testStartStage = false;
                StartStage();
            }
        }

        private IEnumerator DelayedStageStart(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartStage();
        }

        /// <summary>
        /// Starts the stage progression. Only the Master Client can initiate this.
        /// </summary>
        public void StartStage()
        {
            // if (!PhotonNetwork.IsMasterClient)
            // {
            //     Log.W("[GameManager] Non-MasterClient tried to start the stage. Ignored.");
            //     return;
            // }

            if (CurrentState != GameState.Waiting)
            {
                Log.W($"[GameManager] Tried to start stage in state {CurrentState}. Ignored.");
                return;
            }
            
            Log.D("[GameManager] Starting stage.");
            CurrentState = GameState.InProgress;
            
            // Raise the event for all clients -> 몬스터 소환하기 시작
            onStageStart?.RaiseEvent();
            
            // Start the timer on the Master Client
            _stageTimerCoroutine = StartCoroutine(StageTimer());
        }

        /// <summary>
        /// Coroutine that manages the stage timer and triggers time-based events.
        /// Should only be run by the Master Client.
        /// </summary>
        private IEnumerator StageTimer()
        {
            stageElapsedTime = 0f; // Initialize the inspector-visible timer
            int eliteSpawns = 0;

            Log.D("[GameManager] Stage Timer started.");

            // The timer runs until the boss is spawned. Stage duration is now implicitly defined by bossSpawnTime.
            while (stageElapsedTime < bossSpawnTime) 
            {
                stageElapsedTime += Time.deltaTime;

                // Check for elite spawns ( 3분, 6분 총 2번)
                if (eliteSpawns < eliteSpawnTimes.Count && stageElapsedTime >= eliteSpawnTimes[eliteSpawns])
                {
                    Log.D($"[GameManager] Triggering Elite Spawn at {stageElapsedTime}s.");
                    onEliteSpawn?.RaiseEvent();
                    eliteSpawns++;
                }

                yield return null;
            }
            
            // 9분이 되면 보스 몬스터 소환
            Log.D($"[GameManager] Triggering Boss Spawn at {stageElapsedTime}s.");
            CurrentState = GameState.Boss;
            onBossSpawn?.RaiseEvent();

            // The actual StageCleared() should be called when the boss is defeated,
            // likely triggered by the MonsterManager or the boss's own script.
        }

        /// <summary>
        /// Called when the stage is cleared (e.g., boss defeated).
        /// Transitions game state and prepares for next stage.
        /// This method is subscribed to the onStageClear event.
        /// </summary>
        private void StageCleared()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Log.W("[GameManager] Non-MasterClient tried to call StageCleared(). Ignored.");
                return;
            }

            if (CurrentState == GameState.Cleared || CurrentState == GameState.Waiting)
            {
                Log.W($"[GameManager] StageCleared() called in an invalid state: {CurrentState}. Ignored.");
                return;
            }

            Log.D("[GameManager] Stage Cleared!");
            CurrentState = GameState.Cleared;
            
            if (_stageTimerCoroutine != null)
            {
                StopCoroutine(_stageTimerCoroutine);
                _stageTimerCoroutine = null;
            }
            
            stageElapsedTime = 0f; // Reset the timer on stage clear
            // The onStageClear event was already raised by the system that defeated the boss.
            StartCoroutine(ResetStageStateAfterDelay(5f)); // Reset to Waiting after 5 seconds
        }

        /// <summary>
        /// Resets the game state to Waiting after a delay, allowing for stage transition UI or player actions.
        /// </summary>
        private IEnumerator ResetStageStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Log.D("[GameManager] Resetting stage to Waiting state.");
            CurrentState = GameState.Waiting;
            stageElapsedTime = 0f; // Reset the timer when returning to waiting state
            // Optionally, trigger UI for moving to next stage or re-activating central device
        }

        private IEnumerator InstantiatePlayerWhenInRoom()
        {
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
            }

            Log.D($"[GameManager] LocalPlayer spawned in {SceneManager.GetActiveScene().name}");
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        
        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }
}
