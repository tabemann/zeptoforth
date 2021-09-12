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
CODA_ADDR = 0x000FFF00

# Block flags
FLAGS = 0x00000000

# Get the block count
def block_count(image_size):
    image_size = ((image_size - 1) | (BLOCK_SIZE - 1)) + 1
    return (image_size // BLOCK_SIZE) + 1

# Pack a block
def pack_block(buf, index, total_count, addr, data, data_off, data_len):
    if data_off + data_len > len(data):
        data_len = len(data) - data_off
    padded_data = data[data_off:data_off + data_len].ljust(476, b'\x00')
    struct.pack_into('<IIIIIIII476sI', buf, index * UF2_BLOCK_SIZE,
                     MAGIC_0, MAGIC_1, FLAGS, addr, BLOCK_SIZE,
                     index, total_count, 0, padded_data, MAGIC_2);

# Write the bootblock and padding to the output image
def pack_boot_block(buf, boot_block, total_count):
    pack_block(buf, 0, total_count, 0, boot_block, 0, 256)
    pack_block(buf, 1, total_count, 256, b'', 0, 0)
    pack_block(buf, 2, total_count, 512, b'', 0, 0)
    pack_block(buf, 3, total_count, 768, b'', 0, 0)

# Write the image to the output image
def pack_image(buf, image, total_count, pad_count):
    for i in range(0, total_count - (1 + pad_count)):
        pack_block(buf, i + pad_count, total_count,
                   (i + pad_count) * BLOCK_SIZE, image, i * BLOCK_SIZE, 256)

# Write the coda (specifying the image size) to the output image
def pack_coda(buf, total_count):
    end_addr = ((total_count - 2) | 4095) + 1
    coda = struct.pack('<I', end_addr)
    pack_block(buf, total_count - 1, total_count, CODA_ADDR, coda, 0, 4)
    
# The main body of the code
def main():
    if len(sys.argv) == 3 or len(sys.argv) == 4:
        boot_block = None
        if len(sys.argv) == 4:
            try:
                boot_block_file = open(sys.argv[1], 'rb')
            except:
                sys.exit('%s: %s: could not open file'
                         % (sys.argv[0], sys.argv[1]))
            boot_block = boot_block_file.read()
            boot_block_file.close()
            if len(boot_block) != 256:
                sys.exit('Boot block is not 256 bytes in size')
        try:
            image_file = open(sys.argv[len(sys.argv) - 2], 'rb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], sys.argv[len(sys.argv) - 2]))
        image = image_file.read()
        image_file.close()
        try:
            output_file = open(sys.argv[len(sys.argv) - 1], 'wb')
        except:
            sys.exit('%s: %s: could not open file'
                     % (sys.argv[0], sys.argv[len(sys.argv) - 1]))
        pad_count = 0
        if boot_block != None:
            total_count = block_count(1024 + len(image))
            pad_count = 4
        else:
            total_count = block_count(len(image))
        output_image = bytearray(total_count * UF2_BLOCK_SIZE)
        if boot_block != None:
            pack_boot_block(output_image, boot_block, total_count)
        pack_image(output_image, image, total_count, pad_count)
        pack_coda(output_image, total_count)
        output_file.write(output_image)
        output_file.close()
    else:
        sys.exit('%s: [boot-block] image output' % sys.argv[0])

if __name__ == '__main__':
    main()
