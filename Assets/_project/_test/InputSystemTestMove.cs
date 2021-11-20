using NonStandard.Inputs;
using UnityEngine;

public class InputSystemTestMove : MonoBehaviour
{
	public InputSystemInterface inputSystemTest;
	Rigidbody rb;
	public bool fast = false;
	public float fastSpeed = 3;
	private void VelocityAdd(int dim, float value) { Vector3 v = rb.velocity; v[dim] += value; rb.velocity = v; }
	private void VelocityMult(int dim, float value) { Vector3 v = rb.velocity; v[dim] *= value; rb.velocity = v; }
	private void VelocitySet(int dim, float value) { Vector3 v = rb.velocity; v[dim] = value; rb.velocity = v; }

	private void Start() {
		rb = GetComponent<Rigidbody>();
		inputSystemTest.OnPressed[KCode.LeftShift] = (kCode, o) => fast = true;
		inputSystemTest.OnRelease[KCode.LeftShift] = (kCode, o) => fast = false;
		inputSystemTest.OnPressing[KCode.W] = (kCode, o) => VelocitySet(1, (fast ? fastSpeed : 1));
		inputSystemTest.OnPressing[KCode.A] = (kCode, o) => VelocitySet(0,-(fast ? fastSpeed : 1));
		inputSystemTest.OnPressing[KCode.S] = (kCode, o) => VelocitySet(1,-(fast ? fastSpeed : 1));
		inputSystemTest.OnPressing[KCode.D] = (kCode, o) => VelocitySet(0, (fast ? fastSpeed : 1));
		inputSystemTest.OnRelease[KCode.W] = (kCode, o) => VelocitySet(1, 0);
		inputSystemTest.OnRelease[KCode.A] = (kCode, o) => VelocitySet(0, 0);
		inputSystemTest.OnRelease[KCode.S] = (kCode, o) => VelocitySet(1, 0);
		inputSystemTest.OnRelease[KCode.D] = (kCode, o) => VelocitySet(0, 0);
	}
}
