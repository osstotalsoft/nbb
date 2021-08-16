// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Core.Effects
{

    public interface ISideEffect<out TResult>
    {
    }

    public interface ISideEffect : ISideEffect<Unit>
    {
    }
}
