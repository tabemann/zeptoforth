# Lambda Words

In zeptoforth the user can define lightweight anonymous functions, known as lambdas, inline in other words with `[:` and `;]`, where `[:` starts compiling a lambda, and `;]` finishes it. compiling the `xt` for the lambda as a literal into the containing word. Multiple lambdas can be nested. Note that lambdas have no headers, making them lighter-weight than normal words. Provided that all words compiled into them are themselves inlined, they can be inlined into other words.

`[:` and `;]` are defined in `src/common/forth/basic.fs`, but the other words discussed here are defined in `src/common/forth/lambda.fs`.

### `lambda`

These words are in the `lambda` module`.

There are two simple conditional combinators, namely:

##### `qif`
( ??? flag true-xt -- ??? )

which takes *f* and *true-xt*, which is executed when *f* is non-zero; *true-xt* has the following signature:

( ??? -- ??? )

##### `qifelse`
( ??? flag true-xt false-xt -- ??? )

which takes *f*, *true-xt*, and *false-xt*; *true-xt* is executed when *f* is non-zero and *false-xt* is executed when *f* is zero. These have the following signature:

( ??? -- ??? )

There are three simple looping combinators:

##### `quntil`
( ??? xt -- ??? )

which takes *xt* and executes it repeatedly until it returns a non-zero value; *xt* has the following signature:

( ??? -- ??? flag )

##### `qagain`
( ??? xt -- ??? )

which takes *xt* and executes it repeatedly forever until an exception is raised; *xt* has the following signature:

( ??? -- ??? )

##### `qwhile`
( ??? while-xt body-xt -- ??? )

which in a loop first executes *while-xt* and, if it returns a non-zero value, then it executes *body-xt* and continues looping, else it exits the loop; *while-xt* has the signature:

( ??? -- ??? flag )

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
( ??? c-addr count xt -- ??? )

which iterates over the byte array specified by *c-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, which has the signature:

( ??? c -- ??? )

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

which iterates over the cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them to *iter-xt*; *get-xt* has the signature:

( ??? i -- ??? x )

and *xt* has the signature:

( ??? x -- ??? )

##### `2iter-get`
( ??? get-xt count iter-xt -- ??? )

which iterates over the double-cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them to *iter-xt*; *get-xt* has the signature:

( ??? i -- ??? d )

and *xt* has the signature:

( ??? d -- ??? )

There are the following combinators for finding indices of values in arrays:

##### `citeri`
( ??? c-addr count xt -- ??? )

which iterates over the byte array specified by *c-addr* and *count*, passing each byte from the lowest address to the highest along with its index to *xt*, which has the signature:

( ??? c i -- ??? )

##### `hiteri`
( ??? h-addr count xt -- ??? )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest along with its index to *xt*, which has the signature:

( ??? h i -- ??? )

##### `iteri`
( ??? addr count xt -- ??? )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest along with its index to *xt*, which as the signature:

( ??? x i -- ??? )

##### `2iteri`
( ??? addr count xt -- ??? )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest along with its index to *xt*, which as the signature:

( ??? d i -- ??? )

There are the following combinators for iterating over values from getters:

##### `iteri-get`
( ??? get-xt count iter-xt -- ??? )

which iterates over the cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them along with their index to *iter-xt*; *get-xt* has the signature:

( ??? i -- ??? x )

and *xt* has the signature:

( ??? x i -- ??? )

##### `2iteri-get`
( ??? get-xt count iter-xt -- ??? )

which iterates over the double-cell values returned by *get-xt* when passed an index starting from 0 up to but not including *count*, passing them along with their index to *iter-xt*; *get-xt* has the signature:

( ??? i -- ??? d )

and *xt* has the signature:

( ??? d i -- ??? )

There are the following combinators for mapping from one array to another array with different-sized members; note that two arrays may be the same array:

##### `cqmap`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one byte array *src-addr* to another byte array *dst-addr* with *count* elements using *xt*, where *xt* has the signature:

( ??? c -- ??? c' )

##### `hqmap`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one halfword array *src-addr* to another halfword array *dst-addr* with *count* elements using *xt*, where *xt* has the signature:

( ??? h -- ??? h' )

##### `qmap`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one cell array *src-addr* to another cell array *dst-addr* with *count* elements using *xt*, where *xt* has the signature:

( ??? x -- ??? x' )

##### `2qmap`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one double-cell array *src-addr* to another double-cell array *dst-addr* with *count* elements using *xt*, where *xt* has the signature:

( ??? d -- ??? d' )

##### `qmap-get-set`
( ??? get-xt count map-xt set-xt -- ??? )

which maps from *count* single-cell elements generated by the getter *get-xt* with *map-xt*, and then passing the resulting values and their indices to *set-xt*, where *get-xt* has the signature:

( ??? i -- ??? x )

and *map-xt* has the signature:

( ??? x -- ??? x'  )

and *set-xt* has the signature

( ??? x' i -- ??? )

##### `2qmap-get-set`
( ??? get-xt count map-xt set-xt -- ??? )

which maps from *count* double-cell elements generated by the getter *get-xt* with *map-xt*, and then passing the resulting values and their indices to *set-xt*, where *get-xt* has the signature:

( ??? i -- ??? d )

and *map-xt* has the signature:

( ??? d -- ??? d'  )

and *set-xt* has the signature

( ??? d' i -- ??? )

##### `cqmapi`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one byte array *src-addr* to another byte array *dst-addr* with *count* elements using *xt* with both each element and its index, where *xt* has the signature:

( ??? c i -- ??? c' )

##### `hqmapi`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one halfword array *src-addr* to another halfword array *dst-addr* with *count* elements using *xt* with both each element and its index, where *xt* has the signature:

( ??? h i -- ??? h' )

##### `qmapi`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one cell array *src-addr* to another cell array *dst-addr* with *count* elements using *xt* with both each element and its index, where *xt* has the signature:

( ??? x i -- ??? x' )

##### `2qmapi`
( ??? src-addr dst-addr count xt -- ??? )

which maps from one double-cell array *src-addr* to another double-cell array *dst-addr* with *count* elements using *xt* with both each element and its index, where *xt* has the signature:

( ??? d i -- ??? d' )

There are the following combinators for filtering from one array to another array with different-sized members; note that the two arrays may be the same array:

##### `qmapi-get-set`
( ??? get-xt count map-xt set-xt -- ??? )

which maps from *count* single-cell elements generated by the getter *get-xt* and their indices with *map-xt*, and then passing the resulting values and their indices to *set-xt*, where *get-xt* has the signature:

( ??? i -- ??? x )

and *map-xt* has the signature:

( ??? x i -- ??? x'  )

and *set-xt* has the signature

( ??? x' i -- ??? )

##### `2qmapi-get-set`
( ??? get-xt count map-xt set-xt -- ??? )

which maps from *count* double-cell elements generated by the getter *get-xt* and their indices with *map-xt*, and then passing the resulting values and their indices to *set-xt*, where *get-xt* has the signature:

( ??? i -- ??? d )

and *map-xt* has the signature:

( ??? d i -- ??? d'  )

and *set-xt* has the signature

( ??? d' i -- ??? )

##### `cfilter`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one byte array *src-addr* to another byte array *dst-addr* with *count* elements using *xt*, returning *count'* elements filtered, where *xt* has the signature:

( ??? c -- ??? flag )

##### `hfilter`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one halfword array *src-addr* to another halfword array *dst-addr* with *count* elements using *xt*, returning *count'* elements filtered, where *xt* has the signature:

( ??? h -- ??? flag )

##### `filter`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one cell array *src-addr* to another cell array *dst-addr* with *count* elements using *xt*, returning *count'* elements filtered, where *xt* has the signature:

( ??? x -- ??? flag )

##### `2filter`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one double-cell array *src-addr* to another double-cell array *dst-addr* with *count* elements using *xt*, returning *count'* elements filtered, where *xt* has the signature:

( ??? d -- ??? flag )

##### `filter-get-set`
( ??? get-xt count filter-xt set-xt -- ??? count' )

which filters *count* single-cell elements from getter *get-xt* with *filter-xt*, and passes the filtered values and their ultimate indices to *set-xt*, returnining *count'* elements filtered, where *get-xt* has the signature:

( ??? i -- ??? x )

and *filter-xt* has the signature

( ??? x -- ??? flag )

and *set-xt* has the signature

( ??? x i' -- ??? )

##### `2filter-get-set`
( ??? get-xt count filter-xt set-xt -- ??? count' )

which filters *count* double-cell elements from getter *get-xt* with *filter-xt*, and passes the filtered values and their ultimate indices to *set-xt*, returnining *count'* elements filtered, where *get-xt* has the signature:

( ??? i -- ??? d )

and *filter-xt* has the signature

( ??? d -- ??? flag )

and *set-xt* has the signature

( ??? d i' -- ??? )

##### `cfilteri`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one byte array *src-addr* to another byte array *dst-addr* with *count* elements using *xt* with both each element and its index, returning *count'* elements filtered, where *xt* has the signature:

( ??? c i -- ??? flag )

##### `hfilteri`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one halfword array *src-addr* to another halfword array *dst-addr* with *count* elements using *xt* with both each element and its index, returning *count'* elements filtered, where *xt* has the signature:

( ??? h i -- ??? flag )

##### `filteri`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one cell array *src-addr* to another cell array *dst-addr* with *count* elements using *xt* with both each element and its index, returning *count'* elements filtered, where *xt* has the signature:

( ??? x i -- ??? flag )

##### `2filteri`
( ??? src-addr dst-addr count xt -- ??? count' )

which filters from one double-cell array *src-addr* to another double-cell array *dst-addr* with *count* elements using *xt* with both each element and its index, returning *count'* elements filtered, where *xt* has the signature:

( ??? d i -- ??? flag )

##### `filteri-get-set`
( ??? get-xt count filter-xt set-xt -- ??? count' )

which filters *count* single-cell elements from getter *get-xt* along with their indices with *filter-xt*, and passes the filtered values and their ultimate indices to *set-xt*, returnining *count'* elements filtered, where *get-xt* has the signature:

( ??? i -- ??? x )

and *filter-xt* has the signature

( ??? x i -- ??? flag )

and *set-xt* has the signature

( ??? x i' -- ??? )

##### `2filteri-get-set`
( ??? get-xt count filter-xt set-xt -- ??? count' )

which filters *count* double-cell elements from getter *get-xt* along with their indices with *filter-xt*, and passes the filtered values and their ultimate indices to *set-xt*, returnining *count'* elements filtered, where *get-xt* has the signature:

( ??? i -- ??? d )

and *filter-xt* has the signature

( ??? d i -- ??? flag )

and *set-xt* has the signature

( ??? d i' -- ??? )

There are the following combinators for finding indices of values in arrays:

##### `cfind-index`
( ??? c-addr count xt -- ??? i|-1 )

which iterates over the byte array specified by *c-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that byte, or if it reaches the end of the array, where then it returns -1; *xt* as the signature:

( ??? c -- ??? flag )

##### `hfind-index`
( ??? h-addr count xt -- ??? i|-1 )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that halfword, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? h -- ??? flag )

##### `find-index`
( ??? addr count xt -- ??? i|-1 )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that cell, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? x -- ??? flag )

##### `2find-index`
( ??? addr count xt -- ??? i|-1 )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that double cell, or if it reaches the end of the array, where then it returns -1; *xt* has the signature:

( ??? d -- ??? flag )

There are the following combinators for finding indices of values from getters:

##### `find-get-index`
( ??? get-xt count pred-xt -- ??? i|-1 )

which iterates over the cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that cell, or if it reaches *count*, where then it returns -1; *get-xt* has the signature:

( ??? i -- ??? x )

and *pred-xt* has the signature:

( ??? x -- ??? flag )

##### `2find-get-index`
( ??? get-xt count pred-xt -- ??? i|-1 )

which iterates over the double-cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each double cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the index of that double cell, or if it reaches *count*, where then it returns -1; *get-xt* has the signature:

( ??? i -- ??? d )

and *pred-xt* has the signature:

( ??? d -- ??? flag )

There are the following combinators for finding values in arrays:

##### `cfind-value`
( ??? c-addr count xt -- ??? b|0 flag )

which iterates over the byte array specified by *c-addr* and *count*, passing each byte from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that byte and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* as the signature:

( ??? c -- ??? flag )

##### `hfind-value`
( ??? h-addr count xt -- ??? h|0 flag )

which iterates over the halfword array specified by *h-addr* and *count*, passing each halfword from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that halfword and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? h -- ??? flag )

##### `find-value`
( ??? addr count xt -- ??? x|0 flag )

which iterates over the cell array specified by *addr* and *count*, passing each cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that cell and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? x -- ??? flag )

##### `2find-value`
( ??? addr count xt -- ??? d|0 flag )

which iterates over the double-cell array specified by *addr* and *count*, passing each double cell from the lowest address to the highest to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that double cell and true, or if it reaches the end of the array, where then it returns 0 and false; *xt* has the signature:

( ??? d -- ??? flag )

There are the following combinators for finding values from getters:

##### `find-get-value`
( ??? get-xt count pred-xt -- ??? x|0 flag )

which iterates over the cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that cell and true, or if it reaches *count*, where then it returns 0 and false; *get-xt* has the signature:

( ??? i -- ??? x )

and *pred-xt* has the signature:

( ??? x -- ??? flag )

##### `2find-get-value`
( ??? get-xt count pred-xt -- ??? d|0 flag )

which iterates over the double-cell values returned by *get-xt* when passed indices starting from zero up to but not including *count*, passing each double cell to *xt*, until it either reaches a value for which it returns a non-zero value, where then it returns the value of that double cell and true, or if it reaches *count*, where then it returns 0 and false; *get-xt* has the signature:

( ??? i -- ??? d )

and *pred-xt* has the signature:

( ??? d -- ??? flag )
