﻿module QuerySignleTests

open Expecto

open FSharp.Data.Dapper
open FSharp.Data.Dapper.Query.Parameters

open Fixtures

[<Tests>]
let querySingleTests = 
    testList "query single tests" [  
        yield! testFixture ``connection with empty person table`` [

            "Must return Some when query count", 
            fun connection -> 

                let script = "select count(1) from Person"

                let countOfPersons = 
                    Query(script)
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously

                Expect.isSome countOfPersons "count of persons not Some(0)"
                Expect.equal countOfPersons.Value 0 "count of persons not equal 0"

            "Must return None when person not found",
            fun connection ->

                let parameters = Parameters.Create [ "Id" <=> 1 ]
                let script = "select * from Person where Id = @Id"

                let person = 
                    Query(script, parameters)
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously

                Expect.isNone person "got Some instead None from empty table"            
        ]

        yield! testFixture ``connection with filled person table`` [
            
            "Must return Some when person found",
            fun connection -> 

                let parameters = Parameters.Create [ "Id" <=> 1 ]
                let script = "select * from Person where Id = @Id"
                
                let person = 
                    Query(script, parameters)
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously
                
                Expect.isSome person "got Some instead None from empty table"
        ]
    ]