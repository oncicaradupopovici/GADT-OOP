using System;
using System.Threading.Tasks;

namespace GADTs
{
    public interface ISideEffect<out TOutput>
    {
    }

    public interface IEffectVisitor<T, out TResult>
    {
        TResult Visit(PureEffect<T> eff);
        TResult Visit<TOutput>(ImpureEffect<TOutput, T> eff);
    }
    public abstract class Effect<T> {
        public abstract TResult Accept<TResult>(IEffectVisitor<T,TResult> v);
    }

    public class PureEffect<T> : Effect<T>
    {
        public T Value { get; }
        public override TResult Accept<TResult>(IEffectVisitor<T, TResult> v)
            => v.Visit(this);
    }

    public class ImpureEffect<TOutput, T> : Effect<T>
    {
        public ISideEffect<TOutput> SideEffect { get; }
        public Func<TOutput, Effect<T>> Next { get; }
        public override TResult Accept<TResult>(IEffectVisitor<T, TResult> v)
            => v.Visit(this);
    }

    public class IterativeEffectInterpreterVisitor<T> : IEffectVisitor<T, Task<(Effect<T>,T)>>
    {
        public Task<(Effect<T>, T)> Visit(PureEffect<T> eff)
            => Task.FromResult<(Effect<T>, T)>((null, eff.Value));

        public async Task<(Effect<T>,T)> Visit<TOutput>(ImpureEffect<TOutput, T> eff)
        {
            var valueOfSideEffect = await GetValueFrom(eff.SideEffect);
            var nextEffect = eff.Next(valueOfSideEffect);
            return (nextEffect, default);
        }

        private Task<T> GetValueFrom<T>(ISideEffect<T> sideEffect)
        {
            throw new NotImplementedException();
        }
    }

    public class EffectInterpreter
    {
        public async Task<T> Interpret<T>(Effect<T> eff)
        {
            var v = new IterativeEffectInterpreterVisitor<T>();
            var currentEffect = eff;
            T result;
            do
            {
                (currentEffect, result) = await currentEffect.Accept(v);
            } while (currentEffect != null);

            return result;
        }
    }

}
