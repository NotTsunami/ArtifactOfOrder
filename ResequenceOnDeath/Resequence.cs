using BepInEx;
using RoR2;
using UnityEngine;

namespace Tsunami
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.ResequenceOnDeath", "ResequenceOnDeath", "1.0.1")]
    public class ResequenceOnDeath : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterBody.OnDeathStart += (orig, self) =>
            {
                if (self.isPlayerControlled && (self.inventory.HasAtLeastXTotalItemsOfTier(ItemTier.Tier1, 2) || self.inventory.HasAtLeastXTotalItemsOfTier(ItemTier.Tier2, 2) || self.inventory.HasAtLeastXTotalItemsOfTier(ItemTier.Tier3, 2)))
                {
                    self.inventory.ShrineRestackInventory(new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = self,
                        baseToken = "SHRINE_RESTACK_USE_MESSAGE"
                    });
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                    {
                            origin = self.corePosition,
                            rotation = Quaternion.identity,
                            scale = 1f,
                            color = new Color(1f, 0.23f, 0.6337214f)
                    }, true);
                }
                orig(self);
            };
        }
    }
}
