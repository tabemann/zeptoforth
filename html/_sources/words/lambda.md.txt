# Lambda Words

In zeptoforth the user can define lightweight anonymous functions, known as lambdas, inline in other words with `[:` and `;]`, where `[:` starts compiling a lambda, and `;]` finishes it. compiling the `xt` for the lambda as a literal into the containing word. Multiple lambdas can be nested. Note that lambdas have no headers, making them lighter-weight than normal words. Provided that all words compiled into them are themselves inlined, they can be inlined into other words.

`[:` and `;]` are defined in `src/common/forth/basic.fs`, but the other words discussed here are defined in `src/common/forth/lambda.fs`.

### `forth`

These words are in `forth`.

There are two simple conditional combinators, namely:

##### `qif`
( ??? f true-xt -- ??? )

which takes *f* and *true-xt*, which is executed when *f* is non-zero; *true-xt* has the following signature:

( ??? -- ??? )

##### `qifelse`
( ??? f true-xt false-xt -- ??? )

which takes *f*, *true-xt*, and *false-xt*; *true-xt* is executed when *f* is non-zero and *false-xt* is executed when *f* is zero. These have the following signature:

( ??? -- ??? )

There are three simple looping combinators:

##### `quntil`
( ??? xt -- ??? )

which takes *xt* and executes it repeatedly until it returns a non-zero value; *xt* has the following signature:

( ??? -- ??? f )

##### `qagain`
( ??? xt -- ??? )

which takes *xt* and executes it repeatedly forever until an exception is raised; *xt* has the following signature:

( ??? -- ??? )

##### `qwhile`
( ??? while-xt body-xt -- ??? )

which in a loop first executes *while-xt* and, if it returns a non-zero value, then it executes *body-xt* and continues looping, else it exits the loop; *while-xt* has the signature:

( ??? -- ??? f )

and *body-xt* has the signature:

( ??? -- ??? )

There are two counted looping combinators:

##### `qcount`
( ??? limit init xt -- ??? )

which counts up with an increment of one from *init* until it reaches *limit*, not including it, executing *xt* at each step, passing it the current count; *xt* has the signature:

( ??? i -- ??? )

##### `qcount+`
( ??? limit init xt -- ??? )

which counts up or down with a variable increment from *init* until it reaches *limit*, not including it for counting up but including it for counting down, executing *xt* at each step, passing it the current count and receiving the increment/decrement for the next step; *xt* has the signature:

( ??? i -- ??? increment )

There are the following combinators for iterating over arrays with with different-sized members:

##### `citer`
( ??? b-addr count xt -- ??? )

which iterates over the byte array specified by *b-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, which has the signature:

( ??? b -- ??? )

##### `hiter`
( ??? h-addr count xt -- ??? )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest to *xt*, which has the signature:

( ??? h -- ??? )

##### `iter`
( ??? addr count xt -- ??? )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest to *xt*, which as the signature:

( ??? x -- ??? )

##### `2iter`
( ??? addr count xt -- ??? )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest to *xt*, which as the signature:

( ??? d -- ??? )

There are the following combinators for iterating over values from getters:

##### `iter-get`
( ??? get-xt count iter-xt -- ??? )

which iterates over the cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them to *xt*; *get-xt* has the signature:

( ??? i -- ??? x )

and *xt* has the signature:

( ??? x -- ??? )

##### `2iter-get`
( ??? get-xt count iter-xt -- ??? )

which iterates over the double-cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them to *xt*; *get-xt* has the signature:

( ??? i -- ??? d )

and *xt* has the signature:

( ??? d -- ??? )

There are the following combinators for finding indices of values in arrays:

##### `cfind-index`
( ??? b-addr count xt -- ??? i|-1 )

which iterates over the byte array specified by *b-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that byte, or if it reaches the end of the array, where then it returns -1; *xt* as the signature:

( ??? b -- ??? f )

##### `hfind-index`
( ??? h-addr count xt -- ??? i|-1 )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that halfword, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? h -- ??? f )

##### `find-index`
( ??? addr count xt -- ??? i|-1 )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that cell, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? x -- ??? f )

##### `2find-index`
( ??? addr count xt -- ??? i|-1 )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that double cell, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? d -- ??? f )

There are the following combinators for finding indices of values from getters:

##### `find-get-index`
( ??? get-xt count pred-xt -- ??? i|-1 )

which iterates over the cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that cell, or if it reaches *count*, where then it returns -1; *get-xt* has the signature:

( ??? i -- ??? x )

and *pred-xt* has the signature:

( ??? x -- ??? f )

##### `2find-get-index`
( ??? get-xt count pred-xt -- ??? i|-1 )

which iterates over the double-cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each double cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that double cell, or if it reaches *count*, where then it returns -1; *get-xt* has the signature:

( ??? i -- ??? d )

and *pred-xt* has the signature:

( ??? d -- ??? f )

There are the following combinators for finding values in arrays:

##### `cfind-value`
( ??? b-addr count xt -- ??? b|0 f )

which iterates over the byte array specified by *b-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that byte and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* as the signature:

( ??? b -- ??? f )

##### `hfind-value`
( ??? h-addr count xt -- ??? h|0 f )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that halfword and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? h -- ??? f )

##### `find-value`
( ??? addr count xt -- ??? x|0 f )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that cell and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? x -- ??? f )

##### `2find-value`
( ??? addr count xt -- ??? d|0 f )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that double cell and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? d -- ??? f )

There are the following combinators for finding values from getters:

##### `find-get-value`
( ??? get-xt count pred-xt -- ??? x|0 f )

which iterates over the cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that cell and true, or if it reaches *count*, where then it returns 0 and false; *get-xt* has the signature:

( ??? i -- ??? x )

and *pred-xt* has the signature:

( ??? x -- ??? f )

##### `2find-get-value`
( ??? get-xt count pred-xt -- ??? d|0 f )

which iterates over the double-cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each double cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that double cell and true, or if it reaches *count*, where then it returns 0 and false; *get-xt* has the signature:

( ??? i -- ??? d )

and *pred-xt* has the signature:

( ??? d -- ??? f )
