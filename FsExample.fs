namespace Example

open System
open System.Threading
open ZennoLab.CommandCenter
open ZennoLab.Emulation
open ZennoLab.InterfacesLibrary.ProjectModel
open ZennoLab.InterfacesLibrary.ProjectModel.Enums
open Example.Extensions

type Lang = En | Ru

type TemplateExample(instance: Instance, project: IZennoPosterProjectModel) =

  member this.Run() =
    
    let tab = instance.ActiveTab

    let toLog = project.SendInfoToLog

    let langState =
      match project.Profile.Language with
      | lang when lang.CompareStr("en") -> Lang.En
      | lang when lang.CompareStr("ru") -> Lang.Ru
      | _ -> failwith ("Неподдерживаемый язык")

    let getLangWord (ru, en) =
      match langState with
      | Lang.Ru -> ru
      | Lang.En -> en

    toLog "Установка белого списка url"
    instance.SetContentPolicy(policy = "WhiteList", regexs = [| "lessons.zennolab.com" |])

    toLog "Открытие страницы в соответствии с профилем"
    tab.OpenPage ("https://lessons.zennolab.com/" + getLangWord("ru", "en") + "/index")

    let list = [|
      "Windows";
      "*nix";
      "Mac OS";
      getLangWord ("Другая", "Other");
      getLangWord ("Россия", "USA");
    |]

    toLog "Выбор элементов"
    
    for text in list do
      toLog ("Выбор " + text)
      tab.FoundAndClick ("id('inputs')//h2[contains(text(), '" + text + "')]/preceding-sibling::input[1]")

    toLog "Ввод текста"
    tab.FoundAndClick "//textarea[@name='text']"
    
    instance.SendText (project.Profile.Name + " " +
                       project.Profile.Surname + " " +
                       getLangWord ("возраст:", "age:") + " " +
                       project.Profile.Age.ToString(), 50)

    toLog "Выбор пола"
    if project.Profile.Sex = ProfileSex.Male then
        tab.FoundAndClick "//select[@name='gender']"
        Emulator.SendKey(tab.Handle, Windows.Forms.Keys.Up, KeyboardEvent.Press) |> ignore

    toLog "Выбор языка"
    tab.FoundAndClick ("//option[text()='" + getLangWord ("Русский", "English") + "']")

    toLog "Выбор возраста"
    let age = project.Profile.Age
    let option =
        if age < 16 then 1
        elif 16 <= age && age <= 30 then 2
        elif 31 <= age && age <= 60 then 3
        else 4
    tab.FoundAndClick ("//option[text()='" + getLangWord ("Возраст:", "Age:") + "']/following-sibling::option[" + (string) option + "]")
    0


type public Main() =

    interface IZennoCustomCode with
      member this.ExecuteCode(instance: Instance, project: IZennoPosterProjectModel): int =
        
        TemplateExample(instance, project).Run()
        

    interface IZennoCustomEndCode with
      member this.GoodEnd(instance, project) = ()
      member this.BadEnd(instance, project) = ()
