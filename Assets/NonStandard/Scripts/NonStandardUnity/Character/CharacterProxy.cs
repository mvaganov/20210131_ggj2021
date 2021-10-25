using NonStandard.Inputs;
using NonStandard.Utility.UnityEditor;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterProxy : MonoBehaviour {
		[Tooltip("What character to pass input to")]
		[ContextMenuItem("Create default user controls", "CreateDefaultUserControls")]
		[SerializeField] protected CharacterRoot target;
		public Transform MoveTransform {
			get { return target != null ? target.transform : null; }
			set {
				Target = value.GetComponent<CharacterRoot>();
				//if(target != null) {
				//	transform.parent = MoveTransform;
				//	transform.localPosition = Vector3.zero;
				//	transform.localRotation = Quaternion.identity;
				//}
			}
		}
		public CharacterRoot Target {
			get { return target; }
			set {
				target = value;
				transform.parent = MoveTransform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}
		public float Jump {
			get { return target != null ? target.move.Jump : 0; }
			set { if (target != null) target.move.Jump = value; }
		}
		public float MoveSpeed {
			get { return target != null ? target.move.MoveSpeed : 0; }
			set { if (target != null) target.move.MoveSpeed = value; }
		}
		public float JumpHeight {
			get { return target != null ? target.move.JumpHeight : 0; }
			set { if (target != null) target.move.JumpHeight = value; }
		}
		public float StrafeRightMovement {
			get { return target != null ? target.move.StrafeRightMovement : 0; }
			set { if (target != null) target.move.StrafeRightMovement = value; }
		}
		public float MoveForwardMovement { 
			get { return target != null ? target.move.MoveForwardMovement : 0; }
			set { if (target != null) target.move.MoveForwardMovement = value; }
		}
		public bool IsAutoMoving() { return target.move.IsAutoMoving(); }
		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null, float closeEnough = 0) {
			if (target != null) { target.move.SetAutoMovePosition(position, whatToDoWhenTargetIsReached, closeEnough); }
		}
		public void DisableAutoMove() { if (target != null) target.move.DisableAutoMove(); }
		public float GetJumpProgress() { return target != null ? target.move.GetJumpProgress() : 0; }
		public bool IsStableOnGround() { return target != null ? target.move.IsStableOnGround() : false; }

		public void CreateDefaultUserControls() {
			GameObject userInput = gameObject;// new GameObject(name + " input");
			Transform t = userInput.transform;
			//t.SetParent(transform);
			//t.localPosition = Vector3.zero;
			CharacterCamera camera = target.GetComponentInChildren<CharacterCamera>();
			if (camera == null) { camera = GetComponentInParent<CharacterCamera>(); }
			UserInput mouseLook = userInput.AddComponent<UserInput>();
			if (camera != null) {
				mouseLook.AxisBinds.Add(new AxBind(new Axis(AppInput.StandardAxis.MouseX, 5), "mouselook X",
					camera, "set_HorizontalRotateInput"));
				mouseLook.AxisBinds.Add(new AxBind(new Axis(AppInput.StandardAxis.MouseY, 5), "mouselook Y",
					camera, "set_VerticalRotateInput"));
			}
			mouseLook.enabled = false;
			UserInput userMoves = userInput.AddComponent<UserInput>();
			KBind rightClick = new KBind(KCode.Mouse1, "use mouselook",
				pressFunc: new EventBind(mouseLook, "set_enabled", true),
				releaseFunc: new EventBind(mouseLook, "set_enabled", false));
			rightClick.keyEvent.AddPress(camera, "SetMouseCursorLock", true);
			rightClick.keyEvent.AddRelease(camera, "SetMouseCursorLock", false);
			userMoves.KeyBinds.Add(rightClick);
			userMoves.KeyBinds.Add(new KBind(KCode.PageUp, "zoom in",
				pressFunc: new EventBind(camera, "set_ZoomInput", -5f),
				releaseFunc: new EventBind(camera, "set_ZoomInput", 0f)));
			userMoves.KeyBinds.Add(new KBind(KCode.PageDown, "zoom out",
				pressFunc: new EventBind(camera, "set_ZoomInput", 5f),
				releaseFunc: new EventBind(camera, "set_ZoomInput", 0f)));
			userMoves.KeyBinds.Add(new KBind(KCode.Space, "jump",
				pressFunc: new EventBind(this, "set_Jump", 1f),
				releaseFunc: new EventBind(this, "set_Jump", 0f)));
			userMoves.AxisBinds.Add(new AxBind(new Axis(AppInput.StandardAxis.Horizontal), "strafe right/left",
				this, "set_StrafeRightMovement"));
			userMoves.AxisBinds.Add(new AxBind(new Axis(AppInput.StandardAxis.Vertical), "move forward/backward",
				this, "set_MoveForwardMovement"));
			userMoves.AxisBinds.Add(new AxBind(new Axis(AppInput.StandardAxis.MouseScrollY, -4), "zoom in/out",
				camera, "AddToTargetDistance"));
		}

	}
}