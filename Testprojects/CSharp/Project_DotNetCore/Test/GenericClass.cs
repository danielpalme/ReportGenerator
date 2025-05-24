using System;

namespace Test
{
    public abstract class AbstractGenericClass<TModel, TState> : ISomeClassInterface<TModel, TState>
        where TModel : SomeModel
        where TState : IState
    {
        public abstract bool Process(ISomeObjectInterface<TModel, TState> someObject);

        public virtual bool PostProcess(ISomeObjectInterface<TModel, TState> someObject)
        {
            return true;
        }
    }

    public class GenericClass<TModel, TState> : AbstractGenericClass<TModel, TState>
        where TModel : SomeModel
        where TState : IState
    {
        public override bool Process(ISomeObjectInterface<TModel, TState> someObject)
        {
            Console.WriteLine("Process");
            return true;
        }

        public override bool PostProcess(ISomeObjectInterface<TModel, TState> someObject)
        {
            Console.WriteLine("PostProcess");
            return true;
        }
    }

    public class SomeModel
    {
    }

    public interface IState
    {
    }

    public interface ISomeClassInterface<T1, T2>
    {
    }

    public interface ISomeObjectInterface<T1, T2>
    {
    }
}