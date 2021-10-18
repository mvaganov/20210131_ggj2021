using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.Process;
using NonStandard.Ui;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterCamera : MonoBehaviour
	{
		[Tooltip("which transform to follow with the camera")]
		public Transform _target;
		[Tooltip("if false, camera can pass through walls")]
		public bool clipAgainstWalls = true;

		/// <summary>how the camera should be rotated, calculated in Update, to keep LateUpdate as light as possible</summary>
		private Quaternion targetRotation;
		/// <summary>how far the camera wants to be from the target</summary>
		public float targetDistance = 10;
		/// <summary>calculate how far to clip the camera in the Update, to keep LateUpdate as light as possible
		private float distanceBecauseOfObstacle;
		/// <summary>
		/// user-defined rotation
		/// </summary>
		private Quaternion userRotation;
		/// <summary>
		/// user-defined zoom
		/// </summary>
		public float userDistance;
		private Transform userTarget;
		/// <summary>for fast access to transform</summary>
		private Transform t;

		private Camera cam;

		/// <summary>keep track of rotation, so it can be un-rotated and cleanly re-rotated</summary>
		private float pitch, yaw;

		public float maxVerticalAngle = 100, minVerticalAngle = -100;
		public Vector2 inputMultiplier = Vector2.one;

		public Transform target { get { return _target; } 
			set {
				//Debug.Log("target! "+Show.GetStack(10));
				_target = userTarget = value; 
			}
		}

		/// publicly accessible variables that can be modified by external scripts or UI
		[HideInInspector] public float horizontalRotateInput, verticalRotateInput, zoomInput;
		public float HorizontalRotateInput { get { return horizontalRotateInput; }
			set { horizontalRotateInput = inputMultiplier.x == 1 ? value : inputMultiplier.x * value; }
		}
		public float VerticalRotateInput { get { return verticalRotateInput; } 
			set { verticalRotateInput = inputMultiplier.y == 1 ? value : inputMultiplier.y * value; }
		}
		public float ZoomInput { get { return zoomInput; } set { zoomInput = value; } }
		public void AddToTargetDistance(float value) {
			targetDistance += value;
			if(targetDistance < 0) { targetDistance = 0; }
			OrthographicCameraDistanceChangeLogic();
		}

		public void OrthographicCameraDistanceChangeLogic() {
			if (cam != null && cam.orthographic) {
				if (targetDistance < 1f / 128) { targetDistance = 1f / 128; }
				cam.orthographicSize = targetDistance / 2;
			}
		}

		public void ToggleOrthographic() { cam.orthographic = !cam.orthographic; }
		public void SetCameraOrthographic(bool orthographic) { cam.orthographic = orthographic; }
	
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
			userTarget = target;
			userRotation = t.rotation;
			userDistance = targetDistance;
			targetView.target = userTarget;
			targetView.rotation = userRotation;
			targetView.distance = userDistance;
			cam = GetComponent<Camera>();
			//for (int i = 0; i < knownCameraViews.Count; ++i) {
			//	knownCameraViews[i].ResolveLookRotation();
			//}
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
			targetDistance -= zoom;
			if (zoom != 0) {
				userDistance = targetDistance;
				if (targetDistance < 0) { targetDistance = 0; }
				if (target == null) {
					t.position += t.forward * zoom;
				}
				OrthographicCameraDistanceChangeLogic();
			}
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
				userRotation = targetRotation;
			}
			if (target != null) {
				RaycastHit hitInfo;
				bool usuallyHitsTriggers = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = false;
				if (clipAgainstWalls && Physics.Raycast(target.position, -t.forward, out hitInfo, targetDistance)) {
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
		[System.Serializable] public class CameraView {
			public string name;
			[HideInInspector] public Quaternion rotation;
			[SerializeField] public Vector3 _Rotation;
			/// <summary>
			/// if target is null, use this
			/// </summary>
			public Vector3 position;
			public Transform target;
			public float distance;
			public bool useTransformPositionChanges;
			public bool ignoreLookRotationChanges;
			public bool rotationIsLocal;
			public bool positionLocalToLastTransform;
			public void ResolveLookRotationIfNeeded() {
				if(rotation.x==0&&rotation.y==0&&rotation.z==0&&rotation.w==0) {
					rotation = Quaternion.Euler(_Rotation);
				}
				//Debug.Log(Show.Stringify(this));
			}
		}
		public List<CameraView> knownCameraViews = new List<CameraView>();

		public void ToggleView(string viewName) {
			if(currentViewname != defaultViewName) {
				LerpView(defaultViewName);
			} else {
				LerpView(viewName);
			}
		}
		private string defaultViewName = "user";
		private string currentViewname = "user";
		public string CurrentViewName { get { return currentViewname; } }
		public void SetLerpSpeed(float durationSeconds) { lerpDurationMs = (long)(durationSeconds*1000); }
		private long started, end;
		public long lerpDurationMs = 250;
		private float distStart;
		private Quaternion rotStart;
		private bool lerping = false;
		private Vector3 startPosition;
		private CameraView targetView = new CameraView();
		public void LerpView(string viewName) {
			currentViewname = viewName;
			string n = viewName.ToLower();
			switch (n) {
			case "user":
				LerpRotation(userRotation);
				LerpDistance(userDistance);
				LerpTarget(userTarget);
				return;
			default:
				for(int i = 0; i < knownCameraViews.Count; ++i) {
					if (knownCameraViews[i].name.ToLower().Equals(n)) {
						//Debug.Log("doing " + n + " "+Show.Stringify(knownCameraViews[i]));
						LerpTo(knownCameraViews[i]);
						return;
					}
				}
				break;
			}
			ReflectionParseExtension.TryConvertEnumWildcard(typeof(Direction3D), viewName, out object v);
			if (v != null) {
				LerpDirection((Direction3D)v); return;
			}
			Debug.LogWarning($"unkown view name \"{viewName}\"");
		}
		public void LerpDirection(Direction3D dir) { LerpDirection(dir.GetVector3()); }
		public void LerpDirection(Vector3 direction) { LerpRotation(Quaternion.LookRotation(direction)); }
		public void LerpRotation(Quaternion direction) {
			targetView.rotation = direction;
			StartLerpToTarget();
		}
		public void LerpDistance(float distance) {
			targetView.distance = distance;
			StartLerpToTarget();
		}
		public void LerpTarget(Transform target) {
			targetView.target = target;
			StartLerpToTarget();
		}
		public void LerpTo(CameraView view) {
			targetView.name = view.name;
			targetView.useTransformPositionChanges = view.useTransformPositionChanges;
			targetView.ignoreLookRotationChanges = view.ignoreLookRotationChanges;
			targetView.rotationIsLocal = view.rotationIsLocal;
			targetView.positionLocalToLastTransform = view.positionLocalToLastTransform;
			if (view.useTransformPositionChanges) { targetView.target = view.target; }
			targetView.distance = view.distance;
			if (!view.ignoreLookRotationChanges) {
				view.ResolveLookRotationIfNeeded();
				targetView.rotation = view.rotation;
			}
			StartLerpToTarget();
		}
		public void StartLerpToTarget() {
			if (lerping) return;
			lerping = true;
			rotStart = t.rotation;
			startPosition = t.position;
			distStart = distanceBecauseOfObstacle;
			if (targetView.positionLocalToLastTransform && _target != null) {
				Quaternion q = !targetView.ignoreLookRotationChanges ? targetView.rotation : t.rotation;
				targetView.position = _target.position - (q * Vector3.forward) * targetView.distance;
				Debug.Log("did the thing");
			}
			//if (targetView.target != null) {
				_target = null;
			//}
			started = Proc.Time;
			end = Proc.Time + lerpDurationMs;
			Proc.Delay(0, LerpToTarget);
		}
		private void LerpToTarget() {
			lerping = true;
			long now = Proc.Time;
			long passed = now - started;
			float p = (float)passed / lerpDurationMs;
			if (now >= end) { p = 1; }
			if (!targetView.ignoreLookRotationChanges) {
				targetView.ResolveLookRotationIfNeeded();
				if (targetView.rotationIsLocal) {
					Quaternion startQ = targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity;
					Quaternion.Lerp(rotStart, targetView.rotation * startQ, p);
				} else {
					t.rotation = Quaternion.Lerp(rotStart, targetView.rotation, p);
				}
			}
			//Show.Log("asdfdsafdsa");
			targetDistance = (targetView.distance - distStart) * p + distStart;
			if (targetView.useTransformPositionChanges) {
				if (targetView.target != null) {
					Quaternion rot = targetView.rotation * (targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity);
					Vector3 targ = targetView.target.position;
					Vector3 dir = rot * Vector3.forward;
					RaycastHit hitInfo;
					if (clipAgainstWalls && Physics.Raycast(targ, -dir, out hitInfo, targetView.distance)) {
						distanceBecauseOfObstacle = hitInfo.distance;
					} else {
						distanceBecauseOfObstacle = targetView.distance;
					}
					Vector3 finalP = targ - dir * distanceBecauseOfObstacle;
					//Debug.Log(targetView.distance+"  "+distanceBecauseOfObstacle+"  "+targ+" "+targetView.target);
					t.position = Vector3.Lerp(startPosition, finalP, p);
					//Debug.Log("# "+p+" "+finalP);
				} else {
					t.position = Vector3.Lerp(startPosition, targetView.position, p);
					//Debug.Log("!" + p + " " + targetView.position);
				}
			}
			RecalculateRotation();
			if (p < 1) { Proc.Delay(20, LerpToTarget); } else {
				if (targetView.useTransformPositionChanges) {
					_target = targetView.target;
				}
				lerping = false;
			}
		}
	}
}