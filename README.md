# .NET Hash Bruteforcer

> A project to excercise and solve new problems that are found along the way of making the application.


## 1. Description of the program

+ The program is written in `C#` and ***only*** uses `CPU` threads (uses all available threads).
+ It creates every possible combination from the given hardcoded charset which contains `90` characters including:
  - *Numbers*
  - *Letters*: All lowercase and uppercase letters of the english alphabet
  - *Special characters*
+ This is achieved by starting to generate guesses with the lowest possible length and working its way up to the given max. length for the words.
+ The default max. length is `6` which can be changed by the user from `4 - ∞`
+ The guesses to be checked are distributed evenly, based on the serial number of the given thread and the maximum available threads:
  -  ***E.g.***:    If the thread's serial number is `2` and the max. available threads is `4`<br>
                --> The `2nd` guess will be given to this thread at start and after that the `6th` guess will be given.

## 2. Syntax

```
hash-brute <hash> <maxlength>
```
| Argument | Variable type | Description |
| -------- | ------------- | ----------- |
| hash     | string        | The hash to be cracked by the program. Currently accepted types: MD5, SHA1, SHA-256-384-512 |
| maxlength| int           | The maximum length of the guesses made by the program. |

## 3. How the program works

> Generating guesses

+ For a given **n** max length, the program iterates trough `1 - n` on each thread and generates every possible combination from the used charset with the current length.
+ Guesses are created by having a matrix of integers, `int[,] wordCharVals = new int[maxThreads, n]`, with the length of the available threads in an environment and the max length.
  - When working with the matrix, the ***1st*** parameter tells, which thread the row belongs to and the ***2nd*** parameter tells which char we want to access
  - The matrix's rows contains indexes, where each individual item must be in the `0  -  charsByte.Length - 1 (89)` range.
+ Before creating the hash from the guess, a `byte[] guessBytes = new byte[l]` is declared, in which we then move the correct values from the `charsByte` array, with the corresponding indexes from `wordCharVals`.
