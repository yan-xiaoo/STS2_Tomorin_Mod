using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_Tomorin_Mod.CardPools;
using STS2_Tomorin_Mod.Cards.Base;

namespace STS2_Tomorin_Mod.Cards;

/// <summary>
/// 满载灵感
/// 1费 白卡 攻击 造成 6->9 点伤害，手中每有一张收集品，额外造成 3->5 点伤害
/// </summary>
[Pool(typeof(TomorinCardPool))]
public class FullInspiration() : BaseCardModel(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            var list = base.CanonicalVars.ToList();
            list.Add(new CalculationBaseVar(6));
            list.Add(new ExtraDamageVar(3));
            list.Add(new CalculatedDamageVar(ValueProp.Move).WithMultiplier(GetCollectionCount));
            return list;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    public static decimal GetCollectionCount(CardModel card, Creature? creature)
    {
        var hand = card.Owner.PlayerCombatState?.Hand;
        if (hand == null)
            return 0m;

        var collectionTypes = new HashSet<Type>(
            ModelDb.CardPool<CollectionsCardPool>().AllCards.Select(collection => collection.GetType()));

        return hand.Cards.Count(handCard => collectionTypes.Contains(handCard.GetType()));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
