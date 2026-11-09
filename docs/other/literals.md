# Literals

Traditionally, Forth has only numeric literals, very often with support for base prefixes on integral values. zeptoforth has these, with the following base prefixes supported:

* `$` for base 16
* `#` for base 10
* `/` for base 8
* `%` for base 2
* And a non-standard `0x` for base 16

Of the various kinds of numeric literals it supports, it supports:

* Single-cell integral literals, of the form `[<prefix>]<integer>`
* Double-cell integral literals, of the form `[<prefix>]<integer>.` with support for `.` to be elsewhere in the literal after the prefix (but this is not recommended as it may be confusing)
* S31.32 fixed-point literals, of the form `[<prefix>]<integral>,<fractional>`; do note that the decimal point is a comma, not a period/full stop
* If the module `fixed32` is loaded, S15.16 fixed-point literals, of the form `[<prefix>]<integral>;<fractional>`; do note that the decimal point is a semicolon
* On platforms/builds with hardware single-precision floating-point support (i.e. currently all non-mini, non-RP2040 platforms/builds), single-precision floating point literals, of the form `<integral>[.<fractional>]e<exponent>`
* On platforms/builds with hardware double-precision floating-point support (currently only zeptoforth for the Colour MaxiMite 2 (CMM2)), double-precision floating point literals, of the form `<integral>[.<fractional>]de<exponent>`

In other cases, Forths have traditionally used words such as `s"`, `c"`, `."`, `s\"`, `c\"`, `.\"`, `'`, `[']`, `char`, and `[char]` to prefix values in the place of literals.

However, additionally, as of release 1.17.2, zeptoforth has a number of other kinds of literals that cover these cases, including:

* String literals, equivalent to the use of `s\"` except without an extra separating whitespace, of the form `"<string>"`; these are specifically *addr* *bytes* string literals, and there is no equivalent literals for counted strings
* Execution token literals, equivalent to the use of `'` in interpretation state and `[']` in compilation state except without an extra separating whitespace, of the form `` `<name> ``
* Name token literals, of the form ``` ``<name> ```; these push or compile literals, depending on state, for the headers of the named words
* Character literals, equivalent to the use of `char` in interpretation state and `[char]` in compilation state except without an extra separating whitespace, of the form `'<character>[']`; note that these do support UTF-8, which is evaluated to a Unicode code point

Also note that, as of release 1.17.2, `char` and `[char]` have been extended to support UTF-8, which is evaluated to a Unicode code point.
