using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi;
using NonStandard.Ui;
using NonStandard.Utility.UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Team : MonoBehaviour {
	public List<GameObject> members;
	public ListUi rosterUi;
	public Button prev, next, team;
	private int currentIndex = 0;

	public void Next() { if (++currentIndex >= members.Count) { currentIndex = 0; } ActivateTeamMember(members[currentIndex]); }
	public void Prev() { if (--currentIndex < 0) { currentIndex = members.Count-1; } ActivateTeamMember(members[currentIndex]); }
	public void Start() {
		prev.onClick.AddListener(Prev);
		next.onClick.AddListener(Next);
		if(members == null || members.Count < 1) {
			prev?.gameObject.SetActive(false);
			next?.gameObject.SetActive(false);
		}
	}

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
			CharacterRoot cr = memberObject.GetComponent<CharacterRoot>();
			if (cr) {
				EventBind.IfNotAlready(cr.activateFunction, this, ActivateTeamMember);
			}
		}
		if (members.Count > 1) {
			prev?.gameObject.SetActive(true);
			next?.gameObject.SetActive(true);
			team?.gameObject.SetActive(true);
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
		//void ActivateTeamMember() {
		//	CharacterControlManager ccm = Global.Get<CharacterControlManager>();
		//	ccm.SetCharacter(memberObject);
		//	i3i.showing = false;
		//}
		Action activateMember = () => ActivateTeamMember(memberObject);
		if (i3i != null) {
			i3i.OnInteract = activateMember;
			i3i.Text = "switch";
			i3i.internalState.alwaysOn = true;
		}
		if (rosterUi == null) {
			Show.Log("missing roster UI");
			return null;
		}
		return rosterUi.AddItem(memberObject, name, activateMember);
	}
	public void ActivateTeamMember(GameObject memberObject) {
		CharacterControlManager ccm = Global.GetComponent<CharacterControlManager>();
		Interact3dItem i3i = ccm.moveProxy.GetComponent<Interact3dItem>();
		if (i3i != null) { i3i.showing = true; }
		ccm.SetCharacter(memberObject);
		i3i = memberObject.GetComponent<Interact3dItem>();
		if (i3i != null) { i3i.showing = false; }
	}
	public int FindItemIndex(string name) {
		if (members == null) return -1;
		// TODO wildcard search
		return members.FindIndex(i => i.name == name);
	}
	public GameObject RemoveMember(string name) {
		int index = FindItemIndex(name);
		GameObject go = null;
		if (index >= 0) {
			go = members[index];
			RemoveMemberAt(index);
		}
		return go;
	}
	public void RemoveMemberAt(int index) {
		GameObject go = members[index];
		if (members != null) { members.RemoveAt(index); }
		rosterUi.RemoveItem(go);
		TeamMember teamMember = go.GetComponent<TeamMember>();
		teamMember?.onLeaveTeam?.Invoke(this);
		if (index == currentIndex) {
			Prev();
		} else if (index > currentIndex) {
			--currentIndex;
		}
	}

	internal void Clear() {
		members.Clear();
		prev?.gameObject.SetActive(false);
		next?.gameObject.SetActive(false);
	}
}
