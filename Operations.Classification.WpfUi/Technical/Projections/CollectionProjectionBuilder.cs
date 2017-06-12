using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace Operations.Classification.WpfUi.Technical.Projections
{
    public class CollectionProjectionBuilder<TSource>
    {
        private readonly IEnumerable<TSource> _source;

        public CollectionProjectionBuilder(IEnumerable<TSource> source)
        {
            _source = source;
        }

        public IEnumerable<TTarget> To<TTarget>(Action<TSource, TTarget> onMappedItem)
        {
            return _source.Select(s => Mapper.Map<TSource, TTarget>(s, opts => opts.AfterMap(onMappedItem)));
        }

        public IEnumerable<TTarget> To<TTarget>()
        {
            return To<TTarget>((sourceItem, targetItem) => { });
        }
    }
}