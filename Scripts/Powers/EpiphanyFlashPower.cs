using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Tomorin_Mod.Powers;

/// <summary>
/// 灵光乍现效果
/// 每当拥有者消耗一张牌时，获得等同于层数的心之壁
/// </summary>
public class EpiphanyFlashPower : BasePowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature != Owner)
            return;

        Flash();
        await PowerCmd.Apply<AtFieldPower>(Owner, Amount, Owner, null);
    }
}
