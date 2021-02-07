using NonStandard;
using NonStandard.Character;
using System.Collections.Generic;
using UnityEngine;

// TODO make single base class for Team and Inventory...
public class Team : MonoBehaviour {
	public List<GameObject> members;
	public ListUi inventoryUi;

	public ListItemUi AddMember(GameObject memberObject) {
		if (members == null) { members = new List<GameObject>(); }
		if (inventoryUi != null) { ListItemUi result = inventoryUi.GetListItemUi(memberObject); if (result != null) return result; }
		members.Add(memberObject);
		TeamMember teamMember = memberObject.GetComponent<TeamMember>();
		string name = teamMember != null ? teamMember.name : null;
		if(string.IsNullOrEmpty(name)) { name = memberObject.name; }
		teamMember.onJoinTeam?.Invoke(this);
		// TODO swap out the onInteract behavior from this object's interact3dItem to be the same as the ui item script
		Inventory inv = memberObject.GetComponentInChildren<Inventory>();
		if (inv != null) { inv.proxyFor = Global.Get<Inventory>(); }
		if (inventoryUi == null) { return null; }
		return inventoryUi.AddItem(memberObject, name, () => {
			CharacterControlManager ccm = Global.Get<CharacterControlManager>();
			ccm.SetCharacter(memberObject);
			//RemoveMember(memberObject);
		});
	}
	public GameObject FindItem(string name) {
		if (members == null) return null;
		// TODO wildcard search
		return members.Find(i => i.name == name);
	}
	public GameObject RemoveMember(string name) {
		GameObject go = FindItem(name);
		if(go != null) {
			RemoveMember(go);
		}
		return go;
	}
	public void RemoveMember(GameObject memberObject) {
		if (members != null) { members.Remove(memberObject); }
		inventoryUi.RemoveItem(memberObject);
		TeamMember teamMember = memberObject.GetComponent<TeamMember>();
		teamMember?.onLeaveTeam?.Invoke(this);
	}
}
