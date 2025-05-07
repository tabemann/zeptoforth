# Console Redirection Words

These words concern redirecting the current console within a given task (each task has its own console configuration, which it inherits upon initialization from its parent task) within a given scope defined by an execution token. Console input and output may be independently redirected, including to arbitrary words called by `key` and `key?` for input to `emit` and `emit?` for output, to null inputs and outputs, to the standard serial console, or to streams. Also, error output can be redirected in the same fashion as normal output.

Take care when redirecting the console to streams because the typical dictionary size used for tasks of 320 is insufficient for this due to buffer space that is alloted in the current task's dictionary; rather, a dictionary size of 512 has found to work.

One should also take care, particularly when working with multiple tasks where one task inherits its console redirection from a parent task, to ensure that the redirected console is not accessed after the scope defined by the `with-*input`, `with-*output`, or `with-*error-output` word is defined. However, with the exceptions of `with-null-input`, `with-null-output`, `with-null-error-output`, `with-serial-input`, `with-serial-output`, `with-serial-error-output`, and `with-swd-input`, `with-swd-output`, and `with-swd-error-output`, accessing the redirected console is safe even after its original specification goes out of scope.

### `console`

The following words are in the `console` module:

##### `with-input`
( input-hook input?-hook xt -- )

Call *xt* with `key` redirected to call *input-hook* and `key?` redirected to call *input?-hook*. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-output`
( output-hook output?-hook flush-console-hook xt -- )

Call *xt* with `emit` redirected to call *output-hook*, `emit?` redirected to call *output?-hook*, and `flush-console` redirected to call *flush-console-hook*. The previous redirection of `emit`, `emit?`, and `flush-console` is restored after *xt* returns or an exception is raised.

##### `with-error-output`
( error-output-hook error-output?-hook error-flush-console-hook xt -- )

Call *xt* with `emit` redirected to call *error-output-hook*, `emit?` redirected to call *error-output?-hook*, and `flush-console` redirected to call *error-flush-console-hook* for error output. The previous redirection of `emit`, `emit?`, and `flush-console` for error output is restored after *xt* returns or an exception is raised.

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

### `fat32`

The following words are in the `fat32` module and pertain to direct access to instances of `fat32::<fat32-file>`. Note that there are identically named words in the `fat32-tools` module which pertain to files in the current FAT32 filesystem which are accessed by path.

##### `with-file-input`
( file xt -- )

Call *xt* with `key` and `key?` redirected for input from file *file*. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-file-output`
( file xt -- )

Call *xt* with `emit` and `emit?` redirected for output to file *file*. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-file-error-output`
( file xt -- )

Call *xt* with `emit` and `emit?` redirected for output to file *file* for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

### `fat32-tools`

The following words are in the `fat32-tools` module and pertain to access to files in the current FAT32 filesystem by path. Note that there are identically named words in the `fat32` module, listed above, which pertain to direct access to instances of `fat32::<fat32-file>`.

##### `with-file-input`
( path-addr path-u xt -- )

Call *xt* with `key` and `key?` redirected for input from the file at the specified path. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-file-output`
( path-addr path-u xt -- )

Call *xt* with `emit` and `emit?` redirected for output to the file at the specified path. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-file-error-output`
( path-addr path-u xt -- )

Call *xt* with `emit` and `emit?` redirected for output to the file at the specified path for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

### `uart`

The following words are in the `uart` module:

##### `uart-console`
( uart -- )

Set the current task's console to a be a UART. Child tasks will inherit this console.


##### `with-uart-input`
( uart xt -- )

Call *xt* with `key` and `key?` redirected for U(S)ART console input for U(S)ART *uart*. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-uart-output`
( uart xt -- )

Call *xt* with `emit` and `emit?` redirected for U(S)ART console output for U(S)ART *uart*. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-uart-error-output`
( uart xt -- )

Call *xt* with `emit` and `emit?` redirected for U(S)ART console output for error output for U(S)ART *uart*. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

### `usb`

The following words are in the `usb` module; note that they are only available for `full_usb` builds:

##### `usb-console`
( -- )

Set the console to use USB.

##### `with-usb-input`
( xt -- )

Call *xt* with `key` and `key?` redirected for USB console input. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-usb-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for USB console output. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-usb-error-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for USB console output for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

##### `usb-special-enabled`
( -- addr )

Get the variable address of the USB console special character (control-C and control-T) handling enabled flag. This flag is -1 (enabled) by default;

### `swd`

The following words are in the `swd` module; note that they are only available for `full_swdcom` and `mini_swdcom` builds:

##### `swd-console`
( -- )

Set the console to use `swdcom`.

##### `enable-sleep`
( -- )

Set the console to sleep the CPU while waiting for input or output with
`swdcom` and no other tasks are active.

##### `disable-sleep`
( -- )

Set the console to not sleep the CPU while waiting for input or output with
`swdcom` and no other tasks are active.

##### `with-swd-input`
( xt -- )

Call *xt* with `key` and `key?` redirected for swdcom console input. The previous redirection of `key` and `key?` is restored after *xt* returns or an exception is raised.

##### `with-swd-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for swdcom console output. The previous redirection of `emit` and `emit?` is restored after *xt* returns or an exception is raised.

##### `with-swd-error-output`
( xt -- )

Call *xt* with `emit` and `emit?` redirected for swdcom console output for error output. The previous redirection of `emit` and `emit?` for error output is restored after *xt* returns or an exception is raised.

### `forth`

These words are in the `forth` module:

##### `uart-special-enabled`
( -- addr )

Get the variable address of the UART console special character (control-C and control-T) handling enabled flag. This flag is -1 (enabled) by default;

### `serial`

These words are in the `serial` module:

##### `serial-console`

Set the current task's console to use the interrupt-driven serial console.

##### `enable-serial-int-io`

Enable interrupt-driven serial IO (enabled by default).

##### `disable-serail-int-io`

Disable interrupt-driven serial IO.

##### `enable-int-io`

A deprecated name retained for compatibility's sake for `enable-serial-int-io`. This will be removed at a future date.

##### `disable-int-io`

A deprecated name retained for compatibility's sake for `disable-serial-int-io`. This will be removed at a future date.

### `int-io`

A deprecated name retained for compatibility's sake for `serial`. This will be removed at a future date.
