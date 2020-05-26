# Testing Words

## Stack Testing

Stack testing involves checking the stack pointer before and after the code being tested is executed, and testing for the change in the stack pointer as well as matching the values put on the stack by the code being tested against the values expected. By its very nature this code is applied a task-specific level but can be used in different tasks simultaneously. This code is not part of the prebuilt binaries, but rather is in `test/common/stack.fs` and may be used both compiled to flash and compiled to RAM.

##### `stack-fail-hook`

`stack-fail-hook` specifies an xt to be executed, if set to a value other than 0, when a stack test fails. By default this is set to 0.

##### `x-stack-fail`
( -- )

`x-stack-fail` is an exception raised when `}t` detects a stack position mismatch relative to the stack pointer recorded by `t{` and the number of cells on the stack passed into it. The purpose of this exception is that the stack pointer is unknown, so the only way to recover from it is by raising an exception.

##### `t{`
( -- ) ( R: -- sp )

This word saves the current stack pointer on the return stack for it to be used later by `}t`.

##### `}t`
( x1 ... xn y1 ... yn u -- ) ( R: old-sp -- )

This word checks that the current stack position is two times *u* cells greater than the old stack position recorded on the return stack; if this is not the case the xt stored in `stack-fail-hook` is executed, if it is not 0, and then `x-stack-fail` is raised. If this is the case each value *xi* is matched against its corresponding value *yi*, if any one of these does not match, then the xt stored in `stack-fail-hook` is executed if it is not 0.

## Emit Capture Testing

Emit capture testing involves installing a hook into `emit` transparently, and using this to capture and test each byte emitted against specified rules without interfering with the operation of the system. This is done in a transparently multitasking-friendly manner, and can be used to test behavior spread across multiple tasks without any extra work being needed on the programmer's part. This code is not part of the prebuilt binaries, but rather is in `test/common/capture.fs` and may be used both compiled to flash and compiled to RAM.

##### `capture-fail-hook`

`capture-fail-hook` specifies an xt to be executed, if set to a value other than 0, when an emit capture test fails. By default this is set to 0.

##### `enable-capture`
( -- )

This word enables emit capture testing.

##### `disable-capture`
( -- )

This word disables emit capture testing.

##### `no-capture`
( xt -- )

This word temporarily disables emit capture testing, and then executes *xt*, afterwards enabling emit capture testing if it had been previously enabled.

##### `x-capture-full`
( -- )

This word is the exception raised when the emit capture test buffer is full and the user attempts to add another emit capture test.

##### `clear-capture`
( -- )

This word clears the emit capture test buffer, effectively disabling emit capture test until another emit capture test is added.

##### `add-match-capture`
( b-addr bytes xt -- )

This word adds a matching emit capture test, which fails unless each character in the specified string is matched in order. *xt* is an xt which is executed when either the matching emit capture test fails, where then it is passed `false`, or when the matching emit capture test is successfully completed, where then it is passed `true`, unless it is set to 0, where then it is ignored.

##### `add-skip-capture`
( b-addr bytes limit xt -- )

This word adds a skipping emit capture test, which fails if more than *limit* characters are emitted without the specified string having been matched in full. *xt* is an xt which is executed when either the skipping emit capture test fails, where then it is passed `false`, or when the skipping emit capture test is successfully completed, where then it is passed `true`, unless it is set to 0, where then it is ignored.

##### `add-ignore-capture`
( b-addr bytes limit xt -- )

This word adds an ignoring emit capture test, which fails if more than *limit* characters are emitted without having matched the specified string fully 0 or more times. *xt* is an xt which is executed when either the ignoring emit capture test fails, where then it is passed `false`, or when the ignoring emit capture test is successfully completed, where then it is passed `true`, unless it is set to 0, where then it is ignored.
