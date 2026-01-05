using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool attack;
		public bool skill1;
		public bool skill2;
		public bool skill3;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput playerInput;
		private bool loggedMissingAction;

		private void Awake()
		{
			playerInput = GetComponent<PlayerInput>();
			EnsureActionMap();
		}

		private void OnEnable()
		{
			EnsureActionMap();
		}

		private void Update()
		{
			SyncActionState("Attack", ref attack);
			SyncActionState("Skill1", ref skill1);
			SyncActionState("Skill2", ref skill2);
			SyncActionState("Skill3", ref skill3);
		}

		private void EnsureActionMap()
		{
			if (playerInput == null || playerInput.actions == null)
			{
				return;
			}

			if (!playerInput.actions.enabled)
			{
				playerInput.actions.Enable();
			}

			if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != "Player")
			{
				playerInput.SwitchCurrentActionMap("Player");
			}
		}

		private void SyncActionState(string actionName, ref bool state)
		{
			if (playerInput == null || playerInput.actions == null)
			{
				return;
			}

			InputAction action = playerInput.actions[actionName];
			if (action == null)
			{
				if (!loggedMissingAction)
				{
					loggedMissingAction = true;
					Debug.LogWarning($"StarterAssetsInputs: missing action '{actionName}'.");
				}
				return;
			}

			state = action.IsPressed();
		}

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAttack(InputValue value)
		{
			AttackInput(value.isPressed);
		}

		public void OnSkill1(InputValue value)
		{
			Skill1Input(value.isPressed);
		}

		public void OnSkill2(InputValue value)
		{
			Skill2Input(value.isPressed);
		}

		public void OnSkill3(InputValue value)
		{
			Skill3Input(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void AttackInput(bool newAttackState)
		{
			attack = newAttackState;
		}

		public void Skill1Input(bool newSkillState)
		{
			skill1 = newSkillState;
		}

		public void Skill2Input(bool newSkillState)
		{
			skill2 = newSkillState;
		}

		public void Skill3Input(bool newSkillState)
		{
			skill3 = newSkillState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}
