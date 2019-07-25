module NBB.HandlerUtils

open System.Text.RegularExpressions
open FSharp.Control.Tasks.V2
open Giraffe
open MediatR
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives
open NBB.Application.DataContracts
open NBB.Correlation
open NBB.Core.Abstractions

// ---------------------------------
// Utils
// ---------------------------------
let inject<'T> (f : 'T -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let resolved = ctx.RequestServices.GetService<'T>()
        f resolved next ctx

let bindQuery<'T> = bindQuery<'T> None

// ---------------------------------
// Command & Query processing
// ---------------------------------
let sendCommand (command : IRequest) (mediator : IMediator) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            do! mediator.Send(command)
            return! next ctx
        }

let sendQuery (query : IRequest<'TResponse>) (f : 'TResponse -> HttpHandler) (mediator : IMediator) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task { 
            let! result = mediator.Send(query)
            return! f result next ctx 
        }

let commandResult (command : IMetadataProvider<CommandMetadata>) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let result = {| 
            CommandId = command.Metadata.CommandId 
            CorrelationId = CorrelationManager.GetCorrelationId() 
        |}

        Successful.OK result next ctx

let queryResult =
    Successful.OK 

// ---------------------------------
// Handler compositions
// ---------------------------------
let mediator = inject<IMediator>

let mediatorSendCommand command = 
    mediator (sendCommand command) >=> commandResult command

let mediatorSendQuery query : HttpHandler = 
    mediator (sendQuery query queryResult)

// ---------------------------------
// Model binding hacks
// ---------------------------------
let abort = System.Threading.Tasks.Task.FromResult None

let routeBindNonStrict<'T> (route : string) (routeHandler : 'T -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let pattern =
            route.Replace("{", "(?<").Replace("}", ">[^/\n]+)")
            |> sprintf "^%s$"
        let regex = Regex(pattern, RegexOptions.IgnoreCase)
        let result = regex.Match(SubRouting.getNextPartOfPath ctx)
        match result.Success with
        | true ->
            let groups = result.Groups

            let result =
                regex.GetGroupNames()
                |> Array.skip 1
                |> Array.map (fun n -> n, StringValues groups.[n].Value)
                |> dict
                |> ModelParser.parse<'T> None
            routeHandler result next ctx
        | _ -> abort

let BindJsonOver<'T> (modelHandler : 'T -> HttpHandler) (model : 'T) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let serializer = ctx.GetJsonSerializer()
            do! serializer.PopulateObjectAsync ctx.Request.Body model
            return! modelHandler model next ctx
        }

let routeAndJsonBind<'T> (route : string) (routeHandler : 'T -> HttpHandler) : HttpHandler =
    routeBindNonStrict<'T> route (BindJsonOver<'T> routeHandler)
