﻿using NonStandard.Data;
using NonStandard.Ui;
using System;
using System.Text;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterCamera : MonoBehaviour
	{
		[Tooltip("which transform to follow with the camera")]
		public Transform target;
		[Tooltip("if false, camera can pass through walls")]
		public bool clipAgainstWalls = true;

		/// <summary>how far the camera wants to be from the target</summary>
		public float targetDistance = 10;
		/// <summary>calculate how far to clip the camera in the Update, to keep LateUpdate as light as possible
		private float distanceBecauseOfObstacle;
		/// <summary>how the camera should be rotated, calculated in Update, to keep LateUpdate as light as possible</summary>
		private Quaternion targetRotation;
		/// <summary>for fast access to transform</summary>
		private Transform t;

		/// <summary>keep track of rotation, so it can be un-rotated and cleanly re-rotated</summary>
		private float pitch, yaw;

		public float maxVerticalAngle = 100, minVerticalAngle = -100;
		public Vector2 inputMultiplier = Vector2.one;

		/// publicly accessible variables that can be modified by external scripts or UI
		[HideInInspector] public float horizontalRotateInput, verticalRotateInput, zoomInput;
		public float HorizontalRotateInput { get { return horizontalRotateInput; }
			set { horizontalRotateInput = inputMultiplier.x == 1 ? value : inputMultiplier.x * value; }
		}
		public float VerticalRotateInput { get { return verticalRotateInput; } 
			set { verticalRotateInput = inputMultiplier.y == 1 ? value : inputMultiplier.y * value; }
		}
		public float ZoomInput { get { return zoomInput; } set { zoomInput = value; } }
		public void AddToTargetDistance(float value) { targetDistance += value; }

	#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (target == null) {
				CharacterMove body = null;
				if (body == null) { body = transform.GetComponentInParent<CharacterMove>(); }
				if (body == null) { body = FindObjectOfType<CharacterMove>(); }
				if (body != null) { target = body.head; }
			}
		}
	#endif

		public void SetMouseCursorLock(bool a_lock) {
			Cursor.lockState = a_lock ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !a_lock;
		}
		public void LockCursor() { SetMouseCursorLock(true); }
		public void UnlockCursor() { SetMouseCursorLock(false); }

		public void Awake() { t = transform; }

		public void Start() {
			RecalculateDistance();
			RecalculateRotation();
		}

		public bool RecalculateDistance() {
			float oldDist = targetDistance;
			if (target != null) {
				Vector3 delta = t.position - target.position;
				targetDistance = delta.magnitude;
			}
			return oldDist != targetDistance;
		}
		public bool RecalculateRotation() {
			float oldP = pitch, oldY = yaw;
			targetRotation = t.rotation;
			Vector3 right = Vector3.Cross(t.forward, Vector3.up);
			if(right == Vector3.zero) { right = -t.right; }
			Vector3 straightForward = Vector3.Cross(Vector3.up, right).normalized;
			pitch = Vector3.SignedAngle(straightForward, t.forward, -right);
			yaw = Vector3.SignedAngle(Vector3.forward, straightForward, Vector3.up);
			return oldP != pitch || oldY != yaw;
		}

		public void Update() {
			const float anglePerSecondMultiplier = 100;
			float rotH = horizontalRotateInput * anglePerSecondMultiplier * Time.unscaledDeltaTime,
				rotV = verticalRotateInput * anglePerSecondMultiplier * Time.unscaledDeltaTime,
				zoom = zoomInput * Time.unscaledDeltaTime;
			targetDistance += zoom;
			if (rotH != 0 || rotV != 0) {
				targetRotation = Quaternion.identity;
				yaw += rotH;
				pitch -= rotV;
				if (yaw < -180) { yaw += 360; }
				if (yaw >= 180) { yaw -= 360; }
				if (pitch < -180) { pitch += 360; }
				if (pitch >= 180) { pitch -= 360; }
				pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
				targetRotation *= Quaternion.Euler(pitch, yaw, 0);
			}
			if (targetDistance < 0) { targetDistance = 0; }
			if (target != null) {
				RaycastHit hitInfo;
				bool usuallyHitsTriggers = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = false;
				if (clipAgainstWalls && Physics.Raycast(target.position, -t.forward, out hitInfo, targetDistance))
				{
					distanceBecauseOfObstacle = hitInfo.distance;
				} else {
					distanceBecauseOfObstacle = targetDistance;
				}
				Physics.queriesHitTriggers = usuallyHitsTriggers;
			}
		}

		public void LateUpdate() {
			t.rotation = targetRotation;
			if(target != null) {
				t.position = target.position - (t.rotation * Vector3.forward) * distanceBecauseOfObstacle;
			}
		}

		public void SetLerpSpeed(float durationSeconds) { duration = (long)(durationSeconds*1000); }
		private long started, end, duration = 250;
		private float distStart, distEnd;
		private Quaternion rotStart, rotEnd;
		private bool lerping = false;
		public void LerpDirection(string dir) {
			CodeConvert.TryConvertEnumWildcard(typeof(Direction3D), dir, out object v);
			if (v != null) { LerpDirection((Direction3D)v); }
		}
		public void LerpDirection(Direction3D dir) { LerpDirection(dir.GetVector3()); }
		public void LerpDirection(Vector3 direction) {
			rotStart = t.rotation;
			rotEnd = Quaternion.LookRotation(direction);
			started = Clock.NowRealtime;
			end = Clock.NowRealtime + duration;
			if(!lerping) LerpToTarget();
		}
		public void LerpDistance(float distance) {
			distStart = distanceBecauseOfObstacle;
			distEnd = distance;
			if (!lerping) LerpToTarget();
		}
		public void LerpToTarget() {
			lerping = true;
			long now = Clock.NowRealtime;
			if (now >= end) {
				t.rotation = rotEnd;
				RecalculateRotation();
				targetDistance = distEnd;
				lerping = false;
				return;
			}
			long passed = now - started;
			float p = (float)passed / duration;
			t.rotation = Quaternion.Lerp(rotStart, rotEnd, p);
			targetDistance = (distEnd - distStart) * p + distStart;
			RecalculateRotation();
			Clock.setTimeoutRealtime(LerpToTarget, 20);
		}
	}
}