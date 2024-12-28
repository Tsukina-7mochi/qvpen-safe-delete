using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace net.ts7m.qvpen_safe_clear.udon {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QvPenInteractButtonExtended : UdonSharpBehaviour {
        [SerializeField] private bool canUseEveryone = false;
        [SerializeField] private bool canUseInstanceOwner = false;
        [SerializeField] private bool canUseOwner = false;
        [SerializeField] private bool canUseMaster = false;

        [SerializeField] private bool isGlobalEvent = false;
        [SerializeField] private bool onlySendToOwner = false;
        [SerializeField] private UdonSharpBehaviour udonSharpBehaviour;
        [SerializeField] private UdonSharpBehaviour[] udonSharpBehaviours = { };
        [SerializeField] private string customEventName = "Unnamed";
        [SerializeField] private bool requireDoubleClick = false;
        [SerializeField] private float doubleClickDuration = 0.5f;

        private bool _doubleClickAcceptable = false;

        public override void Interact() {
            if (this.requireDoubleClick && !this._doubleClickAcceptable) {
                this._doubleClickAcceptable = true;
                this.SendCustomEventDelayedSeconds(
                    nameof(this._DoubleClickTimeout),
                    this.doubleClickDuration
                );
                return;
            }

            if (!this.canUseEveryone) {
                if (this.canUseInstanceOwner && !Networking.IsInstanceOwner) return;
                if (this.canUseOwner && !Networking.IsOwner(this.gameObject)) return;
                if (this.canUseMaster && !Networking.IsMaster) return;
            }

            if (this.udonSharpBehaviour) {
                if (this.isGlobalEvent) {
                    this.udonSharpBehaviour.SendCustomNetworkEvent(
                        this.onlySendToOwner ? NetworkEventTarget.Owner : NetworkEventTarget.All,
                        this.customEventName
                    );
                }
                else {
                    this.udonSharpBehaviour.SendCustomEvent(this.customEventName);
                }
            } else if (this.udonSharpBehaviours != null && this.udonSharpBehaviours.Length > 0) {
                if (this.isGlobalEvent) {
                    foreach (var behaviour in this.udonSharpBehaviours) {
                        behaviour.SendCustomNetworkEvent(
                            this.onlySendToOwner ? NetworkEventTarget.Owner : NetworkEventTarget.All,
                            this.customEventName
                        );
                    }
                }
                else {
                    foreach (var behaviour in this.udonSharpBehaviours) {
                        behaviour.SendCustomEvent(this.customEventName);
                    }
                }
            }
        }

        public void _DoubleClickTimeout() {
            this._doubleClickAcceptable = false;
        }
    }

}
