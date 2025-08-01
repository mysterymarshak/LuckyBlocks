using System;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Configuration;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Utils;
using OneOf.Types;
using Serilog;

namespace LuckyBlocks.Loot;

internal interface IRandomItemProvider
{
    Item GetRandomItem(LuckyBlockBrokenArgs args);
}

internal class RandomItemProvider : IRandomItemProvider
{
    private readonly IIdentityService _identityService;
    private readonly IAttributesChecker _attributesChecker;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public RandomItemProvider(IIdentityService identityService, IAttributesChecker attributesChecker,
        IConfigurationService configurationService, ILogger logger)
    {
        _identityService = identityService;
        _attributesChecker = attributesChecker;
        _configurationService = configurationService;
        _logger = logger;
    }

    public Item GetRandomItem(LuckyBlockBrokenArgs args)
    {
        var player = args.IsPlayer ? _identityService.GetPlayerById(args.PlayerId) : new Unknown();

        var configuration = _configurationService.GetConfiguration<IRandomItemProviderConfiguration>();
        var excludedItems = configuration.ExcludedItems;
        var luckyBlockItemValues = Enum
            .GetValues(typeof(Item))
            .Cast<Item>()
            .Where(x => _attributesChecker.Check(x, player))
            .Except(excludedItems.Select(ItemExtensions.Parse))
            .ToList();

        _logger.Debug($"{string.Join(", ", luckyBlockItemValues)}");

        var alwaysItem = luckyBlockItemValues.SingleOrDefault(EnumUtils.AttributeExist<AlwaysAttribute, Item>);
        if (alwaysItem != Item.None)
            return alwaysItem;

        if (luckyBlockItemValues.Count == 0)
        {
            return Item.None;
        }

        return luckyBlockItemValues
            .GetWeightedRandomElement(GetItemWeight);
    }

    private static double GetItemWeight(Item item) =>
        EnumUtils.AttributeExist<WeightAttribute, Item>(item, out var attribute)
            ? attribute.Weight
            : 1;
}