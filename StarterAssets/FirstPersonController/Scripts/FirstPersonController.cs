using UnityEngine;
using UnityEngine.InputSystem;


using Unity.Netcode;
using System.Numerics;
using UnityEditor;
using Unity.Netcode.Components;

namespace UnityEngine
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : NetworkBehaviour
	{
		PlayerControls playerControls;
		public float mouseSense = 1f;
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		public float defaultMoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


		private CharacterController _controller;
		private GameObject _mainCamera;

		public float _threshold = 0.02f;


		InputAction look;
		InputAction move;
		InputAction jump;
		private void Awake()
		{
			playerControls = new PlayerControls();
			look = playerControls.Player.Look;
			move = playerControls.Player.Move;
			jump = playerControls.Player.Jump;
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		void OnEnable()
		{
			look.Enable();
			move.Enable();
			jump.Enable();
		}
		void OnDisable()
		{
			look.Disable();
			move.Disable();
			jump.Disable();

		}
		Animator animator;
		int moveSpeedHash;
		private void Start()
		{

			_controller = GetComponent<CharacterController>();
			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			animator = GetComponentInChildren<Animator>();
			moveSpeedHash = Animator.StringToHash("MoveSpeed");
			Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);
			GetComponent<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeath);
			GameStateController.Instance.GameRestartEvent.AddListener(HandlePlayerRespawn);
		}
		public Vector3 onDeathTeleport = new Vector3(100f, 100f, 100f);
		void HandlePlayerRespawn()
		{
			_controller.enabled = true;
			enabled = true;
		}
		void HandlePlayerDeath()
		{

			_controller.enabled = false;
			enabled = false;
			transform.position = onDeathTeleport;

		}
		void HandleUpgradeEvent(ulong ID, string upgradeName)
		{
			if (ID == NetworkManager.Singleton.LocalClientId)
			{
				//    Debug.Log("Applying:" + upgradeName + "!");
				if (upgradeName == "walkSpeed")
				{
					MoveSpeed = defaultMoveSpeed + (defaultMoveSpeed * 0.1f) * Upgrades.Instance.getUpgradeLevel(upgradeName);
				}

			}
		}
		public float scatterSpawnsBy = 0.5f;

		void correctFallingOffMap()
		{
			if (transform.position.y < -1 * (Constants.MAX_HEIGHT / 2))
			{
				GetComponent<NetworkTransform>().enabled = false;
				GetComponent<CharacterController>().enabled = false;
				serverMove.y = 0f;
				transform.position = GameStateController.Instance.spawnPos.position;
				GetComponent<NetworkTransform>().enabled = true;
				GetComponent<CharacterController>().enabled = true;
			}
		}

		private void Update()
		{
			if (!_controller.enabled)
				return;
			if (CursorManager.Instance.cursorNeeded)
			{
				return;
			}
			JumpAndGravity();
			GroundedCheck();
			Move();
			correctFallingOffMap();
			if (IsOwner) { ExecuteMove(serverMove); }
			if (!IsHost && IsOwner)
			{
				//MoveRpc(serverMove);

			}





		}
		[Rpc(SendTo.Server)]
		public void MoveRpc(Vector3 move)
		{
			_controller.Move(move);

		}
		[Rpc(SendTo.Server)]
		public void RotateRpc(Vector3 rotate)
		{
			CinemachineCameraTarget.transform.localRotation = serverCinema;
			transform.Rotate(rotate);

		}
		private void LateUpdate()
		{
			if (CursorManager.Instance.cursorNeeded)
			{
				return;
			}
			if (!CameraRotation()) return;
			if (IsOwner) { ExecuteRotate(serverRotate); }
		}
		void ExecuteMove(Vector3 move)
		{
			_controller.Move(move);
			animator.SetFloat(moveSpeedHash, _controller.velocity.magnitude);

		}

		void ExecuteRotate(Vector3 rotate)
		{
			CinemachineCameraTarget.transform.localRotation = serverCinema;
			transform.Rotate(rotate);

		}
		private void GroundedCheck()
		{
			// set sphere position, with offset
			UnityEngine.Vector3 spherePosition = new UnityEngine.Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private bool CameraRotation()
		{
			// if there is an input

			Vector2 tmp = look.ReadValue<Vector2>();
			if (tmp.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = mouseSense;

				_cinemachineTargetPitch += tmp.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = tmp.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				//CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				serverCinema = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				// rotate the player left and right

				//transform.Rotate(UnityEngine.Vector3.up * _rotationVelocity);
				serverRotate = UnityEngine.Vector3.up * _rotationVelocity;
				return true;
			}
			return false;


		}
		Quaternion serverCinema;
		Vector3 serverRotate;
		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (move.ReadValue<Vector2>() == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new UnityEngine.Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			UnityEngine.Vector3 inputDirection = new UnityEngine.Vector3(move.ReadValue<Vector2>().x, 0.0f, move.ReadValue<Vector2>().y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (move.ReadValue<Vector2>() != Vector2.zero)
			{
				// move
				inputDirection = transform.right * move.ReadValue<Vector2>().x + transform.forward * move.ReadValue<Vector2>().y;
			}

			// move the player
			//_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new UnityEngine.Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			serverMove = inputDirection.normalized * (_speed * Time.deltaTime) + new UnityEngine.Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
		}
		Vector3 serverMove;
		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (jump.ReadValue<float>() == 1f && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				//jump.ReadValue<bool>() = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new UnityEngine.Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}