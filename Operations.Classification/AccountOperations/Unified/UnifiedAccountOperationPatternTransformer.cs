using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FastMember;

using Newtonsoft.Json;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.GeoLoc;
using Operations.Classification.Properties;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationPatternTransformer
    {
        private readonly List<UnifiedAccountOperationPatternMapping> _patterns;

        private readonly TypeAccessor _unifiedOpAccessors;
        
        private readonly PlaceInfoResolver _placeInfoResolver;
        private readonly AccountToUnifiedOperationMapper _mapper;
        private readonly Dictionary<string, Type> _unifiedOpProperties;

        public UnifiedAccountOperationPatternTransformer()
        {
            _patterns = LoadPatterns();
            _mapper = new AccountToUnifiedOperationMapper();
            _placeInfoResolver = new PlaceInfoResolver();
            _unifiedOpAccessors = TypeAccessor.Create(typeof(UnifiedAccountOperation));
            _unifiedOpProperties = typeof(UnifiedAccountOperation).GetProperties().Where(p => p.CanRead && p.CanWrite).ToDictionary(p => p.Name, p=>p.PropertyType);
        }

        public UnifiedAccountOperation Apply(AccountOperationBase operation)
        {
            var unified = _mapper.Map(operation);
            return Apply(operation, unified);
        }

        public UnifiedAccountOperation Apply(AccountOperationBase source, UnifiedAccountOperation target)
        {
            var tuple =
                _patterns.Where(p => p.SourceKind == source.SourceKind)
                    .Select(p => new { Pattern = p, Match = p.CompiledExpression.Match(source) })
                    .SingleOrDefault(m => m.Match.Success);

            if (tuple == null)
            {
                return target;
            }

            var symbols = new Dictionary<string, object>();

            AddSourcePropertiesToSymbols(source, symbols);

            var pattern = tuple.Pattern;
            var groupNames = pattern.CompiledExpression.GetGroupNames();
            var match = tuple.Match;

            target.PatternName = pattern.Name;
            
            foreach (var groupName in groupNames)
            {
                var matchGroup = match.Groups[groupName];
                if (matchGroup.Success)
                {
                    var input = matchGroup.Value.Trim();
                    symbols[groupName] = input;
                    if (_unifiedOpProperties.ContainsKey(groupName))
                    {
                        _unifiedOpAccessors[target, groupName] = input;
                    }
                    else if (groupName.EndsWith("ThenCity"))
                    {
                        var placeInfo = _placeInfoResolver.ResolveKnowingPlaceInfoIsAtEndOfText(input, false);
                        var freeTextWithoutPlaceInfo = placeInfo.GetFreeTextWithoutPlaceInfo();

                        target.City = placeInfo.City;

                        if (groupName.Equals("ThirdPartyThenCity"))
                        {
                            target.ThirdParty = freeTextWithoutPlaceInfo;
                        }
                        else if (groupName.Equals("CommunicationThenCity"))
                        {
                            target.Communication = freeTextWithoutPlaceInfo;
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

                        target.City = placeInfo.City;
                        target.Address = placeInfo.Adress;

                        if (groupName.Equals("ThirdPartyThenAddress"))
                        {
                            target.ThirdParty = freeTextWithoutPlaceInfo;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            if (pattern.ExplicitMappings != null)
            {
                foreach (var property in pattern.ExplicitMappings.Properties())
                {
                    var memberType = _unifiedOpProperties[property.Name];
                    _unifiedOpAccessors[target, property.Name] = EvaluateValue(symbols, (string)property.Value, memberType);
                }
            }


            return target;
        }

        private object EvaluateValue(Dictionary<string, object> symbols, string template, Type targetType)
        {
            var bindingRegex = new Regex("{(?<binding>.*?)}", RegexOptions.Compiled);
            var completeBindingRegex = new Regex("${(?<binding>.*)}^", RegexOptions.Compiled);
            object result = null;
            if (targetType == typeof(string))
            {
                var evaluator = new MatchEvaluator(
                    match =>
                    {
                        var binding = match.Groups["binding"].Value;
                        var value = EvaluateBinding(symbols, binding);
                        return value?.ToString() ?? string.Empty;
                    });
                result = bindingRegex.Replace(template, evaluator);
            }
            else
            {
                var match = completeBindingRegex.Match(template);
                if (!match.Success)
                {
                    var binding = match.Groups["binding"].Value;
                    result = EvaluateBinding(symbols, binding);
                }
            }

            return result;
        }

        private object EvaluateBinding(Dictionary<string, object> symbols, string binding)
        {
            object result = null;
            var steps = binding.Split('|');
            foreach (var step in steps.Select(s=>s.Trim()))
            {
                if (step.StartsWith("format "))
                {
                    var formattable = result as IFormattable;
                    if (formattable != null)
                    {
                        result = formattable.ToString(step.Substring("format ".Length), CultureInfo.CurrentCulture);
                    }
                }
                else if(symbols.ContainsKey(step))
                {
                    result = symbols[step];
                }
            }

            return result;
        }
        
        private static void AddSourcePropertiesToSymbols(object source, Dictionary<string, object> symbols)
        {
            var sourceProps = source.GetType().GetProperties().Where(p => p.CanRead);
            var sourcePropAccessor = ObjectAccessor.Create(source);
            foreach (var sourceProp in sourceProps)
            {
                symbols[sourceProp.Name] = sourcePropAccessor[sourceProp.Name];
            }
        }

        private static List<UnifiedAccountOperationPatternMapping> LoadPatterns()
        {
            var fortisPatterns = JsonConvert.DeserializeObject<List<UnifiedAccountOperationPatternMapping>>(Resources.FortisUnifiedAccountPatternMappings);
            var sodexoPatterns = JsonConvert.DeserializeObject<List<UnifiedAccountOperationPatternMapping>>(Resources.SodexoUnifiedAccountPatternMappings);
            return fortisPatterns.Union(sodexoPatterns).ToList();
        }
    }
}