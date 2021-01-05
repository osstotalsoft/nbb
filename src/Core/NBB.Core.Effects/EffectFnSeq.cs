using System;

namespace NBB.Core.Effects
{
    public abstract class EffectFnSeq<TA, TB>
    {
        public EffectFnSeq<TA, TC> Append<TC>(Func<TB, Effect<TC>> continuation)
            => new EffectFnSeq<TA, TC>.Node<TB>(this, new EffectFnSeq<TB, TC>.Leaf(continuation));

        public EffectFnSeq<TA, TC> AppendMany<TC>(EffectFnSeq<TB, TC> continuations)
            => new EffectFnSeq<TA, TC>.Node<TB>(this, continuations);

        public abstract LeftEffectFnSeq<TA, TB> ToLeft();

        internal abstract LeftEffectFnSeq<TA, TC> Go<TC>(EffectFnSeq<TB, TC> continuations);

        public Effect<TB> Apply(TA a)
            => ToLeft().Apply(a);

        public abstract int GetCount();

        internal class Leaf : EffectFnSeq<TA, TB>
        {
            public Func<TA, Effect<TB>> Single { get; }


            public Leaf(Func<TA, Effect<TB>> single)
            {
                Single = single;
            }

            public override LeftEffectFnSeq<TA, TB> ToLeft()
                => new LeftEffectFnSeq<TA, TB>.Singleton(Single);

            internal override LeftEffectFnSeq<TA, TC> Go<TC>(EffectFnSeq<TB, TC> continuations)
                => new LeftEffectFnSeq<TA, TC>.Cons<TB>(Single, continuations);

            public override int GetCount() => 1;


        }

        internal class Node<TX> : EffectFnSeq<TA, TB>
        {
            public EffectFnSeq<TA, TX> Left { get; }
            public EffectFnSeq<TX, TB> Right { get; }

            public Node(EffectFnSeq<TA, TX> left, EffectFnSeq<TX, TB> right)
            {
                Left = left;
                Right = right;
            }

            public override LeftEffectFnSeq<TA, TB> ToLeft()
                => Left.Go(Right);

            public override int GetCount() => Left.GetCount() + Right.GetCount();

            internal override LeftEffectFnSeq<TA, TC> Go<TC>(EffectFnSeq<TB, TC> continuations)
                => Left.Go(new EffectFnSeq<TX, TC>.Node<TB>(Right, continuations));
        }
    }

    public abstract class LeftEffectFnSeq<TA, TB>
    {
        public abstract Effect<TB> Apply(TA a);

        internal class Singleton : LeftEffectFnSeq<TA, TB>
        {
            public Func<TA, Effect<TB>> Single { get; }

            public Singleton(Func<TA, Effect<TB>> single)
            {
                Single = single;
            }

            public override Effect<TB> Apply(TA a)
                => Single(a);

        }

        internal class Cons<TX> : LeftEffectFnSeq<TA, TB>
        {
            public Func<TA, Effect<TX>> Head { get; }
            public EffectFnSeq<TX, TB> Tail { get; }

            public Cons(Func<TA, Effect<TX>> head, EffectFnSeq<TX, TB> tail)
            {
                Head = head;
                Tail = tail;
            }

            public override Effect<TB> Apply(TA a)
             => Head(a).Bind2(Tail);
        }
    }
}
