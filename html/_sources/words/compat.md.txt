# ANS Forth / Forth 2012 Compatibility Words

This is a collection of assorted words added for compatibility with ANS Forth and Forth 2012. Because there may be conflicts with existing words, particularly in the case of `find`, these words are placed in a `compat` module from which they may be imported.

One important note is that when this is done within the default module, the word `find` provided by this module will be shadowed by the existing `find` word in the `forth` module; in this case one will have to either refer to it as `compat::find` or shadow the `find` in the `forth` module with:

```
: find compat::find ;
```

### `compat`

The `compat` module contains the following words:

##### `word`
( delim "<delims>word<delim>" -- c-addr )

Parse a toke ndelimited by a given character; note that this is not reentrant because the returned counted string is stored in a single global buffer; for new code `token` / `parse-name` is recommended when possible. Also, this word does not properly handle all sorts of whitespace, such as tabs and values less than $20.

##### `parse`
( delim "text<delim>" -- c-addr u )

Parse text up to a given character; the the returned string is in the input buffer and thus avoids the reentrancy problems of `word`.

##### `find`
( c-addr -- c-addr 0 | xt 1 | xt -1 )

Find a word's xt using a counted string for its name and whether it is immediate (signaled by 1) or non-immediate (signaled by -1); return the name as a counted string and 0 if it is not found.

##### `cmove`
( c-addr1 c-addr2 u -- )

Implement the traditional Forth string copying word `cmove` - for new code using `move` is recommended.

##### `cmove>`
( c-add1 c-addr2 u -- )

Implement the traditional Forth string copying word `cmove>` - for new code using `move` is recommended.

##### `within`
( test low high -- flag )

Determine whether a value is between 'low', inclusive, and 'high', exclusive.

##### `>number`
( D: acc c-addr u -- acc' c-addr' u' )

Parse a number in a string 'c-addr u' with an accumulator initialized as a double-cell value 'acc' using the base stored in BASE

##### `compare`
( c-addr1 u1 c-addr2 u2 -- n )

Compare two strings for both content and length using the numeric values of bytes compared within and shorter common length.

##### `erase`
( c-addr u -- )

Fill a buffer with zero bytes.

##### `align`
( -- )

Align the current `here` pointer to the next closest cell.

##### `aligned`
( a-addr -- a-addr' )

Align an address to the next closest cell.

##### `char+`
( c-addr -- c-addr' )

Increment an address by the size of one character, i.e. one byte.

##### `chars`
( n -- n' )

Get the size of *n* characters in bytes; this is a no-op.

##### `parse-name`
( "token" -- c-addr u )

Parse a single token from the input.

##### `.r`
( n width -- )

Output a right-justified signed value in a specified field width; note that if the value is wider than the specified field width the whole value will be output but no padding spaces will be added.

##### `u.r`
( u width -- )

Output a right-justified unsigned value in a specified field width; note that if the value is wider than the specified field width the whole value will be output but no padding spaces will be added.

##### `holds`
( c-addr u -- )

Add multiple characters to <# # #> numeric formatting.

##### `n>r`
( xn .. x1 N -- ; R: -- x1 .. xn n )

Transfer N items and count to the return stack.

##### `nr>`
( -- xn .. x1 N ; R: x1 .. xn N -- )

Pull N items and count off the return stack.

##### `x-invalid-input-spec`
( -- )

Invalid input specification exception.

##### `save-input`
( -- xn ... x1 n )

Save input specification.

##### `restore-input`
( xn ... x1 n -- )

Restore input specification.

##### `refill`
( -- flag )

Refill the input buffer (and return whether EOF has not been reached)

##### `sm/rem`
( d n -- rem quot )

Symmetric division.

##### `fm/mod`
( d n -- rem quot )

Floored division.

##### `unused`
( -- u )

Get the amount of remainign dictionary space in the current task's RAM dictionary

##### `abort`
( -- )

Raise an exception `x-abort`.

##### `abort"`
( "message" -- ) ( Runtime: flag -- )

Raise an exception that displays a message and a following newline if the value on the stack at runtime is non-zero.

##### `catch`
( xt -- except|0 )

Catch an exception; a synonym for `try`.

##### `throw`
( except -- )

Throw an exception, converting standard exceptions to zeptoforth exceptions. Note that -2 is not handled in a standard way because there is no fixed message buffer for `abort"`.

##### `x-abort`
( -- )

An exception which displays a message `aborted`.

##### `x-unknown`
( -- )

An unknown exception, corresponding to exception numbers < 0 which do not have standard meanings.

##### `?`
( addr -- )

Fetch a value from an address and print it as an integer.

##### `also`
( -- )

Duplicate the first entry on the search order.

##### `definitions`
( -- )

Make the compilation wordlist the same as the first entry on the search order.

##### `forth-wordlist`
( -- wid )

A synonym for zeptoforth FORTH.

##### `forth`
( -- )

Set the topmost wordlist with the FORTH wordlist.

##### `only`
( -- )

Set the wordlist order to the minimum default wordlist order.

##### `order`
( -- )

Display the searchlist order.

##### `previous`
( -- ) get-order ?dup if nip 1- set-order then ;

Remove the topmost entry of the wordlist order.

##### `search-wordlist`
( c-addr u wid -- 0 | xt 1 | xt -1 )

Find a word in a wordlist's xt using a string for its name and whether it is immediate (signaled by 1) or non-immediate (signaled by -1); return 0 if it is not found. Unlike ANS FIND it does not used a counted string and does not return the string being searched for if no word is found.
