using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterMoveProxy : MonoBehaviour {
		public CharacterMove target;
		public Transform MoveTransform {
			get { return target.transform; }
			set {
				target = value.GetComponent<CharacterMove>();
				if(target != null) {
					transform.parent = MoveTransform;
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
				}
			}
		}
		public float Jump { get { return target.Jump; } set { target.Jump = value; } }
		public float MoveSpeed { get { return target.MoveSpeed; } set { target.MoveSpeed = value; } }
		public float JumpHeight { get { return target.JumpHeight; } set { target.JumpHeight = value; } }
		public float StrafeRightMovement { get { return target.StrafeRightMovement; } set { target.StrafeRightMovement = value; } }
		public float MoveForwardMovement { get { return target.MoveForwardMovement; } set { target.MoveForwardMovement = value; } }
		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null) {
			target.SetAutoMovePosition(position, whatToDoWhenTargetIsReached);
		}
		public void DisableAutoMove() { target.DisableAutoMove(); }
		public float GetJumpProgress() { return target.GetJumpProgress(); }
		public bool IsStableOnGround() { return target.IsStableOnGround(); }
	}
}