namespace PluginLib
open System
open System.ServiceProcess
open System.IO
open Oracle.ManagedDataAccess.Client
open Oracle.ManagedDataAccess.Types

[<AutoOpen>]
module Expect =
    type ExpectedValue<'a when 'a :> IComparable> =
        | Eq of 'a
        | Between of 'a * 'a
        | Lt of 'a
        | Le of 'a
        | Ge of 'a
        | Gt of 'a
        | OneOf of 'a list
        | NoneOf of 'a list
        | Null
        | NotNull
        | TestF of ('a -> bool) * string

    type Result<'a> =
        | Valid of 'a
        | Invalid of 'a * string

    type DBNull = DBNull
    type DBNotNull = DBNotNull

    type ConnectionString = string
    type Sql = string
    type Query = ConnectionString * Sql

    let isExpected (v : IComparable) expected =
        match expected with
            | Eq a -> (a :> obj) = v
            | Between (a, b) -> v >= a && v <= b
            | Lt a -> v < a
            | Le a -> v <= a
            | Ge a -> v >= a
            | Gt a -> v > a
            | OneOf xs -> xs |> List.contains v
            | NoneOf xs -> not <| (xs |> List.contains v)
            | Null -> v = DBNull
            | NotNull -> v <> DBNull
            | TestF (f, msg) -> f v

    let checkExpected expected (v : IComparable) = 
        match expected with
            | TestF (f, msg) ->
                if isExpected v expected then
                    Valid v
                else Invalid (v, msg)
            | _ ->
                if isExpected v expected then
                    Valid v
                else Invalid (v, expected.ToString())


module Sql =
    let withConnection (connStr : string) (sql : string) =
        Query (connStr, sql)
    [<RequireQualifiedAccess>]
    module Assert =
        let containsRow (expectedValues : ExpectedValue<IComparable> list) (connStr, sql) : Result<IComparable> list list =
            use conn = new OracleConnection(connStr)
            conn.Open()
            use command = new OracleCommand(sql, conn)
            use reader = command.ExecuteReader()
            if reader.FieldCount <> expectedValues.Length then
                failwith "xxx"
            let rows =
                [ while reader.Read() do
                    let a =
                        [ for i in 1 .. reader.FieldCount - 1 do
                            let v =
                                if reader.IsDBNull(i) then
                                    DBNull :> IComparable
                                else
                                    match reader.GetOracleValue i with
                                    | :? int32 as x -> x
                                    | :? OracleClob as x -> x.Value
                                    | :? OracleBlob as x -> DBNotNull
                                    | x -> x :?> IComparable
                            v
                        ]
                    List.map2 checkExpected expectedValues a
                ]
            rows




module W =
    let sensoAutoupd =  "senso/senso@autoupd"
    let r =
        "select dummy, 3, 4 from dual"
        |> Sql.withConnection sensoAutoupd
        |> Sql.Assert.containsRow
            [ Eq "X"; Eq 3; TestF ((fun x -> x = -1 || x > 5), ""); Null; NotNull ]



module IO =
    let availableDiskspace () =
        DriveInfo.GetDrives()
        |> Array.choose (fun x -> if x.IsReady then Some x.AvailableFreeSpace else None)


module Service =
    let getStatus serviceName =
        try
            use serviceController = new ServiceController(serviceName)
            serviceController.Status
        with
        | e ->
            failwith $"{e}"
    






