using System;
using AutoMapper;

namespace Operations.Classification.WpfUi.Technical.Projections
{
    public class ItemProjectionBuilder<TSource>
    {
        private readonly TSource _source;

        public ItemProjectionBuilder(TSource source)
        {
            _source = source;
        }

        public TTarget To<TTarget>(Action<TSource, TTarget> onMappedItem)
        {
            return Mapper.Map<TSource, TTarget>(_source, opts => opts.AfterMap(onMappedItem));
        }

        public TTarget To<TTarget>()
        {
            return To<TTarget>((sourceItem, targetItem) => { });
        }
    }
}