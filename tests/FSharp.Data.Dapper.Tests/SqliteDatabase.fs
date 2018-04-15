module SqliteDatabase 

open System
open Microsoft.Data.Sqlite

open FSharp.Data.Dapper

module Connection =
    let private mkConnectionString (dataSource : string) =
        sprintf 
            "Data Source = %s; Mode = Memory; Cache = Shared;" 
            dataSource

    let private mkDedicatedConnectionString () = 
        mkConnectionString
            (sprintf 
                "DEDICATED -> %s" 
                (Guid.NewGuid().ToString()))

    let mkShared () = new SqliteConnection (mkConnectionString "MASTER")
    let mkDedicated () = new SqliteConnection (mkDedicatedConnectionString())
    
module Queries = 
    let private connectionF () = Connection.SqliteConnection (Connection.mkShared())

    let querySeqAsync<'R>          = querySeqAsync<'R> (connectionF)
    let querySingleAsync<'R>       = querySingleAsync<'R> (connectionF)
    let querySingleOptionAsync<'R> = querySingleOptionAsync<'R> (connectionF)

module Types =

    [<CLIMutable>]
    type Person = 
        { Id         : int64
          Name       : string
          Patronymic : string option
          Surname    : string }

let private masterConnection = Connection.mkShared()
let masterConnectionF () = Connection.SqliteConnection (Connection.mkShared())
let initializationQuery = querySingleOptionAsync<int> masterConnectionF {
    script """
        CREATE TABLE Person (
            Id integer primary key,
            Name text not null,
            Patronymic text null,
            Surname text not null
        );

        INSERT INTO Person VALUES (null, 'Иван', null, 'Иванов')
    """
} 

let Run f = 
    masterConnection.Open()

    initializationQuery
    |> Async.RunSynchronously
    |> ignore

    let result = f()
    masterConnection.Close()

    result