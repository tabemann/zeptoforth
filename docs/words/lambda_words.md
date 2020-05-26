# Lambda Words

##### `option`
( f true-xt -- ) ( true-xt: ??? -- ??? )

Execute an xt based on whether a condition is true

##### `choose`
( f true-xt false-xt -- ) ( true-xt: ??? -- ??? ) ( false-xt: ??? -- ??? )

Execute one of two different xts based on whether a condition is true or false

##### `loop-until`
( ??? xt -- ??? ) ( xt: ??? -- ??? f )

Execute an until loop with an xt

##### `while-loop`
( ??? while-xt body-xt -- ??? ) ( while-xt: ??? -- ??? f )
( body-xt: ??? -- ??? )

Execute a while loop with a while-xt and a body-xt

##### `count-loop`
( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? )

Execute a counted loop with an xt

##### `count+loop`
( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? increment )

Execute a counted loop with an arbitrary increment with an xt

##### `biter`
( ??? b-addr count xt -- ??? ) ( xt: ??? b -- ??? )

Iterate executing an xt over a byte array

##### `hiter`
( ??? h-addr count xt -- ??? ) ( xt: ??? h -- ??? )

Iterate executing an xt over a halfword array

##### `iter`
( ??? addr count xt -- ??? ) ( xt: ??? x -- ??? )

Iterate executing an xt over a cell array

##### `2iter`
( ??? addr count xt -- ??? ) ( xt: ??? d -- ??? )

Iterate executing an xt over a double-word array

##### `iter-get`
( ??? get-xt count iter-xt -- ??? ) ( get-xt: ??? i -- ??? x )
( iter-xt: ??? x -- ??? )

Iterate executing at xt over values from a getter

##### `2iter-get`
( ??? get-xt count iter-xt -- ??? ) ( get-xt: ??? i -- ??? d ) ( iter-xt: ??? d -- ??? )

Iterate executing at xt over double-word values from a getter

##### `bfind-index`
( ??? b-addr count xt -- ??? i|-1 ) ( xt: ??? b -- ??? f )

Find the index of a value in a byte array with a predicate

##### `hfind-index`
( ??? h-addr count xt -- ??? i|-1 ) ( xt: ??? h -- ??? f )

Find the index of a value in a halfword array with a predicate

##### `find-index`
( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )

Find the index of a value in a cell array with a predicate

##### `2find-index`
( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? d -- ??? f )

Find the index of a value in a double-word array with a predicate

##### `find-get-index`
( ??? get-xt count pred-xt --- ??? i|-1 ) ( get-xt: ??? i -- ??? x )
( pred-xt: ??? x -- ??? f )

Find the index of a value from a getter with a predicate

##### `2find-get-index`
( ??? get-xt count pred-xt --- ??? i|-1 ) ( get-xt: ??? i -- ??? d )
( pred-xt: ??? d -- ??? f )

Find the index of a double-word value from a getter with a predicate

##### `bfind-value`
( ??? a-addr count xt -- ??? b|0 f ) ( xt: ??? b -- ??? f )

Find a value in a byte array with a predicate

##### `hfind-value`
( ??? a-addr count xt -- ??? h|0 f ) ( xt: ??? h -- ??? f )

Find a value in a halfword array with a predicate

##### `find-value`
( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )

Find a value in a cell array with a predicate

##### `2find-value`
( ??? a-addr count xt -- ??? d|0 f ) ( xt: ??? d -- ??? f )

Find a value in a double-word array with a predicate

##### `find-get-value`
( ???? get-xt count pred-xt --- ??? x|0 f ) ( get-xt: ??? i -- ??? x )
( pred-xt: ??? x -- ??? f )

Find a value from a getter with a predicate

##### `2find-get-value`
( ???? get-xt count pred-xt --- ??? d|0 f ) ( get-xt: ??? i -- ??? d )
( pred-xt: ??? d -- ??? f )

Find a double-word value from a getter with a predicate
