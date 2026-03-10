using PCUtils;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaLocomotion;
using BaGUI;

[assembly: MelonInfo(typeof(Plugin), PCUtils.Constants.ModName, PCUtils.Constants.Version, PCUtils.Constants.ModAuthor)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
namespace PCUtils
{
    public class Plugin : MelonMod
    {
        private LayerMask coolLayers;
        private GorillaTriggerColliderHandIndicator clickyCollider;

        public bool isActive = false;
        public bool helpMenu = false;
        
        private Panel utilsPanel;
        private PanelItem sensitivitySlider;
        private PanelItem firstPersonToggle;

        public override void OnInitializeMelon()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawn);
        }

        public void OnPlayerSpawn()
        {
            clickyCollider = GorillaTagger.Instance.rightHandTriggerCollider
                .GetComponent<GorillaTriggerColliderHandIndicator>();

            coolLayers = 1 << 18;
            
            sensitivitySlider = new PanelItem("Sensitivity", 0f, 2f, 1f);
            firstPersonToggle = new PanelItem("First Person", PanelItemType.Checkbox);
        }

        public override void OnUpdate()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                isActive = !isActive;
            
            string gameMode = NetworkSystem.Instance.InRoom
                ? GorillaLibrary.GameModes.Utilities.GameModeUtility.CurrentGamemode.ID
                : GorillaLibrary.GameModes.Constants.ModdedPrefix;

            if (!isActive || !gameMode.StartsWith(GorillaLibrary.GameModes.Constants.ModdedPrefix))
                return;
            
            if (Keyboard.current.hKey.wasPressedThisFrame && isActive)
                helpMenu = !helpMenu;
            
            GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0).GetComponent<Camera>().gameObject.SetActive(!firstPersonToggle.BoolValue);
        }

        public override void OnFixedUpdate()
        {
            string gameMode = NetworkSystem.Instance.InRoom
                ? GorillaLibrary.GameModes.Utilities.GameModeUtility.CurrentGamemode.ID
                : GorillaLibrary.GameModes.Constants.ModdedPrefix;

            if (!isActive || !gameMode.StartsWith(GorillaLibrary.GameModes.Constants.ModdedPrefix))
                return;

            Transform head = GorillaTagger.Instance.headCollider.transform;

            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouse = Mouse.current.delta.ReadValue();
                head.Rotate(Vector3.up, mouse.x * 0.08f  * sensitivitySlider.FloatValue, Space.World);
                head.Rotate(Vector3.right, -mouse.y * 0.08f  * sensitivitySlider.FloatValue, Space.Self);

                VRRig.LocalRig.head.rigTarget.transform.rotation = head.rotation;

                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            Vector3 direction = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) direction += head.forward;
            if (Keyboard.current.sKey.isPressed) direction -= head.forward;
            if (Keyboard.current.aKey.isPressed) direction -= head.right;
            if (Keyboard.current.dKey.isPressed) direction += head.right;
            if (Keyboard.current.spaceKey.isPressed) direction += head.up;
            if (Keyboard.current.leftCtrlKey.isPressed) direction -= head.up;

            Rigidbody playerRB = GorillaTagger.Instance.rigidbody;

            playerRB.transform.position += direction.normalized * Time.fixedDeltaTime *
                                           (Keyboard.current.leftShiftKey.isPressed ? 30f : 10f);
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

            if (Keyboard.current.escapeKey.wasPressedThisFrame && isActive)
                NetworkSystem.Instance.ReturnToSinglePlayer();
        }

        public override void OnGUI()
        {
            if (!helpMenu || !isActive)
                return;

            if (utilsPanel == null)
            {
                utilsPanel = new Panel("PCUtils", new Vector2(20, 20));
                
                var helpSection = new PanelItem("Help", PanelItemType.Section);

                helpSection.SubItems.Add(new PanelItem("Tab - Toggles mod", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("Esc - Leaves the room", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("H - Toggles menu", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("Right Click - Look around", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("Left Click - Click buttons (most mods are incompatible with this feature)", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("W - Move forwards", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("A - Move left", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("S - Move backwards", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("D - Move right", PanelItemType.Text));
                helpSection.SubItems.Add(new PanelItem("Shift - Sprint", PanelItemType.Text));
                
                utilsPanel.Items.Add(helpSection);
                
                
                var settingsSection = new PanelItem("Settings", PanelItemType.Section);
                
                settingsSection.SubItems.Add(sensitivitySlider);
                settingsSection.SubItems.Add(firstPersonToggle);
                
                utilsPanel.Items.Add(settingsSection);
            }

            utilsPanel.Draw();
        }
    }
}