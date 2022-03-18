using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

namespace TarodevController {
	/// <summary>
	/// Hey!
	/// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
	/// Right now it only contains movement and jumping, but it should be pretty easy to expand... I may even do it myself
	/// if there's enough interest. You can play and compete for best times here: https://tarodev.itch.io/
	/// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/GqeHHnhHpz
	/// </summary>
	[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(PlayerControllerInputGetter))]
	public class PlayerController : MonoBehaviour, IPlayerController {
		[SerializeField] private bool _allowDoubleJump, _allowDash, _allowCrouch;


		// Public for external hooks
		public FrameInput Input { get; private set; }
		public Vector3 RawMovement { get; private set; }
		public bool Grounded => _grounded;
		public event Action<bool> OnGroundedChanged;
		public event Action OnJumping, OnDoubleJumping;
		public event Action<bool> OnDashingChanged;
		public event Action<bool> OnCrouchingChanged;


		private Rigidbody2D _rb;
		private BoxCollider2D _collider;
		private PlayerControllerInputGetter _inputGetter;
		private Vector3 _lastPosition;
		private Vector3 _velocity;
		private float _currentHorizontalSpeed, _currentVerticalSpeed;
		private int _fixedFrame;

		void Awake() {
			_rb = GetComponent<Rigidbody2D>();
			_collider = GetComponent<BoxCollider2D>();
			_inputGetter = GetComponent<PlayerControllerInputGetter>();

			_defaultColliderSize = _collider.size;
			_defaultColliderOffset = _collider.offset;
		}

		private void Update() {
			// Calculate velocity
			_velocity = (transform.position - _lastPosition) / Time.deltaTime;
			_lastPosition = transform.position;

			GatherInput();
		}

		void FixedUpdate() {
			_fixedFrame++;
			_frameClamp = _moveClamp;

			RunCollisionChecks();

			CalculateCrouch();
			CalculateWalk();
			CalculateJumpApex();
			CalculateGravity();
			CalculateJump();
			CalculateDash();
			MoveCharacter();
		}

		#region Gather Input

		private void GatherInput() {
			Input = new FrameInput {
				JumpDown = _inputGetter.GetJumpDown(),
				JumpHeld = _inputGetter.GetJumpHeld(),
				DashDown = _inputGetter.GetDashDown(),

				X = _inputGetter.GetHorizontalAxis(),
				Y = _inputGetter.GetVerticalAxis(),
			};

			if (Input.DashDown)
				_dashToConsume = true;
			if (Input.JumpDown) {
				_lastJumpPressed = _fixedFrame;
				_jumpToConsume = true;
			}
		}

		#endregion

		#region Collisions

		[Header("COLLISION")] [SerializeField] private LayerMask _groundLayer;
		[SerializeField] private float _detectionRayLength = 0.1f;

		private readonly RaycastHit2D[] _hitsDown = new RaycastHit2D[1];
		private readonly RaycastHit2D[] _hitsDiscard = new RaycastHit2D[1];

		private bool _hittingCeiling, _grounded, _colRight, _colLeft;

		private float _timeLeftGrounded;


		// We use these raycast checks for pre-collision information
		private void RunCollisionChecks() {
			// Generate ray ranges. 
			var b = _collider.bounds;

			// Ground
			var groundedCheck = RunDetection(Vector2.down, _hitsDown);
			if (_grounded && !groundedCheck) {
				_timeLeftGrounded = _fixedFrame; // Only trigger when first leaving
				OnGroundedChanged?.Invoke(false);
			} else if (!_grounded && groundedCheck) {
				_coyoteUsable = true; // Only trigger when first touching
				_executedBufferedJump = false;
				_doubleJumpUsable = true;
				_canDash = true;
				OnGroundedChanged?.Invoke(true);
			}

			_grounded = groundedCheck;
			_colLeft = RunDetection(Vector2.left, _hitsDiscard);
			_colRight = RunDetection(Vector2.right, _hitsDiscard);

			// The rest
			_hittingCeiling = RunDetection(Vector2.up, _hitsDiscard);

			bool RunDetection(Vector2 dir, RaycastHit2D[] hits) {
				return Physics2D.BoxCastNonAlloc(b.center, b.size, 0, dir, hits, _detectionRayLength, _groundLayer) > 0;
			}
		}

		private void OnDrawGizmos() {
			if (!_collider)
				_collider = GetComponent<BoxCollider2D>();

			Gizmos.color = Color.blue;
			var b = _collider.bounds;
			b.Expand(_detectionRayLength);

			Gizmos.DrawWireCube(b.center, b.size);
		}

		#endregion

		#region Crouch

		[SerializeField, Header("CROUCH")] private float _crouchSizeModifier = 0.5f;
		[SerializeField] private float _crouchSpeedModifier = 0.5f;
		[SerializeField] private int _crouchSlowdownFrames = 20;
		[SerializeField] private float _immediateCrouchSlowdownThreshold = 0.1f;
		private Vector2 _defaultColliderSize, _defaultColliderOffset;
		private float _velocityOnCrouch;
		private bool _crouching;
		private int _frameStartedCrouching;

		private bool CanStand => Physics2D.OverlapBox((Vector2) transform.position + _defaultColliderOffset, _defaultColliderSize * 0.95f, 0, _groundLayer) == null;

		void CalculateCrouch() {
			if (!_allowCrouch)
				return;


			if (_crouching) {
				var immediate = _velocityOnCrouch <= _immediateCrouchSlowdownThreshold ? _crouchSlowdownFrames : 0;
				var crouchPoint = Mathf.InverseLerp(0, _crouchSlowdownFrames, _fixedFrame - _frameStartedCrouching + immediate);
				_frameClamp *= Mathf.Lerp(1, _crouchSpeedModifier, crouchPoint);
			}

			if (_grounded && Input.Y < 0 && !_crouching) {
				_crouching = true;
				OnCrouchingChanged?.Invoke(true);
				_velocityOnCrouch = Mathf.Abs(_velocity.x);
				_frameStartedCrouching = _fixedFrame;

				_collider.size = _defaultColliderSize * new Vector2(1, _crouchSizeModifier);

				// Lower the collider by the difference extent
				var difference = _defaultColliderSize.y - (_defaultColliderSize.y * _crouchSizeModifier);
				_collider.offset = -new Vector2(0, difference * 0.5f);
			} else if (!_grounded || (Input.Y >= 0 && _crouching)) {
				// Detect obstruction in standing area. Add a .1 y buffer to avoid the ground.
				if (!CanStand)
					return;

				_crouching = false;
				OnCrouchingChanged?.Invoke(false);

				_collider.size = _defaultColliderSize;
				_collider.offset = _defaultColliderOffset;
			}
		}

		#endregion

		#region Walk

		[Header("WALKING")] [SerializeField] private float _acceleration = 90;
		[SerializeField] private float _moveClamp = 13;
		[SerializeField] private float _deAcceleration = 60f;
		[SerializeField] private float _apexBonus = 2;
		private float _frameClamp;

		private void CalculateWalk() {
			if (Input.X != 0) {
				// Set horizontal move speed
				_currentHorizontalSpeed += Input.X * _acceleration * Time.fixedDeltaTime;

				// clamped by max frame movement
				_currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_frameClamp, _frameClamp);

				// Apply bonus at the apex of a jump
				var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
				_currentHorizontalSpeed += apexBonus * Time.fixedDeltaTime;
			} else {
				// No input. Let's slow the character down
				_currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0, _deAcceleration * Time.fixedDeltaTime);
			}

			if (_currentHorizontalSpeed > 0 && _colRight || _currentHorizontalSpeed < 0 && _colLeft) {
				// Don't pile up useless horizontal
				_currentHorizontalSpeed = 0;
			}
		}

		#endregion

		#region Gravity

		[Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
		[SerializeField] private float _minFallSpeed = 80f;
		[SerializeField] private float _maxFallSpeed = 120f;
		private float _fallSpeed;

		private void CalculateGravity() {
			if (_grounded) {
				// // Move out of the ground
				if (_currentVerticalSpeed < 0) {
					_currentVerticalSpeed = 0;
				}
			} else {
				// Add downward force while ascending if we ended the jump early
				var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

				// Fall
				_currentVerticalSpeed -= fallSpeed * Time.fixedDeltaTime;

				// Clamp
				if (_currentVerticalSpeed < _fallClamp)
					_currentVerticalSpeed = _fallClamp;
			}
		}

		#endregion

		#region Jump

		[Header("JUMPING")] [SerializeField] private float _jumpHeight = 30;
		[SerializeField] private float _jumpApexThreshold = 10f;
		[SerializeField] private int _coyoteTimeThreshold = 7;
		[SerializeField] private int _jumpBuffer = 7;
		[SerializeField] private float _jumpEndEarlyGravityModifier = 3;
		private bool _jumpToConsume;
		private bool _coyoteUsable;
		private bool _executedBufferedJump;
		private bool _endedJumpEarly = true;
		private float _apexPoint; // Becomes 1 at the apex of a jump
		private float _lastJumpPressed = Single.MinValue;
		private bool _doubleJumpUsable;
		private bool CanUseCoyote => _coyoteUsable && !_grounded && _timeLeftGrounded + _coyoteTimeThreshold > _fixedFrame;
		private bool HasBufferedJump => ((_grounded && !_executedBufferedJump) || _cornerStuck) && _lastJumpPressed + _jumpBuffer > _fixedFrame;
		private bool CanDoubleJump => _allowDoubleJump && _doubleJumpUsable && !_coyoteUsable;

		private void CalculateJumpApex() {
			if (!_grounded) {
				// Gets stronger the closer to the top of the jump
				_apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(_velocity.y));
				_fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
			} else {
				_apexPoint = 0;
			}
		}

		private void CalculateJump() {
			if (_crouching && !CanStand)
				return;

			if (_jumpToConsume && CanDoubleJump) {
				_currentVerticalSpeed = _jumpHeight;
				_doubleJumpUsable = false;
				_endedJumpEarly = false;
				_jumpToConsume = false;
				OnDoubleJumping?.Invoke();
			}


			// Jump if: grounded or within coyote threshold || sufficient jump buffer
			if ((_jumpToConsume && CanUseCoyote) || HasBufferedJump) {
				print(HasBufferedJump);
				_currentVerticalSpeed = _jumpHeight;
				_endedJumpEarly = false;
				_coyoteUsable = false;
				_jumpToConsume = false;
				_timeLeftGrounded = _fixedFrame;
				_executedBufferedJump = true;
				OnJumping?.Invoke();
			}

			// End the jump early if button released
			if (!_grounded && !Input.JumpHeld && !_endedJumpEarly && _velocity.y > 0)
				_endedJumpEarly = true;

			if (_hittingCeiling && _currentVerticalSpeed > 0)
				_currentVerticalSpeed = 0;
		}

		#endregion

		#region Dash

		[Header("DASH")] [SerializeField] private float _dashPower = 50;
		[SerializeField] private int _dashLength = 3;
		[SerializeField] private float _dashEndHorizontalMultiplier = 0.25f;
		private float _startedDashing;
		private bool _canDash;
		private Vector2 _dashVel;

		private bool _dashing;
		private bool _dashToConsume;

		void CalculateDash() {
			if (!_allowDash)
				return;
			if (_dashToConsume && _canDash && !_crouching) {
				var vel = new Vector2(Input.X, _grounded && Input.Y < 0 ? 0 : Input.Y);
				if (vel == Vector2.zero)
					return;
				_dashVel = vel * _dashPower;
				_dashing = true;
				OnDashingChanged?.Invoke(true);
				_canDash = false;
				_startedDashing = _fixedFrame;
			}

			if (_dashing) {
				_currentHorizontalSpeed = _dashVel.x;
				_currentVerticalSpeed = _dashVel.y;
				// Cancel when the time is out or we've reached our max safety distance
				if (_startedDashing + _dashLength < _fixedFrame) {
					_dashing = false;
					OnDashingChanged?.Invoke(false);
					if (_currentVerticalSpeed > 0)
						_currentVerticalSpeed = 0;
					_currentHorizontalSpeed *= _dashEndHorizontalMultiplier;
					if (_grounded)
						_canDash = true;
				}
			}

			_dashToConsume = false;
		}

		#endregion

		#region Move

		// We cast our bounds before moving to avoid future collisions
		private void MoveCharacter() {
			RawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); // Used externally
			var move = RawMovement * Time.fixedDeltaTime;

			// Apply effectors
			move -= EvaluateEffectors();

			_rb.MovePosition(_rb.position + (Vector2) move);

			RunCornerPrevention();
		}

		#region Corner Stuck Prevention

		private Vector2 _lastPos;
		private bool _cornerStuck;

		// This is a little hacky, but it's very difficult to fix.
		// This will allow walking and jumping while right on the corner of a ledge.
		void RunCornerPrevention() {
			// There's a fiddly thing where the rays will not detect ground (right inline with the collider),
			// but the collider won't fit. So we detect if we're meant to be moving but not.
			// The downside to this is if you stand still on a corner and jump straight up, it won't trigger the land
			// when you touch down. Sucks... but not sure how to go about it at this stage
			_cornerStuck = !_grounded && _lastPos == _rb.position && _lastJumpPressed + 1 < _fixedFrame;
			_currentVerticalSpeed = _cornerStuck ? 0 : _currentVerticalSpeed;
			_lastPos = _rb.position;
		}

		#endregion

		#endregion

		#region Effectors

		/// <summary>
		/// This allows a variety of movement modifications to be handled externally. Keeping this controller free
		/// of unique logic like underwater, wind, boosters, gravity zones, etc
		/// </summary>
		/// <returns></returns>
		private Vector3 EvaluateEffectors() {
			var effectorDirection = Vector3.zero;
			// Repeat this for other directions and possibly even area effectors. Wind zones, underwater etc
			effectorDirection += Process(_hitsDown);

			return effectorDirection;

			Vector3 Process(IEnumerable<RaycastHit2D> hits) {
				foreach (var hit2D in hits) {
					if (!hit2D.transform)
						return Vector3.zero;
					if (hit2D.transform.TryGetComponent(out IPlayerEffector effector)) {
						return effector.EvaluateEffector();
					}
				}
				return Vector3.zero;
			}
		}

		#endregion
	}
}