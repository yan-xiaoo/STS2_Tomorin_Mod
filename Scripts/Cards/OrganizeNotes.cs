using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_Tomorin_Mod.CardPools;
using STS2_Tomorin_Mod.Cards.Base;
using STS2_Tomorin_Mod.Powers;

namespace STS2_Tomorin_Mod.Cards;

/// <summary>
/// 整理笔记
/// 非凡技能 1费 消耗手中所有收集品。每消耗1张，获得5->7点格挡和1->2层心之壁
/// </summary>
[Pool(typeof(TomorinCardPool))]
public class OrganizeNotes() : BaseCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new PowerVar<AtFieldPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var list = base.ExtraHoverTips.ToList();
            list.Add(HoverTipFactory.FromPower<AtFieldPower>());
            return list;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null)
            return;

        var collectionTypes = new HashSet<Type>(
            ModelDb.CardPool<CollectionsCardPool>().AllCards.Select(card => card.GetType()));

        var cardsToExhaust = hand.Cards
            .Where(card => card != this && collectionTypes.Contains(card.GetType()))
            .ToList();

        foreach (var card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        var exhaustedCount = cardsToExhaust.Count;
        if (exhaustedCount <= 0)
            return;

        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block.BaseValue * exhaustedCount,
            DynamicVars.Block.Props,
            cardPlay);

        await PowerCmd.Apply<AtFieldPower>(
            Owner.Creature,
            DynamicVars[AtFieldPower.DefaultName].BaseValue * exhaustedCount,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[AtFieldPower.DefaultName].UpgradeValueBy(1m);
    }
}
