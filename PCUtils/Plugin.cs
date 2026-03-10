using PCUtils;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaLocomotion;

[assembly: MelonInfo(typeof(Plugin), PCUtils.Constants.ModName, PCUtils.Constants.Version, PCUtils.Constants.ModAuthor)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
namespace PCUtils
{
    public class Plugin : MelonMod
    {
        private LayerMask coolLayers;
        private GorillaTriggerColliderHandIndicator clickyCollider;
        
        public bool isActive = false;
        
        public override void OnInitializeMelon()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawn);
        }

        public void OnPlayerSpawn()
        {
            clickyCollider = GorillaTagger.Instance.rightHandTriggerCollider
                .GetComponent<GorillaTriggerColliderHandIndicator>();

            coolLayers = 1 << 18;
        }

        public override void OnFixedUpdate()
        {

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                isActive = !isActive;
                VRRig.LocalRig.head.trackingRotationOffset = Vector3.zero;
            }

            string gameMode = NetworkSystem.Instance.InRoom ? GorillaLibrary.GameModes.Utilities.GameModeUtility.CurrentGamemode.ID : GorillaLibrary.GameModes.Constants.ModdedPrefix;
            
            if (!isActive || !gameMode.StartsWith(GorillaLibrary.GameModes.Constants.ModdedPrefix))
                return;
            
            Transform head = GorillaTagger.Instance.headCollider.transform;
            Transform body = GorillaTagger.Instance.bodyCollider.transform;

            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouse = Mouse.current.delta.ReadValue();
                head.Rotate(Vector3.up,    mouse.x  * 0.08f, Space.World);
                head.Rotate(Vector3.right, -mouse.y * 0.08f, Space.Self);
                
                VRRig.LocalRig.head.rigTarget.transform.rotation = head.rotation;

                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            Vector3 direction = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) direction += body.forward;
            if (Keyboard.current.sKey.isPressed) direction -= body.forward;
            if (Keyboard.current.aKey.isPressed) direction -= body.right;
            if (Keyboard.current.dKey.isPressed) direction += body.right;
            if (Keyboard.current.spaceKey.isPressed) direction += body.up;
            if (Keyboard.current.leftCtrlKey.isPressed) direction -= body.up;

            Rigidbody playerRB = GorillaTagger.Instance.rigidbody;
            
            playerRB.transform.position += direction.normalized * Time.fixedDeltaTime * (Keyboard.current.leftShiftKey.isPressed ? 30f : 10f);
            playerRB.linearVelocity = Vector3.zero;
            playerRB.AddForce(-Physics.gravity * playerRB.mass);
            
            Camera thirdPerson = GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0).GetComponent<Camera>();
            
            if (Mouse.current.leftButton.isPressed)
            {
                Camera camera = thirdPerson.gameObject.activeInHierarchy ? thirdPerson : GTPlayer.Instance.mainCamera;

                if (!Physics.Raycast(camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit,
                        25f, coolLayers))
                    return;

                clickyCollider.transform.position = hit.point;   
            }
            
            if (Keyboard.current.cKey.wasPressedThisFrame && isActive)
                thirdPerson.gameObject.SetActive(!thirdPerson.gameObject.activeInHierarchy);
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame && isActive)
                NetworkSystem.Instance.ReturnToSinglePlayer();
        }
    }
}