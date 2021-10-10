using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi;
using UnityEngine;

public class CharacterControlManager : MonoBehaviour
{
	public CharacterCamera cam;
	public CharacterProxy moveProxy;
	public GameObject localPlayerInterfaceObject;
	public ClickToMove clickToMove;
	public FourthPersonController fourthPersonControl;

	public void SetCharacter(GameObject obj) {
		if (moveProxy != null) {
			//Debug.Log("Switching from " + moveProxy.Target);
			if (moveProxy.Target != null) {
				Interact3dItem i3i = moveProxy.Target.GetComponent<Interact3dItem>();
				if (i3i != null) i3i.showing = true;
			}
		}
		CharacterRoot cm = obj.GetComponent<CharacterRoot>();
		if (moveProxy.Target != fourthPersonControl.GetMover()) {
			cam.target = cm != null && cm.move.head != null ? cm.move.head : obj.transform;
			moveProxy.Target = cm;
		}
		if (cm != null) { cm.move.move.orientationTransform = cam.transform; }
		clickToMove.SetFollower(cm.move);
		Transform t = localPlayerInterfaceObject.transform;
		Interact3dUi.TriggerArea ta = t.GetComponent<Interact3dUi.TriggerArea>();
		ta.Blink();
		t.SetParent(obj.transform);
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		// TODO if the Interact3dItem is up, disable it.
		if (moveProxy != null) {
			Interact3dItem i3i = moveProxy.GetComponent<Interact3dItem>();
			if (i3i != null) i3i.showing = false;
		}
	}

	void Start() {
		Global.GetComponent<Team>().AddMember(moveProxy.Target.gameObject);
	}
}
