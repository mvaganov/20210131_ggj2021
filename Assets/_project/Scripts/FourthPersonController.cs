using NonStandard.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourthPersonController : MonoBehaviour
{
    public CharacterCamera _camera;
    public CharacterMoveProxy characterMoveProxy;
    public ClickToMove clickToMove;
    public CharacterCamera.CameraView view;
    public CharacterMove previous;
    private CharacterMove self;
    public CharacterMove GetMover() { return self; }

	public void Awake() {
        self = GetComponent<CharacterMove>();
	}
	public void Toggle() {
        if(characterMoveProxy.Target == self) {
            characterMoveProxy.Target = previous;
            _camera.LerpTarget(previous.transform);
            _camera.LerpView("user");
        } else {
            previous = characterMoveProxy.Target;
            transform.position = (previous.head ? previous.head:previous.transform).position;
            characterMoveProxy.Target = self;
            _camera.LerpTo(view);
            clickToMove.SetSelection(previous);
        }
    }
}
