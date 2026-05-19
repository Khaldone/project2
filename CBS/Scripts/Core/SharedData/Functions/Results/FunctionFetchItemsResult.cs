using PlayFab.ClientModels;
using System.Collections.Generic;

namespace CBS.Models
{
    public class FunctionFetchItemsResult
    {
        public GetCatalogItemsResult ItemsResult;
        public Dictionary<string, string> Categories;
        public CBSRecipeContainer Recipes;
        public CBSItemUpgradesContainer Upgrades;
    }
}
