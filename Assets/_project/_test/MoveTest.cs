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
		public float minJumpHeight = .25f;
		public float maxJumpHeight = 1;
		public float jumpMaxPressDuration = .25f;
		public int doubleJumps = 0;
		public float TimedJumpPress = 0;
		public const long jumpLagForgivenessMs = 200;
		[HideInInspector] public float targetJumpHeight;
		[HideInInspector] public Vector3 position;
		/// <summary>
		/// when jump was started, ideally when the button was pressed
		/// </summary>
		[HideInInspector] public long jumpTime;
		/// <summary>
		/// when jump should reach apex
		/// </summary>
		[HideInInspector] public long peakTime;
		/// <summary>
		/// when jump start position was last recognized as stable
		/// </summary>
		[HideInInspector] public long stableTime;
		[HideInInspector] public int usedDoubleJumps;
		[HideInInspector] Lines.Wire jumpArc;
		[HideInInspector]
		public bool isJumping, peaked, heightSet, Pressed, showJumpArc = false, forgiveLateJumps = true;
		public void Start(Vector3 p) {
			jumpTime = Clock.NowTicks;
			peakTime = 0;
			isJumping = true;
			peaked = false;
			heightSet = false;
			position = p;
		}
		public void Stop() {
			isJumping = false;
			heightSet = true;
			targetJumpHeight = 0;
		}
		public void FixedUpdate(Transform t, ref Vector3 vel, float speed, bool onGround, float gForce) {
			bool peakedAtStart = peaked, jumpingAtStart = isJumping;
			bool jpress = Pressed;
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
			if (jpress && onGround && !isJumping) {
				if(!lateButForgiven) { Start(t.position); }
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
				CalcJumpOverTime(now - jumpTime, gForce, speed, out float y, out float yVelocity);
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
		private void CalcJumpOverTime(long jumpMsSoFar, float gForce, float speed, out float yPos, out float yVel) {
			float jumptiming = jumpMsSoFar / 1000f;
			float jumpP = Mathf.Min(jumptiming / jumpMaxPressDuration, 1);
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
		jump.Stop();
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
