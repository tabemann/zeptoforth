# Glob

Globs enable matching over multiple files and directories, in nested directories, using wildcards. The matched file and directory paths are passed to a given execution token.

There are two wildcards, `?`, which matches any single character, and `*`, which matches any number of characters in a path element. Also, if the last character of a glob is `/`, only directories will be matched (the `/` will be included in the paths passed to the execution token).

By default `*` will match all files and all directories in a given directory; if one only wants to match files one must specify a glob containing `.`.

Note that `.` and `..` are never matched with wildcards, even though they can exist in glob patterns.

There is a limit of 256 bytes in any given matched path; paths longer than this will be ignored.

`glob` uses the current task's RAM dictionary for scratchpad space, so it cannot be used with words such as `included` which add additional data to the current task's RAM dictionary. It can be used with words that only temporarily use the current task's RAM dictionary and which restore `ram-here` to its original value upon completion or which only modify existing `variable`s, `value`s, `buffer:`s, and like already compiled into the RAM dictionary.

This functionality is included in `extra/common/glob.fs`.

### `fat32-tools`

The following word is in the `fat32-tools` module:

##### `glob`
( addr bytes xt -- )

Match any number of file or directory pathes with a glob defined by the string *addr* *bytes* and pass each path to the execution token *xt*.
