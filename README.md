# graphhack

[![Hack Together: Microsoft Graph and .NET](https://img.shields.io/badge/Microsoft%20-Hack--Together-orange?style=for-the-badge&logo=microsoft)](https://github.com/microsoft/hack-together)

Quick console app to get all the teams the authenticated user is in, and produce a contribution summary with how many messages a user has sent and a percentage, by date.

The app uses the dependency injection container to register all it's services, to avoid the mess that sometimes comes with console apps.

To get started, you will need to modify Graph:ClientId in [appsettings.json](/Graph.Console/appsettings.json) to point at the right app registration. Graph:TenantId is optional (in the code if this is not supplied it will coalesce with "common", as per the graph api documentation). 

The main logic of the app lives in [App.cs](/Graph.Console/App.cs), along with the list of scopes that the app needs.
Controlling the output can be done by changing the value of General:OutputFormat in [appsettings.json](/Graph.Console/appsettings.json) to the corresponding [Enum](/Graph.Console/Enums/OutputFormat.cs) flag. Note, it needs to be running in release mode for it to obey this flag, in debug mode it will default to just outputting to the configured Logger, in my case the Console. The logic for the available outputters can be found [here](/Graph.Console/Outputters). The logic of the output is controlled by an injected service, found [here](/Graph.Console/Services/OutputService.cs)

Running the app will produce a file in each of the formats you've specified for each of the teams + for each of the channels in thise teams. It tries to run this in parallel as it needs to produce many files.

What I would have liked to have done in the future if I was to continue this:
* Allow the user to configure where the output location of the file is in appsettings.json
* Produce a UI over the results, e.g. in a Blazor application
* Make the file IO more efficient
* Extend the outputters to include more of them, e.g. pdf
* If an excel spreadsheet is added, maybe a graph can be auto generated
* Check for any changes in the calculated output before writing any changes with a cache of some sort or reading the file beforehand
