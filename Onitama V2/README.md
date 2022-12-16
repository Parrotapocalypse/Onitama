# Onitama
This is a computer implementation of an existing tabletop game. This project was created by Oliver FitzPatrick.
This file is best viewed in something that supports markdown, like GitHub. However, it's perfectly readable text.
## How to Play
Onitama is a game played on a 5x5 board. Each player has 5 pieces, 4 students and 1 master. If a master is captured, its owner loses. You can also win if your master reaches the starting square of your opponent's master
Every piece has the same movement; this is determined by cards each player is given. At any time, each player has 2 cards and 1 is in the middle of the table, for a total of 5, selected from 16 possible cards at the start of the game.
Each card lists possible squares to which a piece can move. On a player's turn, they choose a card to use and a piece to move. The used card is passed into the middle of the table; your opponent will receive it after their turn. The card that was in the middle goes to your side to use on your next turn.
There are 2 colors, red and blue. The human player is always red in this implementation. Because the starting player is determined randomly, this has no effect on your chances of victory.
On your turn, you must make a move. Each card will be displayed with its moves highlighted. Enter moves in the format of [origin square] [destination square].
Some examples:
```
a5 b5
a5 to b5
a5 > b5
A5 B5
etc.
```
The game prints the board state every turn. Horizontal squares are separated by pipes. Each piece is represented as 2 letters; the second one is S for student or M for master.
If you want to view the cards you don't have access to and where they are, type `cards`.
### How to Win
* Capture your opponent's master
* Move your master to the starting square of your opponent's master

If you would like the regular expression that is checked against, it is `[a-e][1-5] ((>|(to)) ){0,1}[a-e][1-5]`. (Option: case-insensitive.)
### Card Diagram
```
dragon:
|   |   |   |   |   |
| # |   |   |   | # |
|   |   | @ |   |   |
|   | # |   | # |   |
|   |   |   |   |   |
```
Imagine this, but with more color, because dragon is a red card.
The `@` represents the initial position of the piece. Each `#` represents a possible destination square. Thus, a piece can move up 1, left 2; up 1, right 2; down 1, left 1; and down 1, right 1. See above for how you'd actually make those moves. Note that a piece can move through other pieces, though it cannot land on a piece of the same color.
## Project Setup
Download and extract the zip archive, leaving all files in their folders. The file for running the game will be called `Onitama V2`, but the exact details depend on the operating system. Because C# is a compiled language, the source files cannot be run directly. The file `CardNames.txt` should be in the same directory as the executable, but it can be elsewhere as long as you know the path.
## Implementation
### Minimax
The most important part of a game like this is the computer opponent. To find the best move for the computer to make, I used the minimax algorithm, the same algorithm that gets used in chess engines (mine isn't as good as Stockfish, of course).
## Code Notes
Because C# is strongly typed, it doesn't worry very much about type checking, and thus preconditions. If a value is of the wrong type, it will throw an exception on its own. Instead, the main concern is null references. Most values cannot be null, but some, such as ones dependent on user input, can be. The compiler gets very worried about null references, and so I have placed ? or ! after some expressions and used the ?? null conditional operator. See [here](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types) for more. As far as other oddities, I use several [TryParse](https://learn.microsoft.com/en-us/dotnet/api/system.int32.tryparse?view=net-7.0#system-int32-tryparse(system-string-system-int32@)) methods. If you're unfamiliar with out parameters, see [here](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out-parameter-modifier). I also use the [ternary conditional operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-operator). Overall, I have tried to add comments regarding how these are used, but the references are here if needed.
<sub>There are also a few tidbits of discussion on how cool C# is. This is because it is cool. I had a lot of fun writing this!</sub>