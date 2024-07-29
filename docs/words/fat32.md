# FAT32 Support

zeptoforth includes FAT32 filesystem support combined with MBR partition table support on devices implementing the `<block-dev>` class defined in the module `block-dev` (currently only the `<sd>` class defined in the module `sd`). It supports creating files and directories, reading files and directories, writing files and (indirectly) directories, seeking in files, removing files and (empty) directories, and renaming (but currently not moving) files and directories. It also supports parsing paths within filesystems. It supports reading partition table entries from the MBR, and uses these when initializing FAT32 filesystems.

Note that, prior to zeptoforth 1.7.0, files and directories did not need closing; it was merely up to the user to not carry out operations such as removing them and then carrying out other operations on them afterwards. This has changed; now files need to be closed with `close-file` and directories need to be closed with `close-dir`, followed by being destroyed with `destroy`, once one is done with them. Not doing so may result in undefined behavior, particularly if the space they occupied in RAM is reused afterwards.

### `fat32`

The `fat32` module contains the following words:

##### `x-sector-size-not-supported`
( -- )

Sector size exception.
  
##### `x-fs-version-not-supported`
( -- )

Filesystem version not supported exception.
  
##### `x-bad-info-sector`
( -- )

Bad info sector exception.
  
##### `x-no-clusters-free`
( -- )

No clusters free exception.
  
##### `x-file-name-format`
( -- )

Unsupported file name format exception.
  
##### `x-out-of-range-entry`
( -- )

Out of range directory entry index exception.
  
##### `x-out-of-range-partition`
( -- )

Out of range partition index exception.
  
##### `x-entry-not-found`
( -- )

Directory entry not found exception.
  
##### `x-entry-already-exists`
( -- )

Directory entry already exists exception.
  
##### `x-entry-not-file`
( -- )

Directory entry is not a file exception.
  
##### `x-entry-not-dir`
( -- )

Directory entry is not a directory exception.
  
##### `x-dir-is-not-empty`
( -- )

Directory is not empty exception.
  
##### `x-forbidden-dir`
( -- )

Directory name being changed or set is forbidden exception.
  
##### `x-empty-path`
( -- )

No file or directory referred to in path within directory exception.
  
##### `x-invalid-path`
( -- )

Invalid path exception.

##### `x-not-open`
( -- )

Attempted to carry out an operation on a file or directory that is not open.

##### `x-shared-file`
( -- )

Attempted to carry out an operation on a file where the file must only be open once (i.e. truncate a file).

##### `x-open`
( -- )

Attempted to carry out an operation on a file or directory that is open (i.e. remove a file or directory).

##### `seek-set`
( -- whence )

Seek from the beginning of a file.

##### `seek-cur`
( -- whence )

Seek from the current position in a file

##### `seek-end`
( -- whence )

##### `<mbr>`

The master boot record class. This class is used to read a partition entry from for initializing a FAT32 filesystem.

The `<mbr>` class includes the following constructor:

##### `new`
( mbr-device mbr -- )

Construct an `<mbr>` instance with the block device *mbr-device*.

The `<mbr>` class includes the following methods:

##### `mbr-valid?`
( mbr -- valid? )

Is the MBR valid?

##### `partition@`
( partition index mbr -- )

Read a partition entry, with an index from 0 to 3.

##### `partition!`
( partition index mbr -- )

Write a partition entry, with an index from 0 to 3.

##### `<partition>`

The master boot record partition entry class. This class is used to read partition entries from the MBR partition table for initializing FAT32 filesystems.

The `<partition>` class includes the following members:

##### `partition-active`
( partition -- partition-active )

Partition active state.
    
##### `partition-type`
( partition -- partition-type )

Partition type.
    
##### `partition-first-sector`
( partition -- partition-first-sector )

Partition first sector index.
    
##### `partition-sectors`
( partition -- partition-sectors )

Partition sectors.
    
##### `partition-active?`
( partition -- active? )

Is the partition active?

