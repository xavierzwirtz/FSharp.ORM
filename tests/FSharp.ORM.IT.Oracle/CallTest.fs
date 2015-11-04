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

module CallTest = 

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

  type ProcNoneParam = 
    { Unit : unit }

  type ProcSingleParam =
    { Param1 : int }

  type ProcMultiParams =
    { Param1 : int
      [<ProcedureParam(Name = "Param2", Direction = Direction.InputOutput)>]
      Hoge : int
      [<ProcedureParam(Direction = Direction.Output)>]
      Param3 : int }

  [<Procedure(Name = "ProcResult")>]
  type ProcEntityResult =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee list }

  [<Procedure(Name = "ProcResult")>]
  type ProcEntityResizeArrayResult =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee ResizeArray }

  [<Procedure(Name = "ProcResult")>]
  type ProcTupleResult =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : (int * string * int * int) list }

  [<Procedure(Name = "ProcResultAndOut")>]
  type ProcEntityResultAndOut =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee list
      [<ProcedureParam(Direction = Direction.Output)>]
      EmployeeCount : int }

  [<Procedure(Name = "ProcResultAndOut")>]
  type ProcTupleResultAndOut =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : (int * string * int * int) list
      [<ProcedureParam(Direction = Direction.Output)>]
      EmployeeCount : int }

  type ProcResultAndUpdate =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee list }

  type ProcUpdateAndResult =
    { EmployeeId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee list }

  [<Procedure(Name = "ProcResults")>]
  type ProcEntityResults =
    { EmployeeId : int
      DepartmentId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : Employee list
      [<ProcedureParam(Direction = Direction.Result)>]
      DeptList : Department list }

  [<Procedure(Name = "ProcResults")>]
  type ProcTupleResults =
    { EmployeeId : int
      DepartmentId : int
      [<ProcedureParam(Direction = Direction.Result)>]
      EmpList : (int * string * int * int) list
      [<ProcedureParam(Direction = Direction.Result)>]
      DeptList : (int * string * int) list }

  type FuncMultiParams =
    { [<ProcedureParam(Direction = Direction.ReturnValue)>]
      ReturnValue : int 
      Param1 : int
      Param2 : int }

  [<Test>]
  let ``call with ADO.NET fashion : ProcMultiParams``() =
    use ts = new TransactionScope()
    let factory = Oracle.config.DbProviderFactory
    use connection = factory.CreateConnection()
    connection.ConnectionString <- Oracle.config.ConnectionString
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandType <- CommandType.StoredProcedure
    command.CommandText <- "ProcMultiParams"
    let p1 = factory.CreateParameter()
    p1.ParameterName <- "Param1"
    p1.Direction <- ParameterDirection.Input
    p1.Value <- box 1
    command.Parameters.Add(p1) |> ignore
    let p2 = factory.CreateParameter()
    p2.ParameterName <- "Param2"
    p2.Direction <- ParameterDirection.InputOutput
    p2.Value <- box 2
    command.Parameters.Add(p2) |> ignore
    let p3 = factory.CreateParameter()
    p3.ParameterName <- "Param3"
    p3.Direction <- ParameterDirection.Output
    p3.Value <- box 3
    command.Parameters.Add(p3) |> ignore
    use reader = command.ExecuteReader()
    assert_equal 1 p1.Value
    assert_equal 3 p2.Value
    assert_equal 1 p3.Value

  [<Test>]
  let ``call with ADO.NET fashion : ProcResult``() =
    use ts = new TransactionScope()
    let factory = Oracle.config.DbProviderFactory
    use connection = factory.CreateConnection()
    connection.ConnectionString <- Oracle.config.ConnectionString
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandType <- CommandType.StoredProcedure
    command.CommandText <- "ProcResult"
    let p1 = factory.CreateParameter()
    p1.ParameterName <- "EmployeeId"
    p1.Direction <- ParameterDirection.Input
    p1.Value <- box 1
    command.Parameters.Add(p1) |> ignore
    let p2 = factory.CreateParameter() :?> Oracle.DataAccess.Client.OracleParameter
    p2.ParameterName <- "curr"
    p2.Direction <- ParameterDirection.Output
    p2.OracleDbType <- Oracle.DataAccess.Client.OracleDbType.RefCursor
    p2.Value <- box 2
    command.Parameters.Add(p2) |> ignore
    use reader = command.ExecuteReader()
    while reader.Read() do
      printfn "%A %A" (reader.GetValue(0)) (reader.GetValue(1))
    printfn "%A" (p2.Value.GetType())

  [<Test>]
  let ``call with ADO.NET fashion : FuncMultiParams``() =
    use ts = new TransactionScope()
    let factory = Oracle.config.DbProviderFactory
    use connection = factory.CreateConnection()
    connection.ConnectionString <- Oracle.config.ConnectionString
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandType <- CommandType.StoredProcedure
    command.CommandText <- "FuncMultiParams"
    let p1 = factory.CreateParameter()
    p1.ParameterName <- "Ret"
    p1.Direction <- ParameterDirection.ReturnValue
    p1.Value <- 0
    command.Parameters.Add(p1) |> ignore
    let p2 = factory.CreateParameter()
    p2.ParameterName <- "Param1"
    p2.Direction <- ParameterDirection.Input
    p2.Value <- box 1
    command.Parameters.Add(p2) |> ignore
    let p3 = factory.CreateParameter()
    p3.ParameterName <- "Param2"
    p3.Direction <- ParameterDirection.InputOutput
    p3.Value <- box 2
    command.Parameters.Add(p3) |> ignore
    use reader = command.ExecuteReader()
    assert_equal 3 p1.Value
    assert_equal 1 p2.Value
    assert_equal 2 p3.Value

  [<Test>]
  let ``call : ProcNoneParam``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcNoneParam> { Unit = () }
    ()

  [<Test>]
  let ``call : ProcSingleParam``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcSingleParam> { Param1 = 10 }
    assert_equal 10 result.Param1

  [<Test>]
  let ``call : PorcMultiParams``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcMultiParams> { Param1 = 1; Hoge = 2; Param3 = 3 }
    assert_equal { ProcMultiParams.Param1 = 1; Hoge = 3; Param3 = 1} result

  [<Test>]
  let ``call : ProcEntityResult``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcEntityResult> { EmployeeId = 1; EmpList = [] }
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length

  [<Test>]
  let ``call : ProcEntityResizeArrayResult``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcEntityResizeArrayResult> { EmployeeId = 1; EmpList = ResizeArray() }
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Count

  [<Test>]
  let ``call : ProcTupleResult``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcTupleResult> { EmployeeId = 1; EmpList = [] }
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length

  [<Test>]
  let ``call : ProcEntityResultAndOut``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcEntityResultAndOut> { EmployeeId = 1; EmpList = [] ; EmployeeCount = 0; }
    printfn "%A" result
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length
    assert_equal 4 result.EmployeeCount

  [<Test>]
  let ``call : ProcTupleResultAndOut``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcTupleResultAndOut> { EmployeeId = 1; EmpList = [] ; EmployeeCount = 0; }
    printfn "%A" result
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length
    assert_equal 4 result.EmployeeCount

  [<Test>]
  let ``call : ProcResultAndUpdate``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcResultAndUpdate> { EmployeeId = 1; EmpList = [] ; }
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length
    let dept = Oracle.find<Department> [1]
    assert_equal "HOGE" dept.DepartmentName

  [<Test>]
  let ``call : ProcUpdateAndResult``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcUpdateAndResult> { EmployeeId = 1; EmpList = [] ; }
    assert_equal 1 result.EmployeeId
    assert_equal 3 result.EmpList.Length
    let dept = Oracle.find<Department> [1]
    assert_equal "HOGE" dept.DepartmentName

  [<Test>]
  let ``call : ProcEntityResults``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcEntityResults> { EmployeeId = 1; DepartmentId = 1; EmpList = [] ; DeptList = [] }
    assert_equal 1 result.EmployeeId
    assert_equal 1 result.DepartmentId
    assert_equal 3 result.EmpList.Length
    assert_equal 1 result.DeptList.Length

  [<Test>]
  let ``call : ProcTupleResults``() =
    use ts = new TransactionScope()
    let result = Oracle.call<ProcTupleResults> { EmployeeId = 1; DepartmentId = 1; EmpList = [] ; DeptList = [] }
    assert_equal 1 result.EmployeeId
    assert_equal 1 result.DepartmentId
    assert_equal 3 result.EmpList.Length
    assert_equal 1 result.DeptList.Length

  [<Test>]
  let ``call : FuncMultiParams``() =
    use ts = new TransactionScope()
    let result = Oracle.call<FuncMultiParams> { Param1 = 1; Param2 = 2; ReturnValue = 0 }
    assert_equal 1 result.Param1
    assert_equal 2 result.Param2
    assert_equal 3 result.ReturnValue
