namespace Example.Extensions

open System
open System.Runtime.CompilerServices
open ZennoLab.CommandCenter

[<Extension>]
type FileExt =

  [<Extension>]
  static member CompareStr(str1: string, str2) =
    str1.Equals(str2, StringComparison.CurrentCultureIgnoreCase ||| StringComparison.InvariantCulture)

    
  [<Extension>]
  static member OpenPage (tab: Tab, url: string) =
    tab.Navigate(url, url)
    if tab.IsBusy then tab.WaitDownloading()

    
  [<Extension>]
  static member FoundAndClick (tab: Tab, xPath: string) =
    let he = tab.FindElementByXPath(xPath, 0)
    
    if he.IsVoid then
      failwith ("Не найден xPath: " + xPath)
    else
      tab.FullEmulationMouseMoveToHtmlElement he
      tab.FullEmulationMouseClick("left", "click")