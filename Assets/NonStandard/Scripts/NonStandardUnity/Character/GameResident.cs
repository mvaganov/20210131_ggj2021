using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
    public class GameResident : MonoBehaviour {
        [Tooltip("if true, set bind point as soon as object Starts")]
        public bool bindOnStart = true;
        public Vector3 homePoint;
        public List<GameArea> gameAreaInhabited = new List<GameArea>();
        public void BindHomePoint(Vector3 position) { homePoint = position; bindOnStart = false; }
        public void BindHomePointHere() { homePoint = transform.position; bindOnStart = false; }
        public void ReturnToHome(bool clearVelocity = true) {
            Rigidbody rb = GetComponentInChildren<Rigidbody>();
            if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            transform.position = homePoint;
        }
        void Start() {
            if (bindOnStart) { BindHomePointHere(); }
        }
        private void OnTriggerEnter(Collider other) {
            GameArea area = other.GetComponent<GameArea>();
            if (area && gameAreaInhabited.IndexOf(area) < 0 && area.enabled) { gameAreaInhabited.Add(area); }
        }
        private void OnTriggerExit(Collider other) {
            GameArea area = other.GetComponent<GameArea>();
            if (area) {
                for (int i = gameAreaInhabited.Count - 1; i >= 0; --i) {
                    if (gameAreaInhabited[i] == null || gameAreaInhabited[i] == area || !gameAreaInhabited[i].enabled || !gameAreaInhabited[i].gameObject.activeInHierarchy) {
                        gameAreaInhabited.RemoveAt(i);
                    }
                }
                if (gameAreaInhabited.Count == 0) { ReturnToHome(); }
            }
        }
    }
}