using GTMLModTemplate;
using MelonLoader;
using UnityEngine;
using GorillaLibrary.GameModes.Attributes;

// !!!
// NOTE: When you build the project, your IDE will automatically try to move the compiled DLL to the Mods folder, with the name of GTMLModTemplate.dll/GTMLModTemplate.pdb. If you change the name of the project, make sure to change the name of the DLL in the .csproj file.
// !!!

// Change the MelonInfo to correspond to your mod info. The first parameter is the class that inherits from MelonMod, so if you change the class name below, make sure to change it here as well.
// Make sure to also change the info in Constants.cs if you are going to use that. Constants dont seem to work here, so it isn't used in the MelonInfo attribute, but you can still use it in your mod code.
[assembly: MelonInfo(typeof(Plugin), GTMLModTemplate.Constants.ModName, GTMLModTemplate.Constants.Version, GTMLModTemplate.Constants.ModAuthor)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
namespace GTMLModTemplate
{
    public class Plugin : MelonMod
    {
        public static bool isModded = false; //Keeps track of it the player is in a modded room.
        
        // InitializeMelon runs as soon as the mod loads. If you instead want to run code when the player is spawned, put your code in OnPlayerSpawn.
        public override void OnInitializeMelon()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawn);
            GorillaLibrary.Events.Room.OnRoomJoined.Subscribe(OnRoomJoined);
            GorillaLibrary.Events.Room.OnRoomLeft.Subscribe(OnRoomLeft);
        }

        public void OnPlayerSpawn()
        {
            //Called when the player spawns.
        }

        public override void OnUpdate()
        {
            //Called every frame.
        }
        
        public override void OnFixedUpdate()
        {
            //Called repetitively at a set interval.
        }

        public override void OnLateUpdate()
        {
            //Called after update.
        }

        public void OnRoomJoined()
        {
            //Called when the player joins a room.
        }
        
        public void OnRoomLeft()
        {
            //Called when the player leaves a room.
        }

        [ModdedGamemodeJoin]
        public void ModdedGamemodeJoin()
        {
            //Called when the player joins a modded room.
            isModded = true;
        }
        
        [ModdedGamemodeLeave]
        public void ModdedGamemodeLeave()
        {
            //Called when the player leaves a modded room.
            isModded = false;
        }
    }
}