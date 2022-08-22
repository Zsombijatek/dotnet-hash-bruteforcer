# .NET Hash Bruteforcer

> A project to excercise and solve new problems that are found along the way of making the application.


## 1. Description of the program

+ The program is written in `C#` and ***only*** uses `CPU` threads.
+ It creates every possible combination from the given hardcoded charset which contains `90` characters including:
  + *Numbers*
  + *Letters*
  + *Special characters*
+ This is achieved by starting to generate guesses with the lowest possible length and working its way up to the given max. length for the words.
+ The default max. length is `6` which can be changed by the user from `4-∞`
+ The guesses to be checked are distributed evenly, based on the serial number of the given thread and the maximum available threads:
  +  `E.g.`:    If the thread's serial number is `2` and the max. available threads is `4`<br>
                --> The `2nd` guess will be given to this thread at start and after that the `6th` guess will be given.

## 2. Syntax

```
hash-brute <hash> <maxlength>
```
| Argument | Variable type | Description |
| -------- | ------------- | ----------- |
| hash     | string        | The hash to be cracked by the program. Currently accepted types: MD5, SHA1, SHA-256-384-512 |
| maxlength| int           | The maximum length of the guesses made by the program. |
