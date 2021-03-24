using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public float speed = 5;

	[System.Serializable]
	public class JumpModule {
		public float minJumpHeight = .25f;
		public float maxJumpHeight = 2;
		public float jumpMaxPressDuration = .25f;
		public float targetJumpHeight;
		public Vector3 position;
		public bool isJumping, peaked, heightSet, jumpRequested;
		public long jumpTime, peakTime;
		public int usedExtraJumps, extraJumps = 3;
		Lines.Wire jumpArc;
		float gForce;

		public void Init() {
			jumpArc = Lines.MakeWire().Line(Vector3.zero);
		}
		public void Start(Vector3 p) {
			jumpTime = Clock.NowTicks;
			peakTime = 0;
			isJumping = true;
			gForce = -Physics.gravity.y;
			peaked = false;
			heightSet = false;
			position = p;
		}
		public void Collided() {
			//Debug.Log("Collided!!!");
			isJumping = false;
			heightSet = true;
		}
		public int tillPeak;
		public void FixedUpdate(Transform transform, ref Vector3 vel, float speed) {
			long now = Clock.NowTicks;
			long jumpDurationSoFar = now - jumpTime;
			bool peakedAtStart = peaked, jumpingAtStart = isJumping;
			if (isJumping) {
				if (!heightSet) {
					float jumptiming = (jumpDurationSoFar) / 1000f;
					float jumpP = Mathf.Min(jumptiming / jumpMaxPressDuration, 1);
					targetJumpHeight = (maxJumpHeight - minJumpHeight) * jumpP + minJumpHeight;
					float jtime = CalcStandardJumpDuration(targetJumpHeight, gForce)/2;
					peakTime = jumpTime + (int)(1000 * jtime);
					jumpArc.Line(CalcJumpPath(position, transform.forward, speed, targetJumpHeight, gForce), Color.red);
					float y = CalcJumpHeightAt(jumptiming, targetJumpHeight, gForce);
					Vector3 p = transform.position;
					p.y = position.y + y;
					transform.position = p;
					vel.y = CalcFallVelocityAt(jumptiming, targetJumpHeight, gForce);
					if(jumpP >= 1) { heightSet = true; }
				}
				tillPeak = (int)(peakTime - now);
				peaked = heightSet && now >= peakTime;
				isJumping = !peaked && jumpRequested;
			}
			if (!isJumping && jumpRequested && usedExtraJumps < extraJumps) {
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

	public JumpModule jump = new JumpModule();

	Rigidbody rb;
	public float moveH, moveV;
	public bool onGround;

	void Start() {
        rb = GetComponent<Rigidbody>();
		jump.Init();
	}

	void Update() {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");
		jump.jumpRequested = Input.GetButton("Jump");
		if (Input.GetButtonDown("Jump") && onGround) {
			jump.Start(transform.position);
		}
	}

	public void OnCollisionEnter(Collision collision) {
		jump.Collided();
		onGround = true;
		jump.usedExtraJumps = 0;
	}
	public void OnCollisionExit(Collision collision) {
		onGround = false;
	}
	private void FixedUpdate() {
		Vector3 vel = rb.velocity;
		vel.x = moveH * speed;
		vel.z = moveV * speed;
		jump.FixedUpdate(transform, ref vel, speed);
		rb.velocity = vel;
		if (moveH != 0 || moveV != 0) {
			Vector3 dir = new Vector3(moveH, 0, moveV);
			dir.Normalize();
			transform.rotation = Quaternion.LookRotation(dir);
		}
	}

	static List<Vector3> CalcJumpPath(Vector3 position, Vector3 dir, float speed, float jumpHeight, float gForce) {
		List<Vector3> points = new List<Vector3>();
		float timeToReachHeight = CalcJumpVelocity(jumpHeight, gForce);
		float stdJumpDuration = 2 * timeToReachHeight / gForce;
		for (float t = 0; t < stdJumpDuration; t += 1f / 32) {
			float vAtPoint = t * gForce - timeToReachHeight;
			float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jumpHeight;
			Vector3 pos = position + dir * (speed * t) + Vector3.up * y;
			points.Add(pos);
		}
		points.Add(position + dir * speed * stdJumpDuration);
		return points;
	}
	static float CalcJumpVelocity(float jumpHeight, float gForce) { return Mathf.Sqrt(2 * jumpHeight * gForce); }
	static float CalcJumpHeightAt(float time, float jumpHeight, float gForce) {
		float vAtPoint = CalcFallVelocityAt(time, jumpHeight, gForce);
		float y = -(vAtPoint * vAtPoint) / (gForce * 2) + jumpHeight;
		return y;
	}
	static float CalcFallVelocityAt(float time, float jumpHeight, float gForce) {
		return -(time * gForce - CalcJumpVelocity(jumpHeight, gForce));
	}
	static float CalcStandardJumpDuration(float jumpHeight, float gForce) {
		return 2 * CalcJumpVelocity(jumpHeight, gForce) / gForce;
	}
}
