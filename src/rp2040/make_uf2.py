#!/usr/bin/env python3

import sys
import struct

# The first magic number
MAGIC_0 = 0x0A324655

# The second magic number
MAGIC_1 = 0x9E5D5157

# The final magic number
MAGIC_2 = 0x0AB16F30

# The block size
BLOCK_SIZE = 256

# The UF2 block size
UF2_BLOCK_SIZE = 512

# The coda flash address
def CODA_ADDR(big):
    if big:
        return 0x0017FF00
    else:
        return 0x000FFF00

# Block flags
FLAGS = 0x00002000

# Base address
BASE_ADDR = 0x10000000

# RP2040 family ID
RP2040_FAMILY_ID = 0xE48BFF56

# Coda count
CODA_COUNT = 1

# Minidictionary size
MINI_DICT_SIZE = 49152

# Big minidictionary size
BIG_MINI_DICT_SIZE = 61440

# Minidictionary address
MINI_DICT_ADDR = 0xF0000

# Big miniditionary address
BIG_MINI_DICT_ADDR = 0x170000

# Get the minidictionary size
def mini_dict_size(big):
    if big:
        return BIG_MINI_DICT_SIZE
    else:
        return MINI_DICT_SIZE

# Get the minidictionary address
def mini_dict_addr(big):
    if big:
        return BIG_MINI_DICT_ADDR
    else:
        return MINI_DICT_ADDR
    
# Get the block count
def block_count(image_size, big):
    image_size = ((image_size - 1) | (BLOCK_SIZE - 1)) + 1
    return ((image_size + mini_dict_size(big)) // BLOCK_SIZE) + CODA_COUNT

# Pack a block
def pack_block(buf, index, total_count, addr, data, data_off, data_len):
    if data_off + data_len > len(data):
        data_len = len(data) - data_off
    padded_data = data[data_off:data_off + data_len].ljust(476, b'\x00')
#    print(("MAGIC_0: %x MAGIC_1: %x FLAGS: %x addr: %x BLOCK_SIZE: %d "
#           "index: %d total_count: %d MAGIC_2: %x")
#          % (MAGIC_0, MAGIC_1, FLAGS, BASE_ADDR + addr, BLOCK_SIZE, index,
#             total_count, MAGIC_2))
    struct.pack_into('<IIIIIIII476sI', buf, index * UF2_BLOCK_SIZE,
                     MAGIC_0, MAGIC_1, FLAGS, BASE_ADDR + addr, BLOCK_SIZE,
                     index, total_count, RP2040_FAMILY_ID, padded_data,
                     MAGIC_2);

# Write the bootblock and padding to the output image
def pack_boot_block(buf, boot_block, total_count):
    pack_block(buf, 0, total_count, 0, boot_block, 0, 256)
    for i in range(1, 16):
        pack_block(buf, i, total_count, i * BLOCK_SIZE, b'', 0, 0)

# Write the image to the output image
def pack_image(buf, image, total_count, pad_count):
    for i in range(0, total_count - (CODA_COUNT + pad_count)):
        pack_block(buf, i + pad_count, total_count,
                   (i + pad_count) * BLOCK_SIZE, image, i * BLOCK_SIZE,
                   BLOCK_SIZE)

# Write the minidictionary to the output image
def pack_mini_dict(buf, mini_dict, total_count, big):
    before_count = (total_count - (mini_dict_size(big) // BLOCK_SIZE)) - 1
    for i in range(0, mini_dict_size(big) // BLOCK_SIZE):
        pack_block(buf, i + before_count, total_count,
                   (i * BLOCK_SIZE) + mini_dict_addr(big), mini_dict,
                   i * BLOCK_SIZE, BLOCK_SIZE)

# Write the coda (specifying the image size) to the output image
def pack_coda(buf, total_count, big):
    end_addr = (((((total_count - 1) * BLOCK_SIZE) \
                  - mini_dict_size(big)) - 1) \
                | 4095) + 1
    coda = struct.pack('<I', end_addr)
    pack_block(buf, total_count - 1, total_count, CODA_ADDR(big), coda, 0, 4)

# Display the usage message
def usage():
    sys.exit('%s: [boot-block] image mini-dictionary output' % sys.argv[0])

# The main body of the code
def main():
    args = sys.argv[1:]
    big = False
    if len(args) > 0:
        if args[0] == '-b' or args[0] == '--big':
            big = True
            args = args[1:]
    else:
        usage()    
    if len(args) == 3 or len(args) == 4:
        boot_block = None
        if len(args) == 4:
            try:
                boot_block_file = open(args[0], 'rb')
            except:
                sys.exit('%s: %s: could not open file'
                         % (sys.argv[0], args[0]))
            boot_block = boot_block_file.read()
            boot_block_file.close()
            if len(boot_block) != 256:
                sys.exit('Boot block is not 256 bytes in size')
        try:
            image_file = open(args[-3], 'rb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], args[-3]))
        image = image_file.read()
        image_file.close()
        try:
            mini_dict_file = open(args[-2], 'rb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], args[-2]))
        mini_dict = mini_dict_file.read()
        mini_dict_file.close()
        try:
            output_file = open(args[-1], 'wb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], args[-1]))
        pad_count = 0
        if boot_block != None:
            total_count = block_count((16 * BLOCK_SIZE) + len(image), big)
            pad_count = 16
        else:
            total_count = block_count(len(image), big)
        output_image = bytearray(total_count * UF2_BLOCK_SIZE)
        if boot_block != None:
            pack_boot_block(output_image, boot_block, total_count)
        pack_image(output_image, image, total_count, pad_count)
        pack_mini_dict(output_image, mini_dict, total_count, big)
        if CODA_COUNT == 1:
            pack_coda(output_image, total_count, big)
        output_file.write(output_image)
        output_file.close()
    else:
        usage()

if __name__ == '__main__':
    main()
