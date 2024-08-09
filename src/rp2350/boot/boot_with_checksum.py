#!/usr/bin/env python3

import sys
import struct
import binascii

# Max boot block length
MAX_BOOT_BLOCK_LEN = 252

# Pad the boot block
def pad_boot_block(boot_block):
    return boot_block.ljust(MAX_BOOT_BLOCK_LEN, b'\x00')

# Bit reversal
def reverse_bits(x, width):
    return int("{:0{w}b}".format(x, w=width)[::-1], 2)

# Carry out the checksum
def get_checksum(data, seed):
    data = bytes(reverse_bits(byte, 8) for byte in data)
    return reverse_bits((binascii.crc32(data, seed ^ 0xFFFFFFFF) ^ 0xFFFFFFFF)
                        & 0xFFFFFFFF, 32)

# The main body of the code
def main():
    if len(sys.argv) == 3:
        try:
            boot_block_file = open(sys.argv[1], 'rb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], sys.argv[1]))
        boot_block = boot_block_file.read()
        boot_block_file.close()
        if len(boot_block) > MAX_BOOT_BLOCK_LEN:
            sys.exit('%s: %s: larger than maximum boot block size (252 bytes)'
                     % (sys.argv[0], sys.argv[1]))
        try:
            output_file = open(sys.argv[2], 'wb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], sys.argv[2]))
        boot_block = pad_boot_block(boot_block)
        checksum = get_checksum(boot_block, -1)
        output_file.write(struct.pack('<252sI', boot_block, checksum))
        output_file.close()
    else:
        sys.exit('%s: input output' % sys.argv[0])    

if __name__ == '__main__':
    main()
