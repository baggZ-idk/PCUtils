using PCUtils;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaLocomotion;
using BaGUI;
using GorillaNetworking;
using Unity.Cinemachine;
using BepInEx;

[assembly: MelonInfo(typeof(PCUtils.MelonLoader), PCUtils.Constants.ModName, PCUtils.Constants.Version, PCUtils.Constants.ModAuthor)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace PCUtils
{
    
    [BepInPlugin(Constants.GUID, Constants.ModName, Constants.Version)]
    public class BelInEx : BaseUnityPlugin
    {
        private Shared shared = new Shared();   

        void Start() => shared.Init();
        void Update() => shared.Update();
        void FixedUpdate() => shared.FixedUpdate();
        void OnGUI() => shared.OnGUI();
    }

    public class MelonLoader : MelonMod
    {
        private Shared shared = new Shared();

        public override void OnInitializeMelon() => shared.Init();
        public override void OnUpdate() => shared.Update();
        public override void OnFixedUpdate() =>  shared.FixedUpdate();
        public override void OnGUI() => shared.OnGUI();
    }
    
    public class Shared
    {
        private LayerMask coolLayers;
        private GorillaTriggerColliderHandIndicator clickyCollider;

        public bool isActive = false;
        public bool helpMenu = false;
        public bool noclipEnabled = false;

        private Panel utilsPanel;
        private Slider sensitivitySlider;
        private Slider speedSlider;
        private TextInput roomCode;
        private TextInput userName;

        public void Init()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawn);
        }

        public void OnPlayerSpawn()
        {
            clickyCollider = GorillaTagger.Instance.rightHandTriggerCollider
                .GetComponent<GorillaTriggerColliderHandIndicator>();

            coolLayers = 1 << 18;

            utilsPanel = new Panel("PCUtils", new Vector2(20, 20));

            var helpSection = utilsPanel.CreateSection("Help");
            helpSection.AddLabel("Tab - Toggles mod");
            helpSection.AddLabel("Esc - Leaves the room");
            helpSection.AddLabel("H - Toggles menu");
            helpSection.AddLabel("Alt - Toggles noclip");
            helpSection.AddLabel("Right Click - Look around");
            helpSection.AddLabel("Left Click - Click buttons (most mods are incompatible with this feature)");
            helpSection.AddLabel("W - Move forwards");
            helpSection.AddLabel("A - Move left");
            helpSection.AddLabel("S - Move backwards");
            helpSection.AddLabel("D - Move right");
            helpSection.AddLabel("Shift - Sprint");

            
            var settingsSection = utilsPanel.CreateSection("Settings");
            sensitivitySlider = settingsSection.AddSlider("Sensitivity", 0f, 2f, 1f);
            sensitivitySlider.OnValueChanged += val => { };
            
            Slider fovSlider = settingsSection.AddSlider("FOV", 30f, 120f, 60f);
            fovSlider.OnValueChanged += val =>
            {
                GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0).transform.GetChild(0).GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = val;
                GorillaTagger.Instance.mainCamera.transform.GetComponent<Camera>().fieldOfView = val;   
            };
            
            speedSlider = settingsSection.AddSlider("Speed", 1f,  20f, 10f);

            Checkbox firstPersonToggle = settingsSection.AddCheckbox("First Person", false);
            firstPersonToggle.OnValueChanged += val => { GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0).gameObject.SetActive(!val); };
            
            
            var gameSection = utilsPanel.CreateSection("Game");
            
            gameSection.AddButton("Disconnect", () => { NetworkSystem.Instance.ReturnToSinglePlayer(); });

            roomCode = gameSection.AddTextInput("Room Code", "");
            Button joinRoom = gameSection.AddButton("Join Room", 
                () => { PhotonNetworkController.Instance.AttemptToAutoJoinSpecificRoom(roomCode.Value, JoinType.Solo); });

        }

        public void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                isActive = !isActive;

            string gameMode = NetworkSystem.Instance.InRoom
                ? GorillaLibrary.GameModes.Utilities.GameModeUtility.CurrentGamemode.ID
                : "MODDED_";

            if (!isActive || !gameMode.StartsWith("MODDED_"))
                return;

            if (Keyboard.current.hKey.wasPressedThisFrame && isActive)
                helpMenu = !helpMenu;

            if (isActive && Keyboard.current.altKey.wasPressedThisFrame)
            {
                noclipEnabled = !noclipEnabled;

                MeshCollider[] array = Resources.FindObjectsOfTypeAll<MeshCollider>();
                for (int i = 0; i < array.Length; i++)
                    array[i].enabled = !noclipEnabled;
            }
        }

        public void FixedUpdate()
        {
            string gameMode = NetworkSystem.Instance.InRoom
                ? GorillaLibrary.GameModes.Utilities.GameModeUtility.CurrentGamemode.ID
                : "MODDED_";

            if (!isActive || !gameMode.StartsWith("MODDED_"))
                return;

            Transform head = GorillaTagger.Instance.headCollider.transform;

            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouse = Mouse.current.delta.ReadValue();
                head.Rotate(Vector3.up, mouse.x * 0.08f * sensitivitySlider.Value, Space.World);
                head.Rotate(Vector3.right, -mouse.y * 0.08f * sensitivitySlider.Value, Space.Self);

                VRRig.LocalRig.head.rigTarget.transform.rotation = head.rotation;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
                Cursor.lockState = CursorLockMode.None;

            Vector3 direction = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) direction += head.forward;
            if (Keyboard.current.sKey.isPressed) direction -= head.forward;
            if (Keyboard.current.aKey.isPressed) direction -= head.right;
            if (Keyboard.current.dKey.isPressed) direction += head.right;
            if (Keyboard.current.spaceKey.isPressed) direction += head.up;
            if (Keyboard.current.leftCtrlKey.isPressed) direction -= head.up;

            Rigidbody playerRB = GorillaTagger.Instance.rigidbody;
            playerRB.transform.position += direction.normalized * Time.fixedDeltaTime * (Keyboard.current.leftShiftKey.isPressed ? 3f : 1f) * speedSlider.Value;
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

        public void OnGUI()
        {
            if (!helpMenu || !isActive)
                return;

            utilsPanel.Draw();
        }
    }
}