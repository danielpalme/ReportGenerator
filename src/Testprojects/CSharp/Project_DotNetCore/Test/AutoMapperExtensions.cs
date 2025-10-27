using System.Linq;
using AutoMapper;

namespace Test
{
    public static class AutoMapperExtensions
    {
        public static TResult MergeInto<TResult>(this IMapper mapper, object item)
        {
            return mapper.Map<TResult>(item);
        }

        public static TResult MergeInto<TResult>(this IMapper mapper, params object?[] objects)
        {
            var res = mapper.Map<TResult>(objects.First());
            return objects.Skip(1).Where(x => x is not null).Aggregate(res, (r, obj) => mapper.Map(obj, r));
        }
    }
}
