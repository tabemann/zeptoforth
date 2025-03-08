# Copying Files Between Filesystems

An optional mechanism for copying files between filesystems is provided by `copy-file-to-fs` in the `inter-fs-copy` module that can be loaded from `extra/common/inter_fs_copy.fs`. This mechanism creates a new file in the destination filesystem and copies the contents of an existing file in the source filesystem into it. Note that the destination file must not already exist; if it does the user must delete it first.

### `inter-fs-copy`

The `inter-fs-copy` module contains the following word:

##### `copy-file-to-fs`
( src-path-addr src-path-bytes src-fs dest-path-addr dest-path-bytes dest-fs -- )

Copy an existing source file at *src-path-addr*/*src-path-bytes* in the filesystem *src-fs* into a newly created destination file at *dest-path-addr*/*dest-path-bytes* in the filesystem *dest-fs*. The destination file must not already exist.
