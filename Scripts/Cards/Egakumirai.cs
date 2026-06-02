using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_Tomorin_Mod.CardPools;
using STS2_Tomorin_Mod.Cards.Base;

namespace STS2_Tomorin_Mod.Cards;

/// <summary>
/// 描绘未来
/// 白卡 0费 技能 常见 从手牌中选择至多两张状态牌并消耗
/// 升级效果：增加保留词条
/// </summary>
[Pool(typeof(TomorinCardPool))]
public class Egakumirai : BaseCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool IsInspiration => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public Egakumirai() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var list = base.CanonicalKeywords.ToList();
            if (IsUpgraded)
                list.Add(CardKeyword.Retain);
            return list;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查手牌中是否有状态牌，没有则直接返回
        var hand = base.Owner.PlayerCombatState?.Hand;
        if (hand == null || !hand.Cards.Any(c => c.Type == CardType.Status))
            return;

        // 从手牌中选择至多 2 张状态牌（min=0，允许一张都不选）
        var selected = await CardSelectCmd.FromHand(
            choiceContext,
            base.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 2),
            c => c.Type == CardType.Status,
            this);

        foreach (var card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}