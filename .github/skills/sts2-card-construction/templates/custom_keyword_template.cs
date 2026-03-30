using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace snakebite.Cards;

public static class SnakebiteKeywords
{
    [CustomEnum("VENOM_CHAIN")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword VenomChain;
}
