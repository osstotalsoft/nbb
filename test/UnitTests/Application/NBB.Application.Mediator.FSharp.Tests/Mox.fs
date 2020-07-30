module Mox
type Mock<'a, 'b> (fn: 'a -> 'b) =
    let mutable wasCalled = false
    member _.Fn = 
        fun (a:'a) -> 
            wasCalled <- true
            fn a
    member _.WasCalled = wasCalled

[<AutoOpen>]
module Mocks =
    let mock (fn: 'a -> 'b) = Mock fn

    let wasCalled (mock:Mock<'a, 'b>) = mock.WasCalled

