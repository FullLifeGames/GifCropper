# GifCropper
A simple application that crops a given gif file or folder containing gifs.

## Usage

* Get the application from the [last successful run](https://github.com/FullLifeGames/GifCropper/actions?query=event%3Apush+is%3Asuccess+branch%3Amain) artifact
* Use: `./GifCropper.exe {inPath} {outPath}`
  * `{inPath}` can be a file or a folder
  * `{outPath}` has to be a folder

## Build

* Install the [.NET 5 SDK](https://dotnet.microsoft.com/download/visual-studio-sdks)
* Use: `dotnet build src`
