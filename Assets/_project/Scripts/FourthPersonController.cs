using NonStandard.Character;
using NonStandard.GameUi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourthPersonController : MonoBehaviour
{
    public CharacterCamera _camera;
    public CharacterProxy characterMoveProxy;
    public ClickToMove clickToMove;
    public CharacterCamera.CameraView view;
    public CharacterRoot previous;
    private CharacterRoot self;
    public Interact3dUi i3dui;
    public CharacterRoot GetMover() { return self; }
    public GameObject visibleObjects;
    Bounds visibleBounds;

	public void Awake() {
        self = GetComponent<CharacterRoot>();
	}
    //public bool useSpecialLogic = false;
    public void CenterOnVisibleBounds(float maxDistance) {
        if (CenterOnVisibleBounds(maxDistance, out Vector3 p, out float d)) {
            transform.position = p;
            _camera.targetDistance = d;
        }
    }
    public static Bounds GetBoundsWithChildren(GameObject gameObject) {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        bool centered = false;
        Bounds bounds = new Bounds();
        for (int i = 0; i < renderers.Length; i++) {
            if (renderers[i].enabled) {
                //Debug.Log(renderers[i].name);
                if (!centered) {
                    bounds.center = renderers[i].transform.position;
                    bounds.size = Vector3.zero;
                    centered = true;
                }
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        return bounds;
    }

    public bool CenterOnVisibleBounds(float maxDistance, out Vector3 finalPosition, out float finalDistance) {
        visibleBounds = GetBoundsWithChildren(visibleObjects);
        float maxExtent = visibleBounds.extents.magnitude;
        Camera camera = _camera.GetComponent<Camera>();
        Vector3 dir = camera.transform.forward, c3d = visibleBounds.center;
        Plane plane = new Plane(transform.up, transform.position);
        float dist;
        Ray r;
        if (dir == Vector3.down || dir == Vector3.up) {
            maxExtent = Mathf.Max(visibleBounds.extents.x, visibleBounds.extents.z);
            //Debug.Log(maxExtent);
		} else {
   //         //Debug.Log(dir);
			//if (useSpecialLogic) {
   //             Vector3 ext3d = visibleBounds.extents;
   //             Vector3[] corner = new Vector3[8];
   //             Vector3 Scale(Vector3 a, Vector3 b) { return new Vector3(a.x*b.x, a.y * b.y, a.z * b.z); }
   //             corner[0] = c3d + Scale(ext3d, new Vector3( 1, 1, 1));
   //             corner[1] = c3d + Scale(ext3d, new Vector3( 1, 1,-1));
   //             corner[2] = c3d + Scale(ext3d, new Vector3( 1,-1, 1));
   //             corner[3] = c3d + Scale(ext3d, new Vector3( 1,-1,-1));
   //             corner[4] = c3d + Scale(ext3d, new Vector3(-1, 1, 1));
   //             corner[5] = c3d + Scale(ext3d, new Vector3(-1, 1,-1));
   //             corner[6] = c3d + Scale(ext3d, new Vector3(-1,-1, 1));
   //             corner[7] = c3d + Scale(ext3d, new Vector3(-1,-1,-1));
   //             Vector2 min = camera.WorldToScreenPoint(corner[0]), max = min;
   //             for(int i = 1; i < corner.Length; ++i) {
   //                 Vector2 loc = camera.WorldToScreenPoint(corner[i]);
   //                 min.x = Mathf.Min(min.x, loc.x);
   //                 min.y = Mathf.Min(min.y, loc.y);
   //                 max.x = Mathf.Max(max.x, loc.x);
   //                 max.y = Mathf.Max(max.y, loc.y);
   //             }
   //             Vector2 ext2d = (max - min) / 2;
   //             Vector2 centerScreen = (min + max) / 2;
   //             Plane hypothetical = new Plane(-dir, c3d);
   //             Vector2 c2d = camera.WorldToScreenPoint(c3d);
   //             r = camera.ScreenPointToRay(c2d + new Vector2(ext2d.x, 0));
   //             hypothetical.Raycast(r, out dist);
   //             Vector3 xhatend = r.GetPoint(dist);
   //             float hExtent = Vector3.Distance(xhatend, c3d);
   //             r = camera.ScreenPointToRay(c2d + new Vector2(0, ext2d.y));
   //             hypothetical.Raycast(r, out dist);
   //             Vector3 yhatend = r.GetPoint(dist);
   //             float vExtent = Vector3.Distance(yhatend, c3d);
   //             maxExtent = Mathf.Max(vExtent, hExtent);
   //             r = camera.ScreenPointToRay(c2d);
   //             plane.Raycast(r, out dist);
   //             c3d = r.GetPoint(dist);
   //             Debug.Log(c3d + " vs " + visibleBounds.center);
   //         }
        }
        float minDistance = (maxExtent) / Mathf.Sin(Mathf.Deg2Rad * camera.fieldOfView / 2f);
        Vector3 p = c3d - dir * minDistance;
        r = new Ray(p, dir);
        plane.Raycast(r, out dist);
        if (dist <= maxDistance || maxDistance <= 0) {
            //transform.position = r.GetPoint(dist);
            //_camera.targetDistance = dist;
            finalPosition = r.GetPoint(dist);
            finalDistance = dist;
            return true;
        }
        finalPosition = transform.position;
        finalDistance = _camera.targetDistance;
        return false;
    }

    public void Toggle() {
        if(characterMoveProxy.Target == self) {
            characterMoveProxy.Target = previous;
            _camera.LerpTarget(previous.transform);
            _camera.LerpView("user");
        } else {
            previous = characterMoveProxy.Target;
            transform.position = (previous.move.head ? previous.move.head:previous.transform).position;
            characterMoveProxy.Target = self;
            _camera.LerpTo(view);
            clickToMove.SetFollower(previous.move);
            i3dui.triggerArea.transform.SetParent(previous.transform);
            i3dui.triggerArea.transform.localPosition = Vector3.zero;
        }
    }
}
