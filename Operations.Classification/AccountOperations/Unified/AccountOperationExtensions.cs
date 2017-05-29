using System;
using System.Collections.Generic;
using System.Linq;

namespace Operations.Classification.AccountOperations.Unified
{
    public static class UnifiedAccountOperationExtensions
    {
        public static IEnumerable<UnifiedAccountOperation> SortByOperationIdDesc(this IEnumerable<UnifiedAccountOperation> source)
        {
            return source.OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase);
        }
    }
}