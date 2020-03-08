# ProfanityFilter
A near-direct port of the Python library [snguyenthanh/better_profanity](https://github.com/snguyenthanh/better_profanity) to C# (.NET Standard 2.1).

## Requirements
This works with .NET Standard 2.1.

## Installation
Clone the repository and build the `ProfanityFilter.Core` project.

## Usage
Create a new `Profanity` object, with an optional `dataDir` as the first parameter to specify the location of your [profanity_wordlist.txt](./ProfanityFilter.Core/profanity_wordlist.txt) and [alphabetic_unicode.json](./ProfanityFilter.Core/alphabetic_unicode.json):
```csharp
var profanity = new Profanity($"C:\Users\Raymonf\source\repos\ProfanityFilter\ProfanityFilter.Core");
```
You may want to call `Profanity.LoadCensorWords()` at initialization, or it'll take a little bit when you call `Profanity.Censor()`:
```csharp
profanity.LoadCensorWords();
```
But we're actually already up and running. Take a look:
```csharp
profanity.Censor("fuck off you literal poophead!!!!"); // "**** off you literal poophead!!!!"
```

**Note:** This is basically completely stolen from the original [snguyenthanh/better_profanity](https://github.com/snguyenthanh/better_profanity)'s [README.md](https://github.com/snguyenthanh/better_profanity/edit/master/README.md): All modified spellings of words in [profanity_wordlist.txt](./ProfanityFilter.Core/profanity_wordlist.txt) will be generated. For example, the word `handjob` would be loaded into:
```
'handjob', 'handj*b', 'handj0b', 'handj@b', 'h@ndjob', 'h@ndj*b', 'h@ndj0b', 'h@ndj@b',
'h*ndjob', 'h*ndj*b', 'h*ndj0b', 'h*ndj@b', 'h4ndjob', 'h4ndj*b', 'h4ndj0b', 'h4ndj@b'
```
The full mapping of the library can be found in [Profanity.cs](./ProfanityFilter.Core/Profanity.cs#L22-L34).

## Quirks
The ported library is less than efficient. There are `char.ToString()` calls sprinkled everywhere. Best case scenario: They don't exist. Worst case scenario: They exist. And they exist.

# Caveats inherited from better_profanity
**Note:** This section is basically completely stolen from the original [README.md](https://github.com/snguyenthanh/better_profanity/edit/master/README.md) of [snguyenthanh/better_profanity](https://github.com/snguyenthanh/better_profanity).


## Unicode characters

Only Unicode characters from categories `Ll`, `Lu`, `Mc` and `Mn` are added. More on Unicode categories can be found [here][unicode category link].

[unicode category link]: https://en.wikipedia.org/wiki/Template:General_Category_(Unicode)

Not all languages are supported yet, such as *Chinese*.

## Wordlist
Most of the words in the default [wordlist](./ProfanityFilter.Core/profanity_wordlist.txt) are referred from [Full List of Bad Words and Top Swear Words Banned by Google](https://github.com/RobertJGabriel/Google-profanity-words).

The wordlist contains a total of __106,992 words__, including 317 words from the default [profanity_wordlist.txt](./ProfanityFilter.Core/profanity_wordlist.txt) and their variants by modified spellings.

## Limitations

1. As the library compares each word by characters, the censor could easily be bypassed by adding any character(s) to the word:

```csharp
var profanity = new Profanity();

profanity.censor("I just have sexx");
// returns 'I just have sexx'

profanity.censor("jerkk off");
// returns 'jerkk off'
```

2. Any word in [wordlist](https://github.com/snguyenthanh/better_profanity/blob/master/better_profanity/profanity_wordlist.txt) that have non-space separators cannot be recognised, such as `s & m`, and therefore, it won't be filtered out. This problem was raised in [#5](https://github.com/snguyenthanh/better_profanity/issues/5).
