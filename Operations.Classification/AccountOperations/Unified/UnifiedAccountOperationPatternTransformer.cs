using System;
using System.Collections.Generic;
using System.Linq;

using FastMember;

using Newtonsoft.Json;

using Operations.Classification.GeoLoc;
using Operations.Classification.Properties;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationPatternTransformer
    {
        private readonly List<UnifiedAccountOperationPatternMapping> _patterns;

        private readonly TypeAccessor _operationDetailsAccessors;

        private readonly HashSet<string> _operationDetailsPropertyNames;

        private readonly PlaceInfoResolver _placeInfoResolver;

        public UnifiedAccountOperationPatternTransformer()
        {
            _patterns = LoadPatterns();
            _placeInfoResolver = new PlaceInfoResolver();
            _operationDetailsAccessors = TypeAccessor.Create(typeof(UnifiedAccountOperation));
            _operationDetailsPropertyNames = new HashSet<string>(typeof(UnifiedAccountOperation).GetProperties().Where(p => p.CanWrite).Select(p => p.Name));
        }

        public UnifiedAccountOperation Apply(UnifiedAccountOperation operation)
        {
            if (string.IsNullOrEmpty(operation.Note))
            {
                return operation;
            }

            var tuple =
                _patterns.Where(p => p.SourceKind == operation.SourceKind)
                    .Select(p => new { Pattern = p, Match = p.CompiledExpression.Match(operation.Note) })
                    .SingleOrDefault(m => m.Match.Success);

            if (tuple == null)
            {
                return operation;
            }

            var pattern = tuple.Pattern;
            var groupNames = pattern.CompiledExpression.GetGroupNames();
            var match = tuple.Match;
            
            operation.PatternName = pattern.Name;

            if (pattern.StaticValues != null)
            {
                foreach (var property in pattern.StaticValues.Properties())
                {
                    _operationDetailsAccessors[operation, property.Name] = (string)property.Value;
                }
            }

            foreach (var groupName in groupNames)
            {
                var matchGroup = match.Groups[groupName];
                if (matchGroup.Success)
                {
                    var input = matchGroup.Value.Trim();
                    if (_operationDetailsPropertyNames.Contains(groupName))
                    {
                        _operationDetailsAccessors[operation, groupName] = input;
                    }
                    else if (groupName.EndsWith("ThenCity"))
                    {
                        var placeInfo = _placeInfoResolver.ResolveKnowingPlaceInfoIsAtEndOfText(input, false);
                        var freeTextWithoutPlaceInfo = placeInfo.GetFreeTextWithoutPlaceInfo();

                        operation.City = placeInfo.City;

                        if (groupName.Equals("ThirdPartyThenCity"))
                        { 
                            operation.ThirdParty = freeTextWithoutPlaceInfo;
                        }
                        else if (groupName.Equals("CommunicationThenCity"))
                        {
                            operation.Communication = freeTextWithoutPlaceInfo;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else if (groupName.EndsWith("ThenAddress"))
                    {
                        var placeInfo = _placeInfoResolver.ResolveKnowingPlaceInfoIsAtEndOfText(input, true);
                        var freeTextWithoutPlaceInfo = placeInfo.GetFreeTextWithoutPlaceInfo();

                        operation.City = placeInfo.City;
                        operation.Address = placeInfo.Adress;
                        
                        if (groupName.Equals("ThirdPartyThenAddress"))
                        {
                            operation.ThirdParty = freeTextWithoutPlaceInfo;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
            
            return operation;
        }

        private static List<UnifiedAccountOperationPatternMapping> LoadPatterns()
        {
            var fortisPatterns = JsonConvert.DeserializeObject<List<UnifiedAccountOperationPatternMapping>>(Resources.FortisUnifiedAccountPatternMappings);
            var sodexoPatterns = JsonConvert.DeserializeObject<List<UnifiedAccountOperationPatternMapping>>(Resources.SodexoUnifiedAccountPatternMappings);
            return fortisPatterns.Union(sodexoPatterns).ToList();
        }
    }
}