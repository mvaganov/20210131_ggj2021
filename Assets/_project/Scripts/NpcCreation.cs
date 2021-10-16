using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.GameUi;
using NonStandard.GameUi.Dialog;
using System.Collections.Generic;
using UnityEngine;

public class NpcCreation : MonoBehaviour
{
	public Game game;
	public TextAsset npcNamesText;
	public CharacterRoot prefab_npcPlayer;
	public List<CharacterRoot> npcs = new List<CharacterRoot>();
	private Dictionary<string, string> npcNames;
	public List<CharacterRoot> startingCharacters = new List<CharacterRoot>();
	public void Init() {
		Tokenizer tokenizer = new Tokenizer();
		CodeConvert.TryParse(npcNamesText.text, out npcNames, null, tokenizer);
	}
	public void GenerateMore(int len) {
		for (int i = npcs.Count; i < len; ++i) {
			Material mat = game.idolCreator.idolMaterials[i];
			GameObject npc = Instantiate(prefab_npcPlayer.gameObject);
			ParticleSystem ps = npc.GetComponentInChildren<ParticleSystem>();
			if (ps != null) {
				ps.name = mat.name;
				ParticleSystem.MainModule m = ps.main;
				m.startColor = mat.color;
			}
			npc.name = prefab_npcPlayer.name + i;
			ScriptedDictionary dict = npc.GetComponentInChildren<ScriptedDictionary>();
			if (dict != null) {
				dict["mycolor"] = mat.name;
			}
			string foundName;
			if (npcNames.TryGetValue(mat.name, out foundName)) {
				npc.name = foundName;
			}
			Interact3dItem i3d = npc.GetComponentInChildren<Interact3dItem>();
			i3d.Text = npc.name;
			i3d.internalState.size = 1.75f;
			i3d.internalState.fontCoefficient = .7f;
			i3d.OnInteract = () => {
				DialogManager.Instance.dialogWithWho = npc;
				DialogManager.Instance.Show();
				Tokenizer tok = new Tokenizer();
				DialogManager.Instance.StartDialog(npc, tok, "dialog" + mat.name);
				tok.ShowErrorTo(DialogManager.ActiveDialog.ShowError);
				ps.Stop();
			};
			npcs.Add(npc.GetComponent<CharacterRoot>());
		}

	}

	public void PopulateWithCharacters(List<object> chars) {
		chars.AddRange(startingCharacters);
		chars.AddRange(npcs);
	}
}