##### `<base-fat32-fs>`
( -- class )

The base FAT32 filesystem class. This class is not to be instantiated by itself.

The `<base-fat32-fs>` class includes the following methods, which are implemented by its subclasses:

##### `root-dir@`
( dir fs -- )

Initialize a root directory of a FAT32 filesystem; the directory object need not be initialized already, but if it is no harm will result.

##### `with-root-path`
( c-addr u xt fs -- ) ( xt: c-addr' u' dir -- )

Parse a path starting at the root directory of a FAT32 filesystem, and pass the leaf's name along with a directory object containing that leaf (or which would contain said leaf if it did not exist already) to the passed in *xt*. Note that said directory object will be destroyed when *xt* returns. The passed in directory object will be closed when the called *xt* returns.

##### `root-path-exists?`
( c-addr u fs -- exists? )

Get whether a file or directory exists at the specified path starting at the root directory of a FAT32 filesystem.

##### `with-create-file-at-root-path`
( c-addr u xt fs -- ) ( xt: file -- )

Parse a path starting at the root directory of a FAT32 filesystem, and attempt to create a file in the leaf directory; if successful a file object for that file is passed to *xt*. The passed in file object will be closed when the called *xt* returns.

##### `with-open-file-at-root-path`
( c-addr u xt fs -- ) ( xt: file -- )

Parse a path starting at the root directory of a FAT32 filesystem, and attempt to open a file in the leaf directory; if successful a file object for that file is passed to *xt*. The passed in file object will be closed when the called *xt* returns.

##### `with-create-dir-at-root-path`
( c-addr u xt fs -- ) ( xt: dir -- )

Parse a path starting at the root directory of a FAT32 filesystem, and attempt to create a directory in the leaf directory; if successful a directory object for that directory is passed to *xt*. The passed in directory object will be closed when the called *xt* returns.

##### `with-open-dir-at-root-path`
( c-addr u xt fs -- ) ( xt: dir -- )

Parse a path starting at the root directory of a FAT32 filesystem, and attempt to open a directory in the leaf directory; if successful a directory object for that directory is passed to *xt*. The passed in directory object will be closed when the called *xt* returns.

##### `flush`
( fs -- )

Flush any dirty blocks cached by the underlying block device.

##### `<fat32-fs>`
( -- class )

The FAT32 filesystem class. This class implements `<base-fat32-fs>`.

The `<fat32-fs>` class includes the following constructor:

##### `new`
( partition device fs -- )

Construct an instance of the `<fat32-fs>` class with block device *device* and MBR partition entry *partition*. Note that after executing this the filesystem will be ready for use, and the block device must be in working order at this time.

##### `<fat32-file>`
( --  class )

The FAT32 file class.

The `<fat32-file>` class includes the following constructor:

##### `new`
( fs file -- )

Construct an instance of `<fat32-file>` with the FAT32 filesystem *fs*.

The `<fat32-file>` class includes the following methods:

##### `close-file`
( file -- )

Close a file.

##### `file-open?`
( file -- open? )

Get whether a file is open.

##### `read-file`
( c-addr u file -- bytes )

Read data from a file.
    
##### `write-file`
( c-addr u file -- bytes )

Write data to a file.
    
##### `truncate-file`
( file -- )

Truncate a file.
    
##### `seek-file`
( offset whence file -- )

Seek in a file.
    
##### `tell-file`
( file -- offset )

Get the current offset in a file.
      
##### `file-size@`
( file -- bytes )

Get the size of a file.

##### `<fat32-dir>`

The FAT32 directory class.

The `<fat32-dir>` class includes the following constructor:

##### `new`
( fs file -- )

Construct an instance of `<fat32-dir>` with the FAT32 filesystem *fs*.

The `<fat32-dir>` class includes the following methods:

##### `close-dir`
( dir -- )

Close a directory.

##### `dir-open?`
( dir -- open? )

Get whether a directory is open.

