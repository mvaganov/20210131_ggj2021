using NonStandard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
	public bool onGround;
	public float moveH, moveV;
	public float speed = 5;
	Rigidbody rb;
	public JumpModule jump = new JumpModule();
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
		[HideInInspector] public bool showJumpArc = false;
		/// <summary>allows ret-con of a missed jump (user presses jump a bit late after walking off a ledge)</summary>
		[HideInInspector] public bool forgiveLateJumps = true;
		[Tooltip("Enable or disable jumping")]
		public bool enabled = true;
		[Tooltip("Tapping the jump button for the shortest amount of time possible will result in this height")]
		public float minJumpHeight = .125f;
		[Tooltip("Holding the jump button for fullJumpPressDuration seconds will result in this height")]
		public float maxJumpHeight = 1;
		[Tooltip("How long the jump button must be pressed to jump the maximum height")]
		public float fullJumpPressDuration = .25f;
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
		protected long jumpTime;
		/// <summary>when jump should reach apex</summary>
		protected long peakTime;
		/// <summary>when jump start position was last recognized as stable</summary>
		protected long stableTime;
		/// <summary>when the jump button was pressed</summary>
		protected long timePressed;
		/// <summary>How many double jumps have happend since being on the ground</summary>
		[HideInInspector] public int usedDoubleJumps;
		/// <summary>debug artifact, for seeing the jump arc</summary>
		[HideInInspector] Lines.Wire jumpArc;
		public bool Pressed {
			get { return pressed; } set { if (value && !pressed) { timePressed = Clock.NowRealTicks; } pressed = value; }
		}
		public void Start(Vector3 p) {
			jumpTime = Clock.NowTicks;
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
		public void FixedUpdate(Transform t, ref Vector3 vel, float speed, bool onGround, float gForce) {
			if (!enabled) return;
			bool peakedAtStart = peaked, jumpingAtStart = isJumping;
			bool jpress = pressed;
			long now = Clock.NowTicks;
			if (TimedJumpPress > 0) {
				jpress = true; TimedJumpPress -= Time.deltaTime; if (TimedJumpPress < 0) { TimedJumpPress = 0; }
			}
			bool lateButForgiven = false;
			long late = 0;
			if (onGround) { usedDoubleJumps = 0; }
			else if (jpress && forgiveLateJumps && (late = Clock.NowRealTicks - stableTime) < jumpLagForgivenessMs) {
				stableTime = 0;
				onGround = lateButForgiven = true;
			}
			if (jpress && onGround && !isJumping && now - timePressed < jumpTooEarlyForgivenessMs) {
				timePressed = 0;
				if (!lateButForgiven) { Start(t.position); }
				else { Start(position); jumpTime -= late; }
			}
			if (isJumping) {
				JumpUpdate(now, gForce, speed, jpress, t, ref vel);
			} else {
				peaked = now >= peakTime;
			}
			if (!isJumping && jpress && usedDoubleJumps < doubleJumps) {
				DoubleJump(t, speed, gForce, peaked && !peakedAtStart && jumpingAtStart);
			}
		}

		private void JumpUpdate(long now, float gForce, float speed, bool jpress, Transform t, ref Vector3 vel) {
			if (!heightSet) {
				CalcJumpOverTime(now - jumpTime, gForce, out float y, out float yVelocity);
				Vector3 p = t.position;
				p.y = position.y + y;
				t.position = p;
				vel.y = yVelocity;
				if (showJumpArc) {
					if (jumpArc == null) { jumpArc = Lines.MakeWire("jump arc").Line(Vector3.zero); }
					jumpArc.Line(CalcJumpPath(position, t.forward, speed, targetJumpHeight, gForce), Color.red);
				}
			}
			peaked = heightSet && now >= peakTime;
			isJumping = !peaked && jpress;
		}
		private void CalcJumpOverTime(long jumpMsSoFar, float gForce, out float yPos, out float yVel) {
			float jumptiming = jumpMsSoFar / 1000f;
			float jumpP = Mathf.Min(jumptiming / fullJumpPressDuration, 1);
			if (jumpP >= 1) { heightSet = true; }
			targetJumpHeight = (maxJumpHeight - minJumpHeight) * jumpP + minJumpHeight;
			float jVelocity = CalcJumpVelocity(targetJumpHeight, gForce);
			float jtime = 500 * CalcStandardJumpDuration_WithJumpVelocity(jVelocity, gForce);
			peakTime = jumpTime + (int)jtime;
			yPos = CalcJumpHeightAt_WithJumpVelocity(jVelocity, jumptiming, targetJumpHeight, gForce);
			yVel = CalcJumpVelocityAt_WithJumpVelocity(jVelocity, jumptiming, gForce);
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
			stableTime = Clock.NowRealTicks;
		}

		static List<Vector3> CalcJumpPath(Vector3 position, Vector3 dir, float speed, float jHeight, float gForce) {
			return CalcJumpPath_WithVelocity(CalcJumpVelocity(jHeight, gForce), position, dir, speed, jHeight, gForce);
		}
		static List<Vector3> CalcJumpPath_WithVelocity(float jVelocity, Vector3 p, Vector3 dir, float speed, float jHeight, float gForce) {
			List<Vector3> points = new List<Vector3>();
			float stdJumpDuration = 2 * jVelocity / gForce;
			for (float t = 0; t < stdJumpDuration; t += 1f / 32) {
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
			return CalcJumpHeightAt_WithJumpVelocity(CalcJumpVelocityAt(time, jumpHeight, gForce), time, jumpHeight, gForce);
		}
		static float CalcJumpHeightAt_WithJumpVelocity(float jumpVelocity, float time, float jumpHeight, float gForce) {
			float vAtPoint = CalcJumpVelocityAt_WithJumpVelocity(jumpVelocity, time, gForce);
			float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jumpHeight;
			return y;
		}
		static float CalcJumpVelocityAt(float time, float jumpHeight, float gForce) {
			return CalcJumpVelocityAt_WithJumpVelocity(CalcJumpVelocity(jumpHeight, gForce), time, gForce);
		}
		static float CalcJumpVelocityAt_WithJumpVelocity(float jumpVelocity, float time, float gForce) {
			return -(time * gForce - jumpVelocity);
		}
		static float CalcStandardJumpDuration(float jumpHeight, float gForce) {
			return CalcStandardJumpDuration_WithJumpVelocity(CalcJumpVelocity(jumpHeight, gForce), gForce);
		}
		static float CalcStandardJumpDuration_WithJumpVelocity(float jumpVelocity, float gForce) {
			return 2 * jumpVelocity / gForce;
		}
	}

	void Start() {
        rb = GetComponent<Rigidbody>();
	}

	void Update() {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");
		jump.Pressed = Input.GetButton("Jump");
	}

	public void OnCollisionEnter(Collision collision) {
		jump.Interrupt();
		float groundiness = Vector3.Dot(collision.contacts[0].normal, Vector3.up);
		if (groundiness > 0.5f) {
			onGround = true;
		}
	}
	public void OnCollisionStay(Collision collision) {
		float groundiness = Vector3.Dot(collision.contacts[0].normal, Vector3.up);
		if (groundiness > 0.5f) {
			jump.MarkStableJumpPoint(transform.position);
		}
	}
	public void OnCollisionExit(Collision collision) {
		onGround = false;
	}
	private void FixedUpdate() {
		Vector3 vel = rb.velocity;
		vel.x = moveH * speed;
		vel.z = moveV * speed;
		jump.FixedUpdate(transform, ref vel, speed, onGround, -Physics.gravity.y);
		rb.velocity = vel;
		if (moveH != 0 || moveV != 0) {
			Vector3 dir = new Vector3(moveH, 0, moveV);
			dir.Normalize();
			transform.rotation = Quaternion.LookRotation(dir);
		}
	}

}
