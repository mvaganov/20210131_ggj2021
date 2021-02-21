using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterMoveProxy : MonoBehaviour {
		public CharacterMove target;
		public Transform MoveTransform {
			get { return target != null ? target.transform : null; }
			set {
				target = value.GetComponent<CharacterMove>();
				if(target != null) {
					transform.parent = MoveTransform;
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
				}
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
	}
}