##### `with-path`
( c-addr u xt dir -- ) ( xt: c-addr' u' dir' -- )

Parse a path starting at a given directory, and pass the leaf's name along with a directory object containing that leaf (or which would contain said leaf if it did not exist already) to the passed in *xt*. Note that said directory object will be destroyed when *xt* returns unless it was the original directory object passed in.

##### `path-exists?`
( c-addr u dir -- exists? )

Get whether a file or directory exist at the specified path starting at a given directory.

##### `with-create-file-at-path`
( c-addr u xt dir -- ) ( xt: file -- )

Parse a path starting at a given directory, and attempt to create a file in the leaf directory; if successful a file object for that file is passed to *xt*. The passed in file object will be closed when the called *xt* returns.

##### `with-open-file-at-path`
( c-addr u xt dir -- ) ( xt: file -- )

Parse a path starting at a given directory, and attempt to open a file in the leaf directory; if successful a file object for that file is passed to *xt*. The passed in file object will be closed when the called *xt* returns.

##### `with-create-dir-at-path`
( c-addr u xt dir -- ) ( xt: dir' -- )

Parse a path starting at a given directory, and attempt to create a directory in the leaf directory; if successful a directory object for that directory is passed to *xt*. The passed in directory object will be closed when the called *xt* returns.

##### `with-open-dir-at-path`
( c-addr u xt dir -- ) ( xt: dir' -- )

Parse a path starting at a given directory, and attempt to open a directory in the leaf directory; if successful a directory object for that directory is passed to *xt*. The passed in directory object will be closed when the called *xt* returns.

##### `exists?`
( c-addr u dir -- exists? )

Get whether a directory contains a file or directory of the specified name.

##### `file?`
( c-addr u -- file? )

Get whether the entry in a directory with the specified name is a file.

##### `dir?`
( c-addr u -- dir? )

Get whether the entry in a directory with the specified name is a directory.

##### `read-dir`
( entry dir -- entry-read? )

Read an entry from a directory, and return whether an entry was read.
    
##### `create-file`
( c-addr u new-file dir -- )

Create a file. Note that *new-file* need not be initialized prior to use, but no harm is done if it is.
    
##### `open-file`
( c-addr u opened-file dir -- )

Open a file. Note that *opened-file* need not be initialized prior to use, but no harm is done if it is.
    
##### `remove-file`
( c-addr u dir -- )

Remove a file.
    
##### `create-dir`
( c-addr u new-dir dir -- )

Create a directory. Note that *new-dir* need not be initialized prior to use, but no harm is done if it is.
    
##### `open-dir`
( c-addr u opened-dir dir -- )

Open a directory. Note that *opened-dir* need not be initialized prior to use, but no harm is done if it is.
    
##### `remove-dir`
( c-addr u dir -- )

Remove a directory.
    
##### `rename`
( new-c-addr new-u c-addr u dir -- )

Rename a file or directory.
    
##### `dir-empty?`
( dir -- empty? )

Get whether a directory is empty.

##### `<fat32-entry>`
( -- class )

The FAT32 directory entry class.

The `<fat32-entry>` class has no constructor.

The `<fat32-entry>` class has the following members:

##### `short-file-name`
( entry -- short-file-name-addr )

This member is 8 bytes in size.

The short file name component, padded with spaces.

The first byte can have the special values:
$00: final entry in the directory entry table
$05: the initial byte is actually $35
$2E: the dot entry
$E5: the directory entry has been deleted

##### `short-file-ext`
( entry -- short-file-ext-addr )

This member is 3 bytes in size.

The short file extension component, padded with spaces.

##### `file-attributes`
( entry -- file-attributes-addr )

This member is 1 bytes in size.

The file attributes.

There are the following bits:
$01: read only
$02: hidden
$04: system (do not move in the filesystem)
$08: volume label
$10: subdirectory (subdirectories have a file size of zero)
$20: archive
$40: device
$80: reserved

##### `nt-vfat-case`
( entry -- nt-vfat-case-addr )

This member is 1 bytes in size.

Windows NT VFAT case information.

##### `create-time-fine`
( entry -- create-time-fine-addr )

This member is 1 bytes in size.

Create time file resolution, 10 ms increments, from 0 to 199.

##### `create-time-coarse`
( entry -- create-time-coarse-addr )

This member is 2 bytes in size.

Create time with coarse resolution, 2 s increments.

bits 15-11: hours (0-23)
bits 10-5: minutes (0-59)
bits 4-0: seconds / 2 (0-29)

##### `create-date`
( entry -- create-date-addr )

This member is 2 bytes in size.

Create date.

bits 15-9: year (0 = 1980)
bits 8-5: month (1-12)
bits 4-0: day (1-31)

##### `access-date`
( entry -- access-date-addr )

This member is 2 bytes in size.

Last access date.

bits 15-9: year (0 = 1980)
bits 8-5: month (1-12)
bits 4-0: day (1-31)

##### `first-cluster-high`
( entry -- first-cluster-high-addr )

This member is 2 bytes in size.

High two bytes of the first cluster number.

##### `modify-time-coarse`
( entry -- modify-time-coarse-addr )

This member is 2 bytes in size.

Last modify time with coarse resolution, 2 s increments.

bits 15-11: hours (0-23)
bits 10-5: minutes (0-59)
bits 4-0: seconds / 2 (0-29)

##### `modify-date`
( entry -- modify-date-addr )

This member is 2 bytes in size.

Last modify date.

bits 15-9: year (0 = 1980)
bits 8-5: month (1-12)
bits 4-0: day (1-31)

##### `first-cluster-low`
( entry -- first-cluster-low-addr )

This member is 2 bytes in size.

Low two bytes of the first cluster number.

##### `entry-file-size`
( entry -- entry-file-size-addr )

This member is 4 bytes in size.

The file size; is always 0 for subdirectories and volume labels.

##### `buffer>entry`
( addr entry -- )

Import a buffer into a directory entry.

##### `entry>buffer`
( addr entry -- )

Export a directory entry as a buffer.

##### `init-blank-entry`
( entry -- )

Initialize a blank directory entry.

##### `init-file-entry`
( file-size first-cluster c-addr u entry -- )

Initialize a file directory entry.

##### `init-dir-entry`
( first-cluster c-addr u entry -- )

Initialize a subdirectory directory entry.

##### `init-end-entry`
( entry -- )

Initialize an end directory entry.

##### `mark-entry-deleted`
( entry -- )

Mark a directory entry as deleted.

##### `entry-deleted?`
( entry -- deleted? )

Get whether a directory entry has been deleted.

##### `entry-end?`
( entry -- end? )

Get whether a directory entry is the last in a directory.

##### `entry-file?`
( entry -- file? )

Get whether a directory entry is for a file.

##### `entry-dir?`
( entry -- subdir? )

Get whether a directory entry is for a subdirectory.

##### `first-cluster@`
( entry -- cluster )

Get the first cluster index of a directory entry.

##### `first-cluster!`
( cluster entry -- )

Set the first cluster index of a directory entry.

##### `file-name!`
( c-addr u entry -- )

Set the file name of a directory entry, converted from a normal string.

##### `dir-name!`
( c-addr u entry -- )

Set the directory name of a directory entry, converted from a normal string.

##### `file-name@`
( c-addr u entry -- c-addr u' )

Get the file name of a directory entry, converted to a normal string.

##### `create-date-time!`
( date-time entry -- )

Set the creation date/time of a directory entry, with second resolution.

##### `create-date-time@`
( date-time entry -- )

Get the creation date/time of a directory entry, with second resolution and day-of-the-week calculation.

##### `modify-date-time!`
( date-time entry -- )

Set the modification date/time of a directory entry, with two second resolution.

##### `modify-date-time@`
( date-time entry -- )

Get the modification date/time of a directory entry, with two second resolution and day-of-the-week calculation.
