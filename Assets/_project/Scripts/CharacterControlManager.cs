using NonStandard;
using NonStandard.Character;
using UnityEngine;

public class CharacterControlManager : MonoBehaviour
{
	public CharacterCamera cam;
	public CharacterMoveProxy moveProxy;
	public GameObject localPlayerInterfaceObject;

	public void SetCharacter(GameObject obj) {
		if (moveProxy != null) {
			//Debug.Log("Switching from " + moveProxy.target);
			Interact3dItem i3i = moveProxy.target.GetComponent<Interact3dItem>();
			if(i3i != null) i3i.showing = true;
		}
		CharacterMove cm = obj.GetComponent<CharacterMove>();
		cam.target = cm != null && cm.head != null ? cm.head : obj.transform;
		moveProxy.target = cm;
		if (cm != null) { cm.move.orientationTransform = cam.transform; }
		Transform t = localPlayerInterfaceObject.transform;
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
		Global.Get<Team>().AddMember(moveProxy.target.gameObject);
	}
}
