namespace Example

open System
open System.Threading
open ZennoLab.CommandCenter
open ZennoLab.Emulation
open ZennoLab.InterfacesLibrary.ProjectModel
open ZennoLab.InterfacesLibrary.ProjectModel.Enums

type TemplateExample(instance: Instance, project: IZennoPosterProjectModel) =

    let tab = instance.ActiveTab

    let toLog x = project.SendInfoToLog x

    let openPage url =
        tab.Navigate(url, url)
        if tab.IsBusy then tab.WaitDownloading()

    let foundAndClick (xPath) =
        let he = tab.FindElementByXPath(xPath, 0)
        if he.IsVoid then
            failwith (xPath + " не найден")
        
        tab.FullEmulationMouseMoveToHtmlElement he
        tab.FullEmulationMouseClick("left", "click")

    let lang = project.Profile.Language
    let langState =
        if lang.Equals("ru", StringComparison.CurrentCultureIgnoreCase) then 1
        elif lang.Equals("en", StringComparison.CurrentCultureIgnoreCase) then 2
        else failwith ("Неизвестный язык профиля: " + lang)
        
  //let lang = "En"
  //let lang = "Ru"
    do toLog lang

    let langSet (ru, en) =
        match langState with
        | 1 -> ru | 2 -> en
        | _ -> failwith ("langState за пределами: " + lang)

    member this.Start() =

        toLog "Установка белого списка url"
        instance.SetContentPolicy(policy = "WhiteList", regexs = [| "lessons.zennolab.com" |])

        toLog "Открытие страницы в соответствии с профилем"
        openPage ("https://lessons.zennolab.com/" + lang.ToLower() + "/index")

        let list = [|
          "Windows";
          "*nix";
          "Mac OS";
          langSet ("Другая", "Other");
          langSet ("Россия", "USA");
        |]

        toLog "Выбор элементов"
        // Применение метода к каждому элементу массива list
        Array.iter (fun text ->
            toLog ("Выбор " + text)
            foundAndClick ("id('inputs')//h2[contains(text(), '" + text + "')]/preceding-sibling::input[1]")) list

        toLog "Ввод текста"
        foundAndClick "//textarea[@name='text']"
        instance.SendText (project.Profile.Name + " " +
                           project.Profile.Surname + " " +
                           langSet ("возраст:", "age:") + " " +
                           project.Profile.Age.ToString(), 50)

        toLog "Выбор пола"
        if project.Profile.Sex = ProfileSex.Male then
            foundAndClick "//select[@name='gender']"
            Emulator.SendKey(tab.Handle, Windows.Forms.Keys.Up, KeyboardEvent.Press) |> ignore

        toLog "Выбор языка"
        foundAndClick ("//option[text()='" + langSet ("Русский", "English") + "']")

        toLog "Выбор возраста"
        let age = project.Profile.Age
        let option =
            if age < 16 then 1
            elif 16 <= age && age <= 30 then 2
            elif 31 <= age && age <= 60 then 3
            else 4
        foundAndClick ("//option[text()='" + langSet ("Возраст:", "Age:") + "']/following-sibling::option[" + (string) option + "]")


type public Main() =

    interface IZennoCustomCode with
        member this.ExecuteCode(instance: Instance, project: IZennoPosterProjectModel): int =
            (new TemplateExample(instance, project)).Start()
            0

    interface IZennoCustomEndCode with
        member this.GoodEnd(instance, project) = ()
        member this.BadEnd(instance, project) = ()
