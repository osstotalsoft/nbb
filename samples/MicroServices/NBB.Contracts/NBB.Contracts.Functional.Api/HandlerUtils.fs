[<AutoOpen>]
module NBB.HandlerUtils

open System.Text.RegularExpressions
open FSharp.Control.Tasks.V2
open Giraffe
open Giraffe.ComputationExpressions
open MediatR
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives
open NBB.Application.DataContracts
open NBB.Correlation

// ---------------------------------
// Utils
// ---------------------------------
let toOption value =
    opt {
        if not (isNull value) then return value
    }

let inject<'T> (f : 'T -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let resolved = ctx.RequestServices.GetService<'T>()
        f resolved next ctx

// ---------------------------------
// Command & Query processing
// ---------------------------------
let sendCommand (command : 'T :> IRequest) (mediator : IMediator) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            do! mediator.Send(command)
            return! next ctx
        }

let sendQuery<'TQuery, 'TResponse when 'TQuery :> IRequest<'TResponse> and 'TResponse : null> 
        (query : 'TQuery) (f : 'TResponse option -> HttpHandler) (mediator : IMediator) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task { let! result = mediator.Send(query)
               return! f (toOption result) next ctx 
        }

let commandResult (command : 'T :> Command) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let result = {| 
            CommandId = command.Metadata.CommandId 
            CorrelationId = CorrelationManager.GetCorrelationId() 
        |}

        Successful.OK result next ctx

let queryResult (result : 'T option) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        match result with
        | Some x -> Successful.OK x next ctx
        | None -> RequestErrors.NOT_FOUND "Item not found" next ctx

// ---------------------------------
// Handler compositions
// ---------------------------------
let handleCommand command = 
    inject<IMediator> (sendCommand command) >=> commandResult command

let handleQuery query = 
    inject<IMediator> (sendQuery query queryResult)

let handleCommandFromRequest<'T when 'T :> Command> = 
    bindJson<'T> handleCommand

let handleQueryFromRequest<'TQuery, 'TResponse when 'TQuery :> Query<'TResponse> and 'TResponse : null> =
    bindQuery<'TQuery> None handleQuery

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
