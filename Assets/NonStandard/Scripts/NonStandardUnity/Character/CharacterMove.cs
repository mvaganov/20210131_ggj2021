using NonStandard.Data;
using NonStandard.Process;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Character {
	[RequireComponent(typeof(Rigidbody))]
	public class CharacterMove : MonoBehaviour
	{
		[HideInInspector] public Rigidbody rb;
		public Transform head;
		private Transform body;
		private CapsuleCollider capsule;

		private void Awake() { body = transform; capsule = GetComponentInChildren<CapsuleCollider>(); }
		void Start()
		{
			rb = GetComponent<Rigidbody>();
			rb.freezeRotation = true;
		}
		/// <summary>
		/// how many seconds to hold down the jump button. if a non-zero value, a jump impulse will be applied. if zero, stop jumping.
		/// </summary>
		public float Jump { get; set; }
		/// <summary>
		/// how many seconds to hold down the jump button. if a non-zero value, a jump impulse will be applied. decays to zero with time.
		/// </summary>
		public float JumpButtonTimed { get { return jump.TimedJumpPress; } set { jump.TimedJumpPress = value; } }
		public float MoveSpeed { get { return move.speed; } set { move.speed = value; } }
		public float JumpHeight { get { return jump.max; } set { jump.max = value; } }
		private float lastJump = -1;
		public float StrafeRightMovement { get { return move.strafeRightMovement; } set { move.strafeRightMovement = value; } }
		public float MoveForwardMovement { get { return move.moveForwardMovement; } set { move.moveForwardMovement = value; } }

		// TODO move to NonStandard.Character.AutoMove
		[System.Serializable] public struct AutoMove {
			public Vector3 targetPosition;
			public System.Action whatToDoWhenTargetIsReached;
			public float closeEnough;
			public bool enabled;
			public bool jumpAtObstacle;
			public bool arrived;
			public static bool GetClickedLocation(Camera camera, out Vector3 targetPosition) {
				Ray ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
				RaycastHit rh = new RaycastHit();
				if (Physics.Raycast(ray, out rh)) {
					targetPosition = rh.point;
					return true;
				}
				targetPosition = Vector3.zero;
				return false;
			}
			public Vector3 CalculateMoveDirection(Vector3 position, float speed, Vector3 upNormal, ref bool arrived) {
				if (arrived) return Vector3.zero;
				Vector3 delta = targetPosition - position;
				if (upNormal != Vector3.zero) {
					delta = Vector3.ProjectOnPlane(delta, upNormal);
				}
				float dist = delta.magnitude;
				if (dist <= closeEnough || dist <= closeEnough+Time.deltaTime * speed) {
					arrived = true;
					if(whatToDoWhenTargetIsReached != null) { whatToDoWhenTargetIsReached.Invoke(); }
					return Vector3.zero;
				}
				return delta / dist; // normalized vector indicating direciton
			}
			public void SetAutoMovePosition(Vector3 position, float closeEnough) {
				targetPosition = position;
				this.closeEnough = closeEnough;
				enabled = true;
				arrived = false;
			}
			public void DisableAutoMove() { enabled = false; }
		}
		public bool IsAutoMoving() { return move.automaticMovement.enabled; }
		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null, float closeEnough = 0) {
			move.automaticMovement.SetAutoMovePosition(position, closeEnough);
			move.automaticMovement.whatToDoWhenTargetIsReached = whatToDoWhenTargetIsReached;
		}
		public void DisableAutoMove() { move.automaticMovement.DisableAutoMove(); MoveForwardMovement = StrafeRightMovement = 0; move.moveDirection = move.oppositionDirection; }
		public void GetLocalCapsule(out Vector3 top, out Vector3 bottom, out float rad) {
			float h = capsule.height / 2f;
			Vector3 r = Vector3.zero;
			switch (capsule.direction) {
			case 0: r = new Vector3(h, 0); break;
			case 1: r = new Vector3(0, h); break;
			case 2: r = new Vector3(0, 0, h); break;
			}
			top = capsule.center + r;
			bottom = capsule.center - r;
			top = body.rotation * top;
			bottom = body.rotation * bottom;
			rad = capsule.radius;
		}


		// TODO move to NonStandard.Character.CharacterMoveControls
		[System.Serializable] public class CharacterMoveControls {
			public float speed;
			[Tooltip("anything steeper than this cannot be moved on")]
			public float maxStableAngle;
			public AutoMove automaticMovement;

			public bool disabled;
			public bool canMoveInAir;
			public bool lookForwardMoving;
			public bool maintainSpeedAgainstWall;
			public bool systemMovement;
			[HideInInspector] public bool isStableOnGround;
			[HideInInspector] public float strafeRightMovement;
			[HideInInspector] public float moveForwardMovement;
			[HideInInspector] public float turnClockwise;

			[HideInInspector] public Vector3 moveDirection;
			[HideInInspector] public Vector3 groundNormal; // TODO refactor in CharacterMove
			[HideInInspector] public Vector3 oppositionDirection;
			[HideInInspector] public Vector3 lastVelocity;
			[HideInInspector] public Vector3 lastOppositionDirection;

			[Tooltip("Set this to enable movement based on how a camera is looking")]
			public Transform orientationTransform;

			Vector3 ConvertIntentionToRealDirection(Vector3 intention, Transform playerTransform, out float speed) {
				speed = intention.magnitude;
				if (orientationTransform) {
					intention = orientationTransform.TransformDirection(intention);
					Vector3 lookForward = orientationTransform.forward;
					Vector3 lookRight = orientationTransform.right;
					Vector3 groundNormal = Vector3.up;
					Vector3 groundForward = Vector3.ProjectOnPlane(lookForward, groundNormal);
					if (groundForward == Vector3.zero) { groundForward = orientationTransform.up; }
					else { groundForward.Normalize(); }
					float a = Vector3.SignedAngle(groundForward, lookForward, lookRight);
					intention = Quaternion.AngleAxis(-a, lookRight) * intention;
				} else {
					intention = playerTransform.transform.TransformDirection(intention);
				}
				intention /= speed;
				return intention;
			}
			public Vector3 AccountForBlocks(Vector3 moveVelocity) {
				if (oppositionDirection != Vector3.zero) {
					float opposition = -Vector3.Dot(moveDirection, oppositionDirection);
					if (opposition > 0) {
						float s = speed;
						if (maintainSpeedAgainstWall) { s = moveVelocity.magnitude; }
						moveVelocity += opposition * oppositionDirection;
						if (maintainSpeedAgainstWall) { moveVelocity.Normalize(); moveVelocity *= s; }
					}
				}
				return moveVelocity;
			}

			public void ApplyMoveFromInput(CharacterMove cm) {
				Vector3 moveVelocity = Vector3.zero;
				Transform t = cm.body;
				Vector3 oldDirection = moveDirection;
				moveDirection = new Vector3(strafeRightMovement, 0, moveForwardMovement);
				float intendedSpeed = 1;
				if (moveDirection != Vector3.zero) {
					moveDirection = ConvertIntentionToRealDirection(moveDirection, t, out intendedSpeed);
					if (intendedSpeed > 1) { intendedSpeed = 1; }
					// else { Debug.Log(intendedSpeed); }
				}
				if (automaticMovement.enabled) {
					if (moveDirection == Vector3.zero) {
						if (!automaticMovement.arrived) {
							moveDirection = automaticMovement.CalculateMoveDirection(t.position, speed * intendedSpeed, Vector3.up, ref automaticMovement.arrived);
							if (automaticMovement.arrived) { cm.callbacks.arrived.Invoke(automaticMovement.targetPosition); }
						}
					} else {
						automaticMovement.arrived = true; // if the player is providing input, stop calculating automatic movement
					}
				}
				if (moveDirection != Vector3.zero) {
					moveVelocity = AccountForBlocks(moveDirection);
					// apply the direction-adjusted movement to the velocity
					moveVelocity *= (speed * intendedSpeed);
				}
				if(moveDirection != oldDirection) { cm.callbacks.moveDirectionChanged.Invoke(moveDirection); }
				float gravity = cm.rb.velocity.y; // get current gravity
				moveVelocity.y = gravity; // apply to new velocity
				if(lookForwardMoving && moveDirection != Vector3.zero && orientationTransform != null)
				{
					cm.body.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
					if(cm.head != null) { cm.head.localRotation = Quaternion.identity; } // turn head straight while walking
				}
				if (!systemMovement) {
					cm.rb.velocity = moveVelocity;
				} else {
					cm.body.position += moveVelocity * Time.unscaledDeltaTime;
				}
				lastVelocity = moveVelocity;
				if(oppositionDirection == Vector3.zero && lastOppositionDirection != Vector3.zero)
				{
					cm.callbacks.wallCollisionStopped.Invoke(); // done colliding
					lastOppositionDirection = Vector3.zero;
				}
				oppositionDirection = Vector3.zero;
			}
			public void FixedUpdate(CharacterMove c) {
				if (isStableOnGround || canMoveInAir) {
					if(isStableOnGround) {
						c.jump.MarkStableJumpPoint(c.body.position); 
					}
					ApplyMoveFromInput(c);
				}
			}
			
			/// <summary>
			/// 
			/// </summary>
			/// <param name="cm"></param>
			/// <param name="collision"></param>
			/// <returns>the index of collision that could cause stability</returns>
			public int CollisionStabilityCheck(CharacterMove cm, Collision collision) {
				float biggestOpposition = -Vector3.Dot(moveDirection, oppositionDirection);
				int stableIndex = -1, wallCollisions = -1;
				Vector3 standingNormal = Vector3.zero;
				// identify that the character is on the ground if it's colliding with something that is angled like ground
				for (int i = 0; i < collision.contacts.Length; ++i) {
					Vector3 surfaceNormal = collision.contacts[i].normal;
					float a = Vector3.Angle(Vector3.up, surfaceNormal);
					if (a <= maxStableAngle) {
						isStableOnGround = true;
						stableIndex = i;
						standingNormal = surfaceNormal;
					} else {
						float opposition = -Vector3.Dot(moveDirection, surfaceNormal);
						if(opposition > biggestOpposition) {
							biggestOpposition = opposition;
							wallCollisions = i;
							oppositionDirection = surfaceNormal;
						}
						if(automaticMovement.jumpAtObstacle){
							cm.jump.TimedJumpPress = cm.jump.fullPressDuration;
						}
					}
				}
				if(wallCollisions != -1) {
					if (lastOppositionDirection != oppositionDirection) {
						cm.callbacks.wallCollisionStart.Invoke(oppositionDirection);
					}
					lastOppositionDirection = oppositionDirection;
				}
				return stableIndex;
			}
		}
	
		public CharacterMoveControls move = new CharacterMoveControls {
			speed = 5,
			maxStableAngle = 60,
			lookForwardMoving = true,
			automaticMovement = new AutoMove { }
		};

		public float GetJumpProgress() {
			return move.isStableOnGround ? 1 : (1 - ((float)(jump.usedDoubleJumps+1) / (jump.doubleJumps+1)));
		}
		private void Update() {
			if (move.systemMovement && !move.disabled && Time.timeScale ==0) { move.FixedUpdate(this); }
		}
		void FixedUpdate() {
			if (Jump != lastJump) {
				jump.Pressed = Jump > 0;//jump.PressJump = Jump;
				lastJump = Jump;
			}
			if (!move.disabled) { move.FixedUpdate(this); }
			if (jump.enabled) {
				bool wasJumping = jump.isJumping;
				jump.FixedUpdate(this);
				if(jump.isJumping && !wasJumping) {
					callbacks.jumped.Invoke(Vector3.up);
				}
			}
			if (!move.isStableOnGround && !jump.isJumping && move.groundNormal != Vector3.zero) {
				move.groundNormal = Vector3.zero;
				callbacks.fall.Invoke();
			}
			move.isStableOnGround = false; // invalidate stability *after* jump state is calculated
		}

		public bool IsStableOnGround() {
			return move.isStableOnGround;
		}

		private void OnCollisionStay(Collision collision) {
			if (collision.impulse != Vector3.zero && move.moveDirection != Vector3.zero && Vector3.Dot(collision.impulse.normalized, move.moveDirection) < -.75f) {
				rb.velocity = move.lastVelocity; // on a real collision, very much intentionally against a wall, maintain velocity
			}
			int contactThatMakesStability = move.CollisionStabilityCheck(this, collision);
			if(contactThatMakesStability >= 0) {
				Vector3 standingNormal = collision.contacts[contactThatMakesStability].normal;
				if (standingNormal != move.groundNormal) {
					callbacks.stand.Invoke(standingNormal);
				}
				move.groundNormal = standingNormal;
			}
		}

		private void OnCollisionEnter(Collision collision) {
			if (collision.impulse != Vector3.zero && move.CollisionStabilityCheck(this, collision) < 0) {
				rb.velocity = move.lastVelocity; // on a real collision, where the player is unstable, maintain velocity
			}
			jump.Interrupt(); //jump.collided = true;
		}

		public JumpModule jump = new JumpModule();
		// TODO move to NonStandard.Character.JumpModule
		[System.Serializable] public class JumpModule {
			/// <summary>if true, the jump is intentionally happening and hasn't been interrupted</summary>
			[HideInInspector] public bool isJumping;
			/// <summary>if true, the jump has passed it's apex</summary>
			[HideInInspector] public bool peaked;
			/// <summary>if true, the jump is no longer adjusting it's height based on Pressed value</summary>
			[HideInInspector] public bool heightSet;
			/// <summary>while this is true, the jump module is trying to jump</summary>
			[HideInInspector] public bool pressed;
			/// <summary>for debugging: shows the jump arc, and how it grows as Pressed is held</summary>
			public bool showJumpArc = false;
			/// <summary>allows ret-con of a missed jump (user presses jump a bit late after walking off a ledge)</summary>
			[HideInInspector] public bool forgiveLateJumps = true;
			[Tooltip("Enable or disable jumping")]
			public bool enabled = true;
			[Tooltip("Tapping the jump button for the shortest amount of time possible will result in this height")]
			public float min = .125f;
			[Tooltip("Holding the jump button for fullJumpPressDuration seconds will result in this height")]
			public float max = 1.5f;
			[Tooltip("How long the jump button must be pressed to jump the maximum height")]
			public float fullPressDuration = .25f;
			[Tooltip("For double-jumping, put a 2 here. To eliminate jumping, put a 0 here.")]
			public int doubleJumps = 0;
			[Tooltip("Used for AI driven jumps of different height")]
			public float TimedJumpPress = 0; // TODO just set targetJumpHeight?
			/// <summary>how long to wait for a jump after walking off a ledge</summary>
			public const long jumpLagForgivenessMs = 200;
			/// <summary>how long to wait to jump if press happens while still in the air</summary>
			public const long jumpTooEarlyForgivenessMs = 500;
			/// <summary>calculated target jump height</summary>
			[HideInInspector] public float targetJumpHeight;
			[HideInInspector] public Vector3 position;
			/// <summary>when jump was started, ideally when the button was pressed</summary>
			protected ulong jumpTime;
			/// <summary>when jump should reach apex</summary>
			protected ulong peakTime;
			/// <summary>when jump start position was last recognized as stable</summary>
			protected ulong stableTime;
			/// <summary>when the jump button was pressed</summary>
			protected ulong timePressed;
			/// <summary>How many double jumps have happend since being on the ground</summary>
			[HideInInspector] public int usedDoubleJumps;
			/// <summary>debug artifact, for seeing the jump arc</summary>
			[HideInInspector] Wire jumpArc;
			public bool Pressed {
				get { return pressed; }
				set { if (value && !pressed) { timePressed = Proc.Now; } pressed = value; }
			}
			public void Start(Vector3 p) {
				jumpTime = Proc.Now;
				peakTime = 0;
				isJumping = true;
				peaked = false;
				heightSet = false;
				position = p;
			}
			public void Interrupt() {
				isJumping = false;
				heightSet = true;
				targetJumpHeight = 0;
			}
			public void FixedUpdate(CharacterMove cm) {
				if (!enabled) return;
				bool peakedAtStart = peaked, jumpingAtStart = isJumping;
				bool jpress = pressed;
				ulong now = Proc.Now;
				if (TimedJumpPress > 0) {
					jpress = true; TimedJumpPress -= Time.deltaTime; if (TimedJumpPress < 0) { TimedJumpPress = 0; }
				}
				bool lateButForgiven = false;
				ulong late = 0;
				if (cm.move.isStableOnGround) { usedDoubleJumps = 0; } else if (jpress && forgiveLateJumps && (late = Proc.Now - stableTime) < jumpLagForgivenessMs) {
					stableTime = 0;
					cm.move.isStableOnGround = lateButForgiven = true;
				}
				if (jpress && cm.move.isStableOnGround && !isJumping && now - timePressed < jumpTooEarlyForgivenessMs) {
					timePressed = 0;
					if (!lateButForgiven) { Start(cm.body.position); } else { Start(position); jumpTime -= late; }
				}
				float gForce = Mathf.Abs(Physics.gravity.y);
				if (isJumping) {
					Vector3 vel = cm.rb.velocity;
					JumpUpdate(now, gForce, cm.move.speed, jpress, cm.body, ref vel);
					cm.rb.velocity = vel;
				} else {
					peaked = now >= peakTime;
				}
				if (!isJumping && jpress && usedDoubleJumps < doubleJumps) {
					DoubleJump(cm.body, cm.move.speed, gForce, peaked && !peakedAtStart && jumpingAtStart);
				}
			}

			private void JumpUpdate(ulong now, float gForce, float speed, bool jpress, Transform t, ref Vector3 vel) {
				if (!heightSet) {
					CalcJumpOverTime(now - jumpTime, gForce, out float y, out float yVelocity);
					if (float.IsNaN(y)) {
						Show.Log("if you see this error message, there might be a timing problem\n"+
							(now - jumpTime)+" bad y value... "+yVelocity+"  "+ peakTime+" vs "+now); // TODO why bad value happens sometimes?
						y = 0;
						yVelocity = 0;
					}
					Vector3 p = t.position;
					p.y = position.y + y;
					t.position = p;
					vel.y = yVelocity;
					if (showJumpArc) {
						if (jumpArc == null) { jumpArc = Lines.MakeWire("jump arc").Line(Vector3.zero); }
						jumpArc.Line(CalcPath(position, t.forward, speed, targetJumpHeight, gForce), Color.red);
					}
				}
				peaked = heightSet && now >= peakTime;
				isJumping = !peaked && jpress;
			}
			private void CalcJumpOverTime(ulong jumpMsSoFar, float gForce, out float yPos, out float yVel) {
				float jumptiming = jumpMsSoFar / 1000f;
				float jumpP = Mathf.Min(jumptiming / fullPressDuration, 1);
				if (jumpP >= 1) { heightSet = true; }
				targetJumpHeight = (max - min) * jumpP + min;
				float jVelocity = CalcJumpVelocity(targetJumpHeight, gForce);
				float jtime = 500 * CalcStandardDuration_WithJumpVelocity(jVelocity, gForce);
				peakTime = jumpTime + (uint)jtime;
				yPos = CalcHeightAt_WithJumpVelocity(jVelocity, jumptiming, targetJumpHeight, gForce);
				yVel = CalcVelocityAt_WithJumpVelocity(jVelocity, jumptiming, gForce);
			}
			private void DoubleJump(Transform t, float speed, float gForce, bool justPeaked) {
				if (justPeaked) {
					float peakHeight = position.y + targetJumpHeight;
					Vector3 delta = t.position - position;
					delta.y = 0;
					float dist = delta.magnitude;
					float peakTime = CalcStandardJumpDuration(targetJumpHeight, gForce) / 2;
					float expectedDist = peakTime * speed;
					if (dist > expectedDist) {
						Vector3 p = position + delta * expectedDist / dist;
						p.y = peakHeight;
						t.position = p;
					}
					position = t.position;
					position.y = peakHeight;
				} else {
					position = t.position;
				}
				Start(position);
				++usedDoubleJumps;
			}
			public void MarkStableJumpPoint(Vector3 position) {
				this.position = position;
				stableTime = Proc.Now;
			}
			/// <param name="pos">starting position of the jump</param>
			/// <param name="dir"></param>
			/// <param name="speed"></param>
			/// <param name="jHeight"></param>
			/// <param name="gForce"></param>
			/// <returns></returns>
			public static List<Vector3> CalcPath(Vector3 pos, Vector3 dir, float speed, float jHeight, float gForce, float timeIncrement = 1f/32) {
				return CalcPath_WithVelocity(CalcJumpVelocity(jHeight, gForce), pos, dir, speed, jHeight, gForce, timeIncrement);
			}
			static List<Vector3> CalcPath_WithVelocity(float jVelocity, Vector3 p, Vector3 dir, float speed, float jHeight, float gForce, float timeIncrement) {
				List<Vector3> points = new List<Vector3>();
				float stdJumpDuration = 2 * jVelocity / gForce;
				for (float t = 0; t < stdJumpDuration; t += timeIncrement) {
					float vAtPoint = t * gForce - jVelocity;
					float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jHeight;
					Vector3 pos = p + dir * (speed * t) + Vector3.up * y;
					points.Add(pos);
				}
				points.Add(p + dir * speed * stdJumpDuration);
				return points;
			}
			static float CalcJumpVelocity(float jumpHeight, float gForce) { return Mathf.Sqrt(2 * jumpHeight * gForce); }
			static float CalcJumpHeightAt(float time, float jumpHeight, float gForce) {
				return CalcHeightAt_WithJumpVelocity(CalcJumpVelocityAt(time, jumpHeight, gForce), time, jumpHeight, gForce);
			}
			static float CalcHeightAt_WithJumpVelocity(float jumpVelocity, float time, float jumpHeight, float gForce) {
				float vAtPoint = CalcVelocityAt_WithJumpVelocity(jumpVelocity, time, gForce);
				float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jumpHeight;
				return y;
			}
			static float CalcJumpVelocityAt(float time, float jumpHeight, float gForce) {
				return CalcVelocityAt_WithJumpVelocity(CalcJumpVelocity(jumpHeight, gForce), time, gForce);
			}
			static float CalcVelocityAt_WithJumpVelocity(float jumpVelocity, float time, float gForce) {
				return -(time * gForce - jumpVelocity);
			}
			static float CalcStandardJumpDuration(float jumpHeight, float gForce) {
				return CalcStandardDuration_WithJumpVelocity(CalcJumpVelocity(jumpHeight, gForce), gForce);
			}
			static float CalcStandardDuration_WithJumpVelocity(float jumpVelocity, float gForce) {
				return 2 * jumpVelocity / gForce;
			}
		}

		[Tooltip("hooks that allow code execution when character state changes (useful for animation)")]
		public Callbacks callbacks = new Callbacks();

		[System.Serializable] public class Callbacks {
			[Tooltip("when player changes direction, passes the new direction")]
			public UnityEvent_Vector3 moveDirectionChanged;
			[Tooltip("when player changes their standing angle, passes the new ground normal")]
			public UnityEvent_Vector3 stand;
			[Tooltip("when player jumps, passes the direction of the jump")]
			public UnityEvent_Vector3 jumped;
			[Tooltip("when player starts to fall")]
			public UnityEvent fall;
			[Tooltip("when player collides with a wall, passes the wall's normal")]
			public UnityEvent_Vector3 wallCollisionStart;
			[Tooltip("when player is no longer colliding with a wall")]
			public UnityEvent wallCollisionStopped;
			[Tooltip("when auto-moving player reaches their goal, passes absolute location of the goal")]
			public UnityEvent_Vector3 arrived;
		}
	}
}