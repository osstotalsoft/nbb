[<AutoOpen>]
module ServiceCollectionExtensions

open System
open System.Reflection
open MediatR
open Microsoft.Extensions.DependencyInjection

type Assembly with 
    member this.ScanFor (assignableType: Type) =
        this.GetTypes() 
        |> Seq.filter (fun t -> assignableType.IsAssignableFrom(t) && not t.IsInterface &&  not t.IsAbstract)

type IServiceCollection with
    member this.AddScopedContravariant<'TBase, 'TResolve> ?assembly =
        let isInvalid = not typeof<'TBase>.IsGenericType || typeof<'TBase>.IsOpenGeneric()
        if isInvalid then ()
        else  
            let baseDescription = typeof<'TBase>.GetGenericTypeDefinition()
            let baseInnerType = typeof<'TBase>.GetGenericArguments() |> Seq.head
            let searchedAssebly = 
                match assembly with
                | Some assembly -> assembly
                | None -> baseInnerType.Assembly

            let types = searchedAssebly.ScanFor(baseInnerType);
                
            for t in types do
                this.AddScoped(baseDescription.MakeGenericType t, typeof<'TResolve>) |> ignore

            //types |> Seq.iter (fun t -> this.AddScoped(baseDescription.MakeGenericType(t), typeof<'TResolve>) |> ignore)
            //()
            

