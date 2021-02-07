using NonStandard;
using NonStandard.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlManager : MonoBehaviour
{
	public CharacterCamera cam;
	public CharacterMoveProxy moveProxy;
	public GameObject localPlayerInterfaceObject;

	public void SetCharacter(GameObject obj) {
		// TODO if the old target doesn't have an Interact3dItem (original player) make one
		// TODO turn on the Interact3dItem UI, which allows for easy swapping
		CharacterMove cm = obj.GetComponent<CharacterMove>();
		cam.target = cm != null && cm.head != null ? cm.head : obj.transform;
		moveProxy.target = cm;
		if (cm != null) { cm.move.orientationTransform = cam.transform; }
		Transform t = localPlayerInterfaceObject.transform;
		t.SetParent(obj.transform);
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		// TODO if the Interact3dItem is up, disable it.
	}

	void Start() {
		Global.Get<Team>().AddMember(moveProxy.target.gameObject);
	}
}
