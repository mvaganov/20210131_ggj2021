using NonStandard.Character;
using System.Collections.Generic;
using UnityEngine;

public class SelectionUi : MonoBehaviour
{
	public ClickToMove c2m;
    public List<ClickToMoveFollower> selection = new List<ClickToMoveFollower>();
    public GameObject prefab_selectionObject;
    public bool showSelectionObject;

#if UNITY_EDITOR
	/// called when created by Unity Editor
	void Reset() {
		ResetSelectionVisual();
	}
#endif
	class SelectedVisual : MonoBehaviour {
		public static bool Show(GameObject go, bool show, GameObject prefab) {
			SelectedVisual selvis = go.GetComponentInChildren<SelectedVisual>();
			if (!selvis && show) {
				GameObject vis = Instantiate(prefab, go.transform);
				vis.SetActive(true);
				vis.AddComponent<SelectedVisual>();
				return true;
			}
			if (selvis && selvis.gameObject.activeSelf != show) {
				selvis.gameObject.SetActive(show);
				return true;
			}
			return false;
		}
	}

	void ShowSelectedVisual(GameObject go, bool show) {
		SelectedVisual.Show(go, show && prefab_selectionObject && showSelectionObject, prefab_selectionObject);
	}
	public void SetSelection(CharacterRoot character) {
		selection.ForEach(f => {
			f.ShowPath(false);
			ShowSelectedVisual(f.gameObject, false);
		});
		selection.Clear();
		if (character != null) {
			selection.Add(c2m.Follower(character.move));
		}
		ResetSelectionVisual();
	}
	void ResetSelectionVisual() {
		selection.ForEach(f => {
			f.ShowPath(true);
			ShowSelectedVisual(f.gameObject, true);
		});
	}

	private void Update() {
		if (selection.Count == 0 && c2m.characterToMove.Target != null) {
			SetSelection(c2m.characterToMove.Target);
		}
		if (Input.GetKey(c2m.key)) {
			c2m.RaycastClick(rh => {
				if (selection.Count == 0) { return; }
				for (int i = 0; i < selection.Count; ++i) {
					if (selection[i] == null) {
						selection.RemoveAt(i--);
						continue;
					}
					c2m.ClickFor(selection[i], rh);
				}
			});
		}
		if (c2m.prefab_waypoint != null && Input.GetKeyUp(c2m.key)) {
			//if (follower != null) { follower.ShowCurrentWaypoint(); }
			if (selection.Count > 0) {
				for (int i = 0; i < selection.Count; ++i) {
					selection[i].ShowCurrentWaypoint();
				}
			}
		}
	}
}
