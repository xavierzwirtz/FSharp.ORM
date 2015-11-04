﻿//----------------------------------------------------------------------------
//
// Copyright (c) 2011 The Soma Team. 
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.txt file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------

namespace FSharp.ORM.IT

open System
open System.Configuration
open System.Data
open System.Data.Common
open System.Transactions
open NUnit.Framework
open FSharp.ORM

module DeleteTest = 

  type Department =
    { [<Id>]
      DepartmentId : int
      DepartmentName : string
      [<Version>]
      VersionNo : int }

  type Employee =
    { [<Id(IdKind.Identity)>]
      EmployeeId : int option
      EmployeeName : string option
      DepartmentId : int option
      [<Version>]
      VersionNo : int option }

  type NoId =
    { Name : string
      VersionNo : int }

  type NoVersion =
    { [<Id>]
      Id : int
      Name : string }

  [<Test>]
  let ``delete : no id``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    try
      SQLite.delete con { NoId.Name = "aaa"; VersionNo = 0 } |> ignore
      fail ()
    with 
    | :? InvalidOperationException as ex -> 
      assert_true <| ex.Message.StartsWith "[SOMA4005]"
      printfn "%A" ex
    | ex -> 
      fail ex

  [<Test>]
  let ``delete : no version``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    SQLite.delete con { NoVersion.Id = 1; Name = "aaa" }

  [<Test>]
  let ``delete : incremented version``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    let department = SQLite.find<Department> con [1]
    SQLite.delete con department

  [<Test>]
  let ``delete : incremented version : optimistic lock confliction``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    let department = { DepartmentId = 1; DepartmentName = "hoge"; VersionNo = -1 }
    try
      SQLite.delete con department |> ignore
      fail ()
    with 
    | OptimisticLockException _ as ex -> printfn "%s" (string ex)
    | ex -> fail ex

  [<Test>]
  let ``deleteIgnoreVersion``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    let department = { DepartmentId = 1; DepartmentName = "aaa"; VersionNo = -1 }
    SQLite.deleteWithOpt con department (DeleteOpt(IgnoreVersion = true))

  [<Test>]
  let ``deleteIgnoreVersion : no affected row``() =
    use ts = new TransactionScope()
    use con = SQLite.createConnection()
    let department = { DepartmentId = 0; DepartmentName = "aaa"; VersionNo = -1 }
    try
      SQLite.deleteWithOpt con department (DeleteOpt(IgnoreVersion = true)) |> ignore
      fail ()
    with 
    | NoAffectedRowException _ as ex -> printfn "%s" (string ex)
    | ex -> fail ex