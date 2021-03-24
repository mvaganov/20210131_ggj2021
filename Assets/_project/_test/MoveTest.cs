using NonStandard;
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
		public int doubleJumps = 2;
		public float TimedJumpPress;
		[HideInInspector] public float targetJumpHeight;
		[HideInInspector] public Vector3 position;
		[HideInInspector] public long jumpTime, peakTime;
		[HideInInspector] public int usedExtraJumps;
		[HideInInspector] Lines.Wire jumpArc;
		[HideInInspector]
		public bool isJumping, peaked, heightSet, Pressed, showJumpArc = false;
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
		}
		public void FixedUpdate(Transform transform, ref Vector3 vel, float speed, bool onGround, float gForce) {
			bool jpress = Pressed;
			if (onGround) { usedExtraJumps = 0; }
			if (TimedJumpPress > 0) {
				jpress = true; TimedJumpPress -= Time.deltaTime; if (TimedJumpPress < 0) { TimedJumpPress = 0; }
			}
			if (jpress && onGround && !isJumping) {
				Start(transform.position);
			}
			long now = Clock.NowTicks;
			bool peakedAtStart = peaked, jumpingAtStart = isJumping;
			if (isJumping) {
				if (!heightSet) {
					long jumpDurationSoFar = now - jumpTime;
					float jumptiming = (jumpDurationSoFar) / 1000f;
					float jumpP = Mathf.Min(jumptiming / jumpMaxPressDuration, 1);
					targetJumpHeight = (maxJumpHeight - minJumpHeight) * jumpP + minJumpHeight;
					float jVelocity = CalcJumpVelocity(targetJumpHeight, gForce);
					float jtime = 500 * CalcStandardJumpDuration_WithJumpVelocity(jVelocity, gForce);
					peakTime = jumpTime + (int)jtime;
					if (showJumpArc) {
						if (jumpArc == null) { jumpArc = Lines.MakeWire("jump arc").Line(Vector3.zero); }
						jumpArc.Line(CalcJumpPath_WithJumpVelocity(jVelocity, position, transform.forward, speed, targetJumpHeight, gForce), Color.red);
					}
					float y = CalcJumpHeightAt_WithJumpVelocity(jVelocity, jumptiming, targetJumpHeight, gForce);
					Vector3 p = transform.position;
					p.y = position.y + y;
					transform.position = p;
					vel.y = CalcJumpVelocityAt_WithJumpVelocity(jVelocity, jumptiming, gForce);
					if(jumpP >= 1) { heightSet = true; }
				}
				peaked = heightSet && now >= peakTime;
				isJumping = !peaked && jpress;
			} else {
				peaked = now >= peakTime;
			}
			if (!isJumping && jpress && usedExtraJumps < doubleJumps) {
				if (peaked && !peakedAtStart && jumpingAtStart) {
					float peakHeight = position.y + targetJumpHeight;
					position = transform.position;
					position.y = peakHeight;
				} else {
					position = transform.position;
				}
				Start(position);
				++usedExtraJumps;
			}
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
		onGround = true;
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

	static List<Vector3> CalcJumpPath(Vector3 position, Vector3 dir, float speed, float jumpHeight, float gForce) {
		return CalcJumpPath_WithJumpVelocity(CalcJumpVelocity(jumpHeight, gForce), position, dir, speed, jumpHeight, gForce);
	}
	static List<Vector3> CalcJumpPath_WithJumpVelocity(float jumpVelocity, Vector3 position, Vector3 dir, float speed, float jumpHeight, float gForce) {
		List<Vector3> points = new List<Vector3>();
		float stdJumpDuration = 2 * jumpVelocity / gForce;
		for (float t = 0; t < stdJumpDuration; t += 1f / 32) {
			float vAtPoint = t * gForce - jumpVelocity;
			float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jumpHeight;
			Vector3 pos = position + dir * (speed * t) + Vector3.up * y;
			points.Add(pos);
		}
		points.Add(position + dir * speed * stdJumpDuration);
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
