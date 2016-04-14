namespace SQLQUERYtestApp

open WebSharper
open MySql
open MySql.Data
open System.Data
open WebSharper.Json
open WebSharper.Sitelets
module Server =
    let [<Literal>]connectionStr = "server=145.24.200.232;user=mustafa;database=project;password=root"
    let [<Literal>]trommelA = "SELECT DISTINCT DATE_FORMAT(`datetime`, '%Y-%m-%d') FROM `trommel` ORDER BY `datetime` DESC" //prepend them later to gather them DESC way
    let trommelB date = "SELECT COUNT(`datetime`) FROM `trommel` WHERE `datetime` <= '" + date + "'"
    [<Remote>]
    let DoSomething (input:string) =
        let connection = new MySqlClient.MySqlConnection(connectionStr)
        do connection.Open()
        let content stuff =
          Content.Json stuff
        let command() =
          System.Console.WriteLine("start")
          let x = new MySqlClient.MySqlCommand(trommelA, connection)
          let reader = x.ExecuteReader()
          let data = reader |> (Seq.unfold (fun (reader) ->
              if reader.Read() then
                Some(List.init reader.FieldCount (fun i -> reader.GetValue(i)), reader)
              else
                None)) |> Seq.toList
          let data' = List.fold (fun s x -> match x with
                                            | h::t -> (h.ToString())::s
                                            | [] -> s) [] data
          do reader.Dispose()
          let dateswithcount data =
            List.map (fun x ->
              let y = new MySqlClient.MySqlCommand((trommelB x), connection)
              let reader = y.ExecuteReader()
              let count = reader |> (Seq.unfold (fun (reader) ->
                    if reader.Read() then
                      Some(List.init reader.FieldCount (fun i -> reader.GetValue(i)), reader)
                    else
                      None)) |> Seq.toList
              let count' = List.fold (fun s x ->  match x with
                                                  | h::t -> (h.ToString())::s
                                                  | [] -> s) [] count
              let count'' = List.nth count' 0
              do reader.Dispose()
              x, count''
              ) data
          try
            let finalList = dateswithcount data'
            List.iter (fun (x,y) -> System.Console.WriteLine(x.ToString()+"   "+y.ToString())) finalList
            finalList.ToString()
          with
            | x -> System.Console.WriteLine(x)
                   ""
        async {
            return command()
        }