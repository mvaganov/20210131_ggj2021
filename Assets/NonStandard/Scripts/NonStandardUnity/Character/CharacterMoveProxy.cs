using UnityEngine;

namespace NonStandard.Character {
	public class CharacterMoveProxy : MonoBehaviour {
		[Tooltip("What character to pass input to")]
		[SerializeField] protected CharacterMove target;
		public Transform MoveTransform {
			get { return target != null ? target.transform : null; }
			set {
				Target = value.GetComponent<CharacterMove>();
				//if(target != null) {
				//	transform.parent = MoveTransform;
				//	transform.localPosition = Vector3.zero;
				//	transform.localRotation = Quaternion.identity;
				//}
			}
		}
		public CharacterMove Target {
			get { return target; }
			set {
				target = value;
				transform.parent = MoveTransform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}
		public float Jump {
			get { return target != null ? target.Jump : 0; }
			set { if (target != null) target.Jump = value; }
		}
		public float MoveSpeed {
			get { return target != null ? target.MoveSpeed : 0; }
			set { if (target != null) target.MoveSpeed = value; }
		}
		public float JumpHeight {
			get { return target != null ? target.JumpHeight : 0; }
			set { if (target != null) target.JumpHeight = value; }
		}
		public float StrafeRightMovement {
			get { return target != null ? target.StrafeRightMovement : 0; }
			set { if (target != null) target.StrafeRightMovement = value; }
		}
		public float MoveForwardMovement { 
			get { return target != null ? target.MoveForwardMovement : 0; }
			set { if (target != null) target.MoveForwardMovement = value; }
		}
		public bool IsAutoMoving() { return target.IsAutoMoving(); }
		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null, float closeEnough = 0) {
			if (target != null) { target.SetAutoMovePosition(position, whatToDoWhenTargetIsReached, closeEnough); }
		}
		public void DisableAutoMove() { if (target != null) target.DisableAutoMove(); }
		public float GetJumpProgress() { return target != null ? target.GetJumpProgress() : 0; }
		public bool IsStableOnGround() { return target != null ? target.IsStableOnGround() : false; }

		public GameObject groundShadow;
		public void Start() {
			if (groundShadow) { groundShadow.transform.SetParent(null); }
		}
		public void LateUpdate() {
			if (groundShadow) { UpdateGroundShadow(); }
		}
		public void UpdateGroundShadow() {
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit rh, 100)) {
				groundShadow.transform.position = rh.point;
				if (rh.normal != Vector3.up) {
					Vector3 r = Vector3.Cross(rh.normal, Vector3.up);
					Vector3 f = Vector3.Cross(rh.normal, r);
					groundShadow.transform.rotation = Quaternion.LookRotation(f, rh.normal);
				} else {
					groundShadow.transform.rotation = Quaternion.identity;
				}
				groundShadow.SetActive(true);
			} else {
				groundShadow.SetActive(false);
			}
		}
	}
}