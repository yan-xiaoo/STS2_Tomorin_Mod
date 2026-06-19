using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_Tomorin_Mod.CardPools;
using STS2_Tomorin_Mod.Cards.Base;
using STS2_Tomorin_Mod.Cards.Collections;

namespace STS2_Tomorin_Mod.Cards;

[Pool(typeof(TomorinCardPool))]
public class Dejected() : BaseCardModel(
    canonicalEnergyCost: 0,
    type: CardType.Skill,
    rarity: CardRarity.Common,
    targetType: TargetType.Self)
{
	public override bool GainsBlock => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 将一张满是划痕的笔记本加入手牌
        var brokenNote = base.CombatState!.CreateCard<BrokenNote>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(brokenNote, PileType.Hand, Owner);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var list = base.ExtraHoverTips.ToList();
            list.Add(HoverTipFactory.FromCard<BrokenNote>(false));
            return list;
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
    }
}
