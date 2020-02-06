using BepInEx;
using RoR2;
using UnityEngine;

namespace Tsunami
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.SequenceOnDeath", "SequenceOnDeath", "1.0.2")]
    public class SequenceOnDeath : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterBody.OnDeathStart += (orig, self) =>
            {
                // Check to see if we are a player, otherwise this is applied to monsters as well
                if (self.isPlayerControlled)
                {
                    // Check if we're in multiplayer OR have an unused Dio's Best Friend
                    // This avoids sequencing on a single player run without a Dio's Best Friend
                    if (PlayerCharacterMasterController.instances.Count > 1 || (self.inventory.GetItemCount(ItemIndex.ExtraLife) > 0))
                    {
                        // Sequence 
                        self.inventory.ShrineRestackInventory(new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));

                        // Mimic the original Shrine of Order chat broadcast and effect
                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            subjectAsCharacterBody = self,
                            baseToken = "SHRINE_RESTACK_USE_MESSAGE"
                        });
                        EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"),
                            new EffectData
                            {
                                origin = self.footPosition,
                                rotation = Quaternion.identity,
                                scale = 1f,
                                color = new Color(1f, 0.23f, 0.6337214f)
                            }, true);
                    }
                }
                orig(self);
            };
        }
    }
}
