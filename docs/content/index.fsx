(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/FSharp.ORM"

(**
FSharp.ORM
======================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The FSharp.ORM library can be <a href="https://nuget.org/packages/FSharp.ORM">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.ORM</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

This example demonstrates setting up a database connection and using it to insert, edit, query, and delete records.

## Schema
```sql
create table Department (
    DepartmentId int primary key,
    DepartmentName varchar(50) unique,
    VersionNo int not null
);
create table Employee (
    EmployeeId int identity primary key,
    EmployeeName varchar(50),
    DepartmentId int FOREIGN KEY REFERENCES Department(DepartmentId),
    VersionNo int not null
);
```
*)

#r "FSharp.QueryProvider.dll"
#r "FSharp.ORM.dll"

open FSharp.ORM

open System
open FSharp.ORM

// define a module wraps Soma.Core.Db module
module MyDb = 
    let config = 
        { new MsSqlConfig() with
            member this.ConnectionString = "Data Source=localhost;Initial Catalog=FSharp.ORM.Tutorial;Integrated Security=True" }
    let query<'T> = Db.query<'T> config
    let queryOnDemand<'T> = Db.queryOnDemand<'T> config
    let queryable<'T when 'T : not struct> = Db.queryable<'T> config
    let queryableDirectSql<'T when 'T : not struct> = Db.queryableDirectSql<'T> config
    let queryableDelete<'T when 'T : not struct> : Linq.IQueryable<'T> -> unit = Db.queryableDelete<'T> config
    let execute sql expr = Db.execute config sql expr
    let find<'T when 'T : not struct> = Db.find<'T> config
    let tryFind<'T when 'T : not struct> = Db.tryFind<'T> config
    let insert<'T when 'T : not struct> = Db.insert<'T> config
    let update<'T when 'T : not struct> = Db.update<'T> config
    let delete<'T when 'T : not struct> = Db.delete<'T> config
    let call<'T when 'T : not struct> = Db.call<'T> config

type DepartmentId = int
// define a record mapped to a table 
type Department =
    { [<Id>]
      DepartmentId : DepartmentId
      DepartmentName : string
      [<Version>]
      VersionNo : int option }
let createDepartment id name =
    { DepartmentId = id
      DepartmentName = name
      VersionNo = None }

type Employee =
    { [<Id(IdKind.Identity)>]
      EmployeeId : int option
      EmployeeName : string option
      DepartmentId : DepartmentId option
      [<Version>]
      VersionNo : int option }

let salesDepartmentId = 1
// insert 
let salesDepartment = 
    createDepartment salesDepartmentId "Sales"
    |> MyDb.insert

// load
let salesDepartmentLoaded =
    query { for x in MyDb.queryable<Department> do
            where(x.DepartmentId = salesDepartmentId)
            exactlyOne }

// load with a partial string comparison
let searchDepartments =
    query { for x in MyDb.queryable<Department> do
            where(x.DepartmentName.Contains("Sa")) }

printfn "Search results: %A" (searchDepartments |> Seq.toList)

// update
let updatedSalesDepartment = MyDb.update { salesDepartment with DepartmentName = "Sales & Marketing" }

printfn "Updated record: %A" updatedSalesDepartment

// delete
MyDb.delete updatedSalesDepartment
printfn "Deleted record: %A" updatedSalesDepartment

// delete all records that would be returned by a linq query
MyDb.queryableDelete
    (query {
        for x in MyDb.queryable<Department> do
        where(x.DepartmentName.Contains("Sa")) })
 
// execute arbitrary SQL
let rows = 
    MyDb.execute @"
    delete from Employee 
    " []
printfn "Affected rows: %A" rows

(**
Some more info

Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this sample library.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/FSharp.ORM/tree/master/docs/content
  [gh]: https://github.com/fsprojects/FSharp.ORM
  [issues]: https://github.com/fsprojects/FSharp.ORM/issues
  [readme]: https://github.com/fsprojects/FSharp.ORM/blob/master/README.md
  [license]: https://github.com/fsprojects/FSharp.ORM/blob/master/LICENSE.txt
*)
