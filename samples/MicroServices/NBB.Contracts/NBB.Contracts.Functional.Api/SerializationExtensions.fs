[<AutoOpen>]
module SerializationExtensions

open Newtonsoft.Json
open Giraffe.Serialization
open System.IO
open System.Threading.Tasks


type MyNewtonsoftJsonSerializer(settings : JsonSerializerSettings) =
    inherit NewtonsoftJsonSerializer(settings) 

    member this.PopulateObject1<'T> (stream : Stream) (obj: 'T) =
        use sr = new StreamReader(stream, true)
        use jr = new JsonTextReader(sr)
        let sr = JsonSerializer.Create settings
        sr.Populate(jr, obj)
        Task.CompletedTask

type IJsonSerializer with
    member this.PopulateObjectAsync<'T> (stream : Stream) (obj: 'T) =
        let jsonSer = this :?> MyNewtonsoftJsonSerializer
        jsonSer.PopulateObject1 stream obj
