[Back](../README.md)

# Excute an alias from a command line

## How is written a command line? 
 > command arg1 arg2 arg3

* If the *command line* starts with `$ & | @ # ( ) § ! { } - _ \\ + - * / = < >  ; : % ` then the command is this character
 * Otherwise everything before the first *space* is the command
 * Everything after the first *space* is the arguments

## Activity diagram

 ```mermaid

 flowchart TD
    id1(Build cmdline from user query)
    id2{{Has cmdline parameters?}}
    id3(Replace params with user params)
    id4{{Is CurrentAlias null?}}
    id5(Nothing to execute)
    id6{{Is Executable?}}
    id7(Execute it)
    id8(Display results)

    id1 --> id2
    id2 -->|no| id3
    id3 --> id4
    id4 -->|yes| id5
    id4 -->|no| id6
    id6 -->|yes| id7
    id2 -->|yes| id7
    id6 -->|no| id8
    id7 --> id8
 ```
## Code

https://github.com/jibedoubleve/lanceur-bis/blob/5ec7b3065170f31ac4399c9ec3191c95a620053e/src/Lanceur/Views/MainViewModel.cs#L179-L221