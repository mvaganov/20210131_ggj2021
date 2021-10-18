using NonStandard.Utility.UnityEditor;
using UnityEngine;

namespace NonStandard.Character
{
	public class SuperCyanGlue : MonoBehaviour
	{
		public Animator animator;
		[ContextMenuItem("Bind Events", "BindEvents")]
		public CharacterRoot character;
		private void Start() {
			Init();
		}
		private void Init() {
			if (animator == null) { animator = GetComponent<Animator>(); }
			if (animator == null) { animator = GetComponentInParent<Animator>(); }
			if (character == null) { character = GetComponent<CharacterRoot>(); }
			if (character == null) { character = GetComponentInParent<CharacterRoot>(); }
			if (character == null) {
				Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { character = f.whoToFollow.GetComponent<CharacterRoot>(); }
			}
			if (character) character.Init();
			//cb.jumped.AddListener(Jump);
			//cb.stand.AddListener(Stand);
			//cb.fall.AddListener(Fall);
			//cb.arrived.AddListener(Wave);
		}
		public void BindEvents() {
			Init();
			CharacterMove.Callbacks cb = character.move.callbacks;
			EventBind.On(cb.jumped, this, nameof(Jump));
			EventBind.On(cb.stand, this, nameof(Stand));
			EventBind.On(cb.fall, this, nameof(Fall));
			EventBind.On(cb.arrived, this, nameof(Wave));
		}

		bool shouldTriggerJumpAnimation = false;
		public void Jump(Vector3 dir) {
			//Show.Log("jump");
			//animator.SetTrigger("Land");
			animator.SetBool("Grounded", false);
			if (character.move.IsStableOnGround()) {
				shouldTriggerJumpAnimation = true;
			}
		}

		public void Stand(Vector3 upDir) {
			//Show.Log("stand");
			animator.SetTrigger("Land");
			animator.SetBool("Grounded", true);
		}

		public void Fall() {
			//Show.Log("fall");
			animator.SetBool("Grounded", false);
		}

		public void Wave(Vector3 location) {
			//Show.Log("wave");
			animator.SetTrigger("Wave");
		}

		public void FixedUpdate() {
			if (animator.GetBool("Grounded")) {
				float speed = character.move.rb.velocity.magnitude;
				animator.SetFloat("MoveSpeed", speed);
			}
			if (shouldTriggerJumpAnimation && !animator.IsInTransition(0)) {
				animator.SetTrigger("Jump");
				animator.SetBool("Grounded", false);
				shouldTriggerJumpAnimation = false;
			}
		}
	}
}