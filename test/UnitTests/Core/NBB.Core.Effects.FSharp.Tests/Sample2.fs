// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module Sample2

open System
open NBB.Core.Effects.FSharp

module Eff =
    let consoleWriteLine (str:string) =
        effect {
            Console.WriteLine(str)
        }

    let consoleReadLine =
        effect {
            return Console.ReadLine()
        }

    let program =
        effect{
            do! consoleWriteLine "Hello world!"
            let! x = consoleReadLine
            do! consoleWriteLine x
        }

