# Excute an alias from a command line

## How is written a command line? 
 > command arg1 arg2 arg3

* If the *command line* starts with `$ & | @ # ( ) § ! { } - _ \\ + - * / = < >  ; : % ` then the command is this character
 * Otherwise everything before the first *space* is the command
 * Everything after the first *space* is the arguments

## Sequence diagram
```mermaid
sequenceDiagram

participant vm   as MainViewModel
participant exec as ExecutionManager
participant qr   as QueryResult
participant wcm  as WildcardManager

vm ->> +exec: ExecuteAsync
    alt is Executable
        rect rgb(212, 253, 205	)
        alt is AliasQueryResult?
            exec ->>  +qr: ExecuteAsync();
            note right of qr: Parameter handling<br>can differ from<br>QueryResult to another
            qr ->>  qr: Execute behaviour
            qr -->> -exec: ExecutionResponse
            exec -->> -vm: ExecutionResponse
        else
            exec ->>  +exec: ExecuteAliasAsync
            note right of exec: call ExecuteProcess
            exec ->> +wcm: replace wildcards with values
            note right of wcm: Takes user parameters.<br>If none, use the parameters<br>specified in the alias
            wcm -->> -exec: Parameters
            exec ->>  exec: Process.Start()
            exec -->> -vm: ExecutionResponse
        end
        end
    else
        exec -->> exec: ExecuteResponse
        note right of exec: If request.QueryResult is null,<br>returns "Alias does not exist".<br>Else returns an EmptyResult  
    end
```

# Search an alias from a command line

## Index

| Name               | Definition                                                                      |
| ------------------ | ------------------------------------------------------------------------------- |
| **Alias**          | a short name used to launch a specific application with specfic argument        |
| **Plugin**         | special command with a UI (or not)                                              |
| **Repository**     | a list of shortcuts managed by something else than *Lanceur*                    |
| **Macro**          | special keyword used in a shortcut (ie: @MULTI@ that launches multiple aliases) |
| **Internal Alias** | a keyword reserved by *Lanceur*                                                 |

## Sequence diagram

```mermaid
sequenceDiagram

participant vm   as MainViewModel
participant src  as SearchService
participant clm  as CommandLineManager
participant sld  as StoreLoader
participant str  as Store

note right of str: A store should be able to search<br>and reutrn QueryResult.<br>Each store has its own source
vm ->> +clm : BuildFromText()
clm -->> -vm: CmdLine
vm -->> +src: Search()
src ->> +sld: Load()
sld -->> -src: Stores

loop For each store
rect rgb(212, 253, 205	)
    src ->> +str: Search()
    str -->> -src: Append results
end
end
src ->> src: Bubble up exact match
src ->> src: RefreshThumbnails
src -->> -vm: IEnumerable<QueryResult>
```