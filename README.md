# CSHARPNESS

A collections of dotNET C# projects aimed to make my life easier. These projects are remnants of projects i've done in the past and are here for reference only. I don't think I will make all of this work anytime soon, but then again, who knows.

### Adding a reference to a project

Goto the project that you want to make the reference from and add a reference to the project you want to link to. Example: (https://docs.microsoft.com/en-us/dotnet/core/tutorials/testing-with-cli)

``sh
cd ./cs-futils.console
dotnet add reference ./cs-futils.lib/cs-futils.lib.csproj
``

### Installing prerequisites

Make sure you have the dotnet-host installed to use the System.Web.UI.WebControls and the System.Web.UI.HtmlControls namespace.

``sh
sudo apt-get install dotnet-host
``

To bundle to compiled assembly and its dependencies for dotnet-core please use the following package. (https://github.com/qmfrederik/dotnet-packaging).

