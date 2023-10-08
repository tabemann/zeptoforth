# ANS Forth / Forth 2012 Compatibility Words

This is a collection of assorted words added for compatibility with ANS Forth and Forth 2012. Because there may be conflicts with existing words, particularly in the case of `find`, these words are placed in a `compat` module from which they may be imported.

One important note is that when this is done within the default module, the word `find` provided by this module will be shadowed by the existing `find` word in the `forth` module; in this case one will have to either refer to it as `compat::find` or shadow the `find` in the `forth` module with:

```
: find compat::find ;
```

### `compat`

The `compat` module contains the following words:

##### `word`
( delim "<delims>word<delims>" -- c-addr )

Parse a word delimited by a given character; note that this is not reentrant because the returned counted string is stored in a single global buffer; for new code TOKEN / PARSE-NAME is recommended when possible. Also, this word does not properly handle all sorts of whitespace, such as tabs and values less than $20.

##### `find`
( c-addr -- c-addr 0 | xt 1 | xt -1 )

Find a word's xt and whether it is immediate (signaled by 1) or non-immediate (signaled by 0)

##### `cmove`
( c-addr1 c-addr2 u -- )

Implement the traditional Forth string copying word CMOVE - for new code using MOVE is recommended.

##### `cmove>`
( c-add1 c-addr2 u -- )

Implement the traditional Forth string copying word CMOVE> - for new code using MOVE is recommended.

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

##### `parse-name`
( "token" -- c-addr u )

Parse a single token from the input.

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

##### `abort`
( -- )

Raise an exception `x-abort`.

##### `abort"`
( "message" -- ) ( Runtime: flag -- )

Raise an exception that displays a message and a following newline if the value on the stack at runtime is non-zero.

##### `x-abort`
( -- )

An exception which displays a message `aborted`.

##### `?`
( addr -- )

Fetch a value from an address and print it as an integer.