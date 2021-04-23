using BepInEx;
using R2API.Utils;
using R2API;
using RoR2;
using System.Reflection;
using UnityEngine;


namespace ArtifactOfOrder
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.ArtifactOfOrder", "ArtifactOfOrder", "1.0.2")]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]

    public class ArtifactOfOrder : BaseUnityPlugin
    {

        // Artifact definition
        public ArtifactDef Order = ScriptableObject.CreateInstance<ArtifactDef>();
        public void Awake()
        {
            Order.nameToken = "Artifact of Order";
            Order.descriptionToken = "Applies the sequencing effect of a Shrine of Order to a player upon death.";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArtifactOfOrder.artifactoforder"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                Order.smallIconSelectedSprite = bundle.LoadAsset<Sprite>("Assets/Import/Icons/artifact_selected.png");
                Order.smallIconDeselectedSprite = bundle.LoadAsset<Sprite>("Assets/Import/Icons/artifact_unselected.png");


            }
            R2API.ArtifactAPI.Add(Order);
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
            return master.inventory.GetItemCount(RoR2.RoR2Content.Items.ExtraLife);
        }
    }
}
