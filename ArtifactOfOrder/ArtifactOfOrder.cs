using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace ArtifactOfOrder
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.ArtifactOfOrder", "ArtifactOfOrder", "1.0.1")]
    public class ArtifactOfOrder : BaseUnityPlugin
    {
        public void Awake()
        {
            Order.nameToken = "Artifact of Order";
            Order.descriptionToken = "Applies the sequencing effect of a Shrine of Order to a player upon death.";
            Order.smallIconDeselectedSprite = CreateSprite(Properties.Resources.artifact_unselected);
            Order.smallIconSelectedSprite = CreateSprite(Properties.Resources.artifact_selected);

            ArtifactCatalog.getAdditionalEntries += (list) => { list.Add(Order); };

            On.RoR2.CharacterMaster.OnBodyDeath += (orig, self, body) =>
            {
                // The original OnBodyDeath function removes a Dio's Best Friend from the inventory if one is
                // available, so the easiest way to track usage is to compare the count before and after the function
                int dioCount = GetDioCount(self);

                orig(self, body);

                // Bail if the artifact is not enabled
                if (!RunArtifactManager.instance.IsArtifactEnabled(Order.artifactIndex)) return;

                // Bail if body belongs to a monster
                if (!body.isPlayerControlled) return;

                // Bail if we're single player without a Dio's Best Friend
                if (!IsMultiplayer() && (dioCount == GetDioCount(self))) return;

                // Sequence the inventory
                self.inventory.ShrineRestackInventory(new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));

                // Mimic the original Shrine of Order chat broadcast and effect
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = body,
                    baseToken = "SHRINE_RESTACK_USE_MESSAGE"
                });
                EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), 
                    new EffectData
                    {
                        origin = body.footPosition,
                        rotation = Quaternion.identity,
                        scale = 1f,
                        color = new Color(1f, 0.23f, 0.6337214f)
                    }, true);
            };
        }

        // Artifact definition
        public ArtifactDef Order = ScriptableObject.CreateInstance<ArtifactDef>();

        // Helper function to load image into a Texture2D, which is then used to generate the sprite
        // Based on code from: https://github.com/risk-of-thunder/R2Wiki/wiki/Embedding-and-loading-resources-(The-sane-way)
        public static Sprite CreateSprite(byte[] resourceBytes)
        {
            // Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));
	
            // Create a temporary texture, then load the texture onto it.
            var tex = new Texture2D(56, 56, TextureFormat.RGBA32, false);
            tex.LoadImage(resourceBytes, false);
	
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Return true if more then 1 player in-game
        /// </summary>
        private static bool IsMultiplayer()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
        }

        /// <summary>
        /// Returns amount of unused Dio's Best Friends owned by player
        /// </summary>
        private static int GetDioCount(CharacterMaster master)
        {
            return master.inventory.GetItemCount(ItemIndex.ExtraLife);
        }
    }
}
