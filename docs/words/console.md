# Console Redirection Words

These words concern redirecting the current console within a given task (each task has its own console configuration, which it inherits upon initialization from its parent task) within a given scope defined by an execution token. Console input and output may be independently redirected, including to arbitrary words called by `key` and `key?` for input to `emit` and `emit?` for output, to null inputs and outputs, to the standard serial console, or to streams. Also, error output can be redirected in the same fashion as normal output.

Take care when redirecting the console to streams because the typical dictionary size used for tasks of 320 is insufficient for this due to buffer space that is alloted in the current task's dictionary; rather, a dictionary size of 512 has found to work.

### `console`

The following words are in the `console` module:

##### `with-input`
( input-hook input?-hook xt -- )

Call *xt* with `key` redirected to call *input-hook* and `key?` redirected to call *input?-hook*. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-output`
( output-hook output?-hook xt -- )

Call *xt* with `emit` redirected to call *output-hook* and `emit?` redirected to call *output?-hook*. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-error-output`
( error-output-hook error-output?-hook xt -- )

Call *xt* with `emit` redirected to call *output-hook* and `emit?` redirected to call *output?-hook* for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

##### `with-null-input`
( xt -- )

Call *xt* with `key` redirected to return 0 and `key?` redirected to return `false`. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-null-output`
( xt -- )

Call *xt* with `emit` redirected to call `drop` and `emit?` redirected to return `true`. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-null-error-output`
( xt -- )

Call *xt* with `emit` redirected to call `drop` and `emit?` redirected to return `true` for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

##### `with-serial-input`
( xt -- )

Call *xt* with `key` and `key?` redirected for serial console input. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-serial-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for serial console output. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-serial-error-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for serial console output for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

##### `with-stream-input`
( stream xt -- )

Call *xt* with `key` and `key?` redirected for input from stream *stream*. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-stream-output`
( stream xt -- )

Call *xt* with `emit` and `emit?` redirected for output to stream *stream*. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-stream-error-output`
( stream xt -- )

Call *xt* with `emit` and `emit?` redirected for output to stream *stream* for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

##### `with-output-as-error-output`
( xt -- )

Call *xt* with error output redirected to the current output. The previous redirection of error output is restored after *xt* returns or an exception is raised.