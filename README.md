# <span style="color:#e6a40b">.NET Hash Bruteforcer</span>

> A project to excercise and solve new problems that are found along the way of making the application.


## <span style="color:#3ca11d">1. Description of the program</span>

+ The program is written in **`C#`** using the `.NET Core 3.1 Framework` and ***only*** uses `CPU` threads (all available threads).
+ It creates every possible combination from the given hardcoded charset which contains `90` characters including:
  - ***Numbers***
  - ***Letters***: All lowercase and uppercase letters of the english alphabet
  - ***Special characters***
+ This is achieved by starting to generate guesses with the lowest possible length and working its way up to the given max. length for the words.
+ The default max. length is `6` which can be changed by the user from `4 - ∞` .
+ The guesses to be checked are distributed evenly, based on the serial number of the given thread and the maximum available threads:
  -  ***E.g.***:    If the thread's serial number is `2` and the max. available threads is `4`<br>
                --> The `2nd` guess will be given to this thread at start and after that the `6th` guess will be given.

## <span style="color:#3ca11d">2. Syntax</span>

```
hash-brute <hash> <maxlength>
```
| Argument | Variable type | Description |
| -------- | ------------- | ----------- |
| hash     | string        | The hash to be cracked by the program. Currently accepted types: MD5, SHA1, SHA-256-384-512 |
| maxlength| int           | The maximum length of the guesses made by the program. |

## <span style="color:#3ca11d">3. How the program works</span>

> <span style="color:#1f8cb8">Generating guesses</span>

+ For a given ***n*** max length, the program iterates trough the `1 - n` range on each thread and generates every possible combination from the used charset with the current length.
+ Guesses are created by having a matrix of integers, `int[,] wordCharVals = new int[maxThreads, n]`, with the length of the available threads in an environment and the max length.
  - When working with the matrix, the ***1st*** parameter tells, which thread the row belongs to and the ***2nd*** parameter tells which char we want to access
  - The matrix's rows contains indexes, where each individual item must be in the `0  -  charsByte.Length - 1 (89)` range.
+ Before creating the hash from the guess, a `byte[] guessBytes = new byte[l]` is declared, in which we then move the correct values from the `charsByte` array, with the corresponding indexes from `wordCharVals`.

> <span style="color:#1f8cb8">Overall workflow</span>

1. (On start) The program checks for the amount of available threads in the given environment.
2. It validates the given arguments:
    + The ***hash***, based on its length and used characters
    + The optional ***maxlength*** argument based on wether it's convertable to an integer and if it's bigger or equal to 4.<br>
    --> If so, then the maxlength is set to the custom value.
3. The hash then is converted from its hexadecimal format and placed into the `byte[] hashToCrackBytes` array.
4. Based on the `hashToCrack`'s length the proper method is assigned to the available threads.