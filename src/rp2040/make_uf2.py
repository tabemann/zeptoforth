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

# Get the block count
def block_count(image_size):
    image_size = ((image_size - 1) | (BLOCK_SIZE - 1)) + 1
    return (image_size / BLOCK_SIZE) + 1

# Pack a block
def pack_block(buf, index, total_count, addr, data, data_off, data_len):
    if data_off + data_len > len(data):
        data_len = len(data) - data_off
    padded_data = data[data_off:data_off + data_len].ljust(476, b'\x00')
    struct.pack_into('I<I<I<I<I<I<I<I<sI<', buf, index * UF2_BLOCK_SIZE,
                     MAGIC_0, MAGIC_1, flags, addr, BLOCK_SIZE,
                     index, total_count, 0, padded_data, MAGIC2);

# Write the bootblock and padding to the output image
def pack_boot_block(buf, boot_block, total_count):
    pack_block(buf, 0, total_count, 0, boot_block, 0, 256)
    pack_block(buf, 1, total_count, 256, b'', 0, 0)
    pack_block(buf, 2, total_count, 512, b'', 0, 0)
    pack_block(buf, 3, total_count, 768, b'', 0, 0)

# Write the image to the output image
def pack_image(buf, image, total_count):
    for i in range(0, total_count - 5):
        pack_block(output_image, i + 4, total_count,
                   (i * BLOCK_SIZE) + 1024, image, i * BLOCK_SIZE, 256)

# Write the coda (specifying the image size) to the output image
def pack_coda(buf, total_count):
    end_addr = ((total_count - 2) | 4095) + 1
    coda = struct.pack('I<', end_addr)
    pack_block(buf, total_count - 1, total_count, CODA_ADDR, coda, 0, 4)
    
# The main body of the code
def main():
    if len(sys.argv) != 4:
        sys.exit('%s: boot-block image output' % sys.argv[0])
    else:
        boot_block_file = open(sys.argv[1], 'rb')
        boot_block = boot_block_file.read()
        boot_block_file.close()
        if len(boot_block) != 256:
            sys.exit('Boot block is not 256 bytes in size')
        image_file = open(sys.argv[2], 'rb')
        image = image_file.read()
        image_file.close()
        output = open(sys.argv[3], 'wb')
        total_count = block_count(1024 + len(image))
        output_image = bytearray(total_count * UF2_BLOCK_SIZE)
        pack_boot_block(output_image, boot_block, total_count)
        pack_image(output_image, image, total_count)
        pack_coda(output_image. total_count)
        output.write(output_image)
        output.close()

if __name__ == '__main__':
    main()
