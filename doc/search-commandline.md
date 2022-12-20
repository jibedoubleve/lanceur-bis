[Back](../README.md)

# Search an alias from a command line

## Index

| Name               | Definition                                                                      |
| ------------------ | ------------------------------------------------------------------------------- |
| **Alias**          | a short name used to launch a specific application with specfic argument        |
| **Plugin**         | special command with a UI (or not)                                              |
| **Repository**     | a list of shortcuts managed by something else than *Lanceur*                    |
| **Macro**          | special keyword used in a shortcut (ie: @MULTI@ that launches multiple aliases) |
| **Internal Alias** | a keyword reserved by *Lanceur*                                                 |

## Activity diagram

```mermaid
flowchart TB
    id1(Search in aliases)
    id2(Search in plugins)
    id3(Search in repositories)
    id4(Search in internal aliases)

    id5{{Any result?}}
    id6{{Exact match?}}
    id7(Promote match)
    id8(Return results)
    id9(End)

    subgraph REP[Repositories]
        id1 --> id2
        id2 --> id3
        id3 --> id3
        id3 --> id4
    end
    REP --> id5
    id5 --> |yes| id6
    id5 --> |no| id9
    id6 --> |yes| id7
    id6 --> |no| id8
    id7 --> id8
    id8 --> id9
```