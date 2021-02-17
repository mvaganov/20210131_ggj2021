using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi;
using NonStandard.GameUi.Inventory;
using NonStandard.Ui;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {
	public List<GameObject> members;
	public ListUi rosterUi;

	public ListItemUi AddMember(GameObject memberObject) {
		//Show.Log("adding member " + memberObject);
		if (members == null) { members = new List<GameObject>(); }
		// make sure this character adds to the communal inventory! (assuming there is one)
		TeamMember teamMember = memberObject.GetComponent<TeamMember>();
		if(teamMember == null) { teamMember = memberObject.AddComponent<TeamMember>(); }
		// add them to the roster
		if (members.IndexOf(memberObject) < 0) {
			members.Add(memberObject);
			teamMember.onJoinTeam?.Invoke(this);
		}
		if (rosterUi != null) {
			ListItemUi result = rosterUi.GetListItemUi(memberObject);
			if (result != null) {
				//Show.Log("already ui");
				return result;
			}
		}
		string name = teamMember != null ? teamMember.name : null;
		if(string.IsNullOrEmpty(name)) { name = memberObject.name; }

		Interact3dItem i3i = teamMember.GetComponent<Interact3dItem>();
		void ActivateTeamMember() {
			CharacterControlManager ccm = Global.Get<CharacterControlManager>();
			ccm.SetCharacter(memberObject);
			i3i.showing = false;
		}
		if (i3i != null) {
			i3i.OnInteract = ActivateTeamMember;
			i3i.Text = "switch";
			i3i.alwaysOn = true;
		}
		if (rosterUi == null) {
			Show.Log("missing roster UI");
			return null;
		}
		return rosterUi.AddItem(memberObject, name, ActivateTeamMember);
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
		rosterUi.RemoveItem(memberObject);
		TeamMember teamMember = memberObject.GetComponent<TeamMember>();
		teamMember?.onLeaveTeam?.Invoke(this);
	}
}
