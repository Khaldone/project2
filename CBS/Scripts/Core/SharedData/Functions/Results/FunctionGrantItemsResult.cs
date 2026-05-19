using PlayFab.ClientModels;
using System.Collections.Generic;

namespace CBS.Models
{
    public class FunctionGrantItemsResult
    {
        public string TargetID;
        public List<ItemInstance> GrantedInstances;
        public Dictionary<string, uint> GrantedCurrencies;
    }
}
