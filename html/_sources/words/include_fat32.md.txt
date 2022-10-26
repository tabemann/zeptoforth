# FAT32 Code Including Support Words

zeptoforth includes support for including code for execution within FAT32 filesystems. This includes support for handling nested included files, up to a maximum of eight included files. Note that including code is intended to only be done from within the main task, and undefined results may occur if done from within any other task.

### `include-fat32`

##### `x-include-fs-not-set`
( -- )

Include filesystem not set exception.

##### `x-include-stack-overflow`
( -- )

Include stack overflow exception, raised if the number of nested includes exceeds eight includes.

##### `include-fs!`
( fs -- )

Set the include FAT32 filesystem. This filesystem is a subclass of `<base-fat32-fs>` in the `fat32` module.

##### `include-fs@`
( -- fs )

Get the include FAT32 filesystem. This filesystem is a subclass of `<base-fat32-fs>` in the `fat32` module.

##### `load-file`
( file -- )

Load code from a file in the FAT32 filesystem. Note that the file object will be duplicated in the process.

##### `included`
( c-addr u -- )

Load code from a file with the specified path in the current include FAT32 filesystem.

##### `include`
( "path" -- )

Load code from a file with the specified path as a token in the current include FAT32 filesystem.
